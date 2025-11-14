<%@ Language=VBScript %>
<%Option Explicit%>
<!--simpleFrameContainer.asp-->
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
</HEAD>
<BODY>

<iframe src="<%=Application("url")%>lab/results/simpleFrame.asp?text=<%=Request.QueryString("name")%>" scrolling="no" width="100%" height="10%" frameborder=0></iframe>
<iframe src="<%=Application("url")%>lab/results/testSampleList.asp?id=<%=Request.QueryString("id")%>&name=<%=Request.QueryString("name")%>" scrolling="yes" width="100%" height="90%" frameborder=0></iframe>

</BODY>
</HTML>
<!--#include virtual="includes/footer.js"-->