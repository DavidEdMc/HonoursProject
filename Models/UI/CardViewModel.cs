using Microsoft.AspNetCore.Html;

namespace HonoursProject.Models.UI
{
    public class CardViewModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string ImageUrl { get; set; }
        public IHtmlContent Content { get; set; }
        public IHtmlContent Footer { get; set; }
        public string Variant { get; set; } = "";
        public string CustomClasses { get; set; } = "";
        public string ButtonText { get; set; }
        public string ButtonUrl { get; set; }
    }
}
