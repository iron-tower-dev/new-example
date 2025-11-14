<%@ Language=VBScript %>
<%Option Explicit%>
<!--resultsContainer.asp-->
<script src="../../includes/jquery-3.7.1.js"></script>
<!--#include virtual="includes/security.asp"-->
<%dim tid,tname,mode,sid,tag,comp,loc,url,overall,statuscode,history,concise

tid=Request.QueryString("id")
tname=Request.QueryString("name")
mode=Request.QueryString("mode")
sid=Request.QueryString("sid")
tag=Request.QueryString("tag")
comp=Request.QueryString("comp")
loc=Request.QueryString("loc")
overall=Request.QueryString("overall")
statuscode=Request.QueryString("statuscode")
history=Request.QueryString("history")
concise=Request.QueryString("concise")

url=Application("url") & "lab/results/enterResults.asp?mode=" & mode & "&tid=" & tid & "&sid=" & sid & "&tname=" & tname & "&tag=" & tag & "&comp=" & comp & "&loc=" & loc & "&overall=" & overall & "&statuscode=" & statuscode & "&history=" & history & "&concise=" & concise
%>
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
	<title>Results-<%=tname%></title>

<SCRIPT ID=clientEventHandlersJS LANGUAGE=javascript>
<!--
function refresh_opener() {
  window.opener.location.reload()
}
function toggle_history() {
    if ($('frameset')[0].rows == '85%,*')
    {
        $('frameset')[0].rows = '60%,*'
    }
    else
    {
        $('frameset')[0].rows = '85%,*'
    }
}
//-->
</SCRIPT>
<SCRIPT LANGUAGE=javascript FOR=document EVENT=onerrorupdate>
<!--
 document_onerrorupdate()
//-->
</SCRIPT>
</HEAD>

<%if (history="n") then%>
<frameset ROWS="*" FRAMEBORDER="1" FRAMESPACING="0" BORDER="1">
  <frame NAME='fraEnterResults' SRC='<%=url%>'>
</frameset>
<%else%>
<frameset ROWS="85%,*" FRAMEBORDER="1" FRAMESPACING="0" BORDER="1">
  <frame NAME='fraEnterResults' SRC='<%=url%>'>
  <frame NAME='fraLubePointHistory' SRC='<%=Application("URL")%>lab/results/lubePointHistory.asp?mode=blank'>
</frameset>
<%end if%>

<noframes>
<body>
<h4>This page uses frames, but your browser doesn't support them.</h4>
</body>
</noframes>

</HTML>
<!--#include virtual="includes/footer.js"-->
