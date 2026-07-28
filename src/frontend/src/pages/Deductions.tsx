import { useState, useEffect } from 'react';
import { Typography, Box, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Chip, CircularProgress, Alert, Button, Dialog, DialogTitle, DialogContent, DialogActions, Snackbar } from '@mui/material';
import axios from 'axios';
import { useAuth } from '../contexts/AuthContext';
import { AlertCircle, CheckCircle2, XCircle, Trash2, Edit2 } from 'lucide-react';
import { IconButton, TextField } from '@mui/material';

interface Deduction {
  id: number;
  employeeId?: number;
  employeeName?: string;
  relatedAttendanceId: number;
  attendanceDate: string;
  deductionAmount: number;
  reason: string;
  appliedOnDate: string;
  status: string;
}

export default function Deductions() {
  const { user } = useAuth();
  const isAdminOrHR = user?.role === 'Admin' || user?.role === 'HR';
  const [deductions, setDeductions] = useState<Deduction[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [waiveDialog, setWaiveDialog] = useState<{ open: boolean; id: number | null; reason: string }>({ open: false, id: null, reason: '' });
  const [rejectDialog, setRejectDialog] = useState<{ open: boolean; id: number | null; reason: string }>({ open: false, id: null, reason: '' });
  const [deleteDialog, setDeleteDialog] = useState<{ open: boolean; id: number | null }>({ open: false, id: null });
  const [deleteAllDialog, setDeleteAllDialog] = useState(false);
  const [editDialog, setEditDialog] = useState<{ open: boolean; id: number | null; amount: string }>({ open: false, id: null, amount: '' });
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const showMessage = (message: string, severity: 'success' | 'error') => {
    setSnackbar({ open: true, message, severity });
  };

  const handleCloseSnackbar = () => {
    setSnackbar({ ...snackbar, open: false });
  };

  const fetchDeductions = async () => {
    setLoading(true);
    try {
      const endpoint = isAdminOrHR 
        ? 'http://localhost:5222/api/Deductions' 
        : 'http://localhost:5222/api/Deductions/my-deductions';
        
      const res = await axios.get(endpoint, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setDeductions(res.data);
    } catch (err) {
      setError('Failed to load deductions.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (user) fetchDeductions();
  }, [user]);

  const handleWaive = async () => {
    if (!waiveDialog.id) return;
    try {
      await axios.post(`http://localhost:5222/api/Deductions/${waiveDialog.id}/waive`, 
        { reason: waiveDialog.reason }, 
        { headers: { Authorization: `Bearer ${user?.token}` } }
      );
      setWaiveDialog({ open: false, id: null, reason: '' });
      showMessage('Exception approved successfully.', 'success');
      fetchDeductions();
    } catch (err: any) {
      showMessage(err.response?.data?.message || err.response?.data || 'Failed to approve exception.', 'error');
    }
  };

  const handleReject = async () => {
    if (!rejectDialog.id) return;
    try {
      await axios.post(`http://localhost:5222/api/Deductions/${rejectDialog.id}/reject`, 
        { rejectionReason: rejectDialog.reason },
        { headers: { Authorization: `Bearer ${user?.token}` } }
      );
      setRejectDialog({ open: false, id: null, reason: '' });
      showMessage('Exception rejected successfully.', 'success');
      fetchDeductions();
    } catch (err: any) {
      showMessage(err.response?.data?.message || err.response?.data || 'Failed to reject exception.', 'error');
    }
  };

  const handleDelete = async () => {
    if (!deleteDialog.id) return;
    try {
      await axios.delete(`http://localhost:5222/api/Deductions/${deleteDialog.id}`, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setDeleteDialog({ open: false, id: null });
      showMessage('Deduction deleted successfully.', 'success');
      fetchDeductions();
    } catch (err: any) {
      showMessage(err.response?.data?.message || err.response?.data || 'Failed to delete deduction.', 'error');
    }
  };

  const handleEdit = async () => {
    if (!editDialog.id || !editDialog.amount) return;
    try {
      await axios.put(`http://localhost:5222/api/Deductions/${editDialog.id}`, 
        { deductionAmount: parseFloat(editDialog.amount) },
        { headers: { Authorization: `Bearer ${user?.token}` } }
      );
      setEditDialog({ open: false, id: null, amount: '' });
      showMessage('Deduction amount updated successfully.', 'success');
      fetchDeductions();
    } catch (err: any) {
      showMessage(err.response?.data?.message || err.response?.data || 'Failed to update deduction.', 'error');
    }
  };

  const handleDeleteAll = async () => {
    try {
      await axios.delete('http://localhost:5222/api/Deductions/all', {
        headers: { Authorization: `Bearer ${user?.token}` }
      });
      setDeleteAllDialog(false);
      showMessage('All deductions deleted successfully.', 'success');
      fetchDeductions();
    } catch (err: any) {
      showMessage(err.response?.data?.message || err.response?.data || 'Failed to delete all deductions.', 'error');
    }
  };

  const getStatusChip = (status: string) => {
    switch (status) {
      case 'PendingPayroll':
        return <Chip size="small" label="Pending Payroll" color="warning" icon={<AlertCircle size={16} />} sx={{ borderRadius: 1 }} />;
      case 'Processed':
        return <Chip size="small" label="Processed" color="success" icon={<CheckCircle2 size={16} />} sx={{ borderRadius: 1 }} />;
      case 'Waived':
        return <Chip size="small" label="Waived" color="default" icon={<XCircle size={16} />} sx={{ borderRadius: 1 }} />;
      case 'Rejected':
        return <Chip size="small" label="Rejected" color="error" icon={<XCircle size={16} />} sx={{ borderRadius: 1 }} />;
      default:
        return <Chip size="small" label={status} />;
    }
  };

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', mt: 10 }}><CircularProgress /></Box>;

  return (
    <Box sx={{ maxWidth: '1400px', margin: '0 auto', pb: 8 }}>
      <Box sx={{ mb: 5, display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'center' }, gap: 2 }}>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 400, color: '#0F172A', mb: 1, fontSize: { xs: '2rem', sm: '2.5rem' } }}>
            Salary Deductions
          </Typography>
          <Typography variant="subtitle1" sx={{ color: '#64748B' }}>
            {isAdminOrHR ? 'Manage employee salary deductions and AWOL penalties' : 'View your applied deductions'}
          </Typography>
        </Box>
        {isAdminOrHR && deductions.length > 0 && (
          <Button 
            variant="contained" 
            color="error" 
            startIcon={<Trash2 size={18} />}
            onClick={() => setDeleteAllDialog(true)}
            sx={{ borderRadius: 2, textTransform: 'none', boxShadow: 'none', width: { xs: '100%', sm: 'auto' } }}
          >
            Delete All
          </Button>
        )}
      </Box>

      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

      <TableContainer component={Paper} elevation={0} sx={{ borderRadius: '20px', border: '1px solid #E2E8F0', overflowX: 'auto' }}>
        <Table sx={{ minWidth: 650 }}>
          <TableHead sx={{ bgcolor: '#F8FAFC' }}>
            <TableRow>
              {isAdminOrHR && <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Employee</TableCell>}
              <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Absence Date</TableCell>
              <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Amount</TableCell>
              <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Reason</TableCell>
              <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Status</TableCell>
              {isAdminOrHR && <TableCell align="right" sx={{ fontWeight: 600, color: '#475569' }}>Actions</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {deductions.length === 0 ? (
              <TableRow>
                <TableCell colSpan={isAdminOrHR ? 6 : 4} align="center" sx={{ py: 5 }}>
                  <Typography color="text.secondary">No deductions found.</Typography>
                </TableCell>
              </TableRow>
            ) : (
              deductions.map((row) => (
                <TableRow key={row.id} sx={{ '&:last-child td, &:last-child th': { border: 0 } }}>
                  {isAdminOrHR && (
                    <TableCell>
                      <Typography variant="subtitle2">{row.employeeName}</Typography>
                    </TableCell>
                  )}
                  <TableCell>{new Date(row.attendanceDate).toLocaleDateString()}</TableCell>
                  <TableCell>
                    <Typography variant="subtitle2" color="error.main">
                      {row.deductionAmount} Day(s)
                    </Typography>
                  </TableCell>
                  <TableCell sx={{ minWidth: 200, whiteSpace: 'normal', wordBreak: 'break-word' }}>
                    {row.reason}
                  </TableCell>
                  <TableCell>{getStatusChip(row.status)}</TableCell>
                  {isAdminOrHR && (
                    <TableCell align="right">
                      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, flexWrap: { xs: 'nowrap', sm: 'wrap' } }}>
                        {row.status === 'PendingPayroll' && (
                          <>
                            <Button 
                              size="medium" 
                              variant="outlined" 
                              color="success" 
                              sx={{ textTransform: 'none', borderRadius: 2, p: { xs: '6px 12px', sm: '4px 10px' }, minWidth: { xs: '80px', sm: 'auto' } }}
                              onClick={() => setWaiveDialog({ open: true, id: row.id, reason: '' })}
                            >
                              Approve
                            </Button>
                            <Button 
                              size="medium" 
                              variant="outlined" 
                              color="error" 
                              sx={{ textTransform: 'none', borderRadius: 2, p: { xs: '6px 12px', sm: '4px 10px' }, minWidth: { xs: '80px', sm: 'auto' } }}
                              onClick={() => setRejectDialog({ open: true, id: row.id, reason: '' })}
                            >
                              Reject
                            </Button>
                          </>
                        )}
                        {row.status === 'PendingPayroll' && (
                          <IconButton size="medium" color="primary" onClick={() => setEditDialog({ open: true, id: row.id, amount: row.deductionAmount.toString() })} sx={{ p: { xs: 1, sm: 1 } }}>
                            <Edit2 size={20} />
                          </IconButton>
                        )}
                        <IconButton size="medium" color="error" onClick={() => setDeleteDialog({ open: true, id: row.id })} sx={{ p: { xs: 1, sm: 1 } }}>
                          <Trash2 size={20} />
                        </IconButton>
                      </Box>
                    </TableCell>
                  )}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Waive Dialog */}
      <Dialog open={waiveDialog.open} onClose={() => setWaiveDialog({ open: false, id: null, reason: '' })} sx={{ '& .MuiDialog-paper': { borderRadius: 3, width: '400px' } }}>
        <DialogTitle sx={{ fontWeight: 600 }}>Approve Exception</DialogTitle>
        <DialogContent>
          <Typography sx={{ mb: 2 }}>Are you sure you want to approve this exception and waive the salary deduction? This action cannot be easily reversed.</Typography>
          <TextField
            fullWidth
            label="Approval Reason/Comment (Optional)"
            multiline
            rows={3}
            value={waiveDialog.reason}
            onChange={(e) => setWaiveDialog({ ...waiveDialog, reason: e.target.value })}
            variant="outlined"
          />
        </DialogContent>
        <DialogActions sx={{ p: 2, pt: 0 }}>
          <Button onClick={() => setWaiveDialog({ open: false, id: null, reason: '' })} color="inherit" sx={{ textTransform: 'none', minHeight: '44px' }}>Cancel</Button>
          <Button onClick={handleWaive} variant="contained" color="primary" sx={{ textTransform: 'none', borderRadius: 2, boxShadow: 'none', minHeight: '44px' }}>
            Approve Exception
          </Button>
        </DialogActions>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog open={rejectDialog.open} onClose={() => setRejectDialog({ open: false, id: null, reason: '' })} sx={{ '& .MuiDialog-paper': { borderRadius: 3, width: '400px' } }}>
        <DialogTitle sx={{ fontWeight: 600, color: 'error.main' }}>Reject Exception</DialogTitle>
        <DialogContent>
          <Typography sx={{ mb: 2 }}>Are you sure you want to reject this exception? The salary deduction will be enforced.</Typography>
          <TextField
            fullWidth
            label="Rejection Reason (Optional)"
            multiline
            rows={3}
            value={rejectDialog.reason}
            onChange={(e) => setRejectDialog({ ...rejectDialog, reason: e.target.value })}
            variant="outlined"
          />
        </DialogContent>
        <DialogActions sx={{ p: 2, pt: 0 }}>
          <Button onClick={() => setRejectDialog({ open: false, id: null, reason: '' })} color="inherit" sx={{ textTransform: 'none', minHeight: '44px' }}>Cancel</Button>
          <Button onClick={handleReject} variant="contained" color="error" sx={{ textTransform: 'none', borderRadius: 2, boxShadow: 'none', minHeight: '44px' }}>
            Reject Exception
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={deleteDialog.open} onClose={() => setDeleteDialog({ open: false, id: null })} sx={{ '& .MuiDialog-paper': { borderRadius: 3 } }}>
        <DialogTitle sx={{ fontWeight: 600, color: 'error.main' }}>Delete Deduction</DialogTitle>
        <DialogContent>
          <Typography>Are you sure you want to permanently delete this salary deduction? This action cannot be undone.</Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2, pt: 0 }}>
          <Button onClick={() => setDeleteDialog({ open: false, id: null })} color="inherit" sx={{ textTransform: 'none' }}>Cancel</Button>
          <Button onClick={handleDelete} variant="contained" color="error" sx={{ textTransform: 'none', borderRadius: 2, boxShadow: 'none' }}>
            Delete Deduction
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete All Dialog */}
      <Dialog open={deleteAllDialog} onClose={() => setDeleteAllDialog(false)} sx={{ '& .MuiDialog-paper': { borderRadius: 3 } }}>
        <DialogTitle sx={{ fontWeight: 600, color: 'error.main' }}>Delete All Deductions</DialogTitle>
        <DialogContent>
          <Typography>Are you sure you want to permanently delete ALL salary deductions? This action cannot be undone.</Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2, pt: 0 }}>
          <Button onClick={() => setDeleteAllDialog(false)} color="inherit" sx={{ textTransform: 'none' }}>Cancel</Button>
          <Button onClick={handleDeleteAll} variant="contained" color="error" sx={{ textTransform: 'none', borderRadius: 2, boxShadow: 'none' }}>
            Delete All
          </Button>
        </DialogActions>
      </Dialog>

      {/* Edit Dialog */}
      <Dialog open={editDialog.open} onClose={() => setEditDialog({ open: false, id: null, amount: '' })} sx={{ '& .MuiDialog-paper': { borderRadius: 3, width: '400px' } }}>
        <DialogTitle sx={{ fontWeight: 600 }}>Edit Deduction Amount</DialogTitle>
        <DialogContent>
          <Typography sx={{ mb: 2 }}>Enter the new deduction amount (Days):</Typography>
          <TextField
            fullWidth
            type="number"
            value={editDialog.amount}
            onChange={(e) => setEditDialog({ ...editDialog, amount: e.target.value })}
            variant="outlined"
            size="small"
            slotProps={{ htmlInput: { min: 0, step: "0.5" } }}
          />
        </DialogContent>
        <DialogActions sx={{ p: 2, pt: 0 }}>
          <Button onClick={() => setEditDialog({ open: false, id: null, amount: '' })} color="inherit" sx={{ textTransform: 'none' }}>Cancel</Button>
          <Button onClick={handleEdit} variant="contained" color="primary" sx={{ textTransform: 'none', borderRadius: 2, boxShadow: 'none' }}>
            Save Changes
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar 
        open={snackbar.open} 
        autoHideDuration={2000} 
        onClose={handleCloseSnackbar} 
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert onClose={handleCloseSnackbar} severity={snackbar.severity} sx={{ width: '100%', borderRadius: 2 }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
