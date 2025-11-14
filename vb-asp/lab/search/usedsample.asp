<%@ Language=VBScript %>
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<%
dim conn,rs,sql, rsStatus
dim intLoop
	set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
	if trim(Request.Form("cmdSearch"))<>"" or trim(Request.form("hidValidate"))="off" then
		sql="SELECT UsedLubeSamples.ID, UsedLubeSamples.tagNumber, Component.name AS component, Location.name AS location, "
		sql=sql & "UsedLubeSamples.woNumber, UsedLubeSamples.lubeType, UsedLubeSamples.sampleDate, UsedLubeSamples.receivedOn, "
		sql=sql & "TestList.Description "
		sql=sql & "FROM UsedLubeSamples INNER JOIN Component ON UsedLubeSamples.component = Component.code INNER JOIN "
		sql=sql & "Location ON UsedLubeSamples.location = Location.code INNER JOIN "
		sql=sql & "TestList ON UsedLubeSamples.status = TestList.Status "
		if trim(Request.Form("cmdSearch"))<>"" then
			sql=sql & "WHERE (UsedLubeSamples.newUsedFlag = 0) "
			if trim(Request.Form("txtID"))<>"" then
				sql=sql & "AND (UsedLubeSamples.ID LIKE '" & Request.Form("txtID") & "%') "
			end if
			if trim(Request.Form("txtTag"))<>"" then
				sql=sql & "AND (UsedLubeSamples.tagNumber LIKE '" & Request.Form("txtTag") & "%') "
			end if
			if trim(Request.Form("txtComponent"))<>"" then
				sql=sql & "AND (Component.name LIKE '" & Request.Form("txtComponent") & "%') "
			end if		
			if trim(Request.Form("txtLocation"))<>"" then
				sql=sql & "AND (Location.name LIKE '" & Request.Form("txtLocation") & "%') "
			end if	
			if trim(Request.Form("txtWONum"))<>"" then
				sql=sql & "AND (UsedLubeSamples.woNumber LIKE '" & Request.Form("txtWONum") & "%') "
			end if		
			if trim(Request.Form("txtLubeType"))<>"" then
				sql=sql & "AND (UsedLubeSamples.lubeType LIKE '" & Request.Form("txtLubeType") & "%') "
			end if		
			if trim(Request.Form("txtSampleDate"))<>"" then
				sql=sql & "AND (UsedLubeSamples.sampleDate = '" & Request.Form("txtSampleDate") & "') "
			end if	
			if trim(Request.Form("txtReceivedDate"))<>"" then
				sql=sql & "AND (UsedLubeSamples.receivedOn = '" & Request.Form("txtReceivedDate") & "') "
			end if	
			if trim(Request.Form("txtSampleStatus"))<>"-" then
				sql=sql & "AND (TestList.status = " & Request.Form("txtSampleStatus") & ") "
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
	sql="SELECT STATUS, DESCRIPTION FROM TESTLIST ORDER BY STATUS"
	set rsStatus=DisconnectedRS(sql,conn)
	CloseDBObject(conn)	
%>
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
	<TITLE>Lookup Used Samples</TITLE>
<SCRIPT ID=clientEventHandlersJS LANGUAGE=javascript>
<!--

function cmdReset_onclick() {
	frmLookup.txtID.value='';
	frmLookup.txtTag.value='';
	frmLookup.txtComponent.value='';
	frmLookup.txtLocation.value='';
	frmLookup.txtWONum.value='';
	frmLookup.txtLubeType.value='';
	frmLookup.txtSampleDate.value='';
	frmLookup.txtReceivedDate.value='';
	frmLookup.txtSampleStatus.selectedIndex=0;
}

function frmLookup_onsubmit() {
	if(frmLookup.hidValidate.value=='on')
	{
		if(frmLookup.txtID.value.length==0 && frmLookup.txtTag.value.length==0 && frmLookup.txtComponent.value.length==0 && frmLookup.txtLocation.value.length==0 && frmLookup.txtWONum.value.length==0 && frmLookup.txtLubeType.value.length==0 && frmLookup.txtSampleDate.value.length==0 && frmLookup.txtReceivedDate.value.length==0 && frmLookup.txtSampleStatus.value.length==0)
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
	<H3 ALIGN=CENTER>Lookup Used Samples</H3>
	<hr>
	<FORM ID=frmLookup NAME=frmLookup method=POST action='usedsample.asp'  LANGUAGE=javascript onsubmit="return frmLookup_onsubmit()">
	<table cols=9 width=100%>
		<tr>
			<th align=center>Sample ID</td>
			<th align=center>EQID</td>
			<th align=center>Component</td>
			<th align=center>Location</td>
			<th align=center>WM#</td>
			<th align=center>Lube Type</td>
			<th align=center>Date Sampled</td>
			<th align=center>Date Received</td>
			<th align=center>Sample Status</td>
		</tr>
		<tr>
			<td align=center><input type=text id=txtID name=txtID size=10 value='<%=Request.Form("txtID")%>'></td>
			<td align=center><input type=text id=txtTag name=txtTag size=10 value='<%=Request.Form("txtTag")%>'></td>
			<td align=center><input type=text id=txtComponent name=txtComponent size=10 value='<%=Request.Form("txtComponent")%>'></td>
			<td align=center><input type=text id=txtLocation name=txtLocation size=10 value='<%=Request.Form("txtLocation")%>'></td>
			<td align=center><input type=text id=txtWONum name=txtWONum size=5 value='<%=Request.Form("txtWONum")%>'></td>
			<td align=center><input type=text id=txtLubeType name=txtLubeType size=10 value='<%=Request.Form("txtLubeType")%>'></td>
			<td align=center><input type=text id=txtSampleDate name=txtSampleDate size=10 value='<%=Request.Form("txtSampleDate")%>'></td>
			<td align=center><input type=text id=txtReceivedDate name=txtReceivedDate size=10 value='<%=Request.Form("txtReceivedDate")%>'></td>
			<td align=center>
				<select id=txtSampleStatus name=txtSampleStatus>
					<option value='-'<%if Request.Form("txtSampleStatus")="-" then Response.Write " selected"%>>-All-</option>
					<%
						do until rsStatus.eof
							Response.Write "<option value=" & rsStatus("status")
							if cstr(Request.Form("txtSampleStatus"))=cstr(rsStatus("status")) then
								Response.Write " selected"
							end if
							Response.write ">" & rsStatus("Description") & "</option>" & vbcrlf
							rsStatus.movenext
						loop
						rsStatus.close
						set rsStatus=nothing
					%>
				</select>
			</td>
		</tr>
		<tr>
			<td colspan=6 align=center>
				<input type=button id=cmdReset name=cmdReset value='  Clear  ' LANGUAGE=javascript onclick="return cmdReset_onclick()">&nbsp;&nbsp;<input type=submit id=cmdSearch name=cmdSearch value='Search'>&nbsp;&nbsp;<input type=button id=cmdAll name=cmdAll value='     All     ' LANGUAGE=javascript onclick="return cmdAll_onclick()">
				<input type=hidden id=hidValidate name=hidValidate value='on'>
			</td>
      <td colspan=3 align=center><input type=button name=cmdmenu value='Lab Menu' onclick='cmdmenu_onclick()'></td>
		</tr>
<%
		if isopen(rs) then
			if rs.eof then
%>
		<tr>
			<td colspan=9 align=center>
				<i>Sorry, but there are no records matching the above criteria...</i>
			</td>
		</tr>
<%
			else
				do until rs.eof
					Response.Write "<tr>"
					Response.Write "<td><a href='" & Application("URL") & "/lab/release/default.asp?id=" & rs("ID") & "&src=search" & "' target=_blank'>" & rs("ID") & "</a></td>"
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
