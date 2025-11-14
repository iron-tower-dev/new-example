<%@ Language=VBScript %>
<%dim details
details = Request.QueryString("details")
%>
<HTML>
<HEAD>
<title><%=Application("ShortName") & " " & Application("Version")%> Unauthorised Access</title>
<SCRIPT ID=clientEventHandlersJS LANGUAGE=javascript>
<!--

function cmdClose_onclick() {
	window.close();
}

//-->
</SCRIPT>
</HEAD>
<BODY>
<div align=center>
<H3 align=center>Unauthorized Access
<%if len(trim(details)) > 0 then
    Response.Write " " & details
  end if%>
</H3>
<h4>Sorry, but you are not authorized to view the requested 
page.</h4>
<h4>Access roles are managed using <a href="https://iam.apsc.com/identityiq/home.jsf">IAM</a></h4>
</div>
</BODY>
</HTML>