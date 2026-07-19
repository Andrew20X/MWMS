import { useState, useEffect } from 'react';
import { Box, Typography, Card, CardContent, CircularProgress, Button, Avatar, Chip, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Snackbar, Alert } from '@mui/material';
import { Check, X, Clock, Plus, Trash2 } from 'lucide-react';
import axios from 'axios';
import { formatTime12Hour } from '../utils/dateUtils';
import { useAuth } from '../contexts/AuthContext';

export default function Corrections() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';

  const [corrections, setCorrections] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [open, setOpen] = useState(false);
  const [confirmDialog, setConfirmDialog] = useState<{ open: boolean, id: number | null, action: 'approve' | 'reject', note: string }>({ open: false, id: null, action: 'approve', note: '' });
  const [deleteDialog, setDeleteDialog] = useState<{ open: boolean, id: number | null, action: 'delete' | 'deleteAll' }>({ open: false, id: null, action: 'delete' });
  const [formData, setFormData] = useState({ date: '', requestedCheckIn: '', requestedCheckOut: '', reason: '' });
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' | 'info' | 'warning' }>({ open: false, message: '', severity: 'info' });

  const showMessage = (message: string, severity: 'success' | 'error' | 'info' | 'warning' = 'info') => {
    setSnackbar({ open: true, message, severity });
  };

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const url = isAdmin ? 'http://localhost:5222/api/corrections' : 'http://localhost:5222/api/corrections/me';
      const res = await axios.get(url);
      setCorrections(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleConfirmAction = async () => {
    if (!confirmDialog.id) return;
    try {
      await axios.put(`http://localhost:5222/api/corrections/${confirmDialog.id}/${confirmDialog.action}`, { note: confirmDialog.note });
      setConfirmDialog({ open: false, id: null, action: 'approve', note: '' });
      fetchData();
      showMessage(`Correction request ${confirmDialog.action}d successfully.`, 'success');
    } catch (err) {
      console.error(err);
      showMessage(`Failed to ${confirmDialog.action} correction request.`, "error");
    }
  };

  const handleSubmitCorrection = async () => {
    if (!formData.date || !formData.reason) {
      showMessage("Please provide a Date and a Reason for the correction.", "warning");
      return;
    }

    if (!formData.requestedCheckIn || !formData.requestedCheckOut) {
      showMessage("Please provide both Check-In and Check-Out times.", "warning");
      return;
    }

    try {
      const data = {
        date: formData.date,
        reason: formData.reason,
        requestedCheckIn: formData.requestedCheckIn ? formData.requestedCheckIn + ':00' : null,
        requestedCheckOut: formData.requestedCheckOut ? formData.requestedCheckOut + ':00' : null
      };
      await axios.post('http://localhost:5222/api/Corrections', data);
      setOpen(false);
      setFormData({ date: '', requestedCheckIn: '', requestedCheckOut: '', reason: '' });
      fetchData();
      showMessage('Correction request submitted successfully.', 'success');
    } catch (err: any) {
      console.error(err);
      let errMsg = 'Failed to submit correction request. Please check your connection.';
      if (err.response?.data) {
        errMsg = typeof err.response.data === 'string' ? err.response.data : (err.response.data.title || errMsg);
      }
      showMessage(errMsg, "error");
    }
  };

  const handleDeleteCorrection = (id: number) => {
    setDeleteDialog({ open: true, id, action: 'delete' });
  };

  const handleDeleteAllCorrections = () => {
    setDeleteDialog({ open: true, id: null, action: 'deleteAll' });
  };

  const executeDelete = async () => {
    try {
      if (deleteDialog.action === 'deleteAll') {
        await axios.delete(`http://localhost:5222/api/corrections/all`, {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        showMessage('All correction requests deleted successfully.', 'success');
      } else if (deleteDialog.id) {
        await axios.delete(`http://localhost:5222/api/corrections/${deleteDialog.id}`, {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        showMessage('Correction request deleted successfully.', 'success');
      }
      fetchData();
    } catch (err) {
      showMessage('Failed to delete correction request(s).', 'error');
    } finally {
      setDeleteDialog({ open: false, id: null, action: 'delete' });
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' }, gap: 2, mb: 4 }}>
        <Typography variant="h4" sx={{ fontWeight: 'normal', color: '#1E293B', fontSize: { xs: '1.75rem', sm: '2.125rem' } }}>
          {isAdmin ? 'Pending Corrections' : 'My Corrections'}
        </Typography>
        {!isAdmin && (
          <Box sx={{ display: 'flex', gap: 2, width: { xs: '100%', sm: 'auto' } }}>
            <Button variant="outlined" color="error" startIcon={<Trash2 size={18} />} onClick={handleDeleteAllCorrections} sx={{ flex: { xs: 1, sm: 'none' } }}>
              Delete All
            </Button>
            <Button variant="contained" startIcon={<Plus size={18} />} onClick={() => setOpen(true)} sx={{ flex: { xs: 1, sm: 'none' } }}>
              Fix Missed Punch
            </Button>
          </Box>
        )}
      </Box>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 5 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(2, 1fr)', lg: 'repeat(3, 1fr)' }, gap: 3 }}>
          {corrections.map((req) => (
            <Box key={req.id}>
              <Card sx={{ height: '100%', borderRadius: 3, boxShadow: '0 4px 20px rgba(0,0,0,0.05)' }}>
                <CardContent>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                    <Avatar sx={{ bgcolor: 'primary.main' }}>
                      {req.employee?.firstName?.[0] || 'E'}
                    </Avatar>
                    <Box>
                      <Typography variant="subtitle1" sx={{ fontWeight: 'normal' }}>
                        {req.employee?.firstName} {req.employee?.lastName}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {req.employee?.employeeCode}
                      </Typography>
                    </Box>
                  </Box>

                  <Box sx={{ backgroundColor: '#F8FAFC', p: 2, borderRadius: 2, mb: 2 }}>
                    <Typography variant="body2" sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                      <Clock size={16} /> Date: <span>{new Date(req.date).toLocaleDateString()}</span>
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Req In: {formatTime12Hour(req.requestedCheckIn)}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Req Out: {formatTime12Hour(req.requestedCheckOut)}
                    </Typography>
                  </Box>

                  <Typography variant="body2" sx={{ fontStyle: 'italic', mb: req.adminNote ? 1 : 3 }}>
                    "{req.reason}"
                  </Typography>

                  {req.adminNote && (
                    <Box sx={{ backgroundColor: 'rgba(0,0,0,0.03)', p: 1.5, borderRadius: 2, mb: 3 }}>
                      <Typography variant="caption" sx={{ fontWeight: 'normal', display: 'block', mb: 0.5 }}>
                        Admin Note:
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {req.adminNote}
                      </Typography>
                    </Box>
                  )}

                  <Box sx={{ display: 'flex', gap: 2 }}>
                    {isAdmin ? (
                      <>
                        <Button 
                          variant="contained" 
                          color="success" 
                          fullWidth 
                          startIcon={<Check />}
                          onClick={() => setConfirmDialog({ open: true, id: req.id, action: 'approve', note: '' })}
                          sx={{ borderRadius: 2 }}
                        >
                          Approve
                        </Button>
                        <Button 
                          variant="outlined" 
                          color="error" 
                          fullWidth 
                          startIcon={<X />}
                          onClick={() => setConfirmDialog({ open: true, id: req.id, action: 'reject', note: '' })}
                          sx={{ borderRadius: 2 }}
                        >
                          Reject
                        </Button>
                      </>
                    ) : (
                      <>
                        <Chip 
                          label={req.status} 
                          color={req.status === 'Approved' ? 'success' : req.status === 'Rejected' ? 'error' : 'warning'} 
                          sx={{ width: '100%', borderRadius: 2 }} 
                        />
                        <Button 
                          variant="outlined" 
                          color="error" 
                          onClick={() => handleDeleteCorrection(req.id)}
                          sx={{ borderRadius: 2, minWidth: 'auto', p: 1 }}
                        >
                          <Trash2 size={18} />
                        </Button>
                      </>
                    )}
                  </Box>
                </CardContent>
              </Card>
            </Box>
          ))}
          {corrections.length === 0 && (
            <Box sx={{ gridColumn: '1 / -1' }}>
              <Typography variant="body1" color="text.secondary">
                No correction requests found.
              </Typography>
            </Box>
          )}
        </Box>
      )}

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 'normal' }}>Request Missed Punch Correction</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            type="date"
            label="Date"
            margin="normal"
            slotProps={{ inputLabel: { shrink: true } }}
            value={formData.date}
            onChange={(e) => setFormData({ ...formData, date: e.target.value })}
          />
          <TextField
            fullWidth
            type="time"
            label="Requested Check-In"
            margin="normal"
            slotProps={{ inputLabel: { shrink: true } }}
            value={formData.requestedCheckIn}
            onChange={(e) => setFormData({ ...formData, requestedCheckIn: e.target.value })}
          />
          <TextField
            fullWidth
            type="time"
            label="Requested Check-Out"
            margin="normal"
            slotProps={{ inputLabel: { shrink: true } }}
            value={formData.requestedCheckOut}
            onChange={(e) => setFormData({ ...formData, requestedCheckOut: e.target.value })}
          />
          <TextField
            fullWidth
            label="Reason"
            margin="normal"
            multiline
            rows={2}
            placeholder="e.g., Forgot to check out, power went down..."
            value={formData.reason}
            onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
          />
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setOpen(false)} color="inherit">Cancel</Button>
          <Button onClick={handleSubmitCorrection} variant="contained" color="secondary">Submit Request</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={confirmDialog.open} onClose={() => setConfirmDialog({ ...confirmDialog, open: false })} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 'normal' }}>
          Confirm {confirmDialog.action === 'approve' ? 'Approval' : 'Rejection'}
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            Are you sure you want to {confirmDialog.action} this correction request?
          </Typography>
          <TextField
            fullWidth
            label="Admin Note (Optional)"
            margin="dense"
            multiline
            rows={3}
            placeholder="Write a note to the employee..."
            value={confirmDialog.note}
            onChange={(e) => setConfirmDialog({ ...confirmDialog, note: e.target.value })}
          />
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setConfirmDialog({ ...confirmDialog, open: false })} color="inherit">Cancel</Button>
          <Button 
            onClick={handleConfirmAction} 
            variant="contained" 
            color={confirmDialog.action === 'approve' ? 'success' : 'error'}
          >
            Confirm {confirmDialog.action === 'approve' ? 'Approve' : 'Reject'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={deleteDialog.open} onClose={() => setDeleteDialog({ ...deleteDialog, open: false })} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 'normal', color: 'error.main' }}>
          Confirm Deletion
        </DialogTitle>
        <DialogContent>
          <Typography variant="body1" sx={{ mt: 1 }}>
            {deleteDialog.action === 'deleteAll' 
              ? "Are you sure you want to delete ALL your correction requests? This action cannot be undone."
              : "Are you sure you want to delete this correction request?"}
          </Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setDeleteDialog({ ...deleteDialog, open: false })} color="inherit">Cancel</Button>
          <Button onClick={executeDelete} variant="contained" color="error">
            Delete
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={snackbar.open} autoHideDuration={6000} onClose={() => setSnackbar({ ...snackbar, open: false })}>
        <Alert onClose={() => setSnackbar({ ...snackbar, open: false })} severity={snackbar.severity} sx={{ width: '100%' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}




