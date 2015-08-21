(function ($) {
    $.fn.tmDataTable = function(options) {
        'use strict';

        var initPopover = function () {
                var api = this.api();
                api.$('[data-toggle=popover]').each(function (index, element) {
                    $(element).popover();
                });
            },

            defaults = {
            useAjaxTable: false,

            dtSettings: {
                orderCellsTop: true,
                columnDefs: [
                    { targets: 'tm-dt-dom-numeric-order', orderDataType: 'dom-numeric-order' },
                    { targets: 'tm-dt-not-orderable', orderable: false },
                    { targets: 'tm-dt-not-searchable', searchable: false }
                ],
                responsive: true,
                lengthMenu: [[5, 10, 25, 50, -1], [5, 10, 25, 50, 'All']],
                pageLength: 25,
                initComplete: initPopover
            },

            dtAjaxSettings: {
                pageLength: 25,
                lengthMenu: [[5, 10, 25, 50], [5, 10, 25, 50]],
                serverSide: true,
                processing: true,
                ajax: {
                    type: 'POST',
                    dataSrc: 'catalog'
                },
                autoWidth: false,
                columns: [
                    { data: 'trainingProvider' },
                    { data: 'category' },
                    { data: 'courseTitle' },
                    { data: 'authors', orderable: false },
                    { data: 'level' },
                    { data: 'duration' },
                    { data: 'releaseDate' },
                    { data: 'rating' }
                ],
                colVis: {
                    restore: 'Restore',
                    showAll: 'Show all'
                },

                drawCallback: initPopover
            },

            contorlClickCallbacks: {
                onAllControlClick: function() {},
                onAviableControlClick: function() {},
                onPlannedControlClick: function() {},
                onStartedControlClick: function() {},
                onFinishedControlClick: function() {}
            },

            selectors: {
                statisticContainer: '#statistic-container',
                ajaxActions: '[data-actions]',
                controlColumn: '#control-column',
                tokenColumn: '#token-column'
            }
        };

        var settings = $.extend(true, {}, defaults, options);
        var dtApi;

        if (settings.useAjaxTable) {
            dtApi = $(this).DataTable(settings.dtAjaxSettings);
        } else {
            dtApi = $(this).DataTable(settings.dtSettings);

            $.fn.dataTable.ext.order['dom-numeric-order'] = function(settings, col) {
                return dtApi.column(col, { order: 'index' }).nodes().map(function(td) {
                    return $(td).attr('data-order') * 1;
                });
            };
        }


        function actionNotFoundException(name) {
            this.actionName = name;
            this.message = 'Could not find action ' + name + '. Provide attribute "data-actions" with "{actionName: actionUrl}" on the html "table" tag';
            this.toString = function() {
                return this.message(this.actionName);
            };
        };


        // ReSharper disable once InconsistentNaming
        function TmDtController(dtApi, selectors, callbacks) {
            this.api = dtApi;
            this.selectors = selectors;

            this.statisticContainer = $(selectors.statisticContainer);

            var self = this;

            var onStatisticControlClick = function(event) {
                var target = $(event.currentTarget),
                    token = target.data('token');

                switch (token) {
                case 'available':
                    callbacks.onAviableControlClick();
                    break;
                case 'plan':
                    callbacks.onPlannedControlClick();
                    break;
                case 'start':
                    callbacks.onStartedControlClick();
                    break;
                case 'finish':
                    callbacks.onFinishedControlClick();
                    break;
                case '':
                    callbacks.onAllControlClick();
                    break;
                };

                target.blur();

                $('li.active', self.statisticContainer).removeClass('active');
                target.closest('li').addClass('active');

                dtApi.column(selectors.tokenColumn).search(token).draw();

                return false;
            };

            if (this.statisticContainer.length) {
                this.statisticContainer.on('click', 'a', onStatisticControlClick);

                this.calculateStatistic();
            }
        };

        TmDtController.prototype = {
            antiforgeryToken: {
                name: '__RequestVerificationToken',
                value: $('[name=__RequestVerificationToken]').val()
            },

            calculateStatistic: function () {
                // ReSharper disable once AssignedValueIsNeverUsed
                var allCount = 0,
                    availableCount = 0,
                    planCount = 0,
                    startCount = 0,
                    finishCount = 0;

                this.api.column(this.selectors.tokenColumn).data().each(function(value) {
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

                $('span', this.statisticContainer).each(function(index, element) {
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
            },

            getTableRow: function(control) {
                var tableRow = control.closest('tr');
                if (tableRow.hasClass('child')) {
                    tableRow = tableRow.prev();
                };
                return tableRow;
            },

            getActionUrl: function(actionName) {
                var actions = $(this.selectors.ajaxActions).data('actions'),
                    targetAction = actions[actionName];

                if (!targetAction) {
                    throw actionNotFoundException(actionName);
                }
                return targetAction;
            },

            getRequestData: function(currentTableRow) {
                var courseId = $('input:hidden', this.api.cell(currentTableRow, this.selectors.controlColumn).node()).val(),
                    formData = new FormData();

                formData.append('courseId', courseId);

                if (this.antiforgeryToken.value) {
                    formData.append(this.antiforgeryToken.name, this.antiforgeryToken.value);
                }

                return formData;
            },

            postRequest: function(url, formData) {
                var request = $.ajax(
                    {
                        url: url,
                        type: 'POST',
                        data: formData,
                        processData: false,
                        contentType: false
                    }
                );
                return request;
            },

            processError: function(jqXHR) {
                console.log(jqXHR.responseText);
                this.notifyError('Error.' + jqXHR.statusMessage);
            },

            notifyError: function(message) {
                var text = message || 'Error. See log for details.';
                $.notify(text, { autoHideDelay: 3000 });
            },

            notifySuccess: function(message) {
                $.notify(message, { autoHideDelay: 3000, className: 'success' });
            }
        };

        return new TmDtController(dtApi, settings.selectors, settings.contorlClickCallbacks);
    };
})(jQuery);