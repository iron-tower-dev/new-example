<%
function GetLEDetails(strEQID, strComp, strLoc)
dim vntDetails(8,0)
'Index	Usage
' 0			EQID
' 1			Component
' 2			Location
' 3			LE Due Date
' 4			LE Status
' 5			Months since last changed
' 6			Date of last completed sample
' 7			ID of last completed sample
' 8			WM# used to derive LE status

dim dbLEConn, rsLETemp, strLESQL
dim dteChanged, strNext3, dteMostRecent, dteUseThis
dim intTolerance, intSinceChange, blnLEWritten, intSinceWritten
dim intMonthDayValue
dim vntNext3, blnSchedFind
	intMonthDayValue=28
	vntDetails(0,0)=strEQID
	vntDetails(1,0)=strComp
	vntDetails(2,0)=strLoc
	vntDetails(8,0)=""
	if trim(strEQID)<>"" and trim(strComp)<>"" and trim(strLoc)<>"" then
		Set dbLEConn = Server.CreateObject("ADODB.Connection")
		dbLEConn.open Application("dbSWMS_ConnectionString")
		vntDetails(3,0)=""
		vntDetails(4,0)=""
		strLESQL = "SELECT EQ_TAG_NUMBER,LUBE_COMPONENT_CODE,LUBE_LOCATION_CODE,SAMPLE_NEXT_3WO,LUBE_RT_LAST_COMP_DATE FROM LUBELAB.CPX_LUBE_SAMPLE_DATA_V "
		strLESQL=strLESQL & " WHERE EQ_TAG_NUMBER = '" & ucase(strEQID) & "' AND LUBE_COMPONENT_CODE='" & strComp & "' AND LUBE_LOCATION_CODE = '" & strLoc & "'"
		strLESQL=strLESQL & " ORDER BY EQ_TAG_NUMBER,LUBE_COMPONENT_CODE,LUBE_LOCATION_CODE"
		Set rsLETemp = dbLEConn.Execute(strLESQL)
		intMonths=""
		if not rsLETemp.eof then
			if isdate(rsLETemp(4)) then
				dteChanged=rsLETemp(4)
				vntDetails(5,0)=DateDiff("m",dteChanged,Now())
				if DatePart("d",dteChanged) <> DatePart("d",Now()) then
				  vntDetails(5,0) = vntDetails(5,0) + 1
				end if
			else
				dteChanged="01/01/1986"
			end if
			strNext3=rsLETemp(3)
		else
			dteChanged="01/01/1986"
			strNext3=""
		end if
		CloseDBObject(rsLETemp)
		CloseDBObject(dbLEConn)
		dbLEConn.open Application("dbLubeLab_ConnectionString")
		strLESQL = "SELECT lcde_t.completed_date, UsedLubeSamples.sampleDate FROM lcde_t LEFT OUTER JOIN UsedLubeSamples ON lcde_t.eq_tag_num = UsedLubeSamples.tagNumber "
		strLESQL = strLESQL & "AND lcde_t.sample_id = UsedLubeSamples.ID "
		strLESQL = strLESQL & "AND lcde_t.lube_component_code = UsedLubeSamples.component AND lcde_t.lube_location_code = UsedLubeSamples.location "
		strLESQL = strLESQL & "WHERE lcde_t.eq_tag_num='" & strEQID & "' AND lcde_t.lube_component_code ='" & strComp & "' AND lcde_t.lube_location_code ='" & strLoc & "'"
		strLESQL = strLESQL & " ORDER BY UsedLubeSamples.sampleDate DESC"
		Set rsLETemp = dbLEConn.Execute(strLESQL)
		blnLEWritten=false
		if not rsLETemp.eof then
		  dteMostRecent = rsLETemp(1)
		  blnLEWritten=true
		else
		  dteMostRecent=""
		end if
		CloseDBObject(rsLETemp)
        ' get the date and ID of the last completed (by the lab) sample for the point
		strLESQL = "SELECT TOP 1 ID, sampleDate "
		strLESQL = strLESQL & "FROM UsedLubeSamples "
		strLESQL = strLESQL & "WHERE (status IN (80, 120, 250)) "
		strLESQL = strLESQL & "AND tagNumber='" & strEQID & "' AND component ='" & strComp & "' AND location ='" & strLoc & "'"
		strLESQL = strLESQL & " ORDER BY sampleDate DESC"
		Set rsLETemp = dbLEConn.Execute(strLESQL)
		if not rsLETemp.eof then
			vntDetails(6,0)=rsLETemp(1)
			vntDetails(7,0)=rsLETemp(0)
		else
			vntDetails(6,0)=""
			vntDetails(7,0)=""
		end if
		CloseDBObject(rsLETemp)
		' now we need to get the tolerances etc...
		strLESQL="SELECT limits.ulim3 "
		strLESQL = strLESQL & "FROM limits INNER JOIN limits_xref ON limits.limits_xref_id = limits_xref.valueat "
		strLESQL = strLESQL & "WHERE (limits.testid = 550) AND limits_xref.tagNumber='" & strEQID & "' AND limits_xref.component='" & strComp & "' AND limits_xref.location='" & strLoc & "'"
		Set rsLETemp = dbLEConn.Execute(strLESQL)
		if not rsLETemp.eof then
			intTolerance = rsLETemp(0)
			CloseDBObject(rsLETemp)
			strLESQL="SELECT limits.llim3, limits.ulim3 "
			strLESQL = strLESQL & "FROM limits INNER JOIN limits_xref ON limits.limits_xref_id = limits_xref.valueat "
			strLESQL = strLESQL & "WHERE (limits.testid = 500) AND limits_xref.tagNumber='" & strEQID & "' AND limits_xref.component='" & strComp & "' AND limits_xref.location='" & strLoc & "'"
			Set rsLETemp = dbLEConn.Execute(strLESQL)
			if not rsLETemp.eof then
				intSinceChange = rsLETemp(0)
				intSinceWritten = rsLETemp(1)
				if trim(intSinceChange)<>"" and trim(intSinceWritten)<>"" then
					if blnLEWritten then
						if datediff("d",dteChanged,dteMostRecent)>0 then
							vntDetails(3,0)=formatdatetime(dateadd("d",((intSinceWritten+intTolerance)*intMonthDayValue),dteMostRecent),2)
							dteUseThis=dteMostRecent
						else
							vntDetails(3,0)=formatdatetime(dateadd("d",((intSinceChange+intTolerance)*intMonthDayValue),dteChanged),2)
							dteUseThis=dteChanged
						end if
					else
						vntDetails(3,0)=formatdatetime(dateadd("d",((intSinceChange+intTolerance)*intMonthDayValue),dteChanged),2)
						dteUseThis=dteChanged
					end if
					if trim(strNext3 & "")="" then
						if datediff("d",vntDetails(3,0),now)>0 then
							vntDetails(4,0)="<font color='#ff0033'>OVERDUE</font>"
						else
							vntDetails(4,0)="<font color='#ff6000'>TARGETED</font>"
						end if
					else
						vntDetails(4,0)="NA"
						if datediff("d",vntDetails(3,0),now)>0 then
							vntDetails(4,0)="<font color='#ff0033'>OVERDUE</font>"
						else
							vntNext3=split(strNext3,",")
							blnSchedFind=false
							for intLoop=1 to ubound(vntNext3) step 2
								if datediff("d",vntDetails(3,0),vntNext3(intLoop)) > (-2*intTolerance*intMonthDayValue) and datediff("d",vntDetails(3,0),vntNext3(intLoop))< 0 then
									' check for review status of complete...?
									strLESQL="select results_review_date from usedlubesamples where woNumber='" & vntNext3(intLoop-1) & "' and results_review_date IS NOT NULL"
									Set rsLETemp = dbLEConn.Execute(strLESQL)
									if rsLETemp.eof then
										vntDetails(3,0)=formatdatetime(vntNext3(intLoop),2)
										vntDetails(8,0)=trim(vntNext3(intLoop-1))
										vntDetails(4,0)="SCHEDULED"
										blnSchedFind=true
										exit for
									end if
									rsLETemp.close
								end if
							next
							if not blnSchedFind then
								vntDetails(4,0)="<font color='#009900'>OPEN</font>"
							end if
						end if	
					 end if
				else
					vntDetails(3,0)="-"
					vntDetails(4,0)="NA"
				end if		
			else
				vntDetails(3,0)="-"
				vntDetails(4,0)="NA"		
			end if
		else
			vntDetails(3,0)="-"
			vntDetails(4,0)="NA"		
		end if
		CloseDBObject(rsLETemp)
		CloseDBObject(dbLEConn)
	else
		vntDetails(3,0)="-"
		vntDetails(4,0)="NA"
	end if
	GetLEDetails=vntDetails
end function
%>