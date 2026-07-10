<?php
    header("access-control-allow-origin: *");
    header('Content-Type: application/json');
    include 'Functions.php';
	$text = "DatabaseAddress=192.168.1.3;DatabasePort=7200;DatabaseUser=user;DatabasePassword=password;DatabaseName=SIS";
    $encrypted = encrypt($text, "99a21fccfd6ecaff34f6514fd06702ac");
    echo $encrypted;
    file_put_contents("database.txt", $encrypted); 
    $text = getDatabase();
    echo json_encode($text);
	die();
	
	    if($_POST["func"] == "valid")
    {
        $date = $_POST["date"];
        $idContact = $_POST["idContact"];
        $sql = "select top 1 idContact from Contact where idContact>0";
        $params[0] = array(1, $idContact);
        $rows = query($sql, $params);
        for($i = 0; $i < count($rows); $i++)
        {
            
            $rows[$i]["DoNum"] =$rows[$i]["idContact"];
        }
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Card"] = $rows[0];
        echo json_encode($returnArray);
    }
	
		    if($_POST["func"] == "notValidExt")
    {
        $date = $_POST["date"];
        $idContact = $_POST["idContact"];
        $sql = "select top 1 idContact from Contact where idContact>0";
        $params[0] = array(1, $idContact);
        $rows = query($sql, $params);
        for($i = 0; $i < count($rows); $i++)
        {
            
			
            $rows[$i]["DoNum"] =$_SERVER['REMOTE_ADDR'];
        }
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Card"] = $rows[0];
        echo json_encode($returnArray);
    }
	
		    if($_POST["func"] == "validExt44")
    {
        $date = $_POST["date"];
        $idContact = $_POST["idContact"];
        $sql = "select 'test' as DoNum";
        $params[0] = array(1, $idContact);
        $rows = query($sql, $params);
        for($i = 0; $i < count($rows); $i++)
        {
            
			
            $rows[$i]["DoNum"] =$_SERVER['REMOTE_ADDR'];
			 $rows[$i]["DoNum"] ="08:09 ZTR<BR>08:09 LTR";
        }
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Card"] = $rows[0];
        echo json_encode($returnArray);
    }	
 
 if($_POST["func"] == "validExt")
    {
        $remoteAddr = $_SERVER['REMOTE_ADDR'];
       
        $sql = "exec custCatezInfo ?";
        $params[0] = array($remoteAddr);
        $rows = query($sql, $params);
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Card"] = $rows[0];
        echo json_encode($returnArray);
    }	
	
    if($_POST["func"] == "cas")
    {
        $remoteAddr = $_SERVER['REMOTE_ADDR'];
       
        $sql = "exec customWoop_infodisplayTime ?";
        $params[0] = array($remoteAddr);
        $rows = query($sql, $params);
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Card"] = $rows[0];
        echo json_encode($returnArray);
    }

    if($_POST["func"] == "security")
    {
        $remoteAddr = $_SERVER['REMOTE_ADDR'];
       
        $sql = "exec customWoop_infodisplaySecurity ?";
        $params[0] = array($remoteAddr);
        $rows = query($sql, $params);
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Card"] = $rows[0];
        echo json_encode($returnArray);
    }

    if($_POST["func"] == "omarica")
    {
        $remoteAddr = $_SERVER['REMOTE_ADDR'];
       
        $sql = "exec infodisplayShowData ?";
        $params[0] = array($remoteAddr);
        $rows = query($sql, $params);
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Card"] = $rows[0];
        echo json_encode($returnArray);
    }		
	
    if($_POST["func"] == "valid2")
    {
        $date = $_POST["date"];
        $idContact = $_POST["idContact"];
        $sql = "exec APIs_TaskCard ?, ?, ?, ?, ?, ?, ?, ?, ?, ?";
        $params[0] = array(1, '1.0', 0, 1, $date, $date, $idContact, null, null, null);
        $rows = query($sql, $params);
        for($i = 0; $i < count($rows); $i++)
        {
            $rows[$i]["DateFrom"] = date('D M d Y H:i:s O', strtotime($rows[$i]["DateFrom"]));
            $rows[$i]["DateTo"] = date('D M d Y H:i:s O', strtotime($rows[$i]["DateTo"]));
        }
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Card"] = $rows[0];
        echo json_encode($returnArray);
    }
    if($_POST["func"] == "notValid2")
    {
        $idContact = $_POST["idContact"];
        $sql = "exec APIs_TaskCard ?, ?, ?, ?, ?, ?, ?, ?, ?, ?";
        $params[0] = array(2, '1.0', 0, 1, null, null, $idContact, null, null, null);
        $rows = query($sql, $params);
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Card"] = $rows[0];
        echo json_encode($returnArray);
    }
?>