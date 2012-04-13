CREATE TABLE [dbo].[SubmittedForm]
(
[ID] [uniqueidentifier] NOT NULL,
[FormID] [uniqueidentifier] NOT NULL,
[Referrer] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Datetime] [datetime] NOT NULL,
[ClientIP] [nvarchar] (40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Data] [xml] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
GRANT INSERT ON  [dbo].[SubmittedForm] TO [WebApplicationUser]
GRANT UPDATE ON  [dbo].[SubmittedForm] TO [WebApplicationUser]
GO

ALTER TABLE [dbo].[SubmittedForm] ADD CONSTRAINT [PK_SubmittedForm_1] PRIMARY KEY CLUSTERED  ([ID]) ON [PRIMARY]
GO
