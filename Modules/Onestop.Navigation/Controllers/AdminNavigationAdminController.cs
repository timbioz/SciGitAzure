using System.Linq;
using System.Web.Mvc;
using Onestop.Navigation.Models;
using Orchard;
using Orchard.Core.Contents.Controllers;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

namespace Onestop.Navigation.Controllers {
    [Admin]
    [OrchardFeature("Onestop.Navigation.AdminMenu")]
    public class AdminNavigationAdminController : Controller {
        private readonly INavigationManager _navigationManager;
        private readonly IRepository<AdminMenuItemRecord> _navigationRecords;

        public AdminNavigationAdminController(IOrchardServices services, IShapeFactory shapeFactory, INavigationManager navigationManager, IRepository<AdminMenuItemRecord> navigationRecords) {
            Services = services;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
            _navigationManager = navigationManager;
            _navigationRecords = navigationRecords;
        }

        public IOrchardServices Services { get; set; }

        public Localizer T { get; set; }

        private dynamic Shape { get; set; }

        public ActionResult Index() {
            var menu = _navigationManager.BuildMenu("admin");
            var customItems = _navigationRecords.Table
                .Select(i => i)
                .ToList();
            var shape = Shape.AdminMenu_Admin(
                Menu: menu,
                CustomItems: customItems);

            return new ShapeResult(this, shape);
        }

        public ActionResult Create() {
            return View();
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreateMenuItem() {
            if (!Services.Authorizer.Authorize(Orchard.Core.Navigation.Permissions.ManageMainMenu, T("Couldn't create menu item"))) {
                return new HttpUnauthorizedResult();
            }

            var model = new AdminMenuItemRecord();
            TryUpdateModel(model, new[] {"Text", "Url", "Position", "ItemGroup", "GroupPosition"});

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            _navigationRecords.Create(model);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id) {
            var model = _navigationRecords.Get(id);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditMenuItemSavePOST(int id) {
            if (!Services.Authorizer.Authorize(Orchard.Core.Navigation.Permissions.ManageMainMenu, T("Couldn't edit menu item"))) {
                return new HttpUnauthorizedResult();
            }

            var model = _navigationRecords.Get(id);
            TryUpdateModel(model, new[] { "Text", "Url", "Position", "ItemGroup", "GroupPosition" });

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            _navigationRecords.Update(model);

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(Orchard.Core.Navigation.Permissions.ManageMainMenu, T("Couldn't edit menu item"))) {
                return new HttpUnauthorizedResult();
            }

            var model = _navigationRecords.Get(id);
            _navigationRecords.Delete(model);

            return RedirectToAction("Index");
        }
    }
}