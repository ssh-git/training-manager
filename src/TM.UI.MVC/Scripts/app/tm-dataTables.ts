(($: JQueryStatic) => {
    'use strict';

    var defaultSettings: TmDtOptions = {
        statisticCallbacks: {
            onAllControlClick() {
            },
            onAviableControlClick() {
            },
            onPlannedControlClick() {
            },
            onStartedControlClick() {
            },
            onFinishedControlClick() {
            }
        },
        selectors: {
            statisticContainer: '#statistic-container',
            ajaxActions: '[data-actions]',
            controlColumn: '#control-column',
            tokenColumn: '#token-column'
        }
    };

    $.fn.tmDataTable = function (options?: TmDtOptions) {
        var initPopover = (dtSettings: DataTables.Settings) => {
            const api: DataTables.DataTable = new $.fn.dataTable.Api(dtSettings);
            api.$('[data-toggle=popover]').each((index, element) => {
                $(element).popover();
            });
        },
            dtDefauls: DataTables.Settings = {
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
            settings: TmDtOptions = $.extend(true, {}, defaultSettings, { dtSettings: dtDefauls }, options),
            dtApi = $(this).DataTable(settings.dtSettings);

        $.fn.dataTable.ext.order['dom-numeric-order'] = (dtsettings: DataTables.Settings, col: any) =>
            dtApi.column(col, { order: 'index' })
                .nodes()
                .map(td => (<any>$(td).attr('data-order') * 1));

        return new App.TmDtController(dtApi, settings.selectors, settings.statisticCallbacks);
    };

    $.fn.tmDataTableAjax = function (options?: TmDtOptions) {
        var initPopover = (dtSettings: DataTables.Settings) => {
            const api: DataTables.DataTable = new $.fn.dataTable.Api(dtSettings);
            api.$('[data-toggle=popover]').each((index, element) => {
                $(element).popover();
            });
        },
            dtDefauls: DataTables.Settings = {
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
            settings: TmDtOptions = $.extend(true, {}, defaultSettings, { dtSettings: dtDefauls }, options),
            dtApi = $(this).DataTable(settings.dtSettings);

        return new App.TmDtController(dtApi, settings.selectors, settings.statisticCallbacks);
    };
})(jQuery);