var daysShort = ['Ned.','Pon.','Tor.','Sre.','Čet.','Pet.','Sob.'];
var date = null;
var dateOriginal = null;
var dateRefresh = 600000;
var dateRefreshCurrent = 600000;
var functionsFile = "Action.php";
var checkOutType = {};
checkOutType["-510"] = "Malica";
checkOutType["-504"] = "Službeni odhod";
checkOutType["-505"] = "Privatni odhod";
var analogClock;
var fileNames = new Array();
var screenSaverStart = 120;
var screenSaverFadeTime = 2;
var screenSaverImageChange = 10 + screenSaverFadeTime;
var screenSaverTimer;
var ControllerCode = localStorage.getItem("ControllerCode");


if ( ControllerCode === null || ControllerCode === undefined || ControllerCode === "")
{
    ControllerCode = getParameterByName("ControllerCode", window.location.href);
    localStorage.setItem("ControllerCode", ControllerCode);
}

//localStorage.getItem("lastname");


window.addEventListener('load', function() 
{   
    setupScreenSaver();
    getDate();
});

$(document).on('click','body *', function(event)
{
    clearTimeout(screenSaverTimer);
    screenSaver(false);
    screenSaverTimer = setTimeout(function(){ screenSaver(true)}, screenSaverStart * 1000);
    event.stopPropagation();
});

function setupScreenSaver()
{
    $("body").append('<div class="screenSaver"><img class="screenSaverImage" /></div>');
    $.ajax({
        url: "img/ScreenSaver/",
        success: function(data)
        {
            $(data).find("td > a").each(function()
            {
                if(openFile($(this).attr("href")))
                {
                    fileNames.push($(this).attr("href"));
                }           
            });
        }
    }); 

    setTimeout(screenSaverTick, screenSaverStart / 2 * 1000);
    screenSaverTimer = setTimeout(function(){ screenSaver(true)}, screenSaverStart * 1000);
}

function screenSaver(show)
{
    if(show)
    {
        $(".screenSaver").fadeIn(screenSaverFadeTime * 1000);
    }
    else
    {
        $(".screenSaver").fadeOut(0);
    }
}

function screenSaverTick()
{
    if(fileNames.length < 2)
    {
        return;
    }

    var imageSrc = "abc";
    while(imageSrc == $('.screenSaverImage').attr('src') || imageSrc == "abc")
    {
        rand = Math.floor(Math.random() * fileNames.length);
        imageSrc = "img/screenSaver/" + fileNames[rand];
    }
    
    $(".screenSaverImage").fadeOut(screenSaverFadeTime * 1000, function(){
        $('.screenSaverImage').attr('src', imageSrc);
        $(".screenSaverImage").fadeIn(screenSaverFadeTime * 1000);
    });
    
    setTimeout(screenSaverTick, screenSaverImageChange * 1000);
}

function openFile(file) 
{
    var extension = file.substr((file.lastIndexOf('.') + 1));
    switch(extension) {
        case 'jpg':
        case 'png':
            return true;
            break;
        default:
            return false;
    }
}

function getDayNameShort(now = new Date())
{
    return daysShort[now.getDay()];
}

function formatDate(date, format, utc) 
{
    //utc = true (time in UTC)
    var MMMM = ["\x00", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    var MMM = ["\x01", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    var dddd = ["\x02", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    var ddd = ["\x03", "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    function ii(i, len) { var s = i + ""; len = len || 2; while (s.length < len) s = "0" + s; return s; }

    var y = utc ? date.getUTCFullYear() : date.getFullYear();
    format = format.replace(/(^|[^\\])yyyy+/g, "$1" + y);
    format = format.replace(/(^|[^\\])yy/g, "$1" + y.toString().substr(2, 2));
    format = format.replace(/(^|[^\\])y/g, "$1" + y);

    var M = (utc ? date.getUTCMonth() : date.getMonth()) + 1;
    format = format.replace(/(^|[^\\])MMMM+/g, "$1" + MMMM[0]);
    format = format.replace(/(^|[^\\])MMM/g, "$1" + MMM[0]);
    format = format.replace(/(^|[^\\])MM/g, "$1" + ii(M));
    format = format.replace(/(^|[^\\])M/g, "$1" + M);

    var d = utc ? date.getUTCDate() : date.getDate();
    format = format.replace(/(^|[^\\])dddd+/g, "$1" + dddd[0]);
    format = format.replace(/(^|[^\\])ddd/g, "$1" + ddd[0]);
    format = format.replace(/(^|[^\\])dd/g, "$1" + ii(d));
    format = format.replace(/(^|[^\\])d/g, "$1" + d);

    var H = utc ? date.getUTCHours() : date.getHours();
    format = format.replace(/(^|[^\\])HH+/g, "$1" + ii(H));
    format = format.replace(/(^|[^\\])H/g, "$1" + H);

    var h = H > 12 ? H - 12 : H == 0 ? 12 : H;
    format = format.replace(/(^|[^\\])hh+/g, "$1" + ii(h));
    format = format.replace(/(^|[^\\])h/g, "$1" + h);

    var m = utc ? date.getUTCMinutes() : date.getMinutes();
    format = format.replace(/(^|[^\\])mm+/g, "$1" + ii(m));
    format = format.replace(/(^|[^\\])m/g, "$1" + m);

    var s = utc ? date.getUTCSeconds() : date.getSeconds();
    format = format.replace(/(^|[^\\])ss+/g, "$1" + ii(s));
    format = format.replace(/(^|[^\\])s/g, "$1" + s);

    var f = utc ? date.getUTCMilliseconds() : date.getMilliseconds();
    format = format.replace(/(^|[^\\])fff+/g, "$1" + ii(f, 3));
    f = Math.round(f / 10);
    format = format.replace(/(^|[^\\])ff/g, "$1" + ii(f));
    f = Math.round(f / 10);
    format = format.replace(/(^|[^\\])f/g, "$1" + f);

    var T = H < 12 ? "AM" : "PM";
    format = format.replace(/(^|[^\\])TT+/g, "$1" + T);
    format = format.replace(/(^|[^\\])T/g, "$1" + T.charAt(0));

    var t = T.toLowerCase();
    format = format.replace(/(^|[^\\])tt+/g, "$1" + t);
    format = format.replace(/(^|[^\\])t/g, "$1" + t.charAt(0));

    var tz = -date.getTimezoneOffset();
    var K = utc || !tz ? "Z" : tz > 0 ? "+" : "-";
    if (!utc) {
        tz = Math.abs(tz);
        var tzHrs = Math.floor(tz / 60);
        var tzMin = tz % 60;
        K += ii(tzHrs) + ":" + ii(tzMin);
    }
    format = format.replace(/(^|[^\\])K/g, "$1" + K);

    var day = (utc ? date.getUTCDay() : date.getDay()) + 1;
    format = format.replace(new RegExp(dddd[0], "g"), dddd[day]);
    format = format.replace(new RegExp(ddd[0], "g"), ddd[day]);

    format = format.replace(new RegExp(MMMM[0], "g"), MMMM[M]);
    format = format.replace(new RegExp(MMM[0], "g"), MMM[M]);

    format = format.replace(/\\(.)/g, "$1");

    return format;
};

function animate(selector)
{
    $(selector).removeClass("animateClick");
    $(selector).addClass("animateClick");
    setTimeout(function(){$(selector).removeClass("animateClick");}, 400);
}

function getDate()
{
    var info = {};
    info["func"] = "GetDate";
    post(info, function(data)
    {
        date = new Date(data["Date"]);
        dateOriginal = new Date();
        if(analogClock !== undefined && analogClock)
        {            
            $.fn.thooClock.setTime(new Date(date));
        }

        dateTimer();
    });
}

function dateTimer()
{        
    var diffTime = new Date() - dateOriginal;
    var theDate = new Date(formatDate(date, "yyyy-MM-dd HH:mm:ss"));
    theDate.setMilliseconds(theDate.getMilliseconds() + diffTime);
    $("#currentDate").text(getDayNameShort(theDate) + " " + formatDate(theDate, "dd.MM.yyyy"));
    $("#currentTime").text(formatDate(theDate, "HH:mm"));
    theDate.setSeconds(theDate.getSeconds() + 1);
    dateRefreshCurrent -= 1000;
    if(dateRefreshCurrent > 0)
    {
        if(window.location.href.includes("checkOut.html"))
        {
            recalculateElapsed(dateCheck, theDate);
        }

        setTimeout(dateTimer, 1000);
    }
    else
    {
        dateRefreshCurrent = dateRefresh;
        getDate();
    }
}

function recalculateElapsed(dateFrom, dateTo)
{
    if(dateTo == null || dateFrom == null)
    {
        return;
    }

    var diff = dateTo - dateFrom;
    var dateTemp = new Date("2019-10-07 00:00:00");
    dateTemp.setMilliseconds(diff);
    var time = formatDate(dateTemp, "HH:mm").split(":")[0] + "h " + formatDate(dateTemp, "HH:mm").split(":")[1] + "min";
    $(".timeElapsed").text(time);
}

function post(info, callback)
{
    /* Vsili podatek ControllerCode v vse API klice */
    info["ControllerCode"] = ControllerCode;

    $.post(functionsFile,
        info,
        function (data, status)
        {
            if (status == "success")
            {
                if (data["Success"] == 0)
                {
                    alert("Napaka: " + data["Message"]);
                    return;
                }
                else if (data["Success"] == 1)
                {
                    callback(data);
                }
                else
                {                    
                    alert("Prišlo je do napake.");
                }
            }
            else
            {
                alert("Prišlo je do napake.");
            }
        });
}

function getParameterByName(name, text)
{
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(text);
    if (results == null)
    {
        return "";
    }
    else
    {
        return decodeURIComponent(results[1].replace(/\+/g, " "));
    }
}