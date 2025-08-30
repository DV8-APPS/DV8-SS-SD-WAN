-- One-click schema build for SQL Server
CREATE DATABASE SS_NETWORK_MANAGER;
GO
USE SS_NETWORK_MANAGER;
GO

-- Devices table
IF OBJECT_ID('devices') IS NULL
BEGIN
    CREATE TABLE devices (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(100) NOT NULL UNIQUE,
        device_type NVARCHAR(50) NULL,
        ports INT DEFAULT 0,
        status NVARCHAR(50) DEFAULT 'up',
        latitude FLOAT NULL,
        longitude FLOAT NULL
    );
END;
GO

-- Firmware versions
IF OBJECT_ID('firmware') IS NULL
BEGIN
    CREATE TABLE firmware (
        id INT IDENTITY(1,1) PRIMARY KEY,
        device_id INT UNIQUE REFERENCES devices(id),
        version NVARCHAR(50) NOT NULL
    );
END;
GO

-- Zero-touch enrollment
IF OBJECT_ID('zero_touch') IS NULL
BEGIN
    CREATE TABLE zero_touch (
        id INT IDENTITY(1,1) PRIMARY KEY,
        device_id INT UNIQUE REFERENCES devices(id),
        template NVARCHAR(100) NOT NULL,
        applied BIT DEFAULT 0
    );
END;
GO

-- Warning indicators
IF OBJECT_ID('warnings') IS NULL
BEGIN
    CREATE TABLE warnings (
        id INT IDENTITY(1,1) PRIMARY KEY,
        device_id INT REFERENCES devices(id),
        name NVARCHAR(50) NOT NULL,
        active BIT DEFAULT 1
    );
END;
GO

-- Audit log
IF OBJECT_ID('audit_log') IS NULL
BEGIN
    CREATE TABLE audit_log (
        id INT IDENTITY(1,1) PRIMARY KEY,
        action NVARCHAR(100) NOT NULL,
        entity NVARCHAR(100) NOT NULL,
        entity_id INT NULL,
        details NVARCHAR(MAX) NULL,
        [timestamp] DATETIME2 DEFAULT SYSUTCDATETIME()
    );
END;
GO
