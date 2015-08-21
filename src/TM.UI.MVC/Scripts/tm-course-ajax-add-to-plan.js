$(function () {
    'use strict';

    $('#tm-add-form').submit(function () {
        var container = $('#tm-form-container'),
            control = $(':submit', this),
            onSuccess = function () {
                control.hide();
                $('.alert-success', container).show(100).delay(2000).hide(function () {
                    $('#tm-form-container').empty();
                });
            },
            onError = function () {
                control.hide();
                $('.alert-danger', container).show(100).delay(2000).hide(function () {
                    control.prop('disabled', false);
                    control.show();
                });
            };

        control.prop('disabled', true);

        $.post(this.action, $(this).serialize(), onSuccess).error(onError);

        return false;
    });
});