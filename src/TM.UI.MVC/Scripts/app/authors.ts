(($: JQueryStatic) => {
    'use strict';

    $('#char-navigator').on('click', 'a', (event) => {
        const targetId = $(event.target).attr('href');
        $('html, body').animate({ scrollTop: $(targetId).offset().top });

        return false;
    });

    $('#authors-search-box').tmDomSearch($('#search-source'), $('#search-result'));
})(jQuery);
