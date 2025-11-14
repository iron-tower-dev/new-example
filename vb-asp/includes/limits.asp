<%
'Requires page to include constants.asp
Dim lcde()
Dim flcde()
Dim llim()
Dim ulim()
redim lcde(NUMBER_OF_LIMITS)
redim flcde(NUMBER_OF_LIMITS)
redim llim(NUMBER_OF_LIMITS,2)
redim ulim(NUMBER_OF_LIMITS,2)
dim sqllim
dim RSlim
dim grouping, group_count, groupid
dim DBConnLim
Set DBConnLim = Server.CreateObject("ADODB.Connection")
DBConnLim.Open Application("dbLUBELAB_ConnectionString")


  sqllim = "SELECT ID, group_name, valueat FROM limits_xref "
  sqllim = sqllim & " WHERE tagNumber = '"
  sqllim = sqllim & fortag & "' and component = '" 
  sqllim = sqllim & forcomp & "' and location = '" 
  sqllim = sqllim & forloc & "' AND exclude IS NULL"


	Set RSlim = DBConnLim.Execute(sqllim)
	if RSlim.EOF then
		grouping = "No Group"
		group_count = 0
		groupid = 0
	else
		groupid = RSlim.Fields("valueat")
		grouping = RSlim.Fields("group_name")
		sqllim = "SELECT * FROM limits WHERE limits_xref_id = " & groupid
		sqllim = sqllim &	" AND exclude IS NULL ORDER BY limits_xref_id, testid, testname"
		set RSlim=nothing
		Set RSlim = DBConnLim.Execute(sqllim)
		if RSlim.EOF then
		   	grouping = "No Group"
   			group_count = 0
		 	groupid = 0
		else
			for i = 1 to NUMBER_OF_LIMITS
				lcde(i) = RSlim.Fields("lcde")
				llim(i,2) = limblankifnull(RSlim.Fields("llim3"))
				ulim(i,2) = limblankifnull(RSlim.Fields("ulim3"))
				llim(i,1) = limblankifnull(RSlim.Fields("llim2"))
				ulim(i,1) = limblankifnull(RSlim.Fields("ulim2"))
				llim(i,0) = limblankifnull(RSlim.Fields("llim1"))
				ulim(i,0) = limblankifnull(RSlim.Fields("ulim1"))
			    flcde(i) = RSlim.Fields("flcde")
				RSlim.MoveNext
			next
		end if
	end if

RSlim.Close
Set RSlim = Nothing
set DBConnLim = nothing

function zeroifempty(value)
  if len(trim(value)) > 0 then
    zeroifempty = value
  else
    zeroifempty = ""
  end if
end function

function limblankifnull(value)
  if len(trim(value)) > 0 then
    limblankifnull = value
  else
    limblankifnull = ""
  end if
end function

function highestlimit(index)
'find the highest applicable limit for given index
dim lim
	if len(ulim(index,2))>0 then
    lim = FormatNumber(ulim(index,2),2)
	  lim = cdbl(Replace(lim,",",""))
    if lim>0 then
	    highestlimit=lim
	    exit function
	  end if
  end if

	if len(ulim(index,1))>0 then
    lim = FormatNumber(ulim(index,1),2)
	  lim = cdbl(Replace(lim,",",""))
    if lim>0 then
	    highestlimit=lim
	    exit function
	  end if
  end if
  
	if len(ulim(index,0))>0 then
    lim = FormatNumber(ulim(index,0),2)
	  lim = cdbl(Replace(lim,",",""))
    if lim>0 then
	    highestlimit=lim
	    exit function
	  end if
  end if

  highestlimit = 0

end function

function lowestlimit(index)
'find the highest applicable limit for given index
dim lim

	if len(llim(index,2))>0 then
    lim = FormatNumber(llim(index,2),2)
	  lim = cdbl(Replace(lim,",",""))
    if lim>0 then
	    lowestlimit=lim
	    exit function
	  end if
  end if

	if len(llim(index,1))>0 then
    lim = FormatNumber(llim(index,1),2)
	  lim = cdbl(Replace(lim,",",""))
    if lim>0 then
	    lowestlimit=lim
	    exit function
	  end if
  end if
  
	if len(llim(index,0))>0 then
    lim = FormatNumber(llim(index,0),2)
	  lim = cdbl(Replace(lim,",",""))
    if lim>0 then
	    lowestlimit=lim
	    exit function
	  end if
  end if

  lowestlimit = 0

end function

function textcolor(index,disp)
'determine appropriate text color forgiven value and index
dim x
	x=cdbl(disp)
	
  'test outer limits first
	if len(llim(index,2))>0 then
  lolim = FormatNumber(llim(index,2),2)
	lolim = cdbl(Replace(lolim,",",""))
    if (lolim>0 and x<=lolim) then
	    textcolor="class=redtext"
	    exit function
	  end if
  end if

	if len(ulim(index,2))>0 then
	hilim = FormatNumber(ulim(index,2),2)
	hilim = cdbl(Replace(hilim,",",""))
	  if (hilim>0 and x>=hilim) then
	  	textcolor="class=redtext"
	  	exit function
	  end if
	end if

  'test middle limits
	if len(llim(index,1))>0 then
  lolim = FormatNumber(llim(index,1),2)
	lolim = cdbl(Replace(lolim,",",""))
    if (lolim>0 and x<=lolim) then
	    textcolor="class=orangetext"
	    exit function
	  end if
  end if

	if len(ulim(index,1))>0 then
	hilim = FormatNumber(ulim(index,1),2)
	hilim = cdbl(Replace(hilim,",",""))
	  if (hilim>0 and x>=hilim) then
	  	textcolor="class=orangetext"
	  	exit function
	  end if
	end if

  'test inner limits
	if len(llim(index,0))>0 then
  lolim = FormatNumber(llim(index,0),2)
	lolim = cdbl(Replace(lolim,",",""))
    if (lolim>0 and x<=lolim) then
	    textcolor="class=greentext"
	    exit function
	  end if
  end if

	if len(ulim(index,0))>0 then
	hilim = FormatNumber(ulim(index,0),2)
	hilim = cdbl(Replace(hilim,",",""))
	  if (hilim>0 and x>=hilim) then
	  	textcolor="class=greentext"
	  	exit function
	  end if
	end if

  textcolor=""
end function
%>
