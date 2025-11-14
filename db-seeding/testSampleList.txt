<%@ Language=VBScript %>
<%Option Explicit%>
<!--testSampleList.asp-->
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<%
dim strTestID,strTestName
dim conn,rs,sql,strErrors,strTitle

strTestID=Request.QueryString("id")
strTestName=Request.QueryString("name")
on error resume next
sql="SELECT distinct t.sampleID,u.tagNumber,u.Component,u.Location, l.qualityClass FROM TestReadings t INNER JOIN UsedLubeSamples u ON t.sampleID = u.ID LEFT OUTER JOIN Lube_Sampling_Point l ON u.tagNumber = l.tagNumber AND u.component = l.component AND u.location = l.location WHERE t.status='A' AND t.testID=" & strTestID &" AND t.sampleID=u.ID ORDER BY t.sampleID"
set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
if IsOpen(conn) then
  set rs=DisconnectedRS(sql,conn)
  strErrors=DBErrors()
else
  strErrors=DBErrors()
end if
CloseDBObject(conn)
set conn=nothing
on error goto 0
%>
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
</HEAD>
<BODY>
<%if IsOpen(rs)=false then
    Response.Write "<h4>Error reading sample data.</h4><br>"
    Response.Write sql & "<br>"
    Response.Write strErrors
    Response.End
  end if%>
<table width="100%">
<%
if NumberOfRecords(rs)>0 then
  rs.MoveFirst
  do
    strTitle="EQID: " & getField(rs,"tagNumber") & " QClass: " & getField(rs,"qualityClass")
%><tr><td class=ScheduledList>
      <a href="<%=Application("url")%>lab/results/enterResults.asp?sid=<%=rs.Fields("sampleID")%>&tid=<%=strTestID%>&tname=<%=strTestName%>&tag=<%=rs.Fields("tagNumber")%>&comp=<%=rs.Fields("component")%>&loc=<%=rs.Fields("location")%>" target='fraEnterResults' title='<%=strTitle%>'><%=rs.Fields("sampleID")%></a>
  </td></tr>
<%  rs.MoveNext
  loop until rs.EOF
%>
<%
else
%>
<tr><td>No samples pending</td></td>
<%
end if
%>
</table>
<%
CloseDBObject(rs)
set rs=nothing%>

</BODY>
</HTML>
<!--#include virtual="includes/footer.js"-->
