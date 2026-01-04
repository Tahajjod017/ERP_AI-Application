-- Registration 
-- Please add 'Admin' and 'Role Permission' menus before creating SuperAdmin.
-- Insert MenuTab Data
-- Insert Permissions Table data
SET IDENTITY_INSERT [dbo].[Permissions] ON 
GO
INSERT [dbo].[Permissions] ([Id], [Name]) VALUES (1, N'View')
GO
INSERT [dbo].[Permissions] ([Id], [Name]) VALUES (2, N'Create')
GO
INSERT [dbo].[Permissions] ([Id], [Name]) VALUES (3, N'Edit')
GO
INSERT [dbo].[Permissions] ([Id], [Name]) VALUES (4, N'Delete')
GO
INSERT [dbo].[Permissions] ([Id], [Name]) VALUES (5, N'Export')
GO
INSERT [dbo].[Permissions] ([Id], [Name]) VALUES (6, N'Download')
GO
SET IDENTITY_INSERT [dbo].[Permissions] OFF
GO
-- https://localhost:7086/Account/Register
GO
SET IDENTITY_INSERT TenantInfo ON;
INSERT INTO TenantInfo (Id, TenantName, Identifier, ConnectionString, Description) VALUES (1, 'CLS', 'cls', '', '');
SET IDENTITY_INSERT TenantInfo OFF;
GO
UPDATE AspNetRoles SET TenantInfoId = 1;
GO
SET IDENTITY_INSERT [dbo].[LanguageLists] ON 
GO
INSERT [dbo].[LanguageLists] ([ID], [LanguageCode], [LanguageName]) VALUES (1, N'en', N'English')
GO
INSERT [dbo].[LanguageLists] ([ID], [LanguageCode], [LanguageName]) VALUES (2, N'bn', N'Bangla')
GO
INSERT [dbo].[LanguageLists] ([ID], [LanguageCode], [LanguageName]) VALUES (3, N'de', N'German')
GO
SET IDENTITY_INSERT [dbo].[LanguageLists] OFF
GO