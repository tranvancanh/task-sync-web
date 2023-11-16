// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    DatepickerRun();
});

function modalShow() {
    //モーダルを開く
    MicroModal.show('modal-2');
    document.body.classList.add("no-scroll");
    DatepickerRun();
}

function closeModal() {
    //モーダルを閉じる
    document.body.classList.remove("no-scroll");
}

function DatepickerRun() {
    // カレンダー
    $('.datepicker').on('click', function () {
        Datepicker(this);
    });
    // ブラウザのオートコンプリートをオフにする
    $('.datepicker').attr('autocomplete', 'off');
    // 数字が8文字なら日付形式に変換
    $('.datepicker').on('change blur', function () {
        let date = $.trim($(this).val());
        if (date.length === 8) {
            let year = date.substr(0, 4);
            let month = date.substr(4, 2);
            let day = date.substr(6, 2);
            let setDate = year + "/" + month + "/" + day;
            $(this).val(setDate);
        }
    });
}

function Datepicker(textBox) {
    //datepickerオプション 参考：https://www.sejuku.net/blog/44165
    $(textBox).datepicker({
        showAnim: 'fadeIn',
        dateFormat: 'yy/mm/dd'
    })
    $(textBox).trigger('focus');
}

function onlyNumberKey(evt) {
    let ch = String.fromCharCode(evt.which);
    if (!(/[0-9]/.test(ch))) {
        evt.preventDefault();
    }
}

function change_viewport_0(id) {
    //alert(id);
    let value = document.getElementById(id).value;
    if (value.trim().length == 0) {
        document.getElementById(id).value = 0;
    }
}