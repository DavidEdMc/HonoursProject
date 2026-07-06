namespace HonoursProject.Models
{
    public class Lesson
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string difficulty { get; set; }
        public int order_index { get; set; }
        public bool is_active { get; set; }
    }
}
