<%@ Language=VBScript %>
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<%
dim conn,rs,sql
dim intLoop
	if trim(Request.Form("cmdSearch"))<>"" then
		sql="SELECT UsedLubeSamples.ID, UsedLubeSamples.lubeType, "
		sql=sql & "UsedLubeSamples.woNumber, UsedLubeSamples.trackingNumber, UsedLubeSamples.warehouseID, UsedLubeSamples.batchNumber, "
		sql=sql & "UsedLubeSamples.classItem, UsedLubeSamples.sampleDate, UsedLubeSamples.receivedOn "
		sql=sql & "FROM UsedLubeSamples "
		sql=sql & "WHERE (UsedLubeSamples.newUsedFlag = 1) "
		if trim(Request.Form("txtID"))<>"" then
			sql=sql & "AND (UsedLubeSamples.ID LIKE '%" & Request.Form("txtID") & "%') "
		end if
		if trim(Request.Form("txtLubeType"))<>"" then
			sql=sql & "AND (UsedLubeSamples.lubeType LIKE '%" & Request.Form("txtLubeType") & "%') "
		end if
		if trim(Request.Form("txtPONumber"))<>"" then
			sql=sql & "AND (UsedLubeSamples.woNumber LIKE '%" & Request.Form("txtPONumber") & "%') "
		end if		
		if trim(Request.Form("txtTracking"))<>"" then
			sql=sql & "AND (UsedLubeSamples.trackingNumber LIKE '%" & Request.Form("txtTracking") & "%') "
		end if	
		if trim(Request.Form("txtWarehouse"))<>"" then
			sql=sql & "AND (UsedLubeSamples.warehouseID LIKE '%" & Request.Form("txtWarehouse") & "%') "
		end if		
		if trim(Request.Form("txtBatch"))<>"" then
			sql=sql & "AND (UsedLubeSamples.batchNumber LIKE '%" & Request.Form("txtBatch") & "%') "
		end if		
		if trim(Request.Form("txtClassItem"))<>"" then
			sql=sql & "AND (UsedLubeSamples.classItem LIKE '%" & Request.Form("txtClassItem") & "%') "
		end if				
		if trim(Request.Form("txtSampleDate"))<>"" then
			sql=sql & "AND (UsedLubeSamples.sampleDate = '" & Request.Form("txtSampleDate") & "') "
		end if	
		if trim(Request.Form("txtReceivedDate"))<>"" then
			sql=sql & "AND (UsedLubeSamples.receivedOn = '" & Request.Form("txtReceivedDate") & "') "
		end if	
		sql=sql & " ORDER BY ID DESC"
		set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
		set rs=DisconnectedRS(sql,conn)
		if DBErrorCount(conn)>0 then
			Response.write DBErrors(conn) & "<HR>" & sql
			Response.end
		end if			
		CloseDBObject(conn)
	end if
%>
<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
	<TITLE>Lookup New Material</TITLE>
<SCRIPT ID=clientEventHandlersJS LANGUAGE=javascript>
<!--

function cmdReset_onclick() {
	frmLookup.txtID.value='';
	frmLookup.txtLubeType.value='';
	frmLookup.txtPONumber.value='';
	frmLookup.txtTracking.value='';
	frmLookup.txtWarehouse.value='';
	frmLookup.txtBatch.value='';
	frmLookup.txtClassItem.value='';
	frmLookup.txtSampleDate.value='';
	frmLookup.txtReceivedDate.value='';
}

function frmLookup_onsubmit() {
	if(frmLookup.txtID.value.length==0 && frmLookup.txtLubeType.value.length==0 && frmLookup.txtPONumber.value.length==0 && frmLookup.txtTracking.value.length==0 && frmLookup.txtWarehouse.value.length==0 && frmLookup.txtBatch.value.length==0 && frmLookup.txtClassItem.value.length==0 && frmLookup.txtSampleDate.value.length==0 && frmLookup.txtReceivedDate.value.length==0)
	{
		alert('Please enter some search criteria...');
		return false;
	}
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
	<H3 ALIGN=CENTER>Lookup New Material</H3>
	<hr>
	<FORM ID=frmLookup NAME=frmLookup method=POST action='newmaterial.asp'  LANGUAGE=javascript onsubmit="return frmLookup_onsubmit()">
	<table cols=9 width=100%>
		<tr>
			<th align=center>Sample ID</td>
			<th align=center>Material</td>
			<th align=center>PO/RES#</td>
			<th align=center>Trace#</td>
			<th align=center>Warehouse ID</td>
			<th align=center>Batch/Lot</td>
			<th align=center>APN#</td>
			<th align=center>Date Sampled</td>
			<th align=center>Date Received</td>
		</tr>
		<tr>
			<td align=center><input type=text id=txtID name=txtID size=10 value='<%=Request.Form("txtID")%>'></td>
			<td align=center><input type=text id=txtLubeType name=txtLubeType size=10 value='<%=Request.Form("txtLubeType")%>'></td>
			<td align=center><input type=text id=txtPONumber name=txtPONumber size=10 value='<%=Request.Form("txtPONumber")%>'></td>
			<td align=center><input type=text id=txtTracking name=txtTracking size=10 value='<%=Request.Form("txtTracking")%>'></td>
			<td align=center><input type=text id=txtWarehouse name=txtWarehouse size=5 value='<%=Request.Form("txtWarehouse")%>'></td>
			<td align=center><input type=text id=txtBatch name=txtBatch size=10 value='<%=Request.Form("txtBatch")%>'></td>
			<td align=center><input type=text id=txtClassItem name=txtClassItem size=6 value='<%=Request.Form("txtClassItem")%>'></td>
			<td align=center><input type=text id=txtSampleDate name=txtSampleDate size=10 value='<%=Request.Form("txtSampleDate")%>'></td>
			<td align=center><input type=text id=txtReceivedDate name=txtReceivedDate size=10 value='<%=Request.Form("txtReceivedDate")%>'></td>
		</tr>
		<tr>
			<td colspan=6 align=center>
				<input type=button id=cmdReset name=cmdReset value='  Clear  ' LANGUAGE=javascript onclick="return cmdReset_onclick()">&nbsp;&nbsp;<input type=submit id=cmdSearch name=cmdSearch value='Search'>
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
