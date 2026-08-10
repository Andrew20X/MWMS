const http = require('http');

const data = JSON.stringify({
    ipAddress: '10.10.100.102',
    port: 4370,
    machineNumber: 1,
    startDate: '2026-07-01T00:00:00',
    endDate: '2026-07-31T23:59:59'
});

const options = {
    hostname: 'localhost',
    port: 5222,
    path: '/api/Attendance/fetch-from-device',
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Content-Length': data.length
    }
};

const req = http.request(options, (res) => {
    let responseData = '';
    res.on('data', (chunk) => {
        responseData += chunk;
    });
    res.on('end', () => {
        console.log(`Status: ${res.statusCode}`);
        console.log(`Response: ${responseData}`);
    });
});

req.on('error', (error) => {
    console.error(error);
});

req.write(data);
req.end();
