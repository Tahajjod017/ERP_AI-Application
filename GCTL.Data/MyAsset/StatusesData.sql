GO
SET IDENTITY_INSERT [dbo].[Statuses] ON 
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (1, N'Active', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.557' AS DateTime), NULL, N'Active/Inactive', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (2, N'Inactive', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.560' AS DateTime), NULL, N'Active/Inactive', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (3, N'APPROVED', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.560' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (4, N'DECLINED', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.560' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (5, N'Present', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.563' AS DateTime), NULL, N'3', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (6, N'Absent', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.563' AS DateTime), NULL, N'3', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (7, N'Late In', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.563' AS DateTime), NULL, N'3', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (8, N'Early In', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.567' AS DateTime), NULL, N'3', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (9, N'On Leave', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.567' AS DateTime), NULL, N'3', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (10, N'Half Day', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.567' AS DateTime), NULL, N'3', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (11, N'Pending', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.570' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (12, N'On Hold', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.570' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (13, N'Waiting For Approver', NULL, NULL, NULL, NULL, CAST(N'2025-09-08T11:20:14.570' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (14, N'Late Out', NULL, NULL, NULL, NULL, CAST(N'2025-09-20T08:32:50.043' AS DateTime), NULL, N'3', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (15, N'Early Out', NULL, NULL, NULL, NULL, CAST(N'2025-09-20T08:32:50.043' AS DateTime), NULL, N'3', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (16, N'Holiday', NULL, NULL, NULL, NULL, CAST(N'2025-11-01T10:16:02.687' AS DateTime), NULL, N'3', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (17, N'Weekend', NULL, NULL, NULL, NULL, CAST(N'2025-11-01T10:16:02.687' AS DateTime), NULL, N'3', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (1016, N'Open', N'192.168.0.214', N'C85ACFA3CE99', 12, NULL, CAST(N'2025-12-24T11:25:17.100' AS DateTime), NULL, N'po', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (1017, N'Close', N'192.168.0.214', N'C85ACFA3CE99', 12, NULL, CAST(N'2025-12-24T11:25:25.990' AS DateTime), NULL, N'po', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (1018, N'Cancel', N'192.168.0.214', N'C85ACFA3CE99', 12, NULL, CAST(N'2025-12-24T11:25:32.583' AS DateTime), NULL, N'po', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (1024, N'Draft', NULL, NULL, NULL, NULL, CAST(N'2025-12-24T12:21:16.380' AS DateTime), NULL, N'po', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (1041, N'STOCK_IN', NULL, NULL, 1, NULL, CAST(N'2025-12-24T16:51:14.410' AS DateTime), NULL, N'INVENTORY_TRANSACTION', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (1042, N'STOCK_OUT', NULL, NULL, 1, NULL, CAST(N'2025-12-24T16:51:14.410' AS DateTime), NULL, N'INVENTORY_TRANSACTION', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (1043, N'STOCK_ADJUSTMENT', NULL, NULL, 1, NULL, CAST(N'2025-12-24T16:51:14.410' AS DateTime), NULL, N'INVENTORY_TRANSACTION', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (1044, N'STOCK_TRANSFER', NULL, NULL, 1, NULL, CAST(N'2025-12-24T16:51:14.410' AS DateTime), NULL, N'INVENTORY_TRANSACTION', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (2025, N'Packed', NULL, NULL, NULL, NULL, CAST(N'2025-12-27T12:15:03.983' AS DateTime), NULL, N'default', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (2026, N'Shipped', NULL, NULL, NULL, NULL, CAST(N'2025-12-27T12:15:21.353' AS DateTime), NULL, N'default', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (2027, N'Outbound', NULL, NULL, NULL, NULL, CAST(N'2025-12-27T12:15:30.277' AS DateTime), NULL, N'default', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (2028, N'Delivered', NULL, NULL, NULL, NULL, CAST(N'2025-12-27T12:18:21.927' AS DateTime), NULL, N'default', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (2029, N'Cancelled', NULL, NULL, NULL, NULL, CAST(N'2025-12-27T13:40:52.390' AS DateTime), NULL, N'default', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (2030, N'Inbound', NULL, NULL, NULL, NULL, CAST(N'2025-12-27T13:49:47.763' AS DateTime), NULL, N'default', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (2031, N'Normal', NULL, NULL, NULL, NULL, CAST(N'2026-01-04T14:31:46.480' AS DateTime), NULL, N'priority', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (2032, N'Medium', NULL, NULL, NULL, NULL, CAST(N'2026-01-04T14:31:46.480' AS DateTime), NULL, N'priority', NULL, NULL, NULL)
GO
INSERT [dbo].[Statuses] ([StatusID], [StatusName], [LIP], [LMAC], [CreatedBy], [UpdatedBy], [CreatedAt], [UpdatedAt], [StatusType], [DeletedAt], [DeletedBy], [StatusCode]) VALUES (2033, N'High', NULL, NULL, NULL, NULL, CAST(N'2026-01-04T14:31:46.480' AS DateTime), NULL, N'priority', NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Statuses] OFF
GO
