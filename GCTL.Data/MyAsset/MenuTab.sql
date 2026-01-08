USE [HRM_DB_CLS]
GO
/****** Object:  Table [dbo].[MenuTab]    Script Date: 01/04/26 03:54:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET IDENTITY_INSERT [dbo].[MenuTab] ON 
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (1, N'Home', N'Primary', NULL, 1, NULL, N'pie-chart', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (2, N'CRM', N'Primary', NULL, 2, NULL, N'phone', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (3, N'Employee Management', N'Primary', NULL, 3, NULL, N'users', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (4, N'Attendance Management', N'Primary', NULL, 4, NULL, N'activity', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (5, N'Project Management', N'Primary', NULL, 5, NULL, N'briefcase', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (6, N'Payroll Management', N'Primary', NULL, 6, NULL, N'dollar-sign', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (7, N'Finance', N'Primary', NULL, 9, NULL, N'dollar-sign', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (18, N'Master Setup', N'Primary', NULL, 7, NULL, N'layers', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (19, N'Settings', N'Primary', NULL, 19, NULL, N'settings', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (20, N'Admin', N'Primary', NULL, 20, NULL, N'user', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (21, N'Admin Dashboard', N'Secondary', 1, 1, N'AdminDashboard', N'fa-solid fa-gauge', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (22, N'User Dashboard', N'Secondary', 1, 2, N'UserDashboard', N'fa-solid fa-user', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (31, N'Create Leads', N'Secondary', 2, 1, N'createLead', N'fa-solid fa-plus-circle', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (32, N'Leads', N'Secondary', 2, 2, N'crm', N'fa-solid fa-address-book', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (33, N'Leads Details', N'Secondary', 2, 3, N'LeadDetails', N'fa-solid fa-users-cog', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (41, N'Staff Portal', N'Secondary', 3, 1, N'EmployeeList1', N'fa-solid fa-users', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (42, N'Transfer Management', N'Secondary', 3, 2, N'EmployeeTransferManagement1', N'fa-solid fa-random', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (43, N'Employee Increment Management', N'Secondary', 3, 3, N'pop', N'fa-solid fa-chart-line', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (44, N'Promotion Management', N'Secondary', 3, 4, N'Pro', N'fa-solid fa-level-up-alt', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (45, N'Employee Resign', N'Secondary', 3, 5, N'rrr', N'fas fa-sign-out-alt', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (46, N'Employee Termination', N'Secondary', 3, 6, N'dd', N'fas fa-ban', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (51, N'Attendance Management', N'Secondary', 4, 1, N'Attendance', N'fa-solid fa-clock', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (52, N'Schedule Management', N'Secondary', 4, 2, N'ScheduleManagement', N'fa-solid fa-calendar', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (53, N'Leave Management', N'Secondary', 4, 3, N'Leave', N'fa-solid fa-plane-departure', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (61, N'Create New', N'Secondary', 5, 1, N'CreateNewFile', N'fa-solid fa-plus', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (62, N'Project List View', N'Secondary', 5, 2, N'ProjectListView', N'fa-solid fa-table', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (63, N'Project Card View', N'Secondary', 5, 3, N'ProjectCardView', N'fa-solid fa-id-card-alt', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (64, N'Project Board View', N'Secondary', 5, 4, N'ProjectBoardView', N'fa-solid fa-columns', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (71, N'Payroll Policy', N'Secondary', 6, 1, N'PayrollPolicy', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (72, N'Loan Management', N'Secondary', 6, 2, N'LoanManagement', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (81, N'Leave Settings', N'Secondary', 19, 1, N'LeaveSettings', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (82, N'System Setting', N'Secondary', 19, 2, N'EmailSetting', N'fa-solid fa-sliders-h', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (83, N'Organization Settings', N'Secondary', 19, 3, N'CompanySettings', N'fa-solid fa-building', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (84, N'HRM Setting', N'Secondary', 19, 4, N'ProbationPeriodSetting', N'fa-solid fa-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (85, N'Pay Roll Settings', N'Secondary', 19, 5, N'PayrollSettings', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (91, N'User Profile', N'Secondary', 20, 1, N'UserProfile', N'fa-solid fa-user-circle', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (92, N'Create Role', N'Secondary', 20, 2, N'RoleCreateAssign', N'fa-solid fa-user-plus', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (93, N'Assign User', N'Secondary', 20, 3, N'AccessPermission', N'fa-solid fa-user-lock', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (94, N'Role Permission', N'Secondary', 20, 4, N'RolePermission', N'fa-solid fa-user-shield', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (95, N'Menu Tab', N'Secondary', 20, 5, N'MenuTabs', N'fa-solid fa-bars', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (96, N'Language', N'Secondary', 20, 6, N'Language', N'fa-solid fa-language', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (97, N'Action Log', N'Secondary', 20, 7, N'ActionLog', N'fa-solid fa-history', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (98, N'Visiting Path', N'Secondary', 20, 8, N'VisitingPath', N'fa-solid fa-map-marker-alt', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (99, N'Layout Setting', N'Secondary', 20, 9, N'LayoutManagement', N'fa-solid fa-cogs', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (111, N'Employee List', N'Tertiary', 41, 1, N'EmployeeList', N'fa-solid fa-users', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (112, N'Register Employee', N'Tertiary', 41, 2, N'EmployeePersonal', N'fa-solid fa-user-plus', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (121, N'Transfer Entry', N'Tertiary', 42, 1, N'EmployeeTransferManagement', N'fa-solid fa-random', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (122, N'Transfer Approval', N'Tertiary', 42, 2, N'EmpTransferApprovar', N'fa-solid fa-check-double', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (131, N'Increment List', N'Tertiary', 43, 1, N'IncrementList', N'fas fa-exchange-alt', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (132, N'Increment Entry', N'Tertiary', 43, 2, N'Increment', N'fas fa-arrow-alt-circle-up', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (133, N'Increment Approval', N'Tertiary', 43, 3, N'IncrementApprove', N'fa fa-line-chart', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (141, N'Promotion Entry', N'Tertiary', 44, 1, N'Promotion', N'fas fa-edit', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (142, N'Promotion List', N'Tertiary', 44, 2, N'PromotionList', N'fas fa-align-left', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (143, N'Promotion Approval', N'Tertiary', 44, 3, N'PromotionApprove', N'fas fa-angle-double-up', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (151, N'Resign Entry', N'Tertiary', 45, 1, N'EmployeeResign', N'fas fa-money-check', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (152, N'Resign Approval', N'Tertiary', 45, 2, N'EmployeeResignApproval', N'fas fa-arrow-alt-circle-down', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (161, N'Temination', N'Tertiary', 46, 1, N'EmployeeTermination', N'fas fa-chalkboard-teacher', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (162, N'Temination Approve', N'Tertiary', 46, 2, N'EmployeeTerminationApproval', N'fas fa-file-upload', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (171, N'Employees Attendance', N'Tertiary', 51, 1, N'EmployeesAttendance', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (172, N'Attendence Report', N'Tertiary', 51, 2, N'DailyReportForAll', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (173, N'Manual Attendance', N'Tertiary', 51, 3, N'ManualAttendence', N'fa fa-users', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (181, N'Add Shift', N'Tertiary', 52, 1, N'AddShift', N'fa-solid fa-cloud-moon', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (182, N'Assign Default Shift', N'Tertiary', 52, 2, N'AssignDefaultShift', N'fa-solid fa-cloud-sun', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (183, N'Assign Roster', N'Tertiary', 52, 3, N'OfficeDayRoster', N'fa-regular fa-calendar-check', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (184, N'Create Spiral Pattern', N'Tertiary', 52, 4, N'CreateSpiralPattern', N'fa-solid fa-calendar-plus', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (185, N'Assign Spiral Pattern', N'Tertiary', 52, 5, N'AssignSpiralPattern', N'fa-solid fa-calendar-check', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (186, N'Employee Shift View', N'Tertiary', 52, 6, N'EmployeeShiftView', N'fas fa-eye', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (191, N'Leave Approval ', N'Tertiary', 53, 1, N'LeaveApprovalDecline', N'fas fa-hand-point-right	', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (192, N'Apply Leave ', N'Tertiary', 53, 2, N'LeaveRequest', N'fas fa-hand-point-right	', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (193, N'Leave Balance', N'Tertiary', 53, 3, N'LeaveBalance', N'fas fa-hand-point-right', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (194, N'Leave History', N'Tertiary', 53, 4, N'LeaveHistory', N'fas fa-hand-point-right', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (201, N'Employee Salary', N'Tertiary', 71, 1, N'PayRollEmpSalary', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (202, N'My Pay Slip', N'Tertiary', 71, 2, N'PaySlipForEmp', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (203, N'Employee Benefits', N'Tertiary', 71, 3, N'EmployeeBenefits', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (204, N'Employee Allowance', N'Tertiary', 71, 4, N'PayRollEmployeesAllowance', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (301, N'Blood Group', N'Secondary', 18, 1, N'BloodGroups', N'fa-solid fa-tint', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (302, N'Country', N'Secondary', 18, 2, N'Countrys', N'fa-solid fa-flag', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (303, N'Currency', N'Secondary', 18, 3, N'Currencys', N'fa-solid fa-dollar-sign', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (304, N'Degree', N'Secondary', 18, 4, N'Degrees', N'fa-solid fa-graduation-cap', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (307, N'Education Board', N'Secondary', 18, 7, N'EducationBoards', N'fa-solid fa-university', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (308, N'Education Level', N'Secondary', 18, 8, N'EducationLevels', N'fa-solid fa-layer-group', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (309, N'Employee Type', N'Secondary', 18, 9, N'EmployeeTypes', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (310, N'Employment Nature', N'Secondary', 18, 10, N'EmploymentNatures', N'fa-solid fa-briefcase', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (311, N'Gender', N'Secondary', 18, 11, N'Genders', N'fa-solid fa-venus-mars', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (312, N'Grade', N'Secondary', 18, 12, N'Grades', N'fa-solid fa-signal', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (313, N'Marital Status', N'Secondary', 18, 13, N'MaritalStatus', N'fa-solid fa-heart', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (314, N'Payment Mode', N'Secondary', 18, 14, N'PaymentModes', N'fa-solid fa-credit-card', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (315, N'Payment Period', N'Secondary', 18, 15, N'PaymentPeriods', N'fa-solid fa-calendar-alt', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (316, N'Religion', N'Secondary', 18, 16, N'Religions', N'fa-solid fa-pray', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (317, N'Status', N'Secondary', 18, 18, N'Status', N'fa-solid fa-toggle-on', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (318, N'Licence Type', N'Secondary', 18, 19, N'LicenceType', N'fa-solid fa-id-card', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (320, N'Passing Year', N'Secondary', 18, 21, N'PassingYear', N'fa-solid fa-calendar-check', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (321, N'Provision Period', N'Secondary', 18, 22, N'ProvisionPeriodTime', N'fa-solid fa-hourglass-half', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (322, N'Result Type', N'Secondary', 18, 23, N'ResultType', N'fa-solid fa-percentage', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (323, N'Service Year', N'Secondary', 18, 24, N'ServiceYear', N'fa-solid fa-cogs', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (324, N'Training Year', N'Secondary', 18, 25, N'TrainingYear', N'fa-solid fa-chalkboard-teacher', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (325, N'Yearly End Bonus', N'Secondary', 18, 26, N'YearlyEndBonusType', N'fa-solid fa-gift', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (326, N'Service Type', N'Secondary', 18, 26, N'Services', N'fa-solid fa-certificate', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (327, N'Priority', N'Secondary', 18, 27, N'Priorities', N'fa-solid fa-up-right-and-down-left-from-center', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (328, N'Lead Source', N'Secondary', 18, 28, N'LeadSources', N'fa-brands fa-sourcetree', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (329, N'Lead Status', N'Secondary', 18, 29, N'LeadStatuses', N'fa-solid fa-thermometer', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (401, N'Official Info', N'Secondary', 3, 3, N'EmployeeOfficial', N'fa-solid fa-briefcase', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (402, N'Employee Salary Settings', N'Secondary', 3, 4, N'EmployeeSalary', N'fa-solid fa-money-check', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (403, N'Employee Benefits', N'Secondary', 3, 5, N'EmployeeBenifit', N'fa-solid fa-hand-holding-usd', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (404, N'Employee Allowance', N'Secondary', 3, 6, N'EmployeeAllowance', N'fa-solid fa-coins', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (405, N'Additional Info.', N'Secondary', 3, 7, N'EmployeeAdditional', N'fa-solid fa-info-circle', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (406, N'Education Info.', N'Secondary', 3, 8, N'EmployeeEducation', N'fa-solid fa-book-reader', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (407, N'Tranning Info.', N'Secondary', 3, 9, N'EmployeeTraining', N'fa-solid fa-chalkboard-teacher', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (408, N'Family Info.', N'Secondary', 3, 10, N'EmployeeFamily', N'fa-solid fa-users', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (409, N'Emergency Contact', N'Secondary', 3, 11, N'EmployeeContact', N'fa-solid fa-phone', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (410, N'Off Day Roster', N'Tertiary', 52, 0, N'OffDayRoster', N'fa-solid fa-users-cog', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (501, N'Chart of Account', N'Secondary', 7, 1, N'ChartOfAccount', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (502, N'Add Journal', N'Secondary', 7, 2, N'AddJournal', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (503, N'Master Setup', N'Secondary', 7, 3, N'MasterSetup', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (504, N'Contra Entry', N'Secondary', 7, 4, N'ContraEntry', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (505, N'Opening Balance', N'Secondary', 7, 5, N'OpeningBalance', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (506, N'OB Adjustment', N'Secondary', 7, 6, N'ObAdjustment', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (507, N'Cash Receipt', N'Secondary', 7, 7, N'CashReceipt', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (508, N'Cash Payment', N'Secondary', 7, 8, N'CashPayment', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (509, N'Bank Receipt', N'Secondary', 7, 9, N'BankReceipt', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (510, N'Bank Payment', N'Secondary', 7, 10, N'BankPayment', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (601, N'Add Main Account', N'Tertiary', 501, 1, N'AddMainAccount', N'fa-solid fa-users-cog', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (602, N'Add Sub Account', N'Tertiary', 501, 2, N'AddSubAccount', N'fa-solid fa-users-cog', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (603, N'Transaction Account', N'Tertiary', 501, 1, N'TransactionAccount', N'fa-solid fa-users-cog', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (13070, N'Master Setup Category', N'Primary', NULL, 8, N'MasterSetupCategory', N'layers', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (13071, N' Tax Percentage', N'Tertiary', 85, 1, N'PayRollTaxPercentageSettigns', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (13072, N'Allowance Type', N'Tertiary', 85, 2, N'EmpAllowanceOrganization', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (13073, N'Loan Entry', N'Tertiary', 72, 1, N'PayRollLoanEntry', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (13074, N'Loan View', N'Tertiary', 72, 2, N'PayRollLoanView', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (14073, N'Create User', N'Secondary', 20, 10, N'UserList', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (15076, N'Early Payment', N'Tertiary', 72, 3, N'PayRollEarlyPayment', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (15077, N'Posting Rules', N'Tertiary', 501, 4, N'PostingRules', N'fa-solid fa-users-cog', 0)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (16077, N'POS', N'Primary', NULL, 3, N'333', N'archive', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (16078, N'Product Management', N'Secondary', 16077, 1, N'555', N'fas fa-shopping-cart	', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (16079, N'Add Product', N'Tertiary', 16078, 1, N'SingleProduct', N'fas fa-plus-circle', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (17077, N'Activity Types', N'Secondary', 18, 30, N'LeadActivityTypes', N'fa-solid fa-chart-line', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (17078, N'Organization Types', N'Secondary', 18, 31, N'OrganizationTypes', N'fa-solid fa-sitemap', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (18077, N'Customers', N'Secondary', 18, 32, N'Customers', N'fa-regular fa-user', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (19077, N'Field Services', N'Primary', NULL, 0, NULL, N'fa-solid fa-truck-field', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (19078, N'Create Job', N'Secondary', 19077, 1, N'CreateJobs', N'fa-solid fa-plus', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (19079, N'JobLists', N'Secondary', 19077, 2, N'JobLists', N'fa-solid fa-list-check', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (19080, N'Job Types', N'Secondary', 18, 1, N'JobTypes', N'fa-solid fa-font-awesome', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (20077, N'JobDetails', N'Secondary', 19077, 3, N'JobDetails', N'fa-solid fa-asterisk', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (21077, N'Sales', N'Secondary', 16077, 2, N'55', N'fas fa-file-invoice', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (21078, N'Price Quotation', N'Tertiary', 21077, 1, N'PriceQuotationList', N'fas fa-file-invoice-dollar', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (21079, N'Sales Order', N'Tertiary', 21077, 2, N'SalesOrderLIst', N'fas fa-chart-pie', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (21080, N'Create Invoice', N'Tertiary', 21077, 3, N'Invoice', N'fas fa-layer-group', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (21081, N'Invoice List', N'Tertiary', 21077, 4, N'InvoiceList', N'fas fa-user-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (22077, N'Purchase', N'Secondary', 16077, 3, N'5r4', N'fas fa-chart-pie', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (22078, N'Purchase Order', N'Tertiary', 22077, 1, N'PurchaseOrderList', N'fas fa-receipt', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (23077, N'Advanced Approval', N'Secondary', 19077, 5, N'AdvancedApproval', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (23078, N'Employee Advanced', N'Secondary', 19077, 4, N'EmployeeAdvanced', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (23079, N'Employee Claim', N'Secondary', 19077, 6, N'EmployeeClaim', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (23080, N'Claim Approval', N'Secondary', 19077, 7, N'ClaimApproval', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (23081, N'Purchase Receive', N'Tertiary', 22077, 2, N'PurchaseReceive', N'fas fa-shopping-cart', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (23082, N'POS Settings', N'Secondary', 16077, 7, N'gg', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (23083, N'Approval Matrix', N'Tertiary', 23082, 1, N'ApprovalMatrix', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (23084, N'Requisition', N'Secondary', 16077, 4, N're', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (24082, N'Create Requisition', N'Tertiary', 23084, 1, N'Requisition', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (25082, N'Requisition Approve', N'Tertiary', 23084, 2, N'RequisitionApprover', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (25083, N'Approved Requisition List', N'Tertiary', 23084, 3, N'RequisitionToPurchaseOrder', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (25084, N'All Requisition List', N'Tertiary', 23084, 4, N'AllRequisitionList', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (26082, N'Inventory Managemet', N'Secondary', 16077, 5, N'invMa', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (26083, N'Inventory List', N'Tertiary', 26082, 1, N'Inventory', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (27082, N'Challan', N'Tertiary', 21077, 5, N'ChallanList', N'fa-solid fa-users-cog', 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (28082, N'Inventory DashBoard', N'Tertiary', 26082, 2, N'InventoryDashboard', NULL, 1)
GO
INSERT [dbo].[MenuTab] ([MenuTabId], [Title], [Type], [ParentId], [OrderBy], [ControllerName], [Icon], [IsActive]) VALUES (28083, N'Inventory Report', N'Tertiary', 26082, 3, N'InventoryReport', NULL, 1)
GO
SET IDENTITY_INSERT [dbo].[MenuTab] OFF
GO
ALTER TABLE [dbo].[MenuTab]  WITH CHECK ADD  CONSTRAINT [FK_MenuTab_MenuTab_ParentId] FOREIGN KEY([ParentId])
REFERENCES [dbo].[MenuTab] ([MenuTabId])
GO
ALTER TABLE [dbo].[MenuTab] CHECK CONSTRAINT [FK_MenuTab_MenuTab_ParentId]
GO
