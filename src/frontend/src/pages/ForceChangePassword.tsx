import { useState } from 'react';
import { Box, Paper, Typography, TextField, Button, Alert, CircularProgress, InputAdornment, IconButton } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import axios from 'axios';
import { Eye, EyeOff, Lock } from 'lucide-react';

export default function ForceChangePassword() {
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { user, login } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirmPassword) {
      setError("New passwords do not match.");
      return;
    }
    if (newPassword === oldPassword) {
      setError("New password cannot be the same as the current password.");
      return;
    }
    if (newPassword.length < 6) {
      setError("Password must be at least 6 characters.");
      return;
    }
    
    setError('');
    setLoading(true);

    try {
      await axios.post('http://localhost:5222/api/Auth/change-password', {
        oldPassword,
        newPassword
      }, {
        headers: { Authorization: `Bearer ${user?.token}` }
      });

      // Update the context to reflect requiresPasswordChange = false
      if (user) {
        login({ ...user, requiresPasswordChange: false });
      }
      
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to change password. Please check your old password.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ 
      minHeight: '100vh', 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center',
      bgcolor: 'background.default',
      backgroundImage: 'radial-gradient(circle at 50% 50%, rgba(245, 158, 11, 0.05) 0%, rgba(245, 247, 250, 1) 100%)'
    }}>
      <Paper elevation={12} sx={{ p: 5, width: '100%', maxWidth: '450px', borderRadius: '16px', bgcolor: 'rgba(255, 255, 255, 0.95)', backdropFilter: 'blur(10px)', border: '1px solid rgba(0, 0, 0, 0.05)' }}>
        <Box sx={{ textAlign: 'center', mb: 4 }}>
          <Box sx={{ mx: 'auto', width: 60, height: 60, borderRadius: '50%', bgcolor: 'rgba(245, 158, 11, 0.1)', display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
            <Lock size={32} color="#F59E0B" />
          </Box>
          <Typography variant="h5" sx={{ fontWeight: 'normal' }}>Password Change Required</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            For security reasons, you must change your password before continuing to the dashboard.
          </Typography>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

        <form onSubmit={handleSubmit}>
          <TextField
            fullWidth
            label="Current Password"
            type={showPassword ? 'text' : 'password'}
            variant="outlined"
            margin="normal"
            value={oldPassword}
            onChange={(e) => setOldPassword(e.target.value)}
            required
            autoComplete="current-password"
            sx={{ 
              mb: 2,
              '& input::-ms-reveal, & input::-ms-clear': { display: 'none !important' }
            }}
            slotProps={{
              input: {
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                      {showPassword ? <Eye size={20} /> : <EyeOff size={20} />}
                    </IconButton>
                  </InputAdornment>
                ),
              }
            }}
          />
          <TextField
            fullWidth
            label="New Password"
            type={showPassword ? 'text' : 'password'}
            variant="outlined"
            margin="normal"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            required
            autoComplete="new-password"
            sx={{ 
              mb: 2,
              '& input::-ms-reveal, & input::-ms-clear': { display: 'none !important' }
            }}
            slotProps={{
              input: {
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                      {showPassword ? <Eye size={20} /> : <EyeOff size={20} />}
                    </IconButton>
                  </InputAdornment>
                ),
              }
            }}
          />
          <TextField
            fullWidth
            label="Confirm New Password"
            type={showPassword ? 'text' : 'password'}
            variant="outlined"
            margin="normal"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            autoComplete="new-password"
            sx={{ 
              mb: 4,
              '& input::-ms-reveal, & input::-ms-clear': { display: 'none !important' }
            }}
            slotProps={{
              input: {
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                      {showPassword ? <Eye size={20} /> : <EyeOff size={20} />}
                    </IconButton>
                  </InputAdornment>
                ),
              }
            }}
          />
          <Button
            fullWidth
            type="submit"
            variant="contained"
            color="primary"
            size="large"
            disabled={loading}
            sx={{ 
              py: 1.5, 
              fontSize: '1rem', 
              fontWeight: 400,
              textTransform: 'none',
              borderRadius: '8px',
              boxShadow: '0 4px 14px 0 rgba(46, 125, 50, 0.39)'
            }}
          >
            {loading ? <CircularProgress size={24} color="inherit" /> : 'Update Password & Continue'}
          </Button>
        </form>
      </Paper>
    </Box>
  );
}
