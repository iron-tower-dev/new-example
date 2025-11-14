<%<!--DB functions-->
'---- CursorTypeEnum Values ----
Const adOpenForwardOnly = 0
Const adOpenKeyset = 1
Const adOpenDynamic = 2
Const adOpenStatic = 3

'---- CursorOptionEnum Values ----
Const adHoldRecords = &H00000100
Const adMovePrevious = &H00000200
Const adAddNew = &H01000400
Const adDelete = &H01000800
Const adUpdate = &H01008000
Const adBookmark = &H00002000
Const adApproxPosition = &H00004000
Const adUpdateBatch = &H00010000
Const adResync = &H00020000
Const adNotify = &H00040000
Const adFind = &H00080000
Const adSeek = &H00400000
Const adIndex = &H00800000

'---- LockTypeEnum Values ----
Const adLockReadOnly = 1
Const adLockPessimistic = 2
Const adLockOptimistic = 3
Const adLockBatchOptimistic = 4

'---- ObjectStateEnum Values ----
Const adStateClosed = &H00000000
Const adStateOpen = &H00000001
Const adStateConnecting = &H00000002
Const adStateExecuting = &H00000004
Const adStateFetching = &H00000008

'---- CursorLocationEnum Values ----
Const adUseServer = 2
Const adUseClient = 3

'---- FilterGroupEnum Values ----
Const adFilterNone = 0
Const adFilterPendingRecords = 1
Const adFilterAffectedRecords = 2
Const adFilterFetchedRecords = 3
Const adFilterConflictingRecords = 5


function OpenConnection(strConnectString)
dim conn
	if len(trim(strConnectString)) > 0 then
		set conn = Server.CreateObject("ADODB.Connection")
		conn.Open strConnectString
	else
		set conn = nothing
	end if
	set OpenConnection = conn
	set conn = nothing
end function

sub CloseDBObject(objADO)
on error resume next
	if objADO.State = adStateOpen then
		objADO.Close
	end if
end sub

function DBErrorCount(objConnection)
	DBErrorCount=objConnection.Errors.Count
end function

function DBErrors(objConnection)
dim objErr
	DBErrors = ""
	if objConnection.Errors.Count > 0 then
		for each objErr in objConnection.Errors
			DBErrors = DBErrors & "<br><br>Error #: " & objErr.Number & "<br>"
			DBErrors = DBErrors & "<br>Description: " & objErr.Description & "<br>"
			DBErrors = DBErrors & "<br>Source: " & objErr.Source & "<br>"
			DBErrors = DBErrors & "<br>SQL state: " & objErr.SQLState & "<br><br>"
		next
	end if
	set objErr = nothing
end function

function ForwardOnlyRS(strSQL, objConnection)
dim rs
	set rs = Server.CreateObject("ADODB.Recordset")
	rs.Open strSQL, objConnection, adOpenForwardOnly, adLockReadOnly
	set ForwardOnlyRS = rs
	set rs = nothing
end function

function DisconnectedRS(strSQL, objConnection)
dim rs
	set rs = Server.CreateObject("ADODB.Recordset")
	rs.CursorLocation = adUseClient
	'Response.Write("strSQL: " & strSQL & "<BR>")
	'Response.End()
	rs.Open strSQL, objConnection, adOpenStatic, adLockBatchOptimistic
	set rs.ActiveConnection = nothing
	set DisconnectedRS = rs
	set rs = nothing
end function

function NumberOfRecords(objRS)
  if objRS.Supports(adBookmark or adApproxPosition) then
    if objRS.EOF and objRS.BOF then
      NumberOfRecords = 0
    else
      NumberOfRecords = objRS.RecordCount
    end if
  else
    NumberOfRecords = -1
  end if
end function

function IsOpen(objADO)
  IsOpen = false
  on error resume next
  IsOpen = (objADO.State = adStateOpen)
end function

function OracleDate(strDate)
  OracleDate="NULL"
  if IsDate(strDate) then
    OracleDate="TO_DATE('" & DatePart("m",strDate) & "/" & DatePart("d",strDate) & "/" & DatePart("yyyy",strDate) & "', 'MM/DD/YYYY')"
  end if
end function

function ControlValue(strName)
dim conn,rs,sql

  ControlValue=""
  strName=trim(strName)
  if len(strName)>0 then
    sql="SELECT ControlValue FROM Control_Data WHERE Name='" & strName & "'"
    set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
    if IsOpen(conn) then
      set rs=ForwardOnlyRS(sql,conn)
      if IsOpen(rs) then
        if not(rs.BOF and rs.EOF) then
          rs.MoveFirst
          if not isnull(rs.Fields("ControlValue")) then
            ControlValue=rs.Fields("ControlValue")
          end if
        end if
        CloseDBObject(rs)
      end if
      CloseDBObject(conn)
    end if
  end if
  set rs=nothing
  set conn=nothing
end function

function getField(rs,field)
  Err.Clear
  on error resume next
  if isnull(rs.Fields(field)) then
    getField=""
  else
    getField=rs.Fields(field)
  end if
  if Err.number=3265 then
    getField=""
  end if
end function

sub ApplyFilter(rs,filter)
  rs.Filter=filter
end sub

sub ClearFilter(rs)
  rs.Filter=adFilterNone
end sub
%>