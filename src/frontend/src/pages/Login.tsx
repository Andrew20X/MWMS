import { useState } from 'react';
import { Box, Paper, Typography, TextField, Button, Alert, CircularProgress, InputAdornment, IconButton } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import axios from 'axios';
import { Lock, Eye, EyeOff } from 'lucide-react';

export default function Login() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      // NOTE: Ensure proxy or correct port is used
      const response = await axios.post('https://andrew20x-001-site1.itempurl.com/api/Auth/login', {
        username,
        password
      });

      const { token, username: resUsername, fullName, role, employeeId, requiresPasswordChange, positionName } = response.data;
      
      login({ token, username: resUsername, fullName, role, employeeId, requiresPasswordChange, positionName });
      
      if (requiresPasswordChange) {
        navigate('/force-change-password');
      } else if (role === 'Admin') {
        navigate('/');
      } else {
        navigate('/'); // Now everyone can go to dashboard
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to login. Please check your credentials.');
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
      <Paper elevation={12} sx={{ p: 5, width: '100%', maxWidth: '400px', borderRadius: '16px', bgcolor: 'rgba(255, 255, 255, 0.95)', backdropFilter: 'blur(10px)', border: '1px solid rgba(0, 0, 0, 0.05)' }}>
        <Box sx={{ textAlign: 'center', mb: 4 }}>
          <img src="/logo.png" alt="Measuresoft Logo" style={{ height: '60px', objectFit: 'contain', marginBottom: '16px' }} />
          <Typography variant="h5" sx={{ fontWeight: 'normal' }}>Welcome Back</Typography>
          <Typography variant="body2" color="text.secondary">Sign in to your account</Typography>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

        <form onSubmit={handleLogin}>
          <TextField
            fullWidth
            label="Username"
            variant="outlined"
            margin="normal"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
            sx={{ mb: 2 }}
          />
          <TextField
            fullWidth
            label="Password"
            type={showPassword ? 'text' : 'password'}
            variant="outlined"
            margin="normal"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            sx={{ mb: 1 }}
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
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 3 }}>
            <Typography 
              variant="body2" 
              color="primary" 
              sx={{ cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }}
              onClick={() => navigate('/forgot-password')}
            >
              Forgot Password?
            </Typography>
          </Box>
          <Button
            fullWidth
            type="submit"
            variant="contained"
            color="primary"
            size="large"
            disabled={loading}
            startIcon={loading ? <CircularProgress size={20} color="inherit" /> : <Lock size={20} />}
            sx={{ py: 1.5, fontWeight: 'normal' }}
          >
            {loading ? 'Signing in...' : 'Sign In'}
          </Button>
        </form>
      </Paper>
    </Box>
  );
}




