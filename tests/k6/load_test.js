import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 20 }, // Ramp up to 20 users
        { duration: '1m', target: 20 },  // Stay at 20 users
        { duration: '30s', target: 0 },  // Ramp down to 0 users
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% of requests must complete below 500ms
        http_req_failed: ['rate<0.01'],   // Less than 1% of requests should fail
    },
};

const BASE_URL = 'http://localhost:5222/api'; // Correct port from launchSettings.json

export default function () {
    // 1. Authenticate to get token
    const loginPayload = JSON.stringify({
        username: 'admin',
        password: 'Password123!'
    });
    
    const loginParams = {
        headers: { 'Content-Type': 'application/json' },
    };

    let loginRes = http.post(`${BASE_URL}/Auth/login`, loginPayload, loginParams);
    
    check(loginRes, {
        'login successful': (r) => r.status === 200,
    });

    if (loginRes.status !== 200) {
        return; // Stop if login fails
    }

    let token = loginRes.json('token');
    
    const params = {
        headers: { 
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}` 
        },
    };

    // 2. Fetch Dashboard Stats
    let statsRes = http.get(`${BASE_URL}/Dashboard/stats`, params);
    check(statsRes, { 'dashboard stats 200': (r) => r.status === 200 });
    
    sleep(1);

    // 3. Fetch Employees
    let empRes = http.get(`${BASE_URL}/Employees`, params);
    check(empRes, { 'employees 200': (r) => r.status === 200 });

    sleep(1);

    // 4. Fetch Leaves
    let leavesRes = http.get(`${BASE_URL}/Leaves/all`, params);
    check(leavesRes, { 'leaves 200': (r) => r.status === 200 });

    sleep(1);
}
