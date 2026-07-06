namespace HonoursProject.Models
{
    public class QuestionOption
    {
        public int id { get; set; }
        public int question_id { get; set; }
        public string option_text { get; set; }
        public string match_key { get; set; }
        public bool is_correct { get; set; }
        public int order_position { get; set; }
    }
}
