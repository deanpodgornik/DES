var password = "";
var passwordOriginalTimeout = 5;
var passwordTimeout = -1;
var analogClock = true;
$(document).ready(function() 
{
    $('#clock').thooClock({        
        size:200,
        dialColor:'#848688',
        secondHandColor:'#0098DA',
        minuteHandColor:'#57595b',
        hourHandColor:'#57595b',
        dateStart:new Date()
    });
    setKeyPad();
    passwordTimer();

    document.getElementById("divControllerCode").innerHTML = ControllerCode;

});

function setKeyPad()
{    
    var size = keypadCalculateSize(".keypadContainer");
    $(".keypad").width(size["Width"]);
    $(".keypad").height(size["Height"]);
}

function keypadCalculateSize(selector)
{
    var width = $(selector).width();
    var height = $(selector).height();
    var takeWidth = true
    if(width * (1 + 1 / 3) > height)
    {
        takeWidth = false;
    }

    if(takeWidth)
    {
        height = width * (1 + 1 / 3);
    }
    else
    {
        width = height * (1 - 1 / 4);
    }

    var ret = {};
    ret["Width"] = width;
    ret["Height"] = height;
    return ret;
}

function addPassword(key)
{
    animate("#n" + key);
    if(key == "x")
    {
        resetPassword();
        return;
    }

    if(key == "c")
    {
        passwordTimeout = 100;
        var info = {};
        info["Code"] = password;
        info["func"] = "CheckCode";
        
        post(info, function(data)
        {
            if(data["Data"]["Success"] == 1)
            {
                if(data["Data"]["Status"] == 1)
                {
                    window.location.href = "checkIn.html?idContact=" + data["Data"]["idContact"];
                }
                else if(data["Data"]["Status"] == 2)
                {
                    window.location.href = "checkOut.html?idContact=" + data["Data"]["idContact"];
                }
                else if(data["Data"]["Status"] == 3)
                {
                    window.location.href = "checkBackIn.html?idContact=" + data["Data"]["idContact"];
                }
                else
                {
                    resetPassword();
                    return;
                }
            }
            else if(data["Data"]["Success"] == 2)
            {
                window.location.href = "fail.html";
            }
            else
            {
                resetPassword();
            }
        });

        return;
    }

    passwordTimeout = 5;

    if(password.length > 7)
    {
        return;
    }

    password += key;

    var html = "";
    for(var i = 0; i < password.length; i++)
    {
        if(i > 0)
        {
            html += "&nbsp;";
        }

        html += '<i class="fas fa-circle"></i>';
    }

    $("#password").html(html);
}

function passwordTimer()
{
    passwordTimeout -= 1;
    if(passwordTimeout < 0)
    {
        resetPassword();
    }

    setTimeout(passwordTimer, 1000);
}

function resetPassword()
{
    $("#password").html('<i class="far fa-circle"></i>&nbsp;<i class="far fa-circle"></i>&nbsp;<i class="far fa-circle"></i>&nbsp;<i class="far fa-circle"></i>');
    password = "";
    passwordTimeout = -1;
}