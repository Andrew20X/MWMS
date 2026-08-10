import { HashRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, CssBaseline } from '@mui/material';
import theme from './theme';
import Dashboard from './pages/Dashboard';
import ImportTimesheet from './pages/ImportTimesheet';
import Login from './pages/Login';
import Employees from './pages/Employees';
import Leaves from './pages/Leaves';
import ProtectedRoute from './components/ProtectedRoute';
import MainLayout from './components/MainLayout';
import { AuthProvider } from './contexts/AuthContext';
import Announcements from './pages/Announcements';
import Corrections from './pages/Corrections';
import MyTimesheet from './pages/MyTimesheet';
import Reports from './pages/Reports';
import Overtime from './pages/Overtime';
import MyInsights from './pages/MyInsights';
import Deductions from './pages/Deductions';

import ForgotPassword from './pages/ForgotPassword';
import ResetPassword from './pages/ResetPassword';
import ForceChangePassword from './pages/ForceChangePassword';
import SystemLogs from './pages/SystemLogs';


function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <Router>
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/reset-password" element={<ResetPassword />} />

            <Route element={<ProtectedRoute />}>
              <Route path="/force-change-password" element={<ForceChangePassword />} />
              <Route element={<MainLayout />}>
                <Route path="/" element={<Dashboard />} />
                <Route path="/timesheet" element={<ImportTimesheet />} />
                <Route path="/employees" element={<Employees />} />
                <Route path="/leaves" element={<Leaves />} />
                <Route path="/announcements" element={<Announcements />} />
                <Route path="/corrections" element={<Corrections />} />
                <Route path="/overtime" element={<Overtime />} />
                <Route path="/my-timesheet" element={<MyTimesheet />} />
                <Route path="/my-insights" element={<MyInsights />} />
                <Route path="/reports" element={<Reports />} />
                <Route path="/deductions" element={<Deductions />} />
                <Route path="/system-logs" element={<SystemLogs />} />
              </Route>
            </Route>

            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Router>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;



