import { useState, useEffect } from 'react';
import {
  Box, Typography, Card, CardContent, Button, Chip, TextField, Dialog,
  DialogTitle, DialogContent, DialogActions, CircularProgress, IconButton,
  Snackbar, Alert, Autocomplete, FormControlLabel, Switch
} from '@mui/material';
import { Trash2, Plus } from 'lucide-react';
import axios from 'axios';

interface Employee {
  id: number;
  firstName: string;
  lastName: string;
  email?: string;
}

export default function Announcements() {
  const [announcements, setAnnouncements] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [open, setOpen] = useState(false);
  const [formData, setFormData] = useState({ title: '', content: '', type: 'Notice' });
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [announcementToDelete, setAnnouncementToDelete] = useState<number | null>(null);
  const [toast, setToast] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  // Feature 1: Specific recipient
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [sendToAll, setSendToAll] = useState(true);
  const [selectedEmployee, setSelectedEmployee] = useState<Employee | null>(null);
  const [sending, setSending] = useState(false);

  const handleCloseToast = () => setToast({ ...toast, open: false });

  useEffect(() => {
    fetchAnnouncements();
    fetchEmployees();
  }, []);

  const fetchAnnouncements = async () => {
    try {
      const res = await axios.get('https://andrew20x-001-site1.itempurl.com/api/announcements');
      setAnnouncements(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchEmployees = async () => {
    try {
      const res = await axios.get('https://andrew20x-001-site1.itempurl.com/api/employees');
      setEmployees(res.data.filter((e: any) => e.isActive && e.email && e.email !== '(No Email)'));
    } catch (err) {
      console.error('Failed to load employees for recipient selector');
    }
  };

  const handleCreate = async () => {
    if (!formData.title || !formData.content) {
      setToast({ open: true, message: 'Please provide both a Title and Content.', severity: 'error' });
      return;
    }

    if (!sendToAll && !selectedEmployee) {
      setToast({ open: true, message: 'Please select a recipient employee, or enable "Send to all".', severity: 'error' });
      return;
    }

    setSending(true);
    try {
      const body = {
        ...formData,
        targetEmployeeId: sendToAll ? undefined : selectedEmployee?.id
      };

      await axios.post('https://andrew20x-001-site1.itempurl.com/api/Announcements', body);
      setOpen(false);
      setFormData({ title: '', content: '', type: 'Notice' });
      setSelectedEmployee(null);
      setSendToAll(true);

      const recipientMsg = sendToAll
        ? 'Announcement posted and emailed to all employees.'
        : `Announcement posted and emailed to ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}.`;
      setToast({ open: true, message: recipientMsg, severity: 'success' });
      fetchAnnouncements();
    } catch (err: any) {
      const errMsg = err.response?.data?.error || err.response?.data?.title || 'Failed to post announcement.';
      setToast({ open: true, message: errMsg, severity: 'error' });
    } finally {
      setSending(false);
    }
  };

  const handleDelete = async () => {
    if (announcementToDelete === null) return;
    try {
      await axios.delete(`https://andrew20x-001-site1.itempurl.com/api/announcements/${announcementToDelete}`);
      setToast({ open: true, message: 'Announcement deleted', severity: 'success' });
      fetchAnnouncements();
      setDeleteConfirmOpen(false);
      setAnnouncementToDelete(null);
    } catch (err) {
      console.error(err);
      setToast({ open: true, message: 'Failed to delete announcement.', severity: 'error' });
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' }, gap: 2, mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 'normal', color: '#1E293B', fontSize: { xs: '1.75rem', sm: '2.125rem' } }}>
          Manage Announcements
        </Typography>
        <Button variant="contained" color="primary" startIcon={<Plus />} onClick={() => setOpen(true)} sx={{ borderRadius: 2, width: { xs: '100%', sm: 'auto' } }}>
          Post Announcement
        </Button>
      </Box>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 5 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(2, 1fr)', lg: 'repeat(3, 1fr)' }, gap: 3 }}>
          {announcements.map((ann) => (
            <Box key={ann.id}>
              <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column', borderRadius: 3, boxShadow: '0 4px 20px rgba(0,0,0,0.05)' }}>
                <CardContent sx={{ flexGrow: 1 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                    <Chip
                      label={ann.type}
                      size="small"
                      sx={{
                        bgcolor: ann.type === 'Holiday' ? '#F0FDF4' : '#EFF6FF',
                        color: ann.type === 'Holiday' ? '#16A34A' : '#2563EB',
                        fontWeight: 400, height: 24, fontSize: '0.75rem'
                      }}
                    />
                    <IconButton color="error" size="small" onClick={() => { setAnnouncementToDelete(ann.id); setDeleteConfirmOpen(true); }}>
                      <Trash2 size={18} />
                    </IconButton>
                  </Box>
                  <Typography variant="h6" sx={{ fontWeight: 'normal', mb: 1 }}>{ann.title}</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: 'pre-wrap' }}>
                    {ann.content}
                  </Typography>
                  <Typography variant="caption" color="text.disabled" sx={{ display: 'block', mt: 2 }}>
                    Posted on {new Date(ann.createdAt).toLocaleDateString()}
                  </Typography>
                </CardContent>
              </Card>
            </Box>
          ))}
        </Box>
      )}

      {/* Create Announcement Dialog */}
      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 'normal' }}>Post New Announcement</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth label="Title" margin="normal"
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
          />
          <TextField
            fullWidth select label="Type" margin="normal"
            slotProps={{ select: { native: true } }}
            value={formData.type}
            onChange={(e) => setFormData({ ...formData, type: e.target.value })}
          >
            <option value="Notice">Notice</option>
            <option value="Holiday">Holiday</option>
          </TextField>
          <TextField
            fullWidth label="Content" margin="normal" multiline rows={4}
            value={formData.content}
            onChange={(e) => setFormData({ ...formData, content: e.target.value })}
          />

          {/* ── Feature 1: Recipient Selector ── */}
          <Box sx={{ mt: 2, p: 2, bgcolor: '#F8FAFC', borderRadius: 2, border: '1px solid #E2E8F0' }}>
            <FormControlLabel
              control={
                <Switch
                  checked={sendToAll}
                  onChange={(e) => {
                    setSendToAll(e.target.checked);
                    if (e.target.checked) setSelectedEmployee(null);
                  }}
                  color="primary"
                />
              }
              label={<Typography variant="body2" sx={{ fontWeight: 400 }}>Send to all employees</Typography>}
            />

            {!sendToAll && (
              <Box sx={{ mt: 2 }}>
                <Autocomplete
                  options={employees}
                  getOptionLabel={(e) => `${e.firstName} ${e.lastName}${e.email ? ` — ${e.email}` : ''}`}
                  value={selectedEmployee}
                  onChange={(_, newVal) => setSelectedEmployee(newVal)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Select Recipient"
                      placeholder="Search by name or email…"
                      variant="outlined"
                      size="small"
                      required={!sendToAll}
                    />
                  )}
                  noOptionsText="No employees with email found"
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
                {selectedEmployee && (
                  <Typography variant="caption" color="primary" sx={{ mt: 0.5, display: 'block' }}>
                    ✓ Email will be sent to: {selectedEmployee.email}
                  </Typography>
                )}
              </Box>
            )}
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setOpen(false)} color="inherit">Cancel</Button>
          <Button onClick={handleCreate} variant="contained" color="primary" disabled={sending}>
            {sending ? <CircularProgress size={22} color="inherit" /> : 'Post'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirm Dialog */}
      <Dialog open={deleteConfirmOpen} onClose={() => setDeleteConfirmOpen(false)}>
        <DialogTitle sx={{ fontWeight: 'normal', color: 'error.main' }}>Confirm Deletion</DialogTitle>
        <DialogContent>
          <Typography>Are you sure you want to delete this announcement? This action cannot be undone.</Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setDeleteConfirmOpen(false)} color="inherit">Cancel</Button>
          <Button onClick={handleDelete} variant="contained" color="error">Delete</Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={toast.open} autoHideDuration={5000} onClose={handleCloseToast} anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>
        <Alert onClose={handleCloseToast} severity={toast.severity} sx={{ width: '100%', borderRadius: 2, boxShadow: 3 }}>
          {toast.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
