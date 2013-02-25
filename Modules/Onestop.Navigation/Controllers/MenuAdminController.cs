using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Onestop.Navigation.Models;
using Onestop.Navigation.Security;
using Onestop.Navigation.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Controllers;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.AntiForgery;
using Orchard.Mvc.Extensions;
using Orchard.Services;
using Orchard.Themes;
using Orchard.UI;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Logging;
using Orchard.Settings;
using IMenuService = Onestop.Navigation.Services.IMenuService;

namespace Onestop.Navigation.Controllers
{
    [ValidateInput(false)]
    [Admin]
    public class MenuAdminController : Controller, IUpdateModel
    {
        private readonly IMenuManager _menuManager;
        private readonly IMenuService _menuService;
        private readonly IClock _clock;
        private readonly ISiteService _siteService;
        private readonly dynamic _shape;
        private readonly IOrchardServices _services;
        private readonly Lazy<CultureInfo> _cultureInfo;

        public MenuAdminController(
            IMenuManager menuManager,
            IMenuService menuService,
            IOrchardServices services,
            IClock clock,
            ISiteService siteService,
            IShapeFactory shape)
        {
            _menuManager = menuManager;
            _menuService = menuService;
            _services = services;
            _clock = clock;
            _siteService = siteService;
            _shape = shape;
            T = NullLocalizer.Instance;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_services.WorkContext.CurrentCulture));
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }

        public ILogger Logger { get; set; }

        [HttpGet]
        public ActionResult CreateItem(int menuId, string type)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditAdvancedMenuItemOptions, T("Not allowed to create menu items of type '{0}'", type)))
            {
                return new HttpUnauthorizedResult();
            }

            if (!_services.Authorizer.Authorize(
                Permissions.CreateMenuItems,
                menu,
                T("Not allowed to create menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            var item = _menuService.CreateMenuItem(menuId, type);
            var model = _services.ContentManager.BuildEditor(item);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        private ActionResult CreateItemPOST(int menuId, string type, string returnUrl, Action<ContentItem> postCreateAction)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.CreateMenuItems, menu, T("Not allowed to create menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            var item = _menuService.CreateMenuItem(menuId, type);
            var model = _services.ContentManager.UpdateEditor(item, this);

            _services.ContentManager.Create(item, VersionOptions.Draft);

            if (!ModelState.IsValid)
            {
                _services.TransactionManager.Cancel();
                return View((object)model);
            }

            // Setting values from underlying default parts
            if (item.Is<MenuPart>())
            {
                item.As<ExtendedMenuItemPart>().Text = item.As<MenuPart>().MenuText;
            }

            if (item.Is<MenuItemPart>())
            {
                item.As<ExtendedMenuItemPart>().Url = item.As<MenuItemPart>().Url;
            }

            postCreateAction(item.ContentItem);

            _services.Notifier.Information(T("Your {0} has been created.", item.ContentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl ?? Url.Action("EditItem", "MenuAdmin", new { menuId, itemId = item.Id, area = "Onestop.Navigation" }));
        }

        [HttpPost]
        [ActionName("CreateItem")]
        [FormValueRequired("submit.Save")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult CreateAndEditItemPOST(int menuId, string type)
        {
            return CreateItemPOST(
                menuId,
                type,
                null,
                contentItem => { });
        }

        [HttpPost]
        [ActionName("CreateItem")]
        [FormValueRequired("submit.SaveAndAdd")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult CreateAndAddItemPOST(int menuId, string type)
        {
            return CreateItemPOST(
                menuId,
                type,
                Url.Action("CreateItem", "MenuAdmin", new { menuId, type, area = "Onestop.Navigation" }),
                contentItem => { });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult DeleteItem(int menuId, int itemId, int versionId = 0, string returnUrl = null)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.DeleteMenuItems, menu, T("Not allowed to delete menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            var item = versionId > 0
                ? _menuService.GetMenuItem(-1, VersionOptions.VersionRecord(versionId))
                : _menuService.GetMenuItem(itemId, VersionOptions.DraftRequired);
            if (item == null)
            {
                return HttpNotFound();
            }

            _menuService.DeleteMenuItem(itemId, versionId);
            _services.Notifier.Information(T("Menu item was successfully marked to delete"));

            return this.RedirectLocal(returnUrl, Url.Action("Index", "MenuAdmin", new { menuId, area = "Onestop.Navigation" }));
        }

        [HttpPost]
        public ActionResult UndeleteItem(int menuId, int versionId = 0, bool newVersion = false, string returnUrl = null)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.DeleteMenuItems, menu, T("Not allowed to delete menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            _menuService.UndeleteMenuItem(versionId, newVersion);
            _services.Notifier.Information(T("Menu item was successfully restored."));

            if (newVersion)
            {
                _services.Notifier.Information(T("Restored menu item has been moved to drafts."));
            }

            return this.RedirectLocal(returnUrl, Url.Action("Index", "MenuAdmin", new { menuId, area = "Onestop.Navigation" }));
        }

        [HttpGet]
        public ActionResult EditItem(int menuId, int itemId, int versionId = 0)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenuItems, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            var item = versionId > 0
                ? _menuService.GetMenuItem(-1, VersionOptions.VersionRecord(versionId))
                // we need to create a draft at this point, as editor screen requires a specific version - exactly that version will get edited.
                : _menuService.GetMenuItem(itemId, VersionOptions.DraftRequired);
            if (item == null)
            {
                return HttpNotFound();
            }

            // Setting values from underlying default parts
            if (item.Is<MenuPart>())
            {
                item.As<MenuPart>().MenuText = item.As<ExtendedMenuItemPart>().Text;
            }

            if (item.Is<MenuItemPart>())
            {
                item.As<MenuItemPart>().Url = item.As<ExtendedMenuItemPart>().Url;
            }

            if (item.Is<VersionInfoPart>())
            {
                item.As<VersionInfoPart>().Author = _services.WorkContext.CurrentUser;
            }

            if (item.Is<CommonPart>())
            {
                item.As<CommonPart>().VersionModifiedUtc = _clock.UtcNow;
            }

            var model = _services.ContentManager.BuildEditor(item);

            return View((object)model);
        }

        [HttpPost]
        [ActionName("EditItem")]
        [FormValueRequired("submit.Save")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult EditAndEditItemPOST(int menuId, int itemId, int versionId = 0)
        {
            return EditItemPOST(
                menuId,
                itemId,
                Url.Action("EditItem", "MenuAdmin", new { menuId, itemId, versionId, area = "Onestop.Navigation" }),
                item => { },
                versionId);
        }

        [HttpPost]
        [ActionName("EditItem")]
        [FormValueRequired("submit.SaveAndAdd")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult EditAndAddItemPOST(int menuId, int itemId, int versionId = 0)
        {
            return EditItemPOST(
                menuId,
                itemId,
                null,
                item => { },
                versionId);
        }

        private ActionResult EditItemPOST(int menuId, int itemId, string returnUrl, Action<ContentItem> postEditAction, int versionId = 0)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenuItems, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            var item = versionId > 0
                ? _menuService.GetMenuItem(-1, VersionOptions.VersionRecord(versionId))
                : _menuService.GetMenuItem(itemId, VersionOptions.DraftRequired);

            if (item == null)
            {
                return HttpNotFound();
            }

            // Validate form input
            var model = _services.ContentManager.UpdateEditor(item, this);

            // Setting values from underlying default parts
            if (item.Is<MenuPart>())
            {
                item.As<ExtendedMenuItemPart>().Text = item.As<MenuPart>().MenuText;
            }

            if (item.Is<MenuItemPart>())
            {
                item.As<ExtendedMenuItemPart>().Url = item.As<MenuItemPart>().Url;
            }

            if (item.Is<VersionInfoPart>())
            {
                item.As<VersionInfoPart>().Author = _services.WorkContext.CurrentUser;
            }

            if (item.Is<CommonPart>())
            {
                item.As<CommonPart>().VersionModifiedUtc = _clock.UtcNow;
            }

            if (!ModelState.IsValid)
            {
                _services.TransactionManager.Cancel();

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            postEditAction(item.ContentItem);
            _services.Notifier.Information(T("Your {0} has been saved.", item.ContentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl ?? Url.Action("CreateItem", "MenuAdmin", new { menuId, type = item.ContentItem.ContentType, area = "Onestop.Navigation" }));
        }

        public ActionResult Index(int menuId, DisplayMode? mode = null)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenuItems, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            IEnumerable<IContent> allMenuItems = null;

            if (!mode.HasValue)
            {
                var items = _menuService.GetMenuItems(menu, VersionOptions.Latest).OrderBy(i => i.As<ExtendedMenuItemPart>().Position, new FlatPositionComparer());

                // if there are any changed items - change mode to draft
                var displayMode = DisplayMode.Current;
                if (items.Any(i => i.As<ExtendedMenuItemPart>().IsChanged || i.As<ExtendedMenuItemPart>().IsRemoved))
                {
                    displayMode = DisplayMode.Draft;
                }

                return RedirectToAction("Index", new { menuId, mode = displayMode });
            }

            switch (mode)
            {
                case DisplayMode.Current:
                    allMenuItems = _menuService.GetMenuItems(menu, VersionOptions.Published, r => !r.Position.Contains(".")).OrderBy(i => i.As<ExtendedMenuItemPart>().Position, new FlatPositionComparer());
                    break;
                case DisplayMode.Draft:
                    allMenuItems = _menuService.GetMenuItems(menu, VersionOptions.Latest, r => !r.Position.Contains(".")).OrderBy(i => i.As<ExtendedMenuItemPart>().Position, new FlatPositionComparer());
                    break;
                case null:
                    break;
            }

            var draftItems = _menuService.GetMenuItems(menu, VersionOptions.Latest, r => r.Position == null).Where(item => item.As<ExtendedMenuItemPart>().IsDraft).ToList();
            var currentItems = allMenuItems.Where(item => item.As<ExtendedMenuItemPart>().IsCurrent).ToList();
            var localDate = new Lazy<DateTime>(() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _services.WorkContext.CurrentTimeZone));

            var model = new MenuAdminIndexViewModel
            {
                MenuItemDescriptors = _menuManager.GetMenuItemTypes(),
                Menu = menu,
                Items = allMenuItems,
                DraftItems = draftItems,
                CurrentItems = currentItems,
                MenuVersions = _menuService.GetScheduledMenuVersions(menuId),
                ScheduledDate = localDate.Value.ToString("d", _cultureInfo.Value),
                ScheduledTime = localDate.Value.ToString("t", _cultureInfo.Value),
                ScheduledUtc = localDate.Value,
                Mode = mode.HasValue ? mode.Value : default(DisplayMode)
            };

            return View(model);
        }

        public ActionResult Preview(int menuId, int versionNumber)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null || versionNumber == default(int))
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenuItems, menu, T("Not allowed to preview items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            var allMenuItems = _menuService.GetMenuItems(menu, VersionOptions.Number(versionNumber))
                .OrderBy(i => i.As<ExtendedMenuItemPart>().Position, new FlatPositionComparer());

            var currentItems = allMenuItems.Where(item => item.As<ExtendedMenuItemPart>().IsCurrent).ToList();
            var scheduledTask = _menuService.GetScheduledMenuVersions(menuId).FirstOrDefault(t => t.ContentItem.Version == versionNumber);
            var localDate = new Lazy<DateTime>(() => TimeZoneInfo.ConvertTimeFromUtc(
                scheduledTask != null && scheduledTask.ScheduledUtc.HasValue
                    ? scheduledTask.ScheduledUtc.Value
                    : _clock.UtcNow,
                _services.WorkContext.CurrentTimeZone));

            if (scheduledTask == null)
            {
                var previewHistoryModel = new MenuAdminPreviewViewModel
                {
                    MenuItemDescriptors = _menuManager.GetMenuItemTypes(),
                    Menu = menu,
                    Items = currentItems,
                    MenuVersions = _menuService.GetScheduledMenuVersions(menuId),
                    VersionNumber = versionNumber,
                    ScheduledDate = localDate.Value.ToString("d", _cultureInfo.Value),
                    ScheduledTime = localDate.Value.ToString("t", _cultureInfo.Value),
                    ScheduledUtc = _clock.UtcNow,
                };

                return View("PreviewHistory", previewHistoryModel);
            }

            var previewScheduledModel = new MenuAdminPreviewViewModel
            {
                MenuItemDescriptors = _menuManager.GetMenuItemTypes(),
                Menu = menu,
                Items = currentItems,
                MenuVersions = _menuService.GetScheduledMenuVersions(menuId),
                ScheduledDate = localDate.Value.ToString("d", _cultureInfo.Value),
                ScheduledTime = localDate.Value.ToString("t", _cultureInfo.Value),
                ScheduledUtc = scheduledTask.ScheduledUtc.Value,
                VersionNumber = versionNumber
            };

            return View(previewScheduledModel);
        }

        [HttpGet]
        [ActionName("Removed")]
        public ActionResult RemovedItems(int menuId)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenu, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            var items = _menuService.GetRemovedMenuItems(menuId);

            var model = new MenuAdminRemovedViewModel
            {
                Menu = menu,
                Items = items,
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult History(int menuId, PagerParameters pagerParameters)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenu, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var versions = _menuService.GetMenuHistory(menuId).OrderByDescending(i => i.ContentItem.Version);
            var localDate = new Lazy<DateTime>(() => TimeZoneInfo.ConvertTimeFromUtc(_clock.UtcNow, _services.WorkContext.CurrentTimeZone));

            var model = new MenuHistoryViewModel
            {
                DisplayDraftRow = pager.Page == 1,
                Versions = pager.PageSize == 0
                    ? versions
                    : versions.Skip(pager.GetStartIndex()).Take(pager.PageSize),
                Tasks = _menuService.GetScheduledMenuVersions(menuId),
                Menu = menu,
                Pager = _services.New.Pager(pager).TotalItemCount(versions.Count()),
                CurrentDate = localDate.Value.ToString(_cultureInfo.Value)
            };

            return View(model);
        }

        [HttpPost]
        [ActionName("RevertToVersion")]
        [FormValueRequired("submit.Publish")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult RevertToVersion(int menuId, int versionNumber, string returnUrl)
        {
            var menu = _menuService.GetMenu(menuId, VersionOptions.Number(versionNumber));
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenu, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            try
            {
                var newVersion = _menuService.CreateNewVersion(menuId, versionNumber);
                _menuService.PublishMenu(menuId, newVersion.ContentItem.VersionRecord.Id);
                _services.Notifier.Information(T("Successfully reverted to version '{0}'. Its contents has been copied to new version - '{1}'. It is the current one now.", versionNumber, newVersion.ContentItem.Version));
            }
            catch (Exception ex)
            {
                _services.Notifier.Error(T("Could not revert to version '{0}': {1}", menu.ContentItem.Version, ex.Message));
                Logger.Error(ex, "Could not revert menu '{0}' to version with id: {1}.", menu.ContentItem.VersionRecord.Id);
                _services.TransactionManager.Cancel();
            }

            return this.RedirectLocal(returnUrl ?? Url.Action("Index", new { menuId }));
        }

        [HttpPost]
        [ActionName("RevertToVersion")]
        [FormValueRequired("submit.SchedulePublish")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult RevertToVersionScheduled(int menuId, int versionNumber, string scheduledDate, string scheduledTime, string returnUrl)
        {
            var menu = _menuService.GetMenu(menuId, VersionOptions.Number(versionNumber));
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenu, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            if (!string.IsNullOrWhiteSpace(scheduledDate))
            {
                DateTime scheduled;

                // use current culture
                if (DateTime.TryParse(String.Concat(scheduledDate, " ", scheduledTime), _cultureInfo.Value, DateTimeStyles.None, out scheduled))
                {
                    var publishUtc = TimeZoneInfo.ConvertTimeToUtc(scheduled, _services.WorkContext.CurrentTimeZone);
                    if (publishUtc < _clock.UtcNow)
                    {
                        ModelState.AddModelError("ScheduledDate", T("You cannot schedule a publishing date in the past"));
                    }
                    else
                    {
                        try
                        {
                            var currentScheduledVersions = _menuService.GetScheduledMenuVersions(menuId);
                            var sameTimeScheduled = currentScheduledVersions.FirstOrDefault(v => v.ScheduledUtc == publishUtc);
                            if (sameTimeScheduled != null)
                            {
                                _services.Notifier.Error(
                                    T("There is already a version scheduled for publishing at this time. Pick different time or <a href='{0}'>modify/view the conflicting scheduled version.</a>",
                                      Url.Action(
                                          "Preview",
                                          "MenuAdmin",
                                          new
                                          {
                                              menuName = sameTimeScheduled.ContentItem.As<ITitleAspect>().Title,
                                              area = "Onestop.Navigation",
                                              versionNumber = sameTimeScheduled.ContentItem.Version
                                          })));
                            }
                            else
                            {
                                if (currentScheduledVersions.Any(v => v.ScheduledUtc > publishUtc))
                                {
                                    _services.Notifier.Warning(T("There are currently some versions scheduled for publishing at a later date, which will overwrite the current version when published. Make sure you check those versions and add necessary changes."));
                                }

                                _menuService.SchedulePublication(menuId, publishUtc, versionNumber);
                                _services.Notifier.Information(T("Contents of version '{0}' has been copied to new version, which will be published on {0}", versionNumber, scheduledDate));
                            }

                        }
                        catch (Exception ex)
                        {
                            _services.Notifier.Error(T(ex.Message));
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", T("{0} is an invalid date and time", scheduledDate));
                }
            }
            else if (!string.IsNullOrWhiteSpace(scheduledDate))
            {
                ModelState.AddModelError("", T("Date and time needs to be specified for when this is to be published. If you don't want to schedule publishing then click Save or Publish Now."));
            }

            return this.RedirectLocal(returnUrl ?? Url.Action("Index", new { menuId }));
        }

        [HttpPost]
        public ActionResult Move(int menuId, int parentId, IEnumerable<int> newChildren)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenuItems, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            try
            {
                _menuService.UpdatePositionsFor(menuId, parentId, newChildren);
            }
            catch (Exception ex)
            {
                _services.Notifier.Error(T(ex.Message));
                _services.TransactionManager.Cancel();
            }
            return new JsonResult();
        }

        [HttpPost]
        public ActionResult MovePreview(int menuId, int parentId, IEnumerable<int> newChildren)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenuItems, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            try
            {
                _menuService.UpdatePositionsForPreview(menuId, parentId, newChildren);
            }
            catch (Exception ex)
            {
                _services.Notifier.Error(T(ex.Message));
                _services.TransactionManager.Cancel();
            }
            return new JsonResult();
        }

        [HttpPost]
        public ActionResult MakeDraft(int menuId, int itemId)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return new HttpNotFoundResult();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenuItems, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            try
            {
                _menuService.UnpublishMenuItem(menuId, itemId);
            }
            catch (Exception ex)
            {
                _services.Notifier.Error(T(ex.Message));
                _services.TransactionManager.Cancel();
            }

            return new JsonResult();
        }

        [HttpPost]
        public ActionResult SetPosition(int menuId, int itemId, string position)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenuItems, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            try
            {
                if (!_menuService.SetPosition(menuId, itemId, position))
                {
                    return HttpNotFound();
                }
            }
            catch (Exception ex)
            {
                _services.Notifier.Error(T(ex.Message));
                _services.TransactionManager.Cancel();
            }

            return new JsonResult();
        }

        [HttpPost]
        [ActionName("Save")]
        [FormValueRequired("submit.Publish")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult PublishMenu(int menuId)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenu, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            try
            {
                try
                {
                    _menuService.PublishMenu(menuId);
                    _services.Notifier.Information(T("Menu has been published successfully."));
                }
                catch (Exception ex)
                {
                    _services.Notifier.Error(T(ex.Message));
                }
            }
            catch (Exception ex)
            {
                _services.Notifier.Error(T(ex.Message));
                _services.TransactionManager.Cancel();
            }

            return RedirectToAction("Index", new { menuId });
        }

        [HttpPost]
        [ActionName("Save")]
        [FormValueRequired("submit.SchedulePublish")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult SchedulePublishMenu(int menuId, string scheduledDate, string scheduledTime)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenu, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            if (!string.IsNullOrWhiteSpace(scheduledDate))
            {
                DateTime scheduled;

                // use current culture
                if (DateTime.TryParse(String.Concat(scheduledDate, " ", scheduledTime), _cultureInfo.Value, DateTimeStyles.None, out scheduled))
                {
                    var publishUtc = TimeZoneInfo.ConvertTimeToUtc(scheduled, _services.WorkContext.CurrentTimeZone);
                    if (publishUtc < _clock.UtcNow)
                    {
                        ModelState.AddModelError("ScheduledDate", T("You cannot schedule a publishing date in the past"));
                    }
                    else
                    {
                        try
                        {
                            var currentScheduledVersions = _menuService.GetScheduledMenuVersions(menuId);
                            var sameTimeScheduled = currentScheduledVersions.FirstOrDefault(v => v.ScheduledUtc == publishUtc);
                            if (sameTimeScheduled != null)
                            {
                                _services.Notifier.Error(
                                    T("There is already a version scheduled for publishing at this time. Pick different time or <a href='{0}'>modify/view the conflicting scheduled version.</a>",
                                      Url.Action(
                                          "Preview",
                                          "MenuAdmin",
                                          new
                                          {
                                              menuName = sameTimeScheduled.ContentItem.As<ITitleAspect>().Title,
                                              area = "Onestop.Navigation",
                                              versionNumber = sameTimeScheduled.ContentItem.Version
                                          })));
                            }
                            else
                            {
                                if (currentScheduledVersions.Any(v => v.ScheduledUtc > publishUtc))
                                {
                                    _services.Notifier.Warning(T("There are currently some versions scheduled for publishing at a later date, which will overwrite the current version when published. Make sure you check those versions and add necessary changes."));
                                }

                                _menuService.SchedulePublication(menuId, publishUtc);
                                _services.Notifier.Information(T("Current menu state has been scheduled for publishing on {0}", scheduledDate));
                            }

                        }
                        catch (Exception ex)
                        {
                            _services.Notifier.Error(T(ex.Message));
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", T("{0} is an invalid date and time", scheduledDate));
                }
            }
            else if (!string.IsNullOrWhiteSpace(scheduledDate))
            {
                ModelState.AddModelError("", T("Date and time needs to be specified for when this is to be published. If you don't want to schedule publishing then click Save or Publish Now."));
            }

            return RedirectToAction("Index", new { menuId });
        }

        [HttpPost]
        [ActionName("Save")]
        [FormValueRequired("submit.CancelSchedule")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult CancelSchedule(int menuId, int versionNumber)
        {
            var menu = _menuService.GetMenu(menuId, VersionOptions.Number(versionNumber));
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenu, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            var scheduledTask = _menuService.GetScheduledMenuVersions(menuId).FirstOrDefault(t => t.ContentItem.Version == versionNumber);

            if (scheduledTask == null)
            {
                _services.Notifier.Error(T("Nothing to cancel. Scheduled task for version '{0}' of menu '{1}' does not exist.", versionNumber, menu.As<ITitleAspect>().Title));
            }
            else
            {
                _menuService.CancelSchedule(scheduledTask.ContentItem);
                _services.Notifier.Information(T("Cancelled scheduled publication of '{0}' menu version '{1}'", menu.As<ITitleAspect>().Title, versionNumber));
            }

            return RedirectToAction("Index", new { menuId });
        }

        [HttpPost]
        [ActionName("Save")]
        [FormValueRequired("submit.ChangeSchedule")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult ChangeSchedule(int menuId, int versionNumber, string scheduledDate, string scheduledTime)
        {
            var menu = _menuService.GetMenu(menuId, VersionOptions.Number(versionNumber));
            if (menu == null)
            {
                return HttpNotFound();
            }

            if (!_services.Authorizer.Authorize(Permissions.EditMenu, menu, T("Not allowed to edit menu items for menu '{0}'", menu.As<ITitleAspect>().Title)))
            {
                return new HttpUnauthorizedResult();
            }

            if (!string.IsNullOrWhiteSpace(scheduledDate))
            {
                DateTime scheduled;

                // use current culture
                if (DateTime.TryParse(String.Concat(scheduledDate, " ", scheduledTime), _cultureInfo.Value, DateTimeStyles.None, out scheduled))
                {
                    var publishUtc = TimeZoneInfo.ConvertTimeToUtc(scheduled, _services.WorkContext.CurrentTimeZone);
                    if (publishUtc < _clock.UtcNow)
                    {
                        ModelState.AddModelError("ScheduledDate", T("You cannot schedule a publishing date in the past"));
                    }
                    else
                    {
                        try
                        {
                            var currentScheduledVersions = _menuService.GetScheduledMenuVersions(menuId);
                            var scheduledTask = currentScheduledVersions.FirstOrDefault(t => t.ContentItem.Version == versionNumber);

                            if (scheduledTask == null)
                            {
                                _services.Notifier.Error(T("Scheduled task for version '{0}' of menu '{1}' does not exist.", versionNumber, menu.As<ITitleAspect>().Title));
                            }
                            else
                            {

                                var sameTimeScheduled = currentScheduledVersions.FirstOrDefault(v => v.ScheduledUtc == publishUtc);
                                if (sameTimeScheduled != null)
                                {
                                    _services.Notifier.Error(
                                        T("There is already a version scheduled for publishing at this time. Pick different time or <a href='{0}'>modify/view the conflicting scheduled version.</a>",
                                          Url.Action(
                                              "Preview",
                                              "MenuAdmin",
                                              new
                                              {
                                                  menuName = sameTimeScheduled.ContentItem.As<ITitleAspect>().Title,
                                                  area = "Onestop.Navigation",
                                                  versionNumber = sameTimeScheduled.ContentItem.Version
                                              })));
                                }
                                else
                                {
                                    _menuService.SchedulePublication(scheduledTask.ContentItem, publishUtc);
                                    if (currentScheduledVersions.Any(v => v.ScheduledUtc > publishUtc))
                                    {
                                        _services.Notifier.Warning(T("There are currently some versions scheduled for publishing at a later date, which will overwrite the current version when published. Make sure you check those versions and add necessary changes."));
                                    }

                                    _services.Notifier.Information(T("Current menu version will be published on {0}", scheduledDate));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _services.Notifier.Error(T(ex.Message));
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", T("{0} is an invalid date and time", scheduledDate));
                }
            }
            else if (!string.IsNullOrWhiteSpace(scheduledDate))
            {
                ModelState.AddModelError("", T("Date and time needs to be specified for when this is to be published. If you don't want to schedule publishing then click Save or Publish Now."));
            }

            return RedirectToAction("Preview", new { menuId, versionNumber });
        }

        [HttpPost]
        [ActionName("Save")]
        [FormValueRequired("submit.Save")]
        [ValidateAntiForgeryTokenOrchard]
        public ActionResult SaveMenu(int menuId)
        {
            return RedirectToAction("Index", new { menuId });
        }

        [HttpPost]
        public ActionResult ClearCache(int menuId)
        {
            if (!_services.Authorizer.Authorize(Permissions.EditAdvancedMenuItemOptions)
                && !_services.Authorizer.Authorize(Permissions.EditMenu))
                return new HttpUnauthorizedResult();

            try
            {
                _menuService.ClearCache(menuId);
                _services.Notifier.Information(T("Menu caches have been cleared successfully."));
            }
            catch (Exception ex)
            {
                _services.Notifier.Error(T(ex.Message));
                _services.TransactionManager.Cancel();
            }

            return RedirectToAction("Index", new { menuId });
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        [HttpPost]
        public ActionResult ClearDraft(int itemId, string returnUrl)
        {
            var item = _menuService.GetMenuItem(itemId, VersionOptions.Latest);
            if (item == null)
            {
                return HttpNotFound();
            }

            var menuName = item.As<ExtendedMenuItemPart>().Menu.As<ITitleAspect>().Title;
            _menuService.ClearDraft(itemId);
            _services.Notifier.Information(T("Draft removed successfully."));

            return this.RedirectLocal(returnUrl ?? Url.Action("Index", new { menuName }));
        }

        [HttpPost]
        public ActionResult ClearAllDrafts(int menuId, string returnUrl)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null)
            {
                return HttpNotFound();
            }

            _menuService.ClearAllDrafts(menuId);
            _services.Notifier.Information(T("All existing drafts removed successfully."));

            return this.RedirectLocal(returnUrl ?? Url.Action("Index", new { menuId }));
        }

        [Themed(false)]
        public ActionResult GetChildren(int menuId, int itemId, DisplayMode mode, string differentiator = null)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null) {
                return HttpNotFound();
            }

            try {
                IContent item;
                var predicate = BuildQueryPredicate(itemId, mode, out item);

                // Retrieve child items using the provided predicate
                var options = mode == DisplayMode.Draft ? VersionOptions.Latest : VersionOptions.Published;
                var items = _menuService.GetMenuItems(menu, options, predicate).ToList();
                var children = items
                    .Where(i => i.As<ExtendedMenuItemPart>().IsCurrent)
                    .OrderBy(i => i.As<ExtendedMenuItemPart>().Position, new FlatPositionComparer())
                    .ToList();

                var shape = _shape.MenuItem_List(MenuItems: children, Parent: item, Differentiator: differentiator, ShowToggle: true, UpdateAfterDrop: true, AllowEdit: mode == DisplayMode.Draft);
                return new ShapePartialResult(this, shape);
            }
            catch (HttpException ex) {
                return new HttpStatusCodeResult(ex.ErrorCode);
            }
            catch (Exception) {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult RedirectToLatestMenu()
        {
            var latestMenu = _menuService.GetMenus().OrderByDescending(m => m.As<ICommonPart>().CreatedUtc).FirstOrDefault();
            return latestMenu != null ? RedirectToAction("Index", new { menuId = latestMenu.Id }) : null;
        }

        [Themed(false)]
        public ActionResult CountChildren(int menuId, int itemId, DisplayMode mode)
        {
            var menu = _menuService.GetMenu(menuId);
            if (menu == null) {
                return HttpNotFound();
            }

            try {
                IContent item;
                var predicate = BuildQueryPredicate(itemId, mode, out item);
                var options = mode == DisplayMode.Draft ? VersionOptions.Latest : VersionOptions.Published;
                var count = _menuService.GetMenuItemsCount(menu, options, predicate);

                return Json(new { count }, JsonRequestBehavior.AllowGet);

            }
            catch (HttpException ex) {
                return new HttpStatusCodeResult(ex.ErrorCode);
            }
            catch (Exception) {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        private Expression<Func<ExtendedMenuItemPartRecord, bool>> BuildQueryPredicate(int itemId, DisplayMode mode, out IContent item)
        {
            var options = mode == DisplayMode.Draft ? VersionOptions.Latest : VersionOptions.Published;

            if (itemId != default(int)) {
                item = _menuService.GetMenuItem(itemId, options);

                if (item == null || !item.Is<MenuPart>() || !item.Is<ExtendedMenuItemPart>())
                    throw new HttpException(404, "Such item does not exist.");

                var itemPosition = item.As<ExtendedMenuItemPart>().Position;
                return r => r.Position != null && r.ParentPosition == itemPosition;
            }

            item = null;

            // Top-level items
            return r => r.Position != null && (r.ParentPosition == null || r.ParentPosition == "");
        }
    }
}