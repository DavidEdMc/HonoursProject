namespace HonoursProject.Models
{
    public class ProfileViewModel
    {
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string ProfilePicture { get; set; }
        public int CurrentXp { get; set; }
        public int Level { get; set; }
        public int XpToNextLevel { get; set; }
        public int LessonsCompleted { get; set; }
        public int AchievementsEarned { get; set; }
        public int DayStreak { get; set; }
    }
}