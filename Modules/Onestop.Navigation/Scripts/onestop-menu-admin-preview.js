$(document).ready(function () {
    var removing = false;
    publishedList = $('#current-items > ol').nestedSortable({
        items: 'li',
        handle: '.wrapper > .header',
        dropOnEmpty: true,
        toleranceElement: '> div.wrapper',
        tolerance: 'pointer',
        helper: 'clone',
        opacity: .6,
        placeholder: 'placeholder',
        revert: 200,
        tabSize: 40,
        forcePlaceholderSize: true,
        connectWith: '.sortable',
        start: function (event, ui) {
        },
        receive: function (event, ui) {
            Update(event, ui, 0);
            removing = false;

        },
        remove: function (event, ui) {
            removing = true;
        },
        stop: function (event, ui) {
            if (ui.sender == undefined && !removing) Update(event, ui);
            removing = false;
        },
        disableNesting: 'no-nest'
    }).disableSelection();

    draftList = $('#draft-items > ol').nestedSortable({
        items: 'li',
        handle: '.wrapper > .header',
        receive: MakeDraft,
        dropOnEmpty: true,
        toleranceElement: '> div.wrapper',
        tolerance: 'pointer',
        helper: 'clone',
        opacity: .6,
        placeholder: 'placeholder',
        revert: 200,
        tabSize: 40,
        forcePlaceholderSize: true,
        connectWith: '.sortable',
        disableNesting: 'no-nest'
    }).disableSelection();

    var toggleCallback = function () {
        var item = $(this);
        var children = item.siblings(".itemlist").children("li");
        if (item.hasClass("toggle-expand")) {
            item.removeClass("toggle-expand");
            item.addClass("toggle-collapse");

            children.slideToggle(200);
        } else {
            if (item.hasClass("toggle-collapse")) {
                item.removeClass("toggle-collapse");
                item.addClass("toggle-expand");

                children.slideToggle(200);
            }
        }
    };

    var toggleChangedCallback = function () {
        var item = $(this);
        var siblings = item.siblings(".itemlist:has(.changed)");
        if (siblings.length > 0) {
            var children = siblings.children("li");
            if (item.hasClass("toggle-expand")) {
                item.removeClass("toggle-expand");
                item.addClass("toggle-collapse");


            } else {
                if (item.hasClass("toggle-collapse")) {
                    item.removeClass("toggle-collapse");
                    item.addClass("toggle-expand");
                }
            }

            children.slideToggle(200);
        }
    };

    var toggleExpandChangedCallback = function () {
        if ($(this).hasClass("collapse-changed")) {
            $(".changed, .removed").parents(".item").children(".toggle-collapse").each(toggleChangedCallback);
            $(this).addClass("expand-changed");
            $(this).removeClass("collapse-changed");
            $(this).html("Expand all");
        }
        else {
            $(".changed, .removed").parents(".item").children(".toggle-expand").each(toggleChangedCallback);
            $(this).addClass("collapse-changed");
            $(this).removeClass("expand-changed");
            $(this).html("Collapse all");
        }
    };

    var toggleExpandAllCallback = function () {
        if ($(this).hasClass("collapse-all")) {
            $(".toggle-collapse").each(toggleCallback);
            $(this).addClass("expand-all");
            $(this).removeClass("collapse-all");
            $(this).html("Expand all");
        }
        else {
            $(".toggle-expand").each(toggleCallback);
            $(this).addClass("collapse-all");
            $(this).removeClass("expand-all");
            $(this).html("Collapse all");
        }
    };

    $(".item > .itemlist > li").hide();
    $(".toggle").click(toggleCallback);
    $("#expand-all").click(toggleExpandAllCallback);
    $("#expand-changed").click(toggleExpandChangedCallback);

    $(".item .enter-position").click(function () {
        DisplayPositionForm($(this));
    });

    // Expand changed items by default.
    $("#expand-changed").click();

    var list = $("#published-items");
    var bar = $("#loading");
    if (list) {
        if (bar) {
            bar.fadeOut(500);
        }
        list.fadeIn(1000);
    }
});

$(function () {
    // SET-UP BOOTSTRAP FEATURES

    // Setup drop down menu
    //$('.dropdown-toggle').dropdown();

    // Fix input element click problem
    $('.dropdown-menu input, .dropdown-menu label, .dropdown-menu').click(function (e) {
        e.stopPropagation();
    });

    $('[rel=tooltip]').tooltip();

});

function Update(event, ui) {
    var newChildren = new Array();
    var parentId = 0;
    $(ui.item).removeClass('no-nest');
    $(ui.item).parent().parent('li').each(function () {
        parentId = $(this).data("id");
    });

    ui.item.parent().children('.item').each(function () {
        newChildren.push($(this).data("id"));
    });

    SendTreeUpdateRequest(ui.item, parentId, newChildren);
};

function SendTreeUpdateRequest(item, parentId, newChildren) {
    var moveUrl = $('#itemsheader').data("moveurl");
    var draftViewUrl = $('#itemsheader').data("draftviewurl");

    var ajaxData = ({
        __RequestVerificationToken: $('[name=__RequestVerificationToken]').attr('value'),
        newChildren: newChildren,
        parentId: parentId
    });

    $.ajax({
        type: 'POST',
        data: ajaxData,
        url: moveUrl,
        traditional: true
    }).done(function () {
        // We need to issue a second request to update initial place from which the item has been moved.
        var oldParentChildren = new Array();
        var oldParentId = $(item).data("parentid");
        var oldParentElement = $($(item).data("source"));

        if (oldParentElement.length && oldParentElement.data("updatesource").toLowerCase() === 'true') {
            oldParentElement.children('.item').each(function () {
                oldParentChildren.push($(this).data("id"));
            });

            if (oldParentChildren.length) {
                ajaxData = ({
                    __RequestVerificationToken: $('[name=__RequestVerificationToken]').attr('value'),
                    newChildren: oldParentChildren,
                    parentId: oldParentId
                });

                $.ajax({
                    type: 'POST',
                    data: ajaxData,
                    url: moveUrl,
                    traditional: true
                }).done(function () {
                    window.location.href = draftViewUrl;
                });
            }
            else {
                window.location.href = draftViewUrl;
            }
        } else {
            window.location.href = draftViewUrl;
        }
    });
}

function MakeDraft(event, ui) {
    if (EnsureDraftMode) {
        var itemId = $(ui.item).data("id");
        $(ui.item).addClass('no-nest');
        var draftUrl = $('#itemsheader').data("drafturl");
        var ajaxData = ({
            __RequestVerificationToken: $('[name=__RequestVerificationToken]').attr('value'),
            itemId: itemId
        });

        Async(draftUrl, ajaxData, 'POST');
    }
};
function DisplayPositionForm(item) {
    if (item.hasClass("enter-position")) {
        item.removeClass("enter-position");
        item.addClass("form-position");
        item.html(
            "<input type=\"textbox\" class=\"position-text\"></input>"
                + "<button type=\"submit\" class=\"submit\" onclick=\"SetPosition($(this));\">Save</button>&nbsp;"
                    + "<button type=\"submit\" class=\"submit\" onclick=\"CancelSetPosition($(this));\">Cancel</button>"
            );

        item.children(".submit-save").click(function () {
            SetPosition(item);
        });
        item.children(".submit-cancel").click(function () {
            CancelSetPosition(item);
        });
    }
    else {
        item.addClass("enter-position");
        item.removeClass("form-position");
    }
}


function SetPosition(item) {
    if (EnsureDraftMode) {
        var value = $(item).siblings(".position-text").val();
        var posUrl = $('#itemsheader').data("setpositionurl");
        var ajaxData = ({
            __RequestVerificationToken: $('[name=__RequestVerificationToken]').attr('value'),
            itemId: $(item).parent().parent().parent().parent().data("id"),
            position: value
        });

        var div = $(item).parent();

        div.html("Saving...");
        div.unbind('click');

        Async(posUrl, ajaxData, 'POST', function() {
            PositionSetSuccess(div, value);
            var draftViewUrl = $('#itemsheader').data("draftviewurl");
            window.location.href = draftViewUrl;
        }, function() {
            PositionSetError(div);
        });
    }
}

function PositionSetSuccess(item, value) {
    item.html("Position set to <b>" + value + "</b>.");
}

function PositionSetError(item) {
    item.html("<b>Error!</b><br/>Please try again.");
    item.unbind('click').click(function () {
        DisplayPositionForm(item);
    });
}

function CancelSetPosition(item) {
    item.parent().html("Click to edit");
    item.parent().unbind('click').click(function () {
        DisplayPositionForm(item.parent());
    });
}

function ScrollTo(element) {
    $('html, body').animate({
        scrollTop: $(element).offset().top
    }, 2000);
}

$(function () {
    $("[data-toggle]").each(function () {
        var controller = $("#" + $(this).attr("data-toggle"));
        var controlee = $(this);

        var hidden = $(this).attr("data-defaultstate") == "hidden";
        if (hidden) {
            $(this).hide();
        } else {
            $(this).hide().show();
        }

        controller.css("cursor", "pointer");
        controller.click(function () {
            controlee.slideToggle(100);
        });
    });
});