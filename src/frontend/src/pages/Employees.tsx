import { useState, useEffect } from 'react';
import { Typography, Box, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Button, CircularProgress, Alert, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Snackbar, InputAdornment, Autocomplete, Checkbox } from '@mui/material';
import { Plus, Trash2, Edit2, Key, Search, Calendar, UserCog } from 'lucide-react';
import axios from 'axios';
import { useAuth } from '../contexts/AuthContext';

interface Employee {
  id: number;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  departmentId: number;
  positionId: number;
  position?: { id: number; name: string };
  role?: string;
  username?: string;
  managerId?: number | null;
  managerName?: string;
  subordinateIds?: number[];
  subordinatesList?: string;
}

export default function Employees() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';
  const [employees, setEmployees] = useState<Employee[]>([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [open, setOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [positionInput, setPositionInput] = useState('');
  const defaultEmployeeState = {
    employeeCode: '',
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    deviceUserId: 0,
    departmentId: 1,
    positionId: 1,
    shiftId: 1,
    managerId: null as number | null,
    subordinateIds: [] as number[]
  };
  const [newEmployee, setNewEmployee] = useState(defaultEmployeeState);
  const [isEditing, setIsEditing] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);

  const [resetConfirmOpen, setResetConfirmOpen] = useState(false);
  const [employeeToReset, setEmployeeToReset] = useState<number | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [employeeToDelete, setEmployeeToDelete] = useState<number | null>(null);

  const [editUserOpen, setEditUserOpen] = useState(false);
  const [editUserLoading, setEditUserLoading] = useState(false);
  const [editUserForm, setEditUserForm] = useState({
    username: '',
    password: '',
    fullName: '',
    email: '',
    employeeCode: '',
    positionName: '',
    role: 'Employee',
    managerId: null as number | null,
    subordinateIds: [] as number[]
  });


  const [balanceDialogOpen, setBalanceDialogOpen] = useState(false);
  const [balanceEmployee, setBalanceEmployee] = useState<Employee | null>(null);
  const [leaveBalance, setLeaveBalance] = useState({
    year: new Date().getFullYear(),
    annualLeaveTotal: 15,
    annualLeaveUsed: 0,
    emergencyLeaveTotal: 6,
    emergencyLeaveUsed: 0
  });

  const fetchEmployees = async (showLoading = true) => {
    if (showLoading) setLoading(true);
    try {
      const response = await axios.get('http://localhost:5222/api/Employees');
      setEmployees(response.data);
    } catch (err: any) {
      setError('Failed to load employees.');
    } finally {
      if (showLoading) setLoading(false);
    }
  };

  useEffect(() => {
    fetchEmployees(true);
  }, []);

  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const showMessage = (message: string, severity: 'success' | 'error') => {
    setSnackbar({ open: true, message, severity });
  };

  const handleCloseSnackbar = () => {
    setSnackbar({ ...snackbar, open: false });
  };

  const handleOpenBalance = async (employee: Employee) => {
    try {
      setBalanceEmployee(employee);
      const currentYear = new Date().getFullYear();
      const response = await axios.get(`http://localhost:5222/api/Leaves/balance/${employee.id}?year=${currentYear}`);
      setLeaveBalance({
        year: currentYear,
        annualLeaveTotal: response.data.annualLeaveTotal,
        annualLeaveUsed: response.data.annualLeaveUsed,
        emergencyLeaveTotal: response.data.emergencyLeaveTotal,
        emergencyLeaveUsed: response.data.emergencyLeaveUsed
      });
      setBalanceDialogOpen(true);
    } catch (err) {
      showMessage('Failed to fetch leave balance.', 'error');
    }
  };

  const handleSaveBalance = async () => {
    if (!balanceEmployee) return;
    try {
      await axios.put(`http://localhost:5222/api/Leaves/balance/${balanceEmployee.id}`, leaveBalance);
      showMessage('Leave balance updated successfully.', 'success');
      setBalanceDialogOpen(false);
    } catch (err) {
      showMessage('Failed to update leave balance.', 'error');
    }
  };

  const confirmDelete = (id: number) => {
    setEmployeeToDelete(id);
    setDeleteConfirmOpen(true);
  };

  const handleDelete = async () => {
    if (employeeToDelete === null) return;
    try {
      await axios.delete(`http://localhost:5222/api/Employees/${employeeToDelete}`);
      showMessage('Employee deleted successfully', 'success');
      fetchEmployees(false);
    } catch (err) {
      showMessage('Failed to delete employee. They might be tied to existing attendance records.', 'error');
    } finally {
      setDeleteConfirmOpen(false);
      setEmployeeToDelete(null);
    }
  };

  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [bulkDeleteConfirmOpen, setBulkDeleteConfirmOpen] = useState(false);

  const handleSelectAllClick = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      const newSelecteds = filteredEmployees.map((n) => n.id);
      setSelectedIds(newSelecteds);
      return;
    }
    setSelectedIds([]);
  };

  const handleSelectClick = (id: number) => {
    const selectedIndex = selectedIds.indexOf(id);
    let newSelected: number[] = [];

    if (selectedIndex === -1) {
      newSelected = newSelected.concat(selectedIds, id);
    } else if (selectedIndex === 0) {
      newSelected = newSelected.concat(selectedIds.slice(1));
    } else if (selectedIndex === selectedIds.length - 1) {
      newSelected = newSelected.concat(selectedIds.slice(0, -1));
    } else if (selectedIndex > 0) {
      newSelected = newSelected.concat(
        selectedIds.slice(0, selectedIndex),
        selectedIds.slice(selectedIndex + 1),
      );
    }
    setSelectedIds(newSelected);
  };

  const isSelected = (id: number) => selectedIds.indexOf(id) !== -1;

  const handleBulkDelete = async () => {
    if (selectedIds.length === 0) return;
    try {
      await axios.post(`http://localhost:5222/api/Employees/bulk-delete`, selectedIds);
      showMessage('Selected employees deleted successfully', 'success');
      fetchEmployees(false);
      setSelectedIds([]);
    } catch (err) {
      showMessage('Failed to bulk delete employees.', 'error');
    } finally {
      setBulkDeleteConfirmOpen(false);
    }
  };

  const confirmResetPassword = (id: number) => {
    setEmployeeToReset(id);
    setResetConfirmOpen(true);
  };

  const handleResetPassword = async () => {
    if (employeeToReset === null) return;
    try {
      await axios.post(`http://localhost:5222/api/Auth/force-reset-password/${employeeToReset}`);
      showMessage('Password reset successfully to measuresoft', 'success');
    } catch (err: any) {
      showMessage(err.response?.data?.message || 'Failed to reset password', 'error');
    } finally {
      setResetConfirmOpen(false);
      setEmployeeToReset(null);
    }
  };

  const handleAddOrEdit = async () => {
    try {
      let currentPosId = newEmployee.positionId;
      if (positionInput.trim()) {
        const posRes = await axios.post('http://localhost:5222/api/Positions/get-or-create', { name: positionInput.trim() });
        currentPosId = posRes.data.id;
      }

      const payload = { ...newEmployee, positionId: currentPosId, isActive: true };

      if (isEditing && editingId) {
        await axios.put(`http://localhost:5222/api/Employees/${editingId}`, payload);
      } else {
        await axios.post('http://localhost:5222/api/Employees', payload);
      }
      setOpen(false);
      showMessage('Employee saved successfully', 'success');
      fetchEmployees(false);

    } catch (err: any) {
      showMessage(err.response?.data?.error || 'Failed to save employee', 'error');
    }
  };

  const handleGenerateLogins = async () => {
    try {
      const response = await axios.post('http://localhost:5222/api/Auth/generate-logins');
      showMessage(response.data.message || 'Logins generated successfully', 'success');
    } catch (err: any) {
      showMessage('Failed to generate logins', 'error');
    }
  };

  const openAddDialog = () => {
    setIsEditing(false);
    setEditingId(null);
    setNewEmployee(defaultEmployeeState);
    setPositionInput('');
    setOpen(true);
  };

  const openEditDialog = (employee: Employee) => {
    setIsEditing(true);
    setEditingId(employee.id);
    setNewEmployee({
      ...defaultEmployeeState,
      employeeCode: employee.employeeCode,
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email || '',
      positionId: employee.positionId || 1,
      managerId: employee.managerId || null,
      subordinateIds: employee.subordinateIds || []
    });
    setPositionInput(employee.position?.name || '');
    setOpen(true);
  };

  const openEditUserDialog = async (employee: Employee) => {
    setEditingId(employee.id);
    setEditUserLoading(true);
    setEditUserOpen(true);
    try {
      // First, get the latest employee details
      const empRes = await axios.get(`http://localhost:5222/api/Employees/${employee.id}`);
      const emp = empRes.data;

      // Fetch the actual user linked to this employee
      let currentUsername = emp.username || (emp.role === 'Manager' ? `MANAGER-SYNC-${emp.employeeCode}` : `EMP-SYNC-${emp.employeeCode}`);
      let userRole = employee.role || 'Employee';
      let userEmail = emp.email || '';

      try {
        const userRes = await axios.get(`http://localhost:5222/api/Users/by-employee/${employee.id}`);
        if (userRes.data) {
          currentUsername = userRes.data.username;
          userRole = userRes.data.role;
          if (userRes.data.email) userEmail = userRes.data.email;
        }
      } catch (e) {
        // Fallback to defaults if user isn't found (e.g. hasn't generated logins yet)
      }

      setEditUserForm({
        username: currentUsername,
        password: '',
        fullName: `${emp.firstName} ${emp.lastName}`,
        email: userEmail,
        employeeCode: emp.employeeCode || '',
        positionName: emp.position?.name || '',
        role: userRole,
        managerId: emp.managerId || null,
        subordinateIds: employees.filter(e => e.managerId === employee.id).map(e => e.id)
      });
    } catch (err: any) {
      showMessage('Failed to load employee details.', 'error');
      setEditUserOpen(false);
    } finally {
      setEditUserLoading(false);
    }
  };

  const handleSaveEditUser = async () => {
    if (!editingId) return;
    setEditUserLoading(true);
    try {
      await axios.put(`http://localhost:5222/api/Users/${editingId}`, editUserForm);
      showMessage('User account updated successfully.', 'success');
      setEditUserOpen(false);
      fetchEmployees(false);
    } catch (err: any) {
      showMessage(err.response?.data?.error || 'Failed to update user account.', 'error');
    } finally {
      setEditUserLoading(false);
    }
  };



  const filteredEmployees = employees.filter(emp => {
    const q = searchQuery.toLowerCase();
    const fullName = `${emp.firstName} ${emp.lastName}`.toLowerCase();
    return (
      fullName.includes(q) ||
      emp.employeeCode.toLowerCase().includes(q) ||
      (emp.email && emp.email.toLowerCase().includes(q))
    );
  }).sort((a, b) => {
    const nameA = `${a.firstName} ${a.lastName}`.toLowerCase();
    const nameB = `${b.firstName} ${b.lastName}`.toLowerCase();
    return nameA.localeCompare(nameB);
  });

  return (
    <Box>
      <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' }, gap: 2, mb: 3 }}>
        <Typography variant="h4" sx={{ m: 0 }}>
          Employee Directory
        </Typography>
        <Box sx={{ display: 'flex', gap: 2, width: { xs: '100%', sm: 'auto' } }}>
          <Button variant="outlined" color="error" onClick={() => setBulkDeleteConfirmOpen(true)} disabled={selectedIds.length === 0}>
            Delete Selected ({selectedIds.length})
          </Button>
          <Button variant="outlined" onClick={handleGenerateLogins}>
            Generate Logins
          </Button>
          <Button variant="contained" startIcon={<Plus size={18} />} onClick={openAddDialog}>
            Add Employee
          </Button>
        </Box>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

      <Dialog open={open} onClose={() => setOpen(false)}>
        <DialogTitle>{isEditing ? 'Edit Employee' : 'Add New Employee'}</DialogTitle>
        <DialogContent>
          <TextField autoFocus margin="dense" label="Employee Code (e.g. EMP002)" fullWidth variant="outlined" value={newEmployee.employeeCode} disabled={isEditing} onChange={e => setNewEmployee({ ...newEmployee, employeeCode: e.target.value })} sx={{ mb: 2, mt: 1 }} />
          <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
            <TextField label="First Name" fullWidth variant="outlined" value={newEmployee.firstName} onChange={e => setNewEmployee({ ...newEmployee, firstName: e.target.value })} />
            <TextField label="Last Name" fullWidth variant="outlined" value={newEmployee.lastName} onChange={e => setNewEmployee({ ...newEmployee, lastName: e.target.value })} />
          </Box>
          <TextField margin="dense" label="Email Address" type="email" fullWidth variant="outlined" value={newEmployee.email} onChange={e => setNewEmployee({ ...newEmployee, email: e.target.value })} sx={{ mb: 2 }} />
          <TextField
            margin="dense"
            label="Job Position"
            fullWidth
            variant="outlined"
            value={positionInput}
            onChange={e => setPositionInput(e.target.value)}
            sx={{ mb: 2 }}
          />
          <Autocomplete
            multiple
            options={employees}
            getOptionLabel={(option) => `${option.firstName} ${option.lastName} (${option.employeeCode}) - ${option.email && option.email !== '(No Email)' ? option.email : 'No Email'}`}
            value={employees.filter(e => newEmployee.subordinateIds.includes(e.id))}
            onChange={(_, newValue) => setNewEmployee({ ...newEmployee, subordinateIds: newValue.map(v => v.id) })}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            renderInput={(params) => (
              <TextField
                {...params}
                margin="dense"
                label="Subordinates (Optional)"
                fullWidth
                variant="outlined"
              />
            )}
            sx={{ mb: 2 }}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setOpen(false)}>Cancel</Button>
          <Button onClick={handleAddOrEdit} variant="contained">Save Employee</Button>
        </DialogActions>
      </Dialog>

      <Box sx={{ mb: 3 }}>
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Search by name, code, or email..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          autoComplete="off"
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search size={20} color="#94a3b8" />
                </InputAdornment>
              ),
            }
          }}
          sx={{ bgcolor: 'white', borderRadius: 1 }}
        />
      </Box>

      <TableContainer component={Paper} elevation={2}>
        <Table sx={{ minWidth: 650 }} aria-label="employee table">
          <TableHead sx={{ bgcolor: 'rgba(0,0,0,0.02)' }}>
            <TableRow>
              <TableCell padding="checkbox">
                <Checkbox
                  color="primary"
                  indeterminate={selectedIds.length > 0 && selectedIds.length < filteredEmployees.length}
                  checked={filteredEmployees.length > 0 && selectedIds.length === filteredEmployees.length}
                  onChange={handleSelectAllClick}
                  aria-label="select all employees"
                />
              </TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Code</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Name</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Email</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Managed Employees</TableCell>
              <TableCell sx={{ fontWeight: 'normal' }}>Position</TableCell>
              <TableCell align="right" sx={{ fontWeight: 'normal' }}>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 5 }}>
                  <CircularProgress />
                </TableCell>
              </TableRow>
            ) : filteredEmployees.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 5 }}>
                  <Typography color="text.secondary">No employees found.</Typography>
                </TableCell>
              </TableRow>
            ) : (
              filteredEmployees.map((row) => {
                const isItemSelected = isSelected(row.id);
                return (
                <TableRow
                  key={row.id}
                  selected={isItemSelected}
                  sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                >
                  <TableCell padding="checkbox">
                    <Checkbox
                      color="primary"
                      checked={isItemSelected}
                      onChange={() => handleSelectClick(row.id)}
                    />
                  </TableCell>
                  <TableCell component="th" scope="row">{row.employeeCode.startsWith('no ID') ? 'no ID' : row.employeeCode}</TableCell>
                  <TableCell>{row.firstName} {row.lastName}</TableCell>
                  <TableCell>{row.email}</TableCell>
                  <TableCell>
                    {row.subordinatesList ? row.subordinatesList : <span style={{ color: '#94a3b8' }}>None</span>}
                  </TableCell>
                  <TableCell>
                    {row.position?.name || 'N/A'}
                    <span style={{ color: '#64748b', fontSize: '0.85em', marginLeft: '8px' }}>
                      ({row.role || 'Employee'})
                    </span>
                  </TableCell>
                  <TableCell align="right">
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', flexWrap: 'nowrap', gap: 1 }}>
                      <Button color="success" size="small" onClick={() => handleOpenBalance(row)} title="Set Leave Balance" sx={{ minWidth: 32 }}>
                        <Calendar size={16} />
                      </Button>
                      <Button color="secondary" size="small" onClick={() => confirmResetPassword(row.id)} title="Reset Password" sx={{ minWidth: 32 }}>
                        <Key size={16} />
                      </Button>
                      {isAdmin ? (
                        <Button color="primary" size="small" onClick={() => openEditUserDialog(row)} title="Edit User Account" sx={{ minWidth: 32 }}>
                          <UserCog size={16} />
                        </Button>
                      ) : (
                        <Button color="primary" size="small" onClick={() => openEditDialog(row)} title="Edit Employee" sx={{ minWidth: 32 }}>
                          <Edit2 size={16} />
                        </Button>
                      )}
                      <Button color="error" size="small" onClick={() => confirmDelete(row.id)} title="Delete Employee" sx={{ minWidth: 32 }}>
                        <Trash2 size={16} />
                      </Button>
                    </Box>
                  </TableCell>
                </TableRow>
              )})
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={editUserOpen} onClose={() => setEditUserOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 'normal' }}>Edit User Account</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 1, display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField label="Username" fullWidth variant="outlined" value={editUserForm.username} onChange={e => setEditUserForm({ ...editUserForm, username: e.target.value })} />

            <TextField label="Full Name" fullWidth variant="outlined" value={editUserForm.fullName} onChange={e => setEditUserForm({ ...editUserForm, fullName: e.target.value })} />
            <TextField label="Email" type="email" fullWidth variant="outlined" value={editUserForm.email} onChange={e => setEditUserForm({ ...editUserForm, email: e.target.value })} />
            <TextField label="Employee Code" fullWidth variant="outlined" value={editUserForm.employeeCode} onChange={e => setEditUserForm({ ...editUserForm, employeeCode: e.target.value })} />
            <TextField label="Position" fullWidth variant="outlined" value={editUserForm.positionName} onChange={e => setEditUserForm({ ...editUserForm, positionName: e.target.value })} />

            <TextField select fullWidth label="Role" value={editUserForm.role} onChange={e => setEditUserForm({ ...editUserForm, role: e.target.value })} slotProps={{ select: { native: true } }}>
              <option value="Employee">Employee</option>
              <option value="Manager">Manager</option>
              <option value="HR">HR</option>
              <option value="Admin">Admin</option>
            </TextField>

            <Autocomplete
              multiple
              options={employees.filter(emp => emp.id !== editingId)}
              getOptionLabel={(option) => `${option.firstName} ${option.lastName}`}
              value={employees.filter(e => editUserForm.subordinateIds.includes(e.id))}
              onChange={(_, newValue) => setEditUserForm({ ...editUserForm, subordinateIds: newValue.map(v => v.id) })}
              isOptionEqualToValue={(option, value) => option.id === value.id}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Managed Employees"
                  fullWidth
                  variant="outlined"
                />
              )}
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setEditUserOpen(false)} color="inherit" disabled={editUserLoading}>Cancel</Button>
          <Button onClick={handleSaveEditUser} variant="contained" color="primary" disabled={editUserLoading}>
            {editUserLoading ? <CircularProgress size={24} /> : 'Save Changes'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={resetConfirmOpen} onClose={() => setResetConfirmOpen(false)}>
        <DialogTitle sx={{ fontWeight: 'normal' }}>Reset Password</DialogTitle>
        <DialogContent>
          <Typography>Are you sure you want to reset this employee's password to 'measuresoft'?</Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setResetConfirmOpen(false)} color="inherit">Cancel</Button>
          <Button onClick={handleResetPassword} variant="contained" color="secondary">Reset Password</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={deleteConfirmOpen} onClose={() => setDeleteConfirmOpen(false)}>
        <DialogTitle sx={{ fontWeight: 'normal', color: 'error.main' }}>Delete Employee</DialogTitle>
        <DialogContent>
          <Typography>Are you sure you want to delete this employee? This action cannot be undone.</Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setDeleteConfirmOpen(false)} color="inherit">Cancel</Button>
          <Button onClick={handleDelete} variant="contained" color="error">Delete</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={balanceDialogOpen} onClose={() => setBalanceDialogOpen(false)}>
        <DialogTitle sx={{ fontWeight: 'normal' }}>Leave Balance for {balanceEmployee?.firstName} {balanceEmployee?.lastName}</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 3 }}>
            <Box>
              <Typography variant="subtitle1" sx={{ mb: 1, fontWeight: 'normal' }}>Annual Leave (RDO)</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Remaining: <span>{leaveBalance.annualLeaveTotal - leaveBalance.annualLeaveUsed}</span>
              </Typography>
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField
                  label={`Used (${leaveBalance.year})`}
                  type="number"
                  fullWidth
                  variant="outlined"
                  value={leaveBalance.annualLeaveUsed}
                  onChange={e => setLeaveBalance({ ...leaveBalance, annualLeaveUsed: parseInt(e.target.value) || 0 })}
                />
                <TextField
                  label={`Total Allowed (${leaveBalance.year})`}
                  type="number"
                  fullWidth
                  variant="outlined"
                  value={leaveBalance.annualLeaveTotal}
                  onChange={e => setLeaveBalance({ ...leaveBalance, annualLeaveTotal: parseInt(e.target.value) || 0 })}
                />
              </Box>
            </Box>

            <Box>
              <Typography variant="subtitle1" sx={{ mb: 1, fontWeight: 'normal' }}>Emergency Leave (EDO)</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Remaining: <span>{leaveBalance.emergencyLeaveTotal - leaveBalance.emergencyLeaveUsed}</span>
              </Typography>
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField
                  label={`Used (${leaveBalance.year})`}
                  type="number"
                  fullWidth
                  variant="outlined"
                  value={leaveBalance.emergencyLeaveUsed}
                  onChange={e => setLeaveBalance({ ...leaveBalance, emergencyLeaveUsed: parseInt(e.target.value) || 0 })}
                />
                <TextField
                  label={`Total Allowed (${leaveBalance.year})`}
                  type="number"
                  fullWidth
                  variant="outlined"
                  value={leaveBalance.emergencyLeaveTotal}
                  onChange={e => setLeaveBalance({ ...leaveBalance, emergencyLeaveTotal: parseInt(e.target.value) || 0 })}
                />
              </Box>
            </Box>
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setBalanceDialogOpen(false)} color="inherit">Cancel</Button>
          <Button onClick={handleSaveBalance} variant="contained" color="primary">Save Balance</Button>
        </DialogActions>
      </Dialog>

      {/* Bulk Delete Confirm Dialog */}
      <Dialog open={bulkDeleteConfirmOpen} onClose={() => setBulkDeleteConfirmOpen(false)}>
        <DialogTitle sx={{ fontWeight: 'normal', color: 'error.main' }}>Bulk Delete Employees</DialogTitle>
        <DialogContent>
          <Typography>Are you sure you want to delete {selectedIds.length} employees? This action cannot be undone and will erase all their records.</Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setBulkDeleteConfirmOpen(false)} color="inherit">Cancel</Button>
          <Button onClick={handleBulkDelete} variant="contained" color="error">Delete All Selected</Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={snackbar.open} autoHideDuration={3000} onClose={handleCloseSnackbar} anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}>
        <Alert onClose={handleCloseSnackbar} severity={snackbar.severity} sx={{ width: '100%' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
