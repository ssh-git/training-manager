    (($: JQueryStatic) => {
        'use strict';

        $.fn.tmDomSearch = function (sourceContainer: JQuery, resultContainer: JQuery, options?: TmDomSearchOptions) {
            const defaults: TmDomSearchOptions = {
                resultTitle: 'Filtered content',
                countToActivate: 2
            };
            var settings: TmDomSearchOptions = $.extend({}, defaults, options),
                resultHeader = $('<h4></h4>').text(settings.resultTitle + ':'),
                targets = $('[data-tm-search=clone]', sourceContainer),
                nodesToHide = $('[data-tm-search=hide]', sourceContainer);

            if (!nodesToHide.length) {
                nodesToHide = sourceContainer;
            }

            this.keyup(function () {
                var searchString: string = $(this).val();

                resultContainer.hide().empty();
                nodesToHide.show();

                if (searchString && searchString.length >= settings.countToActivate) {
                    var innerContainer = $(document.createElement('div'))
                        .addClass(resultContainer.attr('class'));

                    targets.each((index, elem) => {
                        var block = $(elem),
                            match = $('[data-tm-search=target]', block).text().search(new RegExp(searchString, 'i')) !== -1;

                        if (match) {
                            innerContainer.append(block.clone());
                        }
                    });

                    resultContainer.append(resultHeader, innerContainer)
                        .show();

                    nodesToHide.hide();
                }
            });
        };
    })(jQuery);