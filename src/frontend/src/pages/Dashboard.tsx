import { useState, useEffect } from 'react';
import { Box, Typography, Paper, Chip, CircularProgress, Alert, Avatar, Divider, Button, Dialog, DialogTitle, DialogContent, IconButton } from '@mui/material';
import { Users, Clock, AlertCircle, CheckCircle2, CalendarDays, ArrowUpRight, Megaphone, X, RefreshCw } from 'lucide-react';

import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import axios from 'axios';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { formatTime12Hour } from '../utils/dateUtils';

interface DashboardStats {
  totalEmployees: number;
  presentToday: number;
  lateArrivals: number;
  absent: number;
}

interface AttendanceTrend {
  date: string;
  presentCount: number;
  absentCount: number;
  lateCount: number;
}

interface LiveAttendance {
  employeeId: number;
  employeeName: string;
  positionName: string;
  checkInTime: string;
  status: string;
}

export default function Dashboard() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [trend, setTrend] = useState<AttendanceTrend[]>([]);
  const [liveList, setLiveList] = useState<LiveAttendance[]>([]);
  const [announcements, setAnnouncements] = useState<any[]>([]);
  const [pendingAbsences, setPendingAbsences] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [openLiveDialog, setOpenLiveDialog] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const { user } = useAuth();
  const navigate = useNavigate();

  const refreshLiveActivity = async () => {
    if (!user) return;
    setRefreshing(true);
    try {
      const liveRes = await axios.get('http://localhost:5222/api/Dashboard/live', { headers: { Authorization: `Bearer ${user.token}` } });
      setLiveList(liveRes.data);
    } catch (err: any) {
      console.error("Failed to refresh live activity", err);
    } finally {
      setRefreshing(false);
    }
  };

  useEffect(() => {
    const fetchAnnouncements = async () => {
      try {
        const annRes = await axios.get('http://localhost:5222/api/Announcements', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        setAnnouncements(annRes.data);
      } catch (err) {
        console.error("Failed to load announcements", err);
      }
    };

    const fetchPendingAbsences = async () => {
      try {
        const res = await axios.get('http://localhost:5222/api/Attendance/pending-resolution', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        setPendingAbsences(res.data);
      } catch (err) {
        console.error("Failed to load pending absences", err);
      }
    };

    const fetchAdminData = async () => {
      try {
        const [statsRes, trendRes, liveRes] = await Promise.all([
          axios.get('http://localhost:5222/api/Dashboard/stats', { headers: { Authorization: `Bearer ${user?.token}` } }),
          axios.get('http://localhost:5222/api/Dashboard/trend?days=7', { headers: { Authorization: `Bearer ${user?.token}` } }),
          axios.get('http://localhost:5222/api/Dashboard/live', { headers: { Authorization: `Bearer ${user?.token}` } })
        ]);
        setStats(statsRes.data);
        setTrend(trendRes.data);
        setLiveList(liveRes.data);
      } catch (err: any) {
        setError('Failed to load dashboard statistics.');
      }
    };

    const loadAll = async () => {
      setLoading(true);
      await fetchAnnouncements();
      await fetchPendingAbsences();
      if (user?.role === 'Admin' || user?.role === 'HR') {
        await fetchAdminData();
      }
      setLoading(false);
    };
    
    if (user) {
      loadAll();
    }
  }, [user]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '60vh' }}>
        <CircularProgress size={40} thickness={4} sx={{ color: '#0F172A' }} />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error" sx={{ borderRadius: 2 }}>{error}</Alert>;
  }

  const statCards = [
    { 
      title: 'Total Workforce', 
      value: stats?.totalEmployees ?? 0, 
      icon: <Users size={24} color="#6366F1" />, 
      bgColor: 'rgba(99, 102, 241, 0.08)',
      borderColor: 'rgba(99, 102, 241, 0.2)',
    },
    { 
      title: 'Present Today', 
      value: stats?.presentToday ?? 0, 
      icon: <CheckCircle2 size={24} color="#10B981" />, 
      bgColor: 'rgba(16, 185, 129, 0.08)',
      borderColor: 'rgba(16, 185, 129, 0.2)',
    },
    { 
      title: 'Late Arrivals', 
      value: stats?.lateArrivals ?? 0, 
      icon: <Clock size={24} color="#F59E0B" />, 
      bgColor: 'rgba(245, 158, 11, 0.08)',
      borderColor: 'rgba(245, 158, 11, 0.2)',
    },
    { 
      title: 'Absent', 
      value: stats?.absent ?? 0, 
      icon: <AlertCircle size={24} color="#F43F5E" />, 
      bgColor: 'rgba(244, 63, 94, 0.08)',
      borderColor: 'rgba(244, 63, 94, 0.2)',
    },
  ];

  return (
    <Box sx={{ maxWidth: '1400px', margin: '0 auto', pb: 8 }}>
      <Box sx={{ mb: 5, display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'flex-end' }, gap: 2 }}>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 400, color: '#0F172A', letterSpacing: '-0.02em', mb: 1, fontSize: { xs: '2.25rem', sm: '3rem' } }}>
            Overview
          </Typography>
          <Typography variant="subtitle1" sx={{ color: '#64748B', fontWeight: 400, fontSize: { xs: '0.875rem', sm: '1rem' } }}>
            {user?.role === 'Admin' ? 'Measuresoft Operations & Workforce' : 'Company Announcements & Updates'}
          </Typography>
        </Box>
        <Typography variant="body2" sx={{ color: '#94A3B8', fontWeight: 400, display: { xs: 'block', sm: 'block' } }}>
          {new Date().toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
        </Typography>
      </Box>

      {pendingAbsences.length > 0 && (
        <Box sx={{ mb: 4 }}>
          {pendingAbsences.map(absence => (
            <Alert 
              key={absence.id}
              icon={<AlertCircle size={24} color="#FFF" />}
              sx={{ 
                borderRadius: 2, 
                mb: 2,
                display: 'flex',
                alignItems: 'center',
                bgcolor: '#EF4444', // Tailwind red-500
                color: 'white',
                boxShadow: '0 4px 6px -1px rgba(239, 68, 68, 0.4)'
              }}
              action={
                <Button 
                  color="inherit" 
                  size="small" 
                  sx={{ 
                    border: '1px solid rgba(255,255,255,0.5)', 
                    borderRadius: 2,
                    textTransform: 'none',
                    px: 2,
                    fontWeight: 600,
                    '&:hover': { bgcolor: 'rgba(255,255,255,0.15)', borderColor: 'white' }
                  }}
                  onClick={() => navigate(`/leaves?linkedAttendanceId=${absence.id}&date=${encodeURIComponent(absence.date)}`)}
                >
                  Submit Leave
                </Button>
              }
            >
              <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                Unexcused Absence Detected on {new Date(absence.date).toLocaleDateString()}
              </Typography>
              <Typography variant="body2">
                Please submit a leave request by {new Date(absence.deadlineForLeaveRequest).toLocaleString()} to avoid salary deduction.
              </Typography>
            </Alert>
          ))}
        </Box>
      )}

      {user?.role === 'Admin' && (
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', lg: 'repeat(4, 1fr)' }, gap: { xs: 2, sm: 3 }, mb: { xs: 4, sm: 6 } }}>
          {statCards.map((stat, idx) => (
            <Paper key={idx} elevation={0} sx={{ 
              p: { xs: 2.5, sm: 3 }, 
              borderRadius: '20px',
              border: `1px solid ${stat.borderColor}`,
              bgcolor: '#FFFFFF',
              position: 'relative',
              overflow: 'hidden',
              transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
              '&:hover': { 
                boxShadow: '0 12px 24px -10px rgba(0,0,0,0.08)',
                transform: 'translateY(-2px)',
                borderColor: 'transparent'
              }
            }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 3 }}>
                <Box sx={{ 
                  p: 1.5, 
                  borderRadius: '14px', 
                  bgcolor: stat.bgColor,
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'center'
                }}>
                  {stat.icon}
                </Box>
                <ArrowUpRight size={20} color="#94A3B8" />
              </Box>
              <Typography variant="h3" sx={{ fontWeight: 'bold', color: '#0F172A', mb: 0.5, fontFamily: 'system-ui, -apple-system, sans-serif', fontSize: { xs: '2rem', sm: '3rem' } }}>
                {stat.value}
              </Typography>
              <Typography variant="body2" sx={{ color: '#64748B', fontWeight: 400 }}>
                {stat.title}
              </Typography>
            </Paper>
          ))}
        </Box>
      )}

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', lg: user?.role === 'Admin' ? '2fr 1fr' : '1fr' }, gap: 4 }}>
        {user?.role === 'Admin' && (
          <Box sx={{ minWidth: 0 }}>
            <Paper elevation={0} sx={{ 
              p: 4, 
              borderRadius: '24px', 
              border: '1px solid #E2E8F0',
              bgcolor: '#FFFFFF',
              height: '100%',
              minHeight: '450px'
            }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Box>
                  <Typography variant="h6" sx={{ fontWeight: 400, color: '#0F172A' }}>
                    Attendance Analytics
                  </Typography>
                  <Typography variant="body2" sx={{ color: '#64748B' }}>
                    Past 7 days performance
                  </Typography>
                </Box>
                <Chip icon={<CalendarDays size={16} />} label="Last 7 Days" size="small" sx={{ bgcolor: '#F1F5F9', color: '#475569', fontWeight: 400, borderRadius: '8px' }} />
              </Box>
              
              <Box sx={{ height: 320, width: '100%', mt: 2 }}>
                <ResponsiveContainer>
                  <AreaChart data={trend} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                    <defs>
                      <linearGradient id="colorPresent" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="#10B981" stopOpacity={0.2}/>
                        <stop offset="95%" stopColor="#10B981" stopOpacity={0}/>
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#F1F5F9" />
                    <XAxis 
                      dataKey="date" 
                      axisLine={false} 
                      tickLine={false} 
                      tick={{ fill: '#94A3B8', fontSize: 13, fontWeight: 400 }} 
                      dy={10} 
                    />
                    <YAxis 
                      axisLine={false} 
                      tickLine={false} 
                      tick={{ fill: '#94A3B8', fontSize: 13, fontWeight: 400 }} 
                    />
                    <Tooltip 
                      contentStyle={{ borderRadius: '12px', border: '1px solid #E2E8F0', boxShadow: '0 10px 15px -3px rgba(0,0,0,0.1)', padding: '12px 16px' }}
                      cursor={{ stroke: '#94A3B8', strokeWidth: 1, strokeDasharray: '4 4' }}
                    />
                    <Area type="monotone" dataKey="presentCount" name="Present" stroke="#10B981" strokeWidth={3} fillOpacity={1} fill="url(#colorPresent)" />
                    <Area type="monotone" dataKey="lateCount" name="Late" stroke="#F59E0B" strokeWidth={2} fill="none" strokeDasharray="5 5" />
                  </AreaChart>
                </ResponsiveContainer>
              </Box>
            </Paper>
          </Box>
        )}

        <Box sx={{ minWidth: 0 }}>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 4, height: '100%' }}>
            {user?.role === 'Admin' && (
              <Paper elevation={0} sx={{ 
                p: 3, 
                borderRadius: '24px', 
                border: '1px solid #E2E8F0',
                bgcolor: '#FFFFFF',
                flexGrow: 1
              }}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Typography variant="h6" sx={{ fontWeight: 400, color: '#0F172A' }}>
                      Live Activity
                    </Typography>
                    <IconButton onClick={refreshLiveActivity} size="small" disabled={refreshing}>
                      <RefreshCw size={16} color={refreshing ? '#94A3B8' : '#64748B'} className={refreshing ? 'spin' : ''} />
                    </IconButton>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, bgcolor: 'rgba(16, 185, 129, 0.1)', px: 1.5, py: 0.5, borderRadius: 4 }}>
                    <Box sx={{ width: 6, height: 6, borderRadius: '50%', bgcolor: '#10B981', animation: 'pulse 2s infinite' }} />
                    <Typography variant="caption" sx={{ color: '#059669', fontWeight: 400 }}>Live</Typography>
                  </Box>
                </Box>
                
                {liveList.length === 0 ? (
                  <Box sx={{ textAlign: 'center', py: 4 }}>
                    <Typography color="text.secondary" variant="body2">No one is currently clocked in.</Typography>
                  </Box>
                ) : (
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                    {liveList.slice(0, 5).map((emp) => (
                      <Box key={emp.employeeId} sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <Avatar sx={{ bgcolor: 'rgba(99, 102, 241, 0.1)', color: '#4F46E5', width: 40, height: 40, fontSize: '1rem', fontWeight: 400 }}>
                          {emp.employeeName.charAt(0)}
                        </Avatar>
                        <Box sx={{ flex: 1, minWidth: 0 }}>
                          <Typography variant="subtitle2" sx={{ fontWeight: 400, color: '#1E293B', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                            {emp.employeeName}
                          </Typography>
                          <Typography variant="body2" sx={{ color: '#64748B', fontSize: '0.8rem', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                            {emp.positionName}
                          </Typography>
                        </Box>
                        <Typography variant="caption" sx={{ color: '#0F172A', fontWeight: 400, bgcolor: '#F8FAFC', px: 1.5, py: 0.5, borderRadius: '6px' }}>
                          {formatTime12Hour(emp.checkInTime)}
                        </Typography>
                      </Box>
                    ))}
                    {liveList.length > 5 && (
                      <Button variant="text" onClick={() => setOpenLiveDialog(true)} sx={{ color: '#6366F1', textTransform: 'none', fontWeight: 400 }}>
                        View all {liveList.length} employees
                      </Button>
                    )}
                  </Box>
                )}
              </Paper>
            )}

            <Paper elevation={0} sx={{ 
              p: 3, 
              borderRadius: '24px', 
              border: '1px solid #E2E8F0',
              bgcolor: '#FFFFFF',
              flexGrow: user?.role !== 'Admin' ? 1 : 0
            }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 3 }}>
                <Box sx={{ p: 1, bgcolor: '#FEF2F2', borderRadius: '10px' }}>
                  <Megaphone size={20} color="#EF4444" />
                </Box>
                <Typography variant="h6" sx={{ fontWeight: 400, color: '#0F172A' }}>
                  Announcements
                </Typography>
              </Box>

              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                {announcements.length === 0 ? (
                  <Typography color="text.secondary" variant="body2">No active announcements.</Typography>
                ) : (
                  announcements.map((ann, idx) => (
                    <Box key={ann.id}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                        <Chip 
                          label={ann.type} 
                          size="small" 
                          sx={{ 
                            bgcolor: ann.type === 'Holiday' ? '#F0FDF4' : '#EFF6FF', 
                            color: ann.type === 'Holiday' ? '#16A34A' : '#2563EB',
                            fontWeight: 400,
                            height: 24,
                            fontSize: '0.75rem'
                          }} 
                        />
                        <Typography variant="caption" sx={{ color: '#94A3B8', fontWeight: 400 }}>
                          {new Date(ann.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}
                        </Typography>
                      </Box>
                      <Typography variant="subtitle2" sx={{ fontWeight: 400, color: '#1E293B', mb: 0.5 }}>
                        {ann.title}
                      </Typography>
                      {user?.role === 'Admin' && (
                        <Typography variant="caption" sx={{ color: '#64748B', display: 'block', mb: 0.5 }}>
                          {ann.targetEmployee ? `Sent to: ${ann.targetEmployee.firstName} ${ann.targetEmployee.lastName}` : 'Sent to: All Employees'}
                        </Typography>
                      )}
                      <Typography variant="body2" sx={{ color: '#64748B', lineHeight: 1.5 }}>
                        {ann.content}
                      </Typography>
                      {idx < announcements.length - 1 && <Divider sx={{ mt: 3, borderColor: '#F1F5F9' }} />}
                    </Box>
                  ))
                )}
              </Box>
            </Paper>
          </Box>
        </Box>
      </Box>
      
      <Dialog open={openLiveDialog} onClose={() => setOpenLiveDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontWeight: 'normal' }}>
          Live Activity ({liveList.length})
          <IconButton onClick={() => setOpenLiveDialog(false)} size="small"><X size={20} /></IconButton>
        </DialogTitle>
        <DialogContent dividers>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            {liveList.map((emp) => (
              <Box key={emp.employeeId} sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Avatar sx={{ bgcolor: 'rgba(99, 102, 241, 0.1)', color: '#4F46E5', width: 40, height: 40, fontSize: '1rem', fontWeight: 400 }}>
                  {emp.employeeName.charAt(0)}
                </Avatar>
                <Box sx={{ flex: 1, minWidth: 0 }}>
                  <Typography variant="subtitle2" sx={{ fontWeight: 400, color: '#1E293B', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                    {emp.employeeName}
                  </Typography>
                  <Typography variant="body2" sx={{ color: '#64748B', fontSize: '0.8rem', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                    {emp.positionName}
                  </Typography>
                </Box>
                <Typography variant="caption" sx={{ color: '#0F172A', fontWeight: 400, bgcolor: '#F8FAFC', px: 1.5, py: 0.5, borderRadius: '6px' }}>
                  {formatTime12Hour(emp.checkInTime)}
                </Typography>
              </Box>
            ))}
          </Box>
        </DialogContent>
      </Dialog>
      
      <style>
        {`
          @keyframes pulse {
            0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.4); }
            70% { transform: scale(1); box-shadow: 0 0 0 6px rgba(16, 185, 129, 0); }
            100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); }
          }
          @keyframes spin {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
          }
          .spin {
            animation: spin 1s linear infinite;
          }
        `}
      </style>
    </Box>
  );
}
