const sql = require('mssql');

const config = {
    user: 'your_username', // Not needed for trusted connection if running on windows with integrated auth
    password: 'your_password',
    server: 'localhost\\SQLEXPRESS',
    database: 'MWMS_DB',
    options: {
        encrypt: false,
        trustServerCertificate: true,
        trustedConnection: true
    }
};

const connStr = 'Server=localhost\\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;';

async function test() {
    try {
        await sql.connect(connStr);
        const result = await sql.query`SELECT * FROM Attendances WHERE CAST(Date as Date) = CAST(GETDATE() as Date)`;
        console.log(`Found ${result.recordset.length} attendances for today.`);
        console.dir(result.recordset);
    } catch (err) {
        console.error(err);
    } finally {
        sql.close();
    }
}

test();
