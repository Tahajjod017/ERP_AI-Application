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
    /****** Object:  StoredProcedure [dbo].[sp_ProcessPunch]    Script Date: 09/20/25 09:50:57 AM ******/
    SET ANSI_NULLS ON
    GO
    SET QUOTED_IDENTIFIER ON
    GO
     
    ALTER PROCEDURE [dbo].[sp_ProcessPunch]
        @enroll_id INT,
        @CHECKTIME DATETIME2,
        @DeviceSN VARCHAR(50),
        @SourceType VARCHAR(20) = 'Device'
    AS
    BEGIN
        SET NOCOUNT ON;
     
        -----------------------------------
        -- DECLARE VARIABLES
        -----------------------------------
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
        DECLARE @GraceTime TIME;
        DECLARE @IsLateCount BIT;
        DECLARE @IsAllowOvertime BIT;
        DECLARE @MinimumWorkingTime INT;
        DECLARE @MinimumRequiredOvertime INT;
        DECLARE @ShiftStartToday DATETIME2;
        DECLARE @ShiftEndToday DATETIME2;
        DECLARE @PunchCount INT;
        DECLARE @LatestPunch DATETIME2;
        DECLARE @LatestPunchUTC DATETIMEOFFSET;
     
        -----------------------------------
        -- Convert local punch to UTC
        -----------------------------------
        SET @CHECKTIME_UTC = (@CHECKTIME AT TIME ZONE 'Bangladesh Standard Time') AT TIME ZONE 'UTC';
        SET @EmpAttCode = FORMAT(@enroll_id, '000');
     
        SELECT TOP 1 
            @EmployeeID = EOF.EmployeeID
        FROM [HRM_DB_GCTL].[dbo].[EmployeeOfficeInfo] EOF 
        WHERE EOF.AttendanceId = CAST(@EmpAttCode AS INT);
     
        IF @EmployeeID IS NULL RETURN;
     
        -----------------------------------
        -- CHECK FOR TODAY'S ATTENDANCE
        -----------------------------------
        SELECT TOP 1 
            @AttendanceID = Att.AttendanceID
        FROM [HRM_DB_GCTL].[dbo].[Attendance] Att
        WHERE CAST(Att.AttendanceDate AS DATE) = CAST(@CHECKTIME AS DATE)
            AND Att.EmployeeID = @EmployeeID
        ORDER BY Att.AttendanceID DESC;
     
        IF @AttendanceID > 0
        BEGIN
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
                (@AttendanceID, @CHECKTIME, @SourceType, @DeviceSN, CAST(@CHECKTIME_UTC AS DATETIME2));
            END
        END
        ELSE
        BEGIN
            -----------------------------------
            -- FIRST PUNCH: CREATE ATTENDANCE RECORD
            -----------------------------------
            SELECT TOP 1 
                @DefaultShiftID = DS.ShiftID
            FROM [HRM_DB_GCTL].[dbo].[DefaultShifts] DS
            WHERE DS.EmployeeID = @EmployeeID;
     
            SELECT TOP 1
                @ShiftStartTime = S.StartTime,
                @ShiftEndTime = S.EndTime,
                @GraceTime = ISNULL(S.GraceTime, '00:00:00'),
                @IsLateCount = S.IsLateCount,
                @IsAllowOvertime = S.IsAllowOvertime,
                @MinimumWorkingTime = ISNULL(DATEDIFF(MINUTE, S.StartTime, S.EndTime), 0),
                @MinimumRequiredOvertime = ISNULL(DATEDIFF(MINUTE, 0, S.MinimumRequiredOvertime), 0),
                @OfficeTimeMinutes = DATEDIFF(MINUTE, S.StartTime, S.EndTime)
            FROM [HRM_DB_GCTL].[dbo].[Shifts] S
            WHERE S.ShiftID = @DefaultShiftID;
     
            SET @LateTimeMinutes = 0;
            SET @EarlyTimeMinutes = 0;
            
            SET @ShiftStartToday = DATEADD(MINUTE, DATEDIFF(MINUTE, 0, @ShiftStartTime), CAST(CAST(@CHECKTIME_UTC AS DATE) AS DATETIME2));
     
            IF @IsLateCount = 1 AND @CHECKTIME_UTC > DATEADD(MINUTE, DATEDIFF(MINUTE, 0, @GraceTime), @ShiftStartToday)
            BEGIN
                SET @LateTimeMinutes = DATEDIFF(MINUTE, @ShiftStartToday, @CHECKTIME_UTC); 
            END
     
            IF CAST(@CHECKTIME_UTC AS TIME) < @ShiftStartTime
            BEGIN
                SET @EarlyTimeMinutes = DATEDIFF(MINUTE, @CHECKTIME_UTC, @ShiftStartToday);
            END
     
            INSERT INTO [HRM_DB_GCTL].[dbo].[Attendance] 
            ([EmployeeID], [AttendanceDate], [CheckInTime], [ShiftID], [OfficeTimeMinutes], [LateTimeMinutes], [EarlyTimeMinutes])
            VALUES
            (@EmployeeID, CAST(@CHECKTIME AS DATE), CAST(@CHECKTIME_UTC AS DATETIME2), @DefaultShiftID, @OfficeTimeMinutes, @LateTimeMinutes, @EarlyTimeMinutes);
     
            SET @AttendanceID = SCOPE_IDENTITY();
     
            INSERT INTO [HRM_DB_GCTL].[dbo].[AttendanceLog]
            ([AttendanceID], [PunchTime], [SourceType], [DeviceSN], [CHECKTIME_UTC])
            VALUES
            (@AttendanceID, @CHECKTIME, @SourceType, @DeviceSN, CAST(@CHECKTIME_UTC AS DATETIME2));
        END
     
        -----------------------------------
        -- GET LATEST PUNCH INFO
        -----------------------------------
        SELECT 
            @PunchCount = COUNT(*),
            @LatestPunch = MAX(PunchTime)
        FROM [HRM_DB_GCTL].[dbo].[AttendanceLog]
        WHERE AttendanceID = @AttendanceID AND CAST(PunchTime AS DATE) = CAST(@CHECKTIME AS DATE);
     
        IF @LatestPunch IS NOT NULL
        BEGIN
            SET @LatestPunchUTC = (@LatestPunch AT TIME ZONE 'Bangladesh Standard Time') AT TIME ZONE 'UTC';
        END
     
        -----------------------------------
        -- CALCULATE WORKING TIME USING ODD-EVEN PUNCHES
        -----------------------------------
        
        -- Get the employee's shift details
        SELECT TOP 1
            @DefaultShiftID = DS.ShiftID,
            @ShiftEndTime = S.EndTime
        FROM [HRM_DB_GCTL].[dbo].[DefaultShifts] DS
        JOIN [HRM_DB_GCTL].[dbo].[Shifts] S
            ON DS.ShiftID = S.ShiftID
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
     
        -----------------------------------
        -- CALCULATE OVERTIME
        -----------------------------------
        SET @OvertimeMinutes = 0;
     
        IF @IsAllowOvertime = 1 
           AND @WorkingTimeMinutes >= @MinimumWorkingTime
           AND (@WorkingTimeMinutes - @OfficeTimeMinutes) >= @MinimumRequiredOvertime
        BEGIN
            SET @OvertimeMinutes = @WorkingTimeMinutes - @OfficeTimeMinutes;
        END
     
        -----------------------------------
        -- FINAL UPDATE
        -----------------------------------
        UPDATE [HRM_DB_GCTL].[dbo].[Attendance]
        SET 
            CheckOutTime = CASE WHEN @PunchCount >= 2 THEN CAST(@LatestPunchUTC AS DATETIME2) ELSE NULL END
            ,WorkingTimeMinutes = @WorkingTimeMinutes
            ,OvertimeMinutes = @OvertimeMinutes
            ,BreakTimeMinutes = @BreakTimeMinutes
            ,EarlyOutTimeMinutes = @EarlyOutTimeMinutes
            ,LateOutTimeMinutes = @LateOutTimeMinutes
        WHERE AttendanceID = @AttendanceID;
    END;