CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Companies` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `TaxId` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Currency` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Companies` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Users` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `PasswordHash` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Accounts` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Code` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `IsGroup` tinyint(1) NOT NULL,
        `ParentId` char(36) COLLATE ascii_general_ci NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Accounts` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_Accounts_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_Accounts_Accounts_CompanyId_ParentId` FOREIGN KEY (`CompanyId`, `ParentId`) REFERENCES `Accounts` (`CompanyId`, `Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Accounts_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Approvals` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Reference` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Amount` decimal(19,4) NOT NULL,
        `Status` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Approvals` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_Approvals_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_Approvals_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Audit` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ResourceId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Action` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Resource` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `OccurredAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_Audit` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_Audit_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_Audit_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `BankStatements` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Reference` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `BookingDate` date NOT NULL,
        `Amount` decimal(19,4) NOT NULL,
        `Description` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_BankStatements` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_BankStatements_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_BankStatements_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Budgets` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Year` int NOT NULL,
        `Amount` decimal(19,4) NOT NULL,
        `Status` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Budgets` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_Budgets_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_Budgets_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `CostCenters` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Code` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_CostCenters` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_CostCenters_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_CostCenters_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `JournalEntries` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `OperationId` char(36) COLLATE ascii_general_ci NOT NULL,
        `RequestHash` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Reference` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Date` date NOT NULL,
        `Status` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_JournalEntries` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_JournalEntries_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_JournalEntries_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Notices` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Message` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `IsDemo` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Notices` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_Notices_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_Notices_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Periods` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `StartDate` date NOT NULL,
        `EndDate` date NOT NULL,
        `Status` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Periods` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_Periods_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_Periods_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Reconciliations` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `StatementDate` date NOT NULL,
        `Status` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Reconciliations` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_Reconciliations_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_Reconciliations_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `TaxRules` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Rate` decimal(9,6) NOT NULL,
        `ValidFrom` date NOT NULL,
        `ValidTo` date NULL,
        CONSTRAINT `PK_TaxRules` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_TaxRules_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_TaxRules_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `ThirdParties` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Document` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_ThirdParties` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_ThirdParties_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_ThirdParties_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Memberships` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Role` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Memberships` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Memberships_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Memberships_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `JournalLines` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EntryId` char(36) COLLATE ascii_general_ci NOT NULL,
        `AccountId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CostCenterId` char(36) COLLATE ascii_general_ci NULL,
        `Debit` decimal(19,4) NOT NULL,
        `Credit` decimal(19,4) NOT NULL,
        CONSTRAINT `PK_JournalLines` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_JournalLines_Accounts_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `Accounts` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_JournalLines_CostCenters_CostCenterId` FOREIGN KEY (`CostCenterId`) REFERENCES `CostCenters` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_JournalLines_JournalEntries_EntryId` FOREIGN KEY (`EntryId`) REFERENCES `JournalEntries` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Invoices` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ThirdPartyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Number` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Amount` decimal(19,4) NOT NULL,
        `IssueDate` date NOT NULL,
        `DueDate` date NOT NULL,
        `Status` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Invoices` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_Invoices_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_Invoices_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Invoices_ThirdParties_CompanyId_ThirdPartyId` FOREIGN KEY (`CompanyId`, `ThirdPartyId`) REFERENCES `ThirdParties` (`CompanyId`, `Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Obligations` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ThirdPartyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Kind` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Amount` decimal(19,4) NOT NULL,
        `DueDate` date NOT NULL,
        `Status` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Obligations` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_Obligations_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_Obligations_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Obligations_ThirdParties_CompanyId_ThirdPartyId` FOREIGN KEY (`CompanyId`, `ThirdPartyId`) REFERENCES `ThirdParties` (`CompanyId`, `Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE TABLE `Payments` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ThirdPartyId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Reference` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Direction` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Amount` decimal(19,4) NOT NULL,
        `Status` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Payments` PRIMARY KEY (`Id`),
        CONSTRAINT `AK_Payments_CompanyId_Id` UNIQUE (`CompanyId`, `Id`),
        CONSTRAINT `FK_Payments_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Payments_ThirdParties_CompanyId_ThirdPartyId` FOREIGN KEY (`CompanyId`, `ThirdPartyId`) REFERENCES `ThirdParties` (`CompanyId`, `Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE UNIQUE INDEX `IX_Accounts_CompanyId_Code` ON `Accounts` (`CompanyId`, `Code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE INDEX `IX_Accounts_CompanyId_ParentId` ON `Accounts` (`CompanyId`, `ParentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE UNIQUE INDEX `IX_Companies_TaxId` ON `Companies` (`TaxId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE UNIQUE INDEX `IX_CostCenters_CompanyId_Code` ON `CostCenters` (`CompanyId`, `Code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE UNIQUE INDEX `IX_Invoices_CompanyId_Number` ON `Invoices` (`CompanyId`, `Number`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE INDEX `IX_Invoices_CompanyId_ThirdPartyId` ON `Invoices` (`CompanyId`, `ThirdPartyId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE UNIQUE INDEX `IX_JournalEntries_CompanyId_OperationId` ON `JournalEntries` (`CompanyId`, `OperationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE INDEX `IX_JournalLines_AccountId` ON `JournalLines` (`AccountId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE INDEX `IX_JournalLines_CostCenterId` ON `JournalLines` (`CostCenterId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE INDEX `IX_JournalLines_EntryId` ON `JournalLines` (`EntryId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE INDEX `IX_Memberships_CompanyId` ON `Memberships` (`CompanyId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE UNIQUE INDEX `IX_Memberships_UserId_CompanyId` ON `Memberships` (`UserId`, `CompanyId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE INDEX `IX_Obligations_CompanyId_ThirdPartyId` ON `Obligations` (`CompanyId`, `ThirdPartyId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE INDEX `IX_Payments_CompanyId_ThirdPartyId` ON `Payments` (`CompanyId`, `ThirdPartyId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023054030_InitialAccounting') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261023054030_InitialAccounting', '8.0.32');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

