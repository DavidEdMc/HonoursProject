namespace HonoursProject.Models
{
    public class UserStats
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LessonsCompleted { get; set; }
        public int Score { get; set; }
        public int FastestLessonSeconds { get; set; }

        public int CurrentStreak { get; set; }
        public int HighestStreak { get; set; }
        public DateTime? LastLessonDate { get; set; }
    }

}
