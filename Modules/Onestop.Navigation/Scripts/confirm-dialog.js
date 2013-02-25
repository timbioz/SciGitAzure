jQuery.fn.setConfirmModals = function () {
    this.each(function(options) {
        var _this = $(this);
        if (!_this.is('[itemprop~=RequiresConfirm]')) return;

        _this.off('click');
        var data = _this.data();
        var title = "title" in data ? data.title : "Confirm action";
        var text = "text" in data ? data.text : "Do you want to confirm this action?";
        var type = "type" in data && data.type == "modal" ? "modal" : "default";
        var previewId = "preview" in data ? data.preview : null;

        if (type == "default") {
            _this.click(function (e) {
                if (!confirm(text)) {
                    e.stopImmediatePropagation();
                    return false;
                }

                return true;
            });
        } else {
            var modal = $("<div class=\"modal fade\" id=\"confirm\">");
            var header = $("<div class=\"modal-header\"><h3>" + title + "</h3></div>");
            var body = $("<div class=\"modal-body\" style=\"max-height: 300px;\">");
            body.append("<div class=\"alert alert-error\">" + text + "</div>");
            if (previewId != null) {
                var preview = $("<p class=\"well\">");
                body.append(preview);
            }
            var footer = $("<div class=\"modal-footer\">");
            var okButton = $("<a class=\"btn btn-primary\">Ok</a>");
            var cancelButton = $("<a class=\"btn\">Cancel</a>");
            footer.append(okButton);
            footer.append(cancelButton);
            modal.append(header);
            modal.append(body);
            modal.append(footer);
            var modalOn = false;
            var clicked = false;
            modal.modal({
                keyboard: false,
                show: false
            });
            modal.on('hide', function () {
                modalOn = false;
            });

            modal.on('show', function () {
                if (previewId != null) {
                    preview.empty();
                    preview.append($(previewId).clone());
                }
                modalOn = true;
            });

            okButton.click(function (e) {
                e.preventDefault();
                e.stopImmediatePropagation();
                
                clicked = true;
                _this.click();
                modal.modal('hide');

                return false;
            });
            cancelButton.click(function (e) {
                modal.modal('hide');
            });
            _this.click(function (e) {
                if (!modalOn && !clicked) {
                    modal.modal('show');
                    e.stopImmediatePropagation();
                }
                
                return false;
            });

        }
    });
};
