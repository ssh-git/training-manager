(function ($) {
    'use strict';
    $.fn.tmCourseAddController = function (options) {
        var defaults = {
            selectors: {
                addCourseControl: '[data-action=add]',
                courseNameColumn: '#course-name-column',
                tokenColumn: '#token-column'
            },
            tokens: {
                add: 'plan'
            },

            markers: {
                available: { value: 'A', css: 'label-info', title: 'Available' },
                add: { value: 'P', css: 'label-warning', title: 'Planned' }
            }
        },
            settings = $.extend(true, {}, defaults, options),


            dt = $(this).tmDataTable(options),

            onAddCourse = function (event) {
                var addControl = $(event.currentTarget),
                    tableRow = dt.getTableRow(addControl),
                    action = dt.getActionUrl('add'),
                    requestData = dt.getRequestData(tableRow);

                dt.postRequest(action, requestData)
                    .done(function () {
                        var markerNode = $('span', dt.api.cell(tableRow, settings.selectors.courseNameColumn).node()),
                            addMarker = settings.markers.add,
                            availableMarker = settings.markers.available;

                        markerNode.text(addMarker.value);
                        markerNode.removeClass(availableMarker.css);
                        markerNode.addClass(addMarker.css);
                        markerNode.attr('title', addMarker.title);

                        addControl.addClass('disabled');

                        dt.api.cell(tableRow, settings.selectors.tokenColumn).data(settings.tokens.add);

                        dt.calculateStatistic();
                        dt.api.draw(false);

                        dt.notifySuccess('Added');
                    })
                    .fail(dt.processError);

                return false;
            };

        $('tbody').on('click', settings.selectors.addCourseControl, onAddCourse);
    };
})(jQuery);