const fs = require('fs');
const logs = JSON.parse(fs.readFileSync('test_logs.json', 'utf8'));

// Sort logs by recordTime
logs.sort((a, b) => new Date(a.recordTime) - new Date(b.recordTime));

console.log("Oldest 5 logs:");
console.log(logs.slice(0, 5));

console.log("Newest 5 logs:");
console.log(logs.slice(-5));

// Check if there are any logs in 2026
const logs2026 = logs.filter(l => l.recordTime.startsWith('2026'));
console.log(`Logs in 2026: ${logs2026.length}`);
