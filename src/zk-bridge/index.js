const ZKLib = require('node-zklib');
const axios = require('axios');

// Configure the connection to the ZKTeco device
const DEVICE_IP = '10.10.100.102';
const DEVICE_PORT = 4370;
const DEVICE_TIMEOUT = 10000;
const DEVICE_INPORT = 4000;

// API endpoint for our Measuresoft Backend
const API_URL = 'http://localhost:5222/api/AttendanceEngine/process';

async function syncAttendances() {
    console.log(`[zk-bridge] Connecting to ZKTeco device at ${DEVICE_IP}:${DEVICE_PORT}...`);
    
    let zkInstance = new ZKLib(DEVICE_IP, DEVICE_PORT, DEVICE_TIMEOUT, DEVICE_INPORT);

    try {
        // Connect to the device
        await zkInstance.createSocket();
        console.log('[zk-bridge] Connected to ZKTeco device successfully.');

        // Get all attendance logs
        console.log('[zk-bridge] Fetching attendance logs...');
        const logs = await zkInstance.getAttendances();
        
        if (!logs || !logs.data || logs.data.length === 0) {
            console.log('[zk-bridge] No attendance logs found on the device.');
            return;
        }

        console.log(`[zk-bridge] Retrieved ${logs.data.length} logs. Processing and sending to API...`);

        // Transform the logs to match our API DTO (RawPunchDto)
        // ZKLib typically returns: { deviceUserId: '1', recordTime: '2026-07-08T09:00:00.000Z' }
        const payload = logs.data.map(log => ({
            employeeId: parseInt(log.deviceUserId), // Mapping device ID to Employee ID
            punchTime: log.recordTime,
            deviceId: DEVICE_IP
        }));

        // Send to Measuresoft API
        const response = await axios.post(API_URL, payload);
        
        console.log('[zk-bridge] Sync completed successfully!');
        console.log('[zk-bridge] API Response:', response.data);

        // Optional: clear logs from the machine after successful sync
        // await zkInstance.clearAttendanceLog();

    } catch (e) {
        console.error('[zk-bridge] Error during sync:', e.message || e);
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
syncAttendances();
