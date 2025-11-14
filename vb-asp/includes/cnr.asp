<%
function GetApplID(tag,comp,loc)
dim applid, sql, rs, conn
  applid=""
  sql = "SELECT applid FROM lube_sampling_point WHERE tagnumber='" & tag & "'"
  sql=sql & " AND component='" & comp & "' AND location='" & loc & "'"
  Set conn = OpenConnection(Application("dbLUBELAB_ConnectionString"))
  Set rs = DisconnectedRS(sql, conn)
  CloseDBObject(conn)
  set conn = nothing
  if not rs.eof then
    rs.MoveFirst
    applid = rs.Fields("applid")
  end if
  CloseDBObject(rs)
  set rs = nothing
  GetApplID = applid
end function

function GetEQID(applid)
dim eqid, sql, rs, conn
  eqid=""
  sql = "SELECT EQID FROM LUBELAB.LUBELAB_EQUIPMENT_V WHERE APPLID=" & applid
  Set conn = OpenConnection(Application("dbSWMS_ConnectionString"))
  Set rs = DisconnectedRS(sql, conn)
  CloseDBObject(conn)
  set conn = nothing
  if not rs.eof then
    rs.MoveFirst
    eqid = rs.Fields("EQID")
  end if
  CloseDBObject(rs)
  set rs = nothing
  GetEQID = eqid
end function

function GetEQTAGNUM(applid)
dim eqtagnum, sql, rs, conn
  eqtagnum=""
  sql = "SELECT EQ_TAG_NUM FROM LUBELAB.LUBELAB_EQUIPMENT_V WHERE APPLID=" & applid
  Set conn = OpenConnection(Application("dbSWMS_ConnectionString"))
  Set rs = DisconnectedRS(sql, conn)
  CloseDBObject(conn)
  set conn = nothing
  if not rs.eof then
    rs.MoveFirst
    eqtagnum = rs.Fields("EQ_TAG_NUM")
  end if
  CloseDBObject(rs)
  set rs = nothing
  GetEQTAGNUM = eqtagnum
end function

function GetSeverityInfo(whereClause, severity, color, fcolor)
dim sSQL, DBConnEHealth, RSStatus, url
url=""
    sSQL = "select S.CURRENT_EQID_STATUS, S.TE_URL, S.TE_COLOR_HTML, S.TE_COLOR_ID from [" & Application("CNR_DB") & "].[dbo].[VCOMPONENT_TECH_SEVERITIES] S where " & whereClause & " Order By CURRENT_EQID_STATUS asc"
    Set DBConnEHealth = OpenConnection(Application("CNR_ConnectionString"))
    set RSStatus = DisconnectedRS(sSQL, DBConnEHealth)
    CloseDBObject(DBConnEHealth)
    If not (RSStatus.BOF and RSStatus.EOF) then
        RSStatus.Movefirst
        url=RSStatus.fields("TE_URL")
        severity=RSStatus.fields("CURRENT_EQID_STATUS")
        color=RSStatus.fields("TE_COLOR_HTML")
        select case UCase(RSStatus.fields("TE_COLOR_ID"))
            case "YELLOW"
                fcolor = "#000000"
            case "LIGHTGRAY"
                fcolor = "#000000"
            case else
                fcolor = "#FFFFFF"
        end select
    else
        severity=""
        color=""
        fcolor=""
    end if
    CloseDBObject(RSStatus)
    set RSStatus = nothing
    CloseDBObject(DBConnEHealth)
    GetSeverityInfo = url
end function
%>
