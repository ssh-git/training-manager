(($: JQueryStatic) => {
    'use strict';

    $(() => {

        var COURSE_NAME_COLUMN_SELECTOR = '#course-name-column',
            START_DATE_COLUMN_SELECTOR = '#start-date-column',
            FINISH_DATE_COLUMN_SELECTOR = '#finish-date-column',
            CONTROL_COLUMN_SELECTOR = '#control-column',

            PLAN_TOKEN = 'plan',
            START_TOKEN = 'start',
            FINISH_TOKEN = 'finish',

            MARKER_FOR_PLAN: TmDtMarker = { value: 'P', css: 'label-warning', title: 'Planned' },
            MARKER_FOR_START: TmDtMarker = { value: 'S', css: 'label-primary', title: 'Started' },
            MARKER_FOR_FINISH: TmDtMarker = { value: 'F', css: 'label-success', title: 'Finished' },

            TOKEN_COLUMN_SELECTOR = '#token-column',

            DELETE_COURSE_CONTROL_SELECTOR = '[data-action=delete]',
            START_COURSE_CONTROL_SELECTOR = '[data-action=start]',
            FINISH_COURSE_CONTROL_SELECTOR = '[data-action=finish]',
            RATE_COURSE_CONTROL_SELECTOR = '[data-action=rate]',

            dtApi: DataTables.DataTable,
            dt: App.TmDtController,

            courseRemoveModal = $('#tm-course-remove-modal'),
            removeModalDeleteButton = $('.modal-footer .btn-danger', courseRemoveModal),
            removeModalCourseTitle = $('.modal-body strong', courseRemoveModal),

            courseRateModal = $('#tm-course-rate-modal'),
            rateModalSaveButton = $('.modal-footer .btn-success', courseRateModal),

            onAllStatisticClick = () => {
                dtApi.column(7).visible(true);
                dtApi.column(8).visible(true);
            },

            onPlannedStatisticClick = () => {
                dtApi.column(7).visible(false);
                dtApi.column(8).visible(false);
            },

            onStartedStatisticClick = () => {
                dtApi.column(7).visible(true);
                dtApi.column(8).visible(false);
            },

            onFinishedStatisticClick = () => {
                dtApi.column(7).visible(false);
                dtApi.column(8).visible(true);
            },

            init = () => {
                dt = $('.tm-data-table').tmDataTable({
                    dtSettings: { colVis: { exclude: [9, 10, 11] } },
                    statisticCallbacks: {
                        onAllControlClick: onAllStatisticClick,
                        onPlannedControlClick: onPlannedStatisticClick,
                        onStartedControlClick: onStartedStatisticClick,
                        onFinishedControlClick: onFinishedStatisticClick
                    }
                });

                dtApi = dt.api;
            },

            resetState = (row: JQuery) => {
                var dtStartDateCell = dtApi.cell(row, START_DATE_COLUMN_SELECTOR),
                    dtFinishDateCell = dtApi.cell(row, FINISH_DATE_COLUMN_SELECTOR),
                    controlsCell = dtApi.cell(row, CONTROL_COLUMN_SELECTOR).node(),
                    markerNode = $('span', dtApi.cell(row, COURSE_NAME_COLUMN_SELECTOR).node());

                dtStartDateCell.data('');
                $(dtStartDateCell.node()).attr('data-order', 0);

                dtFinishDateCell.data('');
                $(dtFinishDateCell.node()).attr('data-order', 0);

                dtApi.cell(row, TOKEN_COLUMN_SELECTOR).data(PLAN_TOKEN);

                markerNode.text(MARKER_FOR_PLAN.value);
                markerNode.removeClass(MARKER_FOR_START.css);
                markerNode.removeClass(MARKER_FOR_FINISH.css);
                markerNode.addClass(MARKER_FOR_PLAN.css);
                markerNode.attr('title', MARKER_FOR_PLAN.title);


                $('button', controlsCell).hide();
                $('button[data-action=start]', controlsCell).show();
            },

            setStartState = (row: JQuery, date: string, dateOrdinal: string) => {
                var dtStartDateCell = dtApi.cell(row, START_DATE_COLUMN_SELECTOR),
                    controlsCell = dtApi.cell(row, CONTROL_COLUMN_SELECTOR).node(),
                    markerNode = $('span', dtApi.cell(row, COURSE_NAME_COLUMN_SELECTOR).node());

                dtStartDateCell.data(date);
                $(dtStartDateCell.node()).attr('data-order', dateOrdinal);
                dtApi.cell(row, TOKEN_COLUMN_SELECTOR).data(START_TOKEN);

                markerNode.text(MARKER_FOR_START.value);
                markerNode.removeClass(MARKER_FOR_PLAN.css);
                markerNode.addClass(MARKER_FOR_START.css);
                markerNode.attr('title', MARKER_FOR_START.title);

                $('button', controlsCell).hide();
                $('button[data-action=finish]', controlsCell).show();
            },

            setFinishState = (row: JQuery, date: string, dateOrdinal: string) => {
                var dtFinishDateCell = dtApi.cell(row, FINISH_DATE_COLUMN_SELECTOR),
                    controlsCell = dtApi.cell(row, CONTROL_COLUMN_SELECTOR).node(),
                    markerNode = $('span', dtApi.cell(row, COURSE_NAME_COLUMN_SELECTOR).node());

                dtFinishDateCell.data(date);
                $(dtFinishDateCell.node()).attr('data-order', dateOrdinal);
                dtApi.cell(row, TOKEN_COLUMN_SELECTOR).data(FINISH_TOKEN);

                markerNode.text(MARKER_FOR_FINISH.value);
                markerNode.removeClass(MARKER_FOR_START.css);
                markerNode.addClass(MARKER_FOR_FINISH.css);
                markerNode.attr('title', MARKER_FOR_FINISH.title);

                $('button', controlsCell).hide();
                $('button[data-action=rate]', controlsCell).show();
            },


            onStartLearning = (event: JQueryEventObject) => {
                var startControl = $(event.currentTarget),
                    tableRow = dt.getTableRow(startControl),
                    action = dt.getActionUrl('start'),
                    requestData = dt.getRequestData(tableRow);

                dt.postRequest(action, requestData)
                    .done((response: any) => {
                        setStartState(tableRow, response['changeDate'], response['sortOrdinal']);

                        dt.calculateStatistic();
                        dtApi.draw(false);

                        dt.notifySuccess('Started');
                    })
                    .fail(dt.processError);

                return false;
            },

            onFinishLearning = (event: JQueryEventObject) => {
                var finishControl = $(event.currentTarget),
                    tableRow = dt.getTableRow(finishControl),
                    action = dt.getActionUrl('finish'),
                    requestData = dt.getRequestData(tableRow);


                dt.postRequest(action, requestData)
                    .done((response: any) => {
                        setFinishState(tableRow, response['changeDate'], response['sortOrdinal']);

                        dt.calculateStatistic();
                        dtApi.draw(false);

                        dt.notifySuccess('Finished');
                    })
                    .fail(dt.processError);

                return false;
            },

            onRateCourse = (event: JQueryEventObject) => {
                var rateControl = $(event.currentTarget),
                    tableRow = dt.getTableRow(rateControl),
                    courseId = $('input:hidden', dtApi.cell(tableRow, CONTROL_COLUMN_SELECTOR).node()).val(),
                    url = courseRateModal.data('url') + '?courseId=' + courseId,

                    onStateChanged = function () {
                        var selectedState = $('option:selected', this).val();
                        switch (selectedState) {
                            case '1': // planned
                                $('#StartDate, #FinishDate, #Comment', courseRateModal).val(null).prop('disabled', true);
                                $('#OwnRating').rating('clear').rating('refresh', { disabled: true, showClear: false, showCaption: true });
                                break;
                            case '2': // in progress
                                $('#StartDate', courseRateModal).prop('disabled', false);

                                $('#FinishDate, #Comment', courseRateModal).val(null).prop('disabled', true);
                                $('#OwnRating').rating('clear').rating('refresh', { disabled: true, showClear: false, showCaption: true });
                                break;

                            case '3': // finished
                                $('#StartDate, #FinishDate, #Comment', courseRateModal).prop('disabled', false);
                                $('#OwnRating').rating('refresh', { disabled: false, showClear: true, showCaption: true });
                                break;
                        }
                    },

                    onSaveSuccessed: JQueryAjaxSuccess = (data, textStatus, jqXHR) => {
                        var newState = data.state;

                        if (data.isModelValid !== true) {
                            $('#validation-error-container', courseRateModal).text(data.errorMessage);
                            return false;
                        }

                        resetState(tableRow);

                        switch (newState) {
                            case 'InProgress':
                                setStartState(tableRow, data['startDate'], data['startDateOrdinal']);
                                break;
                            case 'Finished':
                                setStartState(tableRow, data['startDate'], data['startDateOrdinal']);
                                setFinishState(tableRow, data['finishDate'], data['finishDateOrdinal']);
                                break;
                        };

                        dt.calculateStatistic();
                        dtApi.draw(false);

                        courseRateModal.modal('hide');
                    },

                    onSaveFailed: JQueryAjaxError = (jqXHR, textStatus, errorThrown) => {
                        dt.notifyError('Error occurred.');
                    },

                    onSaveChanges = () => {
                        var form = $('form', courseRateModal);
                        $.post(form.attr('action'), form.serialize(), onSaveSuccessed)
                            .fail(onSaveFailed);
                    };

                rateModalSaveButton.on('click', onSaveChanges);

                $('.modal-body', courseRateModal).load(url, () => {
                    var rateForm = $('form', courseRateModal),
                        pickerOptions: JQueryUI.DatepickerOptions = {
                            constrainInput: true,
                            showButtonPanel: true,
                            changeMonth: true,
                            changeYear: true,
                            dateFormat: 'yy-mm-dd',
                            maxDate: 0,
                            beforeShow(input, inst) {
                                $('.ui-datepicker').css('font-size', 12);
                                return this;
                            }
                        };
                    if (rateForm.length) {
                        $('.tm-datepicker', courseRateModal).datepicker(pickerOptions);
                        $('.tm-state-dropdown', rateForm).change(onStateChanged);
                        $('#OwnRating').rating({ size: 'xs' });
                    }

                    courseRateModal.modal('show');
                });
            },

            onRemoveCourse = (event: JQueryEventObject) => {
                var removeControl = $(event.currentTarget),
                    tableRow = dt.getTableRow(removeControl),
                    courseTitle = $('a', dtApi.cell(tableRow, COURSE_NAME_COLUMN_SELECTOR).node()).text(),
                    action = dt.getActionUrl('delete'),
                    requestData = dt.getRequestData(tableRow),

                    onRemoveConfirmed = () => {
                        courseRemoveModal.modal('hide');
                        dt.postRequest(action, requestData)
                            .done(() => {
                                var dtRow = dtApi.row(tableRow);
                                if (tableRow.hasClass('parent')) {
                                    dtRow.child().remove();
                                }

                                dtRow.remove();
                                dtApi.draw(false);

                                dt.calculateStatistic();

                                dt.notifySuccess('Deleted');
                                if (dtApi.rows().flatten().length === 0) {
                                    window.location.reload(true);
                                }
                            })
                            .fail(dt.processError);
                    };

                removeModalCourseTitle.text(courseTitle);
                removeModalDeleteButton.on('click', onRemoveConfirmed);
                courseRemoveModal.modal('show');
            },

            onCourseRemoveModalHide = () => {
                removeModalDeleteButton.off();
            },

            onCourseRateModalHide = () => {
                $('.modal-body', courseRateModal).empty();
                rateModalSaveButton.off();
            };



        init();
        dt.calculateStatistic();


        $('tbody').on('click', START_COURSE_CONTROL_SELECTOR, onStartLearning);
        $('tbody').on('click', FINISH_COURSE_CONTROL_SELECTOR, onFinishLearning);
        $('tbody').on('click', RATE_COURSE_CONTROL_SELECTOR, onRateCourse);
        $('tbody').on('click', DELETE_COURSE_CONTROL_SELECTOR, onRemoveCourse);

        courseRemoveModal.on('hide.bs.modal', onCourseRemoveModalHide);
        courseRateModal.on('hide.bs.modal', onCourseRateModalHide);
    });
})(jQuery);
