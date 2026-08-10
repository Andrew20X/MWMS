const fs = require('fs');
const logs = JSON.parse(fs.readFileSync('test_logs.json', 'utf8'));

const dates = logs.map(l => l.recordTime.substring(0, 7)); // e.g. "2026-08"
const counts = {};
for (const date of dates) {
    counts[date] = (counts[date] || 0) + 1;
}
console.log(counts);
