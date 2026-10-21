/* ============================================================
   اسکریپت مهاجرت: نشست‌های چندنشستی + OTP امن
   پایگاه داده: SQL Server (SanjeshP_Payeh)
   روش جایگزین: به‌جای این اسکریپت می‌توانید بعد از اعمال تغییرات Entity،
   دستور زیر را در پوشه‌ی.solution اجرا کنید:
     dotnet ef migrations add Auth_Sessions_Otp -p SanjeshP.Payeh.Data -s SanjeshP.Payeh.Api
     dotnet ef database update -p SanjeshP.Payeh.Data -s SanjeshP.Payeh.Api
   ⚠️ قبل از اجرا بک‌آپ بگیرید. این اسکریپت idempotent نوشته شده است.
   ============================================================ */

BEGIN TRANSACTION;

/* ------------------------------------------------------------
   ۱) تغییر نام جدول UserTokens به UserSessions
   ------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.UserTokens', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.UserSessions', N'U') IS NULL
BEGIN
    EXEC sp_rename N'dbo.UserTokens', N'UserSessions';
END

/* ------------------------------------------------------------
   ۲) ستون‌های جدید نشست (Refresh چرخشی + حسابرسی)
   ------------------------------------------------------------ */
IF COL_LENGTH('dbo.UserSessions', 'RefreshTokenHash') IS NULL
    ALTER TABLE dbo.UserSessions ADD RefreshTokenHash NVARCHAR(128) NOT NULL CONSTRAINT DF_UserSessions_RefreshTokenHash DEFAULT (N'');

IF COL_LENGTH('dbo.UserSessions', 'PreviousRefreshTokenHash') IS NULL
    ALTER TABLE dbo.UserSessions ADD PreviousRefreshTokenHash NVARCHAR(128) NULL;

IF COL_LENGTH('dbo.UserSessions', 'UserAgent') IS NULL
    ALTER TABLE dbo.UserSessions ADD UserAgent NVARCHAR(512) NULL;

IF COL_LENGTH('dbo.UserSessions', 'LastUsedAt') IS NULL
    ALTER TABLE dbo.UserSessions ADD LastUsedAt DATETIME2 NULL;

/* نشست‌های قدیمی بدون RefreshToken معتبرند ⇒ باطل می‌شوند تا کاربر دوباره وارد شود. */
UPDATE dbo.UserSessions
SET RevocationDate = SYSUTCDATETIME()
WHERE RefreshTokenHash = N'' AND RevocationDate IS NULL;

/* ------------------------------------------------------------
   ۳) امن‌سازی OTP: حذف ستون متن ساده و افزودن Hash و شمارنده تلاش
   ------------------------------------------------------------ */
IF COL_LENGTH('dbo.UserOtps', 'CodeHash') IS NULL
    ALTER TABLE dbo.UserOtps ADD CodeHash NVARCHAR(128) NOT NULL CONSTRAINT DF_UserOtps_CodeHash DEFAULT (N'');

IF COL_LENGTH('dbo.UserOtps', 'FailedAttempts') IS NULL
    ALTER TABLE dbo.UserOtps ADD FailedAttempts INT NOT NULL CONSTRAINT DF_UserOtps_FailedAttempts DEFAULT (0);

IF COL_LENGTH('dbo.UserOtps', 'ConsumedAt') IS NULL
    ALTER TABLE dbo.UserOtps ADD ConsumedAt DATETIME2 NULL;

/* کدهای قدیمی قابل مهاجرت نیستند (متن ساده) ⇒ باطل می‌شوند. */
UPDATE dbo.UserOtps SET IsUsed = 1 WHERE CodeHash = N'' AND IsUsed = 0;

IF COL_LENGTH('dbo.UserOtps', 'Code') IS NOT NULL
    ALTER TABLE dbo.UserOtps DROP COLUMN Code;

/* ------------------------------------------------------------
   ۴) کاربر: رمز عبور اختیاری (ورود OTP-محور)
   ------------------------------------------------------------ */
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = 'PasswordHash' AND is_nullable = 0)
    ALTER TABLE dbo.Users ALTER COLUMN PasswordHash NVARCHAR(MAX) NULL;

/* ------------------------------------------------------------
   ۵) ایندکس‌های کارایی
   ------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserSessions_RefreshTokenHash')
    CREATE UNIQUE INDEX IX_UserSessions_RefreshTokenHash
        ON dbo.UserSessions (RefreshTokenHash) WHERE RefreshTokenHash <> N'';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserSessions_UserId_RevocationDate')
    CREATE INDEX IX_UserSessions_UserId_RevocationDate
        ON dbo.UserSessions (UserId, RevocationDate) INCLUDE (RefreshTokenExpireDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserOtps_UserId_Purpose_IsUsed')
    CREATE INDEX IX_UserOtps_UserId_Purpose_IsUsed
        ON dbo.UserOtps (UserId, Purpose, IsUsed) INCLUDE (CreatedDate, ExpireAt);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_PhoneNumber')
    CREATE INDEX IX_Users_PhoneNumber ON dbo.Users (PhoneNumber) WHERE IsDeleted = 0;

/* ------------------------------------------------------------
   ۶) حذف جدول بلااستفاده UserVerificationCodes (کد آن در پروژه کامنت بود)
   ------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.UserVerificationCodes', N'U') IS NOT NULL
    DROP TABLE dbo.UserVerificationCodes;

COMMIT TRANSACTION;