/*!
 * JS 1Klik d.o.o. v1.0.1
 *
 * Date: 2022-12-06T14:19Z
 */

"use strict";

 /***************************< 221206 >*************************** */

function encrypt(string, key) {
    string = encodeURI(string);
    var result = "";
    for (var i = 0; i < string.length; i++) {
        var char = string.substr(i, 1);
        var keychar = key.substr(i % key.length - 1, 1);
        char = String.fromCharCode(char.charCodeAt(0) + keychar.charCodeAt(0));
        result += char;
    }

    var salt_string = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxys0123456789~!@#$^&*()_+`-={}|:<>?[]\;',./";
    var length = Math.floor(Math.random() * 16 + 1);
    var salt = "";
    for (var i = 0; i < length; i++) {
        salt += salt_string.substr(Math.floor(Math.random() * salt_string.length), 1);
    }

    var salt_length = salt.length;
    var end_length = (salt_length + "").length;
    return btoa(result + salt + salt_length + end_length);
}

function decrypt(string, key) {
    var result = "";
    string = atob(string);
    var end_length = parseInt(string.substr(string.length - 1, 1));
    string = string.substr(0, string.length - 1);
    var salt_length = parseInt(string.substr(string.length - end_length, end_length));
    string = string.substr(0, string.length - (end_length + salt_length));
    for (var i = 0; i < string.length; i++) {
        var char = string.substr(i, 1);
        var keychar = key.substr(i % key.length - 1, 1);
        char = String.fromCharCode(char.charCodeAt(0) - keychar.charCodeAt(0));
        result += char;
    }

    try {
        return decodeURI(result);
    }
    catch (err) {
        return "";
    }
}

function randomString(length, charPool) {
    var text = "";
    if (charPool == null || charPool == "") {
        charPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    }

    for (var i = 0; i < length; i++) {
        text += charPool.charAt(Math.floor(Math.random() * charPool.length));
    }

    return text;
}

function getRandomArbitrary(min, max) {
    return parseInt(Math.random() * (max - min) + min);
}

/***************************< /221206 >*************************** */


function getParameterByName(name, text) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(text);
    if (results == null)
        return "";
    else
        return decodeURIComponent(results[1].replace(/\+/g, " "));
}

