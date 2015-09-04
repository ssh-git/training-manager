(($: JQueryStatic) => {
    'use strict';

    $.fn.tmTrainingProvider = (availableUpdateDates: string[], selectedUpdateDate: string, options: TmTrainingProviderOptions) => {
        var dateFormat = 'yy-mm-dd',
            previousUpdateDate: string,

            defaults: TmTrainingProviderOptions = {
                datePickerSelector: '#tm-modal-datepicker',
                modalSelector: '#tm-date-select-modal',
                modalOkButtonSelector: '#tm-modal-applay-date',
                selectedDateContainerSelector: '#tm-ui-selected-date',

                datePickerSettings: {
                    dateFormat: dateFormat,
                    defaultDate: selectedUpdateDate,
                    onSelect(dateText) {
                        selectedUpdateDate = dateText;
                    },
                    beforeShowDay(date) {
                        const currentDate = $.datepicker.formatDate(dateFormat, date);
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

            settings: TmTrainingProviderOptions = $.extend(true, {}, defaults, options),
            coursesContainer = $('#partialContent'),

            loadCourses = (url: string) => {
                coursesContainer.load(url, () => {
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

        $(settings.modalSelector).on('show.bs.modal', () => {
            previousUpdateDate = selectedUpdateDate;
            $(settings.datePickerSelector).datepicker('setDate', selectedUpdateDate);

            $(settings.modalOkButtonSelector).one('click', () => {
                if (selectedUpdateDate !== previousUpdateDate) {
                    $(settings.selectedDateContainerSelector).text(selectedUpdateDate);
                    const url = coursesContainer.attr('data-url').replace(previousUpdateDate, selectedUpdateDate);
                    coursesContainer.attr('data-url', url);

                    loadCourses(url);
                }
                $(settings.modalSelector).modal('hide');
            });
        });
    };
})(jQuery);
