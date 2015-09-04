(($: JQueryStatic) => {
    'use strict';

    $(() => {
        var coursesContainer = $('#partialContent'),
            url = coursesContainer.data('url');

        coursesContainer.load(url, () => {
            $('table', coursesContainer).tmCourseAddController({
                dtSettings: {
                    order: [[5, 'desc']],
                    pageLength: 10
                }
            });
        });
    });
}) (jQuery)