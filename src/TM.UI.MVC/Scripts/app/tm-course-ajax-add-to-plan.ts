(($: JQueryStatic) => {
    'use strict';

    $(() => {
        $('#tm-add-form').submit((event) => {
            var container = $('#tm-form-container'),
                form = <HTMLFormElement>event.target,
                control = $(':submit', form),
                onSuccess = () => {
                    control.hide();
                    $('.alert-success', container)
                        .show(100)
                        .delay(2000)
                        .hide(() => $('#tm-form-container').empty());
                },
                onError = () => {
                    control.hide();
                    $('.alert-danger', container)
                        .show(100)
                        .delay(2000)
                        .hide(() => control.prop('disabled', false).show());
                };

            control.prop('disabled', true);

            $.post(form.action, $(form).serialize(), onSuccess)
                .fail(onError);

            return false;
        });
    });
})(jQuery);