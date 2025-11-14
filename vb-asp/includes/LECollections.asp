
<%
'=============================================================================================================
' SQLQueryCollection ASP Class Module
' ---------------------
'               
' Created By  : Kwaku Afriyie-Fordjour
'               
' Last Update : October 23, 2009
'               
' IIS Version : 4.0 / 5.0 and above
'               
' Requires    : Microsoft Data Access Components (MDAC) 2.5 or better  (http://www.microsoft.com/data/download.htm)
'               * NOTE - This shows up in the VB references screen as "Microsoft ActiveX Data Objects 2.5 Library"
'               
' Description : This class module gives you professional quality database functionality with minimal coding.
'               It does all the work for you, all the error checking, and all the object validation.  Using
'               this class module will make your database code cleaner, more efficient, and less error-prone.
'               
' IMPORTANT   : Make sure that you use the connection object exposed by the "ConnectionObject" property if
'               you need a connection to a database.  The reason for this is you should only have one
'               connection open through the life of your program (to conserve resources).  Also, that
'               connection should stay open the entired time so you don't have to keep opening and closing
'               it (for performance reasons).
'               
' NOTE        : If you get a recordset back that contains NULL as the value returned from the database, the
'               only way that I know of to check that value without getting an "Invalid use of NULL" error is
'               by trimming the value and then tacking on a blank string and testing against a blank string:
'               
'               If Trim(rs("MyField") & "") = "" Then MsgBox "NULL or BLANK value returned"
'               
' NOTE        : If you are trying to retrieve a recordset with one variable, then return it by setting a return
'               varialbe equal to the first, you CAN NOT close the recordset by calling the "Close" method
'               of the ADODB.Recordset object.  If you are not going to return the recordset, then you should
'               close it.
'               
' See Also    : http://www.microsoft.com/data/
'               http://www.microsoft.com/data/ado/default.htm
'               
'_____________________________________________________________________________________________________________
'  Example Usage - Generate / Read Connection String Information:
'                                                                                                             
'
'<!--#include file = "LECollections.asp"-->
'< %
'  Dim queryColl As New SQLQueryCollection

'  Dim CompletedDateColl	'As Collection
'  Dim CompletedIDColl		'As Collection
'  Dim ToleranceColl		'As Collection
'  Dim LimitsColl			'As Collection
'  Dim ReviewDateColl		'As Collection

'  Set CompletedDateColl = queryColl.GetLESampleCompletedDate()
'  Set CompletedIDColl = queryColl.GetLECompletedID()
'  Set ToleranceColl = queryColl.GetLETolerance()
'  Set LimitsColl = queryColl.GetLELimits()
'  Set ReviewDateColl = queryColl.GetLELastReviewDate()
'
'  ...............>
'  Perform functionalities as required
'  ...............>
'
'  Set queryColl = Nothing
' 
'% >
'
'=============================================================================================================
' workorder and last component date from SWMS
Class LELastComponentDate
	'Option Explicit
    Private Sample_Next_3WO
    Private Lube_Rt_Last_Comp_Date
    
    Public Property Get GetSampleNext3WO()
		GetSampleNext3WO = Sample_Next_3WO
	End	Property
	
	Public Property Let SetSampleNext3WO(sampleNext) 
		Sample_Next_3WO = sampleNext
	End	Property 
	
	Public Property Get GetLubeLastComponentDate()
		GetLubeLastComponentDate = Lube_Rt_Last_Comp_Date
	End	Property
	
	Public Property Let SetLubeLastComponentDate(compdt) 
		Lube_Rt_Last_Comp_Date = compdt
	End	Property    
End class
'-------------------------- End LELastComponentDate ----------------------------------------------

' completed and sample dates
Class LESampleCompletedDate

    Private Completed_Date
    Private SampleDate
    
    Public Property Get GetSampleDateCompleted()
		GetSampleDateCompleted = Completed_Date		
	End	Property
	
	Public Property Let SetSampleDateCompleted(dt)
		Completed_Date = dt
	End	Property
	
	Public Property Let SetSampleDate(dt) 
		SampleDate = dt
	End	Property 
	
	Public Property Get GetSampleDate()
		GetSampleDate = Completed_Date
	End	Property   
End Class
'------------------------------- End LESampleCompletedDate -------------------------

' top 1 completed and last ID
Class LELastCompletedDateAndID
    Private ID
    Private SampleDate
    
    Public Property Get GetId()
		GetId = ID		
	End	Property
	
	Public Property Let SetId(i)
		ID = i
	End	Property
	
	Public Property Get GetSampleDate() 
		GetSampleDate = SampleDate
	End	Property 
	
	Public Property Let SetSampleDate(dt) 
		SampleDate = dt
	End	Property     
End Class
'------------------------------ End LELastCompletedDateAndID ---------------------------------

Class LETolerance
    Private ulim3
    
    Public Property Get GetTolerance() 
		GetTolerance = ulim3
	End	Property 
	
	Public Property Let SetTolerance(trance) 
		ulim3 = trance
	End	Property 
End Class
'-------------------------- End LETolerance -------------------------------------------------

Class LELimits
    Private llim3
    Private ulim3
    
    Public Property Get GetLowerLimit()
		GetLowerLimit = llim3		
	End	Property
	
	Public Property Let SetLowerLimit(i)
		llim3 = i
	End	Property
	
	Public Property Get GetUpperLimit()
		GetUpperLimit = ulim3		
	End	Property
	
	Public Property Let SetUpperLimit(i)
		ulim3 = i
	End	Property    
End Class
'-------------------------- End LELimits -------------------------------------------------


Class LEResultsReviewDate

    Private Results_Review_Date
    
    Public Property Get GetResultsReviewDate()
		GetResultsReviewDate = Results_Review_Date		
	End	Property
	
	Public Property Let SetResultsReviewDate(dt)
		Results_Review_Date = dt
	End	Property
End Class


' class for managing SQL collections used in the LStatus reporting
Class SQLQueryCollection

	dim LastComponentDateColl, SampleCompletedDateColl, LastCompletedDateAndIDColl
	dim ToleranceColl, LimitsColl, ResultsReviewDateColl
'--------------------------------------------------------------------------------------------
'	Get and store collection of work orders and last component date from SWMS
'   This will aid in the performance related issue being experienced while reporting LE Status
'	In the old scenario, the SWMS database is accessed for each equip, component and location
'	10/23/09 by KAF

	' class level variable to hold collection records for each database select instance
'	Purpose: Fires when reference to this class is created
	Private Sub Class_Initialize() 
	
		' create the relative set of collections
		set LastComponentDateColl = Server.CreateObject("Scripting.Dictionary")
		set SampleCompletedDateColl = Server.CreateObject("Scripting.Dictionary")
		set LastCompletedDateAndIDColl = Server.CreateObject("Scripting.Dictionary")
		set ToleranceColl = Server.CreateObject("Scripting.Dictionary")
		set LimitsColl = Server.CreateObject("Scripting.Dictionary")
		set ResultsReviewDateColl = Server.CreateObject("Scripting.Dictionary")
	End Sub 
	
	'Class_Terminate
	'Purpose: Fires when reference to this class is destroyed
	'   Closes Recordset and connections if they were left open after an error
	Private Sub Class_Terminate()
		LastComponentDateColl.RemoveAll
		SampleCompletedDateColl.RemoveAll
		LastCompletedDateAndIDColl.RemoveAll
		LimitsColl.RemoveAll
		ResultsReviewDateColl.RemoveAll
		ToleranceColl.RemoveAll
		
		LastComponentDateColl = Nothing
		SampleCompletedDateColl = Nothing
		LastCompletedDateAndIDColl = Nothing
		ToleranceColl = Nothing
		LimitsColl = Nothing
		ResultsReviewDateColl = Nothing
	End Sub	
	
	Private Function GetLastComponentDate()

		dim dbLEConn, rsLE, strLESQL, key, sampleNext
		
		on error resume next

		Set dbLEConn = Server.CreateObject("ADODB.Connection")	
		dbLEConn.open Application("dbSWMS_ConnectionString")
		
		' connect to SWMS, get the work order and last component date. Include the equip, component and location
		' as key items for the collection.	
		strLESQL = "SELECT EQ_TAG_NUMBER, LUBE_COMPONENT_CODE, LUBE_LOCATION_CODE, SAMPLE_NEXT_3WO, LUBE_RT_LAST_COMP_DATE FROM LUBELAB.CPX_LUBE_SAMPLE_DATA_V"
		strLESQL=strLESQL & " ORDER BY EQ_TAG_NUMBER, LUBE_COMPONENT_CODE, LUBE_LOCATION_CODE"
		Set rsLE = dbLEConn.Execute(strLESQL)
		
		if not rsLE.eof then
			do until rsLE.eof
				set sampleNext = New LELastComponentDate
				With sampleNext
					.SetSampleNext3WO = rsLE(3)				' next sample
					.SetLubeLastComponentDate = rsLE(4)		' last component date	
				End With			
				key = trim(rsLE(0)) & trim(rsLE(1)) & trim(rsLE(2))	' create composite key with equip, component, location
				LastComponentDateColl.Add key, sampleNext

				set sampleNext = Nothing
				rsLE.MoveNext
			loop
		end if
		CloseDBObject(rsLE)
		CloseDBObject(dbLEConn)

		GetLastComponentDate = LastComponentDateColl
	End Function

'--------------------------------------------------------------------------------------------
'	Get and store collection of completed and sample date from LUBLAB database
'   This will aid in the performance related issue being experienced while reporting LE Status
'	In the old scenario, the SWMS database is accessed for each equip, component and location
'	10/23/09 by KAF
Private function GetLESampleCompletedDate()

	dim dbLEConn, rsLE, strLESQL, key, compDate
	
	on error resume next

	Set dbLEConn = Server.CreateObject("ADODB.Connection")	
	dbLEConn.open Application("dbLubeLab_ConnectionString")

	' connect to LUBELAB, get the completed and sample date. Include the equip, component and location
	' as key items for the collection.	

	strLESQL = "SELECT lcde_t.eq_tag_num, lcde_t.lube_component_code, lcde_t.lube_location_code, lcde_t.completed_date, UsedLubeSamples.sampleDate FROM lcde_t LEFT OUTER JOIN UsedLubeSamples ON lcde_t.eq_tag_num = UsedLubeSamples.tagNumber "
	strLESQL = strLESQL & "AND lcde_t.sample_id = UsedLubeSamples.ID "
	strLESQL = strLESQL & "AND lcde_t.lube_component_code = UsedLubeSamples.component AND lcde_t.lube_location_code = UsedLubeSamples.location "
	strLESQL = strLESQL & " ORDER BY lcde_t.eq_tag_num, lcde_t.lube_component_code, lcde_t.lube_location_code, UsedLubeSamples.sampleDate DESC"
	
	Set rsLE = dbLEConn.Execute(strLESQL)
		
	if not rsLE.eof then
		do until rsLE.eof
			set compDate = New LESampleCompletedDate
			With compDate
				.SetSampleDateCompleted = rsLE(3)	' completed date
				.SetSampleDate = rsLE(4)			' sample date				 			
			End With
			set key = trim(rsLE(0)) & trim(rsLE(1)) & trim(rsLE(2))	' create composite key with equip, component, location
			SampleCompletedDateColl.Add key, compDate 
			rsLE.MoveNext
			set compDate = Nothing
		loop
	end if

	CloseDBObject(rsLE)
	CloseDBObject(dbLEConn)
	GetLESampleCompletedDate = SampleCompletedDateColl

end function

'--------------------------------------------------------------------------------------------
'	Get and store collection of completed and ID of last date from LUBLAB database
'   This will aid in the performance related issue being experienced while reporting LE Status
'	In the old scenario, the SWMS database is accessed for each equip, component and location
'	10/23/09 by KAF
Private function GetLECompletedID()

	dim dbLEConn, rsLE, strLESQL, key, compID
	
	on error resume next

	Set dbLEConn = Server.CreateObject("ADODB.Connection")	
	dbLEConn.open Application("dbLubeLab_ConnectionString")
	
	' get the date and ID of the last completed (by the lab) sample for the point
	strLESQL = "SELECT tagNumber, component, location, ID, sampleDate "
	strLESQL = strLESQL & "FROM UsedLubeSamples "
	strLESQL = strLESQL & "WHERE (status IN (80, 120, 250)) "
	strLESQL = strLESQL & " ORDER BY tagNumber, component, location, sampleDate DESC"	

	Set rsLE = dbLEConn.Execute(strLESQL)
		
	if not rsLE.eof then
		do until rsLE.eof
			set compID = New LELastCompletedDateAndID
			With compID
				.SetId = rsLE(3)								' ID
				.SetSampleDate = rsLE(4)						' sample date
			End With
			set key = trim(rsLE(0)) & trim(rsLE(1)) & trim(rsLE(2))	' create composite key with equip, component, location
			LastCompletedDateAndIDColl.Add key, compID
			rsLE.MoveNext
			set compID = Nothing
		loop			
	end if

	CloseDBObject(rsLE)
	CloseDBObject(dbLEConn)
	GetLECompletedID = LastCompletedDateAndIDColl
end function


'--------------------------------------------------------------------------------------------
'	Get and store collection of completed and ID of last date from LUBLAB database
'   This will aid in the performance related issue being experienced while reporting LE Status
'	In the old scenario, the SWMS database is accessed for each equip, component and location
'	10/23/09 by KAF
Private function GetLETolerance()

	dim dbLEConn, rsLE, strLESQL, key, compTolerance
	set ToleranceColl = Server.CreateObject("Scripting.Dictionary")
	
	on error resume next
	
	'Set dbLEConn = Server.CreateObject("ADODB.Connection")	
	dbLEConn.open Application("dbLubeLab_ConnectionString")
	
	' get tolerance
	strLESQL="SELECT limits_xref.tagNumber, limits_xref.component, limits_xref.location, limits.ulim3 "
	strLESQL = strLESQL & "FROM limits INNER JOIN limits_xref ON limits.limits_xref_id = limits_xref.valueat "
	strLESQL = strLESQL & "WHERE (limits.testid = 550) "
	strLESQL = strLESQL & "ORDER BY limits_xref.tagNumber, limits_xref.component, limits_xref.location"	
	
	Set rsLE = dbLEConn.Execute(strLESQL)

	if not rsLE.eof then
		do until rsLE.eof
			set compTolerance = New LETolerance
			With compTolerance
				.SetTolerance = rsLE(3)				' Tolerance
			End With
			set key = trim(rsLE(0)) & trim(rsLE(1)) & trim(rsLE(2))	' create composite key with equip, component, location
			ToleranceColl.Add key, compTolerance 
			rsLE.MoveNext
			set compTolerance = Nothing
		loop
	end if

	CloseDBObject(rsLE)
	CloseDBObject(dbLEConn)
	GetLETolerance = ToleranceColl
end function


'--------------------------------------------------------------------------------------------
'	Get and store collection of completed and ID of last date from LUBLAB database
'   This will aid in the performance related issue being experienced while reporting LE Status
'	In the old scenario, the SWMS database is accessed for each equip, component and location
'	10/23/09 by KAF
Private function GetLELimits()

	dim dbLEConn, rsLE, strLESQL, key, compLimits
	
	on error resume next

	Set dbLEConn = Server.CreateObject("ADODB.Connection")	
	dbLEConn.open Application("dbLubeLab_ConnectionString")
	
	' get limits
	strLESQL="SELECT limits_xref.tagNumber, limits_xref.component, limits_xref.location, limits.llim3, limits.ulim3 "
	strLESQL = strLESQL & "FROM limits INNER JOIN limits_xref ON limits.limits_xref_id = limits_xref.valueat "
	strLESQL = strLESQL & "WHERE (limits.testid = 500) "
	strLESQL = strLESQL & "ORDER BY limits_xref.tagNumber, limits_xref.component, limits_xref.location"	
	Set rsLE = dbLEConn.Execute(strLESQL)

	if not rsLE.eof then
		do until rsLE.eof
			set compLimits = New LELimits
			With compLimits
				.SetLowerLimit = rsLE(3)						' lower limit
				.SetUpperLimit = rsLE(4)						' upper limit
			End With
			set key = trim(rsLE(0)) & trim(rsLE(1)) & trim(rsLE(2))	' create composite key with equip, component, location
			LimitsColl.Add key, compLimits
			rsLE.MoveNext
			set compLimits = Nothing
		loop
	end if
	CloseDBObject(rsLE)
	CloseDBObject(dbLEConn)
	GetLELimits = LimitsColl
end function

'--------------------------------------------------------------------------------------------
'	Get and store collection of completed and ID of last date from LUBLAB database
'   This will aid in the performance related issue being experienced while reporting LE Status
'	In the old scenario, the SWMS database is accessed for each equip, component and location
'	10/23/09 by KAF
Private function GetLELastReviewDate()

	dim dbLEConn, rsLE, strLESQL, key, rvDate
	'set ResultsReviewDateColl = Server.CreateObject("Scripting.Dictionary")

	on error resume next

	Set dbLEConn = Server.CreateObject("ADODB.Connection")	
	dbLEConn.open Application("dbLubeLab_ConnectionString")
	
	' get limits
	strLESQL="select results_review_date, woNumber from usedlubesamples where results_review_date IS NOT NULL"
	Set rsLE = dbLEConn.Execute(strLESQL)

	if not rsLE.eof then
		do until rsLE.eof
			set rvDate = New LEResultsReviewDate
			rvDate.SetResultsReviewDate = rsLE(0)					' review date
			set key = trim(rsLE(1))									' WONumber as key
			ResultsReviewDateColl.Add key, rvDate
			rsLE.MoveNext
			set rvDate = Nothing
		loop
	end if
	CloseDBObject(rsLE)
	CloseDBObject(dbLEConn)
	GetLELastReviewDate = ResultsReviewDateColl
end function


'--------------------------------------------------------------------------------------------
'	Exposed function responsible for the creation of database collections for use in processing
'   analysis. This elimintaes the need to loop and call each database.
Public Sub CreateDatabaseCollections()

	GetLastComponentDate
	GetLESampleCompletedDate
	GetLECompletedID
	GetLETolerance
	GetLELimits
	GetLELastReviewDate	
end Sub

Public function GetLEDetails(strEQID, strComp, strLoc)
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
	dim key, oClass
	dim vntNext3, blnSchedFind
	
	intMonthDayValue=28
	vntDetails(0,0)=strEQID
	vntDetails(1,0)=strComp
	vntDetails(2,0)=strLoc
	vntDetails(8,0)=""
	
	' verify that composite key exists
	if trim(strEQID)<>"" and trim(strComp)<>"" and trim(strLoc)<>"" then
		key = trim(strEQID) & trim(strComp) & trim(strLoc)
		vntDetails(3,0)=""
		vntDetails(4,0)=""
		intMonths=""
		dteChanged="01/01/1986"
		strNext3=""

		if LastComponentDateColl.Exists(key) then
			set oClass = LastComponentDateColl.Item(key)
			if not (oClass is nothing) then
				if isdate(oClass.GetLubeLastComponentDate) then
					dteChanged = oClass.GetLubeLastComponentDate
					vntDetails(5,0) = DateDiff("m", dteChanged, Now())
					if DatePart("d",dteChanged) <> DatePart("d",Now()) then
						vntDetails(5,0) = vntDetails(5,0) + 1
					end if
				else
					dteChanged="01/01/1986"
				end if
				strNext3=oClass.GetSampleNext3WO
			else
				dteChanged="01/01/1986"
				strNext3=""
			end if
		end if
		
		blnLEWritten=false			
		oClass = SampleCompletedDateColl.Item(key)

		if not (oClass is nothing) then
		  dteMostRecent = oClass.GetSampleDate		  
		  blnLEWritten=true
		else
		  dteMostRecent=""
		end if

		oClass = LastCompletedDateAndIDColl.Item(key) 
		if not (oClass is nothing) then
			vntDetails(6,0) = oClass.GetSampleDate
			vntDetails(7,0) = oClass.GetId			
		else
			vntDetails(6,0)=""
			vntDetails(7,0)=""
		end if

		oClass = ToleranceColl.Item(key) 
		if not (oClass is nothing) then
			intTolerance = oClass.GetTolerance
			
			oClass = LimitsColl.Item(key) 
			if oClass <> "" then
				intSinceChange = oClass.GetLowerLimit
				intSinceWritten = oClass.GetUpperLimit
				
				if trim(intSinceChange)<> "" and trim(intSinceWritten) <> "" then
					if blnLEWritten then
						if datediff("d", dteChanged, dteMostRecent)>0 then
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
									oClass = ResultsReviewDateColl.Item(vntNext3(intLoop-1)) 
									if not (oClass is nothing) then
										vntDetails(3,0)=formatdatetime(vntNext3(intLoop),2)
										vntDetails(8,0)=trim(vntNext3(intLoop-1))
										vntDetails(4,0)="SCHEDULED"
										blnSchedFind=true
										exit for
									end if
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
	else
		vntDetails(3,0)="-"
		vntDetails(4,0)="NA"
	end if
	GetLEDetails=vntDetails
end function

End Class

%>
