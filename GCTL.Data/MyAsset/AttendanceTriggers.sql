    -----------------------------------
    -- Trigger for Attendance Insert/Update
    -----------------------------------

    USE [fingerprint]
    GO
    /****** Object:  Trigger [dbo].[ProcessPunchToHRMApp]    Script Date: 9/20/2025 10:58:48 AM ******/
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
     
        DECLARE @enroll_id INT;
        DECLARE @CHECKTIME DATETIME2;
        DECLARE @DeviceSN VARCHAR(50);
     
        SELECT 
            @enroll_id = enroll_id,
            @CHECKTIME = records_time,
            @DeviceSN = device_serial_num
        FROM inserted;
     
        EXEC [HRM_DB_GCTL].[dbo].[sp_ProcessPunch] 
             @enroll_id = @enroll_id,
             @CHECKTIME = @CHECKTIME,
             @DeviceSN = @DeviceSN;
    END





    -----------------------------------
    -- Procedure
    -----------------------------------

    --USE [HRM_DB_GCTL]
    GO
    /****** Object:  StoredProcedure [dbo].[sp_ProcessPunch]    Script Date: 10/8/2025 12:36:23 PM ******/
    SET ANSI_NULLS ON
    GO
    SET QUOTED_IDENTIFIER ON
    GO
    
    ALTER PROCEDURE [dbo].[sp_ProcessPunch]
        @enroll_id INT,
        @CHECKTIME DATETIME2,
        @DeviceSN VARCHAR(50),
        @SourceType VARCHAR(20) = 'Device',
        @Latitude VARCHAR(100) = '',
        @Longitude VARCHAR(100) = ''
    AS
    BEGIN
        SET NOCOUNT ON;
    
        ----------------------------------------------------------------
        -- VARIABLES
        ----------------------------------------------------------------
        DECLARE @CHECKTIME_UTC DATETIMEOFFSET;
        DECLARE @EmpAttCode VARCHAR(3);
        DECLARE @EmployeeID INT;
        DECLARE @AttendanceID INT;
        DECLARE @DefaultShiftID INT;
        DECLARE @OfficeTimeMinutes INT;
        DECLARE @LateTimeMinutes INT;
        DECLARE @EarlyTimeMinutes INT;
        DECLARE @WorkingTimeMinutes INT;
        DECLARE @OvertimeMinutes INT;
        DECLARE @BreakTimeMinutes INT;
        DECLARE @EarlyOutTimeMinutes INT;
        DECLARE @LateOutTimeMinutes INT;
        DECLARE @ShiftStartTime TIME;
        DECLARE @ShiftEndTime TIME;
        DECLARE @GraceTime INT;
        DECLARE @IsLateCount BIT;
        DECLARE @IsAllowOvertime BIT;
        DECLARE @MinimumWorkingTime INT; 
        DECLARE @MinimumRequiredOvertime INT; 
        DECLARE @MaximumAllowedOvertime INT; 
        DECLARE @ShiftStartToday DATETIME2;
        DECLARE @ShiftEndToday DATETIME2;
        DECLARE @PunchCount INT;
        DECLARE @LatestPunch DATETIME2;
        DECLARE @LatestPunchUTC DATETIMEOFFSET;
    
        ----------------------------------------------------------------
        -- normalize punch time to UTC and find employee
        ----------------------------------------------------------------
        SET @CHECKTIME_UTC = (@CHECKTIME AT TIME ZONE 'Bangladesh Standard Time') AT TIME ZONE 'UTC';
        SET @EmpAttCode = FORMAT(@enroll_id, '000');
    
        SELECT TOP 1 @EmployeeID = EOF.EmployeeID
        FROM [HRM_DB_GCTL].[dbo].[EmployeeOfficeInfo] EOF
        WHERE EOF.AttendanceId = CAST(@EmpAttCode AS INT);
    
        IF @EmployeeID IS NULL RETURN;
    
        ----------------------------------------------------------------
        -- find today's attendance (use date of incoming punch)
        ----------------------------------------------------------------
        SELECT TOP 1 @AttendanceID = Att.AttendanceID
        FROM [HRM_DB_GCTL].[dbo].[Attendance] Att
        WHERE CAST(Att.AttendanceDate AS DATE) = CAST(@CHECKTIME AS DATE)
          AND Att.EmployeeID = @EmployeeID
        ORDER BY Att.AttendanceID DESC;
    
        ----------------------------------------------------------------
        -- Decide employee's shift id (IMPORTANT: always determine ShiftID
        -- so we can load shift rules whether attendance already exists or not)
        ----------------------------------------------------------------
        IF @AttendanceID IS NOT NULL AND @AttendanceID > 0
        BEGIN
            -- if attendance exists take its ShiftID (in case shift was saved on attendance)
            SELECT TOP 1 @DefaultShiftID = ShiftID
            FROM [HRM_DB_GCTL].[dbo].[Attendance]
            WHERE AttendanceID = @AttendanceID;
        END
        ELSE
        BEGIN
            -- otherwise, get user's default shift
            SELECT TOP 1 @DefaultShiftID = DS.ShiftID
            FROM [HRM_DB_GCTL].[dbo].[DefaultShifts] DS
            WHERE DS.EmployeeID = @EmployeeID;
        END
    
        ----------------------------------------------------------------
        -- load shift rules (convert TIME columns -> minutes INT)
        -- handle potential overnight shifts for OfficeTimeMinutes
        ----------------------------------------------------------------
        SELECT TOP 1
            @ShiftStartTime = S.StartTime
            ,@ShiftEndTime   = S.EndTime
            ,@GraceTime      = S.GraceTime
            ,@IsLateCount    = S.IsLateCount
            ,@IsAllowOvertime= S.IsAllowOvertime
            -- convert TIME -> minutes by casting to datetime2 anchored at 1900-01-01
            ,@MinimumWorkingTime = S.MinimumWorkingTime
            ,@MinimumRequiredOvertime = S.MinimumRequiredOvertime
            ,@MaximumAllowedOvertime = S.MaximumAllowedOvertime
            -- OfficeTimeMinutes: handle if EndTime < StartTime (overnight) by adding 1 day to EndTime
            ,@OfficeTimeMinutes = CASE
                WHEN S.EndTime >= S.StartTime
                    THEN DATEDIFF(MINUTE, CAST(S.StartTime AS DATETIME2), CAST(S.EndTime AS DATETIME2))
                ELSE DATEDIFF(MINUTE, CAST(S.StartTime AS DATETIME2), DATEADD(day,1,CAST(S.EndTime AS DATETIME2)))
            END
        FROM [HRM_DB_GCTL].[dbo].[Shifts] S
        WHERE S.ShiftID = @DefaultShiftID;
    
        ----------------------------------------------------------------
        -- If attendance exists -> append AttendanceLog (skip duplicates within 60s)
        -- otherwise create Attendance + first AttendanceLog
        ----------------------------------------------------------------
        IF @AttendanceID > 0
        BEGIN
            DECLARE @LastPunchTime DATETIME2;
            SELECT TOP 1 @LastPunchTime = PunchTime
            FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
            WHERE AttendanceID = @AttendanceID
            ORDER BY PunchTime DESC;
    
            IF @LastPunchTime IS NULL OR DATEDIFF(SECOND, @LastPunchTime, @CHECKTIME) > 60
            BEGIN
                INSERT INTO [HRM_DB_GCTL].[dbo].[AttendanceLog] ([AttendanceID],[PunchTime],[SourceType],[DeviceSN],[CHECKTIME_UTC])
                VALUES (@AttendanceID, @CHECKTIME, @SourceType, @DeviceSN, CAST(@CHECKTIME_UTC AS DATETIME2));
            END
        END
        ELSE
        BEGIN
            -- Combine shift start time with the punch date (use UTC punch's DATE to be consistent)
            SET @ShiftStartToday = DATEADD(MINUTE, DATEDIFF(MINUTE, 0, @ShiftStartTime), CAST(CAST(@CHECKTIME_UTC AS DATE) AS DATETIME2));
    
            -- Late and early calculation for first punch
            SET @LateTimeMinutes = 0;
            SET @EarlyTimeMinutes = 0;
    
            IF @IsLateCount = 1
               AND @CHECKTIME_UTC > DATEADD(MINUTE, @GraceTime, @ShiftStartToday)
            BEGIN
                -- late counted from the shift START (not from grace)
                SET @LateTimeMinutes = DATEDIFF(MINUTE, @ShiftStartToday, @CHECKTIME_UTC);
            END
            ELSE IF CAST(@CHECKTIME_UTC AS TIME) < @ShiftStartTime
            BEGIN
                SET @EarlyTimeMinutes = DATEDIFF(MINUTE, @CHECKTIME_UTC, @ShiftStartToday);
            END
    
            INSERT INTO [HRM_DB_GCTL].[dbo].[Attendance] ([EmployeeID],[AttendanceDate],[CheckInTime],[ShiftID],[OfficeTimeMinutes],[LateTimeMinutes],[EarlyTimeMinutes])
            VALUES (@EmployeeID, CAST(@CHECKTIME AS DATE), CAST(@CHECKTIME_UTC AS DATETIME2), @DefaultShiftID, @OfficeTimeMinutes, @LateTimeMinutes, @EarlyTimeMinutes);
    
            SET @AttendanceID = SCOPE_IDENTITY();
    
            INSERT INTO [HRM_DB_GCTL].[dbo].[AttendanceLog] ([AttendanceID],[PunchTime],[SourceType],[DeviceSN],[CHECKTIME_UTC])
            VALUES (@AttendanceID, @CHECKTIME, @SourceType, @DeviceSN, CAST(@CHECKTIME_UTC AS DATETIME2));
        END
    
        ----------------------------------------------------------------
        -- Recompute latest punch info (count + latest)
        ----------------------------------------------------------------
        SELECT @PunchCount = COUNT(*), @LatestPunch = MAX(PunchTime)
        FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
        WHERE AttendanceID = @AttendanceID
          AND CAST(PunchTime AS DATE) = CAST(@CHECKTIME AS DATE);
    
        IF @LatestPunch IS NOT NULL
            SET @LatestPunchUTC = (@LatestPunch AT TIME ZONE 'Bangladesh Standard Time') AT TIME ZONE 'UTC';
    
        ----------------------------------------------------------------
        -- build IN/OUT pairs and compute WorkingTimeMinutes & BreakTimeMinutes
        -- and early/late out using the Pairs' OutTime values
        ----------------------------------------------------------------
    	SELECT TOP 1
    	    @DefaultShiftID = DS.ShiftID,
    	    @ShiftEndTime = S.EndTime
    	FROM [HRM_DB_GCTL].[dbo].[DefaultShifts] DS
    	JOIN [HRM_DB_GCTL].[dbo].[Shifts] S ON DS.ShiftID = S.ShiftID
    	WHERE DS.EmployeeID = @EmployeeID;
    	
    	-- Compute shift end datetime for today
    	SET @ShiftEndToday = DATEADD(MINUTE, DATEDIFF(MINUTE, 0, @ShiftEndTime), CAST(CAST(@CHECKTIME_UTC AS DATE) AS DATETIME2));
    	   
    	;WITH Punches AS (
    	    SELECT 
    	        ROW_NUMBER() OVER (ORDER BY CHECKTIME_UTC) AS RN,
    	        CHECKTIME_UTC
    	    FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
    	    WHERE AttendanceID = @AttendanceID
    	),
    	Pairs AS (
    	    SELECT 
    	        INP.CHECKTIME_UTC AS InTime,
    	        OUTP.CHECKTIME_UTC AS OutTime
    	    FROM Punches INP
    	    JOIN Punches OUTP ON OUTP.RN = INP.RN + 1
    	    WHERE INP.RN % 2 = 1
    	),
    	Breaks AS (
    	    SELECT 
    	        OUTP.CHECKTIME_UTC AS OutTime,
    	        INP.CHECKTIME_UTC AS NextInTime
    	    FROM Punches OUTP
    	    JOIN Punches INP ON INP.RN = OUTP.RN + 1
    	    WHERE OUTP.RN % 2 = 0
    	)
    	SELECT 
    	    @WorkingTimeMinutes = ISNULL(SUM(DATEDIFF(MINUTE, InTime, OutTime)), 0)
    	    ,@BreakTimeMinutes = ISNULL((SELECT SUM(DATEDIFF(MINUTE, OutTime, NextInTime)) FROM Breaks), 0)
    	    ,@EarlyOutTimeMinutes = ISNULL((SELECT TOP 1 CASE WHEN OutTime < @ShiftEndToday THEN DATEDIFF(MINUTE, OutTime, @ShiftEndToday) ELSE 0 END FROM Pairs ORDER BY OutTime DESC), 0)
    	    ,@LateOutTimeMinutes = ISNULL(MAX(CASE WHEN OutTime > @ShiftEndToday THEN DATEDIFF(MINUTE, @ShiftEndToday, OutTime) END), 0)
    	FROM Pairs;
    
        ----------------------------------------------------------------
        -- OVERTIME calculation (all compared in minutes ints)
        ----------------------------------------------------------------
        SET @OvertimeMinutes = 0;
    
        IF @IsAllowOvertime = 1
           AND @WorkingTimeMinutes >= ISNULL(@MinimumWorkingTime, 0)
           AND (@WorkingTimeMinutes - ISNULL(@MinimumWorkingTime, 0)) >= ISNULL(@MinimumRequiredOvertime, 0)
        BEGIN
            SET @OvertimeMinutes = @WorkingTimeMinutes - ISNULL(@MinimumWorkingTime, 0);
    
            -- cap to maximum allowed if configured (>0)
            IF ISNULL(@MaximumAllowedOvertime, 0) > 0 AND @OvertimeMinutes > @MaximumAllowedOvertime
                SET @OvertimeMinutes = @MaximumAllowedOvertime;
        END
    
        ----------------------------------------------------------------
        -- final update to Attendance
        ----------------------------------------------------------------
        UPDATE [HRM_DB_GCTL].[dbo].[Attendance]
        SET 
            CheckOutTime = CASE WHEN @PunchCount >= 2 THEN CAST(@LatestPunchUTC AS DATETIME2) ELSE NULL END,
            WorkingTimeMinutes = @WorkingTimeMinutes,
            OvertimeMinutes = @OvertimeMinutes,
            BreakTimeMinutes = @BreakTimeMinutes,
            EarlyOutTimeMinutes = @EarlyOutTimeMinutes,
            LateOutTimeMinutes  = @LateOutTimeMinutes
        WHERE AttendanceID = @AttendanceID;
    
        -- 
        DECLARE @LastPunchRowNumber INT;
        DECLARE @InTime BIT = 0;
        DECLARE @OutTime BIT = 0;
        
        ;WITH Punches AS (
            SELECT 
                ROW_NUMBER() OVER (ORDER BY CHECKTIME_UTC) AS RN,
                CHECKTIME_UTC
            FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
            WHERE AttendanceID = @AttendanceID
        )
        SELECT @LastPunchRowNumber = MAX(RN)
        FROM Punches;
        
        IF @LastPunchRowNumber IS NOT NULL
        BEGIN
            IF @LastPunchRowNumber % 2 = 1
            BEGIN
                -- Odd = In
                SET @InTime = 1;
                SET @OutTime = 0;
            END
            ELSE
            BEGIN
                -- Even = Out
                SET @InTime = 0;
                SET @OutTime = 1;
            END
        END
    
        -- Return result for frontend
        SELECT 
        @InTime AS InTime,
        @OutTime AS OutTime
    
        -- Result 2: Punch list with SlNo, AttendenceType, and PunchTime
        ;WITH Punches AS (
            SELECT 
                ROW_NUMBER() OVER (ORDER BY CHECKTIME_UTC) AS SlNo,
                CHECKTIME_UTC,
                ROW_NUMBER() OVER (ORDER BY CHECKTIME_UTC) % 2 AS TypeValue
            FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
            WHERE AttendanceID = @AttendanceID
        )
        SELECT 
            SlNo,
            CASE WHEN TypeValue = 1 THEN 'IN' ELSE 'OUT' END AS AttendenceType,
            CHECKTIME_UTC AT TIME ZONE 'UTC' AT TIME ZONE 'Bangladesh Standard Time' AS PunchTime
        FROM Punches
        ORDER BY SlNo;
    END;
