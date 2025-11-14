<%
Sub GetSWMSRecords()
dim DaysBack, DaysFwd, temp
dim connSWMS, rsSWMS, lngSWMSCount, sql
    lngSWMSCount=0
    DaysBack=ControlValue("WODaysBack")
    DaysFwd=ControlValue("WODaysFwd")
    
    if len(DaysBack)=0 then
      DaysBack=0
    end if
    if len(DaysFwd)=0 then
      DaysFwd=0
    end if
    DaysBack=clng(DaysBack) * -1

    sql="SELECT DISTINCT (CASE WHEN INSTR(EQUIPMENT_ID, '*') > 0 THEN substr(EQUIPMENT_ID, 1, instr(EQUIPMENT_ID, '*') -1) ELSE EQUIPMENT_ID END) AS EQUIPMENT_ID,"
    sql=sql & "COMP_CODE, LOC_CODE, SCHED_DATE, WMECH_DB_ID, DESCRIPTION_TEXT FROM LUBELAB.EQ_WORKMECHS_V "
    sql=sql & "WHERE SCHED_DATE BETWEEN " & OracleDate(FormatDateTime(DateAdd("d",DaysBack,Now()),2)) & " AND " & OracleDate(FormatDateTime(DateAdd("d",DaysFwd,Now()),2))
    set connSWMS=OpenConnection(Application("dbSWMS_ConnectionString"))
    if IsOpen(connSWMS) then
      set rsSWMS = DisconnectedRS(sql, connSWMS)
      CloseDBObject connSWMS
      if IsOpen(rsSWMS) then
        lngSWMSCount=NumberOfRecords(rsSWMS)
      end if
    end if
    set connSWMS=nothing

    if lngSWMSCount > 0 then
        sqlInsert = "INSERT INTO SWMSRecords (tagNumber, component, location, scheduledDate, woNumber, description) VALUES ('"
        set conn = OpenConnection(Application("dbLUBELAB_ConnectionString"))
        EmptySWMSTable conn
        Do While Not rsSWMS.eof

temp = Replace(rsSWMS.Fields("DESCRIPTION_TEXT"),"'","''")

            sql = sqlInsert & rsSWMS.Fields("EQUIPMENT_ID") & "', '" & rsSWMS.Fields("COMP_CODE") & "', '" & rsSWMS.Fields("LOC_CODE") & "', '" & rsSWMS.Fields("SCHED_DATE") & "', '" & rsSWMS.Fields("WMECH_DB_ID") & "', '" & temp & "')"
            rsSWMS.MoveNext
            Set rsInsert = conn.Execute(sql)
        loop
    end if
    set rsSWMS = Nothing

end Sub

Sub EmptySWMSTable (conn)
    dim cmd
    SET cmd = Server.CreateObject("ADODB.Command")
    SET cmd.ActiveConnection = conn
    cmd.CommandText = "sp_EmptySWMSRecords"
    cmd.CommandType = 4  'adCmdStoredProc
    cmd.Parameters.Refresh
    cmd.Execute
End Sub
%>