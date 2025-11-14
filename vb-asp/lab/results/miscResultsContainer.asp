<%@ Language=VBScript %>
<%Option Explicit%>
<!--resultsContainer.asp-->
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<%dim tag,comp,loc
tag=Request.QueryString("tag")
comp=Request.QueryString("comp")
loc=Request.QueryString("loc")
%>
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
	<title>Enter Misc Results</title>
</HEAD>

<frameset ROWS="100%" FRAMEBORDER="1" FRAMESPACING="0" BORDER="1">
  <frame NAME="fraEnterResults" SRC="<%=Application("URL")%>lab/results/enterMiscResults.asp?tag=<%=tag%>&comp=<%=comp%>&loc=<%=loc%>">
</frameset>

<noframes>
<body>
<h4>This page uses frames, but your browser doesn't support them.</h4>
</body>
</noframes>

</HTML>
<!--#include virtual="includes/footer.js"-->