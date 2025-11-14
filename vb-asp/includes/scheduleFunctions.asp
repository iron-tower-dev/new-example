<%
function AddTestToSchedule(sampleid,testid,schedtype)
dim rs,conn,sql,sched

'Response.Write "<br>___Entering function AddTestToSchedule " & sampleid & "," & testid & "," & schedtype & "<br>"

  if len(trim(schedtype))=0 or schedtype="NULL" then
    sched="NULL"
  else
    sched="'" & schedtype & "'"
  end if
  on error resume next
  sql="SELECT * FROM TestReadings WHERE sampleid=" & sampleid & " AND testid=" & testid
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))

'Response.Write "____Executing SQL: " & sql & "<br>"

  set rs=ForwardOnlyRS(sql,conn)
  if IsOpen(rs) and rs.EOF then  
    sql="INSERT INTO TestReadings (sampleid,testid,trialnumber,trialcomplete,status,schedType) VALUES ("
    sql=sql & sampleid & "," & testid & ",1,0,'A'," & sched & ")"
  else
    sql="UPDATE TestReadings SET status='A' WHERE sampleid=" & sampleid & " AND testid=" & testid
  end if

'Response.Write "____Executing SQL: " & sql & "<br>"

  set rs=conn.Execute(sql)
  CloseDbObject(rs)
  set rs=nothing
  AddTestToSchedule=(DBErrorCount(conn)=0)

'Response.Write "____AddTestToSchedule return value: " & (DBErrorCount(conn)=0) & "<br>"

  if DBErrorCount(conn)=0 then
    sql="UPDATE usedlubesamples SET status = 90, returnedDate = GetDate() WHERE status IN (80,120) AND ID=" & sampleid

'Response.Write "____Executing SQL: " & sql & "<br>"

    set rs=conn.Execute(sql)
  end if
  CloseDBObject(rs)
  set rs=nothing
  CloseDBObject(conn)
  set conn=nothing

'Response.Write "___Exiting AddTestToSchedule<br><br>"

end function

function RemoveTestFromSchedule(sampleid,testid,reason)
dim rs,conn,sql,success
  sql="INSERT INTO ScheduleDeletions (sampleid,testid,deletiondate,reason) VALUES ("
  sql=sql & sampleid & "," & testid & ",GetDate(),'" & reason & "')"
  on error resume next
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=conn.Execute(sql)
  Err.Clear
  sql="DELETE FROM TestReadings WHERE sampleid=" & sampleid & " AND testid=" & testid
  set rs=conn.Execute(sql)
  success=(DBErrorCount(conn)=0)
  RemoveTestFromSchedule=success
  CloseDBObject(rs)
  set rs=nothing
  CloseDBObject(conn)
  set conn=nothing
end function

sub SetSampleScheduleType(sampleid,scheduletype,blnoverwrite)
dim rs,conn,sql
  sql="SELECT schedule FROM UsedLubeSamples WHERE id=" & sampleid
  on error resume next
  Err.Clear
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=conn.Execute(sql)
  if not rs.EOF and (blnoverwrite or len(getField(rs,"schedule"))=0) then
    sql="UPDATE UsedLubeSamples SET schedule='" & scheduletype & "' WHERE id=" & sampleid
    set rs=conn.Execute(sql)
  end if
  CloseDBObject(rs)
  set rs=nothing
  CloseDBObject(conn)
  set conn=nothing
end sub

function CreateSampleSchedule(sampleid,tag,comp,loc,wm)
dim rs,conn,sql,created
dim interval,testid,month
dim vntTemp,leDue

'Response.Write "_Entering CreateSampleSchedule " & sampleid & "," & tag & "," & comp & "," & loc & "<br>"

  created=false

	'Check if we need to schedule tests on the basis of the LE status
	leDue = false
	vntTemp = GetLEDetails(tag, comp, loc)
'Response.Write "__LE Status: " & vntTemp(4,0) & "<br>"
'Response.Write "__LE Due Date: " & vntTemp(3,0) & "<br>"
'Response.Write "__LE WM#: " & vntTemp(8,0) & "<br>"
	if InStr(vntTemp(4,0),"OVERDUE") > 0 then
		leDue = true
	else
		if InStr(vntTemp(4,0),"SCHEDULED") > 0 and vntTemp(8,0) = wm then
			leDue = true
		end if
	end if

  Err.Clear
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  conn.CommandTimeout=600
  conn.Errors.Clear

	if leDue = true then
		sql="SELECT testid FROM vwLEScheduleByEQID "
		sql=sql & "WHERE tagNumber='" & tag & "' "
		sql=sql & "AND component='" & comp & "' "
		sql=sql & "AND location='" & loc & "' ORDER BY tagNumber, component, location, testid"
		'on error resume next
		set rs=DisconnectedRS(sql,conn)
		do until rs.EOF
 'response.write getField(rs,"TestID")
        if AddTestToSchedule(sampleid,getField(rs,"TestID"),"") then
'Response.Write "__Test added: " & getField(rs,"TestID") & "<br>"
          created=true
        end if
        rs.MoveNext
		loop
		set rs=nothing
	end if
'response.write created
'response.end

  sql="SELECT * FROM vwTestScheduleDefinitionByEQID WHERE Tag='" & tag & "' AND ComponentCode='" & comp & "' AND LocationCode='" & loc & "' ORDER BY TestID"
  on error resume next

'Response.Write "__Executing SQL: " & sql & "<br>"

  set rs=DisconnectedRS(sql,conn)
  if not rs.EOF then

'Response.Write "__Test list found<br>"

    rs.MoveFirst

    if len(getField(rs,"TestScheduleID"))=0 then
      'no Test Schedule defined for this sample point

'Response.Write "__TestScheduleID is zero-length<br>" 

    if Err.number <>0 then
      Response.Write "Error: " & Err.number & "<BR>"
      Response.Write ".." & Err.description & "<BR>"
      Response.Write "..." & Err.source & "<BR>"
      Err.Clear
    end if

    else
      'a Test Schedule exists - apply it

'Response.Write "__Test schedule exists<br>"

      do until rs.EOF
        interval=getField(rs,"TestInterval")
        month=getField(rs,"DuringMonth")

'Response.Write "__Interval: " & interval & "<br>"
'Response.Write "__Month: " & month & "<br>"

        if len(interval)>0 then
          'apply schedule for current test ID
          testid=getField(rs,"TestID")

'Response.Write "__Test ID: " & testid & "<br>"

          if cint(interval)=1 then
            'Test is required for each sample

'Response.Write "__Calling AddTestToSchedule because interval is 1<br>"

            if AddTestToSchedule(sampleid,testid,"") then
              created=true
            end if
          else

'Response.Write "__Calling IsTestRequired<br>"

            if IsTestRequired(sampleid,tag,comp,loc,testid,interval,month,"")=true then
              'Test is required this time

'Response.Write "__Calling AddTestToSchedule because test is required this time<br>"

              if AddTestToSchedule(sampleid,testid,"") then
                created=true
              end if
            end if
          end if
        end if
        rs.movenext
      loop
    end if
  end if

  CloseDBObject(rs)
  set rs=nothing
  CloseDBObject(conn)
  set conn=nothing

  if created=true then
    SetSampleScheduleType sampleid,"A",true
  end if
  CreateSampleSchedule=created

'Response.Write "_Exiting CreateSampleSchedule <br>"

end function

function CreateMaterialSchedule(sampleid,material)
dim rs,conn,sql,created
dim interval,testid,month

'Response.Write "<br>_Entering CreateMaterialSchedule " & sampleid & "," & material & "<br>"

  created=false
  sql="SELECT * FROM vwTestScheduleDefinitionByMaterial WHERE material='" & material & "' ORDER BY TestID"
  on error resume next
  Err.Clear
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  conn.CommandTimeout=600

'Response.Write "__Executing SQL: " & sql & "<br>"

  set rs=DisconnectedRS(sql,conn)
  if not rs.EOF then

'Response.Write "__Test list found<br>"

    rs.MoveFirst

    if len(getField(rs,"TestScheduleID"))=0 then
      'no Test Schedule defined for this sample point

'Response.Write "__TestScheduleID is zero-length<br>"
'dim f
'for each f in rs.Fields
'  Response.Write space(10) & f.Name & " = " & f.Value & "<BR>"
'next

    if Err.number <>0 then
      Response.Write "Error: " & Err.number & "<BR>"
      Response.Write ".." & Err.description & "<BR>"
      Response.Write "..." & Err.source & "<BR>"
      Err.Clear
    end if

    else
      'a Test Schedule exists - apply it

'Response.Write "__Test schedule exists<br>"

      do until rs.EOF
        interval=getField(rs,"TestInterval")
        month=getField(rs,"DuringMonth")

'Response.Write "__Interval: " & interval & "<br>"
'Response.Write "__Month: " & month & "<br>"

        if len(interval)>0 then
          'apply schedule for current test ID
          testid=getField(rs,"TestID")

'Response.Write "__Test ID: " & testid & "<br>"

          if cint(interval)=1 then
            'Test is required for each sample

'Response.Write "__Calling AddTestToSchedule because interval is 1<br>"

            if AddTestToSchedule(sampleid,testid,"") then
              created=true
            end if
          else

'Response.Write "__Calling IsTestRequired<br>"

            if IsTestRequired(sampleid,tag,comp,loc,testid,interval,month,material)=true then
              'Test is required this time

'Response.Write "__Calling AddTestToSchedule because test is required this time<br>"

              if AddTestToSchedule(sampleid,testid,"") then
                created=true
              end if
            end if
          end if
        end if
        rs.movenext
      loop
    end if
  end if

  CloseDBObject(rs)
  set rs=nothing
  CloseDBObject(conn)
  set conn=nothing

  if created=true then
    SetSampleScheduleType sampleid,"A",true
  end if
  CreateMaterialSchedule=created

'Response.Write "<br>_Exiting CreateMaterialSchedule <br>"

end function

function IsTestRequired(sampleid,tag,comp,loc,testid,interval,month,material)
dim rs,conn,sql,iMonth
dim day,year,reqdate,bkmark,position,counter

'Response.Write "___Entering IsTestRequired<br>"

  if cint(interval)=1 then
    IsTestRequired=true
  else
    sql="select * from vwTestsBySampleAndEQID where "
    if len(material)>0 then
      sql=sql&"lubetype='" & material & "' and "
    else
      sql=sql&"Tag='" & tag & "' and "
      sql=sql&"componentcode='" & comp & "' and "
      sql=sql&"locationcode='" & loc & "' and "
    end if
    sql=sql&"sampleid<" & sampleid & " and "
    sql=sql&"testid=" & testid & " and "
    sql=sql&"(SampleSchedule<>'S' or SampleSchedule is null) "
    sql=sql&"order by sampleid desc,testid asc"
    on error resume next
    set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
    set rs=DisconnectedRS(sql,conn)
    CloseDBObject(conn)
    set conn=nothing
    if len(month)>0 then
      if cint(month)>0 then
        'base requirement on month
        reqdate="01/" & month & "/" & DatePart("yyyy",Date())
        if DateDiff("d",reqdate,Date())<0 then
          'haven't yet reached required date in the current year
        else
          ApplyFilter rs,"ApplicableDate>='" & reqdate & "'"
          if rs.EOF then
            'test is required now
            IsTestRequired=true
          end if
        end if
      end if
    else
      'base requirement on interval
      position=NumberOfRecords(rs)+1
'      Response.Write "____test:" & testid & "<br>"
'      Response.Write "____records:" & NumberOfRecords(rs) & "<br>"
'      Response.Write "____interval:" & interval & "<br>"
      counter=0
      do until rs.EOF
        counter=counter + 1
        if not(isnull(rs.Fields("TestStatus"))) then
          position=counter
          exit do
        end if
        rs.MoveNext
      loop
'      Response.Write "____position:" & position & "<br>"
      if interval<=position then
        'test is required now
        IsTestRequired=true
      end if
'      Response.Write "____required:" & (interval<=position) & "<br>"
'      Response.Write "<br>"
    end if
  end if
  set rs=nothing

'Response.Write "___Exiting IsTestRequired<br>"

end function

sub AutoAddRemoveAllTests(sampleid)
dim sql,conn,rs
  sql="SELECT * FROM vwLabOverall WHERE TestDisplayStatus='C' AND SampleID=" & sampleid
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=DisconnectedRS(sql,conn)
  CloseDBObject(conn)
  set conn=nothing
  do until rs.EOF
'    Response.Write "Add-remove for " & getField(rs,"sampleid") & ":" & getField(rs,"tagnumber") & ":" & getField(rs,"component") & ":" & getField(rs,"location") & ":" & getField(rs,"testid") & "<br>"
    AutoAddRemoveTests getField(rs,"sampleid"),getField(rs,"tagnumber"),getField(rs,"component"),getField(rs,"location"),getField(rs,"testid")
    rs.movenext
  loop

  CloseDBObject(rs)
  set rs=nothing
  CloseDBObject(conn)
  set conn=nothing
end sub

sub AutoAddRemoveTests(sampleid,tag,comp,loc,testid)
dim rsRules,conn,sql,rs,ruletestid,continue, interval
on error resume next
  sql="select * from vwtestrulesbyeqid where "
  sql=sql&"Tag='" & tag & "' and "
  sql=sql&"componentcode='" & comp & "' and "
  sql=sql&"locationcode='" & loc & "' and "
  sql=sql&"testid=" & testid
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rsRules=DisconnectedRS(sql,conn)
'  Response.Write "sampleid: " & sampleid & "<br>"
'  Response.Write "eqid: " & tag & "<br>"
'  Response.Write "comp: " & comp & "<br>"
'  Response.Write "loc: " & loc & "<br>"
'  Response.Write "testid: " & testid & "<br>"
'  Response.Write "Rules SQL: "& sql & "<br>"

  do while not rsRules.EOF
    continue=false
    ruletestid=getField(rsRules,"RuleTestID")
    'check result for this test against applicable limits (if any)
    'if limits exist and the result exceeds the appropriate one then
    sql="select result from vwtestresultbysampleandtest where sampleid=" & sampleid & " and testid=" & testid
    set rs=DisconnectedRS(sql,conn)
'    Response.Write "Results SQL: "& sql & "<br>"
    if not rs.EOF then
'      Response.Write " Found result value" & getField(rs,"Result") & "<br>"
'      Response.Write "Rule is " & getField(rsRules,"UpperRule") & "<br>"
      if getField(rsRules,"UpperRule")=true then
'        Response.Write " Upper<br>"
'        Response.Write "------Upper test " & getField(rs,"Result") &">" & getField(rsRules,"UpperLimit") &"*<br>"
        if len(getField(rs,"Result"))>0 and len(getField(rsRules,"UpperLimit"))>0 then
			if (cdbl(getField(rs,"Result")) > cdbl(getField(rsRules,"UpperLimit"))) then
	'			Response.Write "  Upper rule EXCEEDED<br>"
				continue=true
			end if
        else
'          Response.Write "  Upper rule OK<br>"
        end if
      else
'        Response.Write " Lower<br>"
'        Response.Write "------Lower test " & getField(rs,"Result") &"<" & getField(rsRules,"LowerLimit") &"*<br>"
        if len(getField(rs,"Result"))>0 and len(getField(rsRules,"LowerLimit"))>0 then
			if (cdbl(getField(rs,"Result")) < cdbl(getField(rsRules,"LowerLimit"))) then
	'          Response.Write "  Lower rule EXCEEDED<br>"
	          continue=true
	        end if
        else 
'          Response.Write "  Lower rule OK<br>"
        end if
      end if
    end if

    if continue then
      if getField(rsRules,"RuleAction") ="R" then
'        Response.Write "   Checking if we can remove<br>"
        'check if test can be removed
        sql="SELECT * FROM vwTestDeleteCriteria where sampleid=" & sampleid & " and testid=" & ruletestid
'        Response.Write "remove check sql " & sql & "<br>"
        set rs=DisconnectedRS(sql,conn)
        if not rs.EOF then
          'check if deleting the test will violate the minimum interval requirements
          sql="SELECT * FROM vwTestScheduleDefinitionByEQID WHERE Tag='" & tag & "' AND ComponentCode='" & comp & "' AND LocationCode='" & loc & "'AND testid=" & ruletestid & " ORDER BY TestID"
'          Response.Write "check min sql "& sql & "<br>"
          set rs=DisconnectedRS(sql,conn)
          if not rs.EOF and len(getField(rs,"TestScheduleID"))>0 then
            'a Test Schedule exists - apply it
            do until rs.EOF
              interval=getField(rs,"MinimumInterval")
              if len(interval)=0 then 
                interval=0
              end if
              if cint(interval)>1 then
                'apply schedule for current test ID
                if IsTestRequired(sampleid,tag,comp,loc,ruletestid,interval,"","")=false then
                  'Test is not required this time - it can be deleted
'                  Response.Write "    Calling function to remove " & ruletestid & "<br>"
                  if RemoveTestFromSchedule(sampleid,ruletestid,"Test removed due to defined rules") then
                    SetSampleScheduleType sampleid,"A",false
                  end if
                end if
              else
                if cint(interval)=0 then
                  'Test is not required this time - it can be deleted
'                  Response.Write "    Calling function to remove " & ruletestid & "<br>"
                  if RemoveTestFromSchedule(sampleid,ruletestid,"Test removed due to defined rules") then
                    SetSampleScheduleType sampleid,"A",false
                  end if
                end if
              end if
              rs.movenext
            loop
          end if
        end if
      else
'        Response.Write "   Checking if we can add<br>"
        'check if test can be added
        sql="SELECT * FROM vwTestAddCriteria where sampleid=" & sampleid & " and testid=" & ruletestid
        set rs=DisconnectedRS(sql,conn)
        if cstr(getField(rs,"status")) <> "250" and len(getField(rs,"testid"))=0 and len(getField(rs,"reason"))=0 then
          'can add the test to the sample's schedule
'          Response.Write "    Calling function to add " & ruletestid & "<br>"
          if AddTestToSchedule(sampleid,ruletestid,"A") then
            SetSampleScheduleType sampleid,"A",false
          end if
        end if
      end if
    end if
    rsRules.MoveNext
  loop

  CloseDBObject(rs)
  set rs=nothing
  CloseDBObject(rsRules)
  set rsRules=nothing
  CloseDBObject(conn)
  set conn=nothing
end sub
%>