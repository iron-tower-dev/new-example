<%
const commentLen = 1000
sub buildEntryTable(testID,rs,sampleid,rows,lubetype,qclass,displayMode,displayConcise)
dim etable(),col,row,celltext,lngPos,blnData,blnRowData,calc
dim edata(),edata2(),edata3(),temp,subrow,tables,table,elements,edata0(),header
dim lngTabCount
const ITEMS=8
const TITLE=0
const LABEL=1
const DBNAME=2
const REQUIRED=3
const DISABLED=4
const JSCRIPT=5
const ROWSPAN=6
const TEXTSIZE=7
dim intColor
dim strRowCol(1)
tables=1
lngTabCount=1
strRowCol(0)="#F8F8FF"
strRowCol(1)="#FFDEAD"
intColor=0

  qclass=ucase(qclass)
  header = false
  select case testID
    case "10"
      redim edata(4)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Sample weight||numvalue1|t|f|||8|"
      edata(2)="Final buret||numvalue3|t|f| onblur='calculateTANResult()'||8|"
      edata(3)="TAN calc||numvalue2|f|t|||8|"
      edata(4)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "20"
      redim edata(3)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Result||numvalue1|t|f|||8|"
      edata(2)="File data||cmdfile Find|f|f| onclick=" & chr(34) & "javascript:window.open('" & Application("URL") & "lab/results/getResultsFromFile.asp?tid=" & testid & "&sid=" & sampleid & "&row=#');" & chr(34) & "||8|"
      edata(3)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "30","40"
      redim edata(46)
      edata(0)="Trial||###|f|f||4|8|"
      edata(1)="|Na|@@@|||||8|"
      edata(2)="||numna|t|f|||8|"
      edata(3)="|Mo|@@@|||||8|"
      edata(4)="||nummo|t|f|||8|"
      edata(5)="|Mg|@@@|||||8|"
      edata(6)="||nummg|t|f|||8|"
      edata(7)="|P|@@@|||||8|"
      edata(8)="||nump|t|f|||8|"
      edata(9)="|B|@@@|||||8|"
      edata(10)="||numb|t|f|||8|"
      edata(11)="File data||cmdfile Find|f|f| onclick=" & chr(34) & "javascript:window.open('" & Application("URL") & "lab/results/getResultsFromFile.asp?tid=" & testid & "&sid=" & sampleid & "&row=#');" & chr(34) & "|4|8|"
      edata(12)="Select||chksave||||4|8|"
      edata(13)="||>>>|||||8|"
      edata(14)="|Cr|@@@|||||8|"
      edata(15)="||numcr|t|f|||8|"
      edata(16)="|Ca|@@@|||||8|"
      edata(17)="||numca|t|f|||8|"
      edata(18)="|Ni|@@@|||||8|"
      edata(19)="||numni|t|f|||8|"
      edata(20)="|Ag|@@@|||||8|"
      edata(21)="||numag|t|f|||8|"
      edata(22)="|Cu|@@@|||||8|"
      edata(23)="||numcu|t|f|||8|"
      edata(24)="||>>>|||||8|"
      edata(25)="|Sn|@@@|||||8|"
      edata(26)="||numsn|t|f|||8|"
      edata(27)="|Al|@@@|||||8|"
      edata(28)="||numal|t|f|||8|"
      edata(29)="|Mn|@@@|||||8|"
      edata(30)="||nummn|t|f|||8|"
      edata(31)="|Pb|@@@|||||8|"
      edata(32)="||numpb|t|f|||8|"
      edata(33)="|Fe|@@@|||||8|"
      edata(34)="||numfe|t|f|||8|"
      edata(35)="||>>>|||||8|"
      edata(36)="|Si|@@@|||||8|"
      edata(37)="||numsi|t|f|||8|"
      edata(38)="|Ba|@@@|||||8|"
      edata(39)="||numba|t|f|||8|"
      edata(40)="|Zn|@@@|||||8|"
      edata(41)="||numzn|t|f|||8|"
      edata(42)="|H|@@@|||||8|"
      edata(43)="||numh|t|f|||8|"
      if testID="30" then
        edata(44)="|Schedule Large Spec.|chkschedule|||||8|"
      else
        edata(44)="|Schedule Ferrography|chkschedule|||||8|"
      end if
      edata(45)="||>>>|||||8|"
      edata(46)="|&nbsp;|@@@|||||8|"
      focusfield="numna1"
    case "50","60"
      redim edata(6)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Thermometer MTE#|THERMOMETER|+++id3|||||8|"
      edata(2)="Stop Watch MTE#|TIMER|+++id1|||||8|"
      edata(3)="Tube ID|VISCOMETER|+++id2||| onchange='lstViscometer_onchange()'||8|"
      edata(4)="Stop watch time||numvalue1|t|f| onblur='VISTime_onblur()'||8|"
      edata(5)="cSt||numvalue3|f|t|||8|"
      edata(6)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "70"
      rows=1
      redim edata(10)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Delta area||numcontam|f|f|||8|"
      edata(2)="Anti-oxidant||numanti_oxidant|f|f|||8|"
      edata(3)="Oxidation||numoxidation|f|f|||8|"
      edata(4)="H2O||numh2o|f|f|||8|"
      edata(5)="Anti-wear||numzddp|f|f|||8|"
      edata(6)="Soot||numsoot|f|f|||8|"
      edata(7)="Dilution||numfuel_dilution|f|f|||8|"
      edata(8)="Mixture||nummixture|f|f|||8|"
      edata(9)="Weak acid||numnlgi|f|f|||8|"
      edata(10)="Select||chksave|||||8|"
      focusfield="numcontam1"
    case "80"
      redim edata(6)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Barometer MTE#|BAROMETER|+++id1|||||8|"
      edata(2)="Thermometer MTE#|THERMOMETER|+++id2||| onchange='lstThermometer_onchange()'||8|"
      edata(3)="Barometric pressure (mm Hg)||numvalue1|t|f|||8|"
      edata(4)="Flash Point temperature (F)||numvalue2|t|f| onblur='calculateFPResult()'||8|"
      edata(5)="Result||numvalue3|f|t|||8|"
      edata(6)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "110"
      redim edata(2)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Result||numvalue1|t|f|||8|"
      edata(2)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "120"
      rows=1
      if (blnOldTest) then
      redim edata(12)
      edata(0)="Trial||###|f|f||4|8|"
      edata(1)="|Major|@@@|||||8|"
      edata(2)="|insp|---major|t|f|||8|"
      edata(3)="Select||chksave||||4|8|"
      edata(4)="||>>>|||||8|"
      edata(5)="|Minor|@@@|||||8|"
      edata(6)="|insp|---minor|t|f|||8|"
      edata(7)="||>>>|||||8|"
      edata(8)="|Trace|@@@|||||8|"
      edata(9)="|insp|---trace|t|f|||8|"
      edata(10)="||>>>|||||8|"
      edata(11)="|Other|@@@|||||8|"
      edata(12)="||txtnarrative|f|f|||100|"
      focusfield="lstmajor1"
      else
        tables=2
        header = true

        redim edata2(2)
        edata2(0)="|Comments|@@@|f|f|||20|"
        edata2(1)="||txtMainComments|f|f|||170|"
        edata2(2)="Select||chksave|||||8|"

        redim edata0(5)
        edata0(0)="Overall Severity||@@@|||||8|"
        edata0(1)="1|1|radid1|t|f|||8|"
        edata0(2)="2|2|radid1|t|f|||8|"
        edata0(3)="3|3|radid1|t|f|||8|"
        edata0(4)="4|4|radid1|t|f|||8|"
        edata0(5)="|&nbsp;|@@@|||||8|"

        redim edata(127)
        lngpos = particleTypeInfo(edata)
      end if
    case "130"
      redim edata(7)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="1st penetration||numvalue1|t|f| onblur='calculateGPWResult()'||8|"
      edata(2)="2nd penetration||numvalue2|t|f| onblur='calculateGPWResult()'||8|"
      edata(3)="3rd penetration||numvalue3|t|f| onblur='calculateGPWResult()'||8|"
      edata(4)="Result||numtrialcalc|t|t|||8|"
      edata(5)="NLGI||txtid1|t|t|||8|"
      edata(6)="NLGI lookup||cmdlookup <--Get|f|f| onclick=" & chr(34) & "javascript:window.open('" & Application("URL") & "lab/results/lookupvalues.asp?lookup=NLGI&fld=txtid1#&v1='+window.frmEntry.numtrialcalc#.value,'_blank','height=1,width=1,titlebar=no,status=no,toolbar=no,menubar=no,location=no,scrollbars=no');" & chr(34) & "||8|"
      edata(7)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "140"
      redim edata(6)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Dropping Point Thermometer|THERMOMETER|+++id1|||||8|"
      edata(2)="Block Thermometer|THERMOMETER|+++id2|||||8|"
      edata(3)="Dropping Point Temperature||numvalue1|t|f| onblur='calculateGDPResult()'||8|"
      edata(4)="Block Temperature||numvalue3|t|f| onblur='calculateGDPResult()'||8|"
      edata(5)="Result||numvalue2|t|t|||8|"
      edata(6)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "160"
      rows=1
      redim edata(10)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="5-10||nummicron_5_10|t|f|||8|"
      edata(2)="10-15||nummicron_10_15|t|f|||8|"
      edata(3)="15-25||nummicron_15_25|t|f|||8|"
      edata(4)="25-50||nummicron_25_50|t|f|||8|"
      edata(5)="50-100||nummicron_50_100|t|f|||8|"
      edata(6)=">100||nummicron_100|t|f|||8|"
      edata(7)="File data||cmdfile Find|f|f| onclick=" & chr(34) & "javascript:window.open('" & Application("URL") & "lab/results/getResultsFromFile.asp?tid=" & testid & "&sid=" & sampleid & "&row=#');" & chr(34) & "||8|"
      edata(8)="NAS||txtnas_class|t|t|||8|"
      edata(9)="NAS lookup||cmdlookup <--Get|f|f| onclick=" & chr(34) & "javascript:window.open('" & Application("URL") & "lab/results/lookupvalues.asp?lookup=NAS&fld=txtnas_class#&v1='+window.frmEntry.nummicron_5_10#.value+'&v2='+window.frmEntry.nummicron_10_15#.value+'&v3='+window.frmEntry.nummicron_15_25#.value+'&v4='+window.frmEntry.nummicron_25_50#.value+'&v5='+window.frmEntry.nummicron_50_100#.value+'&v6='+window.frmEntry.nummicron_100#.value,'_blank','height=1,width=1,titlebar=no,status=no,toolbar=no,menubar=no,location=no,scrollbars=no');" & chr(34) & "||8|"
      edata(10)="Save?||chksave|||||8|"
      focusfield="nummicron_5_101"
    case "170"
      redim edata(4)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Thermometer MTE#|THERMOMETER|+++id1|||||8|"
      edata(2)="Fail time||numvalue1|t|f|||8|"
      edata(3)="File data||cmdfile Find|f|f| onclick=" & chr(34) & "javascript:window.open('" & Application("URL") & "lab/results/getResultsFromFile.asp?tid=" & testid & "&sid=" & sampleid & "&row=#');" & chr(34) & "||8|"
      edata(4)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "180"
      rows=1
      if (blnOldTest) then
        redim edata(24)
        edata(0)="Trial||###|f|f||4|8|"
        edata(1)="|Major|@@@|||||8|"
        edata(2)="|filt|---major|t|f|||8|"
        edata(3)="Sample size||numvalue1|t|f| onblur='calculateFRResult()'||8|"
        edata(4)="Residue weight||numvalue3|t|f| onblur='calculateFRResult()'||8|"
        edata(5)="Final weight||numvalue2|f|t|||8|"
        edata(6)="Select||chksave||||4|8|"
        edata(7)="||>>>|||||8|"
        edata(8)="|Minor|@@@|||||8|"
        edata(9)="|filt|---minor|t|f|||8|"
        edata(10)="||___|||||8|"
        edata(11)="||___|||||8|"
        edata(12)="||___|||||8|"
        edata(13)="||>>>|||||8|"
        edata(14)="|Trace|@@@|||||8|"
        edata(15)="|filt|---trace|t|f|||8|"
        edata(16)="||___|||||8|"
        edata(17)="||___|||||8|"
        edata(18)="||___|||||8|"
        edata(19)="||>>>|||||8|"
        edata(20)="|Other|@@@|||||8|"
        edata(21)="||txtnarrative|f|f|||80|"
        edata(22)="||___|||||8|"
        edata(23)="||___|||||8|"
        edata(24)="||___|||||8|"
        focusfield="numvalue11"
      else
        tables=3
        header = true

        redim edata0(5)
        edata0(0)="Overall Severity||@@@|||||8|"
        edata0(1)="1|1|radid1|t|f|||8|"
        edata0(2)="2|2|radid1|t|f|||8|"
        edata0(3)="3|3|radid1|t|f|||8|"
        edata0(4)="4|4|radid1|t|f|||8|"
        edata0(5)="|&nbsp;|@@@|||||8|"

        redim edata3(2)
        edata3(0)="|Comments|@@@|f|f|||20|"
        edata3(1)="||txtMainComments|f|f|||170|"
        edata3(2)="Select||chksave|||||8|"

        redim edata2(3)
        edata2(0)="Sample size||numvalue1|t|f| onblur='calculateFRResult()'||8|"
        edata2(1)="Residue weight||numvalue3|t|f| onblur='calculateFRResult()'||8|"
        edata2(2)="Final weight||numvalue2|f|t|||8|"
        edata2(3)="|&nbsp;|@@@|||||8|"

        redim edata(127)
        lngpos = particleTypeInfo(edata)
      end if
    case "210"
      rows=1
      tables=3

      redim edata3(2)
      edata3(0)="|Comments|@@@|f|f|||20|"
      edata3(1)="||txtMainComments|f|f|||170|"
      edata3(2)="Select||chksave|||||8|"

      redim edata2(5)
      edata2(0)="|Dilution Factor|@@@|||||8|"
      edata2(1)="3:2|3:2|radid2|t|f|||8|"
      edata2(2)="1:10|1:10|radid2|t|f|||8|"
      edata2(3)="1:100|1:100|radid2|t|f|||8|"
      edata2(4)="X/YYYY|x:y|radid2|t|f|||8|"
      edata2(5)="||txtid3|f|f|||10|"

      if (blnOldTest) then
        redim edata(383)
        edata(0)="Particles|(&lt;15&micro;m) Normal Rubbing|@@@|||||8|"
        lngpos=ferrogramBlock(edata,1,"normalrubbing",true)
        edata(lngpos)="|Severe Sliding|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"severewearsliding",true)
        edata(lngpos)="|(&gt;15&micro;m) Severe Fatigue|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"severewearfatigue",true)
        edata(lngpos)="|Cutting|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"cuttingpart",true)
        edata(lngpos)="|(&gt;15&micro;m) Laminar|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"laminarpart",true)
        edata(lngpos)="|Spheres|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"spheres",true)
        edata(lngpos)="|Dark Metallo-Oxide|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"darkmetalloode",true)
        edata(lngpos)="|Red Oxide|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"redode",true)
        edata(lngpos)="|Corrosive Wear|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"corrosivewearpart",true)
        edata(lngpos)="|Non-Ferrous Metal|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"nonferrousmetal",true)
        edata(lngpos)="|Non-Metallic Inorganic|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"nonmetallicinorganic",true)
        edata(lngpos)="|Birefringent Organic|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"birefringentorganic",true)
        edata(lngpos)="|Non-Metallic Amorphous|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"nonmetallicamporphous",true)
        edata(lngpos)="|Friction Polymers|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"frictionpolymers",true)
        edata(lngpos)="|Fibers|@@@|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"fibers",true)
        edata(lngpos)="|Other:|txtothertext|||||8|"
        lngpos=lngpos+1
        lngpos=ferrogramBlock(edata,lngpos,"other",true)
      else
        header = true

        redim edata0(5)
        edata0(0)="Overall Severity||@@@|||||8|"
        edata0(1)="1|1|radid1|t|f|||8|"
        edata0(2)="2|2|radid1|t|f|||8|"
        edata0(3)="3|3|radid1|t|f|||8|"
        edata0(4)="4|4|radid1|t|f|||8|"
        edata0(5)="|&nbsp;|@@@|||||8|"

        redim edata(127)
        lngpos = particleTypeInfo(edata)
      end if

    case "220"
      redim edata(6)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Thermometer MTE#|THERMOMETER|+++id1|||||8|"
      edata(2)="Pass|1|radvalue1|t|f|||8|"
      edata(3)="Fail - Light|2|radvalue1|t|f|||8|"
      edata(4)="Fail - Moderate|3|radvalue1|t|f|||8|"
      edata(5)="Fail - Severe|4|radvalue1|t|f|||8|"
      edata(6)="Select||chksave|||||8|"
      focusfield="lstid11"
    case "230"
      redim edata(3)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Thermometer MTE#|THERMOMETER|+++id1|||||8|"
      edata(2)="Fail time||numvalue1|t|f|||8|"
      edata(3)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "240"
      rows=1
      if (blnOldTest) then
        redim edata(12)
        edata(0)="Trial||###|f|f||4|8|"
        edata(1)="|Major|@@@|||||8|"
        edata(2)="|insp|---major|t|f|||8|"
        edata(3)="Select||chksave||||4|8|"
        edata(4)="||>>>|||||8|"
        edata(5)="|Minor|@@@|||||8|"
        edata(6)="|insp|---minor|t|f|||8|"
        edata(7)="||>>>|||||8|"
        edata(8)="|Trace|@@@|||||8|"
        edata(9)="|insp|---trace|t|f|||8|"
        edata(10)="||>>>|||||8|"
        edata(11)="|Other|@@@|||||8|"
        edata(12)="||txtnarrative|f|f|||100|"
        focusfield="lstmajor1"
      else
        tables=3
        header = true

        redim edata0(5)
        edata0(0)="Overall Severity||@@@|||||8|"
        edata0(1)="1|1|radid1|t|f|||8|"
        edata0(2)="2|2|radid1|t|f|||8|"
        edata0(3)="3|3|radid1|t|f|||8|"
        edata0(4)="4|4|radid1|t|f|||8|"
        edata0(5)="|&nbsp;|@@@|||||8|"

        redim edata3(2)
        edata3(0)="|Comments|@@@|f|f|||20|"
        edata3(1)="||txtMainComments|f|f|||170|"
        edata3(2)="Select||chksave|||||8|"

        redim edata2(8)
        edata2(0)="|Volume of Oil Used|@@@|||||8|"
        edata2(1)="~500ml|~500ml|radid2|t|f|||8|"
        edata2(2)="~250ml|~250ml|radid2|t|f|||8|"
        edata2(3)="~ 50ml|~ 50ml|radid2|t|f|||8|"
        edata2(4)="~25ml|~25ml|radid2|t|f|||8|"
        edata2(5)="Appr.  X ml|Appr.  X ml|radid2|t|f|||8|"
        edata2(6)="||txtid3|f|f|||10|"
        edata2(7)="|&nbsp;|@@@|||||8|"

        redim edata(126)
        lngpos = particleTypeInfo(edata)
      end if
    case "250"
      redim edata(5)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Deleterious MTE#|DELETERIOUS|+++id1|||||8|"
      edata(2)="Pressure||numvalue1|t|f|||8|"
      edata(3)="Scratches||numvalue2|t|f|||8|"
      edata(4)="Pass/Fail|PASSFAIL|===id2|||||8|"
      edata(5)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "270"
      rows=1
      redim edata(1)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Select||chksave|||||8|"

    case "284"
      redim edata(2)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="D-inch||numvalue1|t|f|||8|"
      edata(2)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "285"
      redim edata(2)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Oil Content||numvalue1|t|f|||8|"
      edata(2)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case "286"
      redim edata(2)
      edata(0)="Trial||###|f|f|||8|"
      edata(1)="Varnish Potential Rating||numvalue1|t|f|||8|"
      edata(2)="Select||chksave|||||8|"
      focusfield="numvalue11"
    case else
  end select

dim start
if (header) then
    start = 0
else
    start = 1
end if
for table = start to tables

  select case table
    case 0
      redim etable(ubound(edata0),ITEMS) 'items = 8
      for row=0 to ubound(edata0)
        temp=split(edata0(row),"|")
        for col=0 to ubound(temp)
          etable(row,col)=temp(col)
        next
      next
    case 1
      if not (rs.BOF and rs.EOF) then
        rs.MoveFirst
      end if
      redim etable(ubound(edata),ITEMS) 'items = 8
      for row=0 to ubound(edata)
        temp=split(edata(row),"|")
        for col=0 to ubound(temp)
          etable(row,col)=temp(col)
        next
      next
    case 2
      if not (rs.BOF and rs.EOF) then
        rs.MoveFirst
      end if
      redim etable(ubound(edata2),ITEMS)
      for row=0 to ubound(edata2)
        temp=split(edata2(row),"|")
        for col=0 to ubound(temp)
          etable(row,col)=temp(col)
        next
      next
    case 3
      if not (rs.BOF and rs.EOF) then
        rs.MoveFirst
      end if
      redim etable(ubound(edata3),ITEMS)
      for row=0 to ubound(edata3)
        temp=split(edata3(row),"|")
        for col=0 to ubound(temp)
          etable(row,col)=temp(col)
        next
      next
  end select
  Response.Write "<table>"
  Response.Write "<tr>"
  for col=lbound(etable,1) to ubound(etable,1)
    if etable(col,DBNAME) = ">>>" then
      exit for
    end if
    Response.Write "<th width='" & (100/ubound(etable,1)) & "%'>" & etable(col,TITLE) & "</th>"
  next
  Response.Write "</tr>"

  if IsOpen(rs) then
    blnData=not(rs.EOF)
  else
    blnData=false
  end if
  for row=1 to rows
    Response.Write "<tr>"
    blnRowData=false
    if blnData then
      if rs.Fields("trial")=row then
        blnRowData=true
      end if
    end if
    subrow=0
    for col=lbound(etable,1) to ubound(etable,1)
      select case left(etable(col,DBNAME),3)
        case "___"  'dummy cell
          celltext="<td>&nbsp;</td>"
        
        case "###"  'insert loop counter
          celltext="<td"
          if len(etable(col,ROWSPAN))>0 then
            celltext=celltext & " rowspan="& etable(col,ROWSPAN)
          end if
          celltext=celltext & ">" & row & "</td>"
          
        case ">>>"  'end row and start a new one
          celltext="</tr><tr>"
          subrow=subrow+1
          
        case "pti"  'particle type information sub table
          celltext="</tr><tr id='pti-" & etable(col,LABEL) & "'><td colspan=" & ITEMS & ">" & ptiTable2(etable(col,LABEL),rs) & "</td></tr><tr>"
          subrow=subrow+1
          if (Not rs.EOF) then
            rs.MoveNext
          elseif (Not rs.BOF) then
            rs.MoveLast
          end if

        case "pte"  'particle type evaluation cell
          dim pte
          pte = getField(rs,"severity")
          if (len(pte) = 0) then
            pte = "N/A"
          end if
          celltext="<td><div id='txteval" & etable(col,LABEL) & "'>" & pte & "</div></td>"
          
        case "@@@"  'non-entry label field
          if ((strTestID = "120" or strTestID = "180" or strTestID = "210" or strTestID = "240") and etable(col,LABEL) = "Comments" ) then
            celltext="<td>Comments from this characterization <br>"
            celltext=celltext & "<input type=button name='cmdComments' id='cmdComments' value='Add to main comment ->' onclick='addToMainComment()' />"
            celltext=celltext & "<div id='fullComments'></div>"
            celltext=celltext & "</td>"
          else
            celltext="<td>" & etable(col,LABEL) & "</td>"
          end if
          
        case "+++"  'M&TE equipment list
          celltext="<td>" & MTEList(etable(col,LABEL),mid(etable(col,DBNAME),4),testID,row,lubetype,etable(col,JSCRIPT),getField(rs,mid(etable(col,DBNAME),4))) & "</td>"

        case "---"  'comment listbox
          celltext="<td>" & CommentList(etable(col,LABEL),mid(etable(col,DBNAME),4),testID,row,etable(col,JSCRIPT),getField(rs,mid(etable(col,DBNAME),4))) & "</td>"

        case "==="  'standard listbox
          celltext="<td>" & ListBox(etable(col,LABEL),mid(etable(col,DBNAME),4),row,etable(col,JSCRIPT),getField(rs,mid(etable(col,DBNAME),4))) & "</td>"

        case "rad"  'radio button
          elements = Split(etable(col,DBNAME),"-")
          dim instance
          instance = ""
          if (UBound(elements) > 0) then
            instance = elements(1)
          end if
          if (blnOldTest) then
            temp=cstr(getField(rs,mid(etable(col,DBNAME),4)))
          else
            temp=cstr(getField(rs,mid(elements(0),4)))
          end if
          if len(temp)=0 then
            temp="0"
          end if

          if temp = cstr(etable(col,LABEL)) then
            if (blnOldTest) then
              intColor=1 - intColor
		      if testID="210" and isnumeric(etable(col,LABEL)) then
		      	celltext="<td style='BACKGROUND-COLOR:" & strRowCol(intColor) & "' title='" & (etable(col,LABEL)/10) & "%'><input type=radio name='" & etable(col,DBNAME) & row & "' value='" & etable(col,LABEL) & "' CHECKED>"
		      else
		      	celltext="<td><input type=radio name='" & etable(col,DBNAME) & row & "' value='" & etable(col,LABEL) & "' CHECKED>"
		      end if
            else
 			   if testID="120" or testID="180" or testID="210" or testID="240" then
                 if (UBound(elements) > 0) then
 		      	   celltext="<td><input type=radio name='"  & elements(0) & instance & "' value='" & etable(col,LABEL) & "' CHECKED onclick='toggleshow(" & elements(1) & ", false)'/></td>"
                 else
 		      	   celltext="<td><input type=radio name='"  & elements(0) & instance & "' value='" & etable(col,LABEL) & "' CHECKED /></td>"
                 end if
		       else
			     celltext="<td><input type=radio name='" & elements(0) & row & "' value='" & etable(col,LABEL) & "' CHECKED /></td>"
			   end if
            end if
          else
            if (blnOldTest) then
		      if testID="210" and isnumeric(etable(col,LABEL)) then
		      	celltext="<td style='BACKGROUND-COLOR:" & strRowCol(intColor) & "' title='" & (etable(col,LABEL)/10) & "%'><input type=radio name='" & etable(col,DBNAME) & row & "' value='" & etable(col,LABEL) & "'>"
		      else
		      	celltext="<td><input type=radio name='" & etable(col,DBNAME) & row & "' value='" & etable(col,LABEL) & "'>"
		      end if
            else
			   if testID="120" or testID="180" or testID="210" or testID="240" then
                 if (UBound(elements) > 0) then
			       celltext="<td><input type=radio name='" & elements(0) & instance & "' value='" & etable(col,LABEL) & "' onclick='toggleshow(" & elements(1) & ", false)' /></td>"
                 else
			       celltext="<td><input type=radio name='" & elements(0) & instance & "' value='" & etable(col,LABEL) & "' /></td>"
                 end if
			   else
			     celltext="<td><input type=radio name='" & elements(0) & row & "' value='" & etable(col,LABEL) & "' /></td>"
			   end if
            end if
          end if

        case "ptr"  'radio button modified
          elements = Split(etable(col,DBNAME),"-")
          temp=cstr(getField(rs,mid(elements(0),4)))
          if len(temp)=0 then
            temp="0"
          end if
          if temp = cstr(etable(col,LABEL)) then
            intColor=1 - intColor
		      	celltext="<td><input type=radio name='" & replace(elements(0),"ptr","rad") & elements(1) & "' value='" & etable(col,LABEL) & "' CHECKED onclick='toggleshow(" & elements(1) & ", true)'/>"
                celltext=celltext & "<input type=button name='cmdEval" & elements(1) & "' id='cmdEval" & elements(1) & "' value='Show' onclick='togglepti(" & elements(1) & ")'"
            else
		      	celltext="<td><input type=radio name='" & replace(elements(0),"ptr","rad") & elements(1) & "' value='" & etable(col,LABEL) & "' onclick='toggleshow(" & elements(1) & ", true)'/>"
                celltext=celltext & "<input type=button name='cmdEval" & elements(1) & "' id='cmdEval" & elements(1) & "' value='Show' onclick='togglepti(" & elements(1) & ")' disabled='disabled'"
          end if
          temp=etable(col,JSCRIPT)
          lngPos=instr(temp,"#")
          do while lngPos>0
            temp=left(temp,lngPos-1) & row & mid(temp,lngPos+1)
            lngPos=instr(temp,"#")
          loop
          celltext=celltext & temp & "/></td>"

        case "cmd"
          celltext="<td"
          if len(etable(col,ROWSPAN))>0 then
            celltext=celltext & " rowspan="& etable(col,ROWSPAN)
          end if
          celltext=celltext & "><input type=button name=" & left(etable(col,DBNAME),instr(etable(col,DBNAME)," ")-1) & row
          celltext=celltext & " value='" & mid(etable(col,DBNAME),instr(etable(col,DBNAME)," ")+1) & "'"
          if etable(col,DISABLED)="t" then
            celltext=celltext&" disabled=true"
          end if
          temp=etable(col,JSCRIPT)
          lngPos=instr(temp,"#")
          do while lngPos>0
            temp=left(temp,lngPos-1) & row & mid(temp,lngPos+1)
            lngPos=instr(temp,"#")
          loop
          celltext=celltext & temp & "></td>"

        case "chk"  'check box
          celltext="<td"
          if len(etable(col,ROWSPAN))>0 then
            celltext=celltext & " rowspan="& etable(col,ROWSPAN)
          end if
          select case etable(col,DBNAME)
            case "chksave"
				if Request.QueryString("mode")="save" and Request.Form("hidpartial")<>"y" then
					celltext=celltext & ">&nbsp;</td>"
				else
					if row=1 or ((testID="50" or testID="60") and row=2 and (qclass="Q" or qclass="QAG")) then
					  celltext=celltext & "><input type=checkbox name=chksave" & row & " checked></td>"
					else
					  celltext=celltext & "><input type=checkbox name=chksave" & row & "></td>"
					end if
				end if
            case "chkschedule"
              if row=1 then
                celltext=celltext & "><input type=checkbox name=chkschedule" & row & ">" & etable(col,LABEL) & "</td>"
              end if
            case else
              celltext=celltext & "><input type=checkbox name=" & etable(col,DBNAME) & row & "></td>"
          end select
        case else    
          celltext="<td"
          if len(etable(col,ROWSPAN))>0 then
            celltext=celltext & " rowspan="& etable(col,ROWSPAN)
          end if
          lngPos=instr(etable(col,DBNAME),"=")
          if lngPos>0 then
            calc=mid(etable(col,DBNAME),lngPos+1)
            etable(col,DBNAME)=left(etable(col,DBNAME),lngPos-1)
          else
            calc=""
          end if
 		  if (strTestID = "120" or strTestID = "180" or strTestID = "210" or strTestID = "240") and etable(col,TEXTSIZE)> 79 and etable(col,DBNAME)= "txtMainComments" then
			celltext=celltext & " style='width: 80%'>Counter (Limit: " & commentLen & ")<br><input type=text size=8 name=" & etable(col,DBNAME) & "cntr value=" & chr(34) & chr(34) & " readonly><br>"
          else
            celltext=celltext & ">"
		  end if
          if len(etable(col,LABEL))>0 then
            celltext=celltext & etable(col,LABEL)
          end if
          if etable(col,TEXTSIZE)> 79 then          
			celltext=celltext & "<textarea cols=" & etable(col,TEXTSIZE)/2 & " name=" & etable(col,DBNAME) & row & " rows=4"		
			if (strTestID = "120" or strTestID = "180" or strTestID = "210" or strTestID = "240") then
			  celltext=celltext & " onkeyup=" & chr(34) & "validateFerrogramComments(" & chr(39) & commentLen & chr(39) & ")" & chr(34)
			else
			  celltext=celltext & chr(34)
			end if
          else
			celltext=celltext & "<input type=text  size=" & etable(col,TEXTSIZE) & " name=" & etable(col,DBNAME) & row
			if strTestID="30" or strTestID="40" then
			  celltext=celltext & " tabIndex=" & lngTabCount
			  lngTabCount=lngTabCount+1
			end if				      
          end if
          
          if etable(col,DISABLED)="t" then
            celltext=celltext&" disabled=true"
          end if
          if blnRowData then
            if len(calc)>0 then
              on error resume next
              celltext=celltext&" value='" & Eval(calc) & "'"
              on error goto 0
            else
		      if etable(col,TEXTSIZE)>79 then
				celltext=celltext&">" & getField(rs,mid(etable(col,DBNAME),4))
				else
				celltext=celltext&" value='" & getField(rs,mid(etable(col,DBNAME),4)) & "'"
			  end if
            end if
		  else
			if etable(col,TEXTSIZE)>79 then
			  celltext=celltext&">"
			end if
          end if
  	      if etable(col,TEXTSIZE)>79 then
			celltext=celltext & "</TEXTAREA></td>"
		  else
			celltext=celltext & etable(col,JSCRIPT) & "></td>"
		  end if
          if etable(col,REQUIRED)="t" then
            celltext=celltext&"<input type=hidden name=req" & etable(col,DBNAME) & row & ">"
          end if
      end select

      Response.Write celltext
    next
    Response.Write "</tr>"
    if blnRowData then
      if (Not rs.EOF) then
        rs.MoveNext
      end if
      blnData=not(rs.EOF)
    end if
  next
  Response.Write "</table>"
next
end sub

function ParticleTypeCategories()
    dim conn,rs,sql
    sql="SELECT c.ID categoryID, c.Description category, COUNT(s.Value) subtypes FROM ParticleSubTypeDefinition s INNER JOIN ParticleSubTypeCategoryDefinition c ON s.ParticleSubTypeCategoryID = c.ID WHERE s.Active = 1 AND c.Active = 1 GROUP BY c.Description, c.ID, c.SortOrder ORDER BY c.SortOrder"
    on error resume next
    set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
    set rs=DisconnectedRS(sql,conn)
    CloseDBObject(conn)
    set conn=nothing
    Set ParticleTypeCategories = rs
end function

function ParticleSubTypes()
    dim conn,rs,sql
    sql="SELECT c.ID categoryID, c.Description category, s.Value, s.Description FROM ParticleSubTypeDefinition s INNER JOIN ParticleSubTypeCategoryDefinition c ON s.ParticleSubTypeCategoryID = c.ID WHERE s.Active = 1 AND c.Active = 1 ORDER BY c.SortOrder, s.SortOrder"
    on error resume next
    set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
    set rs=DisconnectedRS(sql,conn)
    CloseDBObject(conn)
    set conn=nothing
    Set ParticleSubTypes = rs
end function

function ptiTable2(index,rs)
    dim rsCat, rsSub, header, rows, maxrows, s, t, counter, i, js, results(9)
    Set rsCat = ParticleTypeCategories()

    rs.Find("ParticleTypeDefinitionID=" & index)
    results(0) = getField(rs,"Heat")
    results(1) = getField(rs,"Concentration")
    results(2) = getField(rs,"Size, Ave")
    results(3) = getField(rs,"Size, Max")
    results(4) = getField(rs,"Color")
    results(5) = getField(rs,"Texture")
    results(6) = getField(rs,"Composition")
    results(7) = getField(rs,"Severity")
    results(8) = getField(rs,"Comments")

    header = "<tr>"
    Do While Not rsCat.EOF
      header = header & "<th colspan=3>" & getField(rsCat,"category") & "</th>"
      if getField(rsCat,"subtypes") > maxrows then
        maxrows = getField(rsCat,"subtypes")
      end if
      rsCat.MoveNext
    Loop
    header = header & "<th>Comments</th></tr>"

    redim detail(rsCat.RecordCount, maxrows, 3)
    rows = ""
    s = 0
    Set rsSub = ParticleSubTypes()
    rsCat.MoveFirst
    Do While Not rsCat.EOF
      detail(s,0,0) = "rad" & getField(rsCat,"categoryID")
      ApplyFilter rsSub, "categoryID='" & getField(rsCat,"categoryID") & "'"
      t = 0
      Do While Not rsSub.EOF
        detail(s,t,0) = getField(rsSub,"Description")
        detail(s,t,1) = getField(rsSub,"Value")
        detail(s,t,2) = getField(rsSub,"category")
        detail(s,t,3) = getField(rsSub,"categoryID")
        rsSub.MoveNext
        t = t + 1
      Loop
      ClearFilter rsSub
      rsCat.MoveNext
      s = s + 1
    Loop

    CloseDBObject(rsCat)
    set rsCat=nothing
    CloseDBObject(rsSub)
    set rsSub=nothing

    for counter = 0 to (maxrows - 1)
      rows = rows & "<tr>"
      for i = 0 to ((ubound(detail,1)) - 1)
        if i = ((ubound(detail,1)) - 1) then
          js = "onclick='setSeverity(" & index & ", " & (counter + 1)  & ");'"
        else
          js = ""
        end if
        if (len(detail(i,counter,0)) > 0) then
          rows = rows & "<td colspan='2' style='text-align:right;'><label>" & detail(i,counter,0) & "<input type=radio " & js & " name='rad" & detail(i,counter,2) & index & "' value='" & detail(i,counter,1) & "'" & IIf(results(i)=detail(i,counter,1), " CHECKED", "") & " /></label></td><td></td>"
        else
          rows = rows & "<td></td><td></td><td></td>"
        end if
      next
      if (counter = 0) then
        rows = rows & "<td rowspan=" & maxrows & "><textarea cols=25 rows=4 name='comment" & index & "' onblur='addAllComments()'>" & results(8) & "</textarea></td>"
      end if
      rows = rows & "</tr>"
    next

    ptiTable2 = "<table>" & header & rows & "</table>"
end function

function getField(rs,field)
  Err.Clear
  on error resume next
  if isnull(rs.Fields(field)) then
    getField=""
  else
    getField=rs.Fields(field)
  end if
  if Err.number=3265 then
    getField=""
  end if
end function

Function IIf(Expression, TruePart, FalsePart)
	If Expression = True Then
		If IsObject(TruePart) Then
			Set IIf = TruePart
		Else
			IIf = TruePart
		End If
	Else
		If IsObject(FalsePart) Then
			Set IIf = FalsePart
		Else
			IIf = FalsePart
		End If
	End If
End Function

function MTEList(eqtype,dbname,tid,row,lubetype,jscript,dbvalue)
'build MTE list box
dim html,conn,rs,sql,reqdsize,suffix,tubesize,mtedate

  if len(jscript)>0 then
    html="<select name=lst" & dbname & row & jscript & ">"
  else
    html="<select name=lst" & dbname & row & " onchange='listbox_onchange(" & chr(34) & dbname & row & chr(34) & ")'>"
  end if
  html=html&"<option value=''>"

  select case eqtype
    case "THERMOMETER"
      sql="select equipname,duedate from M_And_T_Equip "
      sql=sql&"where EquipType='" & eqtype & "' "
      sql=sql&"and (exclude is null or exclude <>1) "
      if tid="230" then
        sql=sql&"and testid=170"
      else
        sql=sql&"and testid=" & tid
      end if
    case "TIMER","BAROMETER","DELETERIOUS"
      sql="select equipname,duedate from M_And_T_Equip "
      sql=sql&"where EquipType='" & eqtype & "' "
      sql=sql&"and (exclude is null or exclude <>1) "
    case "VISCOMETER"
      if tid="50" then
        sql="SELECT tube_size_vis40 as size FROM lubricant WHERE type='" & lubetype & "'"
      else
        sql="SELECT tube_size_vis100 as size FROM lubricant WHERE type='" & lubetype & "'"
      end if
      on error resume next
      set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
      if IsOpen(conn) then
        set rs=ForwardOnlyRS(sql,conn)
        if IsOpen(rs) then
          if not rs.EOF then
            reqdsize=getField(rs,"size")
          end if
        end if
      end if
      CloseDBObject(conn)
      set conn=nothing
      CloseDBObject(rs)
      set rs=nothing
      sql="select equipname,duedate,val1,val2 from M_And_T_Equip "
      sql=sql&"where EquipType='" & eqtype & "' "
      sql=sql&"and (exclude is null or exclude <>1) "
      sql=sql&"and testid=" & tid
      sql=sql&" order by val2 desc"
  end select

  on error resume next
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  if IsOpen(conn) then
    set rs=DisconnectedRS(sql,conn)
    CloseDBObject(conn)
    set conn=nothing
    if IsOpen(rs) then
      do until rs.EOF
        if eqtype="VISCOMETER" then
          if getField(rs,"equipname")=dbvalue then
            html=html&"<option value='" & rs.Fields("equipname") & "|" & rs.Fields("val1") & "' selected>"
            dbValue=rs.Fields("equipname") & "|" & rs.Fields("val1") 
          else
            html=html&"<option value='" & rs.Fields("equipname") & "|" & rs.Fields("val1") & "'>"
          end if
          suffix=""
          if len(reqdsize)=0 or isnull(rs.Fields("val2")) then
            tubesize=0
            suffix=""
          else
            tubesize=rs.Fields("val2")
            if clng(tubesize) > clng(reqdsize) then
              suffix="**"
            else
              if clng(tubesize) = clng(reqdsize) then
                suffix="*"
              end if
            end if
          end if
          html=html&rs.Fields("equipname") & suffix
        else
          if getField(rs,"equipname")=dbvalue then
            html=html&"<option value='" & rs.Fields("equipname") & "' selected>"
          else
            html=html&"<option value='" & rs.Fields("equipname") & "'>"
          end if
          if isnull(rs.Fields("duedate")) then
            mtedate="(no date)"
          else
            mtedate=rs.Fields("duedate")
            if datediff("d",now(),mtedate)<29 then
              mtedate=mtedate & "*"
            end if
            mtedate="(" & mtedate & ")"
          end if
          html=html&rs.Fields("equipname") & mtedate
        end if
        rs.MoveNext
      loop
    end if
  end if
  CloseDBObject(rs)
  set rs=nothing
  html=html&"</select>"
  html=html&"<input type=hidden name=txt" & dbname & row & " value='" & dbvalue & "'>"
  MTEList=html
end function

function CommentList(ctype,dbname,tid,row,jscript,dbvalue)
'build comment list box
dim html,conn,rs,sql,dbID

  if len(jscript)>0 then
    html="<select name=lst" & dbname & row & jscript & ">"
  else
    html="<select name=lst" & dbname & row & " onchange='listbox_onchange(" & chr(34) & dbname & row & chr(34) & ")'>"
  end if
  html=html&"<option value=''>"

  sql="SELECT ID,remark FROM comments where area='" & ctype & "'"
  on error resume next
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  if IsOpen(conn) then
    set rs=DisconnectedRS(sql,conn)
    CloseDBObject(conn)
    set conn=nothing
    if IsOpen(rs) then
      do until rs.EOF
        if getField(rs,"remark")=dbvalue then
          dbID=rs.Fields("ID")
          html=html&"<option value='" & rs.Fields("ID") & "' selected>" & rs.Fields("remark")
        else
          html=html&"<option value='" & rs.Fields("ID") & "'>" & rs.Fields("remark")
        end if
        rs.MoveNext
      loop
    end if
  end if
  CloseDBObject(rs)
  set rs=nothing
  html=html&"</select>"
  html=html&"<input type=hidden name=txt" & dbname & row & " value='" & dbID & "'>"

  CommentList=html
end function

function ListBox(lbtype,dbname,row,jscript,dbvalue)
'build standard list box
dim html,conn,rs,sql

  if len(jscript)>0 then
    html="<select name=lst" & dbname & row & jscript & ">"
  else
    html="<select name=lst" & dbname & row & " onchange='listbox_onchange(" & chr(34) & dbname & row & chr(34) & ")'>"
  end if

  select case ucase(lbtype)
    case "PASSFAIL"
      if len(dbvalue)=0 then
        html=html&"<option value='' selected>"
      else
        html=html&"<option value=''>"
      end if
      if dbvalue="pass" then
        html=html&"<option value='pass' selected>pass"
      else
        html=html&"<option value='pass'>pass"
      end if
      if dbvalue="fail" then
        html=html&"<option value='fail' selected>fail"
      else
        html=html&"<option value='fail'>fail"
      end if
  end select

  html=html&"</select>"
  html=html&"<input type=hidden name=txt" & dbname & row & " value='" & dbvalue & "'>"

  ListBox=html
end function

function SQLforTestID(sampleid,testid,oldTest) 
dim sql
  select case testid
    case "30","40"
      sql="SELECT trialnum as trial,* FROM emspectro "
      sql=sql&"WHERE testid='" & testid & "' AND id=" & sampleid
      sql=sql&" ORDER BY trialnum ASC"
    case "70"
      sql="SELECT t.trialNumber AS trial, f.* FROM FTIR f RIGHT OUTER JOIN "
      sql=sql&"TestReadings t ON f.sampleID = t.sampleID "
      sql=sql&"WHERE t.testID='" & testid & "' AND t.sampleID=" & sampleid
      sql=sql&" ORDER BY t.trialNumber ASC"
    case "120"
      if (oldTest) then
        sql="SELECT t.trialNumber AS trial, t.value1, t.value2, t.value3, t.ID1, t.ID2, t.ID3, i.narrative, c1.remark AS major, c2.remark AS minor, c3.remark AS trace "
        sql=sql&"FROM Comments c3 RIGHT OUTER JOIN "
        sql=sql&"InspectFilter i ON c3.ID = i.trace LEFT OUTER JOIN "
        sql=sql&"Comments c2 ON i.minor = c2.ID LEFT OUTER JOIN "
        sql=sql&"Comments c1 ON i.major = c1.ID RIGHT OUTER JOIN "
        sql=sql&"TestReadings t ON i.ID = t.sampleID AND i.testID = t.testID "
        sql=sql&"WHERE t.testid='" & testid & "' AND t.sampleid=" & sampleid
        sql=sql&" ORDER BY trialnumber ASC"
      else
        sql="SELECT * FROM ("
        sql=sql&"SELECT q.trial, q.ID1, q.MainComments, q.SampleID, q.testID, q.ParticleTypeDefinitionID, q.Status, q.Comments, q.Description, s.Value FROM "
        sql=sql&"(SELECT 1 AS trial, t.ID1, t.MainComments, p.SampleID, p.testID, p.ParticleTypeDefinitionID, p.Status, p.Comments, d.Description, d.SortOrder, d.ID ParticleSubTypeCategoryID "
        sql=sql&"FROM TestReadings t RIGHT OUTER JOIN ParticleType p ON p.SampleID = t.sampleID AND t.testID = p.testID, ParticleSubTypeCategoryDefinition d "
        sql=sql&"WHERE t.testID = '" & testid & "' AND t.sampleID=" & sampleid
        sql=sql&") q LEFT OUTER JOIN ParticleSubType s ON q.SampleID = s.SampleID AND q.testID = s.testID AND q.ParticleTypeDefinitionID = s.ParticleTypeDefinitionID AND q.ParticleSubTypeCategoryID = s.ParticleSubTypeCategoryID) src "
        sql=sql&"pivot(sum(src.Value) for src.Description in (" & ControlValue("PSTCats1") & ControlValue("PSTCats2") & ")"
        sql=sql&") piv ORDER BY ParticleTypeDefinitionID ASC"
      end if
    case "160"
      sql="SELECT 1 AS trial,micron_5_10,micron_10_15,micron_15_25,micron_25_50,micron_50_100,micron_100,iso_code,nas_class FROM ParticleCount LEFT OUTER JOIN "
      sql=sql&"TestReadings ON ParticleCount.ID = TestReadings.sampleID "
      sql=sql&"WHERE testID='" & testid & "' AND TestReadings.sampleID=" & sampleid
    case "180"
      if (oldTest) then
        sql="SELECT t.trialNumber AS trial, t.value1, t.value2, t.value3, t.ID1, t.ID2, t.ID3, i.narrative, c1.remark AS major, c2.remark AS minor, c3.remark AS trace "
        sql=sql&"FROM Comments c3 RIGHT OUTER JOIN "
        sql=sql&"InspectFilter i ON c3.ID = i.trace LEFT OUTER JOIN "
        sql=sql&"Comments c2 ON i.minor = c2.ID LEFT OUTER JOIN "
        sql=sql&"Comments c1 ON i.major = c1.ID RIGHT OUTER JOIN "
        sql=sql&"TestReadings t ON i.ID = t.sampleID AND i.testID = t.testID "
        sql=sql&"WHERE t.testid='" & testid & "' AND t.sampleid=" & sampleid
        sql=sql&" ORDER BY trialnumber ASC"
      else
        sql="SELECT * FROM ("
        sql=sql&"SELECT q.trial, q.value1, q.value2, q.value3, q.ID1, q.ID2, q.ID3, q.MainComments, q.SampleID, q.testID, q.ParticleTypeDefinitionID, q.Status, q.Comments, q.Description, s.Value FROM "
        sql=sql&"(SELECT 1 AS trial, t.value1, t.value2, t.value3, t.ID1, t.ID2, t.ID3, t.MainComments, p.SampleID, p.testID, p.ParticleTypeDefinitionID, p.Status, p.Comments, d.Description, d.SortOrder, d.ID ParticleSubTypeCategoryID "
        sql=sql&"FROM TestReadings t RIGHT OUTER JOIN ParticleType p ON p.SampleID = t.sampleID AND t.testID = p.testID, ParticleSubTypeCategoryDefinition d "
        sql=sql&"WHERE t.testID = '" & testid & "' AND t.sampleID=" & sampleid
        sql=sql&") q LEFT OUTER JOIN ParticleSubType s ON q.SampleID = s.SampleID AND q.testID = s.testID AND q.ParticleTypeDefinitionID = s.ParticleTypeDefinitionID AND q.ParticleSubTypeCategoryID = s.ParticleSubTypeCategoryID) src "
        sql=sql&"pivot(sum(src.Value) for src.Description in (" & ControlValue("PSTCats1") & ControlValue("PSTCats2") & ")"
        sql=sql&") piv ORDER BY ParticleTypeDefinitionID ASC"
      end if
    case "210"
      if (oldTest) then
        sql="SELECT 1 AS trial, Ferrogram.*, Ferrogram.Comments MainComments FROM Ferrogram LEFT OUTER JOIN TestReadings ON Ferrogram.SampleID = TestReadings.sampleID "
        sql=sql&"WHERE TestReadings.testID = '" & testid & "' AND TestReadings.sampleID=" & sampleid
      else
        sql="SELECT * FROM ("
        sql=sql&"SELECT q.trial, q.ID1, q.ID2, q.MainComments, q.SampleID, q.testID, q.ParticleTypeDefinitionID, q.Status, q.Comments, q.Description, s.Value FROM "
        sql=sql&"(SELECT 1 AS trial, t.id1, t.id2, t.MainComments, p.SampleID, p.testID, p.ParticleTypeDefinitionID, p.Status, p.Comments, d.Description, d.SortOrder, d.ID ParticleSubTypeCategoryID "
        sql=sql&"FROM TestReadings t RIGHT OUTER JOIN ParticleType p ON p.SampleID = t.sampleID AND t.testID = p.testID, ParticleSubTypeCategoryDefinition d "
        sql=sql&"WHERE t.testID = '" & testid & "' AND t.sampleID=" & sampleid
        sql=sql&") q LEFT OUTER JOIN ParticleSubType s ON q.SampleID = s.SampleID AND q.testID = s.testID AND q.ParticleTypeDefinitionID = s.ParticleTypeDefinitionID AND q.ParticleSubTypeCategoryID = s.ParticleSubTypeCategoryID) src "
        sql=sql&"pivot(sum(src.Value) for src.Description in (" & ControlValue("PSTCats1") & ControlValue("PSTCats2") & ")"
        sql=sql&") piv ORDER BY ParticleTypeDefinitionID ASC"
      end if
    case "240"
      if (oldTest) then
        sql="SELECT t.trialNumber AS trial, t.value1, t.value2, t.value3, t.ID1, t.ID2, t.ID3, i.narrative, c1.remark AS major, c2.remark AS minor, c3.remark AS trace "
        sql=sql&"FROM Comments c3 RIGHT OUTER JOIN "
        sql=sql&"InspectFilter i ON c3.ID = i.trace LEFT OUTER JOIN "
        sql=sql&"Comments c2 ON i.minor = c2.ID LEFT OUTER JOIN "
        sql=sql&"Comments c1 ON i.major = c1.ID RIGHT OUTER JOIN "
        sql=sql&"TestReadings t ON i.ID = t.sampleID AND i.testID = t.testID "
        sql=sql&"WHERE t.testid='" & testid & "' AND t.sampleid=" & sampleid
        sql=sql&" ORDER BY trialnumber ASC"
      else
        sql="SELECT * FROM ("
        sql=sql&"SELECT q.trial, q.value1, q.value2, q.ID1, q.ID2, q.MainComments, q.SampleID, q.testID, q.ParticleTypeDefinitionID, q.Status, q.Comments, q.Description, s.Value FROM "
        sql=sql&"(SELECT 1 AS trial, t.value1, t.value2, t.ID1, t.id2, t.MainComments, p.SampleID, p.testID, p.ParticleTypeDefinitionID, p.Status, p.Comments, d.Description, d.SortOrder, d.ID ParticleSubTypeCategoryID "
        sql=sql&"FROM TestReadings t RIGHT OUTER JOIN ParticleType p ON p.SampleID = t.sampleID AND t.testID = p.testID, ParticleSubTypeCategoryDefinition d "
        sql=sql&"WHERE t.testID = '" & testid & "' AND t.sampleID=" & sampleid
        sql=sql&") q LEFT OUTER JOIN ParticleSubType s ON q.SampleID = s.SampleID AND q.testID = s.testID AND q.ParticleTypeDefinitionID = s.ParticleTypeDefinitionID AND q.ParticleSubTypeCategoryID = s.ParticleSubTypeCategoryID) src "
        sql=sql&"pivot(sum(src.Value) for src.Description in (" & ControlValue("PSTCats1") & ControlValue("PSTCats2") & ")"
        sql=sql&") piv ORDER BY ParticleTypeDefinitionID ASC"
      end if
    case else
      sql="SELECT trialnumber as trial,value1,value2,value3,trialcalc,id1,id2,id3 FROM testreadings "
      sql=sql&"WHERE testid='" & testid & "' AND sampleid=" & sampleid
      sql=sql&" ORDER BY trialnumber ASC"
  end select
  SQLforTestID=sql
end function

function OldDataExists(sampleid,testid)
'For the tests that have been redesigned, check if the current sample was entered using the previous data format
dim conn,rs,sql
    OldDataExists = false
    select case testid
      case "210"
        sql="SELECT SampleID FROM Ferrogram WHERE SampleID=" & sampleid
      case "120", "180", "240"
        sql="SELECT ID SampleID FROM InspectFilter WHERE testid=" & testid & " AND ID=" & sampleid
      case else
    end select
    if (len(sql) > 0) then
      on error resume next
      set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
      set rs=DisconnectedRS(sql,conn)
      CloseDBObject(conn)
      set conn=nothing
      OldDataExists = (rs.RecordCount > 0)
      CloseDBObject(rs)
      set rs=nothing
    end if
end function

function ferrogramBlock(edata(),start,dbname,newline)
dim ipos
 
  ipos=start
  edata(ipos)="0%|0|rad" & dbname & "|t|f|||8|"
  edata(ipos+1)=".3%|3|rad" & dbname & "|t|f|||8|"
  edata(ipos+2)=".7%|7|rad" & dbname & "|t|f|||8|"
  edata(ipos+3)="1%|10|rad" & dbname & "|t|f|||8|"
  edata(ipos+4)="||@@@|||||8|"
  edata(ipos+5)="||@@@|||||8|"
  edata(ipos+6)="2%|20|rad" & dbname & "|t|f|||8|"
  edata(ipos+7)="3%|30|rad" & dbname & "|t|f|||8|"
  edata(ipos+8)="4%|40|rad" & dbname & "|t|f|||8|"
  edata(ipos+9)="5%|50|rad" & dbname & "|t|f|||8|"
  edata(ipos+10)="||@@@|||||8|"
  edata(ipos+11)="||@@@|||||8|"
  edata(ipos+12)="10%|100|rad" & dbname & "|t|f|||8|"
  edata(ipos+13)="13%|130|rad" & dbname & "|t|f|||8|"
  edata(ipos+14)="17%|170|rad" & dbname & "|t|f|||8|"
  edata(ipos+15)="20%|200|rad" & dbname & "|t|f|||8|"
  edata(ipos+16)="||@@@|||||8|"
  edata(ipos+17)="||@@@|||||8|"
  edata(ipos+18)="35%|350|rad" & dbname & "|t|f|||8|"
  edata(ipos+19)="50%|500|rad" & dbname & "|t|f|||8|"
  edata(ipos+20)="75%|750|rad" & dbname & "|t|f|||8|"
  edata(ipos+21)="100%|1000|rad" & dbname & "|t|f|||8|"
  ipos=ipos+22
  if newline=true then
    edata(ipos)="||>>>|||||8|"
    ipos=ipos+1
  end if
  ferrogramBlock=ipos
end function

function particleTypeInfo(edata())
dim conn,rs,sql,ipos

  sql="SELECT ID, Type, Description, Image1, Image2 FROM vwParticleTypeDefinition ORDER BY SortOrder"
  on error resume next
  set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
  if IsOpen(conn) then
    set rs=DisconnectedRS(sql,conn)
    CloseDBObject(conn)
    set conn=nothing
    if IsOpen(rs) then
    ipos=0
      redim edata(143)
    subRows=0
      do until rs.EOF

      edata(ipos)="N/A|0|radstatus-" & rs.Fields("ID") & "|t|f|||8|"
      edata(ipos+1)="Review|1|ptrstatus-" & rs.Fields("ID") & "|t|f|||8|"
      edata(ipos+2)="Particle Type|" & rs.Fields("Type") & "|@@@|||||8|"
      edata(ipos+3)="Description|" & rs.Fields("Description") & "|@@@|||||8|"
      edata(ipos+4)="Image 1|<a href=""imagePreview.asp?name=Ferrogram&type=" & rs.Fields("Type") & "&img=" & rs.Fields("Image1") & """ target=_blank title=""" & rs.Fields("Type") & """ ><img src=""http://" & Request.ServerVariables("SERVER_NAME") & "/images/pti/" & rs.Fields("Image1") & """ height=100 /></a>|@@@|||||8|"
      edata(ipos+5)="Image 2|<a href=""imagePreview.asp?name=Ferrogram&type=" & rs.Fields("Type") & "&img=" & rs.Fields("Image2") & """ target=_blank title=""" & rs.Fields("Type") & """ ><img src=""http://" & Request.ServerVariables("SERVER_NAME") & "/images/pti/" & rs.Fields("Image2") & """ height=100 /></a>|@@@|||||8|"
      edata(ipos+6)="Evaluated Severity|" & rs.Fields("ID") & "|pte|||||8|"
      edata(ipos+7)="||>>>|||||8|"
      edata(ipos+8)=rs.Fields("Type") & "|" & rs.Fields("ID") & "|pti|||||8|"
      ipos=ipos+9

        subRows=subRows + 1
        rs.MoveNext
      loop
    end if
  end if
  CloseDBObject(rs)
  set rs=nothing

end function
%>

