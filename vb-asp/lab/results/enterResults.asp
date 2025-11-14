<%@ Language=VBScript %>
<%Option Explicit%>
<script src="../../includes/jquery-3.7.1.js"></script>
<!--enterResults.asp-->
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<!--#include virtual="includes/cnr.asp"-->
<!--#include virtual="includes/enterResultsFunctions.asp"-->
<!--#include virtual="includes/saveResultsFunctions.asp"-->
<!--#include virtual="includes/updateswmsmte.asp"-->
<!--#include virtual="includes/scheduleFunctions.asp"-->
<!--#include virtual="includes/comments.asp"-->
<!--#include virtual="includes/constants.asp"-->
<%dim strTestID,strTestName,strSampleID,strDisplay,strHistory,strConcise
dim conn,rs,sql, rsQClass,blnOldTest
public connStr 'share with java
dim strLubetype,strQClass,strApplID,strTag,strComp,strLoc,strStatusCode
dim strCNRLevel,strCNRText,strCNRColor,strEQID, equipid,fColor
dim strMode,strCompName,strLocName,strComment,strmediaready,strDelete
dim rows,iloop,strDBError,strSQLFailed,subRows
dim sid,tid,datenow,blnSaved,iTrial,status
dim strEquipment_id,rsEQID,strNewUsed, rsSample
dim overall,strSubDir,strMacro,focusfield
dim filename,filepath,extension, blnMicrNeeded
const maxrows=4

overall=Request.QueryString("overall")
strMode=Request.QueryString("mode")
strStatusCode=Request.QueryString("statuscode")
strHistory=Request.QueryString("history")
strConcise=Request.QueryString("concise")
if strMode="blank" then
  Response.redirect(Application("url") & "lab/blank.asp?mode=menu")
end if
if strMode="save" then
  strSampleID=Request.Form("hidSampleID")
  strTestID=Request.Form("hidTestID")
  blnMicrNeeded = false
  if strTestID = "120" or strTestID = "180" or strTestID = "210" or strTestID = "240" THEN
		blnMicrNeeded = true		
  end if
  rows=Request.Form("hidRows")
  subRows=Request.Form("hidSubRows")
  datenow=FormatDateTime(now(),2)
  overall=Request.Form("hidOverall")
  strDBError=""
  blnSaved=true
  strTestName=Request.Form("hidTestName")
  strTag=Request.Form("hidTag")
  strComp=Request.Form("hidComp")
  strLoc=Request.Form("hidLoc")
	strmediaready = Request.Form("hidmediaready")
	strDelete = Request.Form("hiddelete")
  select case Request.Form("hidMode")
    case "entry"
      select case qualified(strTestID)      
        case "Q/QAG", "MicrE"
          if Request.Form("hidpartial")="y" then
            if strTestID="210" then
			  status="P"
			  enterReadings true
			  markReadyForMicroscope strSampleID, strTestID, status
			else							
			  status = "A"
			  enterReadings true
			  markReadyForMicroscope strSampleID, strTestID, Status
			end if
          else 'not a partial save
			if strDelete = "y" Then
			  deleteSelectedRecords
			else
			  if strmediaready = "y" Then 
'				if strTestID="210" then
				  enterReadings true
'				end if
				markReadyForMicroscope strSampleID, strTestID, "E"
			  else	
				status="S"
				enterReadings false
				UpdateMTE strSampleID, strTestID, datenow
			  end if
			end if
          end if
        case "TRAIN"
          if Request.Form("hidpartial")="y" then
			status="A"
            enterReadings true
          else          
            status="T"
            if strmediaready = "y" Then
              enterReadings true
			  if strTestID = "210" then							
				markReadyForMicroscope strSampleID, strTestID, "E"
			  else
				markReadyForMicroscope strSampleID, strTestID, "T"
			  end if
            else
	          enterReadings false
		      UpdateMTE strSampleID, strTestID, datenow
		    end if
          end if
        case else
		  strDBError="You are not authorized to enter these results"
      end select
    case "reviewaccept"
      select case qualifiedToReview(strSampleID,strTestID)
        case "Q/QAG", "MicrE"
		  if strTestID="210" and qualifiedToReview(strSampleID,strTestID) = "Q/QAG" THEN
			status="P"
			markReadyForMicroscope strSampleID, strTestID, status
		  elseif (strTestID="120" or strTestID="180" or strTestID="240") THEN
			status="E"
			enterReadings false
			markReadyForMicroscope strSampleID, strTestID, status
		  else
			status="S"
	        validateReadings
		  end if
        case else
          strDBError="You are not authorized to review these results"
      end select
    case "reviewreject"
      select case qualifiedToReview(strSampleID,strTestID)
        case "Q/QAG", "MicrE"
          rejectReadings
        case else
          strDBError="You are not authorized to review these results"
      end select
  end select
  if len(strDBError)>0 then
    blnSaved=false
  end if
  if blnSaved=false then
    deleteRecords()
  end if
else
  blnSaved=false
  strTestID=Request.QueryString("tid")
  strSampleID=Request.QueryString("sid")
  strTestName=Request.QueryString("tname")
  strTag=Request.QueryString("tag")
  strComp=Request.QueryString("comp")
  strLoc=Request.QueryString("loc")
end if

select case qualified(strTestID)
  case "Q/QAG","TRAIN", "MicrE"
    'OK
  case else
    if strMode="view" then
      'OK
    else
      Response.Write "<h3>You are not authorized to enter these results</h3>"
	  Response.End
    end if
end select

'***************************************

strComment=""
if len(strSampleID)>0 then

  sql="SELECT u.lubeType,u.newusedflag,p.qualityClass,c.name AS compname,l.name AS locname "
  sql=sql&"FROM UsedLubeSamples u LEFT OUTER JOIN "
  sql=sql&"Lube_Sampling_Point p ON u.tagNumber = p.tagNumber "
  sql=sql&"AND u.component = p.component AND u.location = p.location LEFT OUTER JOIN "
  sql=sql&"Component c ON u.component = c.code LEFT OUTER JOIN "
  sql=sql&"Location l ON u.location = l.code "
  sql=sql&"WHERE u.ID=" & strSampleID
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  connStr = Application("dbLUBELAB_ConnectionString")
  if IsOpen(conn) then
    set rs=ForwardOnlyRS(sql,conn)
    if not(rs.EOF) then
      strLubeType=rs.Fields("lubetype")
      strCompName=rs.Fields("compname") 'GE SF-1154
      strLocName=rs.Fields("locname")  'STORE SAMPLE
      strQClass=rs.Fields("qualityclass")
      strNewUsed=rs.Fields("newusedflag")
    end if
  end if
end if

if strTestID=70 then
  if strNewUsed=0 then
    sql="SELECT details FROM vwTestScheduleDefinitionBySample WHERE sampleid=" & strSampleID & " AND testid=70 ORDER BY TestID"
  else
    sql="SELECT details FROM vwTestScheduleDefinitionByMaterial WHERE material='" & strLubeType & "' AND testid=70"
  end if
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  if IsOpen(conn) then
    set rs=DisconnectedRS(sql,conn)
    strMacro=getField(rs,"details")
  end if
  if len(strMacro)=0 then
    strMacro="UNKNOWN"
  end if
end if

blnOldTest = OldDataExists(strSampleID,strTestID)

if len(strSampleID)>0 then
  set rs=DisconnectedRS(SQLforTestID(strSampleID,strTestID,blnOldTest),conn)
end if

CloseDBObject(conn)
strApplID=GetApplID(strTag,strComp,strLoc)

if len(strApplID)>0 then
  strEQID=GetEQID(strApplID)
  equipid=GetEQTAGNUM(strApplID)
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

var blnPressed;
blnPressed=false;
var blnSave=false;

function data_entered(){
  blnPressed=true
}

function toggleHistory(){
    parent.toggle_history();
}


function labmenu_onclick(strURL){
//return to lab menu
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

function clear_onclick(){
  clear_entry_fields()
}
function clear_entry_fields(){
//clear data entry fields
var f=window.document.frmEntry;
  blnPressed=false;
  for (var i=0; i<f.elements.length; i++){
    if (f.elements[i].type=='text'){
      f.elements[i].value=""
    }
    if (f.elements[i].type=='checkbox'){
	  <%
	  if strTestID="50" or strTestID="60" then
	  %>  
        if (f.elements[i].name!='chksave1' && f.elements[i].name!='chksave2'){
          f.elements[i].checked=false
        }
      <% 
      else
      %>
        if (f.elements[i].name!='chksave1'){
          f.elements[i].checked=false
        }
      <% 
      end if
      %>
    }
    if (f.elements[i].name.substring(0,3)=='lst'){
      f.elements[i].selectedIndex=0
    }
    if (f.elements[i].name.substring(0,3)=='mte'){
      f.elements[i].selectedIndex=0
    }
	<%
	if strTestID="220" then
	%>
	  if (f.elements[i].name.substring(0,3)=='rad'){
	    f.elements[i].checked=false
	  }
	<%
	end if
	%>
    <%
	if strTestID="210" then
	%>
	  if (f.elements[i].name.substring(0,3)=='rad'){
	      f.elements[i].checked = false
	  }
	  if (f.elements[i].name.substring(0,3)=='txt'){
	      if (f.elements[i].name.substring(3,7)=='Main' || f.elements[i].name.substring(3,7)=='main' || f.elements[i].name.substring(3,7)=='eval' || f.elements[i].name.substring(3,7)=='narr'){
	          f.elements[i].value=""
	      }
      }
	  if (f.elements[i].name.substring(0,4)=='comm'){
	      f.elements[i].value=""
	  }
	  <%
	end if
	%>
  }
}
function validateFerrogramComments(limits)
{
	var f=window.document.frmEntry.tostring;
	var old = window.document.frmEntry.txtMainCommentscntr.value;
	window.document.frmEntry.txtMainCommentscntr.value=window.document.frmEntry.txtMainComments1.value.length;  
	if(eval(window.document.frmEntry.txtMainCommentscntr.value) > limits && old <= limits) 
	{
		/* message every time new character goes over limit and wasn't already */
		alert('Too much data in the text box!');
		window.document.frmEntry.txtMainCommentscntr.style.fontWeight = 'bold';
		window.document.frmEntry.txtMainCommentscntr.style.color = '#ff0000'; 
  }
	else if(eval(window.document.frmEntry.txtMainCommentscntr.value) <= limits) 
  {
	    window.document.frmEntry.txtMainCommentscntr.style.fontWeight = 'normal';
	    window.document.frmEntry.txtMainCommentscntr.style.color = '#000000'; 
	} 
}
function save_onclick(){
//save data if validation passed
  if (perform_validation(false)==true){
    save_data(false)
  }
}
function mediaready_onclick(){
  window.document.frmEntry.hidmediaready.value="y";
  window.document.frmEntry.hidMode.value="entry";
  $("[name='numvalue21']").prop("disabled", false);
  window.document.frmEntry.submit();  
}
function delete_onclick(){
  window.document.frmEntry.hiddelete.value="y";
  window.document.frmEntry.hidMode.value="entry";
  window.document.frmEntry.submit();  
}
function partsave_onclick(){
//partial save of data (limited validation)
  if (perform_validation(true)==true){
    save_data(true)
  }
}
function save_data(partial){
var form=window.document.frmEntry;
  for (var i=0; i<form.elements.length; i++){
    if (form.elements[i].disabled==true){
      form.elements[i].disabled=false
    }
  }
  window.document.frmEntry.hidMode.value="entry";
  if (partial==true){
    window.document.frmEntry.hidpartial.value="y";
  }

  window.document.frmEntry.submit()  
}
function disable_leftside_fields(){
var f=window.document.frmEntry;
	for (var i=0; i<f.elements.length; i++){
		if (f.elements[i].name=='lstmajor1' || f.elements[i].name=='lstminor1' || f.elements[i].name=='lsttrace1' || f.elements[i].name=='txtnarrative1'){
			f.elements[i].disabled=true;
		}
	}
}
function acceptResults(){
  window.frmEntry.hidMode.value='reviewaccept';
  var f=window.document.frmEntry;
  for (var i=0; i<f.elements.length; i++){
    if (f.elements[i].disabled==true){
      f.elements[i].disabled=false
    }
  }
  window.document.frmEntry.submit()
}
function rejectResults(){
  window.frmEntry.hidMode.value='reviewreject';
  window.document.frmEntry.submit()
}
function closeWindow(){
  window.parent.close()
}
function perform_validation(partial){
//validate data entered
var f=window.document.frmEntry;
var fldname,row;
var commentLenvar = 1000;
var intCount;
var lngNum1;
var lngNum2;
var lownum;
var highnum;
var perresult;
var intFail, intPass;
var blnEmpty;
  blnSave=false;
  intCount=0;
  lngNum1=0;
  lngNum2=0;
  intFail=0;
  intPass=0;
<%
  if strTestID="140" then
%>
	for (var i=1; i<=4; i++){
		if (f.elements['txtid1' + i].value != '' && f.elements['txtid2' + i].value != ''){
			if (f.elements['txtid1' + i].value == f.elements['txtid2' + i].value){
				alert('The dropping point and the block thermometers can not be the same.');
				f.elements['txtid2' + i].focus();
				return false;
			}
		}
	}
<%
  end if
%>
  for (var i=0; i<f.elements.length; i++){
    fldname=f.elements[i].name;
    if (fldname.substring(0,3)=='num' && partial==false){
      if (f.elements[i].value=='.'){
        f.elements[i].value='0.0'
      }
    }
    if (fldname.substring(0,3)=='chk'){
		if (f.elements[i].checked==true){
			intCount++;
			blnSave=true;
<%
	if strTestID="50" or strTestID="60" then
%>  
			if(lngNum1==0){
				lngNum1=f.elements['numvalue3' + fldname.substring(fldname.length-1,fldname.length)].value;
			}
			else {
				lngNum2=f.elements['numvalue3' + fldname.substring(fldname.length-1,fldname.length)].value;
			}
<%
	elseif strTestID="220" then
%>  
			
			if(f.elements['radvalue1' + fldname.substring(fldname.length-1,fldname.length)][0].checked){
				intPass++;	
			}
			else if(f.elements['radvalue1' + fldname.substring(fldname.length-1,fldname.length)][1].checked || f.elements['radvalue1' + fldname.substring(fldname.length-1,fldname.length)][2].checked || f.elements['radvalue1' + fldname.substring(fldname.length-1,fldname.length)][3].checked){
				intFail++;	
			}
<%
	end if
%>
		}
    }

<%
	if (strTestID="180") then
%>
      if (partial==false){
          if (f.elements['numvalue11'].value.length == 0 || f.elements['numvalue31'].value.length == 0){
              alert('Please enter Sample Size and Residue Weight');
              f.elements['numvalue11'].focus();
              return false;
          }
      }
<%
	end if
%>


<%
	if (strTestID="240") then
%>
      if (partial==false){
          if (!f.elements['radid2'][0].checked && !f.elements['radid2'][1].checked && !f.elements['radid2'][2].checked && !f.elements['radid2'][3].checked && !f.elements['radid2'][4].checked){
              alert('Please enter Volume of Oil Used');
              return false;
          }
      }
<%
    end if
%>

    if (f.elements[i].type=='hidden' && partial==false){
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
<%
	if (strTestID="180") then
%>
    if (partial==false){
<%
	end if
%>
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
<%
	if (strTestID="180") then
%>
		}
<%
	end if
%>
  }


<%
    if (strTestID="120" or strTestID="180" or strTestID="210" or strTestID="240") then
%>
  if (!f.elements['radid1'][0].checked && !f.elements['radid1'][1].checked && !f.elements['radid1'][2].checked && !f.elements['radid1'][3].checked)
{
    alert('Please select Overall Severity');
    return false;
}

if ($('input[name^="radstatus"][value="1"]:checked').length < 1){
    alert('At least 1 particle type should be characterized');
    return false;
}

var pts = '';
var position = 0;
var typeName = '';

$('input[name^="radstatus"][value="1"]:checked').each(function(index){
    var ptMessage = '';
    position = $(this).attr("name").substr(9);
    typeName = $(this).closest('tr').find('td:eq(2)').text();

    if ($('input[name=radHeat' + position + ']:checked').val() === undefined){
        ptMessage = ptMessage + "Heat. ";
    }
    if ($('input[name=radConcentration' + position + ']:checked').val() === undefined){
        ptMessage = ptMessage + "Concentration. ";
    }
    if ($('input[name="radSize, Ave' + position + '"]:checked').val() === undefined){
        ptMessage = ptMessage + "Size, Ave. ";
    }
    if ($('input[name="radSize, Max' + position + '"]:checked').val() === undefined){
        ptMessage = ptMessage + "Size, Max. ";
    }
    if ($('input[name=radColor' + position + ']:checked').val() === undefined){
        ptMessage = ptMessage + "Color. ";
    }
    if ($('input[name=radTexture' + position + ']:checked').val() === undefined){
        ptMessage = ptMessage + "Texture. ";
    }
    if ($('input[name=radComposition' + position + ']:checked').val() === undefined){
        ptMessage = ptMessage + "Composition. ";
    }
    if ($('input[name=radSeverity' + position + ']:checked').val() === undefined){
        ptMessage = ptMessage + "Severity. ";
    }

    if (ptMessage.length > 0){
      pts = pts + typeName + ": " + ptMessage + "\n\n";
    }
});
if (pts.length > 0){
    alert("The following criteria have not been entered:\n\n" + pts);
    return false;
}

<%
	end if
%>



  if (blnSave!=true){
	alert('Please ensure that you have checked a test to save');
	return false;
  }
  else{
<%
	if (strTestID="50" or strTestID="60") and (strQClass="QAG" or strQClass="Q") then
%>  
	if(intCount<2 && partial==false){
		alert('You must have two tests selected for Q/QAG samples');
		return false;
	}
	else{
		if(lngNum1!=0 && lngNum2!=0){
			if(lngNum2<lngNum1) {
				highnum=lngNum1;
				lownum=lngNum2;
			}
			else{
				highnum=lngNum2;
				lownum=lngNum1;
			}
			perresult=((highnum-lownum)/highnum)*100;
			perresult=Math.round(perresult*100)/100;
			if(perresult>0.35){
				alert('Repeatability percent is ' + perresult + ' which is above 0.35');
<%
		if strQClass="QAG" or strQClass="Q" then
%>		
			return false;
<%
		end if
%>		
			}
		}
	}
<%
	elseif (strTestID="120" or strTestID="180" or strTestID="210" or strTestID="240") then
%>  
		if(eval(window.document.frmEntry.txtMainCommentscntr.value)>eval(commentLenvar)){
			alert('Too many characters in Comments, please correct!');
			return false;
		}
<%
	elseif strTestID="220" then
%>  
		if(intFail>0 && intFail<2 && intPass<2){
			alert('Recheck tests due to failure (and check save check is on)');
			return false;
		}
<%
	end if
%>
<%
	if (strTestID="70") then
%>
  blnEmpty=true
  for (var i=0; i<f.elements.length; i++){
    fldname=f.elements[i].name;
    if (fldname.substring(0,3)=='num' && partial==false){
      if (f.elements[i].value.length>0){
        blnEmpty=false
      }
    }
  }
  if (blnEmpty){
		 return confirm('You have not entered data. Are you sure you want to save?')
  }
<%
	end if
%>
    return true;
  }
}
function FTIR_Populate(strText,fldname){
var pos,datavalue;
  pos=strText.indexOf(',');
  if ((pos==0) || (strText.length<1)){
    window.document.frmEntry.elements[fldname].value='';
  }
  else{
    if (pos==-1){
      datavalue=Math.round(strText * 100) / 100
    }
    else{
      datavalue=strText.substring(0,pos);
      datavalue=Math.round(datavalue * 100) / 100;
    }
    window.document.frmEntry.elements[fldname].value=datavalue;
  }
  return strText.substring(pos+1);
}
function listbox_onchange(dbname){
  window.document.frmEntry['txt'+dbname].value=window.document.frmEntry['lst'+dbname].options(window.document.frmEntry['lst'+dbname].selectedIndex).value
}
function document_onkeypress(){
//validate key presses in page
var subtype,fldname;
  blnPressed=true;
  if (window.event.srcElement.getAttribute('type')=='text')
  {
    fldname=window.event.srcElement.getAttribute('name');
    subtype=fldname.substring(0,3);
    if (subtype=='num')
    {
      if (event.keyCode<46 || event.keyCode>57 || event.keyCode==47)
      {
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
//VISCOSITY SPECIFIC FUNCTIONS
function VISTime_onblur(){
var f=window.document.frmEntry;
var fldname,entry,count,pos,nextpos,seconds;
//check the value in the field
  fldname=window.event.srcElement.getAttribute('name');
  entry=f.elements[fldname].value;
	pos=entry.indexOf('.');
	if (pos>-1){
	  nextpos=entry.substring(pos+1).indexOf('.');
	  if (nextpos>-1){
	    seconds=0;
	    if (pos>0){
	      seconds=(entry.substring(0,pos)) * 60
	    }
	    if (nextpos>0){
	      seconds=seconds + (1 * entry.substring(pos+1,pos+nextpos+1))
	    }
	    if (nextpos<entry.length){
	      seconds=seconds + (0.01 * entry.substring(pos+nextpos+2))
	    }
	    if (seconds.toString()=='NaN'){
	      seconds=0
	    }
	    f.elements[fldname].value=seconds
	    }
	  }

/*	if (f.elements[fldname].value.length!=0)
	{
		if(f.elements[fldname].value<=200)
			{
				alert('Stopwatch time must be greater than 200');
				f.elements[fldname].focus();
				return false;
			}
	} */
	  //now trigger the calculation
	count=fldname.charAt(fldname.length-1);
	VISResult(count);
}
function lstViscometer_onchange(){
var count,dbname,fldname;
  fldname=window.event.srcElement.getAttribute('name');
  count=fldname.charAt(fldname.length-1);
  dbname=fldname.substring(3);
  listbox_onchange(dbname);
  VISResult(count);
}
function VISResult(count){
var f=window.document.frmEntry;
var stoptime,calibration,result,pos;
var lngNum1;
var perresult;
var intCount;
lngNum1=0;
  stoptime=f.elements['numvalue1' + count].value;
  if (stoptime=='.'){
    f.elements['numvalue1' + count].value='0.0';
    stoptime=0
  }
  calibration=f.elements['txtid2' + count].value;
  pos=calibration.indexOf('|');
  calibration=new Number(calibration.substring(pos+1));
  result=calibration * stoptime;
  result=Math.round(result*100)/100;
  f.elements['numvalue3' + count].value=result;
	for (var i=0; i<f.elements.length; i++){
		fldname=f.elements[i].name;
		if (fldname.substring(0,3)=='chk'){
			if (f.elements[i].checked==true){
				intCount++;
				if(lngNum1==0){
					lngNum1=f.elements['numvalue3' + fldname.substring(fldname.length-1,fldname.length)].value;
				}
			}
		}
	}
	if(lngNum1!=0 && result!=0){
		if(result<lngNum1) {
			highnum=lngNum1;
			lownum=result;
		}
		else{
			highnum=result;
			lownum=lngNum1;
		}
		perresult=((highnum-lownum)/highnum)*100;
		perresult=Math.round(perresult*100)/100;
		if(perresult>0.35){
			alert('Repeatability percent is ' + perresult + ' which is above 0.35');
<%
	if strQClass="Q/QAG" or strQClass="Q" then
%>		
		return false;
<%
	end if
%>		
		}
	}
}
//FILTER RESIDUE SPECIFIC FUNCTIONS
function calculateFRResult(){
var fldname,count;
  fldname=window.event.srcElement.getAttribute('name');
  count=fldname.charAt(fldname.length-1);
  FRResult(count);
}
function FRResult(count){
var f=window.document.frmEntry;
var size,weight,result;
  size=f.elements['numvalue1' + count].value;
  weight=f.elements['numvalue3' + count].value;
  if (size=='.'){
    f.elements['numvalue1' + count].value='0.0';
    size=0
  }
  if (weight=='.'){
    f.elements['numvalue3' + count].value='0.0';
    weight=0
  }
  if (weight==0){
    result=0
  }
  else{
    result=(100 / new Number(size)) * weight;
    result=Math.round(result * 10) / 10;
  }
  f.elements['numvalue2' + count].value=result;
}

//FLASH POINT SPECIFIC FUNCTIONS
function lstThermometer_onchange(dbname){
var count,dbname,fldname;
  fldname=window.event.srcElement.getAttribute('name');
  count=fldname.charAt(fldname.length-1);
  dbname=fldname.substring(3);
  listbox_onchange(dbname);
  FPResult(count);
}
function calculateFPResult(){
var fldname,count;
  fldname=window.event.srcElement.getAttribute('name');
  count=fldname.charAt(fldname.length-1);
  FPResult(count);
}
function FPResult(count){
var f=window.document.frmEntry;
var pressure,fptemp,result;
  pressure=f.elements['numvalue1' + count].value;
  fptemp=f.elements['numvalue2' + count].value;
  if (pressure=='.'){
    f.elements['numvalue1' + count].value='0.0';
    pressure=0
  }
  if (fptemp=='.'){
    f.elements['numvalue2' + count].value='0.0';
    fptemp=0
  }
  result=new Number(fptemp) + (0.06 * (760 - pressure));
  results=Math.round(result);
  result=Math.round(result/2)*2;
  f.elements['numvalue3' + count].value=result;
}
//GREASE PENETRATION WORKED SPECIFIC FUNCTIONS
function calculateGPWResult(){
var fldname,count;
  fldname=window.event.srcElement.getAttribute('name');
  count=fldname.charAt(fldname.length-1);
  GPWResult(count);
}
function GPWResult(count){
var f=window.document.frmEntry;
var cone1,cone2,cone3,average,nlgi,result;
  cone1=f.elements['numvalue1' + count].value;
  cone2=f.elements['numvalue2' + count].value;
  cone3=f.elements['numvalue3' + count].value;
  if (cone1=='.'){
    f.elements['numvalue1' + count].value='0.0';
    cone1=0
  }
  if (cone2=='.'){
    f.elements['numvalue2' + count].value='0.0';
    cone2=0
  }
  if (cone3=='.'){
    f.elements['numvalue3' + count].value='0.0';
    cone3=0
  }
//calculate average for results next two lines 08/25/06 efs  
  average=Math.round((new Number(cone1) + new Number(cone2) + new Number(cone3)) / 3);
  result=(average * 3.75) + 24;
  result=Math.round(result);
  f.elements['numtrialcalc' + count].value=result;
  //now lookup NLGI
}
//GREASE DROPPING POINT SPECIFIC FUNCTIONS
function calculateGDPResult(){
var fldname,count;
  fldname=window.event.srcElement.getAttribute('name');
  count=fldname.charAt(fldname.length-1);
  GDPResult(count);
}
function GDPResult(count){
var f=window.document.frmEntry;
var odp,bt,result;
  odp=f.elements['numvalue1' + count].value;
  bt=f.elements['numvalue3' + count].value;
  if (odp=='.'){
    f.elements['numvalue1' + count].value='0.0';
    odp=0
  }
  if (bt=='.'){
    f.elements['numvalue3' + count].value='0.0';
    bt=0
  }
  result=new Number(odp) + ((bt - odp) / 3);
  f.elements['numvalue2' + count].value=Math.round(result);
}
//TAN SPECIFIC FUNCTIONS
function calculateTANResult(){
var result,sw,fb,count,fldname;
var f=window.document.frmEntry;

  fldname=window.event.srcElement.getAttribute('name');
  count=fldname.charAt(fldname.length-1);
  sw=f.elements['numvalue1' + count].value;
  fb=f.elements['numvalue3' + count].value;
  if (sw=='.'){
    f.elements['numvalue1' + count].value='0.0';
    sw=0
  }
  if (fb=='.'){
    f.elements['numvalue3' + count].value='0.0';
    fb=0
  }
  if (sw==0){
    result=0
  }
  else{
    result=Math.round(((fb * 5.61)/sw)*100)/100;
    if (result < 0.01 ){
			result = 0.01
    }
  }
  f.elements['numvalue2' + count].value=result;
}

function togglepti(index) {
    $("#pti-" + index).toggle();
    var text = $("#cmdEval" + index).attr("value");
    if (text == "Show")
      $("#cmdEval" + index).attr("value", "Hide");
    else
      $("#cmdEval" + index).attr("value", "Show");
}

function toggleshow(index, show) {
    $("#cmdEval" + index).prop("disabled", !show);
    if (!show && $("#cmdEval" + index).attr("value") == "Hide"){
        togglepti(index);
    }
}

function setSeverity(index, value) {
    $("#txteval" + index).text(value);
}

function allComments(){
    var position = 0;
    var typeName = '';
    var commentText = '';
    var fullComment = '';
    $('input[name^="radstatus"][value="1"]:checked').each(function(index){
        var ptMessage = '';
        position = $(this).attr("name").substr(9);
        typeName = $(this).closest('tr').find('td:eq(2)').text();
        commentText = $('textarea[name=comment' + position + ']').val()

        if (commentText.length > 0){
            fullComment = fullComment + '[' + typeName + "]: " + commentText + ".  ";
        }
    });
    return fullComment;
}

function addAllComments(){
    $('#fullComments').text(allComments());
}

function addToMainComment(){
    $('textarea[name=txtMainComments1]').val(allComments());
}

function window_onbeforeunload() {
  if (blnPressed==true && blnSave==false){
    return 'Data changed...and not saved...';
    }
}
function selSample_onchange(){
  window.document.frmEntry['hidSampleID'].value=window.document.frmEntry['lstselSample'].options(window.document.frmEntry['lstselSample'].selectedIndex).value;
}
function selorchkchange() {
	blnPressed=true;
}

function document_onmousedown() {
	if (window.event.srcElement.name!=null)
	{
		if (window.event.srcElement.name!='labmenu' && window.event.srcElement.name!='clear' && window.event.srcElement.name!='lstFile' && window.event.srcElement.name!='lnkFile')
		{
			blnPressed=true;
		}
	}
}
//-->
</SCRIPT>

<SCRIPT LANGUAGE=javascript FOR=document EVENT=onkeypress>
<!--
 document_onkeypress();
//-->
</SCRIPT>
<SCRIPT LANGUAGE=javascript FOR=document EVENT=onmousedown>
<!--
 document_onmousedown();
//-->
</SCRIPT>

</HEAD>

<BODY LANGUAGE=javascript onbeforeunload="return window_onbeforeunload()">
<div align=center>
<%if strMode="save" then%>
  <%if blnSaved then%>
    <h2>Results saved</h2>
    <%if overall="y" then%>
<SCRIPT LANGUAGE=javascript>
<!--
	parent.refresh_opener()
//-->
</SCRIPT>
    <%else%>
<SCRIPT LANGUAGE=javascript>
<!--
	parent.frames['fraTestSampleList'].location.reload()
//-->
</SCRIPT>
    <%end if%>
  <%else%>
    <h2>Save failed</h2>
    <h4><%=strDBError%></h4>
    <br>
    <h4><%=strSQLFailed%></h4>
  <%end if%>
<%end if%>
<table><tr>
    <td class="blank" style="vertical-align:middle"><h2><%=Request.QueryString("tname")%></h2>
        <%if (strTestID="120" or strTestID="180" or strTestID="210" or strTestID="240") then %>
        <input type=button name='cmdShowAll' id='cmdShowAll' value='Show Reviewed' onclick='toggleTypesDisplayed()' />
        <%end if %>
    </td>
<td>
<table>
  <tr>
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
    <td>
  <%Response.Write strSampleID%>
    </td>
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
    <td>
<%
  DisplayComments strSampleID,siteID,"labr",strTestID,not(strMode="view"),"labr",false,""
%>
    </td>
  <tr>
  </tr>
</table>
</td>
</tr></table>

<%
'\\pvfs-nucpro\pvnucpro\lubelab 
filepath=Application("LabPath")
select case strTestID
case 70
  filename="macro_output"
  filepath=filepath & "\FTIRDATA\Data_File\"
  extension="TXT"
end select
%>
 <form name=frmFSO>
<%if strTestID=70 then%>
<table>
  <tr>
    <th>FTIR Macro</th>
    <td><strong>&nbsp;<%=strMacro%></strong></td>
    <th>Results from file</th>
    <td><select name="lstFile" onchange='lstFile_onchange()'></select></td>
  <tr>
  <input type=hidden name="hidSampleID" value="<%=strSampleID%>">
  <input type=hidden name="hidTestID" value="70">
  <input type=hidden name="hidPath" value="<%=filepath%>">
  <input type=hidden name="hidFilename" value="<%=filename%>">
  <input type=hidden name="hidExtension" value="<%=extension%>">
  <input type=hidden name="hidRow" value="1">
  <input type=hidden name="hidData" value="">
</table>
<%end if%>
</form> 

<form name=frmEntry method=post action="<%=Application("url")%>lab/results/enterResults.asp?mode=save">
  <%buildEntryTable strTestID,rs,strSampleID,maxrows,strLubetype,strQClass,strMode,strConcise%>
  <input type=hidden name=hidSampleID value=<%=strSampleID%>>
  <input type=hidden name=hidTestID value=<%=strTestID%>>
  <input type=hidden name=hidMode value=<%
  if strMode="review" then
    Response.Write "review"
  else
    Response.Write "entry"
  end if
  %>>
  <input type=hidden name=hidRows value=<%=maxrows%>>
  <input type=hidden name=hidTag value=<%=strTag%>>
  <input type=hidden name=hidComp value=<%=strComp%>>
  <input type=hidden name=hidLoc value=<%=strLoc%>>
  <input type=hidden name=hidTestName value=<%=strTestName%>>
  <input type=hidden name=hidOverall value=<%=overall%>>
  <input type=hidden name=hidPrev210DispStat value=<%=strStatusCode%>>
  <input type=hidden name=hidSubRows value=<%=subRows%> />
<table>
  <tr>
<%
if strMode="view" then
	Response.Write "<td><input type=button value='Close' name=close onclick='closeWindow()'></td>"
else
    dim acceptRejectButtons, saveButton, clearButton, menuButton, deleteButton, partSaveButton, mediaReadyButton
    acceptRejectButtons = "<td><input type=button value='Accept' name=save onclick='acceptResults()'></td><td><input type=button value='Reject' name=save onclick='rejectResults()'></td>"
    saveButton = "<td><input type=button value='Save' name=save onclick='save_onclick()'></td>"
    partSaveButton = "<td><input type=button value='Partial Save' name=partsave onclick='partsave_onclick()'></td><td><input type=hidden value='' name=hidpartial></td>"
    clearButton = "<td><input type=button value='Clear' name=clear onclick='clear_onclick()'></td>"
    menuButton = "<input type=button value='Enter Results Menu' name=labmenu onclick='labmenu_onclick(" & chr(34) & Application("url") & "lab/results" & chr(34) & ")'>"
    deleteButton = "<td><input type=button value='Delete' name=delete onclick='delete_onclick()'></td><td><input type=hidden value='' name=hiddelete></td>"
    mediaReadyButton = "<td><input type=button value='Media Ready' name=mediaready onclick='mediaready_onclick()')></td><td><input type=hidden value='' name=hidmediaready></td>"
	if not blnSaved then
    'Display appropriate file links
		select case strTestID
		case "70"
			strSubDir="SPA\SPA_" & cstr(fix(strSampleID/1000)) & "k\"
			Response.Write "<td colspan=3 align=center><i><a HREF=" & chr(34) & Application("FTIRPath") & "\" & strSubDir & strSampleID & ".SPA" & chr(34) & " name='lnkFile'>View the sample file</a></i></td></tr><tr>"
		case "170", "230"
			strSubDir="RBOT\"
			Response.Write "<td colspan=3 align=center><i><a HREF=" & chr(34) & Application("LabPath") & "\" & strSubDir & strSampleID & ".DAT" & chr(34) & " name='lnkFile'>View the sample file</a></i></td></tr><tr>"
		end select

		dim enteredQualification, userQualification, QQAG, MicrE, TRAIN
		enteredQualification = ""
        userQualification = qualified(strTestID)
        QQAG = (userQualification = "Q/QAG")
        MicrE = (userQualification = "MicrE")
        TRAIN = (userQualification = "TRAIN")

		select case strTestID
		case "120","180","210","240"
			if strTestID="210" and not(MicrE) then
				sql = "SELECT entryID from TestReadings WHERE sampleid=" & strSampleID & " AND testid=" & strTestID & " AND trialnumber=1"
				set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
				set rs=conn.Execute(sql)
                dim enteredId
				if not rs.EOF then
					enteredId = rs("entryID").value
				end if
				CloseDBObject(rs)
				set rs=nothing
				sql="SELECT qualificationLevel FROM LubeTechQualification INNER JOIN Test ON LubeTechQualification.testStandID = Test.testStandID WHERE id=" & strTestID & " AND employeeid='" & enteredId & "'"
				set rs=ForwardOnlyRS(sql,conn)
				if not(rs.BOF and rs.EOF) then
					enteredQualification=rs.Fields("qualificationLevel")
				end if
				CloseDBObject(rs)
				CloseDBObject(conn)
				set rs=nothing
				set conn=nothing
			end if

            'Reviewing entered results
			if strMode="review" then
                'If (Ferrogram AND ( User has MicrE qual OR (User has QQAG qual AND result not entered by MicrE qual) ) ) THEN show Partial Save button
    			if (strTestID="210" and ((QQAG and enteredQualification <> "MicrE") or qualified(strTestID)="MicrE")) then
				  Response.Write partSaveButton
				end if
                'If ( (Ferrogram AND User has MicrE qual AND result status "P") OR (NOT Ferrogram AND User has MicrE qual AND result status "T" / "E") ) THEN show Save button
				if ((strTestID="210" and MicrE and strStatusCode = "P") or ((strTestID <> "210") and MicrE and (strStatusCode = "T" or strStatusCode = "E"))) then
					Response.Write saveButton
				end if
                'Show the Menu button
				Response.Write "<td>" & menuButton & "</td>"
                'If (Ferrogram AND (User has MicrE qual OR (User has QQAG qual AND result not entered by MicrE qual) ) AND result status "P") THEN show Delete button
				if strTestID="210" and ((QQAG and enteredQualification <> "MicrE") or MicrE) and strStatusCode = "P" then
					Response.Write deleteButton
				end if
                'If ( (Ferrogram AND User has MicrE qual) OR (NOT Ferrogram AND (User has QQAG qual OR (User has MicrE qual AND result status "T") ) ) THEN show Accept / Reject buttons
				if ((strTestID="210" and MicrE) or ((strTestID <> "210") and (QQAG or (MicrE and strStatusCode = "T")))) then
					Response.Write acceptRejectButtons
				end if
			else 'Entering results
                'If result has been partially saved
				if Request.Form("hidpartial")="y" then
					if MicrE then
                        'If (User has MicrE qual AND Ferrogram) THEN show Accept / Reject buttons
						if strTestID = "210" then
							Response.Write acceptRejectButtons
						else 'If (User has MicrE qual AND NOT Ferrogram) THEN show Save button
							Response.Write saveButton
						end if
					end if
                    'If Ferrogram AND User has QQAG qual AND result not entered by MicrE qual THEN show Partial Save button
					if strTestID = "210" and QQAG and enteredQualification <> "MicrE"  then
						Response.Write partSaveButton
					end if
                    'Show the Menu button
					Response.Write "<td>" & menuButton & "</td>"						
                    'Show Media Ready button
					Response.Write mediaReadyButton
				else
                    'If Ferrogram AND (User has MicrE qual OR User has TRAIN OR (User has QQAG qual and the results not entered by a MicrE qual)) AND result status not "C" THEN show Partial Save button
					if (strTestID="210" and ((QQAG and enteredQualification <> "MicrE") or MicrE or TRAIN) and strStatusCode <> "C") then
						Response.Write partSaveButton
					end if
                    if ((strTestID <> "210") and MicrE and (strStatusCode = "T" or strStatusCode = "E")) or (strTestID = "210" and MicrE) then
						Response.Write saveButton
					end if
                    ' If Ferrogram AND User has TRAIN qual Clear button
				    if strTestID="210" and TRAIN then
					    Response.Write clearButton
				    end if
                    'If result status "C" AND User has qual QQAG / TRAIN / MicrE THEN show Clear button
                    'Show the Menu button
					Response.Write "<td>" & menuButton & "</td>"
                    'If NOT( (NOT Ferrogram AND result status "E" / "C") OR (Ferrogram AND (result status NOT X OR TRAIN) ) ) THEN show Media Ready button
                    if not(((strTestID <> "210") and (strStatusCode = "E" or strStatusCode = "C")) or (strTestID = "210" and (TRAIN OR (strStatusCode <> "X")))) then
						Response.Write mediaReadyButton
					end if
                    'If ( (Ferrogram AND User has MicrE qual AND result status "E" / "C") OR (User has QQAG qual AND result status "E") ) THEN show Delete button
                    if (strTestID="210" and ((MicrE and (strStatusCode = "E" or strStatusCode = "C")) or (QQAG and strStatusCode = "E"))) then
						Response.Write deleteButton
					end if
				end if
			end if
		case else ' Tests other than "120","180","210","240"
			if strMode="review" then
				Response.Write "<td>" & menuButton & "</td>"
				if not ((strTestID="50" or strTestID="60") and TRAIN and strStatusCode = "T") then
					Response.Write acceptRejectButtons
				end if
			else
                'If (Viscosity AND NOT (User has TRAIN qual and result is not startes)) THEN show Partial Save button
				if strTestID="50" or strTestID="60" then
                    if not (TRAIN and strStatusCode <> "X") then
						Response.Write partSaveButton
					end if
				end if
                ' If NOT (User has TRAIN qual AND (result status is "C" / "E" / "P") THEN show Save and Clear buttons
				if not(TRAIN and (strStatusCode = "C" or strStatusCode = "E" or strStatusCode = "P")) then
					Response.Write saveButton
					Response.Write clearButton
				end if
                'Show the Menu button
				Response.Write "<td>" & menuButton & "</td>"
			end if
		end select
		if QQAG or MicrE then
			if strTestID<>"210" Then
                if not(QQAG and (((strTestID="120" or strTestID="180" or strTestID="240") and strStatusCode="C")) or strStatusCode="T") then
					Response.Write deleteButton
				end if
			end if
		end if
	else
		if Request.Form("hidpartial")="y" then
			select case strTestID
			case "120","180","210","240"
				if MicrE then
					Response.Write saveButton
				end if
			end select
			Response.Write "<td colspan=2>" & menuButton & "</td>"
		else
			Response.Write "<td colspan=3>" & menuButton & "</td>"
		end  if
	end if
end if%>
  </tr>
</table>
<%if strTestID="180" and not(MicrE) then%>
<script language=javascript>disable_leftside_fields();</script>
<%end if%>
</form>
<form name=frmHidden>
<%if strTestID=210 and blnOldTest then%>
 	<input type=hidden name=url value="<%=Application("url") & "analysis/ferr1.asp?fortag=" & strTag & "&forcomp=" & strComp & "&forloc=" & strLoc & "&sampleid=" & strSampleID & "&mtype=disp"%>">	
<%else%>
	<input type=hidden name=url value="<%=Application("url") & "lab/results/lubePointHistory.asp?tag=" & strTag & "&comp=" & strComp & "&loc=" & strLoc & "&sid=" & strSampleID & "&tid=" & strTestID & "&tname=" & strTestName & "&cname=" & strCompName & "&lname=" & strLocName%>">
<%end if%>
<script language=javascript>
    (function () {
        $('[id^="pti"]').each(function(index){
            $(this).toggle();
        });

        <%if (strConcise="y") then%>
        hideUnusedTypes();
        <%end if%>
    })();

    function hideUnusedTypes(){
        $('input[name^="radstatus"]:checked').each(function(index){
            if($(this).val() != 1){
                $(this).closest("tr").hide();
            }
        });
    }

    function showUnusedTypes(){
        $('input[name^="radstatus"]:checked').each(function(index){
            if($(this).val() != 1){
                $(this).closest("tr").show();
            }
        });
    }


    function toggleTypesDisplayed(){
        var text = $("#cmdShowAll").attr("value");
        if (text == "Show All"){
            showUnusedTypes();
            $("#cmdShowAll").attr("value", "Show Reviewed");
        }
        else{
            hideUnusedTypes();
            $("#cmdShowAll").attr("value", "Show All");
        }
    }

</script>
</form>
</div>
</BODY>
<%CloseDBObject(rs)
set rs=nothing
%>
</HTML>
<!--#include virtual="includes/footer.js"-->
<SCRIPT LANGUAGE=javascript>
var strURL;
strURL = window.document.frmHidden.url.value;
  <%if (strHistory<>"n") then%>
  parent.frames["fraLubePointHistory"].location.href=strURL;
  <%end if%>

<%if len(focusfield)>0 then
	Response.Write "window.document.frmEntry." & focusfield & ".focus();"
end if%>

</SCRIPT>
