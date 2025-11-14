<%
function historySQL(testID,tag,comp,loc,sampleid,rows,old)
dim sql, strNewLubeType
	strNewLubeType=CheckNew (sampleid)

  select case testID
    case "30","40"		'EM Spectroscopy
      sql="SELECT top " & rows & " u.ID,u.lubeType,u.sampleDate,e.trialNum AS trial,e.testID,e.Na,e.Mo,e.Mg,e.P,e.B,e.H,e.Cr,e.Ca,e.Ni,e.Ag,e.Cu,e.Sn,e.Al,e.Mn,e.Pb,e.Fe,e.Si,e.Ba,e.Sb,e.Zn "
      sql=sql & "FROM UsedLubeSamples u INNER JOIN "
      sql=sql & "EmSpectro e ON u.ID = e.ID "
      if trim(strNewLubeType)="" then
		    sql=sql & "WHERE u.tagNumber = '" & tag & "' "
		    sql=sql & "AND u.component = '" & comp & "' "
		    sql=sql & "AND u.location = '" & loc & "' "
	    else
	    	sql=sql & "WHERE u.lubeType = '" & strNewLubeType & "' "
	    	sql=sql & "AND u.newUsedFlag = 1 "
	    end if
	    if blnInclusive then
				sql=sql & "AND u.ID <= " & sampleid	
	    else
				sql=sql & "AND u.ID < " & sampleid	
	    end if
      sql=sql & " AND e.testid=" & testID
      sql=sql & " ORDER BY u.ID DESC"
    case "70"	'FTIR
      sql="SELECT top " & rows & " u.ID,u.lubeType,u.sampleDate, FTIR.* "
      sql=sql & "FROM UsedLubeSamples u INNER JOIN "
      sql=sql & "FTIR ON u.ID = FTIR.sampleID "
      if trim(strNewLubeType)="" then
		    sql=sql & "WHERE u.tagNumber = '" & tag & "' "
		    sql=sql & "AND u.component = '" & comp & "' "
		    sql=sql & "AND u.location = '" & loc & "' "
	    else
	    	sql=sql & "WHERE u.lubeType = '" & strNewLubeType & "' "
	    	sql=sql & "AND u.newUsedFlag = 1 "
	    end if
	    if blnInclusive then
				sql=sql & "AND u.ID <= " & sampleid	
	    else
				sql=sql & "AND u.ID < " & sampleid	
	    end if
      sql=sql & " ORDER BY u.ID DESC"
    case "120","180","210","240"	'Filter Residue/insp Filter/Ferrography/Debris Identification
      if old then
        if testID = 180 then
            sql="SELECT DISTINCT top " & rows & " u.ID, u.lubeType, u.sampleDate, i.narrative, c1.remark AS major, c2.remark AS minor, c3.remark AS trace,t.value2 "
            sql=sql & "FROM UsedLubeSamples u INNER JOIN "
            sql=sql & "TestReadings t ON u.ID = t.sampleID INNER JOIN "
            sql=sql & "InspectFilter i ON u.ID = i.ID LEFT OUTER JOIN "
            sql=sql & "Comments c2 ON i.minor = c2.ID LEFT OUTER JOIN "
            sql=sql & "Comments c3 ON i.trace = c3.ID LEFT OUTER JOIN "
            sql=sql & "Comments c1 ON i.major = c1.ID "
            if trim(strNewLubeType)="" then
		      sql=sql & "WHERE u.tagNumber = '" & tag & "' "
		      sql=sql & "AND u.component = '" & comp & "' "
		      sql=sql & "AND u.location = '" & loc & "' "
	        else
	          sql=sql & "WHERE u.lubeType = '" & strNewLubeType & "' "
	          sql=sql & "AND u.newUsedFlag = 1 "
	        end if
	        if blnInclusive then
		      sql=sql & "AND u.ID <= " & sampleid	
	        else
		      sql=sql & "AND u.ID < " & sampleid	
	        end if
            sql=sql & " AND (i.testid=" & testID & " OR i.testid IS NULL) "
            sql=sql & "AND t.testid=" & testID
            sql=sql & " ORDER BY u.ID DESC"
        else
            sql="SELECT top " & rows & " u.ID, u.lubeType, u.sampleDate, i.narrative, c1.remark AS major, c2.remark AS minor, c3.remark AS trace "
            sql=sql & "FROM UsedLubeSamples u INNER JOIN "
            sql=sql & "InspectFilter i ON u.ID = i.ID LEFT OUTER JOIN "
            sql=sql & "Comments c2 ON i.minor = c2.ID LEFT OUTER JOIN "
            sql=sql & "Comments c3 ON i.trace = c3.ID LEFT OUTER JOIN "
            sql=sql & "Comments c1 ON i.major = c1.ID "
            if trim(strNewLubeType)="" then
		      sql=sql & "WHERE u.tagNumber = '" & tag & "' "
		      sql=sql & "AND u.component = '" & comp & "' "
		      sql=sql & "AND u.location = '" & loc & "' "
	        else
	          sql=sql & "WHERE u.lubeType = '" & strNewLubeType & "' "
	          sql=sql & "AND u.newUsedFlag = 1 "
	        end if
	        if blnInclusive then
		      sql=sql & "AND u.ID <= " & sampleid	
	        else
		      sql=sql & "AND u.ID < " & sampleid	
	        end if
            sql=sql & " AND i.testid=" & testID
            sql=sql & " ORDER BY u.ID DESC"
        end if
      else
        sql="SELECT s.*, d.Type, d.SortOrder, d.Value, d.comments FROM ("
        sql=sql & "SELECT distinct top " & rows & " u.ID,t.value1,t.value2,t.value3,t.ID1,t.ID2,t.ID3,u.sampleDate,t.testID,t.MainComments "
        sql=sql & "FROM UsedLubeSamples u INNER JOIN "
        sql=sql & "TestReadings t ON u.ID = t.sampleID "
        sql=sql & "INNER JOIN vwParticleType d ON d.SampleID = t.sampleID AND d.testID = t.testID "
        if trim(strNewLubeType)="" then
		  sql=sql & "WHERE u.tagNumber = '" & tag & "' "
		  sql=sql & "AND u.component = '" & comp & "' "
		  sql=sql & "AND u.location = '" & loc & "' "
	    else
	      sql=sql & "WHERE u.lubeType = '" & strNewLubeType & "' "
	      sql=sql & "AND u.newUsedFlag = 1 "
	    end if
	    if blnInclusive then
		  sql=sql & "AND u.ID <= " & sampleid	
	    else
		  sql=sql & "AND u.ID < " & sampleid	
	    end if
        sql=sql & " AND t.testid=" & testID
        sql=sql & " ORDER BY u.ID DESC) s "
        sql=sql & "INNER JOIN vwParticleType d ON d.SampleID = s.ID AND d.testID = s.testID "
        sql=sql & "ORDER BY d.SortOrder, d.SampleID DESC"

      end if
    case "160"		'Particle Count
      sql="SELECT top " & rows & " u.ID,u.lubeType,u.sampleDate,p.micron_5_10,p.micron_10_15,p.micron_15_25,p.micron_25_50,p.micron_50_100,p.micron_100,p.iso_code,p.nas_class "
      sql=sql & "FROM UsedLubeSamples u INNER JOIN "
      sql=sql & "ParticleCount p ON u.ID = p.ID "
      if trim(strNewLubeType)="" then
		    sql=sql & "WHERE u.tagNumber = '" & tag & "' "
		    sql=sql & "AND u.component = '" & comp & "' "
		    sql=sql & "AND u.location = '" & loc & "' "
	    else
	    	sql=sql & "WHERE u.lubeType = '" & strNewLubeType & "' "
	    	sql=sql & "AND u.newUsedFlag = 1 "
	    end if
	    if blnInclusive then
				sql=sql & "AND u.ID <= " & sampleid	
	    else
				sql=sql & "AND u.ID < " & sampleid	
	    end if
      sql=sql & " ORDER BY u.ID DESC"
    case "250"		'Deleterious
      sql="SELECT top " & rows & " u.ID,u.lubetype,t.trialNumber,t.value1,t.value2,t.value3,t.trialcalc,t.ID1,t.ID2,t.ID3,u.sampleDate "
      sql=sql & "FROM UsedLubeSamples u INNER JOIN "
      sql=sql & "TestReadings t ON u.ID = t.sampleID "
      if trim(strNewLubeType)="" then
		    sql=sql & "WHERE u.tagNumber = '" & tag & "' "
		    sql=sql & "AND u.component = '" & comp & "' "
		    sql=sql & "AND u.location = '" & loc & "' "
	    else
	    	sql=sql & "WHERE u.lubeType = '" & strNewLubeType & "' "
	    	sql=sql & "AND u.newUsedFlag = 1 "
	    end if
	    if blnInclusive then
				sql=sql & "AND u.ID <= " & sampleid	
	    else
				sql=sql & "AND u.ID < " & sampleid	
	    end if
      sql=sql & " AND t.testid=" & testID
      sql=sql & " AND t.value2 IS NOT NULL "
      sql=sql & "ORDER BY u.ID DESC"    
    case "270"		'Rheometer
      sql="SELECT top " & rows & " * FROM vwRheometerHistory "
     if trim(strNewLubeType)="" then
		    sql=sql & "WHERE tagNumber = '" & tag & "' "
		    sql=sql & "AND component = '" & comp & "' "
		    sql=sql & "AND location = '" & loc & "' "
	    else
	    	sql=sql & "WHERE lubeType = '" & strNewLubeType & "' "
	    	sql=sql & "AND newUsedFlag = 1 "
	    end if
	    if blnInclusive then
				sql=sql & "AND SampleID <= " & sampleid	
	    else
				sql=sql & "AND SampleID < " & sampleid	
	    end if
      sql=sql & " ORDER BY SampleID DESC"
    case "280"		'Resistivity
      sql="SELECT top " & rows & " * FROM vwMiscTestHistory "
      sql=sql & "WHERE tagNumber = '" & tag & "' "
      sql=sql & "AND component = '" & comp & "' "
      sql=sql & "AND location = '" & loc & "' "
      sql=sql & " ORDER BY ID DESC"
    case else
      sql="SELECT top " & rows & " u.ID,u.lubetype,t.trialNumber,t.value1,t.value2,t.value3,t.trialcalc,t.ID1,t.ID2,t.ID3,u.sampleDate "
      sql=sql & "FROM UsedLubeSamples u INNER JOIN "
      sql=sql & "TestReadings t ON u.ID = t.sampleID "
      if trim(strNewLubeType)="" then
		    sql=sql & "WHERE u.tagNumber = '" & tag & "' "
		    sql=sql & "AND u.component = '" & comp & "' "
		    sql=sql & "AND u.location = '" & loc & "' "
	    else
	    	sql=sql & "WHERE u.lubeType = '" & strNewLubeType & "' "
	    	sql=sql & "AND u.newUsedFlag = 1 "
	    end if
	    if blnInclusive then
				sql=sql & "AND u.ID <= " & sampleid	
	    else
				sql=sql & "AND u.ID < " & sampleid	
	    end if
      sql=sql & " AND t.testid=" & testID
      sql=sql & " AND t.value1 IS NOT NULL "
      sql=sql & "ORDER BY u.ID DESC"
  end select
  historySQL=sql
end function

sub buildHistoryTable(testID,objRS,tag,comp,loc,old,resultsUrl,strCompName,strLocName,isLab)
dim htable(),rs,col, strNEWTest, resizeButton
dim fmtsampledate, lubept
  set rs=objRS
  fmtsampledate="=DatePart(""m"",rs.Fields(""sampleDate""))&""/""&DatePart(""d"",rs.Fields(""sampleDate""))&""/""&Right(DatePart(""yyyy"",rs.Fields(""sampleDate"")),2)"
  select case testID
    case "10"
      redim htable(2,1)
      htable(0,0)="TAN calc"
      htable(1,0)="Sample date"
      htable(2,0)="Sample#"
      htable(0,1)="value2"
      htable(1,1)=fmtsampledate
      htable(2,1)="ID"
    case "20","110", "284", "285", "286"
      redim htable(2,1)
      htable(0,0)="Result"
      htable(1,0)="Sample date"
      htable(2,0)="Sample#"
      htable(0,1)="value1"
      htable(1,1)=fmtsampledate
      htable(2,1)="ID"
    case "30","40"
      redim htable(19,1)
      htable(0,0)="Sample date"
      htable(1,0)="Sample#"
      htable(2,0)="Na"
      htable(3,0)="Mo"
      htable(4,0)="Mg"
      htable(5,0)="P"
      htable(6,0)="B"
      htable(7,0)="Cr"
      htable(8,0)="Ca"
      htable(9,0)="Ni"
      htable(10,0)="Ag"
      htable(11,0)="Cu"
      htable(12,0)="Sn"
      htable(13,0)="Al"
      htable(14,0)="Mn"
      htable(15,0)="Pb"
      htable(16,0)="Fe"
      htable(17,0)="Si"
      htable(18,0)="Ba"
      htable(19,0)="Zn"
      htable(0,1)=fmtsampledate
      htable(1,1)="ID"
      htable(2,1)="Na"
      htable(3,1)="Mo"
      htable(4,1)="Mg"
      htable(5,1)="P"
      htable(6,1)="B"
      htable(7,1)="Cr"
      htable(8,1)="Ca"
      htable(9,1)="Ni"
      htable(10,1)="Ag"
      htable(11,1)="Cu"
      htable(12,1)="Sn"
      htable(13,1)="Al"
      htable(14,1)="Mn"
      htable(15,1)="Pb"
      htable(16,1)="Fe"
      htable(17,1)="Si"
      htable(18,1)="Ba"
      htable(19,1)="Zn"
    case "50","60"
      redim htable(2,1)
      htable(0,0)="Result"
      htable(1,0)="Sample date"
      htable(2,0)="Sample#"
      htable(0,1)="=Round(rs.Fields(""value3""),2)"
      htable(1,1)=fmtsampledate
      htable(2,1)="ID"
    case "70"
	  redim htable(10,1)
      htable(0,0)="Sample date"
      htable(1,0)="Sample#"
      htable(2,0)="Delta area"
      htable(3,0)="Anti-oxidant"
      htable(4,0)="Oxidation"
      htable(5,0)="H2O"
      htable(6,0)="Anti-wear"
      htable(7,0)="Soot"
      htable(8,0)="Dilution"
      htable(9,0)="Mixture"
      htable(10,0)="Weak acid"
      htable(0,1)=fmtsampledate
      htable(1,1)="ID"
      htable(2,1)="contam"
      htable(3,1)="anti_oxidant"
      htable(4,1)="oxidation"
      htable(5,1)="h2o"
      htable(6,1)="zddp"
      htable(7,1)="soot"
      htable(8,1)="fuel_dilution"
      htable(9,1)="mixture"
      htable(10,1)="nlgi"
    case "80"
      redim htable(2,1)
      htable(0,0)="Result"
      htable(1,0)="Sample date"
      htable(2,0)="Sample#"
      htable(0,1)="value3"
      htable(1,1)=fmtsampledate
      htable(2,1)="ID"
    case "120","240"
      redim htable(5,1)
      htable(0,0)="Sample date"
      htable(1,0)="Sample#"
      htable(2,0)="Major"
      htable(3,0)="Minor"
      htable(4,0)="Trace"
      htable(5,0)="Other"
      htable(0,1)=fmtsampledate
      htable(1,1)="ID"
      htable(2,1)="major"
      htable(3,1)="minor"
      htable(4,1)="trace"
      htable(5,1)="narrative"
    case "130"
      redim htable(3,1)
      htable(0,0)="Sample date"
      htable(1,0)="Sample#"
      htable(2,0)="Result"
      htable(3,0)="NLGI"
      htable(0,1)=fmtsampledate
      htable(1,1)="ID"
      htable(2,1)="trialcalc"
      htable(3,1)="id1"
    case "140"
      redim htable(2,1)
      htable(0,0)="Sample date"
      htable(1,0)="Sample#"
      htable(2,0)="Result"
      htable(0,1)=fmtsampledate
      htable(1,1)="ID"
      htable(2,1)="value2"
    case "160"
      redim htable(8,1)
      htable(0,0)="Sample date"
      htable(1,0)="Sample#"
      htable(2,0)="5-10"
      htable(3,0)="10-15"
      htable(4,0)="15-25"
      htable(5,0)="25-50"
      htable(6,0)="50-100"
      htable(7,0)=">100"
      htable(8,0)="NAS"
      htable(0,1)=fmtsampledate
      htable(1,1)="ID"
      htable(2,1)="micron_5_10"
      htable(3,1)="micron_10_15"
      htable(4,1)="micron_15_25"
      htable(5,1)="micron_25_50"
      htable(6,1)="micron_50_100"
      htable(7,1)="micron_100"
      htable(8,1)="nas_class"
    case "170","230"
      redim htable(2,1)
      htable(0,0)="Result"
      htable(1,0)="Sample date"
      htable(2,0)="Sample#"
      htable(0,1)="value1"
      htable(1,1)=fmtsampledate
      htable(2,1)="ID"
    case "180"
      redim htable(6,1)
      htable(0,0)="Sample date"
      htable(1,0)="Sample#"
      htable(2,0)="Major"
      htable(3,0)="Minor"
      htable(4,0)="Trace"
      htable(5,0)="Other"
      htable(6,0)="Final Weight"
      htable(0,1)=fmtsampledate
      htable(1,1)="ID"
      htable(2,1)="major"
      htable(3,1)="minor"
      htable(4,1)="trace"
      htable(5,1)="narrative"
      htable(6,1)="value2"
    case "220"
      redim htable(2,1)
      htable(0,0)="Pass/Fail"
      htable(1,0)="Sample date"
      htable(2,0)="Sample#"
      htable(0,1)="value1"
      htable(1,1)=fmtsampledate
      htable(2,1)="ID"
    case "250"
      redim htable(3,1)
      htable(0,0)="Pass/Fail"
      htable(1,0)="Scratches"
      htable(2,0)="Sample date"
      htable(3,0)="Sample#"
      htable(0,1)="id2"
      htable(1,1)="value2"
      htable(2,1)=fmtsampledate
      htable(3,1)="id"
    case "270"
      redim htable(24,1)
      htable(0,0)="Sample date"
      htable(1,0)="Sample#"
      htable(2,0)="Plastic Flow"
      htable(3,0)="su max"
      htable(4,0)="su work"
      htable(5,0)="su flow"
      htable(6,0)="str% 10s"
      htable(7,0)="str% max"
      htable(8,0)="str% min"
      htable(9,0)="str% rcvry"
      htable(10,0)="G` 30"
      htable(11,0)="G` 100"
      htable(12,0)="Tswp init"
      htable(13,0)="Tswpfinal"
      htable(14,0)="G` 20a"
      htable(15,0)="G` 85"
      htable(16,0)="G` 20b"
      htable(17,0)="G` 0.1 r/s"
      htable(18,0)="G` 1 r/s"
      htable(19,0)="G` 10 r/s"
      htable(20,0)="G` 100 r/s"
      htable(21,0)="1/T&delta; 0.1 r/s"
      htable(22,0)="1/T&delta; 100 r/s"
      htable(23,0)="eta* 0.1 r/s"
      htable(24,0)="eta` 0.1 r/s"

      htable(0,1)=fmtsampledate
      htable(1,1)="sampleid"
      htable(2,1)="Yield stress"
      htable(3,1)="su max"
      htable(4,1)="su work"
      htable(5,1)="su flow"
      htable(6,1)="str% 10s"
      htable(7,1)="str% max"
      htable(8,1)="str% min"
      htable(9,1)="str% rcvry"
      htable(10,1)="G` 30"
      htable(11,1)="G` 100"
      htable(12,1)="Tswp init"
      htable(13,1)="Tswpfinal"
      htable(14,1)="G` 20a"
      htable(15,1)="G` 85"
      htable(16,1)="G` 20b"
      htable(17,1)="G` 0.1 r/s"
      htable(18,1)="G` 1 r/s"
      htable(19,1)="G` 10 r/s"
      htable(20,1)="G` 100 r/s"
      htable(21,1)="1/Td 0.1 r/s"
      htable(22,1)="1/Td 100 r/s"
      htable(23,1)="eta* 0.1 r/s"
      htable(24,1)="eta' 0.1 r/s"
    case "280"
			'doesn't use comments
      redim htable(5,1)
      htable(0,0)="Sample date"
      htable(1,0)="Sample#"
      htable(2,0)="Resistivity"
      htable(3,0)="Chlorides"
      htable(4,0)="Amine"
      htable(5,0)="Phenol"
      htable(0,1)=fmtsampledate
      htable(1,1)="id"
      htable(2,1)="resistivity"
      htable(3,1)="chlorides"
      htable(4,1)="amine"
      htable(5,1)="phenol"
    case else
  end select

  strNEWTest=CheckNew(strCurrentSample)

  if trim(strNEWTest)<>"" then
	lubept=strNEWTest
  else
    lubept=tag & " " & strCompName & "(" & comp & ") " & strLocName & "(" & loc & ")"
  end if

  resizeButton = ""
  if (isLab) then
    resizeButton = resizeButton & "<td class='blank'><input type=button value='Resize' name=close onclick='resizeHistory()'></td>"
    resizeButton = resizeButton & "<td class='blank'><input type=button value='Open' name=open onclick='openHistory()'></td>"
  end if
  if (old) then
    resizeButton = resizeButton & "<td class='blank'><input type=button value='Newer History' name=newer onclick='newerHistory()'></td>"
  else
    resizeButton = resizeButton & "<td class='blank'><input type=button value='Older History' name=older onclick='olderHistory()'></td>"
  end if
  if rs.EOF then
	Response.Write "<table class='blank'><tr><td class='blank'><h4>No test history for " & lubept & "</h4></td>" & resizeButton & "</tr></table>"
    ShowImages testID, tag, comp, loc
  else
    Response.Write "<table class='blank'><tr><td class='blank'><h4>"
    if ((not old) and (testID="120" or testID="180" or testID="210" or testID="240")) then
        Response.Write "History for " & lubept &"</h4></td>" & resizeButton & "</tr></table>"
    else
        Response.Write "Last " & NumberOfRecords(rs) & " results for " & lubept &"</h4></td>" & resizeButton & "</tr></table>"
    end if

    ShowImages testID, tag, comp, loc

	dim strSubDir

    if ((not old) and (testID="120" or testID="180" or testID="210" or testID="240")) then
      Response.Write(ParticleTypes(rs, testID, resultsUrl))
    else
    Response.Write "<table>"
    Response.Write "<tr>"
    for col=lbound(htable,1) to ubound(htable,1)
      Response.Write "<th>" & htable(col,0) & "</th>"
    next
    Response.Write "</tr>"
    do
      Response.Write "<tr>"
      for col=lbound(htable,1) to ubound(htable,1)
        if left(htable(col,1),1)="=" then
          on error resume next
          Err.Clear
          Response.Write "<td>&nbsp;" & Eval(mid(htable(col,1),2)) & "</td>"
          if Err.number>0 then
            Response.Write "<td>&nbsp;</td>"
          end if
          on error goto 0
        else
			    select case testID
			      case "70"
			    	  strSubDir=""
			    	  if len(rs.Fields(htable(1,1)))>0 and col=1 then
			    	  	strSubDir="SPA\SPA_" & cstr(fix(rs.Fields(htable(1,1))/1000)) & "k\"
			    	  	Response.Write "<td>&nbsp;<a HREF=" & chr(34) & Application("FTIRPath") & "\" & strSubDir & rs.Fields(htable(1,1)) & ".SPA" & chr(34) & ">" & rs.Fields(htable(col,1)) & "</a></td>"
			    	  else
			    	  	Response.Write "<td>&nbsp;" & rs.Fields(htable(col,1)) & "</td>"
			    	  end if
	          case "170", "230"
			    	  strSubDir=""
			    	  if len(rs.Fields(htable(2,1)))>0 and col=2 then
	  	          strSubDir="RBOT\"
	  	          Response.Write "<td>&nbsp;<a HREF=" & chr(34) & Application("LabPath") & "\" & strSubDir & rs.Fields(htable(2,1)) & ".DAT" & chr(34) & ">" & rs.Fields(htable(col,1)) & "</a></td>"
			    	  else
			    	  	Response.Write "<td>&nbsp;" & rs.Fields(htable(col,1)) & "</td>"
			    	  end if
			      case else
			    	  Response.Write "<td>&nbsp;" & rs.Fields(htable(col,1)) & "</td>"
			    end select
        end if
      next
      Response.Write "</tr>"
      rs.MoveNext
    loop until rs.EOF
    end if
    Response.Write "</table>"
  end if
end sub

Sub ShowImages(testID, tag, comp, loc)
    if testID="120" or testID="180" or testID="210" or testID="240" then
      Response.Write("<div style='width:100%;overflow-x:scroll;white-space: nowrap;'><iframe src='" & Application("URL") & "lab/results/imageList.asp?tag=" & tag & "&comp=" & comp & "&loc=" & loc & "&tid=" & testID & "' width='100%' height=150 frameborder=0></iframe></div>")
    end if
end Sub

function ExtractFilename(strFullPath)
	do until instr(strFullPath,"\")=0
		strFullPath=mid(strFullPath,instr(strFullPath,"\")+1)
	loop
	ExtractFilename=strFullPath
end function

function CheckNew(strSampleIDNeeded)
dim strNEWSQL, rsNew, dbNEW
	CheckNew=""
	if trim(strSampleIDNeeded)<>"" then
		set dbNEW=OpenConnection(Application("dbLUBELAB_ConnectionString"))
		strNEWSQL="select newUsedFlag, lubetype from usedlubesamples where id=" & strSampleIDNeeded 
		set rsNew=dbNEW.Execute (strNEWSQL)
		if not rsNEW.eof then
			if cint(rsNEW(0))=1 then
				CheckNew=rsNEW(1)
			end if
		end if
		rsNEW.close
		set rsNEW=nothing
		dbNEW.close
		set dbNEW=nothing
	end if
end function

function ParticleTypes(rs, testID, resultsUrl)
dim dates, samples, detail, pso, blnHeadDone, comments, severity, extras, extras2, extras3, shortComment
    pso = ""
    blnHeadDone = false
    do while not rs.EOF
      if (pso <> rs.Fields("SortOrder")) then
        if len(pso) = 0 then
          dates = dates & "<tr><th></th>"
          samples = samples & "<tr><th></th>"
          comments = comments & "<tr><td>Comments</td>"
          severity = severity & "<tr><td>Severity</td>"
          detail = detail & "<tr>"
          select case testID 
            case "210"
              extras = extras & "<tr><td>Dilution Factor</td>"
            case "180"
              extras = extras & "<tr><td>Sample size</td>"
              extras2 = extras2 & "<tr><td>Residue weight</td>"
              extras3 = extras3 & "<tr><td>Final weight</td>"
            case "240"
              extras = extras & "<tr><td>Volume of Oil Used</td>"
            case else
              'nothing
          end select
        else
          detail = detail & "</tr><tr>"
          if not blnHeadDone then
            blnHeadDone = true
          end if
        end if
        detail = detail & "<td>" & rs.Fields("Type") & "</td>"
        pso = rs.Fields("SortOrder")
      end if
      if not blnHeadDone then
        dates = dates & "<th>" & rs.Fields("sampleDate") & "</th>"
        samples = samples & "<th><a href='" & Replace(resultsUrl,"SAMPLEID",rs.Fields("ID")) & "' target='_blank'>" & rs.Fields("ID") & "</a></th>"
        shortComment = rs.Fields("MainComments")
        if (len(shortComment) > 20) then
            shortComment = left(shortComment, 20) & "..."
        end if
        comments = comments & "<td title='" & rs.Fields("MainComments") & "'>" & shortComment & "</td>"
        severity = severity & "<td>" & rs.Fields("id1") & "</td>"
        select case testID 
          case "210"
            if (rs.Fields("id2") = "x:y") then
              extras = extras & "<td>" & rs.Fields("id3") & "</td>"
            else
              extras = extras & "<td>" & rs.Fields("id2") & "</td>"
            end if
          case "180"
              extras = extras & "<td>" & rs.Fields("value1") & "</td>"
              extras2 = extras2 & "<td>" & rs.Fields("value3") & "</td>"
              extras3 = extras3 & "<td>" & rs.Fields("value2") & "</td>"
          case "240"
            if (left(rs.Fields("id2"), 4) = "Appr") then
              extras = extras & "<td>" & rs.Fields("id3") & "</td>"
            else
              extras = extras & "<td>" & rs.Fields("id2") & "</td>"
            end if
          case else
            'nothing
        end select
      end if
      detail = detail & "<td>" & rs.Fields("Value") & "</td>"
      rs.MoveNext
    loop
    if len(dates) > 0 then
      dates = dates & "</tr>"
      samples = samples & "</tr>"
      comments = comments & "</tr>"
    end if
    if len(detail) > 0 then
      detail = detail & "</tr>"
    end if

    ParticleTypes = "<table>" & dates & samples & extras & extras2 & extras3 & severity & comments & detail & "</table>"
end function
%>
