import { useState, useEffect } from 'react';
import {
  Typography, Box, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, Button, CircularProgress, Alert, Chip, Dialog,
  DialogTitle, DialogContent, DialogActions, TextField, MenuItem,
  Snackbar
} from '@mui/material';
import { Plus, Check, X, Trash2, History } from 'lucide-react';
import axios from 'axios';
import { useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

// ─── Types ────────────────────────────────────────────────────────────────────

interface LeaveRequest {
  id: number;
  employeeId: number;
  employeeName: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  reason: string;
  status: string;
  statusLabel: string;
  adminMessage?: string;
  createdAt: string;
}

interface LeaveBalance {
  employeeId: number;
  year: number;
  annualLeaveTotal: number;
  annualLeaveUsed: number;
  annualLeaveRemaining: number;
  emergencyLeaveTotal: number;
  emergencyLeaveUsed: number;
  emergencyLeaveRemaining: number;
}

interface ApprovalHistoryEntry {
  id: number;
  approverName: string;
  approverRole: string;
  decision: string;
  comment?: string;
  decisionAt: string;
}

// ─── Leave type options with codes ────────────────────────────────────────────

const LEAVE_TYPES = [
  { value: 1, label: 'Annual Leave (RDO)' },
  { value: 2, label: 'Emergency Leave (EDO)' },
  { value: 3, label: 'Reported Sick Day (RSD)' },
  { value: 4, label: 'Absence Without Permission (AWD)' },
  { value: 5, label: 'Office Day (OD)' },
  { value: 6, label: 'Official Holiday (OH)' },
  { value: 7, label: 'Arrival Day (Arv)' },
  { value: 8, label: 'Factory Day (FD)' },
  { value: 9, label: 'Week End (WE)' },
  { value: 10, label: 'Work Week End (WWE)' },
  { value: 11, label: 'Egypt Field Day (EF)' },
];

// ─── Status helpers ───────────────────────────────────────────────────────────

const getStatusColor = (status: string): 'success' | 'error' | 'warning' | 'info' | 'default' => {
  switch (status) {
    case 'Approved':               return 'success';
    case 'Rejected':               return 'error';
    case 'PendingHRApproval':      return 'info';
    case 'PendingManagerApproval': return 'warning';
    default:                       return 'warning';
  }
};

const getStatusLabel = (statusLabel: string, status: string): string => {
  if (statusLabel) return statusLabel;
  switch (status) {
    case 'PendingManagerApproval': return 'Pending Manager Approval';
    case 'PendingHRApproval':      return 'Pending HR Approval';
    case 'Approved':               return 'Approved';
    case 'Rejected':               return 'Rejected';
    default:                       return status;
  }
};

// ─── Main Component ───────────────────────────────────────────────────────────

export default function Leaves() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';
  const isHR = user?.role === 'HR';
  const isManager = user?.role === 'Manager';
  const canApprove = isAdmin || isManager || isHR;

  const [leaves, setLeaves] = useState<LeaveRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [toast, setToast] = useState<{ open: boolean; message: string; severity: 'success' | 'error' | 'info' }>({ open: false, message: '', severity: 'success' });

  const showToast = (message: string, severity: 'success' | 'error' | 'info' = 'success') =>
    setToast({ open: true, message, severity });

  // Leave balance (employee view)
  const [balance, setBalance] = useState<LeaveBalance | null>(null);

  // Delete dialog
  const [deleteDialog, setDeleteDialog] = useState<{ open: boolean; id: number | null; action: 'delete' | 'deleteAll' }>({ open: false, id: null, action: 'delete' });

  // Approve/reject dialog
  const [actionOpen, setActionOpen] = useState(false);
  const [actionType, setActionType] = useState<'approve' | 'reject'>('approve');
  const [actionLeaveId, setActionLeaveId] = useState<number | null>(null);
  const [actionMessage, setActionMessage] = useState('');

  // History dialog
  const [historyOpen, setHistoryOpen] = useState(false);
  const [historyData, setHistoryData] = useState<ApprovalHistoryEntry[]>([]);
  const [historyLoading, setHistoryLoading] = useState(false);

  // New leave request dialog
  const location = useLocation();
  const [open, setOpen] = useState(false);
  const [newLeave, setNewLeave] = useState<{
    employeeId: number;
    type: number;
    startDate: string;
    endDate: string;
    reason: string;
    linkedAttendanceId?: number | null;
  }>({
    employeeId: user?.employeeId || 1,
    type: 1,
    startDate: new Date().toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0],
    reason: '',
    linkedAttendanceId: null
  });

  useEffect(() => {
    const query = new URLSearchParams(location.search);
    const linkedAttendanceId = query.get('linkedAttendanceId');
    const dateStr = query.get('date');

    if (linkedAttendanceId && dateStr) {
      const formattedDate = new Date(dateStr).toISOString().split('T')[0];
      setNewLeave(prev => ({
        ...prev,
        startDate: formattedDate,
        endDate: formattedDate,
        linkedAttendanceId: parseInt(linkedAttendanceId)
      }));
      setOpen(true);
    }
  }, [location.search]);

  // ─── Data Fetching ──────────────────────────────────────────────────────────

  const fetchLeaves = async () => {
    setLoading(true);
    try {
      let response;
      let data = [];
      if (isAdmin || isHR) {
        response = await axios.get('http://localhost:5222/api/Leaves/all', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        data = response.data;
      } else if (isManager) {
        const pendingRes = await axios.get('http://localhost:5222/api/Leaves/manager-pending', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        const myRes = await axios.get('http://localhost:5222/api/Leaves/me', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        const combined = [...pendingRes.data, ...myRes.data];
        // Deduplicate
        const unique = new Map(combined.map(item => [item.id, item]));
        data = Array.from(unique.values());
      } else {
        response = await axios.get('http://localhost:5222/api/Leaves/me', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        data = response.data;
      }
      setLeaves(data);
    } catch (err: any) {
      setError('Failed to load leaves.');
    } finally {
      setLoading(false);
    }
  };

  const fetchBalance = async () => {
    if (!user?.employeeId) return;
    try {
      const res = await axios.get(`http://localhost:5222/api/Leaves/balance/me`, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setBalance(res.data);
    } catch {
      // Balance fetch is non-critical
    }
  };

  useEffect(() => {
    fetchLeaves();
    if (!isAdmin) fetchBalance();
  }, []);

  // ─── Actions ────────────────────────────────────────────────────────────────

  const handleOpenAction = (id: number, type: 'approve' | 'reject') => {
    setActionLeaveId(id);
    setActionType(type);
    setActionMessage('');
    setActionOpen(true);
  };

  const handleConfirmAction = async () => {
    if (!actionLeaveId) return;
    try {
      await axios.post(`http://localhost:5222/api/Leaves/${actionLeaveId}/${actionType}`, {
        approverId: user?.employeeId ?? 1,
        adminMessage: actionMessage
      }, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setActionOpen(false);
      showToast(`Leave request ${actionType === 'approve' ? 'approved' : 'rejected'} successfully.`);
      fetchLeaves();
      if (!isAdmin) fetchBalance();
    } catch (err: any) {
      const msg = err.response?.data?.error || `Failed to ${actionType} request.`;
      showToast(msg, 'error');
    }
  };

  const handleAdd = async () => {
    try {
      await axios.post('http://localhost:5222/api/Leaves', newLeave, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setOpen(false);
      showToast('Leave request submitted successfully.');
      fetchLeaves();
      fetchBalance();
    } catch (err: any) {
      showToast(err.response?.data?.error || 'Failed to submit leave request.', 'error');
    }
  };

  const handleDeleteLeave = (id: number) => setDeleteDialog({ open: true, id, action: 'delete' });
  const handleDeleteAllLeaves = () => setDeleteDialog({ open: true, id: null, action: 'deleteAll' });

  const executeDelete = async () => {
    try {
      if (deleteDialog.action === 'deleteAll') {
        await axios.delete('http://localhost:5222/api/Leaves/all', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
      } else if (deleteDialog.id) {
        await axios.delete(`http://localhost:5222/api/Leaves/${deleteDialog.id}`, {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
      }
      showToast('Deleted successfully.');
      fetchLeaves();
    } catch {
      showToast('Failed to delete leave request(s).', 'error');
    } finally {
      setDeleteDialog({ open: false, id: null, action: 'delete' });
    }
  };

  const handleViewHistory = async (id: number) => {
    setHistoryOpen(true);
    setHistoryLoading(true);
    try {
      const res = await axios.get(`http://localhost:5222/api/Leaves/${id}/history`);
      setHistoryData(res.data);
    } catch {
      setHistoryData([]);
    } finally {
      setHistoryLoading(false);
    }
  };

  // ─── Role-based action visibility ────────────────────────────────────────────

  const canManagerApprove = (status: string) =>
    isManager && status === 'PendingManagerApproval';

  const canHRApprove = (status: string) =>
    (isAdmin || isHR) && status === 'PendingHRApproval';

  const canAnyApprove = (status: string) =>
    canManagerApprove(status) || canHRApprove(status);

  const canReject = (status: string) => {
    if (isManager) return status === 'PendingManagerApproval';
    if (isAdmin || isHR) return status === 'PendingManagerApproval' || status === 'PendingHRApproval';
    return false;
  };

  // ─── Render ─────────────────────────────────────────────────────────────────

  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' }, gap: 2, mb: 3 }}>
        <Typography variant="h4" sx={{ m: 0, fontWeight: 'normal', fontSize: { xs: '1.75rem', sm: '2.125rem' } }}>
          Leaves & Permissions
        </Typography>
        <Box sx={{ display: 'flex', gap: 2, width: { xs: '100%', sm: 'auto' } }}>
          <Button variant="outlined" color="error" startIcon={<Trash2 size={18} />} onClick={handleDeleteAllLeaves} sx={{ flex: { xs: 1, sm: 'none' } }}>
            Delete All
          </Button>
          {!canApprove && (
            <Button variant="contained" startIcon={<Plus size={18} />} onClick={() => setOpen(true)} sx={{ flex: { xs: 1, sm: 'none' } }}>
              Request Leave
            </Button>
          )}
        </Box>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

      {/* Leave Balance Cards (Employee view) */}
      {!isAdmin && balance && (
        <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
          <Paper elevation={2} sx={{ p: 2.5, borderRadius: 3, flex: '1 1 200px', borderLeft: '4px solid #2563EB' }}>
            <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 400, textTransform: 'uppercase', letterSpacing: 0.5 }}>
              Annual Leave (RDO)
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 0.5, mt: 0.5 }}>
              <Typography variant="h4" sx={{ fontWeight: 400, color: '#2563EB' }}>
                {balance.annualLeaveRemaining}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                / {balance.annualLeaveTotal} days remaining
              </Typography>
            </Box>
            <Box sx={{ mt: 1, height: 6, bgcolor: '#EFF6FF', borderRadius: 3 }}>
              <Box sx={{ height: '100%', bgcolor: '#2563EB', borderRadius: 3, width: `${(balance.annualLeaveRemaining / balance.annualLeaveTotal) * 100}%` }} />
            </Box>
          </Paper>
          <Paper elevation={2} sx={{ p: 2.5, borderRadius: 3, flex: '1 1 200px', borderLeft: '4px solid #D97706' }}>
            <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 400, textTransform: 'uppercase', letterSpacing: 0.5 }}>
              Emergency Leave (EDO)
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 0.5, mt: 0.5 }}>
              <Typography variant="h4" sx={{ fontWeight: 400, color: '#D97706' }}>
                {balance.emergencyLeaveRemaining}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                / {balance.emergencyLeaveTotal} days remaining
              </Typography>
            </Box>
            <Box sx={{ mt: 1, height: 6, bgcolor: '#FFF7ED', borderRadius: 3 }}>
              <Box sx={{ height: '100%', bgcolor: '#D97706', borderRadius: 3, width: `${(balance.emergencyLeaveRemaining / balance.emergencyLeaveTotal) * 100}%` }} />
            </Box>
          </Paper>
        </Box>
      )}

      {/* New Leave Request Dialog */}
      <Dialog open={open} onClose={() => setOpen(false)}>
        <DialogTitle>Submit Leave Request</DialogTitle>
        <DialogContent>
          <TextField
            select margin="dense" label="Leave Type" fullWidth variant="outlined"
            value={newLeave.type}
            onChange={e => setNewLeave({ ...newLeave, type: Number(e.target.value) })}
            sx={{ mb: 2, mt: 1 }}
          >
            {LEAVE_TYPES.map(lt => (
              <MenuItem key={lt.value} value={lt.value}>{lt.label}</MenuItem>
            ))}
          </TextField>
          <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
            <TextField
              label="Start Date" type="date" fullWidth variant="outlined"
              value={newLeave.startDate}
              onChange={e => setNewLeave({ ...newLeave, startDate: e.target.value })}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              label="End Date" type="date" fullWidth variant="outlined"
              value={newLeave.endDate}
              onChange={e => setNewLeave({ ...newLeave, endDate: e.target.value })}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Box>
          <TextField
            margin="dense" label="Reason" multiline rows={3} fullWidth variant="outlined"
            value={newLeave.reason}
            onChange={e => setNewLeave({ ...newLeave, reason: e.target.value })}
            sx={{ mb: 2 }}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpen(false)}>Cancel</Button>
          <Button onClick={handleAdd} variant="contained">Submit</Button>
        </DialogActions>
      </Dialog>

      <Typography variant="h6" sx={{ mb: 2 }}>
        {(isAdmin || isHR) ? 'All Leave Requests' : isManager ? 'Leave Requests (My Leaves & Team Pending)' : 'My Leave Requests'}
      </Typography>

      {/* Leaves Table */}
      <TableContainer component={Paper} elevation={2}>
        <Table sx={{ minWidth: 650 }}>
          <TableHead sx={{ bgcolor: 'rgba(0,0,0,0.02)' }}>
            <TableRow>
              <TableCell sx={{ fontWeight: 'normal' }}>Employee</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Type</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Duration</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Reason</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Note</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Status</TableCell>
              <TableCell align="right" sx={{ fontWeight: 'normal' }}>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 5 }}><CircularProgress /></TableCell>
              </TableRow>
            ) : leaves.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 5 }}>
                  <Typography color="text.secondary">No leave requests found.</Typography>
                </TableCell>
              </TableRow>
            ) : (
              leaves.map((row) => (
                <TableRow key={row.id}>
                  <TableCell>{row.employeeName}</TableCell>
                  <TableCell>{row.leaveType}</TableCell>
                  <TableCell>{row.startDate} to {row.endDate}</TableCell>
                  <TableCell>{row.reason}</TableCell>
                  <TableCell>{row.adminMessage || '–'}</TableCell>
                  <TableCell>
                    <Chip
                      label={getStatusLabel(row.statusLabel, row.status)}
                      color={getStatusColor(row.status)}
                      size="small"
                      sx={{ fontSize: '0.7rem' }}
                    />
                  </TableCell>
                  <TableCell align="right" sx={{ minWidth: 180 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 0.5, flexWrap: 'wrap' }}>
                      {canAnyApprove(row.status) && (
                        <Button color="success" size="small" title="Approve" onClick={() => handleOpenAction(row.id, 'approve')} sx={{ minWidth: 'auto', p: 1 }}>
                          <Check size={18} />
                        </Button>
                      )}
                      {canReject(row.status) && (
                        <Button color="error" size="small" title="Reject" onClick={() => handleOpenAction(row.id, 'reject')} sx={{ minWidth: 'auto', p: 1 }}>
                          <X size={18} />
                        </Button>
                      )}
                      {canApprove && (
                        <Button color="primary" size="small" title="View History" onClick={() => handleViewHistory(row.id)} sx={{ minWidth: 'auto', p: 1 }}>
                          <History size={18} />
                        </Button>
                      )}
                      <Button color="error" size="small" title="Delete" onClick={() => handleDeleteLeave(row.id)} sx={{ minWidth: 'auto', p: 1 }}>
                        <Trash2 size={18} />
                      </Button>
                    </Box>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Approve / Reject dialog */}
      <Dialog open={actionOpen} onClose={() => setActionOpen(false)}>
        <DialogTitle>{actionType === 'approve' ? 'Approve Leave' : 'Reject Leave'}</DialogTitle>
        <DialogContent>
          <Typography sx={{ mb: 2 }}>
            {actionType === 'approve'
              ? isManager
                ? 'Approving will advance this request to HR for final decision.'
                : 'As HR, this is the final approval. Leave balance will be deducted for RDO/EDO requests.'
              : 'You can provide an optional reason that will be sent to the employee.'}
          </Typography>
          <TextField
            autoFocus margin="dense" label={actionType === 'approve' ? 'Note (optional)' : 'Reason for rejection (optional)'}
            fullWidth multiline rows={3} variant="outlined"
            value={actionMessage}
            onChange={(e) => setActionMessage(e.target.value)}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setActionOpen(false)}>Cancel</Button>
          <Button onClick={handleConfirmAction} variant="contained" color={actionType === 'approve' ? 'success' : 'error'}>
            Confirm {actionType === 'approve' ? 'Approval' : 'Rejection'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Approval History Dialog */}
      <Dialog open={historyOpen} onClose={() => setHistoryOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 'normal' }}>Approval History</DialogTitle>
        <DialogContent>
          {historyLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}><CircularProgress /></Box>
          ) : historyData.length === 0 ? (
            <Typography color="text.secondary" sx={{ py: 2 }}>No approval actions recorded yet.</Typography>
          ) : (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {historyData.map((h) => (
                <Paper key={h.id} elevation={1} sx={{ p: 2, borderRadius: 2, borderLeft: `4px solid ${h.decision.includes('Rejected') ? '#EF4444' : '#22C55E'}` }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                    <Typography variant="body2" sx={{ fontWeight: 400 }}>{h.approverName} ({h.approverRole})</Typography>
                    <Chip label={h.decision} size="small" color={h.decision.includes('Rejected') ? 'error' : 'success'} />
                  </Box>
                  <Typography variant="caption" color="text.secondary">
                    {new Date(h.decisionAt).toLocaleString()}
                  </Typography>
                  {h.comment && (
                    <Typography variant="body2" sx={{ mt: 1, fontStyle: 'italic' }}>"{h.comment}"</Typography>
                  )}
                </Paper>
              ))}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setHistoryOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirm Dialog */}
      <Dialog open={deleteDialog.open} onClose={() => setDeleteDialog({ ...deleteDialog, open: false })} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 'normal', color: 'error.main' }}>Confirm Deletion</DialogTitle>
        <DialogContent>
          <Typography variant="body1" sx={{ mt: 1 }}>
            {deleteDialog.action === 'deleteAll'
              ? 'Are you sure you want to delete ALL your leave requests? This action cannot be undone.'
              : 'Are you sure you want to delete this leave request?'}
          </Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setDeleteDialog({ ...deleteDialog, open: false })} color="inherit">Cancel</Button>
          <Button onClick={executeDelete} variant="contained" color="error">Delete</Button>
        </DialogActions>
      </Dialog>

      {/* Toast */}
      <Snackbar
        open={toast.open}
        autoHideDuration={5000}
        onClose={() => setToast({ ...toast, open: false })}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert onClose={() => setToast({ ...toast, open: false })} severity={toast.severity} sx={{ width: '100%', borderRadius: 2 }}>
          {toast.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
