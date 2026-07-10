<?php

    function encrypt($string, $key){
        $result = "";
        for($i=0; $i<strlen($string); $i++){
            $char = substr($string, $i, 1);
            $keychar = substr($key, ($i % strlen($key))-1, 1);
            $char = chr(ord($char)+ord($keychar));
            $result.=$char;
        }
        $salt_string = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxys0123456789~!@#$^&*()_+`-={}|:<>?[]\;',./";
        $length = rand(1, 15);
        $salt = "";
        for($i=0; $i<=$length; $i++){
            $salt .= substr($salt_string, rand(0, strlen($salt_string)), 1);
        }
        $salt_length = strlen($salt);
        $end_length = strlen(strval($salt_length));
        return base64_encode($result.$salt.$salt_length.$end_length);
	}

	function decrypt($string, $key){
        $result = "";
        $string = base64_decode($string);
        $end_length = intval(substr($string, -1, 1));
        $string = substr($string, 0, -1);
        $salt_length = intval(substr($string, $end_length*-1, $end_length));
        $string = substr($string, 0, $end_length*-1+$salt_length*-1);
        for($i=0; $i<strlen($string); $i++){
            $char = substr($string, $i, 1);
            $keychar = substr($key, ($i % strlen($key))-1, 1);
            $char = chr(ord($char)-ord($keychar));
            $result.=$char;
        }
        return $result;
	}

    
	function connectPDO($type, $address, $port, $user, $pass, $database)
	{
		$connString = "";
		if($type == "Windows")
		{
			$connString = "sqlsrv:Server=$address,$port;Database=$database";
		}
		else if($type == "Linux")
		{
			$connString = "dblib:host=$address:$port;dbname=$database;charset=UTF-8";
		}
		else
		{
			return null;
		}
		
		try
		{   
			$conn = new PDO ($connString, "$user", "$pass");
			if($type == "Windows")
			{
				$conn->setAttribute(PDO::SQLSRV_ATTR_ENCODING, PDO::SQLSRV_ENCODING_UTF8);
			}
		}
		catch (PDOException $e) 
		{
			$error = $e->getMessage();
			$returnArray = Array();
			$returnArray["Success"] = 0;
			$returnArray["Message"] = " Unable to connect or select database! Check if IP address is on allow list at your host.";
			echo json_encode($returnArray);
			die();
		}
		if($conn == null)
		{
			$error = $e->getMessage();
			$returnArray = Array();
			$returnArray["Success"] = 0;
			$returnArray["Message"] = " Unable to connect or select database! Check if IP address is on allow list at your host.";
			echo json_encode($returnArray);
			die();          
		}
		
		return $conn;
	}
	function queryPDO($sql, $params, $conn = null)
	{
		if($conn == null)
		{
			$database = getDatabase();	
			$conn=connectPDO("Windows",$database["DatabaseAddress"], $database["DatabasePort"], $database["DatabaseUser"], $database["DatabasePassword"], $database["DatabaseName"]);		
		}
		$rows = []; 
		$stmt = $conn->prepare($sql);
		foreach($params as $param)
		{
			if(count($param) != 3)
			{
				$returnArray = Array();
				$returnArray["Success"] = 0;
				$returnArray["Message"] = "Wrong parameters";
				echo json_encode($returnArray);
				die();
			}       
			
			$stmt->bindParam($param[0], $param[1], $param[2]);
			
		}
		
		$success = $stmt->execute();
		if(!$success)
		{
			$returnArray = Array();
			$returnArray["Success"] = 0;
			$returnArray["Message"] = json_encode($stmt->errorInfo());
			echo json_encode($returnArray);
			die();
		}
		
		while($row = $stmt->fetch(PDO::FETCH_ASSOC))
		{                   
			if(in_array("JSONData", array_keys($row)))
			{
				if($row["JSONData"] == null)
				{
					$rows = [];
				}
				else
				{
					$rows = json_decode($row["JSONData"], true);
				}
				break;
			}
			
			if(isset($row["Error"]))
			{
				if(isset($row["Status"]) && $row["Status"] != 0)
				{                   
					$ret = Array();
					$ret["Message"] = $row["Error"];
					$ret["Success"] = $row["Status"];
					return $ret;
				}
				else 
				{
					$returnArray = Array();
					$returnArray["Success"] = 0;
					$returnArray["Message"] = $row["Error"];
					echo json_encode($returnArray);
					die();                            
				}               
			}
			array_push($rows, $row);
		}
		
		unset($stmt);
		unset($conn);
		return $rows;		
	}

    function getDatabase()
	{
		$settingsText = file_get_contents("database.txt");
		$rows = [];
        $settingsText = decrypt($settingsText, "99a21fccfd6ecaff34f6514fd06702ac");
        $splitSettingsText = explode(";", $settingsText);
		for ($i = 0; $i < count($splitSettingsText); $i++)
		{
            $line = $splitSettingsText[$i];
			$lineExplode = explode("=", $line);
            if($lineExplode[0] == "" && $lineExplode[1] == "")
            {
                continue;
            }
			$key = $lineExplode[0];
			$value = $lineExplode[1];
			$rows[$key] = $value;
		}
		return $rows;
	}

?>