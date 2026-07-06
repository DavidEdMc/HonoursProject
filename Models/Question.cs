namespace HonoursProject.Models
{
    public class Question
    {
        public int id { get; set; }
        public int lesson_id { get; set; }
        public string question_text { get; set; }
        public string question_type { get; set; }
        public int time_limit_seconds { get; set; }
        public int points { get; set; }

        public List<QuestionOption> Options { get; set; } = new();
    }
}
