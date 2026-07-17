import { useState, useEffect } from 'react';
import { Box, Paper, Typography, TextField, Button, Alert, CircularProgress, InputAdornment, IconButton } from '@mui/material';
import { useNavigate, useLocation } from 'react-router-dom';
import axios from 'axios';
import { KeyRound, Eye, EyeOff } from 'lucide-react';

export default function ResetPassword() {
  const [email, setEmail] = useState('');
  const [token, setToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    // If we passed email and token via query params, we could auto-fill them.
    const searchParams = new URLSearchParams(location.search);
    const emailParam = searchParams.get('email');
    const tokenParam = searchParams.get('token');
    
    if (emailParam) setEmail(emailParam);
    if (tokenParam) setToken(tokenParam);
  }, [location]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }
    if (newPassword.length < 6) {
      setError("Password must be at least 6 characters.");
      return;
    }
    
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      await axios.post('http://localhost:5222/api/Auth/reset-password', {
        email,
        token,
        newPassword
      });

      setSuccess("Your password has been successfully reset. You can now login.");
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reset password. Please check your token.');
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
      backgroundImage: 'radial-gradient(circle at 50% 50%, rgba(46, 125, 50, 0.05) 0%, rgba(245, 247, 250, 1) 100%)'
    }}>
      <Paper elevation={12} sx={{ p: 5, width: '100%', maxWidth: '450px', borderRadius: '16px', bgcolor: 'rgba(255, 255, 255, 0.95)', backdropFilter: 'blur(10px)', border: '1px solid rgba(0, 0, 0, 0.05)' }}>
        <Box sx={{ textAlign: 'center', mb: 4 }}>
          <Box sx={{ mx: 'auto', width: 60, height: 60, borderRadius: '50%', bgcolor: 'rgba(46, 125, 50, 0.1)', display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
            <KeyRound size={32} color="#2E7D32" />
          </Box>
          <Typography variant="h5" sx={{ fontWeight: 'bold' }}>Reset Password</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            Enter your reset token and your new password.
          </Typography>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
        {success && <Alert severity="success" sx={{ mb: 3 }}>{success}</Alert>}

        <form onSubmit={handleSubmit}>
          <TextField
            fullWidth
            label="Reset Token"
            variant="outlined"
            margin="normal"
            value={token}
            onChange={(e) => setToken(e.target.value)}
            required
            autoComplete="off"
            sx={{ mb: 2 }}
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
            disabled={loading || !!success}
            sx={{ 
              py: 1.5, 
              fontSize: '1rem', 
              fontWeight: 600,
              textTransform: 'none',
              borderRadius: '8px',
              mb: 3,
              boxShadow: '0 4px 14px 0 rgba(46, 125, 50, 0.39)'
            }}
          >
            {loading ? <CircularProgress size={24} color="inherit" /> : 'Reset Password'}
          </Button>

          {!success && (
            <Button
              fullWidth
              variant="text"
              color="inherit"
              onClick={() => navigate('/login')}
            >
              Back to Login
            </Button>
          )}
        </form>
      </Paper>
    </Box>
  );
}
