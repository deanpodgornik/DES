<?php
header("access-control-allow-origin: *");
header('Content-Type: application/json');
include "functions.php";

    $sql = "SELECT TOP 2 * FROM Contact ";
    $params = array();
    $rows = queryPDO($sql, $params);
    echo json_encode($rows);
    die();