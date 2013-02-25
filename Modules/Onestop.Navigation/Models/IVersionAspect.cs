using Orchard.ContentManagement;
using Orchard.Security;

namespace Onestop.Navigation.Models {
    public interface IVersionAspect : IContent {
        IUser Author { get; }
        bool Removed { get; }
        bool Draft { get; }
        bool Latest { get; }
        bool Published { get; }
    }
}