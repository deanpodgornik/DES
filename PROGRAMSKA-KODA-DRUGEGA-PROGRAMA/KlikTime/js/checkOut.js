var timeout = 10;
var selected = 0;
var idCheckEmployee = 0;
var dateCheck = null;

$(document).ready(function() 
{
    timerTimeout();
    getCheck();
});

function getCheck()
{
    var info = {};
    info["func"] = "CheckOutGet";
    post(info, function(data)
        {
            if(data["Data"]["Success"] == 1)
            {
                idCheckEmployee = data["Data"]["idCheckEmployee"];
                $("#name").text(data["Data"]["ContactName"]);
                dateCheck = new Date(data["Data"]["DateCheck"]);
                dateCheck.setSeconds(0);
                dateCheck.setMilliseconds(0);
                $("#checkInDate").text(getDayNameShort(dateCheck) + " " + formatDate(dateCheck, "dd.MM.yyyy"));
                $("#checkInTime").text(formatDate(dateCheck, "HH:mm"));
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
    window.location.href = "index.html";
}

function confirmClick(name, idGroup)
{
    animate(name);
    var info = {};
    info["func"] = "CheckOut";
    info["idCheckEmployee"] = idCheckEmployee;
    info["idGroup"] = idGroup;
    post(info, function()
    {            
        window.location.href = "index.html";
    });
}

function changeClick(direction)
{
    var allButtons = $(".changeable");
    var next = selected;
    if(direction == "left")
    {
        next -= 1;
        if(next < 0)
        {
            next = allButtons.length - 1;
        }
    }
    else if(direction == "right")
    {
        next += 1;
        if(next > allButtons.length - 1)
        {
            next = 0;
        }
    }
    
    $(allButtons[selected]).animate({ width: 'hide' }, 100, function(){
        $(allButtons[next]).animate({ width: 'show' }, 100); 
        selected = next;
    }); 
}