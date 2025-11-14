<%@ Language=VBScript %>
<!--#include virtual="includes/DBFunctions.asp"-->
<%
	dim strQueryString, dbConnection, rsResults, strSQL
	dim strError
	strError=""
	strQueryString=Request.QueryString("GOTO")
	if instr(strQueryString,"|")>0 then
		strQueryString=replace(strQueryString,"|","?")
	end if
	if Request.QueryString("Logon")="N" then
		session.Abandon
		Response.Redirect Application("URL")
	end if
	if trim(Request.form("cmdOK"))<>"" then
		if trim(Request.Form("txtUserID"))<>"" and trim(Request.form("txtPassword"))<>"" then
			' check for a valid entry...
			strSQL="SELECT employeeID FROM LubeTechList WHERE employeeID='" & ucase(mid(Request.Form("txtUserID"),2)) & "' and qualificationPassword='" & Request.Form("txtPassword") & "'"
			set dbConnection=OpenConnection(Application("dbLUBELAB_ConnectionString"))
			set rsResults=dbConnection.Execute(strSQL)
			if not rsResults.EOF then
				session("Username")=ucase(trim(Request.Form("txtUserID")))
				session("USR")=mid(ucase(trim(Request.Form("txtUserID"))),2)
				Response.Redirect Application("URL") & strQueryString
			else
				strError="Invalid UserID or Password"
			end if
			rsResults.close
			set rsResults=nothing
			dbConnection.close
			set dbConnection=nothing
		end if
	end if
%>
<HTML>
<HEAD>
<TITLE>Logon User</TITLE>
<SCRIPT ID=clientEventHandlersJS LANGUAGE=javascript>
<!--

function window_onload() {
  window.frmLogon.txtUserID.focus()
}

//-->
</SCRIPT>
</HEAD>
<BODY LANGUAGE=javascript onload="return window_onload()">
<%
	if trim(strError)<>"" then
		Response.Write "<h3 align=center><font color=red>" & strError & "</font></h3>"
	end if
%>
<h3 align=center>Please logon:</h3>
	<form id=frmLogon id=frmLogon method=post action=logon.asp?GOTO=<%=Request.QueryString("GOTO")%>>
	<table width=50% align=center cols=2 border=0>
		<tr>
			<td width=60%>Please enter your user ID (e.g. Z12345)</td>
			<td><input type=text id=txtUserID name=txtUserID size=8></td>
		</tr>
		<tr>
			<td width=60%>Password</td>
			<td><input type=password id=txtPassword name=txtPassword size=8></td>
		</tr>
		<tr>
			<td>&nbsp;</td>
			<td colspan=2><input type=submit name=cmdOK id=cmdOK value='Logon'></td>
		</tr>
		</table>
	</form>
</BODY>
</HTML>
