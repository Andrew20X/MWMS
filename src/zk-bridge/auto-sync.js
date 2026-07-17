const cron = require('node-cron');
const { exec } = require('child_process');

console.log('[zk-bridge-autosync] Starting Auto-Sync Service...');
console.log('[zk-bridge-autosync] Scheduled to run every 5 minutes.');

// Run every 5 minutes
cron.schedule('*/5 * * * *', () => {
    console.log(`\n[zk-bridge-autosync] Triggering sync at ${new Date().toISOString()}`);
    
    // 1. Sync Users
    console.log('[zk-bridge-autosync] Running sync-users...');
    exec('node sync-users.js', (error, stdout, stderr) => {
        if (error) {
            console.error(`[zk-bridge-autosync] sync-users error: ${error.message}`);
            return;
        }
        if (stderr) {
            console.error(`[zk-bridge-autosync] sync-users stderr: ${stderr}`);
        }
        console.log(`[zk-bridge-autosync] sync-users output:\n${stdout}`);
        
        // 2. Sync Attendances (only after users are synced to avoid missing foreign keys)
        console.log('[zk-bridge-autosync] Running sync-attendances (index.js)...');
        exec('node index.js', (err, out, serr) => {
            if (err) {
                console.error(`[zk-bridge-autosync] sync-attendances error: ${err.message}`);
                return;
            }
            if (serr) {
                console.error(`[zk-bridge-autosync] sync-attendances stderr: ${serr}`);
            }
            console.log(`[zk-bridge-autosync] sync-attendances output:\n${out}`);
            console.log('[zk-bridge-autosync] Sync cycle complete.');
        });
    });
});
