namespace HonoursProject.Models
{
    public class DashboardViewModel
    {
        public string Username { get; set; }
        public int LessonsCompleted { get; set; }
        public int AchievementsEarned { get; set; }
        public int DayStreak { get; set; }
        public List<LeaderboardEntry> Leaderboard { get; set; }
    }
}
