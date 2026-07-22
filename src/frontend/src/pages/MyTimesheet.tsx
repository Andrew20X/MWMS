import { useState, useEffect } from 'react';
import { Box, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Button, Dialog, DialogTitle, DialogContent, DialogActions, Chip, CircularProgress, Snackbar, Alert } from '@mui/material';
import { Download, UploadCloud, CheckCircle, Trash } from 'lucide-react';
import axios from 'axios';
import { formatTime12Hour } from '../utils/dateUtils';
import { useAuth } from '../contexts/AuthContext';

/** Converts total minutes to a human-readable string, e.g. 90 → "1h 30m" */
const formatOvertimeMinutes = (minutes: number): string => {
  if (!minutes || minutes <= 0) return '–';
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  if (h === 0) return `${m}m`;
  if (m === 0) return `${h}h`;
  return `${h}h ${m}m`;
};


export default function MyTimesheet() {
  const { user } = useAuth();
  const [attendance, setAttendance] = useState<any[]>([]);
  const [sortAscending, setSortAscending] = useState(true);
  const [loading, setLoading] = useState(true);
  const [openExport, setOpenExport] = useState(false);
  const [openFetch, setOpenFetch] = useState(false);
  const [openImport, setOpenImport] = useState(false);
  const [openSubmit, setOpenSubmit] = useState(false);
  const [openClear, setOpenClear] = useState(false);
  const [file, setFile] = useState<File | null>(null);
  const [importing, setImporting] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [clearing, setClearing] = useState(false);
  const [fetchingDevice, setFetchingDevice] = useState(false);
  const [startDate, setStartDate] = useState(new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0]);
  const [endDate, setEndDate] = useState(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0).toISOString().split('T')[0]);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' | 'info' | 'warning' }>({ open: false, message: '', severity: 'info' });

  const showMessage = (message: string, severity: 'success' | 'error' | 'info' | 'warning' = 'info') => {
    setSnackbar({ open: true, message, severity });
  };

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const attRes = await axios.get('http://localhost:5222/api/attendance/me', {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setAttendance(attRes.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };



  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setFile(e.target.files[0]);
    }
  };

  const handleFetchDevice = async () => {
    if (!startDate || !endDate) {
      showMessage("Please select a date range.", "warning");
      return;
    }
    setFetchingDevice(true);
    try {
      const res = await axios.post('http://localhost:5222/api/Attendance/fetch-from-device', { startDate, endDate }, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      showMessage(res.data.message || 'Device logs fetched and imported successfully!', "success");
      setOpenFetch(false);
      fetchData();
    } catch (error: any) {
      showMessage(error.response?.data || 'Failed to fetch from device. Check device connection.', "error");
    } finally {
      setFetchingDevice(false);
    }
  };

  const handleImport = async () => {
    if (!file) return;
    const formData = new FormData();
    formData.append('file', file);
    setImporting(true);

    try {
      await axios.post('http://localhost:5222/api/Attendance/import/me', formData, {
        headers: { 'Content-Type': 'multipart/form-data', Authorization: `Bearer ${user?.token}` }
      });
      showMessage('Raw timesheet imported successfully!', "success");
      setFile(null);
      setOpenImport(false);
      fetchData();
    } catch (error: any) {
      let errMsg = 'Failed to upload timesheet. Please ensure the server is running.';
      if (error.response?.data && typeof error.response.data === 'string') errMsg = error.response.data;
      showMessage(errMsg, "error");
    } finally {
      setImporting(false);
    }
  };

  const handleSubmitFinal = async () => {
    if (!file) return;
    const formData = new FormData();
    formData.append('file', file);
    setSubmitting(true);

    try {
      await axios.post('http://localhost:5222/api/Attendance/upload-final/me', formData, {
        headers: { 'Content-Type': 'multipart/form-data', Authorization: `Bearer ${user?.token}` }
      });
      showMessage('Final timesheet submitted to HR successfully!', "success");
      setFile(null);
      setOpenSubmit(false);
    } catch (error: any) {
      let errMsg = 'Failed to submit timesheet. Please ensure the server is running.';
      if (error.response?.data && typeof error.response.data === 'string') errMsg = error.response.data;
      showMessage(errMsg, "error");
    } finally {
      setSubmitting(false);
    }
  };

  const handleExport = async () => {
    if (!startDate || !endDate) {
      showMessage("Please select a date range.", "warning");
      return;
    }
    setExporting(true);
    try {
      const response = await axios.get(`http://localhost:5222/api/Attendance/export/me?startDate=${startDate}&endDate=${endDate}`, {
        responseType: 'blob',
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `MyTimesheet_${startDate}_to_${endDate}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      
      setOpenExport(false);
    } catch (error: any) {
      console.error("Export error:", error);
      let errorMsg = 'Failed to export timesheet.';
      if (error.response?.status === 401) {
        errorMsg = 'Unauthorized: You may be logged in as an Admin who does not have a personal timesheet.';
      } else if (error.response?.data instanceof Blob) {
        try {
          errorMsg = await error.response.data.text();
        } catch (e) {
          console.error("Could not parse blob error", e);
        }
      } else if (error.message) {
        errorMsg = error.message;
      }
      showMessage(errorMsg, "error");
    } finally {
      setExporting(false);
    }
  };

  const handleClearData = async () => {
    setClearing(true);
    try {
      await axios.delete('http://localhost:5222/api/Attendance/me', {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      showMessage('Attendance data cleared successfully.', "success");
      fetchData();
      setOpenClear(false);
    } catch (err) {
      showMessage('Failed to clear attendance data.', "error");
    } finally {
      setClearing(false);
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', flexDirection: { xs: 'column', lg: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'stretch', lg: 'center' }, gap: 2, mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 'normal', color: '#1E293B', fontSize: { xs: '1.75rem', sm: '2.125rem' } }}>
          My Timesheet
        </Typography>
        <Box sx={{ display: 'flex', gap: 2, flexDirection: { xs: 'column', sm: 'row' }, flexWrap: 'wrap' }}>
          <Button variant="outlined" color="error" startIcon={<Trash size={18} />} onClick={() => setOpenClear(true)} sx={{ borderRadius: 2, flex: { xs: 1, sm: 'none' } }}>
            Clear Data
          </Button>
          <Button variant="contained" onClick={() => setOpenFetch(true)} disabled={fetchingDevice} sx={{ borderRadius: 2, flex: { xs: 1, sm: 'none' }, bgcolor: '#334155', '&:hover': { bgcolor: '#1e293b' } }}>
            {fetchingDevice ? <CircularProgress size={24} color="inherit" /> : 'Fetch from Device'}
          </Button>
          <Button variant="outlined" startIcon={<UploadCloud size={18} />} onClick={() => {setFile(null); setOpenImport(true);}} sx={{ borderRadius: 2, flex: { xs: 1, sm: 'none' } }}>
            1. Import Raw
          </Button>
          <Button variant="outlined" startIcon={<Download size={18} />} onClick={() => setOpenExport(true)} sx={{ borderRadius: 2, flex: { xs: 1, sm: 'none' } }}>
            2. Export Format
          </Button>
          <Button variant="contained" color="success" startIcon={<CheckCircle size={18} />} onClick={() => {setFile(null); setOpenSubmit(true);}} sx={{ borderRadius: 2, flex: { xs: 1, sm: 'none' } }}>
            3. Submit Final
          </Button>
        </Box>
      </Box>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 5 }}>
          <CircularProgress />
        </Box>
      ) : (
        <>


          <Typography variant="h6" sx={{ mb: 2, fontWeight: 'normal' }}>
            Recent Attendance (Count: {attendance ? attendance.length : 'null'})
          </Typography>
          <TableContainer component={Paper} sx={{ borderRadius: 3, boxShadow: '0 4px 20px rgba(0,0,0,0.05)' }}>
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ backgroundColor: '#F8FAFC' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 'normal' }}>Employee ID</TableCell>
                  <TableCell 
                    sx={{ fontWeight: 'normal', cursor: 'pointer', userSelect: 'none' }}
                    onClick={() => setSortAscending(!sortAscending)}
                  >
                    Date {sortAscending ? '↑' : '↓'}
                  </TableCell>
                  <TableCell sx={{ fontWeight: 'normal' }}>Check In</TableCell>
                  <TableCell sx={{ fontWeight: 'normal' }}>Check Out</TableCell>
                  <TableCell sx={{ fontWeight: 'normal' }}>Status</TableCell>
                  <TableCell sx={{ fontWeight: 'normal' }}>Duty Code</TableCell>
                  <TableCell sx={{ fontWeight: 'normal' }}>Overtime</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {[...attendance].sort((a, b) => {
                  const dateA = new Date(a.date).getTime();
                  const dateB = new Date(b.date).getTime();
                  return sortAscending ? dateA - dateB : dateB - dateA;
                }).map((row, index) => (
                  <TableRow key={index}>
                    <TableCell>{row.employeeCode}</TableCell>
                    <TableCell>{new Date(row.date).toLocaleDateString()}</TableCell>
                    <TableCell>{formatTime12Hour(row.checkIn)}</TableCell>
                    <TableCell>{formatTime12Hour(row.checkOut)}</TableCell>
                    <TableCell>
                      <Chip label={row.status} color="primary" size="small" variant="outlined" />
                    </TableCell>
                    <TableCell>
                      {row.overtimeType || (new Date(row.date).getDay() === 5 || new Date(row.date).getDay() === 6 ? 'WE' : (row.status === 'Absent' ? 'AWP' : 'OD'))}
                    </TableCell>
                    <TableCell>
                      {row.overtimeMinutes > 0 ? (
                        <Chip
                          label={formatOvertimeMinutes(row.overtimeMinutes)}
                          size="small"
                          sx={{ bgcolor: '#FFF3CD', color: '#856404', fontWeight: 400, border: '1px solid #FFEAA7' }}
                        />
                      ) : (
                        <Typography variant="body2" color="text.disabled">–</Typography>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}



      <Dialog open={openExport} onClose={() => setOpenExport(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 'normal' }}>Export My Timesheet (Step 2)</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 1 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Select a date range to generate and download your timesheet in the HR New Format. Review the downloaded file before submitting.
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, flexDirection: 'column' }}>
              <Box>
                <Typography variant="caption" color="text.secondary">From Date</Typography>
                <input 
                  type="date"
                  value={startDate} 
                  onChange={(e) => setStartDate(e.target.value)}
                  style={{ width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ccc', marginTop: '4px' }}
                />
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">To Date</Typography>
                <input 
                  type="date"
                  value={endDate} 
                  onChange={(e) => setEndDate(e.target.value)}
                  style={{ width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ccc', marginTop: '4px' }}
                />
              </Box>
            </Box>
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpenExport(false)}>Cancel</Button>
          <Button onClick={handleExport} variant="contained" disabled={!startDate || !endDate || exporting}>
            {exporting ? <CircularProgress size={24} /> : 'Download Excel'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Dialog for Fetch Format */}
      <Dialog open={openFetch} onClose={() => setOpenFetch(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Fetch Device Data</DialogTitle>
        <DialogContent dividers>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            <Typography variant="body2" color="text.secondary">
              Select the date range to fetch from the fingerprint machine.
            </Typography>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <Box>
                <Typography variant="caption" color="text.secondary">From Date</Typography>
                <input 
                  type="date"
                  value={startDate} 
                  onChange={(e) => setStartDate(e.target.value)}
                  style={{ width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ccc', marginTop: '4px' }}
                />
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">To Date</Typography>
                <input 
                  type="date"
                  value={endDate} 
                  onChange={(e) => setEndDate(e.target.value)}
                  style={{ width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ccc', marginTop: '4px' }}
                />
              </Box>
            </Box>
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpenFetch(false)}>Cancel</Button>
          <Button onClick={handleFetchDevice} variant="contained" disabled={!startDate || !endDate || fetchingDevice}>
            {fetchingDevice ? <CircularProgress size={24} /> : 'Fetch Data'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Dialog for Import Raw */}
      <Dialog open={openImport} onClose={() => setOpenImport(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Import Raw Timesheet (Step 1)</DialogTitle>
        <DialogContent>
          <Box sx={{ border: '2px dashed #CBD5E1', borderRadius: '12px', p: 4, textAlign: 'center', mt: 1 }}>
            <UploadCloud size={48} color="#2E7D32" style={{ marginBottom: '16px' }} />
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Upload your raw timesheet from the biometric device.
            </Typography>
            <Button variant="contained" component="label">
              Choose File
              <input type="file" hidden accept=".xlsx" onChange={handleFileChange} />
            </Button>
            {file && <Typography variant="body2" sx={{ mt: 2, color: '#4CAF50' }}>{file.name}</Typography>}
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpenImport(false)}>Cancel</Button>
          <Button onClick={handleImport} variant="contained" disabled={!file || importing}>
            {importing ? <CircularProgress size={24} /> : 'Import'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Dialog for Submit Final */}
      <Dialog open={openSubmit} onClose={() => setOpenSubmit(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Submit Final Timesheet (Step 3)</DialogTitle>
        <DialogContent>
          <Box sx={{ border: '2px dashed #CBD5E1', borderRadius: '12px', p: 4, textAlign: 'center', mt: 1 }}>
            <CheckCircle size={48} color="#4CAF50" style={{ marginBottom: '16px' }} />
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Upload your finalized New Format timesheet. This will be submitted directly to HR.
            </Typography>
            <Button variant="contained" component="label">
              Choose File
              <input type="file" hidden accept=".xlsx" onChange={handleFileChange} />
            </Button>
            {file && <Typography variant="body2" sx={{ mt: 2, color: '#4CAF50' }}>{file.name}</Typography>}
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpenSubmit(false)}>Cancel</Button>
          <Button onClick={handleSubmitFinal} variant="contained" color="success" disabled={!file || submitting}>
            {submitting ? <CircularProgress size={24} /> : 'Submit to HR'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Dialog for Clear Data */}
      <Dialog open={openClear} onClose={() => setOpenClear(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ color: 'error.main', fontWeight: 'normal' }}>Clear Attendance Data</DialogTitle>
        <DialogContent>
          <Box sx={{ textAlign: 'center', mt: 2, mb: 1 }}>
            <Trash size={48} color="#f44336" style={{ marginBottom: '16px' }} />
            <Typography variant="body1" sx={{ mb: 2 }}>
              Are you sure you want to clear your imported attendance data?
            </Typography>
            <Typography variant="body2" color="error">
              This action cannot be undone. You will need to import your data again.
            </Typography>
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpenClear(false)}>Cancel</Button>
          <Button onClick={handleClearData} variant="contained" color="error" disabled={clearing}>
            {clearing ? <CircularProgress size={24} color="inherit" /> : 'Yes, Clear Data'}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar 
        open={snackbar.open} 
        autoHideDuration={4000} 
        onClose={() => setSnackbar({ ...snackbar, open: false })}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={() => setSnackbar({ ...snackbar, open: false })} severity={snackbar.severity} sx={{ width: '100%', borderRadius: 2, boxShadow: '0 4px 12px rgba(0,0,0,0.1)' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}




