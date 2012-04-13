CREATE ROLE [WebApplicationUser]
AUTHORIZATION [dbo]
GO
EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\ELasater-N216D$'
GO
EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\JUAN-VM-WIN7$'
GO
EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\ngudmunso-n216c$'
GO
EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\SSOUTHVM1-N216K$'
GO
