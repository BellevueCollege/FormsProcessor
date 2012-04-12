CREATE TABLE [dbo].[SubmittedForm] (
    [ID]       UNIQUEIDENTIFIER NOT NULL,
    [FormID]   UNIQUEIDENTIFIER NOT NULL,
    [Referrer] NVARCHAR (1024)  NOT NULL,
    [Datetime] DATETIME         NOT NULL,
    [ClientIP] NVARCHAR (40)    NOT NULL,
    [Data]     XML              NOT NULL
);

