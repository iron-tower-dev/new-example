<%@ Language=VBScript %>
<%Option Explicit%>
<!--default.asp-->
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<%
dim conn,rs,sql

sql="SELECT t.id,t.name,l.qualificationLevel "
sql=sql & "FROM Test t LEFT OUTER JOIN LubeTechQualification l ON t.ID = l.testStandID "
sql=sql & "WHERE (t.exclude IS NULL OR t.exclude <> 'Y') AND (t.Lab = 1) "
sql=sql & "AND l.employeeID='" & Session("USR") & "' "
sql=sql & "ORDER BY t.ID"

set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
if IsOpen(conn) then
  set rs=DisconnectedRS(sql,conn)
end if
CloseDBObject(conn)
set conn=nothing
%>
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
	<title>Enter Results - Select Test</title>
</HEAD>
<BODY>
<div align=center>

<table>
<%
if NumberOfRecords(rs)>0 then%>
  <th>Available Tests</th>
<%rs.MoveFirst
  do
%><tr><td>
<%if getField(rs,"qualificationLevel")="Q/QAG" or getField(rs,"qualificationLevel")="TRAIN" or getField(rs,"qualificationLevel")="MicrE" then%>
      <a href="<%=Application("url")%>lab/results/resultsContainer.asp?id=<%=rs.Fields("id")%>&name=<%=rs.Fields("name")%>"><%=rs.Fields("name")%></a>
<%else
    Response.Write rs.Fields("name")
  end if%>
  </td></tr>
<%rs.MoveNext
  loop until rs.EOF
else
%>
<tr><td>No Authorized Tests Found</td></td>
<%
end if
%>
</table>
<%
CloseDBObject(rs)
set rs=nothing%>
<p align=center><input type=button value='Main Menu' language=javascript onclick='window.location.href="<%=Application("url")%>lab";' id=button1 name=button1></p>
</div>
</BODY>
</HTML>
<!--#include virtual="includes/footer.js"-->
