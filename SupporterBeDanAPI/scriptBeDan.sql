CREATE DATABASE SupporterBeDan;
GO

USE SupporterBeDan;
GO

CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);
GO

INSERT INTO Roles (Name) VALUES 
('Admin'),
('User'),
('Supporter');
GO

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,

    FullName NVARCHAR(100),
    Facebook NVARCHAR(200),
    Phone NVARCHAR(20),

    RoleId INT NOT NULL,

    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Users_Roles 
        FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
GO

CREATE TABLE RegistrationStatuses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);
GO

INSERT INTO RegistrationStatuses (Name) VALUES
('Pending'),
('Approved'),
('Rejected');
GO

CREATE TABLE ExamCompletionStatuses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);
GO

INSERT INTO ExamCompletionStatuses (Name) VALUES
('Not Taken'),
('Passed'),
('Failed');
GO

CREATE TABLE ExamRegistrations (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT NOT NULL,

    Subject NVARCHAR(100) NOT NULL,
    ExamDate DATE NOT NULL,
    Slot NVARCHAR(50) NOT NULL,
    SPCode NVARCHAR(50),
    PaymentStatus NVARCHAR(50),
    ContactInfo NVARCHAR(200),

    RegistrationStatusId INT NOT NULL DEFAULT 1,
    ExamCompletionStatusId INT NOT NULL DEFAULT 1,

    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Exam_User
        FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT FK_Exam_RegistrationStatus
        FOREIGN KEY (RegistrationStatusId) REFERENCES RegistrationStatuses(Id),

    CONSTRAINT FK_Exam_CompletionStatus
        FOREIGN KEY (ExamCompletionStatusId) REFERENCES ExamCompletionStatuses(Id)
);
GO

CREATE INDEX IX_Exam_UserId ON ExamRegistrations(UserId);
GO

CREATE TABLE ExamAssignments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ExamRegistrationId INT NOT NULL,
    SupporterId INT NOT NULL,
    AssignedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_ExamAssignment_Exam
        FOREIGN KEY (ExamRegistrationId) REFERENCES ExamRegistrations(Id),

    CONSTRAINT FK_ExamAssignment_Supporter
        FOREIGN KEY (SupporterId) REFERENCES Users(Id)
);
GO

CREATE INDEX IX_ExamAssignment_ExamId ON ExamAssignments(ExamRegistrationId);
CREATE INDEX IX_ExamAssignment_SupporterId ON ExamAssignments(SupporterId);
GO

INSERT INTO Users (Username, Password, FullName, RoleId)
VALUES 
('chubedan_admin', 'Dungdan2003$', N'Admin Bé Đần', 1),
('bedan_user', '123', N'Chú Bé Đần', 2),
('bedan_sp', '123', N'Supporter Bé Đần', 3);
GO
