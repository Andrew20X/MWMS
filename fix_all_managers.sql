-- Clear all managers first to avoid cycles
UPDATE Employees SET ManagerId = NULL;


-- Manager for Fatma is Hossam Sherif
DECLARE @MgrId_2 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Hossam Sherif%');
IF @MgrId_2 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_2
    WHERE FirstName + ' ' + LastName LIKE '%Fatma%';
END


-- Manager for Riham Abdelaziz is Hossam Sherif
DECLARE @MgrId_3 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Hossam Sherif%');
IF @MgrId_3 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_3
    WHERE FirstName + ' ' + LastName LIKE '%Riham Abdelaziz%';
END


-- Manager for Sales Manager is Hossam Sherif
DECLARE @MgrId_4 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Hossam Sherif%');
IF @MgrId_4 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_4
    WHERE FirstName + ' ' + LastName LIKE '%Sales Manager%';
END


-- Manager for Sales Team is Sales Manager
DECLARE @MgrId_5 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Sales Manager%');
IF @MgrId_5 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_5
    WHERE FirstName + ' ' + LastName LIKE '%Sales Team%';
END


-- Manager for Abdel Aziz Abaza is Hossam Sherif
DECLARE @MgrId_6 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Hossam Sherif%');
IF @MgrId_6 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_6
    WHERE FirstName + ' ' + LastName LIKE '%Abdel Aziz Abaza%';
END


-- Manager for Couriers is Abdel Aziz Abaza
DECLARE @MgrId_7 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Abdel Aziz Abaza%');
IF @MgrId_7 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_7
    WHERE FirstName + ' ' + LastName LIKE '%Couriers%';
END


-- Manager for Ahmed Ghazy is Hossam Sherif
DECLARE @MgrId_8 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Hossam Sherif%');
IF @MgrId_8 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_8
    WHERE FirstName + ' ' + LastName LIKE '%Ahmed Ghazy%';
END


-- Manager for Quality Assistant is Ahmed Ghazy
DECLARE @MgrId_9 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Ahmed Ghazy%');
IF @MgrId_9 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_9
    WHERE FirstName + ' ' + LastName LIKE '%Quality Assistant%';
END


-- Manager for Karim Hanafy is Hossam Sherif
DECLARE @MgrId_10 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Hossam Sherif%');
IF @MgrId_10 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_10
    WHERE FirstName + ' ' + LastName LIKE '%Karim Hanafy%';
END


-- Manager for Sohila Hany is Karim Hanafy
DECLARE @MgrId_11 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Karim Hanafy%');
IF @MgrId_11 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_11
    WHERE FirstName + ' ' + LastName LIKE '%Sohila Hany%';
END


-- Manager for Service Team is Sohila Hany
DECLARE @MgrId_12 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Sohila Hany%');
IF @MgrId_12 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_12
    WHERE FirstName + ' ' + LastName LIKE '%Service Team%';
END


-- Manager for Security is Sohila Hany
DECLARE @MgrId_13 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Sohila Hany%');
IF @MgrId_13 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_13
    WHERE FirstName + ' ' + LastName LIKE '%Security%';
END


-- Manager for Mohamed Taha is Hossam Sherif
DECLARE @MgrId_14 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Hossam Sherif%');
IF @MgrId_14 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_14
    WHERE FirstName + ' ' + LastName LIKE '%Mohamed Taha%';
END


-- Manager for Esraa El-Sayed is Mohamed Taha
DECLARE @MgrId_15 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Mohamed Taha%');
IF @MgrId_15 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_15
    WHERE FirstName + ' ' + LastName LIKE '%Esraa El-Sayed%';
END


-- Manager for El Hassan Mostafa is Mohamed Taha
DECLARE @MgrId_16 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Mohamed Taha%');
IF @MgrId_16 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_16
    WHERE FirstName + ' ' + LastName LIKE '%El Hassan Mostafa%';
END


-- Manager for Drivers is El Hassan Mostafa
DECLARE @MgrId_17 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%El Hassan Mostafa%');
IF @MgrId_17 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_17
    WHERE FirstName + ' ' + LastName LIKE '%Drivers%';
END


-- Manager for Rodina Ahmed is Gaber
DECLARE @MgrId_18 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Gaber%');
IF @MgrId_18 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_18
    WHERE FirstName + ' ' + LastName LIKE '%Rodina Ahmed%';
END


-- Manager for Amr Mabrouk is Gaber
DECLARE @MgrId_19 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Gaber%');
IF @MgrId_19 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_19
    WHERE FirstName + ' ' + LastName LIKE '%Amr Mabrouk%';
END


-- Manager for Abanoub Samir is Amr Mabrouk
DECLARE @MgrId_20 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Amr Mabrouk%');
IF @MgrId_20 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_20
    WHERE FirstName + ' ' + LastName LIKE '%Abanoub Samir%';
END


-- Manager for Nourhan Magdy is Abanoub Samir
DECLARE @MgrId_21 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Abanoub Samir%');
IF @MgrId_21 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_21
    WHERE FirstName + ' ' + LastName LIKE '%Nourhan Magdy%';
END


-- Manager for Donia El-Shamy is Nourhan Magdy
DECLARE @MgrId_22 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Nourhan Magdy%');
IF @MgrId_22 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_22
    WHERE FirstName + ' ' + LastName LIKE '%Donia El-Shamy%';
END


-- Manager for Mahmoud Mehny is Donia El-Shamy
DECLARE @MgrId_23 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Donia El-Shamy%');
IF @MgrId_23 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_23
    WHERE FirstName + ' ' + LastName LIKE '%Mahmoud Mehny%';
END


-- Manager for Accountant is Amr Mabrouk
DECLARE @MgrId_24 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Amr Mabrouk%');
IF @MgrId_24 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_24
    WHERE FirstName + ' ' + LastName LIKE '%Accountant%';
END


-- Manager for Senior Treasury is Amr Mabrouk
DECLARE @MgrId_25 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Amr Mabrouk%');
IF @MgrId_25 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_25
    WHERE FirstName + ' ' + LastName LIKE '%Senior Treasury%';
END


-- Manager for Ramy Zakaria is Senior Treasury
DECLARE @MgrId_26 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Senior Treasury%');
IF @MgrId_26 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_26
    WHERE FirstName + ' ' + LastName LIKE '%Ramy Zakaria%';
END


-- Manager for Ehab Ali is Gaber
DECLARE @MgrId_27 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Gaber%');
IF @MgrId_27 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_27
    WHERE FirstName + ' ' + LastName LIKE '%Ehab Ali%';
END


-- Manager for Nesma Mahmoud is Ehab Ali
DECLARE @MgrId_28 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Ehab Ali%');
IF @MgrId_28 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_28
    WHERE FirstName + ' ' + LastName LIKE '%Nesma Mahmoud%';
END


-- Manager for Buyers Team is Nesma Mahmoud
DECLARE @MgrId_29 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Nesma Mahmoud%');
IF @MgrId_29 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_29
    WHERE FirstName + ' ' + LastName LIKE '%Buyers Team%';
END


-- Manager for Mona Gabr is Ehab Ali
DECLARE @MgrId_30 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Ehab Ali%');
IF @MgrId_30 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_30
    WHERE FirstName + ' ' + LastName LIKE '%Mona Gabr%';
END


-- Manager for Technical Manager is Gaber
DECLARE @MgrId_31 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Gaber%');
IF @MgrId_31 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_31
    WHERE FirstName + ' ' + LastName LIKE '%Technical Manager%';
END


-- Manager for Technical Supervisor is Technical Manager
DECLARE @MgrId_32 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Technical Manager%');
IF @MgrId_32 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_32
    WHERE FirstName + ' ' + LastName LIKE '%Technical Supervisor%';
END


-- Manager for Instrumentation and Control Engineers is Technical Supervisor
DECLARE @MgrId_33 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Technical Supervisor%');
IF @MgrId_33 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_33
    WHERE FirstName + ' ' + LastName LIKE '%Instrumentation and Control Engineers%';
END


-- Manager for Ahmed Hany is Technical Manager
DECLARE @MgrId_34 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Technical Manager%');
IF @MgrId_34 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_34
    WHERE FirstName + ' ' + LastName LIKE '%Ahmed Hany%';
END


-- Manager for Ahmed Khalifa is Technical Manager
DECLARE @MgrId_35 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Technical Manager%');
IF @MgrId_35 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_35
    WHERE FirstName + ' ' + LastName LIKE '%Ahmed Khalifa%';
END


-- Manager for Hisham Abd-Raouf is Ahmed Khalifa
DECLARE @MgrId_36 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Ahmed Khalifa%');
IF @MgrId_36 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_36
    WHERE FirstName + ' ' + LastName LIKE '%Hisham Abd-Raouf%';
END


-- Manager for Ahmed Khater is Gaber
DECLARE @MgrId_37 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Gaber%');
IF @MgrId_37 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_37
    WHERE FirstName + ' ' + LastName LIKE '%Ahmed Khater%';
END


-- Manager for Workshop Team is Ahmed Khater
DECLARE @MgrId_38 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Ahmed Khater%');
IF @MgrId_38 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_38
    WHERE FirstName + ' ' + LastName LIKE '%Workshop Team%';
END


-- Manager for Sherif Salah is Gaber
DECLARE @MgrId_39 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Gaber%');
IF @MgrId_39 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_39
    WHERE FirstName + ' ' + LastName LIKE '%Sherif Salah%';
END


-- Manager for Kyrollos Nabil is Sherif Salah
DECLARE @MgrId_40 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Sherif Salah%');
IF @MgrId_40 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_40
    WHERE FirstName + ' ' + LastName LIKE '%Kyrollos Nabil%';
END


-- Manager for Mohamed Hatem is Kyrollos Nabil
DECLARE @MgrId_41 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Kyrollos Nabil%');
IF @MgrId_41 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_41
    WHERE FirstName + ' ' + LastName LIKE '%Mohamed Hatem%';
END


-- Manager for Mohamed El-Saedy is Kyrollos Nabil
DECLARE @MgrId_42 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Kyrollos Nabil%');
IF @MgrId_42 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_42
    WHERE FirstName + ' ' + LastName LIKE '%Mohamed El-Saedy%';
END


-- Manager for Ahmed Barhoum is Kyrollos Nabil
DECLARE @MgrId_43 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Kyrollos Nabil%');
IF @MgrId_43 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_43
    WHERE FirstName + ' ' + LastName LIKE '%Ahmed Barhoum%';
END


-- Manager for Loay El-Aswad is Gaber
DECLARE @MgrId_44 INT = (SELECT TOP 1 Id FROM Employees WHERE FirstName + ' ' + LastName LIKE '%Gaber%');
IF @MgrId_44 IS NOT NULL
BEGIN
    UPDATE Employees 
    SET ManagerId = @MgrId_44
    WHERE FirstName + ' ' + LastName LIKE '%Loay El-Aswad%';
END
