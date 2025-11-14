<!-- saveResultsFunctions.asp -->
<%
function deleteRecords()
'delete any records apart from trial 1
dim conn,rs,sql,strFields,strValues
on error resume next
  sql="DELETE FROM testreadings WHERE sampleid=" & strSampleID & " AND testid=" & strTestID & " AND trialnumber>1"
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=conn.Execute(sql)
  CloseDBObject(rs)
  CloseDBObject(conn)
  set rs=nothing
  set conn=nothing
  on error goto 0
end function

function deleteSelectedRecords()
'delete any records where Select is checked
dim conn,rs,sql,strFields,strValues, tLoop
on error resume next
  on error goto 0
	set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
	for iloop=1 to rows
	  if Request.Form("chksave" & iloop)="on" then
		  sql="DELETE FROM testreadings WHERE sampleid=" & strSampleID & " AND testid=" & strTestID & " AND trialnumber=" & iloop
		  conn.Execute(sql)
		  select case strTestID
			case "30","40"
				sql="DELETE FROM emspectro WHERE id=" & strSampleID & " AND testid=" & strTestID & " AND trialnum = " & iloop
				conn.Execute(sql)
			case "70"
				sql="DELETE FROM ftir WHERE sampleid=" & strSampleID
				conn.Execute(sql)
			case "160"
				sql="DELETE FROM particlecount WHERE id=" & strSampleID
				conn.Execute(sql)
			case "120","180","210","240"
				sql="DELETE FROM ParticleSubType WHERE sampleid=" & strSampleID & " AND testid=" & strTestID
				conn.Execute(sql)
				sql="DELETE FROM ParticleType WHERE sampleid=" & strSampleID & " AND testid=" & strTestID
				conn.Execute(sql)
		  end select
		end if
	next
	sql = "SELECT * FROM testreadings WHERE sampleid=" & strSampleID & " AND testid=" & strTestID & " order by trialnumber"
  set rs=conn.Execute(sql)
	if not rs.EOF then
	  tLoop=1
		do while not rs.EOF
		  iloop = rs("trialNumber").value
			if rs("trialNumber").value <> tLoop then
				sql = "UPDATE testReadings set trialNumber=" & tLoop & " WHERE sampleid=" & strSampleID & " AND testid=" & strTestID & " AND trialnumber=" & iloop 
			  conn.Execute(sql)
			end if
		  tLoop=tLoop + 1			  
			rs.movenext
		loop
	else
		AddTestToSchedule strSampleID, strTestID, ""
	end if
  CloseDBObject(rs)
  CloseDBObject(conn)
  set rs=nothing
	set conn=nothing
end function

sub enterReadings(blnPartial)
dim valid,valdate,schedtype,entid, curEntryid, curEntryDate, sql, conn, rs, curQualified
  entid=Session("USR")
  if status="T" or status="A" or (strTestID="210" and status="P" and (qualified(strTestID) = "Q/QAG" or qualified(strTestID) = "MicrE")) then
	valid="NULL"
	valdate="NULL"
  else
	valid=entid
	valdate="'" & datenow & "'"
  end if
  if blnPartial=true and not(strTestID="210" and status="P" and (qualified(strTestID) = "Q/QAG" or qualified(strTestID) = "MicrE")) and not((strTestID="180" or strTestID="120" or strTestID="240") and ((status="T" and qualified(strTestID) = "TRAIN") or (status="E" and qualified(strTestID) = "Q/QAG" and Request.Form("hidPrev210DispStat")="X"))) then
    entid = "NULL"
    datenow = "NULL"
  end if
  if (strTestID="210" and qualified(strTestID) = "MicrE" and (status="P" or status="S") and Request.Form("hidPrev210DispStat")="P") or ((strTestID="180" or strTestID="120" or strTestID="240") and qualified(strTestID) = "Q/QAG" and status="E" and Request.Form("hidPrev210DispStat")="T")then
	sql = "SELECT entryID, entryDate from TestReadings WHERE sampleid=" & strSampleID & " AND testid=" & strTestID & " AND trialnumber=1"
	set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
	set rs=conn.Execute(sql)
	if not rs.EOF then
		curEntryID = rs("entryID").value
		curEntryDate = rs("entryDate").value
	end if
	CloseDBObject(rs)
	set rs=nothing
	curQualified = ""
	sql="SELECT qualificationLevel FROM LubeTechQualification INNER JOIN Test ON LubeTechQualification.testStandID = Test.testStandID WHERE id=" & strTestID & " AND employeeid='" & curEntryID & "'"
	set rs=ForwardOnlyRS(sql,conn)
	if not(rs.BOF and rs.EOF) then
		curQualified=rs.Fields("qualificationLevel")
	end if
	CloseDBObject(rs)
	CloseDBObject(conn)
	set rs=nothing
	set conn=nothing
	if (strTestID = "210" and curQualified = "Q/QAG") or ((strTestID="180" or strTestID="120" or strTestID="240") and Request.Form("hidPrev210DispStat")<>"X") then
		entid = curEntryID
		datenow = curEntryDate
	end if
  end if
	
  if entid<>"NULL" then
    entid="'" & entid & "'"
  end if
  if valid<>"NULL" then
    valid="'" & valid & "'"
  end if

  if datenow<>"NULL" then
    datenow="'" & datenow & "'"
  end if
  
  schedtype=scheduleType(strTestID)
  iTrial=0
  select case strTestID
    case "30","40"
      for iloop=1 to rows
        if Request.Form("chksave" & iloop)="on" then
          iTrial=iTrial+1
          if insertRecord(strSampleID,strTestID,iTrial,"NULL","NULL","NULL","NULL","NULL","NULL","NULL",status,schedtype,entid,valid,datenow,valdate, strComment)=false then
            Response.Write("insertRecord Failed<BR>")
            blnSaved=false
            exit for
          end if
          if insertSpectro(strSampleID,strTestID,iTrial,datenow,Request.Form("numna"&iloop),Request.Form("nummo"&iloop),Request.Form("nummg"&iloop),Request.Form("nump"&iloop),Request.Form("numb"&iloop),Request.Form("numh"&iloop),Request.Form("numcr"&iloop),Request.Form("numca"&iloop),Request.Form("numni"&iloop),Request.Form("numag"&iloop),Request.Form("numcu"&iloop),Request.Form("numsn"&iloop),Request.Form("numal"&iloop),Request.Form("nummn"&iloop),Request.Form("numpb"&iloop),Request.Form("numfe"&iloop),Request.Form("numsi"&iloop),Request.Form("numba"&iloop),Request.Form("numsb"&iloop),Request.Form("numzn"&iloop))=false then
            blnSaved=false
            exit for
          end if
        end if
      next
      if Request.Form("chkschedule1")="on" then
        select case strTestID
          case "30"
            AddTestToSchedule strSampleID,"40",schedtype
          case "40"
            AddTestToSchedule strSampleID,"210",schedtype
        end select
      end if
    case "70"
      for iloop=1 to rows
        if Request.Form("chksave" & iloop)="on" then
          iTrial=iTrial+1
          if insertRecord(strSampleID,strTestID,iTrial,"NULL","NULL","NULL","NULL","NULL","NULL","NULL",status,schedtype,entid,valid,datenow,valdate, strComment)=false then
            blnSaved=false
            exit for
          end if
          if insertFTIR(strSampleID,nullIfEmpty(Request.Form("numcontam"&iloop)),nullIfEmpty(Request.Form("numanti_oxidant"&iloop)),nullIfEmpty(Request.Form("numoxidation"&iloop)),nullIfEmpty(Request.Form("numh2o"&iloop)),nullIfEmpty(Request.Form("numzddp"&iloop)),nullIfEmpty(Request.Form("numsoot"&iloop)),nullIfEmpty(Request.Form("numfuel_dilution"&iloop)),nullIfEmpty(Request.Form("nummixture"&iloop)),nullIfEmpty(Request.Form("numnlgi"&iloop)))=false then
            blnSaved=false
            exit for
          else
            AutoAddRemoveTests strSampleID,strTag,strComp,strLoc,strTestID
          end if
        end if
      next
    case "160"
      for iloop=1 to rows
        if Request.Form("chksave" & iloop)="on" then
          iTrial=iTrial+1
          if insertRecord(strSampleID,strTestID,iTrial,"NULL","NULL","NULL","NULL","NULL","NULL","NULL",status,schedtype,entid,valid,datenow,valdate, strComment)=false then
            blnSaved=false
            exit for
          end if
          if insertParticleCount(strSampleID,Request.Form("nummicron_5_10"&iloop),Request.Form("nummicron_10_15"&iloop),Request.Form("nummicron_15_25"&iloop),Request.Form("nummicron_25_50"&iloop),Request.Form("nummicron_50_100"&iloop),Request.Form("nummicron_100"&iloop),Request.Form("txtnas_class"&iloop))=false then
            blnSaved=false
            exit for
          end if
        end if
      next
    case "220"
      dim value1
      for iloop=1 to rows
        if Request.Form("chksave" & iloop)="on" then
          iTrial=iTrial+1
          if insertRecord(strSampleID,strTestID,iTrial,nullIfEmpty(Request.Form("radValue1"&iloop)),nullIfEmpty(Request.Form("numValue2"&iloop)),nullIfEmpty(Request.Form("numValue3"&iloop)),nullIfEmpty(Request.Form("numtrialcalc"&iloop)),nullIfEmpty(Request.Form("txtid1"&iloop)),nullIfEmpty(Request.Form("txtid2"&iloop)),nullIfEmpty(Request.Form("txtid3"&iloop)),status,schedtype,entid,valid,datenow,valdate, strComment)=false then
            blnSaved=false
            exit for
          end if
        end if
      next
    case "120","180","210","240"
      for iloop=1 to rows
        if Request.Form("chksave" & iloop)="on" then
          iTrial=iTrial+1
          if (blnPartial = true or blnMicrNeeded = true) and status = "X" THEN
				status = "E"
		  end if
		  if insertRecord(strSampleID,strTestID,iTrial,nullIfEmpty(Request.Form("numValue1"&iloop)),nullIfEmpty(Request.Form("numValue2"&iloop)), nullIfEmpty(Request.Form("numValue3"&iloop)),"NULL",nullIfEmpty(Request.Form("radid1")),nullIfEmpty(Request.Form("radid2")),nullIfEmpty(Request.Form("txtid3"&iloop)),status,schedtype,entid,valid,datenow,valdate, nullIfEmpty(Request.Form("txtMainComments"&iloop)))=false then
            blnSaved=false
            exit for
          end if          
          blnSaved=processParticleType(strTestID,strSampleID)
        end if
      next

    case "280"
      if Request.Form("chksave1")="on" then
        if insertRecord(strSampleID,280,1,nullIfEmpty(Request.Form("numvalue11")),"NULL","NULL","NULL","NULL","NULL","NULL",status,schedtype,entid,valid,datenow,valdate, strComment)=false then
          blnSaved=false
        end if
      end if
      if Request.Form("chksave2")="on" then
        if insertRecord(strSampleID,281,1,nullIfEmpty(Request.Form("numvalue12")),"NULL","NULL","NULL","NULL","NULL","NULL",status,schedtype,entid,valid,datenow,valdate, strComment)=false then
          blnSaved=false
        end if
      end if
      if Request.Form("chksave3")="on" then
        if insertRecord(strSampleID,282,1,nullIfEmpty(Request.Form("numvalue13")),"NULL","NULL","NULL","NULL","NULL","NULL",status,schedtype,entid,valid,datenow,valdate, strComment)=false then
          blnSaved=false
        end if
      end if
      if Request.Form("chksave4")="on" then
        if insertRecord(strSampleID,283,1,nullIfEmpty(Request.Form("numvalue14")),"NULL","NULL","NULL","NULL","NULL","NULL",status,schedtype,entid,valid,datenow,valdate, strComment)=false then
          blnSaved=false
        end if
      end if
    case else
      for iloop=1 to rows
        if Request.Form("chksave" & iloop)="on" then
          iTrial=iTrial+1
          if insertRecord(strSampleID,strTestID,iTrial,nullIfEmpty(Request.Form("numValue1"&iloop)),nullIfEmpty(Request.Form("numValue2"&iloop)),nullIfEmpty(Request.Form("numValue3"&iloop)),nullIfEmpty(Request.Form("numtrialcalc"&iloop)),nullIfEmpty(Request.Form("txtid1"&iloop)),nullIfEmpty(Request.Form("txtid2"&iloop)),nullIfEmpty(Request.Form("txtid3"&iloop)),status,schedtype,entid,valid,datenow,valdate, strComment)=false then
            blnSaved=false
            exit for
          end if
        end if
      next
  end select
end sub

function nullIfEmpty(strValue)
  if len(strValue)=0 then
    nullIfEmpty="NULL"
  else
    nullIfEmpty=strValue
  end if
end function

sub validateReadings()
  blnSaved=markRecordsValid(strSampleID,strTestID)
end sub

sub rejectReadings()
  blnSaved=markRecordsRejected(strSampleID,strTestID)
end sub

function qualified(tid)
'establish qualification level
dim conn,rs,sql
on error resume next
  qualified=""
  sql="SELECT qualificationLevel FROM LubeTechQualification INNER JOIN Test ON LubeTechQualification.testStandID = Test.testStandID WHERE id=" & tid & " AND employeeid='" & Session("USR") & "'"
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=ForwardOnlyRS(sql,conn)
  if not(rs.BOF and rs.EOF) then
    qualified=rs.Fields("qualificationLevel")
  end if
  CloseDBObject(rs)
  CloseDBObject(conn)
  set rs=nothing
  set conn=nothing
  on error goto 0
end function

function qualifiedToReview(sid,tid)
'establish qualification level
dim conn,rs,sql
on error resume next
  qualifiedToReview=qualified(tid)
  if qualifiedToReview="Q/QAG" or qualifiedToReview="MicrE"then
    sql="SELECT entryID FROM TestReadings WHERE sampleid=" & sid & " AND testid=" & tid
    set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
    set rs=ForwardOnlyRS(sql,conn)
    if not(rs.BOF and rs.EOF) then
      if rs.Fields("entryID")=Session("USR") then
        qualifiedToReview="NO"
      end if
    end if
    CloseDBObject(rs)
    CloseDBObject(conn)
    set rs=nothing
    set conn=nothing
    on error goto 0
  end if
end function

function scheduleType(tid)
'get schedtype from the existing record
dim conn,rs,sql
on error resume next
  scheduleType="NULL"
  sql="SELECT schedtype FROM testreadings WHERE sampleid=" & strSampleID & " AND testid=" & tid & " AND trialnumber=1"
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=ForwardOnlyRS(sql,conn)
  if not(rs.BOF and rs.EOF) then
    if isnull(rs.Fields("schedtype")) then
      scheduleType="NULL"
    else
      scheduleType="'" & rs.Fields("schedtype") & "'"
    end if
  end if
  CloseDBObject(rs)
  CloseDBObject(conn)
  set rs=nothing
  set conn=nothing
  on error goto 0
end function

function insertRecord(sid,tid,tno,v1,v2,v3,tcalc,id1,id2,id3,stat,stype,eid,vid,edate,vdate,com)
'insert a new record in testreadings
dim conn,rs,sql,strFields,strValues,iPos
  insertRecord=false
  iPos=instr(id1,"|")
  if iPos>0 then
    id1=left(id1,iPos-1)
  end if
  iPos=instr(id2,"|")
  if iPos>0 then
    id2=left(id2,iPos-1)
  end if
  iPos=instr(id3,"|")
  if iPos>0 then
    id3=left(id3,iPos-1)
  end if
  if id1<>"NULL" then
    id1="'" & id1 & "'"
  end if
  if id2<>"NULL" then
    id2="'" & id2 & "'"
  end if
  if id3<>"NULL" then
    id3="'" & id3 & "'"
  end if
  if com<>"NULL" then
    com="'" & com & "'"
  end if
  sql="SELECT * FROM testreadings WHERE sampleid=" & sid & " AND testid=" & tid & " AND trialnumber=" & tno
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=ForwardOnlyRS(sql,conn)
  if rs.BOF and rs.EOF then
    strFields="(sampleid,testid,trialnumber,value1,value2,value3,trialcalc,id1,id2,id3,trialcomplete,status,schedtype,entryid,validateid,entrydate,validate,MainComments)"
    strValues="(" & sid & "," & tid & "," & tno & "," & v1 & "," & v2 & "," & v3 & "," & tcalc & "," & id1 & "," & id2 & "," & id3 & ",0,'" & stat & "'," & stype & "," & eid & "," & vid & "," & edate & "," & vdate & "," & com & ")"
    sql="INSERT INTO testreadings " & strFields & " VALUES " & strValues
    Err.Clear
    set rs=conn.Execute(sql)
    if DBErrorCount(conn)>0 then
      insertRecord=false
      strDBError=DBErrors(conn)
      strSQLFailed=sql
    else
      insertRecord=true
    end if
    CloseDBObject(rs)
    CloseDBObject(conn)
    set rs=nothing
    set conn=nothing
  else
    CloseDBObject(rs)
    CloseDBObject(conn)
    set rs=nothing
    set conn=nothing
    insertRecord=updateRecord(sid,tid,tno,v1,v2,v3,tcalc,id1,id2,id3,stat,stype,eid,vid,edate,vdate,com)
  end if
  on error goto 0
end function

function updateRecord(sid,tid,tno,v1,v2,v3,tcalc,id1,id2,id3,stat,stype,eid,vid,edate,vdate,com)
'update an existing record in testreadings
dim conn,rs,sql,strSet,strWhere
on error resume next
  strWhere="sampleid=" & sid & " AND testid=" & tid & " AND trialnumber=" & tno
  strSet="value1=" & v1 & ",value2=" & v2 & ",value3=" & v3 & ",id1=" & id1 & ",id2=" & id2 & ",id3=" & id3 & ",trialcalc=" & tcalc
  strSet=strSet&",status='" & stat & "',entryid=" & eid & ",validateid=" & vid & ",entrydate=" & edate & ",validate=" & vdate & ",MainComments=" & com
  sql="UPDATE testreadings SET " & strSet & " WHERE " & strWhere
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=conn.Execute(sql)
  if DBErrorCount(conn)>0 then
    updateRecord=false
    strDBError=DBErrors(conn)
    strSQLFailed=sql
  else
    updateRecord=true
  end if
  CloseDBObject(rs)
  CloseDBObject(conn)
  set rs=nothing
  set conn=nothing
end function

function markRecordsValid(sid,tid)
'update an existing record in testreadings
dim conn,rs,sql,strSet,strWhere
on error resume next
  strWhere="sampleid=" & sid & " AND testid=" & tid
  strSet="status='D',validateid='" & Session("USR") & "',validate='" & datenow & "'"
  sql="UPDATE testreadings SET " & strSet & " WHERE " & strWhere
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=conn.Execute(sql)
  if DBErrorCount(conn)>0 then
    markRecordsValid=false
    strDBError=DBErrors(conn)
    strSQLFailed=sql
  else
    markRecordsValid=true
  end if
  CloseDBObject(rs)
  CloseDBObject(conn)
  set rs=nothing
  set conn=nothing
end function

function markReadyForMicroscope(sid,tid,status)
dim conn,rs,sql,strSet,strWhere
on error resume next
  strWhere="sampleid=" & sid & " AND testid=" & tid
  sql="Update testreadings set status = '" & status & "' WHERE " & strWhere
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  conn.Execute sql
  sql="Update UsedLubeSamples set status = 90, returnedDate = GetDate() WHERE sampleid=" & sid
  
  conn.Execute sql 
end function

function markRecordsRejected(sid,tid)
'update an existing record in testreadings to require results
dim conn,rs,sql,strSet,strWhere
on error resume next
  strWhere="sampleid=" & sid & " AND testid=" & tid
  sql="DELETE FROM testreadings WHERE " & strWhere
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=conn.Execute(sql)

  select case tid
    case "30","40"
      sql="DELETE FROM emspectro WHERE id=" & sid & " AND testid=" & tid
    case "70"
      sql="DELETE FROM ftir WHERE sampleid=" & sid
    case "160"
      sql="DELETE FROM particlecount WHERE id=" & sid
    case "120","180","210","240"
      sql="DELETE FROM ParticleSubType WHERE sampleid=" & sid & " AND testid=" & tid
	  conn.Execute(sql)
	  sql="DELETE FROM ParticleType WHERE sampleid=" & sid & " AND testid=" & tid
    case else
      sql=""
  end select
  if len(sql)>0 then
    set rs=conn.Execute(sql)
  end if
  if strTestID="210" then
	sql="INSERT INTO testreadings (sampleid,testid,trialnumber,status) VALUES (" & sid & "," & tid & ",1,'E')"
  else
	sql="INSERT INTO testreadings (sampleid,testid,trialnumber,status) VALUES (" & sid & "," & tid & ",1,'A')"
  end if
  set rs=conn.Execute(sql)
  if DBErrorCount(conn)>0 then
    markRecordsRejected=false
    strDBError=DBErrors(conn)
    strSQLFailed=sql
  else
    markRecordsRejected=true
  end if
  CloseDBObject(rs)
  CloseDBObject(conn)
  set rs=nothing
  set conn=nothing
end function

function insertSpectro(sid,tid,tno,tdate,na,mo,mg,p,b,h,cr,ca,ni,ag,cu,sn,al,mn,pb,fe,si,ba,c,zn)
'insert a new record in emSpectro
dim conn,rs,sql,strFields,strValues
on error resume next
  insertSpectro=false
  sql="SELECT * FROM emSpectro WHERE id=" & sid & " AND testid=" & tid & " AND trialnum=" & tno
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=ForwardOnlyRS(sql,conn)
  if rs.BOF and rs.EOF then
		strFields="(id,testid,na,mo,mg,p,b,h,cr,ca,ni,ag,cu,sn,al,mn,pb,fe,si,ba,zn,trialdate,trialnum)"
    strValues="(" & sid & "," & tid & "," & na & "," & mo & "," & mg & "," & p & "," & b & "," & h & "," & cr & "," & ca & "," & ni & "," & ag & "," & cu & "," & sn & "," & al & "," & mn & "," & pb & "," & fe & "," & si & "," & ba & "," & zn & "," & tdate & "," & tno & ")"
    sql="INSERT INTO emSpectro " & strFields & " VALUES " & strValues
    set rs=conn.Execute(sql)
    if DBErrorCount(conn)>0 then
      insertSpectro=false
      strDBError=DBErrors(conn)
      strSQLFailed=sql
    else
      insertSpectro=true
    end if
    CloseDBObject(rs)
    CloseDBObject(conn)
    set rs=nothing
    set conn=nothing
  else
    CloseDBObject(rs)
    CloseDBObject(conn)
    set rs=nothing
    set conn=nothing
    insertSpectro=updateSpectro(sid,tid,tno,tdate,na,mo,mg,p,b,h,cr,ca,ni,ag,cu,sn,al,mn,pb,fe,si,ba,c,zn)
  end if
end function

function updateSpectro(sid,tid,tno,tdate,na,mo,mg,p,b,h,cr,ca,ni,ag,cu,sn,al,mn,pb,fe,si,ba,c,zn)
'update an existing record in emSpectro
dim conn,rs,sql,strSet,strWhere
on error resume next
  strWhere="id=" & sid & " AND testid=" & tid & " AND trialnum=" & tno
  strSet="na=" & na & ",mo=" & mo & ",mg=" & mg & ",p=" & p & ",b=" & b & ",h=" & h & ",cr=" & cr & ",ca=" & ca & ",ni=" & ni & ",ag=" & ag & ",cu=" & cu & ",sn=" & sn & ",al=" & al & ",mn=" & mn & ",pb=" & pb & ",fe=" & fe & ",si=" & si & ",ba=" & ba & ",zn=" & zn 
  strSet=strSet&",trialdate=" & tdate
  sql="UPDATE emSpectro SET " & strSet & " WHERE " & strWhere
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=conn.Execute(sql)
  if DBErrorCount(conn)>0 then
    updateSpectro=false
    strDBError=DBErrors(conn)
    strSQLFailed=sql
  else
    updateSpectro=true
  end if
  CloseDBObject(rs)
  CloseDBObject(conn)
  set rs=nothing
  set conn=nothing
end function

function insertFTIR(sid,da,ao,ox,ho,aw,so,fd,mx,nl)
'insert a new record in FTIR
dim conn,rs,sql,strFields,strValues
on error resume next
  insertFTIR=false
  sql="SELECT * FROM FTIR WHERE sampleid=" & sid
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=ForwardOnlyRS(sql,conn)
  if rs.BOF and rs.EOF then
    strFields="(sampleid,contam,anti_oxidant,oxidation,h2o,zddp,soot,fuel_dilution,mixture,nlgi)"
    strValues="(" & sid & "," & da & "," & ao & "," & ox & "," & ho & "," & aw & "," & so & "," & fd & "," & mx & "," & nl & ")" 
    sql="INSERT INTO FTIR " & strFields & " VALUES " & strValues
    set rs=conn.Execute(sql)
    if DBErrorCount(conn)>0 then
      insertFTIR=false
      strDBError=DBErrors(conn)
      strSQLFailed=sql
    else
      insertFTIR=true
    end if
    CloseDBObject(rs)
    CloseDBObject(conn)
    set rs=nothing
    set conn=nothing
  else
    CloseDBObject(rs)
    CloseDBObject(conn)
    set rs=nothing
    set conn=nothing
    insertFTIR=updateFTIR(sid,da,ao,ox,ho,aw,so,fd,mx,nl)
  end if
end function

function updateFTIR(sid,da,ao,ox,ho,aw,so,fd,mx,nl)
'update an existing record in FTIR
dim conn,rs,sql,strSet,strWhere
on error resume next
  strWhere="sampleid=" & sid
  strSet="contam=" & result
  strSet="contam=" & da & ",anti_oxidant=" & ao & ",oxidation=" & ox & ",h2o=" & ho & ",zddp=" & aw & ",soot=" & so & ",fuel_dilution=" & fd & ",mixture=" & mx & ",nlgi=" & nl
  sql="UPDATE FTIR SET " & strSet & " WHERE " & strWhere
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=conn.Execute(sql)
  if DBErrorCount(conn)>0 then
    updateFTIR=false
    strDBError=DBErrors(conn)
    strSQLFailed=sql
  else
    updateFTIR=true
  end if
  CloseDBObject(rs)
  CloseDBObject(conn)
  set rs=nothing
  set conn=nothing
end function

function insertParticleCount(sid,m1,m2,m3,m4,m5,m6,nas)
'insert a new record in Particle Count
dim conn,rs,sql,strFields,strValues
on error resume next
  insertParticleCount=false
  sql="SELECT * FROM ParticleCount WHERE id=" & sid
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=ForwardOnlyRS(sql,conn)
  if rs.BOF and rs.EOF then
    strFields="(id,micron_5_10,micron_10_15,micron_15_25,micron_25_50,micron_50_100,micron_100,testDate,nas_class)"
    strValues="(" & sid & "," & m1 & "," & m2 & "," & m3 & "," & m4 & "," & m5 & "," & m6 & ",GetDate()," & nas & ")" 
    sql="INSERT INTO ParticleCount " & strFields & " VALUES " & strValues
    set rs=conn.Execute(sql)
    if DBErrorCount(conn)>0 then
      insertParticleCount=false
      strDBError=DBErrors(conn)
      strSQLFailed=sql
    else
      insertParticleCount=true
    end if
    CloseDBObject(rs)
    CloseDBObject(conn)
    set rs=nothing
    set conn=nothing
  else
    CloseDBObject(rs)
    CloseDBObject(conn)
    set rs=nothing
    set conn=nothing
    insertParticleCount=updateParticleCount(sid,m1,m2,m3,m4,m5,m6,nas)
  end if
end function

function updateParticleCount(sid,m1,m2,m3,m4,m5,m6,nas)
'update an existing record in Particle Count
dim conn,rs,sql,strSet,strWhere
on error resume next
  strWhere="id=" & sid
  strSet="micron_5_10=" & m1 & ",micron_10_15=" & m2 & ",micron_15_25=" & m3 & ",micron_25_50=" & m4 & ",micron_50_100=" & m5 & ",micron_100=" & m6 & ",testDate=GetDate(),nas_class=" & nas
  sql="UPDATE ParticleCount SET " & strSet & " WHERE " & strWhere
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  set rs=conn.Execute(sql)
  if DBErrorCount(conn)>0 then
    updateParticleCount=false
    strDBError=DBErrors(conn)
    strSQLFailed=sql
  else
    updateParticleCount=true
  end if
  CloseDBObject(rs)
  CloseDBObject(conn)
  set rs=nothing
  set conn=nothing
end function

function validateFerrogramCommentsLen(comm2)
dim comm3
  if len(comm2) <= commentLen then
    comm3 = comm2
  else
    comm3 = left(comm2,commentLen)
  end if	    
  
  validateFerrogramComments = comm3  
end function

function processParticleType(tid,sid)
dim conn, blnOK
  processParticleType = true
on error resume next
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))

  for iloop = 1 to subRows
    blnOK=insertParticleType(tid,sid,iloop,conn,Request.Form("radstatus"&iloop),Request.Form("comment"&iloop))
  next

  CloseDBObject(conn)
  set conn=nothing
end function

function insertParticleType(tid,sid,ptId,conn,sta,com)
dim rs,sql,strFields,strValues
on error resume next
  insertParticleType=false

  sql="SELECT * FROM ParticleType WHERE sampleID=" & sid & " AND testID=" & tid & " AND ParticleTypeDefinitionID=" & ptId
  set rs=ForwardOnlyRS(sql,conn)
  if rs.BOF and rs.EOF then
    strFields="(sampleID,testID,ParticleTypeDefinitionID,Status,Comments)"
    strValues="(" & sid & "," & tid & "," & ptId & ",'" & sta & "','" & com & "')"
    sql="INSERT INTO ParticleType " & strFields & " VALUES " & strValues
    set rs=conn.Execute(sql)
    if DBErrorCount(conn)>0 then
      insertParticleType=false
      strDBError=DBErrors(conn)
      strSQLFailed=sql
    else
      insertParticleType=true
    end if
    CloseDBObject(rs)
    set rs=nothing
  else
    CloseDBObject(rs)
    set rs=nothing
    insertParticleType=updateParticleType(tid,sid,ptId,conn,sta,com)
  end if
  if (sta = 1) then
    processParticleSubTypes tid,sid,ptId,conn
  else
    deleteParticleSubTypes tid,sid,ptId,conn
  end if
end function

function updateParticleType(tid,sid,ptId,conn,sta,com)
dim rs,sql,strSet,strWhere
on error resume next
  strWhere="testID = " & tid & " AND sampleID=" & sid & " AND ParticleTypeDefinitionID=" & ptId
  strSet="Status='" & sta & "',Comments='" & com & "'"
  sql="UPDATE ParticleType SET " & strSet & " WHERE " & strWhere
  set rs=conn.Execute(sql)
  if DBErrorCount(conn)>0 then
    updateParticleType=false
    strDBError=DBErrors(conn)
    strSQLFailed=sql
  else
    updateParticleType=true
  end if
  CloseDBObject(rs)
  set rs=nothing
end function

function SubTypes(tid,sid,ptId,conn)
    dim rs,sql
    sql="SELECT c.ID categoryID, c.Description, s.Value FROM ParticleSubTypeCategoryDefinition c LEFT OUTER JOIN (SELECT * FROM ParticleSubType WHERE SampleID = " & sid & " AND testID = " & tid & " AND ParticleTypeDefinitionID = " & ptid & ") s ON c.ID = s.ParticleSubTypeCategoryID"
    on error resume next
    set rs=DisconnectedRS(sql,conn)
    Set SubTypes = rs
end function

sub processParticleSubTypes(tid,sid,ptId,conn)
dim rsSub, cat, catId, val, blnOK
on error resume next

  set rsSub = SubTypes(tid,sid,ptId,conn)

  Do While Not rsSub.EOF
    cat = getField(rsSub,"Description")
    val = Request.Form("rad" & cat & ptId)
    if (val <> getField(rsSub,"Value")) then
      blnOK = insertParticleSubType(tid,sid,ptId,getField(rsSub,"categoryID"),val,conn)
    end if
    rsSub.MoveNext
  Loop
end sub

function insertParticleSubType(tid,sid,ptId,cid,val,conn)
dim rs,sql,strFields,strValues
on error resume next
  insertParticleSubType=false

  sql="SELECT * FROM ParticleSubType WHERE sampleID=" & sid & " AND testID=" & tid & " AND ParticleTypeDefinitionID=" & ptId & " AND ParticleSubTypeCategoryID=" & cid
  set rs=ForwardOnlyRS(sql,conn)
  if rs.BOF and rs.EOF then
    strFields="(sampleID,testID,ParticleTypeDefinitionID,ParticleSubTypeCategoryID,Value)"
    strValues="(" & sid & "," & tid & "," & ptId & "," & cid & "," & val & ")"
    sql="INSERT INTO ParticleSubType " & strFields & " VALUES " & strValues
    set rs=conn.Execute(sql)
    if DBErrorCount(conn)>0 then
      insertParticleSubType=false
      strDBError=DBErrors(conn)
      strSQLFailed=sql
    else
      insertParticleSubType=true
    end if
    CloseDBObject(rs)
    set rs=nothing
  else
    CloseDBObject(rs)
    set rs=nothing
    insertParticleSubType=updateParticleSubType(tid,sid,ptId,cid,val,conn)
  end if
end function

function updateParticleSubType(tid,sid,ptId,cid,val,conn)
dim rs,sql,strSet,strWhere
on error resume next
  strWhere="testID = " & tid & " AND sampleID=" & sid & " AND ParticleTypeDefinitionID=" & ptId & " AND ParticleSubTypeCategoryID=" & cid
  strSet="Value=" & val
  sql="UPDATE ParticleSubType SET " & strSet & " WHERE " & strWhere
  set rs=conn.Execute(sql)
  if DBErrorCount(conn)>0 then
    updateParticleType=false
    strDBError=DBErrors(conn)
    strSQLFailed=sql
  else
    updateParticleSubType=true
  end if
  CloseDBObject(rs)
  set rs=nothing
end function

function deleteParticleSubTypes(tid,sid,ptId,conn)
dim rs,sql,strWhere
on error resume next
  strWhere="testID = " & tid & " AND sampleID=" & sid & " AND ParticleTypeDefinitionID=" & ptId
  sql="DELETE FROM ParticleSubType WHERE " & strWhere
  set rs=conn.Execute(sql)
  if DBErrorCount(conn)>0 then
    deleteParticleSubTypes=false
    strDBError=DBErrors(conn)
    strSQLFailed=sql
  else
    deleteParticleSubTypes=true
  end if
  CloseDBObject(rs)
  set rs=nothing
end function
%>