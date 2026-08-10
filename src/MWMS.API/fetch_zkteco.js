const ZKLib = require('node-zklib');
const fs = require('fs');

async function fetchLogs() {
    const ip = process.argv[2] || '10.10.100.102';
    const port = parseInt(process.argv[3]) || 4370;
    const outputPath = process.argv[4] || 'zk_logs.json';
    const usersOutputPath = process.argv[5] || 'zk_users.json';
    
    let zkInstance = new ZKLib(ip, port, 60000, 60000); // Increased timeout to 60s
    try {
        await zkInstance.createSocket();
        
        const attendances = await zkInstance.getAttendances();
        
        try {
            const users = await zkInstance.getUsers();
            fs.writeFileSync(usersOutputPath, JSON.stringify(users.data || []));
        } catch (ue) {
            console.error("Error fetching users: " + ue.message);
            fs.writeFileSync(usersOutputPath, JSON.stringify([]));
        }
        
        // Write to file
        fs.writeFileSync(outputPath, JSON.stringify(attendances.data || []));
        
        await zkInstance.disconnect();
    } catch (e) {
        console.error("Error: " + e.message);
        process.exit(1);
    }
}

fetchLogs();
