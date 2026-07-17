const ZKLib = require('node-zklib');

async function testGetUsers() {
    let zkInstance = new ZKLib({
        ip: '10.10.100.102',
        port: 4370,
        inport: 4000,
        timeout: 10000
    });

    try {
        await zkInstance.createSocket();
        console.log('Connected to ZKTeco device.');
        
        const users = await zkInstance.getUsers();
        console.log('Users:', users.data.slice(0, 3)); // Print first 3 users
    } catch (e) {
        console.error('Error:', e.message || e);
    } finally {
        try {
            await zkInstance.disconnect();
        } catch (e) {}
    }
}

testGetUsers();
