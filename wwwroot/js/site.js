// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function showLoader() {
    $('#loader').css('display', 'flex');
}
function hideLoader() {
    $('#loader').css('display', 'none');
}
function closePopup(id) {
    $('#' + id).modal('hide');
}