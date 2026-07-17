const ZKLib = require('node-zklib');
const axios = require('axios');

// Configure the connection to the ZKTeco device
const DEVICE_IP = '10.10.100.102';
const DEVICE_PORT = 4370;
const DEVICE_TIMEOUT = 10000;
const DEVICE_INPORT = 4000;

// API endpoint for our Measuresoft Backend
const API_URL = 'http://localhost:5222/api/Employees/sync';

async function syncUsers() {
    console.log(`[zk-bridge] Connecting to ZKTeco device at ${DEVICE_IP}:${DEVICE_PORT} to sync users...`);
    
    let zkInstance = new ZKLib(DEVICE_IP, DEVICE_PORT, DEVICE_TIMEOUT, DEVICE_INPORT);

    try {
        // Connect to the device
        await zkInstance.createSocket();
        console.log('[zk-bridge] Connected to ZKTeco device successfully.');

        // Get all users
        console.log('[zk-bridge] Fetching users from device...');
        const users = await zkInstance.getUsers();
        
        if (!users || !users.data || users.data.length === 0) {
            console.log('[zk-bridge] No users found on the device.');
            return;
        }

        console.log(`[zk-bridge] Retrieved ${users.data.length} users. Processing and sending to API...`);

        const payload = users.data.map(user => ({
            deviceUserId: parseInt(user.userId),
            name: user.name,
            role: user.role?.toString(),
            deviceId: DEVICE_IP
        }));

        // Send to Measuresoft API
        const response = await axios.post(API_URL, payload);
        
        console.log('[zk-bridge] User sync completed successfully!');
        console.log('[zk-bridge] API Response:', response.data);

    } catch (e) {
        console.error('[zk-bridge] Error during user sync:', e.response?.data || e.message || e);
    } finally {
        // Disconnect
        try {
            await zkInstance.disconnect();
            console.log('[zk-bridge] Disconnected from device.');
        } catch (e) {
            // Ignore disconnect errors
        }
    }
}

// Run the sync
syncUsers();
