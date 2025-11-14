<%
Function GoodnessCalc(strSampleID)
dim strGoodnessSQL, dbGOODNESS, rsGoodness
dim lngGoodness, blnDone, blnGoodness
	GoodnessCalc=""
	set dbGOODNESS=server.CreateObject("ADODB.Connection")
	dbGOODNESS.Open Application("dbLUBELAB_ConnectionString")
	if trim(strSampleID)<>"" then
		' check to see if there are any goodness & values
		strGoodnessSQL="SELECT * FROM vwGoodnessLimitsAndResultsForSample WHERE SAMPLEID='" & strSampleID & "'"
		set rsGoodness=dbGOODNESS.Execute(strGoodnessSQL)
		blnGoodness=false
		if not rsGoodness.eof then
			lngGoodness=100
			blnGoodness=true
			do until rsGoodness.eof
				blnDone=false
				if not isnull(rsGoodness("llim3")) then
					if rsGoodness("Result")<rsGoodness("llim3") then
						if not isnull(rsGoodness("gl3")) then
							lngGoodness=lngGoodness-rsGoodness("gl3")
						end if
						blnDone=true
					end if
				end if
				if not isnull(rsGoodness("llim2")) and not blnDone then
					if rsGoodness("Result")<rsGoodness("llim2") then
						if not isnull(rsGoodness("gl2")) then
							lngGoodness=lngGoodness-rsGoodness("gl2")
						end if
						blnDone=true
					end if
				end if
				if not isnull(rsGoodness("llim1")) and not blnDone then
					if rsGoodness("Result")<rsGoodness("llim1") then
						if not isnull(rsGoodness("gl1")) then
							lngGoodness=lngGoodness-rsGoodness("gl1")
						end if
						blnDone=true
					end if
				end if		
				if not isnull(rsGoodness("ulim3")) and not blnDone then
					if rsGoodness("Result")>rsGoodness("ulim3") then
						if not isnull(rsGoodness("gu3")) then
							lngGoodness=lngGoodness-rsGoodness("gu3")
						end if
						blnDone=true
					end if
				end if				
				if not isnull(rsGoodness("ulim2")) and not blnDone then
					if rsGoodness("Result")>rsGoodness("ulim2") then
						if not isnull(rsGoodness("gu2")) then
							lngGoodness=lngGoodness-rsGoodness("gu2")
						end if
						blnDone=true
					end if
				end if		
				if not isnull(rsGoodness("ulim1")) and not blnDone then
					if rsGoodness("Result")>rsGoodness("ulim1") then
						if not isnull(rsGoodness("gu1")) then
							lngGoodness=lngGoodness-rsGoodness("gu1")
						end if
						blnDone=true
					end if
				end if								
				rsGoodness.movenext
			loop
			if lngGoodness<0 then
				lngGoodness=0
			end if
			GoodnessCalc=lngGoodness
		end if
		if blnGoodness then
			' see if there is already a value in the database
			strGoodnessSQL="SELECT SAMPLEID FROM TESTREADINGS WHERE SAMPLEID='" & strSampleID & "' AND TESTID=300"
			set rsGoodness=dbGOODNESS.Execute(strGoodnessSQL)
			if rsGoodness.eof then
				' insert a record
				strGoodnessSQL = "INSERT INTO TESTREADINGS (SAMPLEID, TESTID, TRIALNUMBER, VALUE1, TRIALCOMPLETE, STATUS) VALUES "
				strGoodnessSQL = strGoodnessSQL & "('" & strSampleID & "',300,1," & lngGoodness & ",0,'S')"
			else
				' update the record
				strGoodnessSQL = "UPDATE TESTREADINGS SET VALUE1=" & lngGoodness & " WHERE SAMPLEID='" & strSampleID & "' AND TESTID=300"
			end if
			dbGOODNESS.Execute strGoodnessSQL
		end if
		rsGoodness.close
		dbGOODNESS.Close
		set rsGoodness=nothing
		set dbGOODNESS=nothing
	end if
End Function
%>

