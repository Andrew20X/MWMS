import { useState, useEffect } from 'react';
import { Box, Typography, Paper, CircularProgress, Alert, Card, CardContent } from '@mui/material';
import { Clock, AlertTriangle, Target, TrendingUp } from 'lucide-react';
import axios from 'axios';
import { useAuth } from '../contexts/AuthContext';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, ReferenceLine } from 'recharts';

export default function MyInsights() {
  const { user } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  // Data states
  const [totalWorkedHours, setTotalWorkedHours] = useState(0);
  const [targetHours, setTargetHours] = useState(0);
  const [totalLateMinutes, setTotalLateMinutes] = useState(0);
  const [totalOvertimeMinutes, setTotalOvertimeMinutes] = useState(0);
  const [chartData, setChartData] = useState<any[]>([]);

  useEffect(() => {
    const fetchAttendance = async () => {
      try {
        const res = await axios.get('http://localhost:5222/api/attendance/me', {
          headers: { Authorization: `Bearer ${user?.token}` }
        });
        
        const attendances = res.data;
        processInsights(attendances);
      } catch (err: any) {
        setError('Failed to load attendance data.');
      } finally {
        setLoading(false);
      }
    };

    if (user) {
      fetchAttendance();
    }
  }, [user]);

  const processInsights = (attendances: any[]) => {
    const now = new Date();
    const currentYear = now.getFullYear();
    const currentMonth = now.getMonth();

    // Calculate total working days in the current month (Monday to Friday)
    const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
    let workingDays = 0;
    for (let i = 1; i <= daysInMonth; i++) {
      const day = new Date(currentYear, currentMonth, i).getDay();
      if (day !== 0 && day !== 6) { // 0 = Sunday, 6 = Saturday
        workingDays++;
      }
    }
    
    // Target is 8 hours per working day
    const calculatedTargetHours = workingDays * 8;
    setTargetHours(calculatedTargetHours);

    // Filter attendances for current month
    const currentMonthAttendances = attendances.filter(a => {
      const d = new Date(a.date);
      return d.getFullYear() === currentYear && d.getMonth() === currentMonth;
    });

    let worked = 0;
    let late = 0;
    let overtime = 0;
    
    // For chart: group by week or just daily. Let's do daily for the current month
    const dailyData = [];
    for (let i = 1; i <= daysInMonth; i++) {
      const dateStr = `${currentYear}-${String(currentMonth + 1).padStart(2, '0')}-${String(i).padStart(2, '0')}`;
      const record = currentMonthAttendances.find(a => a.date.startsWith(dateStr));
      
      const dayWorked = record ? record.workedHours : 0;
      dailyData.push({
        date: String(i),
        worked: parseFloat(dayWorked.toFixed(1))
      });
      
      if (record) {
        worked += record.workedHours;
        late += record.lateMinutes || 0;
        overtime += record.overtimeMinutes || 0;
      }
    }

    setTotalWorkedHours(parseFloat(worked.toFixed(1)));
    setTotalLateMinutes(late);
    setTotalOvertimeMinutes(overtime);
    setChartData(dailyData);
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '60vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  const progressPercentage = Math.min(100, Math.round((totalWorkedHours / targetHours) * 100)) || 0;

  return (
    <Box sx={{ maxWidth: '1200px', margin: '0 auto', pb: 8 }}>
      <Typography variant="h4" sx={{ fontWeight: 'normal', color: '#1E293B', mb: 1 }}>
        My Insights
      </Typography>
      <Typography variant="body1" sx={{ color: '#64748B', mb: 4 }}>
        Current Month ({new Date().toLocaleString('default', { month: 'long', year: 'numeric' })})
      </Typography>

      {totalLateMinutes > 120 && (
        <Alert severity="warning" icon={<AlertTriangle />} sx={{ mb: 4, borderRadius: 2 }}>
          <span>Warning:</span> You have accumulated {totalLateMinutes} minutes of lateness this month. Please try to arrive on time to avoid deductions.
        </Alert>
      )}

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(4, 1fr)' }, gap: 3, mb: 4 }}>
        {/* Total Worked Hours */}
        <Box>
          <Card sx={{ borderRadius: 3, boxShadow: '0 4px 12px rgba(0,0,0,0.05)' }}>
            <CardContent sx={{ p: 3 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                <Box sx={{ p: 1.5, borderRadius: 2, bgcolor: 'rgba(99, 102, 241, 0.1)' }}>
                  <Clock color="#4F46E5" size={24} />
                </Box>
              </Box>
              <Typography variant="h4" sx={{ fontWeight: 400, color: '#1E293B', mb: 0.5 }}>
                {totalWorkedHours} <Typography component="span" variant="h6" color="text.secondary">hrs</Typography>
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 400 }}>
                Total Worked Hours
              </Typography>
            </CardContent>
          </Card>
        </Box>

        {/* Target Hours */}
        <Box>
          <Card sx={{ borderRadius: 3, boxShadow: '0 4px 12px rgba(0,0,0,0.05)' }}>
            <CardContent sx={{ p: 3 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                <Box sx={{ p: 1.5, borderRadius: 2, bgcolor: 'rgba(16, 185, 129, 0.1)' }}>
                  <Target color="#10B981" size={24} />
                </Box>
              </Box>
              <Typography variant="h4" sx={{ fontWeight: 400, color: '#1E293B', mb: 0.5 }}>
                {targetHours} <Typography component="span" variant="h6" color="text.secondary">hrs</Typography>
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 400 }}>
                Target Hours
              </Typography>
            </CardContent>
          </Card>
        </Box>

        {/* Total Late Minutes */}
        <Box>
          <Card sx={{ borderRadius: 3, boxShadow: '0 4px 12px rgba(0,0,0,0.05)' }}>
            <CardContent sx={{ p: 3 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                <Box sx={{ p: 1.5, borderRadius: 2, bgcolor: 'rgba(245, 158, 11, 0.1)' }}>
                  <AlertTriangle color="#F59E0B" size={24} />
                </Box>
              </Box>
              <Typography variant="h4" sx={{ fontWeight: 400, color: '#1E293B', mb: 0.5 }}>
                {totalLateMinutes} <Typography component="span" variant="h6" color="text.secondary">min</Typography>
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 400 }}>
                Total Late Time
              </Typography>
            </CardContent>
          </Card>
        </Box>

        {/* Overtime Minutes */}
        <Box>
          <Card sx={{ borderRadius: 3, boxShadow: '0 4px 12px rgba(0,0,0,0.05)' }}>
            <CardContent sx={{ p: 3 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                <Box sx={{ p: 1.5, borderRadius: 2, bgcolor: 'rgba(56, 189, 248, 0.1)' }}>
                  <TrendingUp color="#0EA5E9" size={24} />
                </Box>
              </Box>
              <Typography variant="h4" sx={{ fontWeight: 400, color: '#1E293B', mb: 0.5 }}>
                {Math.floor(totalOvertimeMinutes / 60)}<Typography component="span" variant="subtitle1" color="text.secondary">h</Typography> {totalOvertimeMinutes % 60}<Typography component="span" variant="subtitle1" color="text.secondary">m</Typography>
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 400 }}>
                Total Overtime
              </Typography>
            </CardContent>
          </Card>
        </Box>
      </Box>

      {/* Progress Bar */}
      <Paper sx={{ p: 4, borderRadius: 4, mb: 4, border: '1px solid #E2E8F0', boxShadow: 'none' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h6" sx={{ fontWeight: 400, color: '#1E293B' }}>
            Monthly Progress
          </Typography>
          <Typography variant="h6" sx={{ fontWeight: 400, color: '#4F46E5' }}>
            {progressPercentage}%
          </Typography>
        </Box>
        <Box sx={{ width: '100%', height: 12, bgcolor: '#F1F5F9', borderRadius: 6, overflow: 'hidden' }}>
          <Box sx={{ 
            width: `${progressPercentage}%`, 
            height: '100%', 
            bgcolor: progressPercentage >= 100 ? '#10B981' : '#4F46E5',
            transition: 'width 1s ease-in-out'
          }} />
        </Box>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5 }}>
          {totalWorkedHours} / {targetHours} hours completed
        </Typography>
      </Paper>

      {/* Chart */}
      <Paper sx={{ p: 4, borderRadius: 4, border: '1px solid #E2E8F0', boxShadow: 'none' }}>
        <Typography variant="h6" sx={{ fontWeight: 400, color: '#1E293B', mb: 4 }}>
          Daily Hours Worked
        </Typography>
        <Box sx={{ height: 300, width: '100%' }}>
          <ResponsiveContainer>
            <BarChart data={chartData} margin={{ top: 5, right: 0, left: -20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#F1F5F9" />
              <XAxis 
                dataKey="date" 
                axisLine={false} 
                tickLine={false} 
                tick={{ fill: '#94A3B8', fontSize: 12 }} 
                dy={10} 
              />
              <YAxis 
                axisLine={false} 
                tickLine={false} 
                tick={{ fill: '#94A3B8', fontSize: 12 }} 
              />
              <Tooltip 
                cursor={{ fill: '#F8FAFC' }}
                contentStyle={{ borderRadius: '12px', border: '1px solid #E2E8F0', boxShadow: '0 10px 15px -3px rgba(0,0,0,0.1)' }}
              />
              <ReferenceLine y={8} stroke="#10B981" strokeDasharray="3 3" label={{ position: 'top', value: 'Target (8h)', fill: '#10B981', fontSize: 12 }} />
              <Bar dataKey="worked" name="Hours Worked" fill="#4F46E5" radius={[4, 4, 0, 0]} maxBarSize={40} />
            </BarChart>
          </ResponsiveContainer>
        </Box>
      </Paper>
    </Box>
  );
}
