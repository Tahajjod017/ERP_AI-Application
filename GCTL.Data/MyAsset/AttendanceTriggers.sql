USE [fingerprint]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER TRIGGER [dbo].[ProcessPunchToHRMApp]
ON [dbo].[records]  
AFTER INSERT 
AS
BEGIN
    SET NOCOUNT ON;

    -- Declare variables
    DECLARE @enroll_id INT;
    DECLARE @CHECKTIME DATETIME2; 
    DECLARE @CHECKTIME_UTC DATETIMEOFFSET;
    DECLARE @DeviceSN VARCHAR(50);
    DECLARE @EmpAttCode VARCHAR(3);
    DECLARE @EmployeeID INT;
    DECLARE @AttendanceID INT;
    DECLARE @DefaultShiftID INT;
    DECLARE @RegularHour INT;
    DECLARE @LateHour INT;
    DECLARE @EarlyHour INT;
    DECLARE @WorkingHour INT;
    DECLARE @OvertimeHour INT;
    DECLARE @ShiftStartTime TIME;
    DECLARE @ShiftEndTime TIME;
    DECLARE @GraceTime TIME;
    DECLARE @IsLateCount BIT;
    DECLARE @IsAllowOvertime BIT;
    DECLARE @MinimumWorkingTime INT;
    DECLARE @MinimumRequiredOvertime INT;
    DECLARE @ShiftStartWithGrace DATETIME2;

    -- Get punch data
    SELECT 
        @enroll_id = enroll_id,
        @CHECKTIME = records_time,
        @DeviceSN = device_serial_num
    FROM inserted;

    -- Convert punch to UTC
    SET @CHECKTIME_UTC = (@CHECKTIME AT TIME ZONE 'Bangladesh Standard Time') AT TIME ZONE 'UTC';

    SET @EmpAttCode = FORMAT(@enroll_id, '000');

    -- Get EmployeeID
    SELECT TOP 1 
        @EmployeeID = EOF.EmployeeID
    FROM [HRM_DB_GCTL].[dbo].[EmployeeOfficeInfo] EOF 
    WHERE EOF.AttendanceId = CAST(@EmpAttCode AS INT);

    -- Exit if no employee found
    IF @EmployeeID IS NULL RETURN;

    -- Check for existing attendance
    SELECT TOP 1 
        @AttendanceID = Att.AttendanceID
    FROM [HRM_DB_GCTL].[dbo].[Attendance] Att
    WHERE CAST(Att.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
        AND Att.EmployeeID = @EmployeeID
    ORDER BY Att.AttendanceID DESC;

    IF @AttendanceID > 0
    BEGIN
        -- Prevent duplicate punch within 60 seconds
        DECLARE @LastPunchTime DATETIME2;

        SELECT TOP 1 @LastPunchTime = PunchTime
        FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
        WHERE AttendanceID = @AttendanceID
        ORDER BY PunchTime DESC;

        IF @LastPunchTime IS NULL OR DATEDIFF(SECOND, @LastPunchTime, @CHECKTIME) > 60
        BEGIN
            INSERT INTO [HRM_DB_GCTL].[dbo].[AttendanceLog]
            ([AttendanceID], [PunchTime], [SourceType], [DeviceSN], [CHECKTIME_UTC])
            VALUES
            (@AttendanceID, @CHECKTIME, 'Device', @DeviceSN, CAST(@CHECKTIME_UTC AS DATETIME2));
        END
    END
    ELSE
    BEGIN
        -- Get Default Shift
        SELECT TOP 1 
            @DefaultShiftID = DS.ShiftID
        FROM [HRM_DB_GCTL].[dbo].[DefaultShifts] DS
        WHERE DS.EmployeeID = @EmployeeID;

        -- Get Shift info
        SELECT TOP 1
            @ShiftStartTime = S.StartTime,
            @ShiftEndTime = S.EndTime,
            @GraceTime = ISNULL(S.GraceTime, '00:00:00'),
            @IsLateCount = S.IsLateCount,
            @IsAllowOvertime = S.IsAllowOvertime,
            @MinimumWorkingTime = ISNULL(DATEDIFF(MINUTE, S.StartTime, S.EndTime), 0),
            @MinimumRequiredOvertime = ISNULL(DATEDIFF(MINUTE, 0, S.MinimumRequiredOvertime), 0),
            @RegularHour = DATEDIFF(MINUTE, S.StartTime, S.EndTime)
        FROM [HRM_DB_GCTL].[dbo].[Shifts] S
        WHERE S.ShiftID = @DefaultShiftID;

        -- Late and Early Logic
        SET @LateHour = 0;
        SET @EarlyHour = 0;

        -- Calculate shift start datetime with grace for late detection
        SET @ShiftStartWithGrace = DATEADD(MINUTE, 
            DATEDIFF(MINUTE, 0, @ShiftStartTime) + DATEDIFF(MINUTE, 0, @GraceTime),
            CAST(CAST(GETDATE() AS DATE) AS DATETIME2)
        );

        IF @IsLateCount = 1 AND @CHECKTIME > @ShiftStartWithGrace
        BEGIN
            -- LateHour calculated from StartTime (not grace)
            SET @LateHour = DATEDIFF(MINUTE, CAST(@ShiftStartTime AS DATETIME2), @CHECKTIME);
        END
        ELSE IF CAST(@CHECKTIME AS TIME) < @ShiftStartTime
        BEGIN
            SET @EarlyHour = DATEDIFF(MINUTE, @CHECKTIME, CAST(@ShiftStartTime AS DATETIME2));
        END

        -- Insert into Attendance (CheckInTime in UTC)
        INSERT INTO [HRM_DB_GCTL].[dbo].[Attendance] 
        ([EmployeeID], [AttendanceDate], [CheckInTime], [ShiftID], [RegularHour], [LateHour], [EarlyHour])
        VALUES
        (@EmployeeID, CAST(GETDATE() AS DATE), CAST(@CHECKTIME_UTC AS DATETIME2), @DefaultShiftID, @RegularHour, @LateHour, @EarlyHour);

        SET @AttendanceID = SCOPE_IDENTITY();

        -- Insert first punch
        INSERT INTO [HRM_DB_GCTL].[dbo].[AttendanceLog]
        ([AttendanceID], [PunchTime], [SourceType], [DeviceSN], [CHECKTIME_UTC])
        VALUES
        (@AttendanceID, @CHECKTIME, 'Device', @DeviceSN, CAST(@CHECKTIME_UTC AS DATETIME2));
    END

    -- Update CheckOutTime and calculate WorkingHour
    DECLARE @PunchCount INT;
    DECLARE @LatestPunch DATETIME2;
    DECLARE @LatestPunchUTC DATETIMEOFFSET;

    SELECT 
        @PunchCount = COUNT(*),
        @LatestPunch = MAX(PunchTime)
    FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
    WHERE AttendanceID = @AttendanceID AND CAST(PunchTime AS DATE) = CAST(GETDATE() AS DATE);

    -- Convert latest punch to UTC
    IF @LatestPunch IS NOT NULL
    BEGIN
        SET @LatestPunchUTC = (@LatestPunch AT TIME ZONE 'Bangladesh Standard Time') AT TIME ZONE 'UTC';
    END

    ---------------------------------------------
    -- ⏱️ Calculate WorkingHour from punch pairs
    ---------------------------------------------
    DECLARE @TotalMinutes INT = 0;
    DECLARE @PunchIn DATETIME2;
    DECLARE @PunchOut DATETIME2;

    DECLARE PunchCursor CURSOR FOR
    SELECT PunchTime
    FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
    WHERE AttendanceID = @AttendanceID
    ORDER BY PunchTime;

    OPEN PunchCursor;
    FETCH NEXT FROM PunchCursor INTO @PunchIn;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        FETCH NEXT FROM PunchCursor INTO @PunchOut;

        IF @@FETCH_STATUS = 0
        BEGIN
            SET @TotalMinutes += DATEDIFF(MINUTE, @PunchIn, @PunchOut);
            FETCH NEXT FROM PunchCursor INTO @PunchIn;
        END
    END

    CLOSE PunchCursor;
    DEALLOCATE PunchCursor;

    SET @WorkingHour = @TotalMinutes;

    ---------------------------------------------------
    -- ⏳ Calculate OvertimeHour if conditions are met
    ---------------------------------------------------
    SET @OvertimeHour = 0; -- default

    IF @IsAllowOvertime = 1 
       AND @WorkingHour >= @MinimumWorkingTime
       AND (@WorkingHour - @RegularHour) >= @MinimumRequiredOvertime
    BEGIN
        SET @OvertimeHour = @WorkingHour - @RegularHour;
    END

    -- Final update to Attendance (CheckOutTime in UTC, minutes stored)
    UPDATE [HRM_DB_GCTL].[dbo].[Attendance]
    SET 
        CheckOutTime = CASE WHEN @PunchCount >= 2 THEN CAST(@LatestPunchUTC AS DATETIME2) ELSE NULL END,
        WorkingHour = @WorkingHour,
        OvertimeHour = @OvertimeHour,
        LateHour = @LateHour,
        EarlyHour = @EarlyHour
    WHERE AttendanceID = @AttendanceID;

END;
GO
