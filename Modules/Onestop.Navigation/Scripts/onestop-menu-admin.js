var runningAjaxCalls = 0;
var allCompleteCallback = null;

$(document).ready(function () {
    var removing = false;
    $('#current-items > ol.sortable.enabled').nestedSortable({
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

    $('#draft-items > ol.sortable.enabled').nestedSortable({
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

    var toggleExpandChangedCallback = function () {
        var items = $(".changed, .removed, .new").parents('li');
        if ($(this).hasClass("collapse-changed")) {
            items.collapse();
            $(this).addClass("expand-changed");
            $(this).removeClass("collapse-changed");
            $(this).html("Expand all");
        }
        else {
            items.expand();
            $(this).addClass("collapse-changed");
            $(this).removeClass("expand-changed");
            $(this).html("Collapse all");
        }
    };

    var toggleExpandAllCallback = function () {
        if ($(this).hasClass("collapse-all")) {
            $("#current-items .itemlist > li").collapse(true);
            $(this).addClass("expand-all");
            $(this).removeClass("collapse-all");
            $(this).html("Expand all");
        }
        else {
            $("#current-items .itemlist > li").expand(true);
            $(this).addClass("collapse-all");
            $(this).removeClass("expand-all");
            $(this).html("Collapse all");
        }
    };

    $("#expand-all").click(toggleExpandAllCallback);
    $("#expand-changed").click(toggleExpandChangedCallback);

    $(".enabled .item .enter-position").click(function () {
        DisplayPositionForm($(this));
    });

    var list = $("#current-items");

    list.fadeIn(3000);
    allCompleteCallback = function () {
        allCompleteCallback = null;
    };

    // Async load of all items
    EnsureItems(list, function () {
        // Refresh tree after load
        RefreshSubtree();
    }, false, 0);

    var loading = $('#loading');
    var bar = $('#loading .bar');

    $(document).ajaxSend(function (event, jqxhr, settings) {
        if (runningAjaxCalls > 0) {
            loading.show();
            bar.html("Work in progress: " + runningAjaxCalls + " operations left.");
        }
    });

    $(document).ajaxComplete(function (event, xhr, settings) {
        if (runningAjaxCalls == 0) {
            loading.hide();
            if (allCompleteCallback) allCompleteCallback();
        } else {
            loading.show();
            bar.html("Work in progress: " + runningAjaxCalls + " operations left.");
        }
    });
});

function ToggleCallback(recurse, item) {
    item = item ? item : $(this);
    if (item.hasClass("toggle-expand")) {
        item.parent('li').expand(recurse);

    } else {
        if (item.hasClass("toggle-collapse")) {
            item.parent('li').collapse(recurse);
        }
    }
};

function EnsureItems(item, callback, force, recurseTimes) {
    var id = item.data('id');
    var itemList = item.children(".itemlist");
    var toggle = item.children('.toggle');

    if (!id && id != 0) {
        itemList.children("li").each(function () { EnsureItems($(this), callback, force, recurseTimes); });
    } else {
        LoadChildren(id, $('#itemsheader').data("loadchildrenurl"), function (status, isEmpty, alreadyLoaded) {
            if (status && (!alreadyLoaded || force)) {
                if (isEmpty) {
                    toggle.hide();
                    childToggle.removeClass('enabled');
                } else {
                    var children = itemList.children("li");
                    // Preload items
                    if (recurseTimes && recurseTimes > 0) {
                        recurseTimes--;
                        children.each(function () {
                            var child = $(this);
                            EnsureItems(child, null, force, recurseTimes);
                        });
                    }

                    if (children.length) {
                        children.each(function () {
                            var childToggle = $(this).children('.toggle');
                            childToggle.off('click');
                            childToggle.click(function () {
                                ToggleCallback(false, childToggle);
                            });

                            CountChildren($(this).data('id'),
                                function () {
                                    childToggle.show();
                                    childToggle.addClass('enabled');
                                    if (callback) callback();
                                },
                                function () {
                                    childToggle.hide();
                                    childToggle.removeClass('enabled');
                                    if (callback) callback();
                                });
                        });
                    }
                }
            } else {
                if (callback) callback();
            }

        }, force, item);
    }
};

function LoadChildren(id, url, callback, force, item) {
    var ajaxData = { itemId: id, differentiator: "published" };
    var list = item.children('.itemlist');
    var loaded = list.closest('.itemlist').data('loaded') === true;

    if (!loaded || force) {
        Async(url, ajaxData, 'GET', function (data) {
            list.empty().html(data);

            // root should always be shown
            if (id == 0 || IsExpanded(item)) list.show();
            else list.hide();

            list.closest('.itemlist').data('loaded', true);

            if (callback) callback(true, !/\S/.test(data), false);
            RefreshSubtree(list, !/\S/.test(data));
        }, function () {
            list.empty();
            list.closest('.itemlist').data('loaded', false);

            if (callback) callback(false, true, false);
        });
    } else {
        if (callback) callback(true, false, true);
    }
}

function RefreshSubtree(selector, isEmpty) {
    // Fix input element click problem
    var listRoot = selector ? $(selector) : $('#current-items > .itemlist');
    var rootItem = selector ? $(selector) : $(document);
    rootItem.find('.dropdown-menu input, .dropdown-menu label, .dropdown-menu').click(function (e) {
        e.stopPropagation();
    });

    rootItem.find('[rel=tooltip]').tooltip();

    if (!isEmpty) {
        $("#global-trigger-all:hidden").fadeIn(600);
    }

    // Setting changed global triggers
    var changed = listRoot.find(".changed, .removed, .new");
    if (changed.length) {
        $("#global-trigger-changed:hidden").fadeIn(400);
        $("#global-clear-drafts:hidden").fadeIn(400);
    }

    rootItem.find('[data-forclass]').each(function () {
        var item = $(this);
        var li = item.closest('div');
        var classToLook = item.data('forclass');
        if (li && /\S/.test(classToLook) && li.hasClass(classToLook))
            item.show();
    });

    rootItem.find('[data-notforclass]').each(function () {
        var item = $(this);
        var li = item.closest('div');
        var classToLook = item.data('notforclass');
        if (li && /\S/.test(classToLook) && !li.hasClass(classToLook))
            item.show();
    });

    var unsafeItems = rootItem.find('a[itemprop~=UnsafeUrl]');
    var ajaxItems = rootItem.find('a[itemprop~=AjaxUrl]');

    unsafeItems.setConfirmModals();
    ajaxItems.setConfirmModals();

    RefreshUnsafeUrls(unsafeItems);
    AjaxifyUrls(ajaxItems);
};

function Update(event, ui) {
    if (EnsureDraftMode()) {
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
    } else {
        $(ui.sender ? ui.sender : event.target).sortable('cancel');
    }
};

function SendTreeUpdateRequest(item, parentId, newChildren) {
    var moveUrl = $('#itemsheader').data("moveurl");
    var draftViewUrl = $('#itemsheader').data("draftviewurl");

    var ajaxData = ({
        __RequestVerificationToken: $('[name=__RequestVerificationToken]').attr('value'),
        newChildren: newChildren,
        parentId: parentId
    });


    Async(moveUrl, ajaxData, 'POST', function () {
        $(item).data('parentid', parentId);
        var newParent = parentId > 0 ? $('#item-' + parentId) : $('#current-items');
        EnsureItems(newParent, null, true, 0);
    });
}

function MakeDraft(event, ui) {
    if (EnsureDraftMode()) {
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

function CountChildren(id, callbackNonEmpty, callbackEmpty) {
    var url = $('#itemsheader').data("countchildrenurl");
    var ajaxData = { itemId: id };
    Async(url, ajaxData, 'GET', function (data) {
        var c = data.count;
        if (c > 0 && callbackNonEmpty) {
            callbackNonEmpty();
        }
        else if (c <= 0 && callbackEmpty) {
            callbackEmpty();
        }
    }, null, null, true);
}

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
    if (EnsureDraftMode()) {
        var value = $(item).siblings(".position-text").val();
        var posUrl = $('#itemsheader').data("setpositionurl");
        var parentLi = $(item).parent().parent().parent().parent();
        var id = parentLi.attr('id');
        var ajaxData = ({
            __RequestVerificationToken: $('[name=__RequestVerificationToken]').attr('value'),
            itemId: parentLi.data("id"),
            position: value
        });

        var div = $(item).parent();

        div.html("Saving...");
        div.unbind('click');

        Async(posUrl, ajaxData, 'POST', function () {
            parentLi.fadeOut(500).remove();
            var lastDot = value.lastIndexOf(".");
            var newParentPos = lastDot > 0 ? value.substr(0, lastDot) : "0";
            var newParentItem = $('[data-position="' + newParentPos + '"]');
            newParentItem = (newParentItem.length) ? newParentItem : $('[data-position="0"]');
            EnsureItems(newParentItem, function () {
                newParentItem.expand();
                newParentItem.parents('li').expand();
                ScrollTo('#' + id);
            }, true, 0);
        }, function () {
            PositionSetError(div);
        });
    }
}

function EnsureDraftMode() {
    var mode = $("#itemsheader").data('mode');
    if (mode != "Draft") {
        var url = $("#itemsheader").data('draftviewurl');
        if (confirm("Not allowed in 'Current' view mode. You need to be in 'Draft' mode to modify the menu.\n\nClick OK to go to Draft mode, or hit Cancel to stay on the current page.")) {
            window.location.replace(url);
        }
        return false;
    }

    return true;
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
        scrollTop: $(element).offset().top - 200
    }, 500);
}

function IsExpanded(item) {
    return item.children('.toggle').hasClass("toggle-collapse");
}

function Async(url, data, type, success, error, complete, suppressCounting) {
    $.ajax({
        type: type,
        data: data,
        url: url,
        traditional: true,
        tryCount: 0,
        retryLimit: 5,
        beforeSend: function () {
            if (!suppressCounting) {
                runningAjaxCalls++;
            }
        },
        complete: function () {
            if (complete) complete();

            if (!suppressCounting) {
                runningAjaxCalls--;
            }
        },
        success: success,
        error: function (xhr, textStatus, errorThrown) {
            this.tryCount++;
            if (xhr.status != 0 && this.tryCount <= this.retryLimit) {
                console.log("Async call to url '" + url + "' failed. Retrying (" + this.tryCount + " of " + this.retryLimit + ")");
                //try again
                $.ajax(this);
                return;
            }

            if (error) error(xhr, textStatus, errorThrown);
        }
    });
}

// General helpers
function RefreshUnsafeUrls(items) {
    var magicToken = $("input[name=__RequestVerificationToken]").first();
    if (!magicToken) { return; } // no sense in continuing if form POSTS will fail
    items.filter("a[itemprop~=UnsafeUrl]").each(function () {
        var _this = $(this);
        var hrefParts = _this.attr("href").split("?");
        var form = $("<form action=\"" + hrefParts[0] + "\" method=\"POST\" />");
        form.append(magicToken.clone());
        if (hrefParts.length > 1) {
            var queryParts = hrefParts[1].split("&");
            for (var i = 0; i < queryParts.length; i++) {
                var queryPartKVP = queryParts[i].split("=");
                //trusting hrefs in the page here
                form.append($("<input type=\"hidden\" name=\"" + decodeURIComponent(queryPartKVP[0]) + "\" value=\"" + decodeURIComponent(queryPartKVP[1]) + "\" />"));
            }
        }
        form.css({ "position": "absolute", "left": "-9999em" });
        $("body").append(form);
        _this.click(function () {
            form.submit();
            return false;
        });
    });
}

function AjaxifyUrls(items) {
    var magicToken = $("input[name=__RequestVerificationToken]").first();
    if (!magicToken) { return; } // no sense in continuing if form POSTS will fail

    items.filter("a[itemprop~=AjaxUrl]").each(function () {
        var ajaxData = ({
            __RequestVerificationToken: magicToken.val()
        });

        var _this = $(this);
        var hrefParts = _this.attr("href").split("?");
        if (hrefParts.length > 1) {
            var queryParts = hrefParts[1].split("&");
            for (var i = 0; i < queryParts.length; i++) {
                var queryPartKVP = queryParts[i].split("=");
                //trusting hrefs in the page here
                ajaxData[decodeURIComponent(queryPartKVP[0])] = decodeURIComponent(queryPartKVP[1]);
            }
        }

        _this.click(function (e) {
            if (EnsureDraftMode()) {
                e.preventDefault();
                Async(hrefParts[0], ajaxData, 'POST', function () {
                    var parentId = _this.closest('li').data('parentid');
                    if (parentId >= 0) {
                        var newParent = parentId > 0 ? $('#item-' + parentId) : $('#current-items');
                        EnsureItems(newParent, function () { ScrollTo(newParent); }, true, 0);
                    }
                });
            }

            e.stopImmediatePropagation();
            return false;
        });
    });
}

// Toggles
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

// jQuery extensions
jQuery.fn.expand = function (recurse) {
    this.each(function (options) {
        var item = $(this).children('.toggle.enabled');
        if (item.length) {
            var hasClass = item.hasClass("toggle-expand");
            var list = $(this).children(".itemlist");
            EnsureItems($(this), function () {
                if (recurse) {
                    list.children('li').expand(recurse);
                }
                if (hasClass) {
                    list.slideDown(100);
                    list.fadeIn(100);

                    item.removeClass("toggle-expand");
                    item.addClass("toggle-collapse");
                }
            }, false, 0);
        }
    });
};

jQuery.fn.collapse = function (recurse) {
    this.each(function (options) {
        var item = $(this).children('.toggle.enabled');
        var hasClass = item.hasClass("toggle-collapse");
        var list = $(this).children(".itemlist");
        if (recurse) {
            list.children('li').collapse(recurse);
        }

        if (hasClass) {
            list.slideUp(100);
            list.fadeOut(100);

            item.removeClass("toggle-collapse");
            item.addClass("toggle-expand");
        }
    });
};
