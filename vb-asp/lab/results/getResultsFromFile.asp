<%@ Language=VBScript %>
<%Option Explicit%>
<!--getResultsFromFile.asp-->
<!--#include virtual="includes/security.asp"-->
<%dim sampleid,testid,filepath,message,filename,extension,row
sampleid=Request.QueryString("sid")
testid=Request.QueryString("tid")
row=Request.QueryString("row")
message=""
filepath=Application("URL") + "files"
filename=sampleid
select case testid
  case "20"
    filepath=filepath & "\KARLF\"
    extension="PVN"
  case "30"
    filepath=filepath & "\SPECTRO\"
    filename=filename & "S"
    extension="PVN"
  case "40"
    filepath=filepath & "\SPECTRO\"
    filename=filename & "L"
    extension="PVN"
  case "160"
    filepath=filepath & "\PARTICLE\"
    extension="PVN"
  case "170"
    filepath=filepath & "\RBOT\"
    extension="DAT"
  case else
    message="This test does not use file input"
end select
%>

<HTML>
<HEAD>
	<link REL="STYLESHEET" TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
	<title>Retrieve Results From File</title>
<SCRIPT ID=clientEventHandlersJS LANGUAGE=javascript>
<!--
function close_onclick(){
  window.close()
}
function lstFile_onchange(){
var strText;
strText = window['contents' + window.frmFileData.lstFile.selectedIndex].value;
if (window.frmFSO.hidTestID.value == '170') {
    var strLines = strText.split("\n");
    var maxval, maxindex, pos, currval, currindex, line;
    var strLine = strLines[3];
    pos=strLine.indexOf(',');
    maxindex=strLine.substring(0,pos);
    strLine=strLine.substring(pos+1);
    pos=strLine.indexOf(',');
    maxval = new Number(strLine.substring(0, pos));
    for (line = 4; line < strLines.length; line++){
      strLine = strLines[line];
      pos=strLine.indexOf(',');
      currindex=strLine.substring(0,pos);
      currval=new Number(strLine.substring(pos+1));
      if (currval > maxval){
        maxval=new Number(currval);
        maxindex=currindex;
      }
    }
    currval = maxval;
    maxval -= 25.4;
    line = maxindex + 3;
    while ((line < strLines.length) & (currval > maxval)){
      strLine = strLines[line];
      pos=strLine.indexOf(',');
      currindex=strLine.substring(0,pos);
      currval=new Number(strLine.substring(pos+1));
      line++;
    }
    strText="Dropping point at time: " + currindex;
  }
  else{
    while (strText.indexOf('\r') > -1){
      strText=strText.replace('\r','<br>')
    }
    while (strText.indexOf('\n') > -1){
      strText=strText.replace('\n','')
    }
  }
  window.document.getElementById("preFile").innerHTML=strText;
  window.frmFSO.hidData.value=strText;
}
function select_onclick(){
  returnResults();
  window.opener.data_entered();
  window.close()
}
function ltrim(indata){
  var data=indata;
  while (data.charAt(0)==' '){
    data=data.substring(1)
  }
  return data
}
function rtrim(indata){
  var data=indata;
  while (data.charAt(data.length-1)==' '){
    data=data.substring(0,data.length-1)
  }
  return data
}
function trim(indata){
  return rtrim(ltrim(indata))
}
function returnResults(){
var pos,data;
  data=window.frmFSO.hidData.value;
  switch (window.frmFSO.hidTestID.value){
    case '20':
      pos=data.lastIndexOf('H"O');
      if (pos>-1){
        data=data.substring(pos+3);
        pos=data.indexOf('<br>');
        if (pos>-1){
          data=data.substring(0,pos)
        }
        pos=data.indexOf('PPM');
        if (pos>-1){
          data=data.substring(0,pos)
        }
        pos=data.indexOf('%');
        if (pos>-1){
          data=(data.substring(0,pos))*10000;
        }
        data=Math.round(data);
        window.opener.document.frmEntry['numvalue1' +  window.frmFSO.hidRow.value].value=(data * 1)
      }
      break;
    case '30':
    case '40':
      pos=data.lastIndexOf('Na');
      if (pos>-1){
        var names,values,name,val;
        data=data.substring(pos);
        while (data.length>0){
          //get result names
          data=trim(data);
          pos=data.indexOf('<br>');
          names=trim(data.substring(0,pos));
          data=data.substring(pos+4);
          while (data.substring(0,4)=='<br>'){
            data=trim(data.substring(4))
          }
          //get result values
          data=trim(data);
          pos=data.indexOf('<br>');
          values=trim(data.substring(0,pos));
          data=data.substring(pos+4);
          while (data.substring(0,4)=='<br>'){
            data=trim(data.substring(4))
          }
          //update relevant fields in calling window
          while (names.length>0){
            pos=names.indexOf(' ');
            if (pos<0){
              name=trim(names);
              names='';
            }
            else{
              name=trim(names.substring(0,pos));
              names=trim(names.substring(pos));
            }
            pos=values.indexOf(' ');
            if (pos<0){
              val=trim(values);
              values='';
            }
            else{
              val=trim(values.substring(0,pos));
              values=trim(values.substring(pos));
            }
            if (val.charCodeAt(0)<46 || val.charCodeAt(0)>57 || val.charCodeAt(0)==47){
              val=val.substring(1)
            }
            val=Math.round(val);
            if (name.toLowerCase()!='sb' && name.toLowerCase()!='c'){
              window.opener.document.frmEntry['num' + name.toLowerCase() +  window.frmFSO.hidRow.value].value=val;
            }
          }
        }
      }
      break;
    case '170':
      pos=data.indexOf(':');
      data=new Number(trim(data.substring(pos+1)));
      window.opener.document.frmEntry['numvalue1' +  window.frmFSO.hidRow.value].value=data;
      break;
    case '160':
      pos=data.lastIndexOf('CUM<br>');
      if (pos>-1){
        var pcresult=new Array(5);
        data=data.substring(pos+11);
        for (var i=0;i<6;i++){
          pcresult[i]=new Number(data.substring(0,7));
          data=data.substring(24);
        }
        window.opener.document.frmEntry['nummicron_5_10' +  window.frmFSO.hidRow.value].value=pcresult[0];
        window.opener.document.frmEntry['nummicron_10_15' +  window.frmFSO.hidRow.value].value=pcresult[1];
        window.opener.document.frmEntry['nummicron_15_25' +  window.frmFSO.hidRow.value].value=pcresult[2];
        window.opener.document.frmEntry['nummicron_25_50' +  window.frmFSO.hidRow.value].value=pcresult[3];
        window.opener.document.frmEntry['nummicron_50_100' +  window.frmFSO.hidRow.value].value=pcresult[4];
        window.opener.document.frmEntry['nummicron_100' +  window.frmFSO.hidRow.value].value=pcresult[5];
      }
      break;
  }
}
//-->
</SCRIPT>

</HEAD>

<BODY>
<% 
dim fileLoop, fileCount, contents, objHTTP, fileUrl
Set objHTTP = CreateObject( "WinHttp.WinHttpRequest.5.1" )
fileCount = 0
Response.Write "<div style=""display: none;"">"
for fileLoop = 65 to 90
    fileUrl = filepath + filename + Chr(fileLoop) + "." + extension
    objHTTP.Open "GET", fileUrl, False
    objHTTP.Send
    contents = BinaryToString(objHTTP.ResponseBody)
    if Left(contents, 9) = "<!DOCTYPE" then
        'Response.Write "BAD"
    else
        Response.Write "<input type='text' name='filename" + Cstr(fileCount) + "' value='" + filename + Chr(fileLoop) + "." + extension + "'>"
        'Response.Write "<input type='text' name='contents" + CStr(fileCount) + "' value='" + contents + "'>"
        Response.Write "<textarea name='contents" + CStr(fileCount) + "'>" + contents + "</textarea>"
        fileCount = fileCount + 1
    end if
next
Response.Write "</div>"

Function BinaryToString(Binary)
  Const adTypeText = 2
  Const adTypeBinary = 1
  
  Dim BinaryStream
  Set BinaryStream = CreateObject("ADODB.Stream")
  
  BinaryStream.Type = adTypeBinary
  BinaryStream.Open
  BinaryStream.Write Binary
  
  BinaryStream.Position = 0
  BinaryStream.Type = adTypeText
  BinaryStream.CharSet = "us-ascii"
  
  BinaryToString = BinaryStream.ReadText
End Function

%>
<div align=center>
<form name=frmFSO>
  <input type=hidden name="hidSampleID" value="<%=sampleid%>">
  <input type=hidden name="hidTestID" value="<%=testid%>">
  <input type=hidden name="hidPath" value="<%=filepath%>">
  <input type=hidden name="hidFilename" value="<%=filename%>">
  <input type=hidden name="hidExtension" value="<%=extension%>">
  <input type=hidden name="hidRow" value="<%=row%>">
  <input type=hidden name="hidData" value="">
  <input type="hidden" name="hidFileCount" value="<%=fileCount%>">
</form>
<%if len(message)>0 then%>
  <h3><%=message%></h3>
  <input type=button name="cmdClose" value="Close" onclick='close_onclick()'>
<%else%>
<form name=frmFileData>
<table width="100%">
  <tr>
    <th>Sample#</th>
    <th>Files available</th>
  </tr>
  <tr>
    <td><%=sampleid%></td>
    <td><select name="lstFile" onchange='lstFile_onchange()'></select></td>
  </tr>
  <tr>
    <td colspan=2></td>
  </tr>
  <tr>
    <td>
      <input type=button name="cmdClose" value="Close" onclick='close_onclick()'>
    </td>
    <td>
      <input type=button name="cmdSelect" value="Select file" onclick='select_onclick()'>
    </td>
  </tr>
  <tr>
    <td colspan=2></td>
  </tr>
  <tr>
    <th colspan=2>File contents</th>
  </tr>
</table>
<pre id="preFile" class=leftalign>
</pre>
</form>
<%end if%>
</div>
</BODY>
</HTML>
<!--#include virtual="includes/footer.js"-->
<SCRIPT LANGUAGE=javascript>
<!--
    ListFilesInListbox(window.document.frmFileData.lstFile, window.frmFSO.hidFileCount.value);
    if (window.frmFileData.lstFile.length > 0) {
        window.frmFileData.lstFile.selectedIndex = 0;
        lstFile_onchange();
        window.frmFileData.lstFile.focus()
    }

    function ListFilesInListbox(objList, fileCount) {
        objList.length = 0; //clear the list
        for (var loop = 0; loop < fileCount; loop++) {
            strFile = window['filename'+loop].value;
            objList.options[loop] = new Option(strFile, strFile);
        }
    }
    //-->
</SCRIPT>
