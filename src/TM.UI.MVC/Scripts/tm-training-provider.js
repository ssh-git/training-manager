(function($) {

    $.fn.tmTrainingProvider = function (availableUpdateDates, selectedUpdateDate, options) {
        'use strict';

        var dateFormat = 'yy-mm-dd',
            previousUpdateDate,

            defaults = {
                datePickerSelector: '#tm-modal-datepicker',
                modalSelector: '#tm-date-select-modal',
                modalOkButtonSelector: '#tm-modal-applay-date',
                selectedDateContainerSelector: '#tm-ui-selected-date',

                datePickerSettings: {
                    dateFormat: dateFormat,
                    defaultDate: selectedUpdateDate,
                    onSelect: function(dateText) {
                        selectedUpdateDate = dateText;
                    },
                    beforeShowDay: function (date) {
                        var currentDate = $.datepicker.formatDate(dateFormat, date);
                        if ($.inArray(currentDate, availableUpdateDates) !== -1) {
                            if (currentDate === selectedUpdateDate) {
                                return [true, 'tm-selected-background'];
                            }
                            return [true];
                        } else {
                            return [false];
                        }
                    }
                }
            },

            settings = $.extend({}, defaults, options),
            coursesContainer = $('#partialContent'),
            loadCourses = function(url) {
                coursesContainer.load(url, function () {
                    $('table', coursesContainer).tmCourseAddController({
                        dtSettings: {
                            order: [[5, 'desc']],
                            pageLength: 10
                        }
                    });
                });
            };

        
        loadCourses(coursesContainer.data('url'));

        $(settings.datePickerSelector).datepicker(settings.datePickerSettings);

        $(settings.modalSelector).on('show.bs.modal', function () {
            previousUpdateDate = selectedUpdateDate;
            $(settings.datePickerSelector).datepicker('setDate', selectedUpdateDate);

            $(settings.modalOkButtonSelector).one('click', function () {
                if (selectedUpdateDate !== previousUpdateDate) {
                    $(settings.selectedDateContainerSelector).text(selectedUpdateDate);

                    var url = coursesContainer.attr('data-url').replace(previousUpdateDate, selectedUpdateDate);
                    coursesContainer.attr('data-url', url);

                    loadCourses(url);
                }
                $(settings.modalSelector).modal('hide');
            });
        });

    };
})(jQuery);
