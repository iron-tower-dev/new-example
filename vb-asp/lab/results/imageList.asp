<!--imageList.asp -->
<!--#include virtual="/includes/security.asp"-->
<!--#include virtual="/includes/DBFunctions.asp"-->
<script src="../../includes/jquery-3.7.1.js"></script>
<%
testid = Request.QueryString("tid")
fortag = Request.QueryString("tag")
forcomp = Request.QueryString("comp")
forloc = Request.QueryString("loc")

Sub ShowImages(testID, tag, comp, loc)
    if testID="120" or testID="180" or testID="210" or testID="240" then
		dim sConn, sSql, sRS, iConn, iSql, iRS, blnImage, imgClass, imgFile, idList
		blnImage=false
        idList=""

        'get list of sample IDs for the selected sampling point
        sSql = "SELECT CAST((SELECT '''' + CONVERT(VARCHAR,s.ID) + '''' + ',' FROM (SELECT DISTINCT(u.ID) FROM UsedLubeSamples u WHERE u.tagNumber = '" & tag & "' AND u.component = '" & comp & "' AND u.location = '" & loc & "') s for XML path('')) AS VARCHAR(MAX)) AS IDS"
        set sConn=OpenConnection(Application("dbLUBELAB_ConnectionString"))
        if IsOpen(sConn) then
          set sRS=DisconnectedRS(sSql,sConn)
          if not sRS.EOF then
            sRS.MoveFirst
            idList = sRS(0)
          end if
        end if
        CloseDBObject(sConn)
        set sConn=nothing

        if (len(idList) > 0) then
          Response.Write "<td><input type='button' name='cmdShow' id='cmdShow' value='Hide other tests' onclick='toggleImages(" & testID & ")' />"

          idList = left(idList, len(idList)-1)
          iSql = "SELECT TO_NUMBER(T.SAMPLE_ID) ID, T.NETWORKPATH FROM LUBELAB.THERM_PROD_LUBE_IMAGE_V T WHERE T.SAMPLE_ID IN (" & idList & ") ORDER BY ID DESC, FILENAME"
		  Set iConn = Server.CreateObject("ADODB.Connection")
		  iConn.Open Application("dbSWMS_ConnectionString")
          set iRS=DisconnectedRS(iSql,iConn)
          CloseDBObject(iConn)
          set iConn=nothing		  
          do until iRS.eof 'gets image file locations
			blnImage=true
            imgFile = UCase(iRS(1))
            imgClass = "210"
            if (instr(imgFile, "FR") > 0) then
                imgClass = "180"
            elseif (instr(imgFile, "DI") > 0) then
                imgClass = "240"
            elseif (instr(imgFile, "IF") > 0) then
                imgClass = "120"
            end if
			Response.Write "<td class='blank image'><a href=" & chr(34) & iRS(1) & chr(34) & " target=_blank border=0 class='image " & imgClass & "'><img src=" & chr(34)  & "file:" & iRS(1) & chr(34) & " title=" & chr(34) & "Sample: " & iRS(0) & chr(34) & " style='height:120px; width:160px' />&nbsp;</a></td>"
			iRS.movenext
		  loop
		  if blnImage<>true then
			Response.Write "No images found"
		  end if
        else
          Response.Write "No images found"
        end if
	end if
end Sub
%>
<html>
<head>
	<LINK REL=STYLESHEET TYPE="text/css" HREF="<%=Application("URL")%>includes/lab.css">
</head>

<body>
    <table>
        <tr>
<%ShowImages testid, fortag, forcomp, forloc%>
        </tr>
    </table>
</body>
</html>

<script language="javascript">

    function showAllImages() {
        $('td .image').show();
    }

    function hideAllImagesExcept(testID) {
        $('td .image').not('.' + testID).hide();
    }

    function toggleImages(testID) {
        var text = $("#cmdShow").attr("value");
        if (text == "Show All"){
            $("#cmdShow").attr("value", "Hide other tests");
            showAllImages();
        }
        else {
            $("#cmdShow").attr("value", "Show All");
            hideAllImagesExcept(testID);
        }
    }

</script>

<!--#include virtual="/includes/footer.js"-->
