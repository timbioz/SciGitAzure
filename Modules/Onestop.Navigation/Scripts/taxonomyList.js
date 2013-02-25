jQuery(function ($) {
    $('#taxonomyItems tbody').sortable({ update: update }).disableSelection();
    $('.grip').css('cursor','move');

    function update(event, ui) {
        var items = new Array();

        $(".itemRow").each(function () {
            items.push($(this).data("id"));
        });

        var ajaxData = {
            __RequestVerificationToken: $('[name=__RequestVerificationToken]').attr('value'),
            itemIds: items
        };

        $.ajax({
            type: 'POST',
            data: ajaxData,
            url: 'Reorder',
            traditional: true
        });
    };
});
