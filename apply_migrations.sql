-- Mark the TalentX migration as applied (columns already exist in DB)
-- Then apply the new JobPost fields migration

-- Step 1: Mark TalentX migration as applied
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260524132404_AddTalentXResumeScoring')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260524132404_AddTalentXResumeScoring', N'10.0.3');
    PRINT 'Marked AddTalentXResumeScoring as applied';
END
ELSE
BEGIN
    PRINT 'AddTalentXResumeScoring already marked as applied';
END
GO

-- Step 2: Apply JobPost new fields
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260606190844_AddJobPostNewFields')
BEGIN
    ALTER TABLE [JobPosts] ADD [Department] nvarchar(200) NULL;
    ALTER TABLE [JobPosts] ADD [DegreesJson] nvarchar(max) NULL;
    ALTER TABLE [JobPosts] ADD [ExperienceMaxYears] int NULL;
    ALTER TABLE [JobPosts] ADD [ExperienceMinYears] int NULL;
    ALTER TABLE [JobPosts] ADD [ExperiencePriority] nvarchar(20) NULL;
    ALTER TABLE [JobPosts] ADD [GPA] decimal(3,2) NULL;
    ALTER TABLE [JobPosts] ADD [GPAPriority] nvarchar(20) NULL;
    ALTER TABLE [JobPosts] ADD [RolesJson] nvarchar(max) NULL;
    ALTER TABLE [JobPosts] ADD [SkillsJson] nvarchar(max) NULL;

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260606190844_AddJobPostNewFields', N'10.0.3');
    PRINT 'Applied AddJobPostNewFields migration';
END
ELSE
BEGIN
    PRINT 'AddJobPostNewFields already applied';
END
GO
