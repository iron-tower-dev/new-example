<%
dim rheo(22,3), rheoCount

'DB field name
rheo(0,0) = "YieldStress"
rheo(1,0) = "Td_-1"
rheo(2,0) = "Td_2"
rheo(3,0) = "eta*_-1"
rheo(4,0) = "eta_-1"
rheo(5,0) = "g10_-1"
rheo(6,0) = "g10_0"
rheo(7,0) = "g10_1"
rheo(8,0) = "g10_2"
rheo(9,0) = "tswpinit"
rheo(10,0) = "tswpfinal"
rheo(11,0) = "g30"
rheo(12,0) = "g100"
rheo(13,0) = "e10"
rheo(14,0) = "emax"
rheo(15,0) = "emin"
rheo(16,0) = "erecovery"
rheo(17,0) = "sumax"
rheo(18,0) = "suwork"
rheo(19,0) = "suflow"
rheo(20,0) = "g20a"
rheo(21,0) = "g85"
rheo(22,0) = "g20b"

'display name
rheo(0,1) = "Plastic Flow"
rheo(1,1) = "1/T&delta; 0.1 r/s"
rheo(2,1) = "1/T&delta; 100 r/s"
rheo(3,1) = "eta* 0.1 r/s"
rheo(4,1) = "eta' 0.1 r/s"
rheo(5,1) = "G' 0.1 r/s"
rheo(6,1) = "G' 1 r/s"
rheo(7,1) = "G' 10 r/s"
rheo(8,1) = "G' 100 r/s"
rheo(9,1) = "tswp init"
rheo(10,1) = "tswp final"
rheo(11,1) = "G' 30"
rheo(12,1) = "G' 100"
rheo(13,1) = "str% 10s"
rheo(14,1) = "str% max"
rheo(15,1) = "str% min"
rheo(16,1) = "str% rcvry"
rheo(17,1) = "su max"
rheo(18,1) = "su rmp"
rheo(19,1) = "su flow"
rheo(20,1) = "G' 20a"
rheo(21,1) = "G' 85"
rheo(22,1) = "G' 20b"

'test type
rheo(0,2) = 5
rheo(1,2) = 2
rheo(2,2) = 2
rheo(3,2) = 2
rheo(4,2) = 2
rheo(5,2) = 2
rheo(6,2) = 2
rheo(7,2) = 2
rheo(8,2) = 2
rheo(9,2) = 6
rheo(10,2) = 6
rheo(11,2) = 1
rheo(12,2) = 1
rheo(13,2) = 3
rheo(14,2) = 3
rheo(15,2) = 3
rheo(16,2) = 3
rheo(17,2) = 4
rheo(18,2) = 4
rheo(19,2) = 4
rheo(20,2) = 7
rheo(21,2) = 7
rheo(22,2) = 7

'DB result field name
rheo(0,3) = "Calc1"
rheo(1,3) = "Calc5"
rheo(2,3) = "Calc6"
rheo(3,3) = "Calc7"
rheo(4,3) = "Calc8"
rheo(5,3) = "Calc1"
rheo(6,3) = "Calc2"
rheo(7,3) = "Calc3"
rheo(8,3) = "Calc4"
rheo(9,3) = "Calc1"
rheo(10,3) = "Calc2"
rheo(11,3) = "Calc1"
rheo(12,3) = "Calc2"
rheo(13,3) = "Calc1"
rheo(14,3) = "Calc2"
rheo(15,3) = "Calc3"
rheo(16,3) = "Calc4"
rheo(17,3) = "Calc1"
rheo(18,3) = "Calc2"
rheo(19,3) = "Calc3"
rheo(20,3) = "Calc1"
rheo(21,3) = "Calc2"
rheo(22,3) = "Calc3"

function RheoDBFieldName(index)
  if index <= ubound(rheo,1) and index >= 0 then
    RheoDBFieldName = rheo(index,0)
  else
    RheoDBFieldName = ""
  end if
end function

function RheoDisplayName(index)
  if index <= ubound(rheo,1) and index >= 0 then
    RheoDisplayName = rheo(index,1)
  else
    RheoDisplayName = ""
  end if
end function

function RheoTestType(index)
  if index <= ubound(rheo,1) and index >= 0 then
    RheoTestType = rheo(index,2)
  else
    RheoTestType = 0
  end if
end function

function RheoDBResultName(index)
  if index <= ubound(rheo,1) and index >= 0 then
    RheoDBResultName = rheo(index,3)
  else
    RheoDBResultName = ""
  end if
end function

function RheoTestTypeFromName(name)
  RheoTestTypeFromName=0
  for rheoCount = 0 to ubound(rheo,1)
    if rheo(rheoCount,0) = name then
      RheoTestTypeFromName = rheo(rheoCount,2)
    end if
  next
end function

function RheoResultNameFromName(name)
  RheoResultNameFromName=0
  for rheoCount = 0 to ubound(rheo,1)
    if rheo(rheoCount,0) = name then
      RheoResultNameFromName = rheo(rheoCount,3)
    end if
  next
end function
%>