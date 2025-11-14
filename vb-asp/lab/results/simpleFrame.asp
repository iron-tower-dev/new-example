<%@ Language=VBScript %>
<%Option Explicit%>
<!--simpleFrame.asp-->
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<%dim strText
strText=Request.QueryString("text")
%>
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
</HEAD>
<BODY>

<table width="100%">
<tr>
<th><%=strText%></th>
</tr>
</table>

</BODY>
</HTML>
<!--#include virtual="includes/footer.js"-->