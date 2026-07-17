const axios = require('axios');

// API endpoint for our Measuresoft Backend
const API_URL = 'http://localhost:5222/api/AttendanceEngine/process';

async function runMockTest() {
    console.log('[zk-bridge-mock] Starting ZKTeco Mock Test...');
    
    // We are mocking what the ZKLib normally returns from a real machine.
    // Let's pretend employee ID "1" punched in at 8:30 AM and punched out at 5:00 PM today.
    // And employee ID "2" punched in late at 9:15 AM and punched out at 5:00 PM today.
    
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    const datePrefix = `${year}-${month}-${day}`;

    const mockZkLogs = {
        data: [
            // Employee 1 (On Time)
            { deviceUserId: '1', recordTime: `${datePrefix}T08:30:00.000Z` },
            { deviceUserId: '1', recordTime: `${datePrefix}T17:00:00.000Z` },
            
            // Employee 2 (Late)
            { deviceUserId: '2', recordTime: `${datePrefix}T09:15:00.000Z` },
            { deviceUserId: '2', recordTime: `${datePrefix}T17:00:00.000Z` },

            // Employee 3 (Only Check-in)
            { deviceUserId: '3', recordTime: `${datePrefix}T08:45:00.000Z` }
        ]
    };

    console.log(`[zk-bridge-mock] Generated ${mockZkLogs.data.length} fake logs from "Machine".`);

    try {
        // Transform the logs just like the real script
        const payload = mockZkLogs.data.map(log => ({
            employeeId: parseInt(log.deviceUserId),
            punchTime: log.recordTime,
            deviceId: '10.10.100.102' // Fake IP
        }));

        console.log('[zk-bridge-mock] Sending payload to API...', JSON.stringify(payload, null, 2));

        // Send to Measuresoft API
        const response = await axios.post(API_URL, payload);
        
        console.log('[zk-bridge-mock] Success! API Response:', response.data);
        console.log('[zk-bridge-mock] Go check the Dashboard and Timesheet pages to see the new attendance records!');

    } catch (e) {
        console.error('[zk-bridge-mock] Error during sync:', e.response?.data || e.message || e);
    }
}

// Run the mock test
runMockTest();
