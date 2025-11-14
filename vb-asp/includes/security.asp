<!-- security.asp -->
<%
	dim strUserID
	dim objDSA
	dim strDSAExtra
	strDSAExtra = ""
	Set objDSA=Server.CreateObject("APSDSAX.DSACheck.1")
	if trim(Session("Username"))="" or trim(Session("USR"))="" then
		Session("GenericID")=false
		objDSA.DSA_tran_name = Application("TRAN_NAME_LUBELAB")
		if objDSA.GetConnectInfo<>0 then
			' they are not authorised...
			Response.Redirect Application("URL") & "includes/unauthorized.asp" & strDSAExtra
		else
			strUserID=ucase(objDSA.DSA_userid)
			'T-Dev, R-Test
			if left(Application("TRAN_NAME_LUBELAB"),1) = "T" then
			  Application("dbLUBELAB_ConnectionString") = "Provider=sqloledb;Data Source=" & objDSA.DSA_server_name & ";Initial Catalog=lubelab_dev;User Id=" & objDSA.DSA_tran_id & ";Password=" & objDSA.DSA_tran_ticket & ";"
			else
			  Application("dbLUBELAB_ConnectionString") = "Provider=sqloledb;Data Source=" & objDSA.DSA_server_name & ";Initial Catalog=lubelab;User Id=" & objDSA.DSA_tran_id & ";Password=" & objDSA.DSA_tran_ticket & ";"
			end if
			objDSA.DSA_tran_name=Application("TRAN_NAME_LUBELAB_LAB")
			Session("AccessLevel")="READ"
			session("Administrator")="N"
			if objDSA.GetConnectInfo = 0 then
				' see what access level they have!
				if objDSA.DSA_create_flag="Y" and objDSA.DSA_read_flag="Y" and objDSA.DSA_update_flag="Y" then
					if objDSA.DSA_delete_flag="Y" then
						session("Administrator")="Y"
						Session("AccessLevel")="ADMIN"
					else
						Session("AccessLevel")="REVIEWER"
					end if
				elseif objDSA.DSA_read_flag="Y" and objDSA.DSA_update_flag="Y" then
					Session("AccessLevel")="USER"
				end if
			end if


' ENABLE FOR TEMPORARY TESTING CODE TO FORCE ADMIN RIGHTS
'						session("Administrator")="Y"
'						Session("AccessLevel")="ADMIN"
''''''''''''''''''''''''''''''''''''''''''''''

			'DSA details for SWMS
			objDSA.DSA_tran_name=Application("TRAN_NAME_SWMS")
			
			if objDSA.GetConnectInfo <> 0 then
				Response.Redirect Application("URL") & "includes/unauthorized.asp" & strDSAExtra
			else
			  Application("dbSWMS_ConnectionString") = "Provider=ORAOLEDB.ORACLE;Data Source=" & objDSA.DSA_server_name & ";User ID=" & objDSA.DSA_tran_id & ";Password=" & objDSA.DSA_tran_ticket
			  Application("SWMSServer") = objDSA.DSA_server_name
			end if
			objDSA.DSA_tran_name=Application("TRAN_NAME_SWMSPROD")
			if objDSA.GetConnectInfo = 0 then
			  Application("dbSWMSPROD_ConnectionString") = "Provider=ORAOLEDB.ORACLE;Data Source=" & objDSA.DSA_server_name & ";User ID=" & objDSA.DSA_tran_id & ";Password=" & objDSA.DSA_tran_ticket
			end if
		end if
		set objDSA=nothing
		Session("Username")=strUserID
	else
		' we have already been through the application so release it!
		strUserID=Session("Username")
	end if
		if left(ucase(strUserID),1)<>"Z" and left(ucase(strUserID),1)<>"C" and left(ucase(strUserID),1)<>"M" and not (left(ucase(strUserID),1)="P" and isnumeric(mid(ucase(strUserID),2,1))) then
		Session("GenericID")=true
		Response.redirect Application("URL") & "includes/logon.asp?Logon=Y&GOTO=" & mid(Request.ServerVariables("SCRIPT_NAME"),2) & "|" & Request.ServerVariables("QUERY_STRING")
	else
		Session("USR")=mid(strUserID,2)
	end if
%>
