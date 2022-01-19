// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var imgData = {};
var leftSlider = {};
var rightSlider = {};
var topSlider = {};
var bottomSlider = {};


function changeImage() {

    var imgId = $("#imageId").val();
    var img = document.getElementById("theimage");
    var fullsize = document.getElementById("fullsizelink")
    var original = document.getElementById("originallink")
    var rot = document.getElementById("rotVal").value;
    var rot2 = document.getElementById("rot2Val").value;
    var leftcrop = document.getElementById("leftVal").value;
    var rightcrop = document.getElementById("rightVal").value;
    var topcrop = document.getElementById("topVal").value;
    var bottomcrop = document.getElementById("bottomVal").value;
    var screensize = "";
    if ($(window).width() < 1280) {
        screensize = "mobile";
    }


    var imgId = $("#imageId").val();

    var imgQuery = "?" + encodeURI("crop=" + leftcrop + "," + topcrop + "," + rightcrop + "," + bottomcrop + "&rot=" + rot + "&rot2=" + rot2);

    img.src = window.location.origin + "/home/image" + screensize + "/" + imgId + imgQuery;
    fullsize.href = window.location.origin + "/home/imagefull/" + imgId + imgQuery;
    original.href = window.location.origin + "/home/imagefull/" + imgId;
}

function setRotation(rot) {

    if (rot < 0) {
        rot += 360;
    }
    else if (rot > 270) {
        rot %= 360;
    }

    if (rot == 360) {
        rot = 0;
    }

    $("#rotVal").val(rot);

    var ogWidth = $("#ogImageWidth").val();
    var ogHeight = $("#ogImageHeight").val();

    switch (rot) {
        case 0:
            //right(on the top)
            rightSlider.bootstrapSlider("setAttribute", "max", ogWidth);
            rightSlider.bootstrapSlider("refresh");
            rightSlider.bootstrapSlider("setValue", $("#rightVal").val());
            rightSlider.off("slide");
            rightSlider.on("slide", function (slideEvt) {
                $("#rightVal").val(slideEvt.value);
                changeImage();
            });
            //bottom(on the right)
            bottomSlider.bootstrapSlider("setAttribute", "max", ogHeight);
            bottomSlider.bootstrapSlider("refresh");
            bottomSlider.bootstrapSlider("setValue", $("#bottomVal").val());
            bottomSlider.off("slide");
            bottomSlider.on("slide", function (slideEvt) {
                $("#bottomVal").val(slideEvt.value);
                changeImage();
            });
            //left(on the bottom)
            leftSlider.bootstrapSlider("setAttribute", "max", ogWidth);
            leftSlider.bootstrapSlider("refresh");
            leftSlider.bootstrapSlider("setValue", $("#leftVal").val());
            leftSlider.off("slide");
            leftSlider.on("slide", function (slideEvt) {
                $("#leftVal").val(slideEvt.value);
                changeImage();
            });
            //top(on the left)
            topSlider.bootstrapSlider("setAttribute", "max", ogHeight);
            topSlider.bootstrapSlider("refresh");
            topSlider.bootstrapSlider("setValue", $("#topVal").val());
            topSlider.off("slide");
            topSlider.on("slide", function (slideEvt) {
                $("#topVal").val(slideEvt.value);
                changeImage();
            });
            break;
        case 90:
            //right(on the top) - now controls top
            var tc = ogHeight - $("#topVal").val();
            rightSlider.bootstrapSlider("setAttribute", "max", ogHeight);
            rightSlider.bootstrapSlider("refresh");
            rightSlider.bootstrapSlider("setValue", tc);
            rightSlider.off("slide");
            rightSlider.on("slide", function (slideEvt) {
                $("#topVal").val(ogHeight - slideEvt.value);
                changeImage();
            });
            //bottom(on the right) - now controls right
            var rc = $("#rightVal").val();
            bottomSlider.bootstrapSlider("setAttribute", "max", ogWidth);
            bottomSlider.bootstrapSlider("refresh");
            bottomSlider.bootstrapSlider("setValue", rc);
            bottomSlider.off("slide");
            bottomSlider.on("slide", function (slideEvt) {
                $("#rightVal").val(slideEvt.value);
                changeImage();
            });
            //left(on the bottom) - now controls bottom
            var bc = ogHeight - $("#bottomVal").val();
            leftSlider.bootstrapSlider("setAttribute", "max", ogHeight);
            leftSlider.bootstrapSlider("refresh");
            leftSlider.bootstrapSlider("setValue", bc);
            leftSlider.off("slide");
            leftSlider.on("slide", function (slideEvt) {
                $("#bottomVal").val(ogHeight - slideEvt.value);
                changeImage();
            });
            //top(on the left) - now controls left
            topSlider.bootstrapSlider("setAttribute", "max", ogWidth);
            topSlider.bootstrapSlider("refresh");
            topSlider.bootstrapSlider("setValue", $("#leftVal").val());
            topSlider.off("slide");
            topSlider.on("slide", function (slideEvt) {
                $("#leftVal").val(slideEvt.value);
                changeImage();
            });
            break;
        case 180:
            //right(on the top) - now controls left
            rightSlider.bootstrapSlider("setAttribute", "max", ogWidth);
            rightSlider.bootstrapSlider("refresh");
            rightSlider.bootstrapSlider("setValue", ogWidth - $("#leftVal").val());
            rightSlider.off("slide");
            rightSlider.on("slide", function (slideEvt) {
                $("#leftVal").val(ogWidth - slideEvt.value);
                changeImage();
            });
            //bottom(on the right) - now controls top
            bottomSlider.bootstrapSlider("setAttribute", "max", ogHeight);
            bottomSlider.bootstrapSlider("refresh");
            bottomSlider.bootstrapSlider("setValue", ogHeight - $("#topVal").val());
            bottomSlider.off("slide");
            bottomSlider.on("slide", function (slideEvt) {
                $("#topVal").val(ogHeight - slideEvt.value);
                changeImage();
            });
            //left(on the bottom) - now controls right
            leftSlider.bootstrapSlider("setAttribute", "max", ogWidth);
            leftSlider.bootstrapSlider("refresh");
            leftSlider.bootstrapSlider("setValue", ogWidth - $("#rightVal").val());
            leftSlider.off("slide");
            leftSlider.on("slide", function (slideEvt) {
                $("#rightVal").val(ogWidth - slideEvt.value);
                changeImage();
            });
            //top(on the left) - now controls bottom
            topSlider.bootstrapSlider("setAttribute", "max", ogHeight);
            topSlider.bootstrapSlider("refresh");
            topSlider.bootstrapSlider("setValue", ogHeight - $("#bottomVal").val());
            topSlider.off("slide");
            topSlider.on("slide", function (slideEvt) {
                $("#bottomVal").val(ogHeight - slideEvt.value);
                changeImage();
            });
            break;
        case 270:
            //right(on the top) - now controls bottom
            rightSlider.bootstrapSlider("setAttribute", "max", ogHeight);
            rightSlider.bootstrapSlider("refresh");
            rightSlider.bootstrapSlider("setValue", $("#bottomVal").val());
            rightSlider.off("slide");
            rightSlider.on("slide", function (slideEvt) {
                $("#bottomVal").val(slideEvt.value);
                changeImage();
            });
            //bottom(on the right) - now controls left
            bottomSlider.bootstrapSlider("setAttribute", "max", ogWidth);
            bottomSlider.bootstrapSlider("refresh");
            bottomSlider.bootstrapSlider("setValue", ogWidth - $("#leftVal").val());
            bottomSlider.off("slide");
            bottomSlider.on("slide", function (slideEvt) {
                $("#leftVal").val(ogWidth - slideEvt.value);
                changeImage();
            });
            //left(on the bottom) - now controls top
            leftSlider.bootstrapSlider("setAttribute", "max", ogHeight);
            leftSlider.bootstrapSlider("refresh");
            leftSlider.bootstrapSlider("setValue", $("#topVal").val());
            leftSlider.off("slide");
            leftSlider.on("slide", function (slideEvt) {
                $("#topVal").val(slideEvt.value);
                changeImage();
            });
            //top(on the left) - now control right
            topSlider.bootstrapSlider("setAttribute", "max", ogWidth);
            topSlider.bootstrapSlider("refresh");
            topSlider.bootstrapSlider("setValue", ogWidth - $("#rightVal").val());
            topSlider.off("slide");
            topSlider.on("slide", function (slideEvt) {
                $("#rightVal").val(ogWidth - slideEvt.value);
                changeImage();
            });
            break;

    }

    changeImage();
}

$(document).ready(function () {

    var ogWidth = parseInt($("#ogImageWidth").val());
    var ogHeight = parseInt($("#ogImageHeight").val());

    var leftcrop = parseInt(document.getElementById("leftVal").value);
    var topcrop = parseInt(document.getElementById("topVal").value);
    var rightcrop = parseInt(document.getElementById("rightVal").value);
    var bottomcrop = parseInt(document.getElementById("bottomVal").value);

    var rot = parseInt(document.getElementById("rotVal").value);
    var rot2 = parseFloat(document.getElementById("rot2Val").value);

    rotationSlider = $('#rotationSlider').bootstrapSlider({
        min: -10,
        max: 10,
        value: rot2,
        step: 0.1,
        //handle: "custom",
        formatter: function (value) {
            return value + "°";
        }
    });
    rotationSlider.on("slide", function (slideEvt) {
        $("#rot2Val").val(slideEvt.value);
        changeImage();
    });
    leftSlider = $('#leftSlider').bootstrapSlider({
        min: 0,
        max: ogWidth,
        value: leftcrop,
        step: 1,
        //handle: "custom",
        formatter: function (value) {
            return value + "px";
        }
    });
    leftSlider.on("slide", function (slideEvt) {
        $("#leftVal").val(slideEvt.value);
        changeImage();
    });
    rightSlider = $('#rightSlider').bootstrapSlider({
        min: 0,
        max: ogWidth,
        value: rightcrop,
        step: 1,
        //handle: "custom",
        formatter: function (value) {
            return value + "px";
        },
    });
    rightSlider.on("slide", function (slideEvt) {
        $("#rightVal").val(slideEvt.value);
        changeImage();
    });
    topSlider = $('#topSlider').bootstrapSlider({
        min: 0,
        max: ogHeight,
        value: topcrop,
        step: 1,
        //handle: "custom",
        orientation: 'vertical',
        formatter: function (value) {
            return value + "px";
        }
    });
    topSlider.on("slide", function (slideEvt) {
        $("#topVal").val(slideEvt.value);
        changeImage();
    });
    bottomSlider = $('#bottomSlider').bootstrapSlider({
        min: 0,
        max: ogHeight,
        value: bottomcrop,
        step: 1,
        //handle: "custom",
        orientation: 'vertical',
        tooltip_position: 'left',
        formatter: function (value) {
            return value + "px";
        }
    });
    bottomSlider.on("slide", function (slideEvt) {
        $("#bottomVal").val(slideEvt.value);
        changeImage();
    });

    //make sure to rotate the sliders to match initial rotation
    var rot = parseInt(document.getElementById("rotVal").value);
    setRotation(rot);

    //load the cropped/rotated image
    changeImage();
    var aDown = false;
    var sDown = false;
    var dDown = false;
    $("body").keydown(function (event) {

        switch (event.which) {
            case 65://a
                aDown = true;
                break;
            case 83://s
                sDown = true;
                break;
            case 68://d
                dDown = true;
                break;
        }
        //console.log(event.which);
    });

    $("body").keypress(function (event) {

        //console.log(event.which);
    });

    $("body").keyup(function (event) {

        console.log(event.which);

        var currentrot = parseInt(document.getElementById("rotVal").value);
        switch (event.which) {

            case 65://a
                aDown = false;
                break;
            case 83://s
                sDown = false;
                break;
            case 68://d
                sDown = false;
                break;
            case 74://j

                if (aDown && sDown) {
                    //move left slider left
                    var left = leftSlider.bootstrapSlider("getValue");
                    leftSlider.bootstrapSlider("setValue", left - 1, true, true);
                    setZoomXY(0, $("#theimage").height() / 2, 200, 0)
                }
                else if (dDown && sDown) {
                    //move right slider left
                    var right = rightSlider.bootstrapSlider("getValue");
                    rightSlider.bootstrapSlider("setValue", right - 1, true, true);
                    setZoomXY($("#theimage").width(), $("#theimage").height() / 2, -200, 0)
                }
                else if (aDown) {
                    //move left slider left
                    var left = leftSlider.bootstrapSlider("getValue");
                    leftSlider.bootstrapSlider("setValue", left - 10, true, true);
                    setZoomXY(0, $("#theimage").height() / 2, 200, 0)
                }
                else if (dDown) {
                    //move right slider left
                    var right = rightSlider.bootstrapSlider("getValue");
                    rightSlider.bootstrapSlider("setValue", right - 10, true, true);
                    setZoomXY($("#theimage").width(), $("#theimage").height() / 2, -200, 0)
                }
                else {
                    currentrot -= 90;
                }
                break;
            case 75://k
                currentrot += 180;
                break;
            case 76://l

                if (aDown && sDown) {
                    //move left slider right
                    var left = leftSlider.bootstrapSlider("getValue");
                    leftSlider.bootstrapSlider("setValue", left + 1, true, true);//https://stackoverflow.com/a/47411403
                    setZoomXY(0, $("#theimage").height() / 2, 200, 0)
                }
                else if (dDown && sDown) {
                    //move right slider right
                    var right = rightSlider.bootstrapSlider("getValue");
                    rightSlider.bootstrapSlider("setValue", right + 1, true, true);//https://stackoverflow.com/a/47411403
                    setZoomXY($("#theimage").width(), $("#theimage").height() / 2, -200, 0)
                }
                else if (dDown) {
                    //move right slider right
                    var right = rightSlider.bootstrapSlider("getValue");
                    rightSlider.bootstrapSlider("setValue", right + 10, true, true);//https://stackoverflow.com/a/47411403
                    setZoomXY($("#theimage").width(), $("#theimage").height() / 2, -200, 0)
                }
                else if (aDown) {
                    //move left slider right
                    var left = leftSlider.bootstrapSlider("getValue");
                    leftSlider.bootstrapSlider("setValue", left + 10, true, true);//https://stackoverflow.com/a/47411403
                    setZoomXY(0, $("#theimage").height() / 2, 200, 0)
                }
                else {
                    currentrot += 90;
                }
                break;
        }


        setRotation(currentrot);
    });

    $('#prevthumb').on("mousemove", function (e) {
        var img = document.getElementById("thethumb");
        var thumb = document.getElementById("prevthumb");

        if (img.src != thumb.src) {
            img.src = thumb.src.replace("imagethumb", "image");
        }
    });
    $('#thisthumb').on("mousemove", function (e) {
        var img = document.getElementById("thethumb");
        var thumb = document.getElementById("thisthumb");

        if (img.src != thumb.src) {
            img.src = thumb.src.replace("imagethumb", "image");
        }
    });
    $('#nextthumb').on("mousemove", function (e) {
        var img = document.getElementById("thethumb");
        var thumb = document.getElementById("nextthumb");

        if (img.src != thumb.src) {
            img.src = thumb.src.replace("imagethumb","image");
        }
    });

    //hover for closeup of main image
    $('#theimage').on("mousemove", function (e) {
        var offset = $(this).offset();
        //screen image x,y
        var CX = Math.floor((e.pageX - offset.left));
        var CY = Math.floor((e.pageY - offset.top));
        setZoomXY(CX, CY, 0, 0);

    });

    $("#target").mouseup(function () {
        alert("Handler for .mouseup() called.");
    });


    $("#btnSaveCrop").on("click",function (e) {
        // here's where you stop the default submit action of the form
        e.preventDefault();

        //collect all our image properties
        imgData.Image = $("#imageId").val();
        imgData.Rotate = parseInt($("#rotVal").val());
        imgData.Rotate2 = parseFloat($("#rot2Val").val());
        imgData.LeftCrop = parseInt($("#leftVal").val());
        imgData.TopCrop = parseInt($("#topVal").val());
        imgData.RightCrop = parseInt($("#rightVal").val());
        imgData.BottomCrop = parseInt($("#bottomVal").val());
        thedata = JSON.stringify(imgData);
        // Now execute your AJAX
        $.ajax({
            type: "POST",
            url: "/home/updateimage",
            data: thedata,
            dataType: 'json',
            contentType: 'application/json; charset=utf-8'
        }).done(function (response) {
            // handle a successful response
            Toast.create("Success", "Saved!", TOAST_STATUS.SUCCESS, 2500);
        }).fail(function (xhr, status, message) {
            // handle a failure response
            alert("boo");
        });
    });

    $("#whoWhatUpdate").on("click",function (e) {

        saveComment("imageId", "WhoWhat");

    });

    $("#whenUpdate").on("click", function (e) {

        saveComment("imageId", "When");

    });

    $("#whereUpdate").on("click", function (e) {

        saveComment("imageId", "Where");

    });

    $("#whyHowUpdate").on("click", function (e) {

        saveComment("imageId", "WhyHow");

    });

    function saveComment(imageId, commentBox) {

        var whoWhat = {};

        //collect all our comment properties
        whoWhat.Image = $("#" + imageId).val();
        whoWhat.Comment = $("#" + commentBox).val();
        whoWhat.Type = commentBox;
        var thedata = JSON.stringify(whoWhat);

        // Now execute your AJAX
        $.ajax({
            type: "POST",
            url: "/home/updatecomment",
            data: thedata,
            dataType: 'json',
            contentType: 'application/json; charset=utf-8'
        }).done(function (response) {
            // handle a successful response
            Toast.create("Success", "Saved " + commentBox + "!", TOAST_STATUS.SUCCESS, 2500);
        }).fail(function (xhr, status, message) {
            // handle a failure response
            alert("boo");
        });
    }
});


function setZoomXY(CX, CY, TX, TY) {

    //fullsize image crops
    var leftcrop = parseInt(document.getElementById("leftVal").value);
    var topcrop = parseInt(document.getElementById("topVal").value);
    var rightcrop = parseInt(document.getElementById("rightVal").value);
    var bottomcrop = parseInt(document.getElementById("bottomVal").value);

    var rot = parseInt(document.getElementById("rotVal").value);
    var rot2 = parseFloat(document.getElementById("rot2Val").value);

    //image width on screen
    var screenwidth = $("#theimage").width();
    var screenheight = $("#theimage").height();

    //fullsize image cropped
    var croppedwidth = rightcrop - leftcrop;
    var croppedheight = bottomcrop - topcrop;

    //fullsize image uncropped
    var fullwidth = parseInt($("#ogImageWidth").val());
    var fullheight = parseInt($("#ogImageHeight").val());

    //fullsize image crop/margins
    var leftmargin = leftcrop;
    var topmargin = topcrop;

    //half the size of the width/height of the zoomed image
    var halfsize = 200;

    //find crops in fullsize image based on screen x,y
    var tmpleftcrop = leftmargin + Math.floor((CX / screenwidth) * croppedwidth) - halfsize;
    var tmptopcrop = topmargin + Math.floor((CY / screenheight) * croppedheight) - halfsize;
    var tmprightcrop = leftmargin + Math.floor((CX / screenwidth) * croppedwidth) + halfsize;
    var tmpbottomcrop = topmargin + Math.floor((CY / screenheight) * croppedheight) + halfsize;

    if (rot == 90) {
        //rotate right
        tmpleftcrop = leftmargin + Math.floor((CY / screenheight) * croppedwidth) - halfsize;
        tmptopcrop = topmargin + Math.floor((screenwidth - CX) / screenwidth * croppedheight) - halfsize;
        tmprightcrop = leftmargin + Math.floor((CY / screenheight) * croppedwidth) + halfsize;
        tmpbottomcrop = topmargin + Math.floor((screenwidth - CX) / screenwidth * croppedheight) + halfsize;
    } else if (rot == 180) {
        tmpleftcrop = leftmargin + Math.floor((screenwidth - CX) / screenwidth * croppedwidth) - halfsize;
        tmptopcrop = topmargin + Math.floor(((screenheight - CY) / screenheight) * croppedheight) - halfsize;
        tmprightcrop = leftmargin + Math.floor((screenwidth - CX) / screenwidth * croppedwidth) + halfsize;
        tmpbottomcrop = topmargin + Math.floor(((screenheight - CY) / screenheight) * croppedheight) + halfsize;
    } else if (rot == 270) {
        tmpleftcrop = leftmargin + Math.floor(((screenheight - CY) / screenheight) * croppedwidth) - halfsize;
        tmptopcrop = topmargin + Math.floor(CX / screenwidth * croppedheight) - halfsize;
        tmprightcrop = leftmargin + Math.floor(((screenheight - CY) / screenheight) * croppedwidth) + halfsize;
        tmpbottomcrop = topmargin + Math.floor(CX / screenwidth * croppedheight) + halfsize;
    }

    //use TX,TY to translate the crop
    tmpleftcrop += TX;
    tmprightcrop += TX;
    tmptopcrop += TY;
    tmpbottomcrop += TY;

    //make sure we don't go over the edge
    if (tmpleftcrop < 0) { tmpleftcrop = 0; }
    if (tmprightcrop > fullwidth) { tmprightcrop = fullwidth; }
    if (tmptopcrop < 0) { tmptopcrop = 0; }
    if (tmpbottomcrop > fullheight) { tmpbottomcrop = fullheight; }

    var imgQuery = "?" + encodeURI("crop=" + tmpleftcrop + "," + tmptopcrop + "," + tmprightcrop + "," + tmpbottomcrop + "&rot=" + rot + "&rot2=" + rot2);

    //$('#coords').text(imgQuery);

    var imgId = $("#imageId").val();
    var img = document.getElementById("thethumb");
    img.src = window.location.origin + "/home/image/" + imgId + imgQuery;
}

