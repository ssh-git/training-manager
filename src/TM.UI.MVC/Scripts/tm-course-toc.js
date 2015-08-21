$(function () {
    'use strict';

    var toc = $('#toc'),
        tocBlock = $('#toc-block'),
        tocControl = $('#toc-link'),
        descriptionBlock = $('#description-block'),
        descriptionControl = $('#description-link'),

        collapseAllControl = $('#toc-collapse-all'),
        expandAllControl = $('#toc-expand-all'),

        onTocHeaderRowClick = function (event) {
            if (event.target.tagName !== 'I') {
                $(this).next('.panel-collapse').collapse('toggle');
            }

            return false;
        },

        onTocHeaderDescriptionControlClick = function () {
            if (descriptionBlock.is(':visible')) return false;

            tocBlock.hide();
            descriptionBlock.show();

            descriptionControl.toggleClass('active');
            tocControl.toggleClass('active');

            return false;
        },

        onTocHeaderClick = function () {
            if (tocBlock.is(':visible')) return false;

            descriptionBlock.hide();
            tocBlock.show();

            descriptionControl.toggleClass('active');
            tocControl.toggleClass('active');

            return false;
        },

        onExpandAllClick = function () {
            $('.collapse:not(.in)', toc).collapse('show');

            return false;
        },

        onCollapseAllClick = function () {
            $('.collapse.in', toc).collapse('hide');

            return false;
        },

        onTocExpand = function () {
            if (!$('.collapse:not(.in)', toc).length) {
                expandAllControl.hide();
                collapseAllControl.show();
            };
        },

        onTocCollapse = function () {
            if (!$('.collapse.in', toc).length) {
                collapseAllControl.hide();
                expandAllControl.show();
            };
        };


    $('[data-toggle=\'popover\']').popover();

    toc.on('click', '.panel-heading', onTocHeaderRowClick);
    descriptionControl.click(onTocHeaderDescriptionControlClick);
    tocControl.click(onTocHeaderClick);
    collapseAllControl.click(onCollapseAllClick);
    expandAllControl.click(onExpandAllClick);

    toc.on('shown.bs.collapse', onTocExpand);
    toc.on('hidden.bs.collapse', onTocCollapse);

    descriptionBlock.hide();
    collapseAllControl.hide();
});