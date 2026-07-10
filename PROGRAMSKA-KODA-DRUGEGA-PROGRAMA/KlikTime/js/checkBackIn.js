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
    info["func"] = "CheckBackIn";
    post(info, function(data)
    {
        if(data["Data"]["Success"] == 1)
        {
            idCheckEmployee = data["Data"]["idCheckEmployee"];
            $("#name").prepend(data["Data"]["ContactName"]);
            $("#type").text(checkOutType[data["Data"]["idGroup"]]);
            var dateIn = new Date(data["Data"]["DateIn"])
            dateIn.setSeconds(0);
            dateIn.setMilliseconds(0);
            $("#checkInDate").text(getDayNameShort(dateIn) + " " + formatDate(dateIn, "dd.MM.yyyy"));
            $("#checkInTime").text(formatDate(dateIn, "HH:mm"));
            var dateOut = new Date(data["Data"]["DateOut"])
            dateOut.setSeconds(0);
            dateOut.setMilliseconds(0);
            $("#checkOutDate").text(getDayNameShort(dateOut) + " " + formatDate(dateOut, "dd.MM.yyyy"));
            $("#checkOutTime").text(formatDate(dateOut, "HH:mm"));
            recalculateElapsed(dateOut, dateIn);
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