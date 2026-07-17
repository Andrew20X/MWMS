import { createTheme } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#48657B', // Slate blue from the top bar
    },
    secondary: {
      main: '#2E7D32', // Green from success message
    },
    background: {
      default: '#F5F7FA', // Light clean background
      paper: '#FFFFFF', // Pure white paper
    },
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h4: {
      fontWeight: 700,
    },
    h5: {
      fontWeight: 600,
    },
    h6: {
      fontWeight: 600,
    },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          borderRadius: '8px',
          fontWeight: 600,
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
          borderRadius: '12px',
        },
      },
    },
    MuiCssBaseline: {
      styleOverrides: {
        'input::-ms-reveal, input::-ms-clear': {
          display: 'none !important',
        },
      },
    },
  },
});

export default theme;
