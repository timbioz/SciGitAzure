using System.Collections.Generic;

namespace Onestop.Navigation.Breadcrumbs.Services
{
    /// <summary>
    /// Describes a breadcrumb segment.
    /// </summary>
    public class Breadcrumbs
    {
        private readonly LinkedList<Segment> _segments;

        public Breadcrumbs()
        {
            _segments = new LinkedList<Segment>();
        }

        public BreadcrumbsContext Context { get; set; }

        public IEnumerable<Segment> Segments
        {
            get { return _segments; }
        }

        public Breadcrumbs Append(Segment segment)
        {
            _segments.AddLast(segment);
            return this;
        }

        public Breadcrumbs Prepend(Segment segment)
        {
            _segments.AddFirst(segment);
            return this;
        }

        public static Breadcrumbs Empty { get { return new Breadcrumbs(); } }
    }
}