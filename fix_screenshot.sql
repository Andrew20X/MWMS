-- First, ensure positions exist
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Financial Accountant') INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Financial Accountant', 0, GETDATE(), GETDATE());
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Staff') INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Staff', 0, GETDATE(), GETDATE());
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Sales Manager') INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Sales Manager', 0, GETDATE(), GETDATE());
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'IT Engineer') INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('IT Engineer', 0, GETDATE(), GETDATE());
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Quality Control Manager') INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Quality Control Manager', 0, GETDATE(), GETDATE());
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'R&D Engineer') INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('R&D Engineer', 0, GETDATE(), GETDATE());

-- Fix "no ID" for DeviceUserId = 0
UPDATE Employees SET EmployeeCode = 'no ID - ' + CAST(Id AS VARCHAR) WHERE DeviceUserId = 0;

-- Update specific users from screenshot
UPDATE Employees SET PositionId = (SELECT Id FROM Positions WHERE Name = 'Financial Accountant') WHERE FirstName + ' ' + LastName LIKE '%Abanoub Samir%';
UPDATE Employees SET PositionId = (SELECT Id FROM Positions WHERE Name = 'Staff') WHERE FirstName + ' ' + LastName LIKE '%Abdel Fattah Ahmed%';
UPDATE Employees SET PositionId = (SELECT Id FROM Positions WHERE Name = 'Sales Manager') WHERE FirstName + ' ' + LastName LIKE '%Abdelaziz Faiz%';
UPDATE Employees SET PositionId = (SELECT Id FROM Positions WHERE Name = 'Staff') WHERE FirstName + ' ' + LastName LIKE '%Abdullah Hesham%';
UPDATE Employees SET PositionId = (SELECT Id FROM Positions WHERE Name = 'IT Engineer') WHERE FirstName + ' ' + LastName LIKE '%Abdullah Mohammed Abdullah Sharaby%';
UPDATE Employees SET PositionId = (SELECT Id FROM Positions WHERE Name = 'Quality Control Manager') WHERE FirstName + ' ' + LastName LIKE '%Ahmed Abdel Moneim%';
UPDATE Employees SET PositionId = (SELECT Id FROM Positions WHERE Name = 'R&D Engineer') WHERE FirstName + ' ' + LastName LIKE '%Ahmed Hany Mohammed%';

-- Set manager for Abdelaziz Faiz Ismail Abaza
DECLARE @ManagerId INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Abdelaziz Faiz%');
IF @ManagerId IS NOT NULL
BEGIN
    UPDATE Employees SET ManagerId = @ManagerId WHERE FirstName + ' ' + LastName LIKE '%Rodina Ahmed%';
    UPDATE Employees SET ManagerId = @ManagerId WHERE FirstName + ' ' + LastName LIKE '%Mohammad Abdel-Aleem%';
    UPDATE Employees SET ManagerId = @ManagerId WHERE FirstName + ' ' + LastName LIKE '%Mohamed Mohamed Abdelghany%';
END
