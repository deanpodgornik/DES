var fileNames = new Array();
var screenSaverStart = 1;
var screenSaverFadeTime = 2;
var screenSaverImageChange = 10 + screenSaverFadeTime;
var screenSaverTimer;

window.addEventListener('load', function() 
{   
    setupScreenSaver();
    console.log("Test");
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
        url: "assets/img/ScreenSaver/",
        success: function(data)
        {
            console.log(data);
            $(data).find("td > a").each(function()
            {
                if(openFile($(this).attr("href")))
                {
                    fileNames.push($(this).attr("href"));
                    //console.log(fileNames.push($(this).attr("href")));
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
        imageSrc = "assets/img/ScreenSaver/" + fileNames[rand];
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
        case 'jpeg':
        case 'png':
            return true;
            break;
        default:
            return false;
    }
}
