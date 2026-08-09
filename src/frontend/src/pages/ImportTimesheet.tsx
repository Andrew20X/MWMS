import { useState, useEffect } from 'react';
import { Typography, Box, Paper, Button, Alert, CircularProgress, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Dialog, DialogTitle, DialogContent, DialogActions, Checkbox, TextField } from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { Download, Trash, MessageSquare, Calendar } from 'lucide-react';
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

  const [openComments, setOpenComments] = useState(false);
  const [commentsFileName, setCommentsFileName] = useState('');
  const [commentsList, setCommentsList] = useState<any[]>([]);
  const [newComment, setNewComment] = useState('');
  const [addingComment, setAddingComment] = useState(false);

  const [deadline, setDeadline] = useState<string | null>(null);
  const [openDeadlineDialog, setOpenDeadlineDialog] = useState(false);
  const [newDeadline, setNewDeadline] = useState('');
  const [updatingDeadline, setUpdatingDeadline] = useState(false);

  const fetchDeadline = async () => {
    try {
      const res = await axios.get('http://localhost:5222/api/Attendance/settings', {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      if (res.data.deadline) {
        setDeadline(res.data.deadline);
        const localDt = new Date(res.data.deadline);
        localDt.setMinutes(localDt.getMinutes() - localDt.getTimezoneOffset());
        setNewDeadline(localDt.toISOString().slice(0, 16));
      } else {
        setDeadline(null);
        setNewDeadline('');
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleUpdateDeadline = async () => {
    setUpdatingDeadline(true);
    try {
      await axios.post('http://localhost:5222/api/Attendance/settings', 
        { deadline: newDeadline || null },
        { headers: { Authorization: `Bearer ${user?.token}` } }
      );
      setDeadline(newDeadline || null);
      setOpenDeadlineDialog(false);
      setMessage({ type: 'success', text: 'Deadline updated successfully.' });
      setTimeout(() => setMessage(null), 4000);
    } catch (err) {
      console.error(err);
      setMessage({ type: 'error', text: 'Failed to update deadline.' });
      setTimeout(() => setMessage(null), 4000);
    } finally {
      setUpdatingDeadline(false);
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
    fetchSubmitted();
    fetchDeadline();
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

  const handleViewComments = async (fileName: string) => {
    setCommentsFileName(fileName);
    setOpenComments(true);
    setCommentsList([]);
    try {
      const res = await axios.get(`http://localhost:5222/api/Attendance/submitted/comments/${fileName}`, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setCommentsList(res.data);
    } catch(err) {
      console.error(err);
    }
  };

  const handleAddAdminComment = async () => {
    if (!newComment.trim() || !commentsFileName) return;
    setAddingComment(true);
    try {
      const res = await axios.post(`http://localhost:5222/api/Attendance/submitted/comments/${commentsFileName}`, { commentText: newComment }, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setCommentsList([...commentsList, res.data]);
      setNewComment('');
    } catch(err) {
      console.error(err);
    } finally {
      setAddingComment(false);
    }
  };

  const handleDeleteAdminComment = async (commentId: number) => {
    try {
      await axios.delete(`http://localhost:5222/api/Attendance/submitted/comments/${commentId}`, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setCommentsList(commentsList.filter(c => c.id !== commentId));
    } catch(err) {
      console.error(err);
    }
  };

  return (
    <Box>
      {message && <Alert severity={message.type} sx={{ mb: 3 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Box>
        <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' }, gap: 2, mb: 3 }}>
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 'normal', fontSize: { xs: '1.75rem', sm: '2.125rem' } }}>
              Submitted Final Timesheets
            </Typography>
            {deadline && (
              <Typography variant="body2" sx={{ color: 'text.secondary', mt: 0.5 }}>
                Current Deadline: {new Date(deadline).toLocaleString()}
              </Typography>
            )}
          </Box>
          <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, gap: 2, width: { xs: '100%', sm: 'auto' } }}>
            {user?.role === 'Admin' && (
              <Button variant="outlined" color="primary" startIcon={<Calendar size={18} />} onClick={() => setOpenDeadlineDialog(true)} sx={{ width: { xs: '100%', sm: 'auto' } }}>
                Set Deadline
              </Button>
            )}
            <Button variant="outlined" startIcon={<Download size={18} />} onClick={handleDownloadAllSubmitted} sx={{ width: { xs: '100%', sm: 'auto' } }}>
              Download All
            </Button>
            {selectedFiles.length > 0 && (
              <Button variant="contained" color="error" startIcon={<Trash size={18} />} onClick={() => confirmDeleteSubmitted(selectedFiles)} sx={{ width: { xs: '100%', sm: 'auto' } }}>
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
                  submittedFiles.map((file: any, idx: number) => (
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
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end', alignItems: 'center', gap: 1 }}>
                          <Box 
                            onClick={() => handleViewComments(file.fileName)} 
                            sx={{ 
                              cursor: 'pointer', 
                              bgcolor: file.commentCount > 0 ? '#f0f9ff' : 'transparent',
                              p: 0.5,
                              px: 1,
                              borderRadius: 1,
                              border: '1px solid',
                              borderColor: file.commentCount > 0 ? '#bae6fd' : '#e2e8f0',
                              display: 'flex',
                              alignItems: 'center',
                              '&:hover': { bgcolor: file.commentCount > 0 ? '#e0f2fe' : '#f8f9fa' }
                            }}
                          >
                            {file.latestComment ? (
                              <Typography variant="caption" sx={{ color: '#0284c7', fontWeight: 'bold', maxWidth: '200px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis', display: 'flex', alignItems: 'center' }}>
                                <MessageSquare size={14} style={{ marginRight: '6px' }} />
                                "{file.latestComment}" {file.commentCount > 1 && `(+${file.commentCount - 1})`}
                              </Typography>
                            ) : (
                              <Typography variant="caption" sx={{ color: '#94a3b8', display: 'flex', alignItems: 'center' }}>
                                <MessageSquare size={14} style={{ marginRight: '6px' }} />
                                Add comment...
                              </Typography>
                            )}
                          </Box>

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

      <Dialog open={openComments} onClose={() => setOpenComments(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Submission Comments</DialogTitle>
        <DialogContent dividers>
          {commentsList.map((c, i) => (
            <Box key={i} sx={{ mb: 2, pb: 1, borderBottom: '1px solid #f1f3f5' }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <Box>
                  <Typography variant="body2" sx={{ fontWeight: 'bold' }}>{c.author} <span style={{ fontWeight: 'normal', color: '#6c757d', fontSize: '0.85em' }}>- {new Date(c.createdAt).toLocaleString()}</span></Typography>
                  <Typography variant="body2">{c.commentText}</Typography>
                </Box>
                {c.author === 'System Admin' && (
                  <Button size="small" color="error" onClick={() => handleDeleteAdminComment(c.id)} sx={{ minWidth: 'auto', p: 0.5, textTransform: 'none', fontSize: '0.75rem' }}>
                    Delete
                  </Button>
                )}
              </Box>
            </Box>
          ))}
          {commentsList.length === 0 && <Typography variant="body2" sx={{ color: '#6c757d', mb: 2 }}>No comments yet.</Typography>}
          
          <Box sx={{ mt: 2 }}>
            <textarea 
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              placeholder="Add a comment..."
              style={{ width: '100%', minHeight: '80px', padding: '8px', border: '1px solid #ced4da', borderRadius: '4px', marginBottom: '8px' }}
            />
            <Button 
              variant="contained" 
              size="small" 
              onClick={handleAddAdminComment} 
              disabled={addingComment || !newComment.trim()}
              sx={{ bgcolor: '#0069d9', textTransform: 'none', boxShadow: 'none' }}
            >
              {addingComment ? 'Saving...' : 'Save comment'}
            </Button>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenComments(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={openDeadlineDialog} onClose={() => setOpenDeadlineDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Set Submission Deadline</DialogTitle>
        <DialogContent dividers>
          <Typography variant="body2" sx={{ mb: 3, color: 'text.secondary' }}>
            Set a deadline for employees to submit their timesheets. Submissions after this date will be marked as late or blocked. Leave blank to remove the deadline.
          </Typography>
          <TextField
            fullWidth
            label="Deadline Date & Time"
            type="datetime-local"
            value={newDeadline}
            onChange={(e) => setNewDeadline(e.target.value)}
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </DialogContent>
        <DialogActions sx={{ p: 2, pt: 0 }}>
          <Button onClick={() => setOpenDeadlineDialog(false)}>Cancel</Button>
          <Button onClick={handleUpdateDeadline} variant="contained" disabled={updatingDeadline}>
            {updatingDeadline ? <CircularProgress size={24} color="inherit" /> : 'Save Deadline'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
