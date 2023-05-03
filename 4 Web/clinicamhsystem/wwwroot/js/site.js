// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.getElementById("toggle-button").addEventListener("click", function () {
    var sidebar = document.querySelector(".menu");
    var toggleButton = document.querySelector(".toggle-button");
    var container = document.querySelector(".container-larger");
    if (sidebar.classList.contains("hidden")) {
        sidebar.classList.remove("hidden");
        toggleButton.classList.remove("hidden-tgb");
        container.classList.remove("hidden");
    } else {
        sidebar.classList.add("hidden");
        container.classList.add("hidden");
        toggleButton.classList.add("hidden-tgb");
    }
});