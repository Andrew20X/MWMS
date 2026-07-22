const sql = require('mssql');
async function dropAll() {
  try {
    await sql.connect('Server=SQL1002.site4now.net;Database=db_acc482_andrew20x;User Id=db_acc482_andrew20x_admin;Password=Andrg2020;Encrypt=True;TrustServerCertificate=True;');
    const request = new sql.Request();
    
    // Drop foreign keys first
    let { recordset: fks } = await request.query(`
      SELECT 
          'ALTER TABLE [' + OBJECT_SCHEMA_NAME(parent_object_id) + '].[' + OBJECT_NAME(parent_object_id) + '] DROP CONSTRAINT [' + name + ']' AS DropCommand
      FROM sys.foreign_keys
    `);
    for (const row of fks) {
      await request.query(row.DropCommand);
      console.log(row.DropCommand);
    }

    // Drop tables
    let { recordset: tables } = await request.query(`
      SELECT 'DROP TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']' AS DropCommand 
      FROM INFORMATION_SCHEMA.TABLES 
      WHERE TABLE_TYPE = 'BASE TABLE'
    `);
    for (const row of tables) {
      await request.query(row.DropCommand);
      console.log(row.DropCommand);
    }
    console.log("Database cleared successfully.");
  } catch (err) {
    console.error(err);
  } finally {
    sql.close();
  }
}
dropAll();
