<%@ Language=VBScript %>
<%Option Explicit%>
<!--imagePreview.asp-->
<%dim strTestName,strParticleType,strImage

strTestName=Request.QueryString("name")
strParticleType=Request.QueryString("type")
strImage=Request.QueryString("img")

%>
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
	<title><%=strTestName%> Sample Image-<%=strParticleType%></title>
</HEAD>

<body>
    <div align=center>
        <img src="http://<%=Request.ServerVariables("SERVER_NAME")%>/images/pti/<%=strImage%>" />
    </div>
</body>

</HTML>
<!--#include virtual="includes/footer.js"-->