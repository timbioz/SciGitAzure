using System.Collections.Generic;
using galgodage.TopCommented.Models;

namespace galgodage.TopCommented.ViewModels {
    public class galgodageTopCommentedWidgetViewModel {
        public galgodageTopCommentedWidgetPart Part { get; set; }
        public IEnumerable<string> ContentPartNames { get; set; }
    }
}