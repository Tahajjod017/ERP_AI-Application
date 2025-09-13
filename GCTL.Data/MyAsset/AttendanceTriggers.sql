USE [fingerprint]
GO
/****** Object:  Trigger [dbo].[ProcessPunchToHRMApp] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Trigger is fired AFTER INSERT on the 'records' table (device punches)
ALTER TRIGGER [dbo].[ProcessPunchToHRMApp]
ON [dbo].[records]  
AFTER INSERT 
AS
BEGIN
    SET NOCOUNT ON; -- Prevents extra messages about affected rows

    -----------------------------------
    -- DECLARE VARIABLES
    -----------------------------------
    DECLARE @enroll_id INT;             -- Employee's enroll/device id from the inserted punch
    DECLARE @CHECKTIME DATETIME2;       -- Local time of punch from device
    DECLARE @CHECKTIME_UTC DATETIMEOFFSET; -- UTC equivalent of punch
    DECLARE @DeviceSN VARCHAR(50);      -- Device serial number
    DECLARE @EmpAttCode VARCHAR(3);     -- Formatted Employee Attendance Code (3-digit)
    DECLARE @EmployeeID INT;            -- EmployeeID in HRM system
    DECLARE @AttendanceID INT;          -- AttendanceID for today
    DECLARE @DefaultShiftID INT;        -- Employee's default shift
    DECLARE @RegularHour INT;           -- Shift regular duration in minutes
    DECLARE @LateHour INT;              -- Minutes late (if any)
    DECLARE @EarlyHour INT;             -- Minutes early (if any)
    DECLARE @WorkingHour INT;           -- Total worked minutes
    DECLARE @OvertimeHour INT;          -- Minutes of overtime
    DECLARE @ShiftStartTime TIME;       -- Shift start time (from shift master)
    DECLARE @ShiftEndTime TIME;         -- Shift end time
    DECLARE @GraceTime TIME;            -- Grace period for late counting
    DECLARE @IsLateCount BIT;           -- Flag if late counting is active
    DECLARE @IsAllowOvertime BIT;       -- Flag if overtime allowed
    DECLARE @MinimumWorkingTime INT;    -- Minimum minutes to count as working
    DECLARE @MinimumRequiredOvertime INT; -- Minimum minutes required to consider OT
    DECLARE @ShiftStartWithGrace DATETIME2; -- Shift start with grace, converted to datetime2

    -----------------------------------
    -- GET PUNCH DATA
    -----------------------------------
    SELECT 
        @enroll_id = enroll_id,
        @CHECKTIME = records_time,
        @DeviceSN = device_serial_num
    FROM inserted; -- 'inserted' is a pseudo-table containing new rows

    -- Convert local punch to UTC for HRM consistency
    SET @CHECKTIME_UTC = (@CHECKTIME AT TIME ZONE 'Bangladesh Standard Time') AT TIME ZONE 'UTC';

    -- Format enrollment ID to 3-digit code (e.g., 1 -> '001')
    SET @EmpAttCode = FORMAT(@enroll_id, '000');

    -- Map employee via AttendanceId
    SELECT TOP 1 
        @EmployeeID = EOF.EmployeeID
    FROM [HRM_DB_GCTL].[dbo].[EmployeeOfficeInfo] EOF 
    WHERE EOF.AttendanceId = CAST(@EmpAttCode AS INT);

    -- Exit if no employee found
    IF @EmployeeID IS NULL RETURN;

    -----------------------------------
    -- CHECK FOR TODAY'S ATTENDANCE
    -----------------------------------
    SELECT TOP 1 
        @AttendanceID = Att.AttendanceID
    FROM [HRM_DB_GCTL].[dbo].[Attendance] Att
    --WHERE CAST(Att.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
    WHERE CAST(Att.AttendanceDate AS DATE) = CAST(@CHECKTIME AS DATE)
        AND Att.EmployeeID = @EmployeeID
    ORDER BY Att.AttendanceID DESC;

    IF @AttendanceID > 0
    BEGIN
        -----------------------------------
        -- ALREADY EXISTS: HANDLE DUPLICATE PUNCH
        -----------------------------------
        DECLARE @LastPunchTime DATETIME2;

        SELECT TOP 1 @LastPunchTime = PunchTime
        FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
        WHERE AttendanceID = @AttendanceID
        ORDER BY PunchTime DESC;

        -- Only insert if last punch > 60 seconds ago
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
        -----------------------------------
        -- FIRST PUNCH: CREATE ATTENDANCE RECORD
        -----------------------------------
        -- Get employee's default shift
        SELECT TOP 1 
            @DefaultShiftID = DS.ShiftID
        FROM [HRM_DB_GCTL].[dbo].[DefaultShifts] DS
        WHERE DS.EmployeeID = @EmployeeID;

        -- Fetch shift timings and rules
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

        -- Initialize late/early minutes
        SET @LateHour = 0;
        SET @EarlyHour = 0;

        -----------------------------------
        -- COMBINE SHIFT TIME WITH TODAY'S DATE
        -----------------------------------
        DECLARE @ShiftStartToday DATETIME2;
        --SET @ShiftStartToday = DATEADD(MINUTE, DATEDIFF(MINUTE, 0, @ShiftStartTime), CAST(CAST(GETDATE() AS DATE) AS DATETIME2));
        SET @ShiftStartToday = DATEADD(MINUTE, DATEDIFF(MINUTE, 0, @ShiftStartTime), CAST(CAST(@CHECKTIME AS DATE) AS DATETIME2));

        -- LateHour: punch after shift start + grace
        IF @IsLateCount = 1 AND @CHECKTIME > DATEADD(MINUTE, DATEDIFF(MINUTE, 0, @GraceTime), @ShiftStartToday)
        BEGIN
            SET @LateHour = DATEDIFF(MINUTE, @ShiftStartToday, @CHECKTIME); 
            -- Difference in minutes from shift start (without grace) to punch
        END

        -- EarlyHour: punch before shift start
        IF CAST(@CHECKTIME AS TIME) < @ShiftStartTime
        BEGIN
            SET @EarlyHour = DATEDIFF(MINUTE, @CHECKTIME, @ShiftStartToday);
            -- Difference in minutes from punch to shift start
        END

        -- Insert first attendance record
        INSERT INTO [HRM_DB_GCTL].[dbo].[Attendance] 
        ([EmployeeID], [AttendanceDate], [CheckInTime], [ShiftID], [RegularHour], [LateHour], [EarlyHour])
        VALUES
        --(@EmployeeID, CAST(GETDATE() AS DATE), CAST(@CHECKTIME_UTC AS DATETIME2), @DefaultShiftID, @RegularHour, @LateHour, @EarlyHour);
        (@EmployeeID, CAST(@CHECKTIME AS DATE), CAST(@CHECKTIME_UTC AS DATETIME2), @DefaultShiftID, @RegularHour, @LateHour, @EarlyHour);

        SET @AttendanceID = SCOPE_IDENTITY();

        -- Insert first punch log
        INSERT INTO [HRM_DB_GCTL].[dbo].[AttendanceLog]
        ([AttendanceID], [PunchTime], [SourceType], [DeviceSN], [CHECKTIME_UTC])
        VALUES
        (@AttendanceID, @CHECKTIME, 'Device', @DeviceSN, CAST(@CHECKTIME_UTC AS DATETIME2));
    END

    -----------------------------------
    -- CALCULATE TOTAL WORKING HOURS
    -----------------------------------
    DECLARE @PunchCount INT;
    DECLARE @LatestPunch DATETIME2;
    DECLARE @LatestPunchUTC DATETIMEOFFSET;

    SELECT 
        @PunchCount = COUNT(*),
        @LatestPunch = MAX(PunchTime)
    FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
    --WHERE AttendanceID = @AttendanceID AND CAST(PunchTime AS DATE) = CAST(GETDATE() AS DATE);
    WHERE AttendanceID = @AttendanceID AND CAST(PunchTime AS DATE) = CAST(@CHECKTIME AS DATE);

    -- Convert latest punch to UTC
    IF @LatestPunch IS NOT NULL
    BEGIN
        SET @LatestPunchUTC = (@LatestPunch AT TIME ZONE 'Bangladesh Standard Time') AT TIME ZONE 'UTC';
    END

    -----------------------------------
    -- Calculate WorkingHour using punch pairs
    -----------------------------------
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
            -- Calculate difference between IN and OUT punches in minutes
            SET @TotalMinutes += DATEDIFF(MINUTE, @PunchIn, @PunchOut);
            FETCH NEXT FROM PunchCursor INTO @PunchIn;
        END
    END

    CLOSE PunchCursor;
    DEALLOCATE PunchCursor;

    SET @WorkingHour = @TotalMinutes;

    -----------------------------------
    -- Calculate OvertimeHour if allowed
    -----------------------------------
    SET @OvertimeHour = 0; -- default

    IF @IsAllowOvertime = 1 
       AND @WorkingHour >= @MinimumWorkingTime
       AND (@WorkingHour - @RegularHour) >= @MinimumRequiredOvertime
    BEGIN
        SET @OvertimeHour = @WorkingHour - @RegularHour;
    END

    -----------------------------------
    -- FINAL UPDATE OF ATTENDANCE
    -----------------------------------
    UPDATE [HRM_DB_GCTL].[dbo].[Attendance]
    SET 
        CheckOutTime = CASE WHEN @PunchCount >= 2 THEN CAST(@LatestPunchUTC AS DATETIME2) ELSE NULL END,
        WorkingHour = @WorkingHour,       -- total minutes worked
        OvertimeHour = @OvertimeHour,     -- minutes of OT if any
        LateHour = @LateHour,             -- minutes late
        EarlyHour = @EarlyHour            -- minutes early
    WHERE AttendanceID = @AttendanceID;

END;
