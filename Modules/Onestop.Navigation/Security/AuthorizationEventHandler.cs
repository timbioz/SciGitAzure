using JetBrains.Annotations;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Onestop.Navigation.Security {
    [UsedImplicitly]
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Adjust(CheckAccessContext context) {
            if (context.Granted || context.Content == null || context.Content.ContentItem.ContentType != "Menu") {
                return;
            }

            // replace permission if a menu-specific version exists
            var permission = GetMenuVariation(context.Permission);

            if (permission == null) {
                return;
            }

            context.Adjusted = true;
            context.Permission = MenuPermissions.CreateDynamicPermission(permission, context.Content);
        }

        public void Checking(CheckAccessContext context) {}

        public void Complete(CheckAccessContext context) {}

        private static Permission GetMenuVariation(Permission permission) {
            return MenuPermissions.ConvertToDynamicPermission(permission);
        }
    }
}