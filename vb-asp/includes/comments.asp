<%
dim intDisplayCommentsCount, siteID, TestID
	intDisplayCommentsCount=0
	siteID=1

Function ExtractRubbish(strText)
	Do Until Instr(strText,Chr(39))=0
		strText=Left(strText,Instr(strText,Chr(39))-1) & Mid(strText,Instr(strText,Chr(39))+1)
	Loop
	Do Until Instr(strText,vbcrlf)=0
		strText=Left(strText,Instr(strText,vbcrlf)-1) & "<br>" & Mid(strText,Instr(strText,vbcrlf)+2)
	Loop
	ExtractRubbish=strText	
End Function

Function DisplayComments(strSampleID, strSiteID, strType, strTestID, blnAllowAdd, strTypesToShow, blnFreeform, strTabIndex)
  if strTestID = "270" then
		TestID = "270"
	end if
	
	if blnAllowAdd then
		intDisplayCommentsCount=intDisplayCommentsCount+1
		Response.Write "<div id=divComments" & intDisplayCommentsCount & " name=divComments" & intDisplayCommentsCount & ">" & vbcrlf
	end if
	Response.Write GetComments(strSampleID, strSiteID, strType, strTypesToShow)
	Response.Write "</div>" & vbcrlf
	if blnAllowAdd then
		Response.Write "</div>" & vbcrlf
		Response.Write "<br><input type=button id=cmdComments" & intDisplayCommentsCount & " name=cmdComments" & intDisplayCommentsCount & " value='Add/ Update Comments' language=javascript onclick='GetComments" & intDisplayCommentsCount & "();'"
		if len(strTabIndex) >  0 then
			Response.Write " tabindex=" & strTabIndex
		end if
		Response.Write ">" & vbcrlf 
		Response.Write "<script language=javascript>" & vbcrlf
		Response.Write "function GetComments" & intDisplayCommentsCount & "() {" & vbcrlf
		Response.Write "var strReturn;" & vbcrlf
		Response.Write "window.open('" & Application("URL") & "includes/comments.asp?SampleID=" & strSampleID & "&SiteID=" & strSiteID & "&Type=" & strType & "&TestID=" & strTestID & "&divComments=" & intDisplayCommentsCount & "&TypesToShow=" & strTypesToShow & "&Freeform=" & blnFreeform & "','_blank','height=700, width=500,scrollbars=yes, status=no,toolbar=no,menubar=no,location=no');" & vbcrlf		
		Response.Write "}" & vbcrlf
		Response.Write "</script>" & vbcrlf
	end if
End Function

Function GetComments(strSampleID, strSiteID, strType, strTypesToDisplay)
	dim strCommentsSQL, rsComments
	dim strDisplayType, intLoop, strTypesToShow
	GetComments=""
	strTypesToShow=strTypesToDisplay
	if strTypesToShow="labr" then
		strCommentsSQL="SELECT allsamplecomments.*"
		strCommentsSQL = strCommentsSQL & ", test.name"
		strCommentsSQL = strCommentsSQL & " FROM allsamplecomments" 
		strCommentsSQL = strCommentsSQL & " LEFT OUTER JOIN TEST ON allsamplecomments.TESTID = TEST.ID"
	else
		strCommentsSQL="SELECT allsamplecomments.* FROM allsamplecomments"
	end if
	strCommentsSQL = strCommentsSQL & " WHERE SampleID=" & strSampleID & " AND SiteID=" & strSiteID
	
	if len(strTypesToShow)>0 then
    if left(strTypesToShow,1)="~" then
      strCommentsSQL=strCommentsSQL & " and CommentArea NOT IN("
      strTypesToShow=mid(strTypesToShow, 2)
    else
      strCommentsSQL=strCommentsSQL & " and CommentArea IN("
    end if
    if instr(strTypesToShow, "|") > 0 then
      strDisplayType=split(strTypesToShow, "|")
      for intLoop=0 to ubound(strDisplayType)
        if intLoop>0 then
          strCommentsSQL=strCommentsSQL & ","
        end if
        strCommentsSQL=strCommentsSQL & "'" & strDisplayType(intLoop) & "'"
      next
    else
      strCommentsSQL=strCommentsSQL & "'" & strTypesToShow & "'"
    end if
    strCommentsSQL=strCommentsSQL & ")"
	end if
	strCommentsSQL=strCommentsSQL & " ORDER BY commentarea desc, testID"
	set dbComments=server.CreateObject("ADODB.Connection")
	dbComments.Open Application("dbLUBELAB_ConnectionString")
	set rsComments=dbComments.Execute (strCommentsSQL)
	do until rsComments.eof
		GetComments=GetComments & "<font class='" & rsComments("CommentArea") & "'>"
		if trim("" & rsComments("TestID"))<>"" then
			GetComments=GetComments & "(" & rsComments("NAME") & "): "
		end if	
		GetComments=GetComments & rsComments("Comment")
		GetComments=GetComments & "</font>"
		rsComments.movenext
		if not rsComments.eof then
			GetComments=GetComments & "<BR>"
		end if
	loop
	rsComments.close
	set rsComments=nothing
	dbComments.close
	set dbComments=nothing
End Function

Function GetDummySampleID()
	dim strCommentsSQL, rsComments, intDummySampleID
	ClearOldComments
	intDummySampleID=-1
	strCommentsSQL="SELECT MIN(SAMPLEID) AS DUMMYSAMPLEID FROM allsamplecomments WHERE SAMPLEID<0"
	set dbComments=server.CreateObject("ADODB.Connection")
	dbComments.Open Application("dbLUBELAB_ConnectionString")
	set rsComments=dbComments.Execute (strCommentsSQL)
	if not isnull(rsComments("DUMMYSAMPLEID")) then
		intDummySampleID= (rsComments("DUMMYSAMPLEID")-1)
	end if
	strCommentsSQL="INSERT INTO allsamplecomments (SampleID, SiteID, CommentArea, Comment, CommentDate, UserID) values ("
	strCommentsSQL=strCommentsSQL & intDummySampleID & ",-1,'DUMB','DUMMY SAMPLE ID',getDate(),'DUMB')"
	dbComments.Execute strCommentsSQL
	GetDummySampleID=intDummySampleID
	rsComments.close
	set rsComments=nothing
	dbComments.close
	set dbComments=nothing
End Function

Function ClearOldComments()
	dim strCommentsSQL, rsComments
	strCommentsSQL="SELECT DISTINCT sampleID FROM allsamplecomments WHERE sampleid<0 and DATEDIFF(day,commentdate,getdate())>1"
	set dbComments=server.CreateObject("ADODB.Connection")
	dbComments.Open Application("dbLUBELAB_ConnectionString")
	set rsComments=dbComments.Execute (strCommentsSQL)
	do until rsComments.eof
		strCommentsSQL="DELETE FROM allsamplecomments WHERE sampleid=" & rsComments("sampleid")
		dbComments.Execute strCommentsSQL
		rsComments.Movenext
	loop
	rsComments.close
	set rsComments=nothing
	dbComments.close
	set dbComments=nothing
End Function

Function UpdateDummySampleID (strDummyID, strSampleID)
	dim strCommentsSQL
	strCommentsSQL="DELETE FROM allsamplecomments WHERE sampleID=" & strDummyID & " and SiteID=-1 and CommentArea='DUMB' and UserID='DUMB'"
	set dbComments=server.CreateObject("ADODB.Connection")
	dbComments.Open Application("dbLUBELAB_ConnectionString")
	dbComments.Execute strCommentsSQL
	strCommentsSQL="UPDATE allsamplecomments SET SampleID=" & strSampleID & " WHERE sampleid=" & strDummyID
	dbComments.Execute strCommentsSQL
	dbComments.close
	set dbComments=nothing
End Function
%>

<%
if (Request.QueryString("SampleID")<>"" and Request.QueryString("Type")<>"") or _
		(Request.Form("cmdUpdate")<>"" or Request.form("cmdCancel")<>"") then
	dim strCommentsSQL, rsComments, intCommentLoop, dbComments, rsMadeComments, blnComments
	if Request.Form("cmdUpdate")<>"" then
		set dbComments=server.CreateObject("ADODB.Connection")
		dbComments.Open Application("dbLUBELAB_ConnectionString")
		' Update the database!
		' The easiest way of doing this is to delete all of the original comments
		'  and replace them with the assigned ones!
		strCommentsSQL="DELETE FROM allsamplecomments where sampleid=" & Request.QueryString("SampleID") & " and commentarea='" & Request.QueryString("Type") & "'"
		if trim(Request.QueryString("TestID"))<>"" then
			strCommentsSQL=strCommentsSQL & " and TestID=" & Request.QueryString("TestID")
		end if
		dbComments.execute strCommentsSQL
		' now insert all of the records!
		for intCommentLoop=0 to Request.Form("hidMaxComm")
			if ucase(Request.Form("chkComm" & intCommentLoop))="ON" then
				strCommentsSQL="INSERT INTO allsamplecomments (SampleID,SiteID, CommentArea,CommentID,TestID,Comment, CommentDate, UserID) values ("
				strCommentsSQL=strCommentsSQL & Request.QueryString("SampleID") & "," & Request.QueryString("SiteID") & ",'" & Request.QueryString("Type") & "',"
				strCommentsSQL=strCommentsSQL & Request.Form("hidCommID" & intCommentLoop) & ","
				if Request.QueryString("TestID")="" then
					strCommentsSQL=strCommentsSQL & "Null"
				else
					strCommentsSQL=strCommentsSQL & Request.QueryString("TestID")
				end if
				strCommentsSQL=strCommentsSQL & ",'" & Request.Form("hidComm" & intCommentLoop) & "',getdate(),'" & session("USR") & "')"
				dbComments.execute strCommentsSQL
			end if
		next
		' now for the freeform comment
		if trim(Request.Form("txtComment"))<>"" then
				strCommentsSQL="INSERT INTO allsamplecomments (SampleID,SiteID, CommentArea,CommentID,TestID,Comment, CommentDate, UserID) values ("
				strCommentsSQL=strCommentsSQL & Request.QueryString("SampleID") & "," & Request.QueryString("SiteID") & ",'" & Request.QueryString("Type") & "',"
				strCommentsSQL=strCommentsSQL & "Null,"
				if Request.QueryString("TestID")="" then
					strCommentsSQL=strCommentsSQL & "Null"
				else
					strCommentsSQL=strCommentsSQL & Request.QueryString("TestID")
				end if
				strCommentsSQL=strCommentsSQL & ",'" & ExtractRubbish(Request.Form("txtComment")) & "',getdate(),'" & session("USR") & "')"
				dbComments.execute strCommentsSQL
		end if
		dbComments.close
		set dbComments=nothing
	end if
	if Request.Form("cmdUpdate")<>"" or Request.form("cmdCancel")<>"" then
%>
	<script language=javascript>
		opener.divComments<%=request.querystring("divComments")%>.innerHTML="<%=GetComments(Request.QueryString("SampleID"), request.querystring("SiteID"), request.querystring("Type"),request.querystring("TypesToShow"))%>";
		window.close();
	</script>

<%			
	end if
	' if we get here, we are in add/ display mode...!
	strCommentsSQL="SELECT * FROM comments WHERE area='" & Request.querystring("Type") & "' ORDER BY remark"
	set dbComments=server.CreateObject("ADODB.Connection")
	dbComments.Open Application("dbLUBELAB_ConnectionString")
	set rsComments=dbComments.Execute(strCommentsSQL)
	strCommentsSQL="SELECT * FROM allsamplecomments WHERE SampleID=" & Request.QueryString("SampleID") & " and SiteID=" & Request.querystring("SiteID") & " and CommentArea='" & Request.QueryString("Type") & "'"
	if trim(Request.QueryString("TestID"))<>"" then
		strCommentsSQL=strCommentsSQL & " and TestID=" & Request.QueryString("TestID")
	end if
	blnComments=false
	set rsMadeComments=dbComments.Execute(strCommentsSQL)
	if not rsMadeComments.eof then blnComments=true
%>
	<html>
		<head>
			<title>Add/ Modify Comments</title>
			<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
<SCRIPT ID=clientEventHandlersJS LANGUAGE=javascript>
<!--
function document_onkeypress(){
//validate key presses in page

  if ((window.event.srcElement.getAttribute('type')=='text') || (window.event.srcElement.getAttribute('type')=='textarea'))
  {
 		if (event.keyCode>= 97 && event.keyCode <= 122)
		{
			event.keyCode -= 32;
    }
  }
}
//-->
</SCRIPT>
<SCRIPT LANGUAGE=javascript FOR=document EVENT=onkeypress>
<!--
 document_onkeypress()
//-->
</SCRIPT>
		</head>

	<body>
			<p align=center>Select from the comments below:</p>
				<form method=post id=frmComments name=frmComments action="<%=Application("URL")%>includes/comments.asp?SampleID=<%=Request.QueryString("SampleID")%>&SiteID=<%=request.querystring("SiteID")%>&Type=<%=Request.QueryString("Type")%>&TestID=<%=Request.QueryString("TestID")%>&divComments=<%=Request.QueryString("divComments")%>&TypesToShow=<%=request.querystring("TypesToShow")%>&Freeform=<%=request.querystring("Freeform")%>">
				<table width=80% align=center cols=2>
<%
			intCommentLoop=0
			do until rsComments.EOF
				Response.Write "<tr><td width='10%'><input type=checkbox id=chkComm" & intCommentLoop & " name=chkComm" & intCommentLoop
				if blnComments then rsMadeComments.Movefirst
				do until rsMadeComments.EOF
					if rsMadeComments("CommentID")=rsComments("ID") then 
						Response.Write " checked"
						exit do
					end if
					rsMadeComments.movenext
				loop
				Response.Write ">"
				Response.Write "<input type=hidden id=hidComm" & intCommentLoop & " name=hidComm" & intCommentLoop & " value='" & rsComments("Remark") & "'>"
				Response.Write "<input type=hidden id=hidCommID" & intCommentLoop & " name=hidCommID" & intCommentLoop & " value=" & rsComments("ID") & ">"
				Response.Write "</td>"
				Response.Write "<td>&nbsp;" & rsComments("Remark") & "</td></tr>" & vbcrlf
				rsComments.movenext
				intCommentLoop=intCommentLoop+1
			loop
			Response.Write "<input type=hidden id=hidMaxComm name=hidMaxComm value=" & intCommentLoop-1 & ">"
			if lcase(Request.QueryString("Freeform"))="true" then
				Response.Write "<tr><td colspan=2><b><i>Free form comments:</i></b></td></tr>"
				Response.Write "<tr><td colspan=2><textarea cols=50 rows=4 id=txtComment name=txtComment>"
				if blnComments then rsMadeComments.Movefirst
				do until rsMadeComments.EOF
					if ("" & rsMadeComments("CommentID"))="" then 
						Response.Write replace(rsMadeComments("Comment"),"<br>",vbcrlf)
						exit do
					end if
					rsMadeComments.movenext
				loop
				Response.Write "</textarea></td></tr>"
			end if
%>			
			<tr>
				<td colspan=2><input type=submit id=cmdUpdate name=cmdUpdate value='Update'>&nbsp;<input type=submit id=cmdCancel name=cmdCancel value='Cancel'></td>
			</tr>
			</table>
			</form>
		</body>
	</html>
<%
	rsComments.close
	set rsComments=nothing
	dbComments.Close
	set dbComments=nothing
end if
%>