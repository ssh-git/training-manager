(($: JQueryStatic) => {
    'use strict';

    $('.tm-partial-block').each(function () {
        $(this).load($(this).data('url'));
    });

})(jQuery)