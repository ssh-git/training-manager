module App {
    'use strict';

    export class TmDtController {
        statisticContainer: JQuery;

        constructor(public api: DataTables.DataTable, public selectors: TmDtSelectors, public callbacks: TmDtStatisticsCallbacks) {
            this.statisticContainer = $(selectors.statisticContainer);
            if (this.statisticContainer.length) {
                this.statisticContainer.on('click', 'a', this.onStatisticControlClick);
                this.calculateStatistic();
            }
        }

        onStatisticControlClick = (event: JQueryEventObject) => {
            var target = $(event.target),
                token = target.data('token');

            switch (token) {
                case 'available':
                    this.callbacks.onAviableControlClick();
                    break;
                case 'plan':
                    this.callbacks.onPlannedControlClick();
                    break;
                case 'start':
                    this.callbacks.onStartedControlClick();
                    break;
                case 'finish':
                    this.callbacks.onFinishedControlClick();
                    break;
                case '':
                    this.callbacks.onAllControlClick();
                    break;
            };

            target.blur();

            $('li.active', this.statisticContainer).removeClass('active');
            target.closest('li').addClass('active');

            this.api.column(this.selectors.tokenColumn).search(token).draw();

            return false;
        }

        antiforgeryToken: { name: string; value: string } = {
            name: '__RequestVerificationToken',
            value: $('[name=__RequestVerificationToken]').val()
        }

        calculateStatistic = () => {
            // ReSharper disable once AssignedValueIsNeverUsed
            var allCount = 0,
                availableCount = 0,
                planCount = 0,
                startCount = 0,
                finishCount = 0;

            this.api.column(this.selectors.tokenColumn).data().each((value: string) => {
                switch (value) {
                    case 'available':
                        ++availableCount;
                        break;
                    case 'plan':
                        ++planCount;
                        break;
                    case 'start':
                        ++startCount;
                        break;
                    case 'finish':
                        ++finishCount;
                        break;
                    default:
                        throw (value + ' token not supported');
                }
            });

            allCount = availableCount + planCount + startCount + finishCount;

            $('span', this.statisticContainer).each((index, element) => {
                var counter = $(element),
                    id = counter.attr('id');
                switch (id) {
                    case 'all-counter':
                        counter.text(allCount);
                        break;
                    case 'available-counter':
                        counter.text(availableCount);
                        break;
                    case 'planned-counter':
                        counter.text(planCount);
                        break;
                    case 'started-counter':
                        counter.text(startCount);
                        break;
                    case 'finished-counter':
                        counter.text(finishCount);
                        break;
                    default:
                        throw (id + ' identificator not supported');
                }
            });
        }

        getTableRow = (control: JQuery) => {
            var tableRow = control.closest('tr');
            if (tableRow.hasClass('child')) {
                tableRow = tableRow.prev();
            };
            return tableRow;
        }

        getActionUrl = (actionName: string) => {
            var actions = $(this.selectors.ajaxActions).data('actions'),
                targetAction: string = actions[actionName];

            if (!targetAction) {
                throw 'Could not find action ' + actionName + '. Provide attribute "data-actions" with "{actionName: actionUrl}" on the html "table" tag';
            }
            return targetAction;
        }

        getRequestData = (currentTableRow: JQuery) => {
            var courseId = $('input:hidden', this.api.cell(currentTableRow, this.selectors.controlColumn).node()).val(),
                formData = new FormData();

            formData.append('courseId', courseId);

            if (this.antiforgeryToken.value) {
                formData.append(this.antiforgeryToken.name, this.antiforgeryToken.value);
            }

            return formData;
        }

        postRequest = (url: string, formData: FormData) => {
            const request = $.ajax(
                {
                    url: url,
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false
                }
                );
            return request;
        }

        processError = (jqXHR: JQueryXHR) => {
            console.log(jqXHR.responseText);
            this.notifyError('Error.' + jqXHR.statusText);
        }

        notifyError = (message: string) => {
            const text = message || 'Error. See log for details.';
            $.notify(text, { autoHideDelay: 3000 });
        }

        notifySuccess = (message: string) => {
            $.notify(message, { autoHideDelay: 3000, className: 'success' });
        }
    }
}