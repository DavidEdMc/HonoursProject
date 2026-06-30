namespace HonoursProject.Models
{
    public class LeaderboardEntry
    {
        public string Username { get; set; }
        public int Score { get; set; }
        public int LessonsCompleted { get; set; }
        public int HighestStreak { get; set; }
        public int FastestLessonSeconds { get; set; }
    }
}