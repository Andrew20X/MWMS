import React, { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
  Alert,
  TextField,
  Button,
  Checkbox,
  FormControlLabel,
  Select,
  MenuItem,
  InputAdornment
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import axios from 'axios';
import { format } from 'date-fns';
import { useAuth } from '../contexts/AuthContext';

interface AuditLog {
  id: number;
  actionType: string;
  entityName: string;
  entityId: string;
  oldValues: string;
  newValues: string;
  changes: string;
  timestamp: string;
  adminUser: string | null;
  targetEmployee: string | null;
}

const SystemLogs: React.FC = () => {
  const { user } = useAuth();
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Filters State
  const [entityFilter, setEntityFilter] = useState('');
  const [userFilter, setUserFilter] = useState('');
  const [ipFilter, setIpFilter] = useState('');
  const [dateFilter, setDateFilter] = useState('All Time');
  const [showAll, setShowAll] = useState(false);

  const [appliedFilters, setAppliedFilters] = useState({
    entity: '',
    user: '',
    ip: '',
    date: 'All Time',
    showAll: false
  });

  useEffect(() => {
    if (user?.token) {
      fetchLogs();
    }
  }, [user?.token]);

  const fetchLogs = async () => {
    try {
      const response = await axios.get(`http://localhost:5222/api/AuditLogs?limit=100000`, {
        headers: {
          Authorization: `Bearer ${user?.token}`,
        },
      });
      if (Array.isArray(response.data)) {
        setLogs(response.data);
      } else if (response.data && Array.isArray(response.data.value)) {
        setLogs(response.data.value);
      } else {
        console.error('Unexpected API response format:', response.data);
        setLogs([]);
        setError('Received invalid data format from server.');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load audit logs.');
    } finally {
      setLoading(false);
    }
  };

  const handleRunFilter = () => {
    setAppliedFilters({
      entity: entityFilter,
      user: userFilter,
      ip: ipFilter,
      date: dateFilter,
      showAll: showAll
    });
  };

  const filteredLogs = useMemo(() => {
    if (appliedFilters.showAll) return logs;
    return logs.filter(log => {
      const matchEntity = appliedFilters.entity === '' || (log.entityName || '').toLowerCase().includes(appliedFilters.entity.toLowerCase());
      const matchUser = appliedFilters.user === '' || (log.adminUser || 'System').toLowerCase().includes(appliedFilters.user.toLowerCase());
      
      let matchDate = true;
      if (appliedFilters.date && appliedFilters.date !== 'All Time') {
        const logDate = new Date(log.timestamp);
        const now = new Date();
        const diffMs = now.getTime() - logDate.getTime();
        const diffDays = diffMs / (1000 * 60 * 60 * 24);
        
        if (appliedFilters.date === 'Last 24 Hours') {
          matchDate = diffDays <= 1;
        } else if (appliedFilters.date === 'Last 7 Days') {
          matchDate = diffDays <= 7;
        } else if (appliedFilters.date === 'Last 30 Days') {
          matchDate = diffDays <= 30;
        }
      }

      return matchEntity && matchUser && matchDate;
    });
  }, [logs, appliedFilters]);

  const getActionColor = (action: string) => {
    switch (action.toUpperCase()) {
      case 'ADDED': return 'success.main';
      case 'MODIFIED': return 'warning.main';
      case 'DELETED': return 'error.main';
      default: return 'primary.main';
    }
  };

  const cellStyle = {
    borderRight: '1px solid #e2e8f0',
    borderBottom: '1px solid #e2e8f0',
    py: 1.5,
    px: 2
  };

  const headerCellStyle = {
    color: 'white',
    fontWeight: 600,
    borderRight: '1px solid rgba(255,255,255,0.2)',
    borderBottom: 'none',
    py: 1.5,
    px: 2,
    height: 48,
    boxSizing: 'border-box'
  };

  return (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <Box sx={{ mb: 3 }}>
        <Typography variant="body2" sx={{ color: 'text.secondary', mb: 0.5 }}>
          Reporting / Management
        </Typography>
        <Typography variant="h4" sx={{ fontWeight: 400, color: 'text.primary' }}>
          Admin Audit Log
        </Typography>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

      {/* Main Layout Container */}
      <Box sx={{ 
        display: 'flex', 
        flexDirection: { xs: 'column', md: 'row' },
        flexGrow: 1, 
        alignItems: { xs: 'stretch', md: 'flex-start' }, 
        bgcolor: '#f8fafc', 
        p: { xs: 1, sm: 2 }, 
        borderRadius: 1,
        gap: { xs: 2, md: 0 }
      }}>
        
        {/* Sidebar Filters */}
        <Box sx={{ 
          width: { xs: '100%', md: 260 }, 
          flexShrink: 0, 
          bgcolor: '#f1f5f9', 
          border: '1px solid #e2e8f0' 
        }}>
          <Box sx={{ bgcolor: '#475569', color: 'white', px: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center', height: 48, boxSizing: 'border-box', borderBottom: '1px solid #e2e8f0' }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 600, fontSize: '0.85rem' }}>Filters</Typography>
          </Box>
          <Box sx={{ p: 2, display: 'flex', flexDirection: 'column', gap: 2.5 }}>
            <Box>
              <Typography variant="caption" sx={{ fontWeight: 600, color: '#334155', mb: 0.5, display: 'block' }}>
                Filter by Identities & Settings:
              </Typography>
              <TextField 
                size="small" fullWidth placeholder="Select an Identity/Setting" 
                value={entityFilter} onChange={(e) => setEntityFilter(e.target.value)}
                sx={{ bgcolor: 'white' }}
                slotProps={{
                  input: {
                    startAdornment: <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment>,
                  }
                }}
              />
            </Box>
            <Box>
              <Typography variant="caption" sx={{ fontWeight: 600, color: '#334155', mb: 0.5, display: 'block' }}>
                Filter by User:
              </Typography>
              <TextField 
                size="small" fullWidth placeholder="Select a User"
                value={userFilter} onChange={(e) => setUserFilter(e.target.value)}
                sx={{ bgcolor: 'white' }}
                slotProps={{
                  input: {
                    startAdornment: <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment>,
                  }
                }}
              />
            </Box>
            <Box>
              <Typography variant="caption" sx={{ fontWeight: 600, color: '#334155', mb: 0.5, display: 'block' }}>
                Filter by IP Address:
              </Typography>
              <TextField 
                size="small" fullWidth placeholder="Enter a IP Address"
                value={ipFilter} onChange={(e) => setIpFilter(e.target.value)}
                sx={{ bgcolor: 'white' }}
                slotProps={{
                  input: {
                    startAdornment: <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment>,
                  }
                }}
              />
            </Box>
            <Box>
              <Typography variant="caption" sx={{ fontWeight: 600, color: '#334155', mb: 0.5, display: 'block' }}>
                Filter by date:
              </Typography>
              <Select size="small" fullWidth value={dateFilter} onChange={(e) => setDateFilter(e.target.value)} sx={{ bgcolor: 'white' }}>
                <MenuItem value="All Time">All Time</MenuItem>
                <MenuItem value="Last 24 Hours">Last 24 Hours</MenuItem>
                <MenuItem value="Last 7 Days">Last 7 Days</MenuItem>
                <MenuItem value="Last 30 Days">Last 30 Days</MenuItem>
              </Select>
            </Box>
            <FormControlLabel 
              control={<Checkbox size="small" checked={showAll} onChange={(e) => setShowAll(e.target.checked)} sx={{ color: '#64748b' }} />} 
              label={<Typography variant="caption" sx={{ color: '#334155', fontWeight: 500 }}>Show All</Typography>} 
              sx={{ m: 0 }}
            />
            <Button variant="contained" color="primary" fullWidth sx={{ mt: 1, fontWeight: 'bold', py: 1 }} onClick={handleRunFilter}>
              RUN FILTER
            </Button>
          </Box>
        </Box>

        {/* Main Table */}
        <TableContainer sx={{ 
          flexGrow: 1, 
          border: '1px solid #e2e8f0', 
          borderLeft: { xs: '1px solid #e2e8f0', md: 'none' }, 
          bgcolor: 'white', 
          borderRadius: 0,
          overflowX: 'auto'
        }}>
          <Table size="small" sx={{ minWidth: 800 }}>
            <TableHead>
              <TableRow sx={{ bgcolor: '#475569' }}>
                <TableCell sx={headerCellStyle}>Date</TableCell>
                <TableCell sx={headerCellStyle}>Time</TableCell>
                <TableCell sx={headerCellStyle}>IP Address</TableCell>
                <TableCell sx={headerCellStyle}>User</TableCell>
                <TableCell sx={headerCellStyle}>Section</TableCell>
                <TableCell sx={{ ...headerCellStyle, borderRight: 'none' }}>Action</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 5, borderBottom: 'none' }}>
                    <CircularProgress />
                  </TableCell>
                </TableRow>
              ) : filteredLogs.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 5, borderBottom: 'none' }}>
                    No audit logs match the current filters.
                  </TableCell>
                </TableRow>
              ) : (
                filteredLogs.map((log) => {
                  const d = new Date(log.timestamp);
                  const dateStr = isNaN(d.getTime()) ? '-' : format(d, 'MMM. dd, yyyy');
                  const timeStr = isNaN(d.getTime()) ? '-' : format(d, 'h:mm:ss a');
                  const actionColor = getActionColor(log.actionType);

                  return (
                    <TableRow hover key={log.id || Math.random()} sx={{ '&:last-child td': { borderBottom: 'none' } }}>
                      <TableCell sx={cellStyle}>{dateStr}</TableCell>
                      <TableCell sx={cellStyle}>{timeStr}</TableCell>
                      <TableCell sx={cellStyle}>-</TableCell>
                      <TableCell sx={cellStyle}>{log.adminUser || 'System'}</TableCell>
                      <TableCell sx={cellStyle}>{log.entityName || 'General'}</TableCell>
                      <TableCell sx={{ ...cellStyle, borderRight: 'none' }}>
                        <Typography variant="body2" sx={{ color: actionColor, fontWeight: 500 }}>
                          {log.actionType} {log.entityName} {log.entityId ? `(ID: ${log.entityId})` : ''}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
    </Box>
  );
};

export default SystemLogs;
