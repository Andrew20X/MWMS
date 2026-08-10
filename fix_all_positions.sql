
-- Update for Hossam Sherif
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Managing Director') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Managing Director', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Managing Director')
WHERE FirstName + ' ' + LastName LIKE '%Hossam Sherif%';


-- Update for Fatma Abdelgawad
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Legal Affair Specialist') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Legal Affair Specialist', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Legal Affair Specialist')
WHERE FirstName + ' ' + LastName LIKE '%Fatma%';


-- Update for Riham Abdelaziz
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Office Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Office Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Office Manager')
WHERE FirstName + ' ' + LastName LIKE '%Riham Abdelaziz%';


-- Update for Sales Manager
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Sales Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Sales Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Sales Manager')
WHERE FirstName + ' ' + LastName LIKE '%Sales Manager%';


-- Update for Sales Team
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Sales Team%';


-- Update for Abdel Aziz Abaza
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'BD Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('BD Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'BD Manager')
WHERE FirstName + ' ' + LastName LIKE '%Abdel Aziz Abaza%';


-- Update for Couriers
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Couriers%';


-- Update for Ahmed Ghazy
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Quality Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Quality Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Quality Manager')
WHERE FirstName + ' ' + LastName LIKE '%Ahmed Ghazy%';


-- Update for Quality Assistant
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Quality Assistant%';


-- Update for Karim Hanafy
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'HR Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('HR Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'HR Manager')
WHERE FirstName + ' ' + LastName LIKE '%Karim Hanafy%';


-- Update for Sohila Hany
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'HR Coordinator') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('HR Coordinator', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'HR Coordinator')
WHERE FirstName + ' ' + LastName LIKE '%Sohila Hany%';


-- Update for Service Team
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Service Team%';


-- Update for Security
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Security%';


-- Update for Mohamed Taha
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Admin Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Admin Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Admin Manager')
WHERE FirstName + ' ' + LastName LIKE '%Mohamed Taha%';


-- Update for Esraa El-Sayed
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Admin Coordinator') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Admin Coordinator', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Admin Coordinator')
WHERE FirstName + ' ' + LastName LIKE '%Esraa El-Sayed%';


-- Update for El Hassan Mostafa
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Admin Assistant') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Admin Assistant', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Admin Assistant')
WHERE FirstName + ' ' + LastName LIKE '%El Hassan Mostafa%';


-- Update for Drivers
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Drivers%';


-- Update for Gaber Ammar
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Technical Director') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Technical Director', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Technical Director')
WHERE FirstName + ' ' + LastName LIKE '%Gaber%';


-- Update for Rodina Ahmed
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'CEO Assistant') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('CEO Assistant', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'CEO Assistant')
WHERE FirstName + ' ' + LastName LIKE '%Rodina Ahmed%';


-- Update for Amr Mabrouk
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'CFO') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('CFO', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'CFO')
WHERE FirstName + ' ' + LastName LIKE '%Amr Mabrouk%';


-- Update for Abanoub Samir
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Chief Accountant') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Chief Accountant', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Chief Accountant')
WHERE FirstName + ' ' + LastName LIKE '%Abanoub Samir%';


-- Update for Nourhan Magdy
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'AR Accountant') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('AR Accountant', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'AR Accountant')
WHERE FirstName + ' ' + LastName LIKE '%Nourhan Magdy%';


-- Update for Donia El-Shamy
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'AP Accountant') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('AP Accountant', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'AP Accountant')
WHERE FirstName + ' ' + LastName LIKE '%Donia El-Shamy%';


-- Update for Mahmoud Mehny
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Courier') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Courier', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Courier')
WHERE FirstName + ' ' + LastName LIKE '%Mahmoud Mehny%';


-- Update for Accountant
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Accountant') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Accountant', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Accountant')
WHERE FirstName + ' ' + LastName LIKE '%Accountant%';


-- Update for Senior Treasury
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Senior Treasury%';


-- Update for Ramy Zakaria
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Treasury Accountant') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Treasury Accountant', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Treasury Accountant')
WHERE FirstName + ' ' + LastName LIKE '%Ramy Zakaria%';


-- Update for Ehab Ali
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Procurement Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Procurement Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Procurement Manager')
WHERE FirstName + ' ' + LastName LIKE '%Ehab Ali%';


-- Update for Nesma Mahmoud
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Procurement DC') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Procurement DC', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Procurement DC')
WHERE FirstName + ' ' + LastName LIKE '%Nesma Mahmoud%';


-- Update for Buyers Team
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Buyers Team%';


-- Update for Mona Gabr
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Logistic Specialist') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Logistic Specialist', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Logistic Specialist')
WHERE FirstName + ' ' + LastName LIKE '%Mona Gabr%';


-- Update for Technical Manager
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Technical Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Technical Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Technical Manager')
WHERE FirstName + ' ' + LastName LIKE '%Technical Manager%';


-- Update for Technical Supervisor
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Technical Supervisor%';


-- Update for Instrumentation and Control Engineers
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Instrumentation and Control Engineers%';


-- Update for Ahmed Hany
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'R&D Engineer') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('R&D Engineer', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'R&D Engineer')
WHERE FirstName + ' ' + LastName LIKE '%Ahmed Hany%';


-- Update for Ahmed Khalifa
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Inventory Supervisor') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Inventory Supervisor', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Inventory Supervisor')
WHERE FirstName + ' ' + LastName LIKE '%Ahmed Khalifa%';


-- Update for Hisham Abd-Raouf
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Storekeeper Assistant') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Storekeeper Assistant', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Storekeeper Assistant')
WHERE FirstName + ' ' + LastName LIKE '%Hisham Abd-Raouf%';


-- Update for Ahmed Khater
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Mechanical Workshop Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Mechanical Workshop Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Mechanical Workshop Manager')
WHERE FirstName + ' ' + LastName LIKE '%Ahmed Khater%';


-- Update for Workshop Team
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Employee') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Employee', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Employee')
WHERE FirstName + ' ' + LastName LIKE '%Workshop Team%';


-- Update for Sherif Salah
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Projects Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Projects Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Projects Manager')
WHERE FirstName + ' ' + LastName LIKE '%Sherif Salah%';


-- Update for Kyrollos Nabil
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Project Manager') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Project Manager', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Project Manager')
WHERE FirstName + ' ' + LastName LIKE '%Kyrollos Nabil%';


-- Update for Mohamed Hatem
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Civil Engineer') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Civil Engineer', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Civil Engineer')
WHERE FirstName + ' ' + LastName LIKE '%Mohamed Hatem%';


-- Update for Mohamed El-Saedy
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Mechanical Engineer') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Mechanical Engineer', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Mechanical Engineer')
WHERE FirstName + ' ' + LastName LIKE '%Mohamed El-Saedy%';


-- Update for Ahmed Barhoum
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'Document Controller') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('Document Controller', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'Document Controller')
WHERE FirstName + ' ' + LastName LIKE '%Ahmed Barhoum%';


-- Update for Loay El-Aswad
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Name = 'HSE Supervisor') 
    INSERT INTO Positions (Name, IsDeleted, CreatedAt, UpdatedAt) VALUES ('HSE Supervisor', 0, GETDATE(), GETDATE());

UPDATE Employees 
SET PositionId = (SELECT TOP 1 Id FROM Positions WHERE Name = 'HSE Supervisor')
WHERE FirstName + ' ' + LastName LIKE '%Loay El-Aswad%';
