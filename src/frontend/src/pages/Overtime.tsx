import { useState, useEffect } from 'react';
import { Box, Typography, Card, CardContent, CircularProgress, Button, Avatar, Chip, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Snackbar, Alert, MenuItem } from '@mui/material';
import { Check, X, Clock, Plus, Trash2 } from 'lucide-react';
import axios from 'axios';
import { formatTime12Hour } from '../utils/dateUtils';
import { useAuth } from '../contexts/AuthContext';

const OVERTIME_STATUS_LABELS: Record<string, string> = {
  PendingManagerApproval: 'Pending Manager Approval',
  PendingHRApproval: 'Pending Final Approval',
  Approved: 'Approved',
  Rejected: 'Rejected',
};

const getOvertimeStatusColor = (status: string): 'success' | 'error' | 'warning' | 'info' => {
  switch (status) {
    case 'Approved':               return 'success';
    case 'Rejected':               return 'error';
    case 'PendingHRApproval':      return 'info';
    default:                       return 'warning';
  }
};

export default function Overtime() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';
  const isHR = user?.role === 'HR';
  const isManager = user?.role === 'Manager';

  const canApprove = isAdmin || isManager || isHR;
  const [overtimes, setOvertimes] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [open, setOpen] = useState(false);
  const [confirmDialog, setConfirmDialog] = useState<{ open: boolean, id: number | null, action: 'approve' | 'reject' | 'delete' | 'delete_all', note: string }>({ open: false, id: null, action: 'approve', note: '' });
  const [formData, setFormData] = useState({ date: '', startTime: '', endTime: '', reason: '', type: 'WFH' });
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' | 'info' | 'warning' }>({ open: false, message: '', severity: 'info' });

  const showMessage = (message: string, severity: 'success' | 'error' | 'info' | 'warning' = 'info') => {
    setSnackbar({ open: true, message, severity });
  };

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      let data = [];
      if (isAdmin || isHR) {
        const res = await axios.get('http://localhost:5222/api/overtime', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        data = res.data;
      } else if (isManager) {
        const pendingRes = await axios.get('http://localhost:5222/api/overtime/manager-pending', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        const myRes = await axios.get('http://localhost:5222/api/overtime/me', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        const combined = [...pendingRes.data, ...myRes.data];
        const unique = new Map(combined.map(item => [item.id, item]));
        data = Array.from(unique.values());
      } else {
        const res = await axios.get('http://localhost:5222/api/overtime/me', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        data = res.data;
      }
      setOvertimes(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleConfirmAction = async () => {
    try {
      if (confirmDialog.action === 'delete_all') {
        await axios.delete(`http://localhost:5222/api/overtime/all`);
        setConfirmDialog({ open: false, id: null, action: 'approve', note: '' });
        fetchData();
        showMessage(`All overtime requests deleted successfully.`, 'success');
        return;
      }

      if (!confirmDialog.id) return;

      if (confirmDialog.action === 'delete') {
        await axios.delete(`http://localhost:5222/api/overtime/${confirmDialog.id}`, {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
      } else {
        await axios.put(`http://localhost:5222/api/overtime/${confirmDialog.id}/${confirmDialog.action}`, `"${confirmDialog.note}"`, {
          headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${user?.token}` }
        });
      }
      setConfirmDialog({ open: false, id: null, action: 'approve', note: '' });
      fetchData();
      showMessage(`Overtime request ${confirmDialog.action}d successfully.`, 'success');
    } catch (err: any) {
      console.error(err);
      let errMsg = `Failed to ${confirmDialog.action} overtime request.`;
      if (err.response?.data && typeof err.response.data === 'string') errMsg = err.response.data;
      showMessage(errMsg, "error");
    }
  };

  const handleSubmit = async () => {
    if (!formData.date || !formData.startTime || !formData.endTime || !formData.reason) {
      showMessage("Please fill all fields.", "warning");
      return;
    }

    try {
      const data = {
        date: formData.date,
        startTime: formData.startTime + ':00',
        endTime: formData.endTime + ':00',
        reason: formData.reason,
        type: formData.type
      };
      await axios.post('http://localhost:5222/api/overtime/me', data);
      setOpen(false);
      setFormData({ date: '', startTime: '', endTime: '', reason: '', type: 'WFH' });
      fetchData();
      showMessage('Overtime request submitted successfully.', 'success');
    } catch (err: any) {
      console.error(err);
      showMessage("Failed to submit overtime request.", "error");
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' }, gap: 2, mb: 4 }}>
        <Typography variant="h4" sx={{ fontWeight: 'normal', color: '#1E293B', fontSize: { xs: '1.75rem', sm: '2.125rem' } }}>
          {(isAdmin || isHR) ? 'All Overtime Requests' : isManager ? 'Overtime Requests (My Overtime & Team Pending)' : 'My Overtime'}
        </Typography>
        <Box sx={{ display: 'flex', gap: 2, flexDirection: { xs: 'column', sm: 'row' }, width: { xs: '100%', sm: 'auto' } }}>
          <Button variant="outlined" color="error" startIcon={<Trash2 size={18} />} onClick={() => setConfirmDialog({ open: true, id: -1, action: 'delete_all', note: '' })} sx={{ width: { xs: '100%', sm: 'auto' } }}>
            Delete All
          </Button>
          {!canApprove && (
            <Button variant="contained" startIcon={<Plus size={18} />} onClick={() => setOpen(true)} sx={{ width: { xs: '100%', sm: 'auto' } }}>
              Request Overtime
            </Button>
          )}
        </Box>
      </Box>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 5 }}>
          <CircularProgress />
        </Box>
      ) : overtimes.length === 0 ? (
        <Card sx={{ borderRadius: 4, boxShadow: '0 4px 20px rgba(0,0,0,0.05)', textAlign: 'center', py: 5 }}>
          <CardContent>
            <Typography variant="h6" color="text.secondary">No overtime requests found.</Typography>
          </CardContent>
        </Card>
      ) : (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          {overtimes.map((req) => (
            <Card key={req.id} sx={{ borderRadius: 3, boxShadow: '0 2px 10px rgba(0,0,0,0.05)', overflow: 'visible' }}>
              <CardContent sx={{ p: 3, display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 3 }}>
                  <Avatar sx={{ bgcolor: req.status === 'Approved' ? 'success.main' : req.status === 'Rejected' ? 'error.main' : req.status === 'PendingHRApproval' ? 'info.main' : 'warning.main', width: 48, height: 48 }}>
                    {req.status === 'Approved' ? <Check /> : req.status === 'Rejected' ? <X /> : <Clock />}
                  </Avatar>
                  <Box>
                    {(isAdmin || isHR || isManager) && req.employee && (
                      <Typography variant="subtitle1" sx={{ fontWeight: 'normal' }}>
                        {req.employee?.firstName} {req.employee?.lastName} ({req.employee?.employeeCode})
                      </Typography>
                    )}
                    <Typography variant="h6" sx={{ fontWeight: 400 }}>{req.date}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {formatTime12Hour(req.startTime)} - {formatTime12Hour(req.endTime)}
                    </Typography>
                    <Typography variant="body2" sx={{ mt: 1 }}>
                      <Chip size="small" label={req.type || 'WFH'} sx={{ mr: 1, height: 20, fontSize: '0.7rem' }} />
                      <span>Reason:</span> {req.reason}
                    </Typography>
                    {req.adminNote && (
                      <Typography variant="body2" color="primary" sx={{ mt: 0.5 }}>
                        <span>Admin Note:</span> {req.adminNote}
                      </Typography>
                    )}
                  </Box>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Chip
                    label={OVERTIME_STATUS_LABELS[req.status] ?? req.status}
                    color={getOvertimeStatusColor(req.status)}
                    sx={{ fontWeight: 'normal', fontSize: '0.7rem' }}
                  />
                  {((isManager && req.status === 'PendingManagerApproval') ||
                    ((isAdmin || isHR) && req.status === 'PendingHRApproval') ||
                    ((isAdmin || isHR) && req.status === 'PendingManagerApproval')) && (
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      {(isManager ? req.status === 'PendingManagerApproval' : req.status === 'PendingHRApproval') && (
                        <Button variant="outlined" color="success" size="small" onClick={() => setConfirmDialog({ open: true, id: req.id, action: 'approve', note: '' })}>
                          {isManager ? 'Approve (→HR)' : 'Final Approve'}
                        </Button>
                      )}
                      <Button variant="outlined" color="error" size="small" onClick={() => setConfirmDialog({ open: true, id: req.id, action: 'reject', note: '' })}>Reject</Button>
                    </Box>
                  )}
                  <Button color="error" onClick={() => setConfirmDialog({ open: true, id: req.id, action: 'delete', note: '' })} sx={{ minWidth: 'auto', p: 1 }}>
                    <Trash2 size={18} />
                  </Button>
                </Box>
              </CardContent>
            </Card>
          ))}
        </Box>
      )}

      {/* Request Dialog */}
      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth slotProps={{ paper: { sx: { borderRadius: 3 } } }}>
        <DialogTitle sx={{ fontWeight: 'normal', pb: 1 }}>Request Overtime</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, mt: 2 }}>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                select
                label="Overtime Type"
                fullWidth
                value={formData.type}
                onChange={(e) => setFormData({ ...formData, type: e.target.value })}
              >
                <MenuItem value="WFH">Work From Home (WFH)</MenuItem>
                <MenuItem value="OD">Office Day (OD)</MenuItem>
                <MenuItem value="OH">Official Holiday (OH)</MenuItem>
                <MenuItem value="Arv">Arrival Day (Arv)</MenuItem>
                <MenuItem value="FD">Factory Day (FD)</MenuItem>
                <MenuItem value="WE">Week End (WE)</MenuItem>
                <MenuItem value="WWE">Work Week End (WWE)</MenuItem>
                <MenuItem value="EF">Egypt Field Day (EF)</MenuItem>
              </TextField>
              <TextField
                label="Date"
                type="date"
                fullWidth
                slotProps={{ inputLabel: { shrink: true } }}
                value={formData.date}
                onChange={(e) => setFormData({ ...formData, date: e.target.value })}
              />
            </Box>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                label="Start Time"
                type="time"
                fullWidth
                slotProps={{ inputLabel: { shrink: true } }}
                value={formData.startTime}
                onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
              />
              <TextField
                label="End Time"
                type="time"
                fullWidth
                slotProps={{ inputLabel: { shrink: true } }}
                value={formData.endTime}
                onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
              />
            </Box>
            <TextField
              label="Reason / Tasks Completed"
              multiline
              rows={3}
              fullWidth
              value={formData.reason}
              onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpen(false)} color="inherit">Cancel</Button>
          <Button onClick={handleSubmit} variant="contained" color="primary" sx={{ borderRadius: 2 }}>Submit Request</Button>
        </DialogActions>
      </Dialog>

      {/* Admin Confirm Dialog */}
      <Dialog open={confirmDialog.open} onClose={() => setConfirmDialog({ ...confirmDialog, open: false })} maxWidth="sm" fullWidth slotProps={{ paper: { sx: { borderRadius: 3 } } }}>
        <DialogTitle sx={{ fontWeight: 'normal' }}>
          {confirmDialog.action === 'approve' ? 'Approve Request' : confirmDialog.action === 'reject' ? 'Reject Request' : confirmDialog.action === 'delete_all' ? 'Delete All Requests' : 'Delete Request'}
        </DialogTitle>
        <DialogContent>
          {confirmDialog.action === 'delete' || confirmDialog.action === 'delete_all' ? (
            <Typography>Are you sure you want to delete {confirmDialog.action === 'delete_all' ? 'all overtime requests' : 'this overtime request'}? This action cannot be undone.</Typography>
          ) : (
            <TextField
              autoFocus
              margin="dense"
              label="Admin Note (Optional)"
              type="text"
              fullWidth
              multiline
              rows={3}
              value={confirmDialog.note}
              onChange={(e) => setConfirmDialog({ ...confirmDialog, note: e.target.value })}
              placeholder="Add a note to the employee (e.g., approved with modifications, reason for rejection...)"
              sx={{ mt: 2 }}
            />
          )}
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={() => setConfirmDialog({ ...confirmDialog, open: false })} color="inherit">Cancel</Button>
          <Button 
            onClick={handleConfirmAction} 
            variant="contained" 
            color={confirmDialog.action === 'approve' ? 'success' : 'error'}
          >
            Confirm {confirmDialog.action === 'approve' ? 'Approval' : confirmDialog.action === 'reject' ? 'Rejection' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={snackbar.open} autoHideDuration={6000} onClose={() => setSnackbar({ ...snackbar, open: false })} anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>
        <Alert onClose={() => setSnackbar({ ...snackbar, open: false })} severity={snackbar.severity} sx={{ width: '100%', borderRadius: 2, boxShadow: '0 4px 12px rgba(0,0,0,0.1)' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
