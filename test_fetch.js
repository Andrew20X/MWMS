fetch('http://localhost:5222/api/Attendance/fetch-from-device', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ startDate: "2026-07-01", endDate: "2026-07-31" })
}).then(async res => console.log(res.status, await res.text())).catch(err => console.error(err));
