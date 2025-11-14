<%
dim strTableString
Function GetLEDetails(strSampleID, blnWrite)
dim strLEStatusSQL, rsTests
dim dbLEStatus, strLECriteria, blnDone, blnLEFail
dim strTestName, rsLEResult, strLEResult
dim vntFTIR(), lngSubTestLoop, strLEColor
dim vntParticleCount()
dim vntRheometer()
	redim vntFTIR(1,8)
	vntFTIR(0,0)="anti_oxidant"
	vntFTIR(1,0)="AntiOxy"
	vntFTIR(0,1)="oxidation"
	vntFTIR(1,1)="Oxidation"
	vntFTIR(0,2)="H2O"
	vntFTIR(1,2)="H2O"
	vntFTIR(0,3)="zddp"
	vntFTIR(1,3)="Anti-wear"
	vntFTIR(0,4)="soot"
	vntFTIR(1,4)="Soot"
	vntFTIR(0,5)="fuel_dilution"
	vntFTIR(1,5)="Dilution"
	vntFTIR(0,6)="mixture"
	vntFTIR(1,6)="Mixture"
	vntFTIR(0,7)="NLGI"
	vntFTIR(1,7)="Weak Acid"
	vntFTIR(0,8)="contam"
	vntFTIR(1,8)="Delta Area"
	redim vntParticleCount(1,5)
	vntParticleCount(0,0)="micron_5_10"
	vntParticleCount(1,0)="m_5_10"
	vntParticleCount(0,1)="micron_10_15"
	vntParticleCount(1,1)="m_10_15"
	vntParticleCount(0,2)="micron_15_25"
	vntParticleCount(1,2)="m_15_25"
	vntParticleCount(0,3)="micron_25_50"
	vntParticleCount(1,3)="m_25_50"
	vntParticleCount(0,4)="micron_50_100"
	vntParticleCount(1,4)="m_50_100"
	vntParticleCount(0,5)="micron_100"
	vntParticleCount(1,5)="m_>100"
	redim vntRheometer(2,22)
	vntRheometer(0,0)="5"
	vntRheometer(1,0)="Calc1"
	vntRheometer(2,0)="Plastic Flow"
	vntRheometer(0,1)="2"
	vntRheometer(1,1)="Calc5"
	vntRheometer(2,1)="1/T&delta; 0.1 r/s"
	vntRheometer(0,2)="2"
	vntRheometer(1,2)="Calc6"
	vntRheometer(2,2)="1/T&delta; 100 r/s"
	vntRheometer(0,3)="2"
	vntRheometer(1,3)="Calc7"
	vntRheometer(2,3)="eta* 0.1 r/s"
	vntRheometer(0,4)="2"
	vntRheometer(1,4)="Calc8"
	vntRheometer(2,4)="eta` 0.1 r/s"
	vntRheometer(0,5)="2"
	vntRheometer(1,5)="Calc1"
	vntRheometer(2,5)="G` 0.1 r/s"
	vntRheometer(0,6)="2"
	vntRheometer(1,6)="Calc2"
	vntRheometer(2,6)="G` 1 r/s"
	vntRheometer(0,7)="2"
	vntRheometer(1,7)="Calc3"
	vntRheometer(2,7)="G` 10 r/s"
	vntRheometer(0,8)="2"
	vntRheometer(1,8)="Calc4"
	vntRheometer(2,8)="G` 100 r/s"
	vntRheometer(0,9)="6"
	vntRheometer(1,9)="Calc1"
	vntRheometer(2,9)="Tswp init"
	vntRheometer(0,10)="6"
	vntRheometer(1,10)="Calc2"
	vntRheometer(2,10)="Tswp final"
	vntRheometer(0,11)="1"
	vntRheometer(1,11)="Calc1"
	vntRheometer(2,11)="G` 30"
	vntRheometer(0,12)="1"
	vntRheometer(1,12)="Calc2"
	vntRheometer(2,12)="G` 100"
	vntRheometer(0,13)="3"
	vntRheometer(1,13)="Calc1"
	vntRheometer(2,13)="str% 10s"
	vntRheometer(0,14)="3"
	vntRheometer(1,14)="Calc2"
	vntRheometer(2,14)="str% max"
	vntRheometer(0,15)="3"
	vntRheometer(1,15)="Calc3"
	vntRheometer(2,15)="str% min"
	vntRheometer(0,16)="3"
	vntRheometer(1,16)="Calc4"
	vntRheometer(2,16)="str% rcvry"
	vntRheometer(0,17)="4"
	vntRheometer(1,17)="Calc1"
	vntRheometer(2,17)="su max"
	vntRheometer(0,18)="4"
	vntRheometer(1,18)="Calc2"
	vntRheometer(2,18)="su rmp"
	vntRheometer(0,19)="4"
	vntRheometer(1,19)="Calc3"
	vntRheometer(2,19)="su flow"
	vntRheometer(0,20)="7"
	vntRheometer(1,20)="Calc1"
	vntRheometer(2,20)="G` 20a"
	vntRheometer(0,21)="7"
	vntRheometer(1,21)="Calc2"
	vntRheometer(2,21)="G` 85"
	vntRheometer(0,22)="7"
	vntRheometer(1,22)="Calc3"
	vntRheometer(2,22)="G` 20b"
	blnLEFail=false
	strTableString=""
	set dbLEStatus=server.CreateObject("ADODB.Connection")
	dbLEStatus.Open Application("dbLUBELAB_ConnectionString")
  dbLEStatus.CommandTimeout=600
	if trim(strSampleID)<>"" then
		strLEStatusSQL="SELECT * from vwLELimitsForSampleTests WHERE ID=" & strSampleID
		set rsTests=dbLEStatus.Execute (strLEStatusSQL)
		if not rsTests.eof then
			strTableString= "<table cols=3 width='100%'>"
			strTableString=strTableString & "<TH width='30%'>Test Name</TH>"
			strTableString=strTableString & "<TH width='30%'>Test Result</TH>"
			strTableString=strTableString & "<TH width='40%'>Test Criteria</TH>"
			do until rsTests.eof
				strLECriteria="": blnDone=false
				if not isnull(rsTests("LowerLimit")) then
					blnDone=true
					strLECriteria=rsTests("LowerLimit") & " < Val "
				end if
				if not isnull(rsTests("UpperLimit")) then
					if cstr(rsTests("TestID"))="220" then
						select case	cstr(rsTests("UpperLimit"))
							case "1"
								strLECriteria= "Pass"
							case "2"
								strLECriteria= "Fail - Light"
							case "3"
								strLECriteria= "Fail - Moderate"
							case "4"
								strLECriteria= "Fail - Severe"
						end select
					else
						if not blnDone then
							strLECriteria=strLECriteria & "Val "
						end if	
						strLECriteria=strLECriteria & "< " & rsTests("UpperLimit")
					end if
				end if
				' get the results for the test
				select case cstr(rsTests("TestID"))
					case "30","40"
						strTestName=rsTests("testname")
						strLEStatusSQL="SELECT DISTINCT " & strTestName & " FROM vwSpectroscopy WHERE SampleID=" & strSampleID & " AND TestID=" & cstr(rsTests("TestID"))
					case "70"
						strTestName= trim(mid(rsTests("testname"),2))
						for lngSubTestLoop=0 to ubound(vntFTIR,2)
							if vntFTIR(1,lngSubTestLoop)=strTestName then
								strLEStatusSQL="SELECT DISTINCT " & vntFTIR(0,lngSubTestLoop) & " FROM vwFTIR WHERE SampleID=" & strSampleID
								exit for
							end if
						next
					case "270"
						strTestName= trim(mid(rsTests("testname"),2))
						for lngSubTestLoop=0 to ubound(vntRheometer,2)
							if strTestName="Goodness" then
								strLEStatusSQL="SELECT DISTINCT Value1 FROM TestReadings WHERE SampleID=" & strSampleID & " AND TestID=270"   '05/04/06 efs 300"
								exit for
							elseif vntRheometer(2,lngSubTestLoop)=strTestName then
								strLEStatusSQL="SELECT DISTINCT " & vntRheometer(1,lngSubTestLoop) & " FROM vwRheometer WHERE SampleID=" & strSampleID 
								strLEStatusSQL=strLEStatusSQL & " AND TestType=" & vntRheometer(0,lngSubTestLoop)
								exit for
							end if
						next
					case "160"
						strTestName= trim(mid(rsTests("testname"),2))
						for lngSubTestLoop=0 to ubound(vntParticleCount,2)
							if vntParticleCount(1,lngSubTestLoop)=strTestName then
								strLEStatusSQL="SELECT DISTINCT " & vntParticleCount(0,lngSubTestLoop) & " FROM vwParticleCount WHERE SampleID=" & strSampleID
								exit for
							end if
						next
					case else
						strLEStatusSQL="SELECT Result from vwResultsBySample WHERE sampleID=" & strSampleID & " AND TestID=" & rsTests("TestID")
						strTestName= rsTests("testname")
				end select
				strLEResult="NULL"
				if trim(strLEStatusSQL)<>"" then
					set rsLEResult=dbLEStatus.Execute (strLEStatusSQL)
					if not rsLEResult.EOF then
						if isnull(rsLEResult(0)) then
							strLEResult="NULL"
						else
							strLEResult=rsLEResult(0)
						end if
					end if
					rsLEResult.close
					set rsLEResult=nothing
				end if
				' now we have all of the details, lets look at getting the right color!
				strLEColor="black"
				' first, see if there is a problem with the LE's
				if trim(rsTests("lcde") & "")="N" then
					strLEColor="purple"
				else
					if strLEResult="NULL" then
						strLEColor="green"
					else
						' see if it has an lower limit!
						if not isnull(rsTests("LowerLimit")) then
							if clng(round(strLEResult,3))<clng(rsTests("LowerLimit")) then
								strLEColor="red"
								blnLEFail=true
							end if
						end if
						' see if it has an upper limit!
						if not isnull(rsTests("UpperLimit")) then
						  if cstr(rsTests("TestID"))="220" then
						  Response.Write cdbl(round(strLEResult,3)) & "<br>"
						  Response.Write cdbl(rsTests("UpperLimit")) & "<br>"
							  if cdbl(round(strLEResult,3))<>cdbl(rsTests("UpperLimit")) then
							  	strLEColor="red"
							  	blnLEFail=true
							  end if
						  else
							  if cdbl(round(strLEResult,3))>cdbl(rsTests("UpperLimit")) then
							  	strLEColor="red"
							  	blnLEFail=true
							  end if
							end if
						end if
					end if
				end if
				strTableString=strTableString & "<TR>"
				strTableString=strTableString & "<TD align=center><FONT COLOR=" & strLEColor & ">" & rsTests("TestAbbrev")
				if trim(strTestName & "")<>"" then
				  select case lcase(strTestName)
				    case "nlgi"
					    strTableString=strTableString &  "-Weak Acid"
				    case "zddp"
					    strTableString=strTableString &  "-Anti-wear"
				    case "contam"
					    strTableString=strTableString &  "-Delta Area"
					case "fueldilute"
					    strTableString=strTableString &  "- Dilution"
				    case else
					    strTableString=strTableString &  "-" & strTestName
					end select
				end if
				strTableString=strTableString & "</FONT></TD>"
				strTableString=strTableString & "<TD align=center><FONT COLOR=" & strLEColor & ">" 
				if strLEResult<>"NULL" then
					if cstr(rsTests("TestID"))="220" then
						select case	strLEResult
							case "1"
								strTableString=strTableString & "Pass"
							case "2"
								strTableString=strTableString & "Fail - Light"
							case "3"
								strTableString=strTableString & "Fail - Moderate"
							case "4"
								strTableString=strTableString & "Fail - Severe"
						end select
					else
						strTableString=strTableString & strLEResult
					end if
				else
					strTableString=strTableString & "&nbsp;"
				end if
				strTableString=strTableString & "</FONT></TD>"
				strTableString=strTableString & "<TD align=center><FONT COLOR=" & strLEColor & ">" & strLECriteria & "</FONT></TD>"
				strTableString=strTableString & "</TR>" & vbcrlf
				rsTests.movenext
			loop
			strTableString=strTableString &  "</table>"
			if blnWrite then
				Response.Write strTableString
			end if
		end if
		rsTests.close
		set rsTests=nothing
	end if
	dbLEStatus.close
	set dbLEStatus=nothing
	GetLEDetails=blnLEFail
End function
%>
