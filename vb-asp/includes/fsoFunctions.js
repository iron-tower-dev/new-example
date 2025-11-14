<SCRIPT ID=fsoFunctionsJS LANGUAGE=javascript>
<!--
// FSO Constants
var fsoWindowsFolder=0
var fsoSystemFolder=1
var fsoTemporaryFolder=2
var fsoForReading=1
var fsoForWriting=2
var fsoForAppending=8
var fsoTristateUseDefault=-2
var fsoTristateTrue=-1
var fsoTristateFalse=0

function FilesInFolder(strFolder){
var fso=new ActiveXObject("Scripting.FileSystemObject");
var folder=fso.GetFolder(strFolder);
var colFiles=folder.Files;
var result=new Array(colFiles.Count),count=0;

  for(var objEnum = new Enumerator(colFiles); !objEnum.atEnd(); objEnum.moveNext()) {
	result[count++]=objEnum.item()
  }
  delete objEnum;
  objEnum=null;
  colFiles=null;
  folder=null;
  delete fso;
  fso=null;
  return result;
}

function FileContents(strFilename){
var fso=new ActiveXObject("Scripting.FileSystemObject");
var f=fso.OpenTextFile(strFilename, fsoForReading);
var strText='';

  if (!f.AtEndOfStream){
  strText=f.ReadAll()
  }
  delete fso;
  fso=null;
  return(strText);
}

function FileExists(strFilename){
var fso=new ActiveXObject("Scripting.FileSystemObject");
var blnResult=false;
  blnResult=(fso.FileExists(strFilename));
  delete fso;
  fso=null;
  return blnResult;
}

function ListFilesInListbox(strFolder,objList,strFilename,strExtension){
var fso=new ActiveXObject("Scripting.FileSystemObject");
var strFile='',count=0;

  if (strFolder.charAt(strFolder.length-1)!='\\'){
	strFolder+='\\'
  }

  objList.length=0; //clear the list
  for (var loop=65; loop<91; loop++) {
	strFile=strFilename + String.fromCharCode(loop) + "." + strExtension;
	if (FileExists(strFolder+strFile)){
	  objList.options[count++]=new Option(strFile,strFile);
	}
  }

  delete fso;
  fso=null;
}
//-->
</SCRIPT>