import React, { useState } from 'react';
import {
  Typography, Box, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Button, CircularProgress,
  Alert, TextField, MenuItem, Chip
} from '@mui/material';
import { Download, Search } from 'lucide-react';
import axios from 'axios';
import { formatTime12Hour } from '../utils/dateUtils';

interface AttendanceRecord {
  employeeId: number;
  employeeCode: string;
  employeeName: string;
  date: string;
  checkIn: string | null;
  checkOut: string | null;
  status: string;
  workedHours: number;
  lateMinutes: number;
  earlyLeaveMinutes: number;
  overtimeMinutes: number;
}

const formatMinutes = (minutes: number) => {
  if (!minutes || minutes <= 0) return '-';
  if (minutes < 60) return `${minutes} Mins`;
  return `${Math.floor(minutes / 60)}:${(minutes % 60).toString().padStart(2, '0')} Hr(s)`;
};

const formatWorkedHours = (hours: number | undefined | null) => {
  if (!hours || hours <= 0) return '-';
  const totalMins = Math.round(hours * 60);
  if (totalMins < 60) return `${totalMins} Mins`;
  const h = Math.floor(totalMins / 60);
  const m = totalMins % 60;
  return `${h}:${m.toString().padStart(2, '0')} Hr(s)`;
};

export default function Reports() {
  const [records, setRecords] = useState<AttendanceRecord[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const [filters, setFilters] = useState({
    startDate: '',
    endDate: '',
    employeeName: '',
    employeeCode: '',
    departmentId: '',
    status: 'All'
  });

  const statuses = [
    { value: 'All', label: 'All' },
    { value: 'Present', label: 'Present' },
    { value: 'Absent', label: 'Absent' },
    { value: 'Late', label: 'Late' },
    { value: 'HalfDay', label: 'Half Day' }
  ];

  const handleFilterChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFilters(prev => ({ ...prev, [name]: value }));
  };

  const buildQueryString = () => {
    const params = new URLSearchParams();
    if (filters.startDate) params.append('StartDate', filters.startDate);
    if (filters.endDate) params.append('EndDate', filters.endDate);
    if (filters.employeeName) params.append('EmployeeName', filters.employeeName);
    if (filters.employeeCode) params.append('EmployeeCode', filters.employeeCode);
    if (filters.departmentId) params.append('DepartmentId', filters.departmentId);

    // Convert status string to enum integer based on backend enum if needed, 
    // but the backend uses AttendanceStatus which is mapped from string by default in JSON if using strings.
    // Assuming backend enum mapping handles string. Let's send it as is, or we might need mapping.
    // Enum values in backend: Present=1, Absent=2, Late=3, HalfDay=4
    if (filters.status && filters.status !== 'All') {
      let statusVal = '';
      if (filters.status === 'Present') statusVal = '1';
      else if (filters.status === 'Late') statusVal = '2';
      else if (filters.status === 'Absent') statusVal = '3';
      else if (filters.status === 'HalfDay') statusVal = '4';
      if (statusVal) params.append('Status', statusVal);
    }
    return params.toString();
  };

  const handleSearch = async () => {
    setLoading(true);
    setError('');
    try {
      const qs = buildQueryString();
      const response = await axios.get(`http://localhost:5222/api/Reports/search?${qs}`);
      setRecords(response.data);
    } catch (err: any) {
      setError('Failed to fetch reports.');
    } finally {
      setLoading(false);
    }
  };

  const handleExport = async (format: string) => {
    try {
      const qs = buildQueryString();
      const url = `http://localhost:5222/api/Reports/export?${qs}&format=${format}`;

      const response = await axios.get(url, {
        responseType: 'blob'
      });

      const blob = new Blob([response.data], {
        type: response.headers['content-type'] as string
      });

      const downloadUrl = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = downloadUrl;

      let ext = '.xlsx';
      if (format === 'csv') ext = '.csv';
      if (format === 'pdf') ext = '.pdf';

      link.setAttribute('download', `Report_${new Date().getTime()}${ext}`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(downloadUrl);
    } catch (err: any) {
      setError(`Failed to export to ${format.toUpperCase()}.`);
    }
  };

  return (
    <Box>
      <Box sx={{ mb: 4, display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' }, gap: 2 }}>
        <Typography variant="h4" sx={{ fontWeight: 'normal', color: 'text.primary', fontSize: { xs: '1.75rem', sm: '2.125rem' } }}>
          Reports & Advanced Search
        </Typography>
        <Box sx={{ display: 'flex', gap: 2, flexDirection: { xs: 'column', sm: 'row' } }}>
          <Button
            variant="outlined"
            startIcon={<Download size={18} />}
            onClick={() => handleExport('excel')}
            color="success"
            sx={{ width: { xs: '100%', sm: 'auto' } }}
          >
            Excel
          </Button>
          <Button
            variant="outlined"
            startIcon={<Download size={18} />}
            onClick={() => handleExport('csv')}
            color="info"
            sx={{ width: { xs: '100%', sm: 'auto' } }}
          >
            CSV
          </Button>
          <Button
            variant="outlined"
            startIcon={<Download size={18} />}
            onClick={() => handleExport('pdf')}
            color="error"
          >
            PDF
          </Button>
        </Box>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

      <Paper sx={{ p: 3, mb: 4, borderRadius: 2 }}>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3 }}>
          <Box sx={{ flex: '1 1 200px' }}>
            <TextField
              fullWidth
              label="Start Date"
              type="date"
              name="startDate"
              value={filters.startDate}
              onChange={handleFilterChange}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Box>
          <Box sx={{ flex: '1 1 200px' }}>
            <TextField
              fullWidth
              label="End Date"
              type="date"
              name="endDate"
              value={filters.endDate}
              onChange={handleFilterChange}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Box>
          <Box sx={{ flex: '1 1 200px' }}>
            <TextField
              fullWidth
              label="Employee Name"
              name="employeeName"
              value={filters.employeeName}
              onChange={handleFilterChange}
            />
          </Box>
          <Box sx={{ flex: '1 1 200px' }}>
            <TextField
              fullWidth
              label="Employee Code"
              name="employeeCode"
              value={filters.employeeCode}
              onChange={handleFilterChange}
            />
          </Box>
          <Box sx={{ flex: '1 1 200px' }}>
            <TextField
              fullWidth
              select
              label="Status"
              name="status"
              value={filters.status}
              onChange={handleFilterChange}
            >
              {statuses.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </TextField>
          </Box>
          <Box sx={{ flex: '1 1 200px', display: 'flex', alignItems: 'center' }}>
            <Button
              fullWidth
              variant="contained"
              startIcon={<Search size={18} />}
              onClick={handleSearch}
              disabled={loading}
              sx={{ height: '56px' }}
            >
              Search
            </Button>
          </Box>
        </Box>
      </Paper>

      <Paper sx={{ width: '100%', overflow: 'hidden', borderRadius: 2 }}>
        <TableContainer sx={{ maxHeight: 600 }}>
          <Table stickyHeader sx={{ minWidth: 650 }}>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 'normal' }}>Date</TableCell>
                <TableCell sx={{ fontWeight: 'normal' }}>Employee ID</TableCell>
                <TableCell sx={{ fontWeight: 'normal' }}>Name</TableCell>
                <TableCell sx={{ fontWeight: 'normal' }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 'normal' }}>Check In</TableCell>
                <TableCell sx={{ fontWeight: 'normal' }}>Check Out</TableCell>
                <TableCell sx={{ fontWeight: 'normal' }}>Worked Hrs</TableCell>
                <TableCell sx={{ fontWeight: 'normal' }}>Late</TableCell>
                <TableCell sx={{ fontWeight: 'normal' }}>Overtime</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={9} align="center" sx={{ py: 3 }}>
                    <CircularProgress />
                  </TableCell>
                </TableRow>
              ) : records.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={9} align="center" sx={{ py: 3 }}>
                    No records found
                  </TableCell>
                </TableRow>
              ) : (
                records.map((row) => (
                  <TableRow key={`${row.employeeId}-${row.date}`} hover>
                    <TableCell>{new Date(row.date).toLocaleDateString()}</TableCell>
                    <TableCell>{row.employeeCode ? row.employeeCode.replace(/\D/g, '') : row.employeeId}</TableCell>
                    <TableCell>{row.employeeName}</TableCell>
                    <TableCell>
                      <Chip label={row.status} size="small" color={row.status === 'Present' ? 'success' : row.status === 'Absent' ? 'error' : 'warning'} />
                    </TableCell>
                    <TableCell>{formatTime12Hour(row.checkIn)}</TableCell>
                    <TableCell>{formatTime12Hour(row.checkOut)}</TableCell>
                    <TableCell>{formatWorkedHours(row.workedHours)}</TableCell>
                    <TableCell>{formatMinutes(row.lateMinutes)}</TableCell>
                    <TableCell>{formatMinutes(row.overtimeMinutes)}</TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
    </Box>
  );
}



