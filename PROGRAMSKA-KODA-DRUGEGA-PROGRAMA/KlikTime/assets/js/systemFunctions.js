/*!
 * JS 1Klik d.o.o. v1.0.0
 *
 * Date: 2022-09-26T14:19Z
 */

(
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
);
