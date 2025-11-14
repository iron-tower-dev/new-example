<%
function UpdateStats(blnGetSWMSData)
dim dbWorkMan, sqlWorkMan, strDate, cmd, rswm, fieldAvgAge, field7plus
	strDate=formatdatetime(now,2) 
    'strDate="4/26/2016"
	' get the number of samples logged in / released / reviewed
	set dbWorkMan=OpenConnection(Application("dbLUBELAB_ConnectionString"))
	if IsOpen(dbWorkMan) then
        sqlWorkMan="SELECT entry_ave, fielddays_7_over FROM workmgmt WHERE entrydate = '" & strDate & "'"
        set rswm = DisconnectedRS(sqlWorkMan, dbWorkMan)
        fieldAvgAge = null
        if (NumberOfRecords(rswm) > 0) and Not(IsNull(rswm(0))) and Not(IsNull(rswm(1))) then
            'already calculated stats previously today, re-calc the overdue stats from the SMWS records that we got earlier
            set rswm = GetSWMSStats()
            field7plus = rswm(0)
            if field7plus > 0 then
                fieldAvgAge = rswm(1) / field7plus
            end if
        else
            if blnGetSWMSData = True then
                'Did not calculate stats yet today & we should get the SWMS records for determining overdue samples
                GetSWMSRecords
                set rswm = GetSWMSStats()
                field7plus = rswm(0)
                if field7plus > 0 then
                    fieldAvgAge = rswm(1) / field7plus
                end if
            else
                'Did not calculate stats yet today & don't want to get SWMS records this time (for performance reasons)
                field7plus = null
            end if
        end if
        CloseDBObject(rswm)
        SET cmd = Server.CreateObject("ADODB.Command")
        SET cmd.ActiveConnection = dbWorkMan
        cmd.CommandText = "sp_WorkManagement"
        cmd.CommandType = 4  'adCmdStoredProc
        cmd.Parameters.Refresh
        cmd.Parameters("@pDate") = strDate
        cmd.Parameters("@pFieldAvgAge") = fieldAvgAge
        cmd.Parameters("@pField7plus") = field7plus
        cmd.Execute
	end if
	CloseDBObject dbWorkMan
	set dbWorkMan=nothing
end function

function GetSWMSStats()
dim DaysOver, sql, conn, rs
    DaysOver = ControlValue("WODaysOver")
    set conn = OpenConnection(Application("dbLUBELAB_ConnectionString"))
    sql = "SELECT COUNT(DaysOverdue), SUM(DaysOverdue) FROM vwFieldRecords where DaysOverdue > " & DaysOver
    set rs = DisconnectedRS(sql, conn)
    CloseDBObject conn
    GetSWMSStats = rs
end function
%>