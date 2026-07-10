var timeout = 5;

$(document).ready(function() 
{
    timerTimeout();
});

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

    setTimeout(timerTimeout, 1000);
}

function exitClick()
{
    animate("exit");
    window.location.href = "index.html";
}