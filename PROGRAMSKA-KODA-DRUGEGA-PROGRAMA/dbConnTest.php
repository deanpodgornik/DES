<?php

$parameters = $_POST;

//print_r($parameters);

if ($parameters != null) {
    checkConnection($parameters);
}



function checkConnection($parameters)
{

    $type = $parameters["connType"];
    $address = $parameters["ipAddress"];
    $port = $parameters["dbPort"];
    $user = $parameters["dbUser"];
    $pass = $parameters["dbPassword"];
    $database = $parameters["dbName"];

    $conn = connectPDO($type, $address, $port, $user, $pass, $database);

    $sql = "SELECT Contact FROM Contact WHERE idContact = 0";
    $params = array();
    $rows = queryPDO($sql, $params, $conn);

    if (count($rows) < 1) {
        echo '<div style="font-size: 14pt; width: 200px; text-align: center; margin: 80px auto 80px auto; background-color: #DC143C; padding: 40px; color: white;">Conn error!</div>';
    } else {
        echo '<div style="font-size: 14pt; width: 200px; text-align: center; margin: 80px auto 80px auto; background-color: #4CAF50; padding: 40px; color: white;">Conn success!</div>';
    }

    die();
}

function connectPDO($type, $address, $port, $user, $pass, $database)
{
    $connString = "";
    if ($type == "Windows") {
        $connString = "sqlsrv:Server=$address,$port;Database=$database";
    } else if ($type == "Linux") {
        $connString = "dblib:host=$address:$port;dbname=$database;charset=UTF-8";
    } else {
        return null;
    }

    try {
        $conn = new PDO($connString, "$user", "$pass");
        if ($type == "Windows") {
            $conn->setAttribute(PDO::SQLSRV_ATTR_ENCODING, PDO::SQLSRV_ENCODING_UTF8);
        }
    } catch (PDOException $e) {
        $error = $e->getMessage();
        $returnArray = array();
        $returnArray["Success"] = 0;
        $returnArray["Message"] = " Unable to connect or select database! Check if IP address is on allow list at your host. Connection type: " . $type;
        echo json_encode($returnArray);
        die();
    }
    if ($conn == null) {
        $error = $e->getMessage();
        $returnArray = array();
        $returnArray["Success"] = 0;
        $returnArray["Message"] = " Unable to connect or select database! Check if IP address is on allow list at your host. Connection type: " . $type;
        echo json_encode($returnArray);
        die();
    }

    return $conn;
}
function queryPDO($sql, $params, $conn = null)
{
    if ($conn == null) {
        $conn = connectPDO("Windows", $database["DatabaseAddress"], $database["DatabasePort"], $database["DatabaseUser"], $database["DatabasePassword"], $database["DatabaseName"]);
    }
    $rows = [];
    $stmt = $conn->prepare($sql);
    foreach ($params as $param) {
        if (count($param) != 3) {
            $returnArray = array();
            $returnArray["Success"] = 0;
            $returnArray["Message"] = "Wrong parameters";
            echo json_encode($returnArray);
            die();
        }

        $stmt->bindParam($param[0], $param[1], $param[2]);
    }

    $success = $stmt->execute();
    if (!$success) {
        $returnArray = array();
        $returnArray["Success"] = 0;
        $returnArray["Message"] = json_encode($stmt->errorInfo());
        echo json_encode($returnArray);
        die();
    }

    while ($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
        if (in_array("JSONData", array_keys($row))) {
            if ($row["JSONData"] == null) {
                $rows = [];
            } else {
                $rows = json_decode($row["JSONData"], true);
            }
            break;
        }

        if (isset($row["Error"])) {
            if (isset($row["Status"]) && $row["Status"] != 0) {
                $ret = array();
                $ret["Message"] = $row["Error"];
                $ret["Success"] = $row["Status"];
                return $ret;
            } else {
                $returnArray = array();
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

?>

<!DOCTYPE html>
<html>
<style>
    #mainContainer {
        max-width: 320px;
        margin: auto;
    }

    input[type=text],
    input[type=password],
    select {
        width: 100%;
        padding: 12px 20px;
        margin: 8px 0;
        display: inline-block;
        border: 1px solid #ccc;
        border-radius: 4px;
        box-sizing: border-box;
    }

    button {
        width: 100%;
        background-color: #4CAF50;
        color: white;
        padding: 14px 20px;
        margin: 8px 0;
        border: none;
        border-radius: 4px;
        cursor: pointer;
    }

    button:hover {
        background-color: #45a049;
    }

    div {
        border-radius: 5px;
        background-color: #f2f2f2;
        padding: 20px;
    }
</style>

<body>

    <div id="mainContainer">
        <h3>DB connection test</h3>

        <div>
            <form id="testDbConnForm" method="POST">

                <label for="cars">Choose db connection type:</label>

                <select name="connType" id="connType">
                    <option value="Linux">Linux</option>
                    <option value="Windows" selected>Windows</option>
                </select>

                Db name: <input type="text" name="dbName"><br>
                User: <input type="text" name="dbUser"><br>
                Password: <input type="password" name="dbPassword"><br>
                IP address: <input type="text" name="ipAddress"><br>
                Port: <input type="text" name="dbPort"><br>

                <button id="testDbConnFormSubmit">Test</button>

            </form>
        </div>
    </div>

    <script>
        var form = document.getElementById("testDbConnForm");



        document.getElementById("testDbConnFormSubmit").addEventListener("click", function() {

            event.preventDefault();

            var inputs = document.querySelectorAll("#testDbConnForm input[name]");
            var validationError = 0;
            for (i = 0; i < inputs.length; i++) {
                if (inputs[i].value < 2) {
                    alert("Please fill in all fields!");
                    validationError = 1;
                    break;
                }
            }

            if(validationError == 0)
            {
                 form.submit();
            }

        });
    </script>
</body>

</html>