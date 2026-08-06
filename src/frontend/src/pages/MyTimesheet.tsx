import { useState, useEffect } from 'react';
import { Box, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Button, Dialog, DialogTitle, DialogContent, DialogActions, Chip, CircularProgress, Snackbar, Alert } from '@mui/material';
import { Download, UploadCloud, Trash, FilePlus, Folder, Square, Edit } from 'lucide-react';
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

  const [openClear, setOpenClear] = useState(false);
  const [importFile, setImportFile] = useState<File | null>(null);
  const [submissionFiles, setSubmissionFiles] = useState<File[]>([]);
  const [importing, setImporting] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [clearing, setClearing] = useState(false);
  const [fetchingDevice, setFetchingDevice] = useState(false);
  const [startDate, setStartDate] = useState(new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0]);
  const [endDate, setEndDate] = useState(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0).toISOString().split('T')[0]);
  const [deadline, setDeadline] = useState<string | null>(null);
  const [mySubmissions, setMySubmissions] = useState<any[]>([]);
  const [dragActive, setDragActive] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' | 'info' | 'warning' }>({ open: false, message: '', severity: 'info' });

  const [showSubmissionForm, setShowSubmissionForm] = useState(false);
  const [saveAs, setSaveAs] = useState('');
  const [author, setAuthor] = useState('');
  const [license, setLicense] = useState('All rights reserved');

  const [comments, setComments] = useState<any[]>([]);
  const [isCommentsExpanded, setIsCommentsExpanded] = useState(false);
  const [newComment, setNewComment] = useState("");
  const [addingComment, setAddingComment] = useState(false);
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

      const setRes = await axios.get('http://localhost:5222/api/Attendance/settings');
      setDeadline(setRes.data.deadline);

      const subRes = await axios.get('http://localhost:5222/api/Attendance/submitted/my-timesheets', {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setMySubmissions(subRes.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (mySubmissions.length > 0) {
      axios.get(`http://localhost:5222/api/Attendance/submitted/comments/${mySubmissions[0].fileName}`, {
        headers: { Authorization: `Bearer ${user?.token}` }
      }).then(res => {
        setComments(res.data);
      }).catch(err => console.error(err));
    } else {
      setComments([]);
    }
  }, [mySubmissions, user]);

  const handleAddComment = async () => {
    if (!newComment.trim() || mySubmissions.length === 0) return;
    setAddingComment(true);
    try {
      const res = await axios.post(`http://localhost:5222/api/Attendance/submitted/comments/${mySubmissions[0].fileName}`, { commentText: newComment }, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setComments([...comments, res.data]);
      setNewComment('');
      showMessage('Comment added successfully.', 'success');
    } catch(e) {
      showMessage('Failed to add comment.', 'error');
    } finally {
      setAddingComment(false);
    }
  };

  const handleDeleteComment = async (commentId: number) => {
    try {
      await axios.delete(`http://localhost:5222/api/Attendance/submitted/comments/${commentId}`, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setComments(comments.filter(c => c.id !== commentId));
      showMessage('Comment deleted successfully.', 'success');
    } catch(e: any) {
      if (e.response?.status === 403) {
        showMessage('You can only delete your own comments.', 'error');
      } else {
        showMessage('Failed to delete comment.', 'error');
      }
    }
  };

  const handleImportFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setImportFile(e.target.files[0]);
    }
  };

  const handleSubmissionFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const newFiles = Array.from(e.target.files).filter(f => f.name.endsWith('.xlsx'));
      setSubmissionFiles(prev => [...prev, ...newFiles]);
    }
  };

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === "dragenter" || e.type === "dragover") {
      setDragActive(true);
    } else if (e.type === "dragleave") {
      setDragActive(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
    if (e.dataTransfer.files) {
      const validFiles = Array.from(e.dataTransfer.files).filter(f => f.name.endsWith('.xlsx'));
      if (validFiles.length > 0) {
        setSubmissionFiles(prev => [...prev, ...validFiles]);
      } else {
        showMessage('Only .xlsx files are supported.', 'error');
      }
    }
  };

  const handleDeleteSubmission = async (fileName: string) => {
    try {
      await axios.delete(`http://localhost:5222/api/Attendance/submitted/my-timesheets/${fileName}`, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      showMessage('Submission deleted.', 'success');
      fetchData();
    } catch(e) {
      showMessage('Failed to delete submission.', 'error');
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
    if (!importFile) return;
    const formData = new FormData();
    formData.append('file', importFile);
    setImporting(true);

    try {
      await axios.post('http://localhost:5222/api/Attendance/import/me', formData, {
        headers: { 'Content-Type': 'multipart/form-data', Authorization: `Bearer ${user?.token}` }
      });
      showMessage('Raw timesheet imported successfully!', "success");
      setImportFile(null);
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
    if (submissionFiles.length === 0) return;
    const formData = new FormData();
    submissionFiles.forEach(f => formData.append('files', f));
    setSubmitting(true);

    try {
      await axios.post('http://localhost:5222/api/Attendance/upload-final/me', formData, {
        headers: { 'Content-Type': 'multipart/form-data', Authorization: `Bearer ${user?.token}` }
      });
      showMessage('Final timesheet submitted to HR successfully!', "success");
      setSubmissionFiles([]);
      setShowSubmissionForm(false);
      fetchData();
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
          <Button variant="outlined" startIcon={<UploadCloud size={18} />} onClick={() => {setImportFile(null); setOpenImport(true);}} sx={{ borderRadius: 2, flex: { xs: 1, sm: 'none' } }}>
            1. Import Raw
          </Button>
          <Button variant="outlined" startIcon={<Download size={18} />} onClick={() => setOpenExport(true)} sx={{ borderRadius: 2, flex: { xs: 1, sm: 'none' } }}>
            2. Export Format
          </Button>
        </Box>
      </Box>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 5 }}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          {deadline && (
            <Alert severity={new Date(deadline) < new Date() ? 'error' : 'warning'} sx={{ mb: 3 }}>
              Timesheet Submission Deadline: {new Date(deadline).toLocaleString()}
              {new Date(deadline) < new Date() && ' - The deadline has passed. Submissions are now closed.'}
            </Alert>
          )}

          {/* MOODLE SUBMISSION UI */}
          <Box sx={{ mb: 5, border: '1px solid #dee2e6', borderRadius: '4px', overflow: 'hidden', bgcolor: '#fff' }}>
            <Box sx={{ p: 2, borderBottom: '1px solid #dee2e6' }}>
              <Typography variant="h5" sx={{ fontWeight: 'normal', color: '#333' }}>Timesheet Submission</Typography>
            </Box>
            
            {!showSubmissionForm ? (
              <Box sx={{ p: 3 }}>
                <Typography variant="h6" sx={{ fontWeight: 'normal', mb: 2, color: '#333' }}>Submission status</Typography>
                <TableContainer>
                  <Table sx={{ border: '1px solid #dee2e6', '& td, & th': { border: '1px solid #dee2e6', p: { xs: 1, sm: 1.5 }, fontSize: { xs: '0.75rem', sm: '0.875rem' } } }}>
                    <TableBody>
                      <TableRow>
                        <TableCell sx={{ bgcolor: '#f8f9fa', fontWeight: 'bold', width: '25%' }}>Due date</TableCell>
                        <TableCell>{deadline ? new Date(deadline).toLocaleString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' }) : '-'}</TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell sx={{ bgcolor: '#f8f9fa', fontWeight: 'bold' }}>Time remaining</TableCell>
                        <TableCell>
                          {(() => {
                            if (!deadline) return "-";
                            const due = new Date(deadline);
                            const now = new Date();
                            const diffMs = due.getTime() - now.getTime();
                            const isOverdue = diffMs < 0;
                            const absDiff = Math.abs(diffMs);
                            const days = Math.floor(absDiff / (1000 * 60 * 60 * 24));
                            const hours = Math.floor((absDiff / (1000 * 60 * 60)) % 24);
                            const mins = Math.floor((absDiff / 1000 / 60) % 60);
                            
                            let timeStr = "";
                            if (days > 0) timeStr += `${days} days `;
                            if (hours > 0) timeStr += `${hours} hours `;
                            if (mins > 0 || (days === 0 && hours === 0)) timeStr += `${mins} mins`;
                            if (timeStr === "") timeStr = "0 mins";
                            timeStr = timeStr.trim();
                            
                            if (isOverdue) return <span style={{ color: '#d9534f' }}>Assignment is overdue by: {timeStr}</span>;
                            return `${timeStr}`;
                          })()}
                        </TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell sx={{ bgcolor: '#f8f9fa', fontWeight: 'bold' }}>Last modified</TableCell>
                        <TableCell>{mySubmissions.length > 0 ? new Date(mySubmissions[0].submittedAt).toLocaleString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' }) : '-'}</TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell sx={{ bgcolor: '#f8f9fa', fontWeight: 'bold' }}>File submissions</TableCell>
                        <TableCell>
                          {mySubmissions.length > 0 ? (
                            mySubmissions.map((sub, idx) => (
                              <Box key={idx} sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                                <Typography sx={{ color: '#0056b3', display: 'flex', alignItems: 'center', gap: 1, fontSize: { xs: '0.75rem', sm: '0.875rem' }, wordBreak: 'break-all' }}>📄 {sub.fileName}</Typography>
                                {(!deadline || new Date(deadline) >= new Date()) && (
                                  <Button color="error" size="small" onClick={() => handleDeleteSubmission(sub.fileName)} sx={{ minWidth: 'auto', p: 0.5, textTransform: 'none' }}>
                                    <Trash size={14} />
                                  </Button>
                                )}
                              </Box>
                            ))
                          ) : '-'}
                        </TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell sx={{ bgcolor: '#f8f9fa', fontWeight: 'bold' }}>Submission comments</TableCell>
                        <TableCell>
                          <span 
                            style={{ color: '#0056b3', cursor: 'pointer' }}
                            onClick={() => setIsCommentsExpanded(!isCommentsExpanded)}
                          >
                            {isCommentsExpanded ? '▼' : '▶'} Comments ({comments.length})
                          </span>
                          {isCommentsExpanded && (
                            <Box sx={{ mt: 2, p: 2, border: '1px solid #dee2e6', borderRadius: '4px', bgcolor: '#fff' }}>
                              {comments.map((c, i) => (
                                <Box key={i} sx={{ mb: 2, pb: 1, borderBottom: '1px solid #f1f3f5' }}>
                                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                    <Box>
                                      <Typography variant="body2" sx={{ fontWeight: 'bold' }}>{c.author} <span style={{ fontWeight: 'normal', color: '#6c757d', fontSize: '0.85em' }}>- {new Date(c.createdAt).toLocaleString()}</span></Typography>
                                      <Typography variant="body2">{c.commentText}</Typography>
                                    </Box>
                                    {(!deadline || new Date(deadline) >= new Date()) && c.author !== 'System Admin' && (c.employeeId == user?.employeeId || user?.role === 'Admin') && (
                                      <Button 
                                        size="small" 
                                        color="error" 
                                        onClick={() => handleDeleteComment(c.id)}
                                        sx={{ minWidth: 'auto', p: 0.5, textTransform: 'none', fontSize: '0.75rem' }}
                                      >
                                        Delete
                                      </Button>
                                    )}
                                  </Box>
                                </Box>
                              ))}
                              {comments.length === 0 && <Typography variant="body2" sx={{ color: '#6c757d', mb: 2 }}>No comments yet.</Typography>}
                              
                              {(!deadline || new Date(deadline) >= new Date()) && (
                                <>
                                  <textarea 
                                    value={newComment}
                                    onChange={(e) => setNewComment(e.target.value)}
                                    placeholder="Add a comment..."
                                    style={{ width: '100%', minHeight: '80px', padding: '8px', border: '1px solid #ced4da', borderRadius: '4px', marginBottom: '8px' }}
                                  />
                                  <Button 
                                    variant="contained" 
                                    size="small" 
                                    onClick={handleAddComment} 
                                    disabled={addingComment || !newComment.trim()}
                                    sx={{ bgcolor: '#0069d9', textTransform: 'none', boxShadow: 'none' }}
                                  >
                                    {addingComment ? 'Saving...' : 'Save comment'}
                                  </Button>
                                </>
                              )}
                            </Box>
                          )}
                        </TableCell>
                      </TableRow>
                    </TableBody>
                  </Table>
                </TableContainer>
                {(!deadline || new Date(deadline) >= new Date()) && (
                  <Box sx={{ mt: 3, textAlign: 'center' }}>
                    <Button 
                      variant="contained" 
                      sx={{ bgcolor: '#ced4da', color: '#212529', boxShadow: 'none', textTransform: 'none', '&:hover': { bgcolor: '#b1b8be', boxShadow: 'none' } }}
                      onClick={() => { setSubmissionFiles([]); setShowSubmissionForm(true); }}
                    >
                      Add submission
                    </Button>
                  </Box>
                )}
              </Box>
            ) : (
              <Box sx={{ p: 3 }}>
                <Typography variant="h6" sx={{ fontWeight: 'normal', mb: 2, color: '#333' }}>File submissions</Typography>
                <Typography variant="body2" sx={{ textAlign: 'right', color: '#333', mb: 1 }}>Maximum size for new files: 1GB, maximum attachments: 20</Typography>
                <Box sx={{ border: '1px solid #dee2e6', borderRadius: '4px', p: 0, bgcolor: '#fff' }}>
                  <Box sx={{ bgcolor: '#f8f9fa', borderBottom: '1px solid #dee2e6', p: 1, display: 'flex', gap: 1 }}>
                    <Box sx={{ width: 30, height: 30, bgcolor: '#e2e6ea', display: 'flex', justifyContent: 'center', alignItems: 'center' }}><FilePlus size={16} color="#6c757d" /></Box>
                    <Box sx={{ width: 30, height: 30, display: 'flex', justifyContent: 'center', alignItems: 'center' }}><Folder size={16} color="#ffc107" fill="#ffc107" /></Box>
                    <Box sx={{ width: 30, height: 30, bgcolor: '#6c757d', color: '#fff', display: 'flex', justifyContent: 'center', alignItems: 'center' }}><Square size={16} color="#fff" /></Box>
                    <Box sx={{ width: 30, height: 30, display: 'flex', justifyContent: 'center', alignItems: 'center' }}><Edit size={16} color="#6c757d" /></Box>
                  </Box>
                  <Box sx={{ p: 1, borderBottom: '1px solid #dee2e6' }}>
                    <Typography variant="body2" sx={{ display: 'flex', alignItems: 'center', gap: 1, color: '#0056b3' }}>
                      <Folder size={16} color="#ffc107" fill="#ffc107" /> <span style={{ color: '#0056b3' }}>Files</span>
                    </Typography>
                  </Box>
                  <Box sx={{ p: 2, bgcolor: '#fff' }}>
                    <Box 
                      onDragEnter={handleDrag}
                      onDragLeave={handleDrag}
                      onDragOver={handleDrag}
                      onDrop={handleDrop}
                      sx={{ 
                        border: '2px dashed', 
                        borderColor: dragActive ? '#0056b3' : '#ced4da', 
                        bgcolor: dragActive ? '#f8f9fa' : '#fff',
                        p: 5, 
                        textAlign: 'center',
                        position: 'relative',
                        cursor: 'pointer'
                      }}
                    >
                      <input type="file" multiple style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', opacity: 0, cursor: 'pointer' }} accept=".xlsx" onChange={handleSubmissionFileChange} />
                      <Box sx={{ mb: 2 }}>
                        <svg width="48" height="48" viewBox="0 0 24 24" fill="#007bff"><path d="M12 19L5 12h5V5h4v7h5z"/></svg>
                      </Box>
                      {submissionFiles.length > 0 ? (
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, alignItems: 'center', position: 'relative', zIndex: 10 }}>
                          {submissionFiles.map((f, i) => (
                            <Typography key={i} variant="body1" sx={{ color: '#28a745', display: 'flex', alignItems: 'center', gap: 1 }}>
                              {f.name}
                              <Button size="small" color="error" sx={{ minWidth: 'auto', p: 0.5 }} onClick={(e) => { e.stopPropagation(); e.preventDefault(); setSubmissionFiles(submissionFiles.filter((_, idx) => idx !== i)); }}>
                                <Trash size={14} />
                              </Button>
                            </Typography>
                          ))}
                        </Box>
                      ) : (
                        <Typography variant="body1" sx={{ color: '#333' }}>You can drag and drop multiple files here to add them.</Typography>
                      )}
                    </Box>
                  </Box>
                </Box>
                
                {submissionFiles.length > 0 && (
                  <Box sx={{ mt: 3, p: 2, border: '1px solid #dee2e6' }}>
                    <Box sx={{ display: 'flex', mb: 2, alignItems: 'center', flexWrap: 'wrap', gap: 1 }}>
                      <Typography sx={{ width: '150px' }}>Attachment(s)</Typography>
                      <Button variant="contained" component="label" sx={{ bgcolor: '#e9ecef', color: '#333', boxShadow: 'none', textTransform: 'none', '&:hover': { bgcolor: '#dae0e5' } }}>
                        Choose File
                        <input type="file" multiple hidden accept=".xlsx" onChange={handleSubmissionFileChange} />
                      </Button>
                      <Typography sx={{ ml: 2, color: '#6c757d' }}>{submissionFiles.length} file(s) selected</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', mb: 2, alignItems: 'center' }}>
                      <Typography sx={{ width: '150px' }}>Save as</Typography>
                      <input type="text" value={saveAs} onChange={e => setSaveAs(e.target.value)} style={{ flex: 1, padding: '8px', border: '1px solid #ced4da', borderRadius: '4px' }} />
                    </Box>
                    <Box sx={{ display: 'flex', mb: 2, alignItems: 'center' }}>
                      <Typography sx={{ width: '150px' }}>Author</Typography>
                      <input type="text" value={author} onChange={e => setAuthor(e.target.value)} style={{ flex: 1, padding: '8px', border: '1px solid #ced4da', borderRadius: '4px' }} />
                    </Box>
                    <Box sx={{ display: 'flex', mb: 2, alignItems: 'center' }}>
                      <Typography sx={{ width: '150px' }}>Choose license</Typography>
                      <select value={license} onChange={e => setLicense(e.target.value)} style={{ flex: 1, padding: '8px', border: '1px solid #ced4da', borderRadius: '4px' }}>
                        <option value="All rights reserved">All rights reserved</option>
                        <option value="Public domain">Public domain</option>
                        <option value="Creative Commons">Creative Commons</option>
                      </select>
                    </Box>
                    <Box sx={{ mt: 3, textAlign: 'center' }}>
                      <Button onClick={handleSubmitFinal} variant="contained" disabled={submitting} sx={{ mr: 2, bgcolor: '#0069d9', textTransform: 'none', boxShadow: 'none' }}>
                        {submitting ? <CircularProgress size={24} color="inherit" /> : 'Upload this file'}
                      </Button>
                      <Button onClick={() => setShowSubmissionForm(false)} sx={{ bgcolor: '#e9ecef', color: '#333', textTransform: 'none' }}>Cancel</Button>
                    </Box>
                  </Box>
                )}
                
                {submissionFiles.length === 0 && (
                  <Box sx={{ mt: 3, textAlign: 'center' }}>
                    <Button variant="contained" sx={{ mr: 2, bgcolor: '#0069d9', textTransform: 'none', boxShadow: 'none' }} onClick={() => setShowSubmissionForm(false)}>Save changes</Button>
                    <Button onClick={() => setShowSubmissionForm(false)} sx={{ bgcolor: '#ced4da', color: '#212529', textTransform: 'none' }}>Cancel</Button>
                  </Box>
                )}
              </Box>
            )}
          </Box>

          <Typography variant="h6" sx={{ mb: 2, fontWeight: 'normal' }}>
            Recent Attendance (Count: {attendance ? attendance.length : 'null'})
          </Typography>
          <TableContainer component={Paper} sx={{ borderRadius: 3, boxShadow: '0 4px 20px rgba(0,0,0,0.05)' }}>
            <Table sx={{ minWidth: 650, '& .MuiTableCell-root': { whiteSpace: 'nowrap' } }}>
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
        <DialogActions sx={{ p: 3, pt: 2 }}>
          <Button onClick={() => setOpenFetch(false)} sx={{ color: '#476282', fontWeight: 'bold' }}>Cancel</Button>
          <Button 
            onClick={handleFetchDevice} 
            variant="contained" 
            disabled={!startDate || !endDate || fetchingDevice}
            sx={{ bgcolor: '#476282', textTransform: 'none', fontWeight: 'bold', '&:hover': { bgcolor: '#364d68' } }}
          >
            {fetchingDevice ? <CircularProgress size={24} sx={{ color: 'white' }} /> : 'Fetch Data'}
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
              Select your ZKTeco or raw timesheet file (.xlsx) to import it into your personal records.
            </Typography>
            <input type="file" accept=".xlsx" onChange={handleImportFileChange} />
            {importFile && <Typography variant="body2" sx={{ mt: 2, color: '#4CAF50' }}>{importFile.name}</Typography>}
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpenImport(false)}>Cancel</Button>
          <Button onClick={handleImport} variant="contained" disabled={!importFile || importing}>
            {importing ? <CircularProgress size={24} /> : 'Import'}
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




