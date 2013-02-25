namespace Onestop.Navigation.Models {
    /// <summary>
    /// Display modes for a menu widget.
    /// </summary>
    public enum MenuWidgetMode {
        /// <summary>
        /// Default mode. Displays all items in hierarchy.
        /// </summary>
        AllItems, 

        /// <summary>
        /// Display sub-items of an item matching the current URL only or an empty list if none found.
        /// </summary>
        ChildrenOnly, 

        /// <summary>
        /// Displays siblings of an item matching the current URL only or behaves like AllItems if none found.
        /// All other items are removed from displaying so siblings make the first level of the menu.
        /// </summary>
        SiblingsOnly, 

        /// <summary>
        /// Displays siblings of an item matching the current URL only. 
        /// All subtrees not containing a matching item are collapsed up to the top, so if no match is found this mode displays only the root items.
        /// </summary>
        SiblingsExpanded
    }
}