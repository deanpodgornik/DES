var timeout = 5;
var idCheckEmployee = 0;

$(document).ready(function() 
{
    loginContact();
    timerTimeout();
});

function loginContact()
{
    var info = {};
    info["func"] = "CheckIn";
    post(info, function(data)
        {
            if(data["Data"]["Success"] == 1)
            {
                idCheckEmployee = data["Data"]["idCheckEmployee"];
                $("#name").text(data["Data"]["ContactName"]);
            }
            else
            {
                window.location.href = "index.html";
            }
        });
}

function timerTimeout()
{
    var time = timeout;
    if(time < 0)
    {
        time = 0;
    }

    $("#time").text(time + " sek")
    timeout -= 1;
    if(timeout < 0)
    {
        window.location.href = "index.html";
    }
    else
    {
        setTimeout(timerTimeout, 1000);
    }
}

function exitClick()
{
    animate("exit");
    var info = {};
    info["func"] = "CancelCheckIn";
    info["idCheckEmployee"] = idCheckEmployee;
    post(info, function()
    {            
        window.location.href = "index.html";
    });
}

function confirmClick()
{
    animate("confirm");
    window.location.href = "index.html";
}