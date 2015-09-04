(($: JQueryStatic) => {
    'use strict';

    $(() => {
        var timer: number,
            siteUp = $('#tm-site-up');

        $(window).scroll(() => {
            if (timer) {
                window.clearTimeout(timer);
                timer = null;
            }

            timer = window.setTimeout(() => siteUp.removeClass('on-scroll'), 400);
            if (window.scrollY < 275) {
                siteUp.css('visibility', 'hidden');
            } else {
                siteUp.css('visibility', 'visible').addClass('on-scroll');
            }
        });

        siteUp.click(() => $('html, body').animate({ scrollTop: 0 }));

        $('#tm-change-specializations').click(event => {
            window.location.href = $(event.target).attr('href') + '?ReturnUrl=' + window.location.pathname;
            return false;
        });
    });
})(jQuery);
