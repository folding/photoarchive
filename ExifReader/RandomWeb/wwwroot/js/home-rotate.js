// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var imgData = {};


function changeImage() {

    var imgId = $("#imageId").val();
    var img = document.getElementById("theimage");
    var rot = document.getElementById("rotVal").value;
    var imgTransformation = document.getElementById("imgTransformation").value;
    var screensize = "medium";
    if ($(window).width() < 1280) {
        screensize = "large";
    }


    var imgId = $("#imageId").val();

    var imgQuery = "?" + encodeURI(imgTransformation + "&rot=" + rot);// + "&v=" + (621355968e9 + (new Date()).getTime() * 1e4));

    img.src = window.location.origin + "/image/" + screensize + "/" + imgId + imgQuery;
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

    var currentrot = parseInt(document.getElementById("rotVal").value);
    if (currentrot != rot) {
        $("#rotVal").val(rot);

        saveRotation();
    }
    changeImage();
}

function saveRotation() {

    //collect all our image properties
    imgData.Image = $("#imageId").val();
    imgData.Rotate = parseInt($("#rotVal").val());
    thedata = JSON.stringify(imgData);
    // Now execute your AJAX
    $.ajax({
        type: "POST",
        url: "/home/updaterotation",
        data: thedata,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8'
    }).done(function (response) {
        // handle a successful response
        Toast.create("Success", "Rotation Saved!", TOAST_STATUS.SUCCESS, 2500);
    }).fail(function (xhr, status, message) {
        // handle a failure response
        alert("boo");
    });
}

function confirmRotation() {

    //collect all our image properties
    imgData.Image = $("#imageId").val();
    imgData.Rotate = parseInt($("#rotVal").val());
    thedata = JSON.stringify(imgData);
    // Now execute your AJAX
    $.ajax({
        type: "POST",
        url: "/home/confirmrotation",
        data: thedata,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8'
    }).done(function (response) {
        // handle a successful response
        Toast.create("Success", "Rotation Saved!", TOAST_STATUS.SUCCESS, 2500);
        window.location = window.location;
    }).fail(function (xhr, status, message) {
        // handle a failure response
        alert("boo");
    });
}

$(document).ready(function () {
    //load the cropped/rotated image
    changeImage();

    $("body").keyup(function (event) {

        console.log(event.which);

        var currentrot = parseInt(document.getElementById("rotVal").value);
        switch (event.which) {
            case 74://j - rotate left
                currentrot -= 90;
                break;
            case 75://k - rotate 180
                currentrot += 180;
                break;
            case 76://l - rotate right
                currentrot += 90;
                break;
            case 82://r - refresh
            case 70://f - refresh
                window.location = window.location;
                break;
            case 83://s - save rotation
                confirmRotation();
                break;
        }

        setRotation(currentrot);
    });


     

});
