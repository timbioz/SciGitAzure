using Orchard.UI.Resources;

namespace Contrib.Stars {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("Contrib.Stars").SetUrl("Contrib.Stars.css");
            manifest.DefineScript("Contrib.Stars").SetUrl("Contrib.Stars-1.3.js").SetDependencies("jQuery", "ShapesBase");
        }
    }
}
