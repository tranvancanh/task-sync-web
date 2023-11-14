// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
   
});

function modalShow() {
    //モーダルを開く
    MicroModal.show('modal-2');
    document.body.classList.add("no-scroll");
}

function closeModal() {
    //モーダルを閉じる
    document.body.classList.remove("no-scroll");
}



