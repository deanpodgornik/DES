<?php
    header("access-control-allow-origin: *");
    header('Content-Type: application/json');
    include "functions.php";
    $idCompany = 3;
    $func = $_POST["func"];
    $IPAddress = $_SERVER['REMOTE_ADDR']; /* Depricated 250612: If IT is not in same LAN as database you get WAN IP address instead of LAN address. If more then one IT is on remote location, distinguising on IP address is not possible */
    $ControllerCode = $_POST["ControllerCode"];

    if(!isset($ControllerCode))
    {
        $returnArray["Success"] = 0;
        $returnArray["Message"] = "Missing ControllerCode!";
        echo(json_encode($returnArray));
        die();
    }

    if($func == "GetDate")
    {
        $sql = "SELECT GETDATE() AS 'Date'";
        $params = array();
        $rows = queryPDO($sql, $params);
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Date"] = $rows[0]["Date"];
        echo(json_encode($returnArray));
        die();
    }
    if($func == "CheckCode")
    {
        if(!isset($_POST["Code"]))
        {
            $returnArray["Success"] = 0;
            $returnArray["Message"] = "Missing code!";
            echo(json_encode($returnArray));
            die();
        }

        $code = $_POST["Code"];
        $sql = "EXEC qp_CheckEmployee @Select = :Select, @idCompany = :idCompany, @ControllerCode = :ControllerCode, @Code = :Code";
        $select = 2;
        $params = array();
        array_push($params, Array(":Select", $select, PDO::PARAM_INT)); 
        array_push($params, Array(":idCompany", $idCompany, PDO::PARAM_INT)); 
        array_push($params, Array(":ControllerCode", $ControllerCode, PDO::PARAM_STR));
        array_push($params, Array(":Code", $code, PDO::PARAM_STR));
        $rows = queryPDO($sql, $params);
        if($rows[0]["Success"] == 0)
        {
            $returnArray["Success"] = 0;
            $returnArray["Message"] = $rows[0]["ResponseMsg"];
            echo(json_encode($returnArray));
            die();
        }
        
        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Data"] = $rows[0];
        echo(json_encode($returnArray));
        die();
    }
    if($func == "CheckIn")
    {
        $sql = "EXEC qp_CheckEmployee @Select = :Select, @idCompany = :idCompany, @ControllerCode = :ControllerCode";
        $select = 3;
        $params = array();
        array_push($params, Array(":Select", $select, PDO::PARAM_INT)); 
        array_push($params, Array(":idCompany", $idCompany, PDO::PARAM_INT)); 
        array_push($params, Array(":ControllerCode", $ControllerCode, PDO::PARAM_STR));
        $rows = queryPDO($sql, $params);
        if($rows[0]["Success"] == 0)
        {
            $returnArray["Success"] = 0;
            $returnArray["Message"] = $rows[0]["ResponseMsg"];
            echo(json_encode($returnArray));
            die();
        }

        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Data"] = $rows[0];
        echo(json_encode($returnArray));
        die();
    }
    if($func == "CancelCheckIn")
    {
        if(!isset($_POST["idCheckEmployee"]))
        {
            $returnArray["Success"] = 0;
            $returnArray["Message"] = "Missing idCheckEmployee!";
            echo(json_encode($returnArray));
            die();
        }

        $idCheckEmployee = $_POST["idCheckEmployee"];
        $sql = "EXEC qp_CheckEmployee @Select = :Select, @idCompany = :idCompany, @ControllerCode = :ControllerCode, @idCheckEmployee = :idCheckEmployee";
        $select = 4;
        $params = array();
        array_push($params, Array(":Select", $select, PDO::PARAM_INT)); 
        array_push($params, Array(":idCompany", $idCompany, PDO::PARAM_INT)); 
        array_push($params, Array(":ControllerCode", $ControllerCode, PDO::PARAM_STR));
        array_push($params, Array(":idCheckEmployee", $idCheckEmployee, PDO::PARAM_INT));
        $rows = queryPDO($sql, $params);
        if($rows[0]["Success"] == 0)
        {
            $returnArray["Success"] = 0;
            $returnArray["Message"] = $rows[0]["ResponseMsg"];
            echo(json_encode($returnArray));
            die();
        }

        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Data"] = $rows[0];
        echo(json_encode($returnArray));
        die();
    }
    else if($func == "CheckOutGet")
    {
        $sql = "EXEC qp_CheckEmployee @Select = :Select, @idCompany = :idCompany, @ControllerCode = :ControllerCode";
        $select = 5;
        $params = array();
        array_push($params, Array(":Select", $select, PDO::PARAM_INT)); 
        array_push($params, Array(":idCompany", $idCompany, PDO::PARAM_INT)); 
        array_push($params, Array(":ControllerCode", $ControllerCode, PDO::PARAM_STR));
        $rows = queryPDO($sql, $params);
        if($rows[0]["Success"] == 0)
        {
            $returnArray["Success"] = 0;
            $returnArray["Message"] = $rows[0]["ResponseMsg"];
            echo(json_encode($returnArray));
            die();
        }

        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Data"] = $rows[0];
        echo(json_encode($returnArray));
        die();
    }
    else if($func == "CheckOut")
    {
        if(!isset($_POST["idCheckEmployee"]))
        {
            $returnArray["Success"] = 0;
            $returnArray["Message"] = "Missing idCheckEmployee!";
            echo(json_encode($returnArray));
            die();
        }
        
        if(!isset($_POST["idGroup"]))
        {
            $returnArray["Success"] = 0;
            $returnArray["Message"] = "Missing idGroup!";
            echo(json_encode($returnArray));
            die();
        }

        $idCheckEmployee = $_POST["idCheckEmployee"];
        $idGroup = $_POST["idGroup"];
        $sql = "EXEC qp_CheckEmployee @Select = :Select, @idCompany = :idCompany, @ControllerCode = :ControllerCode, @idCheckEmployee = :idCheckEmployee, @idGroup = :idGroup";
        $select = 6;
        $params = array();
        array_push($params, Array(":Select", $select, PDO::PARAM_INT)); 
        array_push($params, Array(":idCompany", $idCompany, PDO::PARAM_INT)); 
        array_push($params, Array(":ControllerCode", $ControllerCode, PDO::PARAM_STR));
        array_push($params, Array(":idCheckEmployee", $idCheckEmployee, PDO::PARAM_INT));
        array_push($params, Array(":idGroup", $idGroup, PDO::PARAM_INT)); 
        $rows = queryPDO($sql, $params);
        if($rows[0]["Success"] == 0)
        {
            $returnArray["Success"] = 0;
            $returnArray["Message"] = $rows[0]["ResponseMsg"];
            echo(json_encode($returnArray));
            die();
        }

        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Data"] = $rows[0];
        echo(json_encode($returnArray));
        die();
    }
    if($func == "CheckBackIn")
    {
        $sql = "EXEC qp_CheckEmployee @Select = :Select, @idCompany = :idCompany, @ControllerCode = :ControllerCode";
        $select = 7;
        $params = array();
        array_push($params, Array(":Select", $select, PDO::PARAM_INT)); 
        array_push($params, Array(":idCompany", $idCompany, PDO::PARAM_INT)); 
        array_push($params, Array(":ControllerCode", $ControllerCode, PDO::PARAM_STR));
        $rows = queryPDO($sql, $params);
        if($rows[0]["Success"] == 0)
        {
            $returnArray["Success"] = 0;
            $returnArray["Message"] = $rows[0]["ResponseMsg"];
            echo(json_encode($returnArray));
            die();
        }

        $returnArray = Array();
        $returnArray["Success"] = 1;
        $returnArray["Data"] = $rows[0];
        echo(json_encode($returnArray));
        die();
    }
    else 
    {
        $returnArray["Success"] = 0;
        $returnArray["Message"] = "Wrong function!";
        echo(json_encode($returnArray));
        die();
    }
?>