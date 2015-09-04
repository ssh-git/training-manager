interface JQueryAjaxSuccess {
    (data: any, textStatus: string, jqXHR: JQueryXHR): any
}

interface JQueryAjaxError {
    (jqXHR: JQueryXHR, textStatus: string, errorThrown: string): any;
}

interface JQuery {
    tmDomSearch(sourceContainer: JQuery, resultContainer: JQuery, options?: TmDomSearchOptions): void;
    tmCourseAddController(options?: TmDtAddControllerSettings): void;
    tmDataTable(options?: TmDtOptions): App.TmDtController;
    tmDataTableAjax(options?: TmDtOptions): App.TmDtController;
    rating: Rating;
}

interface Rating {
    (par: string): JQuery;
    (options: Object): JQuery;
    (par: string, options: Object): JQuery;
}

interface TmDomSearchOptions {
    resultTitle?: string;
    countToActivate?: number;
}

interface TmTrainingProviderOptions {
    datePickerSelector?: string;
    modalSelector?: string;
    modalOkButtonSelector?: string;
    selectedDateContainerSelector?: string;
    datePickerSettings?: JQueryUI.DatepickerOptions;
}

interface TmDtMarker {
    value: string;
    css: string;
    title: string;
}

interface TmDtOptions {
    statisticCallbacks?: TmDtStatisticsCallbacks;
    dtSettings?: DataTables.Settings;
    selectors?: TmDtSelectors;
}

interface TmDtStatisticsCallbacks {
    onAllControlClick?: () => void;
    onAviableControlClick?: () => void;
    onPlannedControlClick?: () => void;
    onStartedControlClick?: () => void;
    onFinishedControlClick?: () => void;
}

interface TmDtSelectors {
    statisticContainer?: string;
    ajaxActions?: string;
    controlColumn?: string;
    tokenColumn?: string;
}

interface TmDtAddControllerSelectors extends TmDtSelectors {
    addCourseControl?: string;
    courseNameColumn?: string;
}

interface TmDtAddControllerSettings extends TmDtOptions {
    selectors?: TmDtAddControllerSelectors;
    tokens?: { add?: string }
    markers?: {
        available?: TmDtMarker;
        add?: TmDtMarker;
    }
} 