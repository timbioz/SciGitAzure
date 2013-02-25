namespace Onestop.Navigation.Breadcrumbs.Models
{
    public class RoutePattern
    {
        public int Id { get; set; }
        public int Priority { get; set; }
        public string Pattern { get; set; }
        public string Provider { get; set; }
        public bool Editable { get; set; }
        public bool Global { get; set; }
    }
}