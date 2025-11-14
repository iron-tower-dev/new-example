<%
Function ExtractRubbish(strText)
On Error Resume Next
	' a single quote
	Do Until Instr(strText,Chr(39))=0
		strText=Left(strText,Instr(strText,Chr(39))-1) & " " & Mid(strText,Instr(strText,Chr(39))+1)
	Loop
	ExtractRubbish=strText
End Function

Function ExtractSpace(strText)
' used in the create filename (PDF) routines
On Error Resume Next
dim lngLoop
	lngLoop=0
	' a space
	Do Until Instr(strText,Chr(32))=0
		strText=Left(strText,Instr(strText,Chr(32))-1) & Mid(strText,Instr(strText,Chr(32))+1)
		if lngLoop>4000 then
			exit do
		end if
	Loop
	' a /
	lngLoop=0
	Do Until Instr(strText,"/")=0
		strText=Left(strText,Instr(strText,"/")-1) & Mid(strText,Instr(strText,"/")+1)
		if lngLoop>4000 then
			exit do
		end if
	Loop	
	lngLoop=0
	Do Until Instr(strText,"\")=0
		strText=Left(strText,Instr(strText,"\")-1) & Mid(strText,Instr(strText,"\")+1)
		if lngLoop>4000 then
			exit do
		end if
	Loop	
	ExtractSpace=strText
End Function

Function ExtractCRLF(strText)
On Error Resume Next
dim lngLoop
	lngLoop=0
	' a space
	Do Until Instr(strText,vbcrlf)=0
		strText=Left(strText,Instr(strText,vbcrlf)-1) & "<br />" & Mid(strText,Instr(strText,vbcrlf)+1)
		lngLoop=lngLoop+1
		if lngLoop>4000 then
			exit do
		end if
	Loop
	ExtractCRLF=strText
End Function
%>
