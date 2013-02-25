using Orchard.Data.Conventions;

namespace Onestop.Navigation.Breadcrumbs.Models
{
    /// <summary>
    /// Describes a record used for storing route patter used for matching URLs and choosing appropriate breadcrumbs provider.
    /// </summary>
    public class RoutePatternRecord
    {
        public virtual int Id { get; set; }
        public virtual int Priority { get; set; }
        [StringLengthMax]
        public virtual string Pattern { get; set; }
        public virtual string Provider { get; set; }
    }
}