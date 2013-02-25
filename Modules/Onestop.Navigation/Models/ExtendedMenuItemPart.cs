using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Utilities;
using Orchard.Core.Navigation.Models;

namespace Onestop.Navigation.Models {
    public class ExtendedMenuItemPart : ContentPart<ExtendedMenuItemPartRecord>
    {
        private readonly LazyField<IContent> _menuVersion = new LazyField<IContent>();
        public LazyField<IContent> MenuVersionField { get { return _menuVersion; } }

        private readonly LazyField<bool> _isDraft = new LazyField<bool>();
        private readonly LazyField<bool> _isPublished = new LazyField<bool>();
        private readonly LazyField<IContent> _publishedVersion = new LazyField<IContent>();
        private readonly LazyField<bool> _hasPublished = new LazyField<bool>();
        private readonly LazyField<bool> _isChanged = new LazyField<bool>();
        private readonly LazyField<bool> _isRemoved = new LazyField<bool>();
        private readonly LazyField<bool> _isCurrent = new LazyField<bool>();
        private readonly LazyField<bool> _isNew = new LazyField<bool>();
        private readonly LazyField<bool> _hasLatest = new LazyField<bool>();

        public LazyField<bool> IsDraftField { get { return _isDraft; } }
        public LazyField<bool> IsPublishedField { get { return _isPublished; } }
        public LazyField<IContent> PublishedVersionField { get { return _publishedVersion; } }
        public LazyField<bool> HasPublishedField { get { return _hasPublished; } }
        public LazyField<bool> IsChangedField { get { return _isChanged; } }
        public LazyField<bool> IsRemovedField { get { return _isRemoved; } }
        public LazyField<bool> IsCurrentField { get { return _isCurrent; } }
        public LazyField<bool> IsNewField { get { return _isNew; } }
        public LazyField<bool> HasLatestField { get { return _hasLatest; } }

        public string Text {
            get { return Record.Text; }
            set { Record.Text = value; }
        }

        public string Url {
            get { return Record.Url; }
            set { Record.Url = value; }
        }

        public string Classes {
            get { return Record.Classes; }
            set { Record.Classes = value; }
        }

        public string CssId {
            get { return Record.CssId; }
            set { Record.CssId = value; }
        }

        public bool DisplayHref {
            get { return Record.DisplayHref; }
            set { Record.DisplayHref = value; }
        }

        public bool DisplayText {
            get { return Record.DisplayText; }
            set { Record.DisplayText = value; }
        }

        public string Position
        {
            get { return Record.Position; }
            set
            {
                Record.Position = value; 
                Record.ParentPosition = ExtractParentPosition(value);
            }
        }

        public string ParentPosition
        {
            get { 
                // Auto-update parent positions if necessary
                if (HasPosition)
                {
                    var parentPosition = ExtractParentPosition(Position);
                    if (parentPosition != Record.ParentPosition)
                    {
                        Record.ParentPosition = parentPosition;
                    }
                }
                return Record.ParentPosition;
            }
        }

        private static string ExtractParentPosition(string position)
        {
            if (position == null)
                return null;

            var segments = position.Split(new[]{'.'}, StringSplitOptions.RemoveEmptyEntries).Where(s => !s.Equals("0")).ToArray();
            return segments.Length > 1 ? string.Join(".", segments.Take(segments.Length - 1)) : null;
        }

        [Obsolete]
        public string DraftPosition {
            get { return Record.DraftPosition; }
            set { Record.DraftPosition = value; }
        }

        public string Permission {
            get { return Record.Permission; }
            set { Record.Permission = value; }
        }

        public string TechnicalName {
            get { return Record.TechnicalName; }
            set { Record.TechnicalName = value; }
        }

        public string GroupName {
            get { return Record.GroupName; }
            set { Record.GroupName = value; }
        }

        public string SubTitle {
            get { return Record.SubTitle; }
            set { Record.SubTitle = value; }
        }

        public bool InNewWindow {
            get { return Record.InNewWindow; }
            set { Record.InNewWindow = value; }
        }

        public bool IsNew
        {
            get { return _isNew.Value; }
        }

        public bool IsDraft
        {
            get { return _isDraft.Value; }
        }

        public bool IsPublished
        {
            get { return _isPublished.Value; }
        }

        public IContent PublishedVersion
        {
            get { return _publishedVersion.Value; }
        }

        public bool HasPublished
        {
            get { return _hasPublished.Value; }
        }

        public bool IsChanged
        {
            get { return _isChanged.Value; }
        }

        public bool IsRemoved
        {
            get { return _isRemoved.Value; }
        }

        public bool IsCurrent
        {
            get { return _isCurrent.Value; }
        }

        public bool HasLatest
        {
            get { return _hasLatest.Value; }
        }

        public bool HasPosition
        {
            get
            {
                return !string.IsNullOrEmpty(Position) && Position != "0";
            }
        }

        /// <summary>
        /// Menu this item belongs to.
        /// </summary>
        public IContent Menu { get { return this.As<MenuPart>().Menu; } }

        /// <summary>
        /// Specific menu version this item belongs to. Null, if item hasn't been bound to a version yet.
        /// </summary>
        public IContent MenuVersion { get { return _menuVersion.Value; } set { _menuVersion.Value = value; } }

        public override string ToString()
        {
            return string.Format("{0}: ({2}) {1}", Position, Text, ContentItem.ContentType);
        }
    }
}