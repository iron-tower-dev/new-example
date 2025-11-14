<%
Function UpdateSWMSMTE(strUsername, strWMNo, strEquipSerial, strUsageDate)
dim dbSWMSPROD, cmdCommand, strReturn
const adCmdUnknown = 0
'const adCmdText = 1
const adCmdTable = 2
const adCmdText = 1
const adParamInput = 1
const adParamOutput = 2
const adVarChar = 200
const adInteger = 3
const adNumeric = 131

	if trim(Application("dbSWMSPROD_ConnectionString"))="" then
		UpdateSWMSMTE="Error in MTE Usage function: No SWMS Database connection string (User unauthorized?)"
	else
		UpdateSWMSMTE=""
		set dbSWMSPROD=server.CreateObject("ADODB.Connection")
		set cmdCommand=server.CreateObject("ADODB.Command")
		dbSWMSPROD.Open Application("dbSWMSPROD_ConnectionString")
		set cmdCommand.ActiveConnection = dbSWMSPROD
		cmdCommand.CommandType = 4
		cmdCommand.CommandText = "LUBELAB.PKG_LUBELAB_WEB.PRC_INSERT_MTE_USAGE"
		cmdCommand.Parameters.Append cmdCommand.CreateParameter ("p_userid", adVarChar, adParamInput,200,strUsername)
		cmdCommand.Parameters.Append cmdCommand.CreateParameter("p_wmech_id" , adNumeric, adParamInput,,strWMNo)
		cmdCommand.Parameters.Append cmdCommand.CreateParameter("p_equip_serial" , adVarChar, adParamInput,200,strEquipSerial)
		cmdCommand.Parameters.Append cmdCommand.CreateParameter("p_usage_date" , adVarChar, adParamInput,200,strUsageDate)
		cmdCommand.Parameters.Append cmdCommand.CreateParameter("p_return_val" , adVarChar, adParamOutput,200)
		cmdCommand.Execute
		strReturn = cmdCommand("p_return_val")
		if cint(left(strReturn,1))<>0 then
			UpdateSWMSMTE = "Error in procedure: " & mid(strReturn,3)
		else
			UpdateSWMSMTE=mid(strReturn,3)
		end if 
		dbSWMSPROD.Close
		set cmdCommand=nothing
		set dbSWMSPROD=nothing
	end if
End Function

Function UpdateMTE(strSampleID, strTestID, strDate)
dim dbLUBEMTE,dbLUBELOG, rsLUBEMTE
dim strMTESQL
dim strResult, strLog
	if trim(strSampleID)<>"" then
		set dbLUBEMTE=server.CreateObject("ADODB.Connection")
		dbLUBEMTE.Open Application("dbLUBELAB_ConnectionString")
		strMTESQL="SELECT * FROM vwMTE_UsageForSample WHERE ID=" & strSampleID & " AND TESTID=" & strTestID
		set rsLUBEMTE=dbLUBEMTE.Execute(strMTESQL)
		do until rsLUBEMTE.eof
			strResult=""
			strLog=""
			if isnumeric(rsLUBEMTE("woNumber")) then
			    If Len(Trim(Replace(rsLUBEMTE("woNumber"),"0",""))) <> 0 Then
			    	strResult= UpdateSWMSMTE(session("USR"),rsLUBEMTE("woNumber"),rsLUBEMTE("SerialNo"),strDate)
				  	if instr(strResult,"Error") then
				  		strLog="Sample ID: " & strSampleID & "   MTE Serial No:" & rsLUBEMTE("SerialNo") & "   -   Error logging MTE into SWMS: " & strResult
				  	else
				  		strLog=""
				  	end if
				  end if
			end if
			if trim(strLog)<>"" then
				' update the system_log
				strMTESQL="INSERT INTO SYSTEM_LOG (EntryDate, EntryComment) values (GetDate(),'" & strLog & "')"
				dbLUBEMTE.Execute strMTESQL
			end if
			rsLUBEMTE.movenext
		loop
	end if
End Function

%>
