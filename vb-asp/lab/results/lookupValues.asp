<%@ Language=VBScript %>
<%Option Explicit%>
<!--lookupValues.asp-->
<!--#include virtual="includes/security.asp"-->
<!--#include virtual="includes/DBFunctions.asp"-->
<%dim conn,rs,sql
dim lookup,resultfield,value1,value2,value3,value4,value5,value6,result
lookup=ucase(Request.QueryString("lookup"))
resultfield=Request.QueryString("fld")
value1=Request.QueryString("v1")
value2=Request.QueryString("v2")
value3=Request.QueryString("v3")
value4=Request.QueryString("v4")
value5=Request.QueryString("v5")
value6=Request.QueryString("v6")

select case lookup
  case "NLGI"
    sql="SELECT NLGIValue as result FROM NLGILookup WHERE lowerValue<=" & value1 & " AND upperValue>" & value1
  case "NAS"
    value1=clng(value1) + clng(value2)
    sql="SELECT TOP 1 NAS as result FROM NAS_lookup "
    sql=sql&"WHERE (ValLo <= " & value1 & ") AND (ValHi > " & value1 & ") AND (Channel = 0) OR "
    sql=sql&"(ValLo <= " & value3 & ") AND (ValHi > " & value3 & ") AND (Channel = 3) OR "
    sql=sql&"(ValLo <= " & value4 & ") AND (ValHi > " & value4 & ") AND (Channel = 4) OR "
    sql=sql&"(ValLo <= " & value5 & ") AND (ValHi > " & value5 & ") AND (Channel = 5) OR "
    sql=sql&"(ValLo <= " & value6 & ") AND (ValHi > " & value6 & ") AND (Channel = 6) "
    sql=sql&"ORDER BY NAS DESC"
end select
result=""
on error resume next
set conn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
set rs=ForwardOnlyRS(sql,conn)
if not rs.EOF then
  result=getField(rs,"result")
end if
CloseDBObject(rs)
set rs=nothing
CloseDBObject(conn)
set conn=nothing
on error goto 0
%>

<HTML>
<HEAD>
</HEAD>

<BODY>
<form name=frmresult>
<%
Response.Write "<input type=hidden name=txtresult value='" & result & "'>"
Response.Write "<input type=hidden name=txtfield value='" & resultfield & "'>"
%>
</form>
<SCRIPT LANGUAGE=javascript>
<!--
  window.opener.frmEntry.elements[window.frmresult.txtfield.value].value=window.frmresult.txtresult.value;
  window.close()
//-->
</SCRIPT>
</BODY>
</HTML>