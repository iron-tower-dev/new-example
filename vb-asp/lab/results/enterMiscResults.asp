<%@ Language=VBScript %>
<%Option Explicit%>
<!--enterMiscResults.asp-->
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<!--#include virtual="includes/cnr.asp"-->
<!--#include virtual="includes/saveResultsFunctions.asp"-->
<!--#include virtual="includes/comments.asp"-->

<%dim strTestID,strTestName,strSampleID,strDisplay
dim conn,rs,sql
dim strLubetype,strQClass,strApplID,strTag,strComp,strLoc
dim strCNRLevel,strCNRText,strCNRColor,strEQID,fColor
dim strMode,strCompName,strLocName,strComment
dim iloop,strDBError,strSQLFailed,blnInvalidSample
dim sid,tid,datenow,blnSaved,iTrial,status
dim strEquipment_id, rsEQID, rsSample, equipid
dim lngComment

strMode=Request.QueryString("mode")
blnInvalidSample=false
if strMode="save" then
  strSampleID=Request.Form("hidSampleID")
  strTestID=Request.Form("hidTestID")
  datenow=FormatDateTime(now(),2)
  strComment=Request.Form("hidComment")
  strDBError=""
  blnSaved=true

  select case Request.Form("hidMode")
    case "entry"
      select case qualified(strTestID)
        case "Q/QAG"
          status="S"
          enterReadings false
        case else
          strDBError="You are not authorized to enter these results"
      end select
  end select
  if len(strDBError)>0 then
    blnSaved=false
  end if
  if blnSaved=false then
    deleteRecords()
  end if
  strTestName=Request.Form("hidTestName")
  strTag=Request.Form("hidTag")
  strComp=Request.Form("hidComp")
  strLoc=Request.Form("hidLoc")
else
  blnSaved=false
  strTestID=280
  strSampleID=Request.QueryString("sid")
  if len(strSampleID)=0 then
    strSampleID=Request.Form("txtSample")
  end if
  strTestName="Misc. Results"
  strTag=Request.QueryString("tag")
  strComp=Request.QueryString("comp")
  strLoc=Request.QueryString("loc")
end if
strComment=""
if len(strSampleID)>0 then
  sql="SELECT u.lubeType,p.qualityClass,c.name AS compname,l.name AS locname,a.comment remark,u.component,u.location,u.tagnumber "
  sql=sql&"FROM UsedLubeSamples u LEFT OUTER JOIN "
  sql=sql&"Lube_Sampling_Point p ON u.tagNumber = p.tagNumber "
  sql=sql&"AND u.component = p.component AND u.location = p.location LEFT OUTER JOIN "
  sql=sql&"Component c ON u.component = c.code LEFT OUTER JOIN "
  sql=sql&"Location l ON u.location = l.code LEFT OUTER JOIN "
  sql=sql&"allsamplecomments a ON u.id = a.sampleid "
  sql=sql&"WHERE u.ID=" & strSampleID

  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  if IsOpen(conn) then
    set rs=ForwardOnlyRS(sql,conn)
    if rs.EOF then
      blnInvalidSample=true
      strSampleID=""
    else
      strLubeType=rs.Fields("lubetype")
      strQClass=rs.Fields("qualityclass")
      strCompName=rs.Fields("compname")
      strLocName=rs.Fields("locname")
      strComment=rs.Fields("remark")
      strComp=rs.Fields("component")
      strLoc=rs.Fields("location")
      strTag=rs.Fields("tagnumber")
    end if
  end if

  if not blnInvalidSample then
    sql="SELECT * FROM vwmisctesthistory WHERE id=" & strSampleID
    set rs=DisconnectedRS(sql,conn)
  end if
end if

if len(strSampleID)=0 then
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  sql="SELECT id FROM UsedLubeSamples WHERE tagnumber='" & strTag & "' AND component='" & strComp & "' AND location='" & strLoc & "' ORDER BY id DESC"
  set rsSample=DisconnectedRS(sql,conn)
end if

CloseDBObject(conn)
strApplID=GetApplID(strTag,strComp,strLoc)

if len(strApplID)>0 then
  strEQID=GetEQID(strApplID)
  equipid=GetEQTAGNUM(strApplID) 'Added function call for the next line change
  strCNRLevel=GetSeverityInfo("IDENTIFIER like '" & equipid & "*%'", strCNRText, strCNRColor, fColor)
end if

if trim(strApplID)<>"" then
	sql = "SELECT EQID FROM LUBELAB.LUBELAB_EQUIPMENT_V WHERE APPLID=" & strApplID
	conn.Open Application("dbSWMS_ConnectionString")
	Set rsEQID = conn.Execute(sql)
	if not rsEQID.eof then
	  strEquipment_id = rsEQID.Fields("EQID")
	end if
	CloseDBObject(rsEQID)
end if

set conn=nothing
%>

<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
<SCRIPT ID=clientEventHandlersJS LANGUAGE=javascript>
<!--
var blnPressed=false;
var blnSave=false;

function labmenu_onclick(strURL){
//return to lab menu
  if (window.txtsampleidcheck.value.length>0)
  {
	  if (blnPressed==true)
	  {
	  	 if(confirm('You have made changes that are not saved. Are you sure you want to continue?')==true)
	  	 {
	  	  blnPressed=false;
	  		parent.location.href=strURL;
	  	 }
	  	 else
	  	 {
	  		return false;
	  	 }
	  }
	  else
	  {
	  	parent.location.href=strURL;
	  }
	}
	else
	{
	  parent.location.href=strURL;
	}
}
function clear_onclick(){
  clear_entry_fields()
}
function clear_entry_fields(){
//clear data entry fields
var f=window.document.frmEntry;

  for (var i=0; i<f.elements.length; i++){
    if (f.elements[i].type=='text'){
      f.elements[i].value=""
    }
    if (f.elements[i].type=='checkbox'){
      f.elements[i].checked=false
    }
    if (f.elements[i].name.substring(0,3)=='lst'){
      f.elements[i].selectedIndex=0
    }
    if (f.elements[i].name.substring(0,3)=='mte'){
      f.elements[i].selectedIndex=0
    }
  }
}
function save_onclick(){
//return to lab menu
  if (perform_validation()==true){
    save_data()
  }
}
function save_data(){
var form=window.document.frmEntry;
  for (var i=0; i<form.elements.length; i++){
    if (form.elements[i].disabled==true){
      form.elements[i].disabled=false
    }
  }
  window.document.frmEntry.submit()
}
function closeWindow(){
  window.close()
}
function perform_validation(){
//validate data entered
var f=window.document.frmEntry;
var fldname,row;

var intCount;
var lngNum1;
var lngNum2;
var lownum;
var highnum;
var perresult;
var intFail, intPass;
  blnSave=false;
  intCount=0;
  lngNum1=0;
  lngNum2=0;
  intFail=0;
  intPass=0;
  for (var i=0; i<f.elements.length; i++){
    fldname=f.elements[i].name;
    if (fldname.substring(0,3)=='num'){
      if (f.elements[i].value=='.'){
        f.elements[i].value='0.0'
      }
    }
    if (fldname.substring(0,3)=='chk'){
		  if (f.elements[i].checked==true){
		  	intCount++;
		  	blnSave=true;
		  }
    }
    if (f.elements[i].type=='hidden'){
      if (fldname.substring(0,3)=='req'){
        fldname=fldname.substring(3,fldname.length);
        row=fldname.charAt(fldname.length-1);
        if (f.elements[fldname].value.length<1){
          if (f.elements['chksave' + row].checked==true){
            alert('Data entry required');
            if (f.elements[fldname].disabled==false){
              f.elements[fldname].focus();
            }
            return false
          }
        }
      }
    }
    if (fldname.substring(0,3)=='lst'){
      row=fldname.charAt(fldname.length-1);
      if (f.elements[fldname].selectedIndex<1){
        if (f.elements['chksave' + row].checked==true){
          alert('Data selection required');
          if (f.elements[fldname].disabled==false){
            f.elements[fldname].focus();
          }
          return false
        }
      }
    }
  }
  if (blnSave!=true){
	  alert('Please ensure that you have checked a test to save');
	  return false;
  }
  else
  {
    return true;
  }
}
function document_onkeypress(){
//validate key presses in page
var subtype,fldname;
  blnPressed=true;
  if (window.event.srcElement.getAttribute('type')=='text'){
    fldname=window.event.srcElement.getAttribute('name');
    subtype=fldname.substring(0,3);
    if (subtype=='num'){
      if (event.keyCode<46 || event.keyCode>57 || event.keyCode==47){
        event.returnValue=false
      }
    }
    else
    {
 		if (event.keyCode>= 97 && event.keyCode <= 122)
		{
			event.keyCode -= 32;
		}
    }
  }
}
function window_onbeforeunload() {
  if (window.txtsampleidcheck.value.length>0)
  {
    if (blnPressed==true && blnSave==false)
    {
      return 'Data changed...and not saved...';
    }
  }
}
function selSample_onchange(){
  window.document.frmSample['txtSample'].value=window.document.frmSample['lstselSample'].options(window.document.frmSample['lstselSample'].selectedIndex).value
}
function window_onload() {
<%if len(strSampleID)=0 then%>
  window.document.frmSample.txtSample.focus()
<%else%>
  window.document.frmEntry.numvalue11.focus()
<%end if%>
}

//-->
</SCRIPT>
<SCRIPT LANGUAGE=javascript FOR=document EVENT=onkeypress>
<!--
 document_onkeypress()
//-->
</SCRIPT>
</HEAD>

<BODY LANGUAGE=javascript onbeforeunload="return window_onbeforeunload()" onload="return window_onload()">
<div align=center>
<%if strMode="save" then%>
  <%if blnSaved then%>
    <h2>Results saved</h2>
  <%else%>
    <h2>Save failed</h2>
    <h4><%=strDBError%></h4>
    <br>
    <h4><%=strSQLFailed%></h4>
  <%end if%>
<%end if%>
<table>
  <tr>
  <input type=hidden name=txtsampleidcheck value=<%=strSampleID%>>
<%if len(strSampleID)=0 then
  if blnInvalidSample then
    Response.Write "<h3>Sample# does not exist - enter another<h3>"
  end if
  Response.Write "<form name=frmSample method=POST action=" & chr(34) & Application("url") & "lab/results/enterMiscResults.asp?tag=" & request.querystring("tag") & "&comp=" & request.querystring("comp") & "&loc=" & Request.QueryString("loc") & chr(34) & ")'>"
  Response.Write "<td>Sample#"
  if not rsSample.EOF then
    strSampleID=getField(rsSample,"id")
    Response.Write "<SELECT NAME=lstselSample id=lstselSample language=javascript onchange='selSample_onchange()'>"
    do until rsSample.EOF
      Response.Write "<option value=" & getField(rsSample,"id") & ">" & getField(rsSample,"id") & "</option>"
      rsSample.MoveNext
    loop
  end if
  CloseDBObject(rsSample)
  set rsSample=nothing
  Response.Write "</td>"
  Response.Write "<td><input type=text name=txtSample value='" & strSampleID & "'>"
  strSampleID=""
  Response.Write "</td>"
  Response.Write "<td><input type=submit name=cmdSubmitSample value='Go'>"
  Response.Write "</td>"
  Response.Write "</form>"
  Response.Write "</tr>"
  Response.Write "</table>"
  Response.Write "<hr>"
  Response.Write "Select or type Sample # and click Go"
  Response.Write ""
else%>
    <th>Sample#</th>
    <th>EQID</th>
    <th>Component</th>
    <th>Location</th>
    <th>Lube type</th>
    <th>Quality class</th>
    <th>CNR</th>
    <th>Lab comment</th>
  </tr>
  <tr>
    <td><%=strSampleID%></td>
    <td><%=strTag%></td>
    <td><%=(strCompName & "(" & strComp & ")")%></td>
    <td><%=(strLocName & "(" & strLoc & ")")%></td>
    <td><%=strLubetype%></td>
    <td><%=strQClass%></td>
    <%if trim(strCNRText)<>"" then%>
      <a href='<%=strCNRLevel%>' target=_blank>
          <td style="BACKGROUND-COLOR:<%=strCNRColor%>;color:<%=fColor%>"><%=strCNRText%></td>
      </a>
	<%else%>
		  <td>&nbsp;</td>
    <%end if%>
  <td>		<%DisplayComments strSampleID,siteID,"labr","",false,"labr",false,""%>
  </td>
  </tr>
  <tr>
  </tr>
</table>

  <%if len(strSampleID)>0 then%>
<form name=frmEntry method=post action="<%=Application("url")%>lab/results/enterMiscResults.asp?mode=save&tag=<%=request.querystring("tag")%>&comp=<%=request.querystring("comp")%>&loc=<%=Request.QueryString("loc")%>">
  <table>
  <tr>
    <th>Test</th>
    <th>Result</th>
    <th>Select</th>
  </tr>
  <tr>
    <td>Resistivity</td>
    <td><input type=text name=numvalue11 value='<%=getField(rs,"resistivity")%>'><input type=hidden name=reqnumvalue11></td>
    <%if strMode="save" then
		Response.Write "<td>&nbsp;</td>"
	else
		Response.Write "<td><input type=checkbox name=chksave1 checked value='on'></td>"
	end if
	%>
  </tr>
  <tr>
    <td>Chloride</td>
    <td><input type=text name=numvalue12 value='<%=getField(rs,"chlorides")%>'><input type=hidden name=reqnumvalue12></td>
    
    <%if strMode="save" then
		Response.Write "<td>&nbsp;</td>"
	else
		Response.Write "<td><input type=checkbox name=chksave2 checked value='on'></td>"
	end if
	%>
  </tr>
  <tr>
    <td>Amine</td>
    <td><input type=text name=numvalue13 value='<%=getField(rs,"amine")%>'><input type=hidden name=reqnumvalue13></td>
    <%if strMode="save" then
		Response.Write "<td>&nbsp;</td>"
	else
		Response.Write "<td><input type=checkbox name=chksave3 value='on'></td>"
	end if
	%>
  </tr>
  <tr>
    <td>Phenol</td>
    <td><input type=text name=numvalue14 value='<%=getField(rs,"phenol")%>'><input type=hidden name=reqnumvalue14></td>
    <%if strMode="save" then
		Response.Write "<td>&nbsp;</td>"
	else
		Response.Write "<td><input type=checkbox name=chksave4 value='on'></td>"
	end if
	%>
  </tr>
  </table>

  <input type=hidden name=hidSampleID value=<%=strSampleID%>>
  <input type=hidden name=hidTestID value=<%=strTestID%>>
  <input type=hidden name=hidMode value=<%
    if strMode="review" then
      Response.Write "review"
    else
      Response.Write "entry"
    end if
  %>>
  <input type=hidden name=hidTag value=<%=strTag%>>
  <input type=hidden name=hidComp value=<%=strComp%>>
  <input type=hidden name=hidLoc value=<%=strLoc%>>
  <input type=hidden name=hidTestName value=<%=strTestName%>>
  <input type=hidden name=hidComment value=<%=lngComment%>>

<table>
  <tr>
<%if not blnSaved then
    Response.Write "<td colspan=3><input type=button value='Save' name=save onclick='save_onclick()'>&nbsp;"
    Response.Write "<input type=button value='Clear' name=clear onclick='clear_onclick()'>&nbsp;"
    Response.Write "<input type=button value='Change Sample#' name=labmenu onclick='labmenu_onclick(" & chr(34) & Application("url") & "lab/results/enterMiscResults.asp" & chr(34) & ")'>&nbsp;"
    Response.Write "<input type=button value='Lab Menu' name=labmenu onclick='labmenu_onclick(" & chr(34) & Application("url") & "lab" & chr(34) & ")'></td>"
  else
    Response.Write "<td colspan=3><input type=button value='Sample from another EQID' name=labmenu onclick='labmenu_onclick(" & chr(34) & Application("url") & "lab/results/enterMiscResults.asp" & chr(34) & ")'>&nbsp;"
    if trim(Request.QueryString("tag"))<>"" then
		Response.Write "<br><input type=button value='            Addition Entry              ' name=addition onclick='labmenu_onclick(" & chr(34) & Application("url") & "lab/results/enterMiscResults.asp?tag=" & request.querystring("tag") & "&comp=" & request.querystring("comp") & "&loc=" & Request.QueryString("loc") & chr(34) & ")'>&nbsp;"
	end if
	Response.Write "<input type=button value='Lab Menu' name=labmenu onclick='labmenu_onclick(" & chr(34) & Application("url") & "lab" & chr(34) & ")'>"
    Response.Write "</td>"
  end if
  %>
  </tr>
</table>
<hr>
<table>
  <tr>
    <td rowspan=4>**NOTICE**</td>
    <td>THESE TESTS ARE PERFORMED BY OFF SITE LABORATORIES.</td>
  </tr>
  <tr>
    <td>Test data is considered Non Quality and receives no further review or verification after data entry.</td>
  </tr>
  <tr>
    <td>Please utilize STAR when entering and/or modifying data.</td>
  </tr>
  <tr>
    <td>VERIFY DATA PRIOR TO USE IN QUALITY RELATED FUNCTIONS</td>
  </tr>
</table>
<%end if%>
</form>
<%end if%>
<form name=frmHidden>
<input type=hidden name=url value="<%=Application("url") & "lab/results/lubePointHistory.asp?tag=" & strTag & "&comp=" & strComp & "&loc=" & strLoc & "&sid=" & strSampleID & "&tid=" & strTestID & "&tname=" & strTestName & "&cname=" & strCompName & "&lname=" & strLocName%>">
</form>
</div>
</BODY>
<%CloseDBObject(rs)
set rs=nothing
CloseDBObject(rsComments)
set rsComments=nothing
%>
</HTML>
<!--#include virtual="includes/footer.js"-->
<%if len(strSampleID)>0 then%>
<SCRIPT LANGUAGE=javascript>
var strURL;
  strURL=window.document.frmHidden.url.value;
</SCRIPT>
<%end if%>