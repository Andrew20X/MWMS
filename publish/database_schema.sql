IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Departments] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(250) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Positions] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Positions] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Shifts] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [GraceMinutes] int NOT NULL,
    [LunchMinutes] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Shifts] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Employees] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeCode] nvarchar(20) NOT NULL,
    [DeviceUserId] int NOT NULL,
    [FirstName] nvarchar(50) NOT NULL,
    [MiddleName] nvarchar(50) NULL,
    [LastName] nvarchar(50) NOT NULL,
    [Email] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [HireDate] date NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [DepartmentId] int NOT NULL,
    [PositionId] int NOT NULL,
    [ShiftId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Employees_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Positions_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [Positions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Employees_Shifts_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [Shifts] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Employees_DepartmentId] ON [Employees] ([DepartmentId]);
GO

CREATE UNIQUE INDEX [IX_Employees_EmployeeCode] ON [Employees] ([EmployeeCode]);
GO

CREATE INDEX [IX_Employees_PositionId] ON [Employees] ([PositionId]);
GO

CREATE INDEX [IX_Employees_ShiftId] ON [Employees] ([ShiftId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260706063730_InitialCreate', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(256) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Role] nvarchar(50) NOT NULL DEFAULT N'Employee',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260706124126_AddUsersTable', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Attendance] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Date] date NOT NULL,
    [CheckIn] time NULL,
    [CheckOut] time NULL,
    [Status] int NOT NULL,
    [WorkedHours] float NOT NULL,
    [LateMinutes] int NOT NULL,
    [EarlyLeaveMinutes] int NOT NULL,
    [OvertimeMinutes] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Attendance] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Attendance_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Attendance_EmployeeId] ON [Attendance] ([EmployeeId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260707090907_AddAttendanceTable', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Attendances] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Date] date NOT NULL,
    [CheckIn] time NULL,
    [CheckOut] time NULL,
    [Status] int NOT NULL,
    [WorkedHours] float NOT NULL,
    [LateMinutes] int NOT NULL,
    [EarlyLeaveMinutes] int NOT NULL,
    [OvertimeMinutes] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Attendances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Attendances_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(256) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Role] nvarchar(50) NOT NULL DEFAULT N'Employee',
    [EmployeeId] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Attendances_EmployeeId] ON [Attendances] ([EmployeeId]);
GO

CREATE UNIQUE INDEX [IX_Users_EmployeeId] ON [Users] ([EmployeeId]);
GO

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260707135116_LinkUserToEmployee', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [RawAttendanceLogs] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [PunchTime] datetime2 NOT NULL,
    [DeviceId] nvarchar(max) NOT NULL,
    [IsProcessed] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_RawAttendanceLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RawAttendanceLogs_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RawAttendanceLogs_EmployeeId] ON [RawAttendanceLogs] ([EmployeeId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260708120714_AddRawAttendanceLogs', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [LeaveRequests] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Type] int NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [ApprovedById] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_LeaveRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeaveRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LeaveRequests_Users_ApprovedById] FOREIGN KEY ([ApprovedById]) REFERENCES [Users] ([Id])
);
GO

CREATE INDEX [IX_LeaveRequests_ApprovedById] ON [LeaveRequests] ([ApprovedById]);
GO

CREATE INDEX [IX_LeaveRequests_EmployeeId] ON [LeaveRequests] ([EmployeeId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260708121522_AddLeaveRequests', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Announcements] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [TargetDate] date NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Announcements] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [CorrectionRequests] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Date] date NOT NULL,
    [RequestedCheckIn] time NULL,
    [RequestedCheckOut] time NULL,
    [Reason] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_CorrectionRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CorrectionRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_CorrectionRequests_EmployeeId] ON [CorrectionRequests] ([EmployeeId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260709113655_AddEmployeeFeatures', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [LeaveRequests] ADD [AdminMessage] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260710212131_AddLeaveAdminMessage', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [PasswordResetToken] nvarchar(max) NULL;
GO

ALTER TABLE [Users] ADD [PasswordResetTokenExpiry] datetime2 NULL;
GO

ALTER TABLE [Users] ADD [RequiresPasswordChange] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260712091320_AddPasswordFeatures', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [CorrectionRequests] ADD [AdminNote] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260712140137_AddCorrectionAdminNote', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [OvertimeRequests] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Date] date NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [AdminNote] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_OvertimeRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OvertimeRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_OvertimeRequests_EmployeeId] ON [OvertimeRequests] ([EmployeeId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260713034854_AddOvertimeRequests', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Attendance] DROP CONSTRAINT [FK_Attendance_Employees_EmployeeId];
GO

ALTER TABLE [Attendance] DROP CONSTRAINT [PK_Attendance];
GO

EXEC sp_rename N'[Attendance]', N'Attendances';
GO

EXEC sp_rename N'[Attendances].[IX_Attendance_EmployeeId]', N'IX_Attendances_EmployeeId', N'INDEX';
GO

ALTER TABLE [Users] ADD [ResetToken] nvarchar(max) NULL;
GO

ALTER TABLE [Users] ADD [ResetTokenExpiry] datetime2 NULL;
GO

ALTER TABLE [Attendances] ADD CONSTRAINT [PK_Attendances] PRIMARY KEY ([Id]);
GO

CREATE TABLE [Announcements] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [TargetDate] date NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Announcements] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [CorrectionRequests] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Date] date NOT NULL,
    [RequestedCheckIn] time NULL,
    [RequestedCheckOut] time NULL,
    [Reason] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [AdminNote] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_CorrectionRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CorrectionRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [LeaveRequests] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Type] int NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [ApprovedById] int NULL,
    [AdminMessage] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_LeaveRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeaveRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LeaveRequests_Users_ApprovedById] FOREIGN KEY ([ApprovedById]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [OvertimeRequests] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Date] date NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [AdminNote] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_OvertimeRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OvertimeRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RawAttendanceLogs] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [PunchTime] datetime2 NOT NULL,
    [DeviceId] nvarchar(max) NOT NULL,
    [IsProcessed] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_RawAttendanceLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RawAttendanceLogs_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_CorrectionRequests_EmployeeId] ON [CorrectionRequests] ([EmployeeId]);
GO

CREATE INDEX [IX_LeaveRequests_ApprovedById] ON [LeaveRequests] ([ApprovedById]);
GO

CREATE INDEX [IX_LeaveRequests_EmployeeId] ON [LeaveRequests] ([EmployeeId]);
GO

CREATE INDEX [IX_OvertimeRequests_EmployeeId] ON [OvertimeRequests] ([EmployeeId]);
GO

CREATE INDEX [IX_RawAttendanceLogs_EmployeeId] ON [RawAttendanceLogs] ([EmployeeId]);
GO

ALTER TABLE [Attendances] ADD CONSTRAINT [FK_Attendances_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260713074020_AddResetTokenToUser', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ApprovalHistories] (
    [Id] int NOT NULL IDENTITY,
    [RequestType] nvarchar(max) NOT NULL,
    [RequestId] int NOT NULL,
    [ApproverId] int NOT NULL,
    [ApproverName] nvarchar(max) NOT NULL,
    [ApproverRole] nvarchar(max) NOT NULL,
    [Decision] nvarchar(max) NOT NULL,
    [Comment] nvarchar(max) NULL,
    [DecisionAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_ApprovalHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApprovalHistories_Users_ApproverId] FOREIGN KEY ([ApproverId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [LeaveBalances] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Year] int NOT NULL,
    [AnnualLeaveTotal] int NOT NULL,
    [AnnualLeaveUsed] int NOT NULL,
    [EmergencyLeaveTotal] int NOT NULL,
    [EmergencyLeaveUsed] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_LeaveBalances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeaveBalances_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ApprovalHistories_ApproverId] ON [ApprovalHistories] ([ApproverId]);
GO

CREATE INDEX [IX_LeaveBalances_EmployeeId] ON [LeaveBalances] ([EmployeeId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260716121543_AddApprovalHistoryAndLeaveBalance', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [OvertimeRequests] ADD [Type] nvarchar(max) NOT NULL DEFAULT N'';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260719092942_AddOvertimeType', N'8.0.8');
GO

COMMIT;
GO

