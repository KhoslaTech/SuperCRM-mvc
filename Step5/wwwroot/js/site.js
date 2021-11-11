// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function hideAlert(alertMessage) {
    alertMessage.style.display = "none";
}

$(document).ready(function () {
    if ($("#errorProvider").html()) {
        $("#alertMessage").css("display", "block");
    } else {
        $("#alertMessage").css("display", "none");
    }
});