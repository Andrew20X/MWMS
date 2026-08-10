$conn = New-Object System.Data.SqlClient.SqlConnection("Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 * FROM Positions"
$reader = $cmd.ExecuteReader()
$schema = $reader.GetSchemaTable()
$schema | Select-Object ColumnName | ConvertTo-Json | Out-File "d:\MWMS\pos_cols.json"
$conn.Close()
