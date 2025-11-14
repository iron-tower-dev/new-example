<%@ Language=VBScript %>
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<%
dim conn,rs,sql, blnWhere
dim intLoop
	set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
	if trim(Request.Form("cmdSearch"))<>"" or trim(Request.form("hidValidate"))="off" then
		sql="SELECT ID, tagNumber, ComponentCode AS Component, LocationCode AS Location, woNumber, "
		sql=sql & "testID, abbrev, entryID, validateID, entryDate "
		sql=sql & "FROM vwTestEntryUsers "
		if trim(Request.Form("cmdSearch"))<>"" then
			blnWhere=false
			if trim(Request.Form("txtID"))<>"" then
				if not blnWhere then
					sql=sql & "WHERE (ID LIKE '" & Request.Form("txtID") & "%') "
					blnWhere=true
				else
					sql=sql & "AND (ID LIKE '" & Request.Form("txtID") & "%') "
				end if
			end if
			if trim(Request.Form("txtTag"))<>"" then
				if not blnWhere then
					sql=sql & "WHERE (tagNumber LIKE '" & Request.Form("txtTag") & "%') "
					blnWhere=true
				else
					sql=sql & "AND (tagNumber LIKE '" & Request.Form("txtTag") & "%') "
				end if
			end if
			if trim(Request.Form("txtComponent"))<>"" then
				if not blnWhere then
					sql=sql & "WHERE (Componentcode LIKE '" & Request.Form("txtComponent") & "%') "
					blnWhere=true
				else
					sql=sql & "AND (Componentcode LIKE '" & Request.Form("txtComponent") & "%') "
				end if
			end if		
			if trim(Request.Form("txtLocation"))<>"" then
				if not blnWhere then
					sql=sql & "WHERE (Locationcode LIKE '" & Request.Form("txtLocation") & "%') "
					blnWhere=true
				else
					sql=sql & "AND (Locationcode LIKE '" & Request.Form("txtLocation") & "%') "
				end if
			end if	
			if trim(Request.Form("txtWONum"))<>"" then
				if not blnWhere then
					sql=sql & "WHERE (woNumber LIKE '" & Request.Form("txtWONum") & "%') "
					blnWhere=true
				else
					sql=sql & "AND (woNumber LIKE '" & Request.Form("txtWONum") & "%') "
				end if
			end if		
			if trim(Request.Form("txtTestID"))<>"" then
				if not blnWhere then
					sql=sql & "WHERE (TestID=" & Request.Form("txtTestID") & ") "
					blnWhere=true
				else
					sql=sql & "AND (TestID=" & Request.Form("txtTestID") & ") "
				end if
			end if		
			if trim(Request.Form("txtTestAbbrev"))<>"" then
				if not blnWhere then
					sql=sql & "WHERE (abbrev LIKE '" & Request.Form("txtTestAbbrev") & "%') "
					blnWhere=true
				else
					sql=sql & "AND (abbrev LIKE '" & Request.Form("txtTestAbbrev") & "%') "
				end if
			end if	
			if trim(Request.Form("txtEntryID"))<>"" then
				if not blnWhere then
					sql=sql & "WHERE (entryID LIKE '" & Request.Form("txtEntryID") & "%') "
					blnWhere=true
				else
					sql=sql & "AND (entryID LIKE '" & Request.Form("txtEntryID") & "%') "
				end if
			end if	
			if trim(Request.Form("txtValidateID"))<>"" then
				if not blnWhere then
					sql=sql & "WHERE (validateID LIKE '" & Request.Form("txtValidateID") & "%') "
					blnWhere=true
				else
					sql=sql & "AND (validateID LIKE '" & Request.Form("txtValidateID") & "%') "
				end if
			end if	
			if trim(Request.Form("txtEntryDate"))<>"" then
				if not blnWhere then
					sql=sql & "WHERE (entryDate = '" & Request.Form("txtEntryDate") & "') "
					blnWhere=true
				else
					sql=sql & "AND (entryDate = '" & Request.Form("txtEntryDate") & "') "
				end if
			end if				
			sql=sql & "ORDER BY ID DESC"
		end if
		if trim(Request.form("hidValidate"))="off" and trim(Request.Form("cmdSearch"))="" then
			sql=sql & "ORDER BY ID DESC"
		end if
		set rs=DisconnectedRS(sql,conn)
		if DBErrorCount(conn)>0 then
			Response.write DBErrors(conn) & "<HR>" & sql
			Response.end
		end if			
	end if
	CloseDBObject(conn)	
%>
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
	<TITLE>Lookup Tests & Users</TITLE>
<SCRIPT ID=clientEventHandlersJS LANGUAGE=javascript>
<!--

function cmdReset_onclick() {
	frmLookup.txtID.value='';
	frmLookup.txtTag.value='';
	frmLookup.txtComponent.value='';
	frmLookup.txtLocation.value='';
	frmLookup.txtWONum.value='';
	frmLookup.txtTestID.value='';
	frmLookup.txtTestAbbrev.value='';
	frmLookup.txtEntryID.value='';
	frmLookup.txtEntryDate.value='';
	frmLookup.txtValidateID.value='';
}

function frmLookup_onsubmit() {
	if(frmLookup.hidValidate.value=='on')
	{
		if(frmLookup.txtID.value.length==0 && frmLookup.txtTag.value.length==0 && frmLookup.txtComponent.value.length==0 && frmLookup.txtLocation.value.length==0 && frmLookup.txtWONum.value.length==0 && frmLookup.txtTestID.value.length==0 && frmLookup.txtTestAbbrev.value.length==0 && frmLookup.txtEntryID.value.length==0 && frmLookup.txtEntryDate.value.length==0 && frmLookup.txtValidateID.value.length==0)
		{
			alert('Please enter some search criteria...');
			return false;
		}
	}
}

function cmdAll_onclick() {
	frmLookup.hidValidate.value='off';
	frmLookup.submit();
}

function cmdmenu_onclick(){
  window.location.href='<%=Application("url")%>lab'
}

function document_onkeypress() {
 	if (event.keyCode>= 97 && event.keyCode <= 122)
	{
		event.keyCode -= 32;
	}
}

function window_onload() {
  window.document.frmLookup.txtID.focus()
}

//-->
</SCRIPT>
<SCRIPT LANGUAGE=javascript FOR=document EVENT=onkeypress>
<!--
 document_onkeypress()
//-->
</SCRIPT>
</HEAD>
<BODY LANGUAGE=javascript onload="return window_onload()">
	<H3 ALIGN=CENTER>Lookup Tests & Users
	<input type=button name=cmdmenu value='Lab Menu' onclick='cmdmenu_onclick()'>
	</H3>
	<hr>
	<FORM ID=frmLookup NAME=frmLookup method=POST action='usertests.asp'  LANGUAGE=javascript onsubmit="return frmLookup_onsubmit()">
	<table cols=9 width=100%>
		<tr>
			<th align=center>Sample ID</td>
			<th align=center>EQID</td>
			<th align=center>Component#</td>
			<th align=center>Location#</td>
			<th align=center>WM#</td>
			<th align=center>Test ID</td>
			<th align=center>Test Abbrev.</td>
			<th align=center>Entry ID</td>
			<th align=center>Review ID</td>
			<th align=center>Entry Date</td>
		</tr>
		<tr>
			<td align=center><input type=text id=txtID name=txtID size=10 value='<%=Request.Form("txtID")%>'></td>
			<td align=center><input type=text id=txtTag name=txtTag size=10 value='<%=Request.Form("txtTag")%>'></td>
			<td align=center><input type=text id=txtComponent name=txtComponent size=10 value='<%=Request.Form("txtComponent")%>'></td>
			<td align=center><input type=text id=txtLocation name=txtLocation size=10 value='<%=Request.Form("txtLocation")%>'></td>
			<td align=center><input type=text id=txtWONum name=txtWONum size=5 value='<%=Request.Form("txtWONum")%>'></td>
			<td align=center><input type=text id=txtTestID name=txtTestID size=10 value='<%=Request.Form("txtTestID")%>'></td>
			<td align=center><input type=text id=txtTestAbbrev name=txtTestAbbrev size=10 value='<%=Request.Form("txtTestAbbrev")%>'></td>
			<td align=center><input type=text id=txtEntryID name=txtEntryID size=10 value='<%=Request.Form("txtEntryID")%>'></td>
			<td align=center><input type=text id=txtValidateID name=txtValidateID size=10 value='<%=Request.Form("txtValidateID")%>'></td>
			<td align=center><input type=text id=txtEntryDate name=txtEntryDate size=10 value='<%=Request.Form("txtEntryDate")%>'></td>
		</tr>
		<tr>
			<td colspan=10 align=center>
				<input type=button id=cmdReset name=cmdReset value='  Clear  ' LANGUAGE=javascript onclick="return cmdReset_onclick()">&nbsp;&nbsp;<input type=submit id=cmdSearch name=cmdSearch value='Search'>&nbsp;&nbsp;<input type=button id=cmdAll name=cmdAll value='     All     ' LANGUAGE=javascript onclick="return cmdAll_onclick()">
				<input type=hidden id=hidValidate name=hidValidate value='on'>
			</td>
		</tr>
<%
		if isopen(rs) then
			if rs.eof then
%>
		<tr>
			<td colspan=10 align=center>
				<i>Sorry, but there are no records matching the above criteria...</i>
			</td>
		</tr>
<%
			else
				do until rs.eof
					Response.Write "<tr>"
					Response.Write "<td><a href='" & Application("URL") & "/lab/release/default.asp?id=" & rs("ID")  & "&src=search" & "' target='_blank'>" & rs("ID") & "</a></td>"
					for intLoop=1 to rs.fields.count-1
						Response.Write "<td>&nbsp;" & rs(intLoop) & "</td>"
					next
					Response.Write "</tr>" & vbcrlf
					rs.movenext	
				loop
			end if
			CloseDBObject(rs)
		end if
%>		
	</table>
	</FORM>
</BODY>
</HTML>
<!--#include virtual="includes/footer.js"-->
