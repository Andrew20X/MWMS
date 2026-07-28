import { useState } from 'react';
import { Box, Drawer, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Typography, AppBar, Toolbar, Button, IconButton } from '@mui/material';
import { LayoutDashboard, FileSpreadsheet, Users, LogOut, Menu, Calendar, KeyRound, Bell, CheckSquare, Clock, BarChart, Home, LineChart, AlertCircle } from 'lucide-react';
import { Link, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import ChangePasswordModal from './ChangePasswordModal';

const drawerWidth = 260;

export default function MainLayout() {
  const { logout, user } = useAuth();
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);
  const [passwordModalOpen, setPasswordModalOpen] = useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const isAdmin = user?.role === 'Admin';
  const isHR = user?.role === 'HR';


  const isHRAdmin = isAdmin || isHR;

  const drawerContent = (
    <>
      <Toolbar />
      <Box sx={{ overflow: 'auto', mt: 2, display: 'flex', flexDirection: 'column', height: '100%' }}>
        <List sx={{ flexGrow: 1 }}>
          <ListItem disablePadding>
            <ListItemButton component={Link} to="/" onClick={() => setMobileOpen(false)}>
              <ListItemIcon><LayoutDashboard color="#48657B" /></ListItemIcon>
              <ListItemText primary={isAdmin ? "Dashboard" : "Notice Board"} />
            </ListItemButton>
          </ListItem>

          {isHRAdmin && (
            <>
              <ListItem disablePadding>
                <ListItemButton component={Link} to="/employees" onClick={() => setMobileOpen(false)}>
                  <ListItemIcon><Users color="#48657B" /></ListItemIcon>
                  <ListItemText primary="Employees & Subordinates" />
                </ListItemButton>
              </ListItem>
              {isAdmin && (
                <ListItem disablePadding>
                  <ListItemButton component={Link} to="/timesheet" onClick={() => setMobileOpen(false)}>
                    <ListItemIcon><FileSpreadsheet color="#48657B" /></ListItemIcon>
                    <ListItemText primary="Timesheets & Excel" />
                  </ListItemButton>
                </ListItem>
              )}
              <ListItem disablePadding>
                <ListItemButton component={Link} to="/announcements" onClick={() => setMobileOpen(false)}>
                  <ListItemIcon><Bell color="#48657B" /></ListItemIcon>
                  <ListItemText primary="Announcements" />
                </ListItemButton>
              </ListItem>
              <ListItem disablePadding>
                <ListItemButton component={Link} to="/reports" onClick={() => setMobileOpen(false)}>
                  <ListItemIcon><BarChart color="#48657B" /></ListItemIcon>
                  <ListItemText primary="Reports" />
                </ListItemButton>
              </ListItem>
            </>
          )}

          {!isAdmin && (
            <>
              <ListItem disablePadding>
                <ListItemButton component={Link} to="/my-insights" onClick={() => setMobileOpen(false)}>
                  <ListItemIcon><LineChart color="#48657B" /></ListItemIcon>
                  <ListItemText primary="My Insights" />
                </ListItemButton>
              </ListItem>
              <ListItem disablePadding>
                <ListItemButton component={Link} to="/my-timesheet" onClick={() => setMobileOpen(false)}>
                  <ListItemIcon><Clock color="#48657B" /></ListItemIcon>
                  <ListItemText primary="My Timesheet" />
                </ListItemButton>
              </ListItem>
            </>
          )}

          <ListItem disablePadding>
            <ListItemButton component={Link} to="/corrections" onClick={() => setMobileOpen(false)}>
              <ListItemIcon><CheckSquare color="#48657B" /></ListItemIcon>
              <ListItemText primary="Corrections" />
            </ListItemButton>
          </ListItem>

          <ListItem disablePadding>
            <ListItemButton component={Link} to="/overtime" onClick={() => setMobileOpen(false)}>
              <ListItemIcon><Home color="#48657B" /></ListItemIcon>
              <ListItemText primary="Overtime" />
            </ListItemButton>
          </ListItem>

          <ListItem disablePadding>
            <ListItemButton component={Link} to="/leaves" onClick={() => setMobileOpen(false)}>
              <ListItemIcon><Calendar color="#48657B" /></ListItemIcon>
              <ListItemText primary="Leaves" />
            </ListItemButton>
          </ListItem>

          <ListItem disablePadding>
            <ListItemButton component={Link} to="/deductions" onClick={() => setMobileOpen(false)}>
              <ListItemIcon><AlertCircle color="#48657B" /></ListItemIcon>
              <ListItemText primary="Deductions" />
            </ListItemButton>
          </ListItem>
        </List>
      </Box>
    </>
  );

  return (
    <Box sx={{ display: 'flex' }}>
      <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1, backgroundColor: 'background.paper', boxShadow: 1 }}>
        <Toolbar sx={{ justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <IconButton
              color="inherit"
              aria-label="open drawer"
              edge="start"
              onClick={handleDrawerToggle}
              sx={{ mr: 2, display: { sm: 'none' }, color: '#48657B' }}
            >
              <Menu />
            </IconButton>
            <img src={`${import.meta.env.BASE_URL}logo.png`} alt="Measuresoft Logo" style={{ height: '45px', objectFit: 'contain' }} />
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: { xs: 2.5, sm: 2 } }}>
            <Typography variant="body2" sx={{ fontWeight: 'normal', display: { xs: 'none', sm: 'block' }, color: 'text.primary' }}>
              {user?.fullName || user?.username} ({user?.role}{user?.positionName && user?.positionName !== user?.role && user?.positionName !== 'Employee' ? ` - ${user?.positionName}` : ''})
            </Typography>

            <Button color="primary" variant="outlined" size="small" startIcon={<KeyRound size={18} />} onClick={() => setPasswordModalOpen(true)} sx={{ minWidth: { xs: 'auto', sm: '150px' }, p: { xs: '10px 14px', sm: '5px 15px' }, '& .MuiButton-startIcon': { mr: { xs: 0, sm: 1 }, ml: { xs: 0, sm: -0.5 } } }}>
              <Box component="span" sx={{ display: { xs: 'none', sm: 'inline' } }}>Change Password</Box>
            </Button>

            <Button color="error" variant="outlined" size="small" startIcon={<LogOut size={18} />} onClick={handleLogout} sx={{ minWidth: { xs: 'auto', sm: '100px' }, p: { xs: '10px 14px', sm: '5px 15px' }, '& .MuiButton-startIcon': { mr: { xs: 0, sm: 1 }, ml: { xs: 0, sm: -0.5 } } }}>
              <Box component="span" sx={{ display: { xs: 'none', sm: 'inline' } }}>Logout</Box>
            </Button>
          </Box>
        </Toolbar>
      </AppBar>

      <Box component="nav" sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}>
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{
            keepMounted: true, // Better open performance on mobile.
          }}
          sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
          }}
        >
          {drawerContent}
        </Drawer>

        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', sm: 'block' },
            '& .MuiDrawer-paper': { width: drawerWidth, boxSizing: 'border-box', borderRight: '1px solid #E2E8F0' },
          }}
          open
        >
          {drawerContent}
        </Drawer>
      </Box>

      <Box component="main" sx={{ flexGrow: 1, p: { xs: 2, sm: 3 }, backgroundColor: 'background.default', minHeight: '100vh', width: { xs: '100%', sm: `calc(100% - ${drawerWidth}px)` } }}>
        <Toolbar />
        <Outlet />
      </Box>

      <ChangePasswordModal open={passwordModalOpen} onClose={() => setPasswordModalOpen(false)} />
    </Box>
  );
}



