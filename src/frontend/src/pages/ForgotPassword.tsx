import { useState } from 'react';
import { Box, Paper, Typography, TextField, Button, Alert, CircularProgress } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { Mail, ArrowLeft } from 'lucide-react';


export default function ForgotPassword() {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      const response = await axios.post('http://localhost:5222/api/Auth/forgot-password', {
        username,
        email
      });
      setSuccess(response.data.message || 'Password reset link has been sent to your email.');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send reset link.');
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
          <Box sx={{ mx: 'auto', width: 60, height: 60, borderRadius: '50%', bgcolor: 'rgba(46, 125, 50, 0.1)', display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
            <Mail size={32} color="#2E7D32" />
          </Box>
          <Typography variant="h5" sx={{ fontWeight: 'normal' }}>Forgot Password</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            Enter your username and email address and we'll send you a link to reset your password.
          </Typography>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
        {success && <Alert severity="success" sx={{ mb: 3 }}>{success}</Alert>}

        <form onSubmit={handleSubmit}>
          <TextField
            fullWidth
            label="Username"
            variant="outlined"
            margin="normal"
            value={username}
            onChange={(e) => {
              setUsername(e.target.value);
              setSuccess('');
              setError('');
            }}
            required
            sx={{ mb: 2 }}
          />
          <TextField
            fullWidth
            label="Email Address"
            type="email"
            variant="outlined"
            margin="normal"
            value={email}
            onChange={(e) => {
              setEmail(e.target.value);
              setSuccess('');
              setError('');
            }}
            required
            sx={{ mb: 3 }}
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
              mb: 3,
              boxShadow: '0 4px 14px 0 rgba(46, 125, 50, 0.39)'
            }}
          >
            {loading ? <CircularProgress size={24} color="inherit" /> : 'Send Reset Link'}
          </Button>

          <Button
            fullWidth
            variant="text"
            color="secondary"
            onClick={() => navigate('/reset-password')}
            sx={{ mb: 2 }}
          >
            Have a reset token? Go to Reset Password
          </Button>

          <Button
            fullWidth
            variant="text"
            color="inherit"
            startIcon={<ArrowLeft size={18} />}
            onClick={() => navigate('/login')}
          >
            Back to Login
          </Button>
        </form>
      </Paper>
    </Box>
  );
}
