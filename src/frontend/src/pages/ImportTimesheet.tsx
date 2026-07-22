import { useState, useEffect } from 'react';
import { Typography, Box, Paper, Button, Alert, CircularProgress, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Chip, Tabs, Tab, Checkbox } from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { UploadCloud, Download, Trash } from 'lucide-react';
import axios from 'axios';
import { formatTime12Hour } from '../utils/dateUtils';

const formatMinutes = (minutes: number) => {
  if (!minutes || minutes <= 0) return '-';
  if (minutes < 60) return `${minutes} Mins`;
  return `${Math.floor(minutes / 60)}:${(minutes % 60).toString().padStart(2, '0')} Hr(s)`;
};

interface AttendanceLog {
  id: number;
  employeeName: string;
  date: string;
  checkIn: string | null;
  checkOut: string | null;
  status: string;
  lateMinutes: number;
  earlyLeaveMinutes: number;
  overtimeMinutes: number;
}



export default function Timesheets() {
  const { user } = useAuth();
  const [logs, setLogs] = useState<AttendanceLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [openImport, setOpenImport] = useState(false);
  const [openExport, setOpenExport] = useState(false);
  const [openDelete, setOpenDelete] = useState(false);
  const [openClearRaw, setOpenClearRaw] = useState(false);
  const [clearingRaw, setClearingRaw] = useState(false);
  const [filesToDelete, setFilesToDelete] = useState<string[]>([]);
  const [selectedFiles, setSelectedFiles] = useState<string[]>([]);
  const [deleting, setDeleting] = useState(false);
  const [file, setFile] = useState<File | null>(null);
  const [importing, setImporting] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [fetchingDevice, setFetchingDevice] = useState(false);
  const [openFetchDialog, setOpenFetchDialog] = useState(false);
  const [fetchStartDate, setFetchStartDate] = useState(new Date().toISOString().split('T')[0] + 'T00:00');
  const [fetchEndDate, setFetchEndDate] = useState(new Date().toISOString().split('T')[0] + 'T23:59');
  const [message, setMessage] = useState<{ type: 'success' | 'error', text: string } | null>(null);
  
  const [tab, setTab] = useState(0);
  const [submittedFiles, setSubmittedFiles] = useState<any[]>([]);
  const [loadingSubmitted, setLoadingSubmitted] = useState(false);
  
  const [startDate, setStartDate] = useState(new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0]);
  const [endDate, setEndDate] = useState(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0).toISOString().split('T')[0]);

  const fetchLogs = async () => {
    setLoading(true);
    try {
      const res = await axios.get('http://localhost:5222/api/Attendance/recent', {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setLogs(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchSubmitted = async () => {
    setLoadingSubmitted(true);
    try {
      const res = await axios.get('http://localhost:5222/api/Attendance/submitted', {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setSubmittedFiles(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingSubmitted(false);
    }
  };

  useEffect(() => {
    if (tab === 0) fetchLogs();
    if (tab === 1) fetchSubmitted();
  }, [tab]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setFile(e.target.files[0]);
    }
  };

  const handleUpload = async () => {
    if (!file) return;
    const formData = new FormData();
    formData.append('file', file);
    setImporting(true);
    setMessage(null);

    try {
      const response = await axios.post('http://localhost:5222/api/Attendance/import', formData, {
        headers: { 
          'Content-Type': 'multipart/form-data',
          Authorization: `Bearer ${user?.token}`
        }
      });
      setMessage({ type: 'success', text: response.data.message });
      setTimeout(() => setMessage(null), 4000);
      setFile(null);
      setOpenImport(false);
      fetchLogs();
    } catch (error: any) {
      setMessage({ type: 'error', text: error.response?.data || 'Failed to upload timesheet' });
      setTimeout(() => setMessage(null), 4000);
    } finally {
      setImporting(false);
    }
  };

  const handleFetchDevice = async () => {
    setFetchingDevice(true);
    setMessage(null);
    try {
      const response = await axios.post('http://localhost:5222/api/Attendance/fetch-from-device', 
      {
        startDate: fetchStartDate,
        endDate: fetchEndDate
      }, 
      {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setMessage({ type: 'success', text: response.data.message });
      setTimeout(() => setMessage(null), 4000);
      setOpenFetchDialog(false);
      fetchLogs();
    } catch (error: any) {
      setMessage({ type: 'error', text: error.response?.data || 'Failed to fetch from device' });
      setTimeout(() => setMessage(null), 4000);
    } finally {
      setFetchingDevice(false);
    }
  };

  const handleExport = async () => {
    if (!startDate || !endDate) {
      setMessage({ type: 'error', text: 'Please select a date range' });
      return;
    }
    setExporting(true);
    try {
      const response = await axios.get(`http://localhost:5222/api/Attendance/export/all?startDate=${startDate}&endDate=${endDate}`, {
        responseType: 'blob', // Important for file downloads
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Timesheets_All_${startDate}_to_${endDate}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      
      setOpenExport(false);
    } catch (error: any) {
      console.error("Export error:", error);
      let errorMsg = 'Failed to export timesheet';
      if (error.response?.data instanceof Blob) {
        try {
          errorMsg = await error.response.data.text();
        } catch (e) {
          console.error("Could not parse blob error", e);
        }
      } else if (error.response?.data) {
        errorMsg = typeof error.response.data === 'string' ? error.response.data : JSON.stringify(error.response.data);
      } else if (error.message) {
        errorMsg = error.message;
      }
      setMessage({ type: 'error', text: errorMsg });
      setTimeout(() => setMessage(null), 4000);
    } finally {
      setExporting(false);
    }
  };

  const getStatusColor = (status: string, lateMinutes: number) => {
    if (status === 'Absent') return 'error';
    if (lateMinutes > 0) return 'warning';
    return 'success';
  };

  const handleClearRaw = async () => {
    setClearingRaw(true);
    try {
      const response = await axios.delete('http://localhost:5222/api/Attendance/raw/all', {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setMessage({ type: 'success', text: response.data.message || 'All raw data cleared.' });
      setTimeout(() => setMessage(null), 4000);
      setOpenClearRaw(false);
      fetchLogs();
    } catch (err: any) {
      console.error(err);
      setMessage({ type: 'error', text: err.response?.data?.message || 'Failed to clear raw data.' });
      setTimeout(() => setMessage(null), 4000);
    } finally {
      setClearingRaw(false);
    }
  };

    const handleDownloadSubmitted = async (fileName: string) => {
      try {
        const res = await axios.get(`http://localhost:5222/api/Attendance/submitted/download/${fileName}`, {
          responseType: 'blob',
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        const url = window.URL.createObjectURL(new Blob([res.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
      } catch (err) {
        console.error("Failed to download", err);
        setMessage({ type: 'error', text: 'Failed to download the submitted timesheet.' });
        setTimeout(() => setMessage(null), 4000);
      }
    };

    const handleDownloadAllSubmitted = async () => {
      try {
        const res = await axios.get('http://localhost:5222/api/Attendance/submitted/download-all', {
          responseType: 'blob',
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        const url = window.URL.createObjectURL(new Blob([res.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `All_Submitted_Timesheets.zip`);
        document.body.appendChild(link);
        link.click();
        link.remove();
      } catch (err: any) {
        console.error("Failed to download all", err);
        setMessage({ type: 'error', text: err.response?.status === 404 ? 'No submitted timesheets found.' : 'Failed to download all submitted timesheets.' });
        setTimeout(() => setMessage(null), 4000);
      }
    };

  const handleSelectAll = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.checked) {
      setSelectedFiles(submittedFiles.map(f => f.fileName));
    } else {
      setSelectedFiles([]);
    }
  };

  const handleSelectRow = (fileName: string) => {
    if (selectedFiles.includes(fileName)) {
      setSelectedFiles(selectedFiles.filter(f => f !== fileName));
    } else {
      setSelectedFiles([...selectedFiles, fileName]);
    }
  };

  const confirmDeleteSubmitted = (fileNames: string[]) => {
    setFilesToDelete(fileNames);
    setOpenDelete(true);
  };

  const handleDeleteSubmitted = async () => {
    if (filesToDelete.length === 0) return;
    setDeleting(true);
    try {
      await Promise.all(filesToDelete.map(fileName => 
        axios.delete(`http://localhost:5222/api/Attendance/submitted/${fileName}`, {
          headers: { Authorization: `Bearer ${user?.token}` }
        })
      ));
      setMessage({ type: 'success', text: 'Timesheet(s) deleted successfully.' });
      setTimeout(() => setMessage(null), 4000);
      setOpenDelete(false);
      setFilesToDelete([]);
      setSelectedFiles([]);
      fetchSubmitted();
    } catch (err) {
      console.error("Failed to delete", err);
      setMessage({ type: 'error', text: 'Failed to delete some or all timesheets.' });
      setTimeout(() => setMessage(null), 4000);
    } finally {
      setDeleting(false);
    }
  };

  return (
    <Box>
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs value={tab} onChange={(_, v) => setTab(v)} variant="scrollable" scrollButtons="auto" allowScrollButtonsMobile>
          <Tab label="Raw Machine Logs" />
          <Tab label="Submitted Final Timesheets" />
        </Tabs>
      </Box>

      {message && <Alert severity={message.type} sx={{ mb: 3 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      {tab === 0 && (
        <>
          <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' }, gap: 2, mb: 3 }}>
            <Typography variant="h4" sx={{ m: 0, fontWeight: 'normal', fontSize: { xs: '1.75rem', sm: '2.125rem' } }}>
              Recent Raw Timesheets
            </Typography>
        <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, gap: 2, width: { xs: '100%', sm: 'auto' } }}>
          <Button variant="outlined" color="error" startIcon={<Trash size={18} />} onClick={() => setOpenClearRaw(true)}>
            Clear Data
          </Button>
          <Button variant="outlined" startIcon={<Download size={18} />} onClick={() => setOpenExport(true)}>
            Export
          </Button>
          <Button variant="contained" color="secondary" onClick={() => setOpenFetchDialog(true)} disabled={fetchingDevice}>
            {fetchingDevice ? <CircularProgress size={24} color="inherit" /> : 'Fetch from Device'}
          </Button>
          <Button variant="contained" startIcon={<UploadCloud size={18} />} onClick={() => setOpenImport(true)}>
            Import Excel
          </Button>
        </Box>
      </Box>

      <TableContainer component={Paper} elevation={2}>
        <Table sx={{ minWidth: 650 }}>
          <TableHead sx={{ bgcolor: 'rgba(0,0,0,0.02)' }}>
            <TableRow>
              <TableCell sx={{ fontWeight: 'normal' }}>Employee</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Date</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Check In</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Check Out</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Status</TableCell>
              <TableCell align="right" sx={{ fontWeight: 'normal' }}>Late</TableCell>
              <TableCell align="right" sx={{ fontWeight: 'normal' }}>Early</TableCell>
              <TableCell align="right" sx={{ fontWeight: 'normal' }}>Overtime</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow><TableCell colSpan={6} align="center" sx={{ py: 5 }}><CircularProgress /></TableCell></TableRow>
            ) : logs.length === 0 ? (
              <TableRow><TableCell colSpan={8} align="center" sx={{ py: 5 }}><Typography color="text.secondary">No attendance records for today.</Typography></TableCell></TableRow>
            ) : (
              logs.map((row) => (
                <TableRow key={row.id}>
                  <TableCell>{row.employeeName}</TableCell>
                  <TableCell>{row.date}</TableCell>
                  <TableCell>{formatTime12Hour(row.checkIn)}</TableCell>
                  <TableCell>{formatTime12Hour(row.checkOut)}</TableCell>
                  <TableCell>
                    <Chip 
                      label={row.lateMinutes > 0 ? 'Late Arrival' : row.status} 
                      color={getStatusColor(row.status, row.lateMinutes) as any} 
                      size="small" 
                    />
                  </TableCell>
                  <TableCell align="right">
                    {row.lateMinutes > 0 ? <Typography color="warning.main" sx={{ fontWeight: 'normal', fontSize: '0.875rem' }}>{formatMinutes(row.lateMinutes)}</Typography> : '-'}
                  </TableCell>
                  <TableCell align="right">
                    {row.earlyLeaveMinutes > 0 ? <Typography color="error.main" sx={{ fontWeight: 'normal', fontSize: '0.875rem' }}>{formatMinutes(row.earlyLeaveMinutes)}</Typography> : '-'}
                  </TableCell>
                  <TableCell align="right">
                    {row.overtimeMinutes > 0 ? <Typography color="success.main" sx={{ fontWeight: 'normal', fontSize: '0.875rem' }}>{formatMinutes(row.overtimeMinutes)}</Typography> : '-'}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
      </>
      )}

      {tab === 1 && (
        <Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4" sx={{ fontWeight: 'normal', fontSize: { xs: '1.75rem', sm: '2.125rem' } }}>
            Submitted Final Timesheets
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, gap: 2, width: { xs: '100%', sm: 'auto' } }}>
            <Button variant="outlined" startIcon={<Download size={18} />} onClick={handleDownloadAllSubmitted}>
              Download All
            </Button>
            {selectedFiles.length > 0 && (
              <Button variant="contained" color="error" startIcon={<Trash size={18} />} onClick={() => confirmDeleteSubmitted(selectedFiles)}>
                Delete Selected ({selectedFiles.length})
              </Button>
            )}
          </Box>
        </Box>
          <TableContainer component={Paper} elevation={2}>
            <Table>
              <TableHead sx={{ bgcolor: 'rgba(0,0,0,0.02)' }}>
                <TableRow>
                  <TableCell padding="checkbox">
                    <Checkbox
                      indeterminate={selectedFiles.length > 0 && selectedFiles.length < submittedFiles.length}
                      checked={submittedFiles.length > 0 && selectedFiles.length === submittedFiles.length}
                      onChange={handleSelectAll}
                    />
                  </TableCell>
                  <TableCell sx={{ fontWeight: 'normal' }}>Employee Name</TableCell>
                  <TableCell sx={{ fontWeight: 'normal' }}>Submission Date</TableCell>
                  <TableCell sx={{ fontWeight: 'normal' }}>File Size</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 'normal' }}>Action</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {loadingSubmitted ? (
                  <TableRow><TableCell colSpan={5} align="center" sx={{ py: 5 }}><CircularProgress /></TableCell></TableRow>
                ) : submittedFiles.length === 0 ? (
                  <TableRow><TableCell colSpan={5} align="center" sx={{ py: 5 }}><Typography color="text.secondary">No submitted timesheets found.</Typography></TableCell></TableRow>
                ) : (
                  submittedFiles.map((file, idx) => (
                    <TableRow key={idx}>
                      <TableCell padding="checkbox">
                        <Checkbox
                          checked={selectedFiles.includes(file.fileName)}
                          onChange={() => handleSelectRow(file.fileName)}
                        />
                      </TableCell>
                      <TableCell>{file.employeeName}</TableCell>
                      <TableCell>{new Date(file.submittedAt).toLocaleString()}</TableCell>
                      <TableCell>{Math.round(file.fileSizeBytes / 1024)} KB</TableCell>
                      <TableCell align="right">
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                          <Button variant="outlined" size="small" startIcon={<Download size={16} />} onClick={() => handleDownloadSubmitted(file.fileName)}>
                            Download
                          </Button>
                          <Button variant="outlined" color="error" size="small" onClick={() => confirmDeleteSubmitted([file.fileName])}>
                            <Trash size={16} />
                          </Button>
                        </Box>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
      )}

      <Dialog open={openImport} onClose={() => setOpenImport(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Import Timesheet</DialogTitle>
        <DialogContent>
          <Box sx={{ border: '2px dashed #CBD5E1', borderRadius: '12px', p: 4, textAlign: 'center', mt: 1 }}>
            <UploadCloud size={48} color="#2E7D32" style={{ marginBottom: '16px' }} />
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Select an Excel file (.xlsx) matching the template.
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
          <Button onClick={handleUpload} variant="contained" disabled={!file || importing}>
            {importing ? <CircularProgress size={24} /> : 'Process Import'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={openExport} onClose={() => setOpenExport(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Export HR Timesheets (All Employees)</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 1 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Select a date range to generate timesheets for all employees in a single Excel file (each employee in a separate tab).
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

      <Dialog open={openDelete} onClose={() => setOpenDelete(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ color: 'error.main', fontWeight: 'normal' }}>Delete Timesheet</DialogTitle>
        <DialogContent>
          <Box sx={{ textAlign: 'center', mt: 2, mb: 1 }}>
            <Trash size={48} color="#f44336" style={{ marginBottom: '16px' }} />
            <Typography variant="body1" sx={{ mb: 2 }}>
              Are you sure you want to delete {filesToDelete.length > 1 ? `these ${filesToDelete.length} submitted timesheets` : 'this submitted timesheet'}?
            </Typography>
            {filesToDelete.length === 1 && (
              <Typography variant="body2" sx={{ color: 'text.secondary', mb: 2, fontWeight: 'normal' }}>
                {filesToDelete[0]}
              </Typography>
            )}
            <Typography variant="body2" color="error">
              This action cannot be undone. The file will be permanently removed.
            </Typography>
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpenDelete(false)}>Cancel</Button>
          <Button onClick={handleDeleteSubmitted} variant="contained" color="error" disabled={deleting}>
            {deleting ? <CircularProgress size={24} color="inherit" /> : 'Yes, Delete'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={openClearRaw} onClose={() => setOpenClearRaw(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ color: 'error.main', fontWeight: 'normal' }}>Clear Raw Data</DialogTitle>
        <DialogContent>
          <Box sx={{ textAlign: 'center', mt: 2, mb: 1 }}>
            <Trash size={48} color="#f44336" style={{ marginBottom: '16px' }} />
            <Typography variant="body1" sx={{ mb: 2 }}>
              Are you sure you want to delete ALL raw attendance records?
            </Typography>
            <Typography variant="body2" color="error">
              This action cannot be undone. All imported logs will be permanently removed.
            </Typography>
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpenClearRaw(false)}>Cancel</Button>
          <Button onClick={handleClearRaw} variant="contained" color="error" disabled={clearingRaw}>
            {clearingRaw ? <CircularProgress size={24} color="inherit" /> : 'Yes, Clear All Data'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={openFetchDialog} onClose={() => setOpenFetchDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Fetch from Device</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 1 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Select a date and time range to fetch attendance logs from the biometric device.
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, flexDirection: 'column' }}>
              <Box>
                <Typography variant="caption" color="text.secondary">Start Date & Time</Typography>
                <input 
                  type="datetime-local"
                  value={fetchStartDate} 
                  onChange={(e) => setFetchStartDate(e.target.value)}
                  style={{ width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ccc', marginTop: '4px' }}
                />
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">End Date & Time</Typography>
                <input 
                  type="datetime-local"
                  value={fetchEndDate} 
                  onChange={(e) => setFetchEndDate(e.target.value)}
                  style={{ width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ccc', marginTop: '4px' }}
                />
              </Box>
            </Box>
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpenFetchDialog(false)}>Cancel</Button>
          <Button onClick={handleFetchDevice} variant="contained" disabled={!fetchStartDate || !fetchEndDate || fetchingDevice}>
            {fetchingDevice ? <CircularProgress size={24} /> : 'Fetch Logs'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}




