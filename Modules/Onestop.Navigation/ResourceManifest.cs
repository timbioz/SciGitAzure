using Orchard.UI.Resources;

namespace Onestop.Navigation {
    public class Resources : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("NestedSortable").SetDependencies("jQuery", "jQueryUI_Sortable", "jQueryUI_Draggable", "jQueryUI_Droppable").SetUrl("jquery.mjs.nestedSortable.js");
            manifest.DefineScript("jQuery_StickyPanel").SetDependencies("jQuery").SetUrl("jquery.stickyPanel.min.js");
            manifest.DefineScript("StickyPanels").SetDependencies("jQuery_StickyPanel").SetUrl("sticky-panels.js");
            manifest.DefineScript("ConfirmDialog").SetUrl("confirm-dialog.js").SetDependencies("jQuery", "Bootstrap");
            manifest.DefineScript("MenuAdminScripts").SetDependencies("StickyPanels", "NestedSortable", "Bootstrap", "ConfirmDialog").SetUrl("onestop-menu-admin.min.js");

            manifest.DefineScript("MenuAdminScripts_Preview").SetDependencies("StickyPanels", "NestedSortable", "Bootstrap", "ConfirmDialog").SetUrl("onestop-menu-admin-preview.js");

            manifest.DefineScript("Bootstrap").SetUrl("bootstrap.js").SetDependencies("jQuery");
            manifest.DefineStyle("Bootstrap").SetUrl("bootstrap.css");
            manifest.DefineStyle("MenuAdmin").SetUrl("onestop-menu-admin.css").SetDependencies("Bootstrap");
        }
    }
}