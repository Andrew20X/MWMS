import { useState, useEffect } from 'react';
import { Typography, Box, Paper, Button, Alert, CircularProgress, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Checkbox } from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { Download, Trash } from 'lucide-react';
import axios from 'axios';



export default function Timesheets() {
  const { user } = useAuth();
  const [openDelete, setOpenDelete] = useState(false);
  const [filesToDelete, setFilesToDelete] = useState<string[]>([]);
  const [selectedFiles, setSelectedFiles] = useState<string[]>([]);
  const [deleting, setDeleting] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error', text: string } | null>(null);
  
  const [submittedFiles, setSubmittedFiles] = useState<any[]>([]);
  const [loadingSubmitted, setLoadingSubmitted] = useState(false);

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
    fetchSubmitted();
  }, []);



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
      {message && <Alert severity={message.type} sx={{ mb: 3 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

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


    </Box>
  );
}




