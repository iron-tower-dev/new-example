<%@ Language=VBScript %>
<%Option Explicit%>
<!--lubePointHistory.asp-->
<script src="../../includes/jquery-3.7.1.js"></script>
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<!--#include virtual="lab/results/historyFunctions.asp"-->
<!--#include virtual="includes/comments.asp"-->
<%dim conn,rs,sql
dim intSamples,strSamples,strTag,strComp,strLoc,strCompName,strLocName
dim strCurrentSample,strTestID,strMode,strFullTag,blnInclusive,blnShowOld
dim resultsUrl,strTestName,blnLab

'just display a blank pane if requested
strMode=Request.QueryString("mode")
if strMode="blank" then
  Response.redirect(Application("url") & "lab/blank.asp")
end if

'get values from QueryString
strTag=Request.QueryString("tag")
strComp=Request.QueryString("comp")
strLoc=Request.QueryString("loc")
strCompName=Request.QueryString("cname")
strLocName=Request.QueryString("lname")
strCurrentSample=Request.QueryString("sid")
strTestID=Request.QueryString("tid")
strFullTag=strTag & " " & strCompName & "(" & strComp & ") " & strLocName & "(" & strLoc & ")"
blnInclusive=(Request.QueryString("INCL") = "Y")
blnShowOld=(Request.QueryString("old") = "Y")
blnLab=(Request.QueryString("lab") <> "Y")
strTestName=Request.QueryString("tname")

resultsUrl = Application("url") & "lab/results/enterResults.asp?mode=view&tid=" & strTestID & "&sid=SAMPLEID&tname=" & strTestName & "&tag=" & strTag & "&comp=" & strComp & "&loc=" & strLoc & "&statuscode=C&history=n&concise=y"

'Find out from DB control table how many samples we show history for
strSamples=ControlValue("HistSamps")
if len(trim(strSamples))>0 then
  intSamples=cint(strSamples)
else
  intSamples=3  'set a default in case not set in the DB
end if
'get details for last 'n' test results on this lube point

sql=historySQL(strTestID,strTag,strComp,strLoc,strCurrentSample,intSamples,blnShowOld)

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
</HEAD>
<BODY>
<div align=center>
<%buildHistoryTable strTestID,rs,strTag,strComp,strLoc,blnShowOld,resultsUrl,strCompName,strLocName,blnLab%>
</div>
</BODY>
<%
CloseDBObject(rs)
set rs=nothing
%>
</HTML>
<SCRIPT LANGUAGE=javascript>
<!--
function resizeHistory(){
    parent.toggle_history();
}
function olderHistory() {
    <% if (strTestID = 210) then%>
        window.location.replace('<%=Application("URL")%>analysis/ferr1.asp?fortag=<%=strTag%>&forcomp=<%=strComp%>&forloc=<%=strLoc%>&sampleid=<%=strCurrentSample%>&mtype=disp&tname=<%=strTestName%>&cname=<%=strCompName%>&lname=<%=strLocName%>&old=Y');
    <% else%>
        window.location.replace(window.location + "&old=Y");
    <% end if%>
}
function newerHistory() {
    var value = window.location.toString();
    value = value.replace("&old=Y", "");
    window.location.replace(value);
}
function openHistory() {
    window.open(window.location, '_blank');
}
//-->
</SCRIPT>
<!--#include virtual="includes/footer.js"-->
