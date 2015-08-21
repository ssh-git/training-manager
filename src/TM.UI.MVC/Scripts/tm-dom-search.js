(function($) {
    'use strict';

    $.fn.tmDomSearch = function(sourceContainer, resultContainer, options) {
        var defaults = {
                resultTitle: 'Filtered content',
                countToActivate: 2
            },
            settings = $.extend({}, defaults, options),
            resultHeader = $('<h4></h4>').text(settings.resultTitle +':'),
            targets = $('[data-tm-search=clone]', sourceContainer),
            nodesToHide = $('[data-tm-search=hide]', sourceContainer);

        if (!nodesToHide.length) {
            nodesToHide = sourceContainer;
        }

        this.keyup(function () {
            var searchString = $(this).val();

            resultContainer.hide();
            resultContainer.empty();
            nodesToHide.show();

            if (searchString && searchString.length >= settings.countToActivate) {
                var innerContainer = $(document.createElement('div'));
                innerContainer.addClass(resultContainer.attr('class'));

                targets.each(function (index, node) {
                    var block = $(node),
                        match = $('[data-tm-search=target]', block).text().search(new RegExp(searchString, 'i')) !== -1;

                    if (match === true) {
                        innerContainer.append(block.clone());
                    }
                });

                resultContainer.append(resultHeader);
                resultContainer.append(innerContainer);

                resultContainer.show();
                nodesToHide.hide();
            }
        });
    };
})(jQuery);