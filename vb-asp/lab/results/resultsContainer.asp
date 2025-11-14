<%@ Language=VBScript %>
<%Option Explicit%>
<!--resultsContainer.asp-->
<script src="../../includes/jquery-3.7.1.js"></script>
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<%dim strTestID,strTestName

strTestID=Request.QueryString("id")
strTestName=Request.QueryString("name")

%>
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
	<title>Enter Results-<%=strTestName%></title>
<SCRIPT ID=clientEventHandlersJS LANGUAGE=javascript>
<!--
function toggle_history() {
    if ($('frameset')[1].rows == '75%,*')
    {
        $('frameset')[1].rows = '60%,*'
    }
    else
    {
        $('frameset')[1].rows = '75%,*'
    }
}
//-->
</SCRIPT>
</HEAD>

<frameset COLS="15%,*" FRAMEBORDER="1" FRAMESPACING="0" BORDER="1">
  <frame NAME="fraTestSampleList" SRC="<%=Application("URL")%>lab/results/simpleFrameContainer.asp?id=<%=strTestID%>&name=<%=strTestName%>">
  <frameset ROWS="75%,*" FRAMEBORDER="1" FRAMESPACING="0" BORDER="1">
    <frame NAME="fraEnterResults" SRC="<%=Application("URL")%>lab/results/enterResults.asp?mode=blank"> 
    <frame NAME="fraLubePointHistory" SRC="<%=Application("URL")%>lab/results/lubePointHistory.asp?mode=blank">  
  </frameset>
</frameset>


<noframes>
<body>
<h4>This page uses frames, but your browser doesn't support them.</h4>
</body>
</noframes>

</HTML>
<!--#include virtual="includes/footer.js"-->
