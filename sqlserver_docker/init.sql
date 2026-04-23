CREATE DATABASE BancaDB;
GO
USE BancaDB;
GO

-- Tabella per le singole transazioni
CREATE TABLE Transactions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Mcc NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Tabella per il riepilogo giornaliero (Spending)
CREATE TABLE DailySpending (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Mcc NVARCHAR(50) NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL,
    Date DATE NOT NULL,
    CONSTRAINT UC_Date_Mcc UNIQUE (Date, Mcc)
);
GO