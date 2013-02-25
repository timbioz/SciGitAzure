$(document).ready(function () {
    var stickyPanelOptions = {
        topPadding: 0,
        afterDetachCSSClass: "detached",
        savePanelSpace: true
    };

    $(".sticky").stickyPanel(stickyPanelOptions);
});