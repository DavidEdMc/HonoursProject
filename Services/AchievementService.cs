using HonoursProject.Models;
using MySql.Data.MySqlClient;

namespace HonoursProject.Services
{
    public class AchievementService
    {
        private readonly DatabaseService _db;
        private readonly LessonService _lessonService;

        public AchievementService(DatabaseService db, LessonService lessonService)
        {
            _db = db;
            _lessonService = lessonService;
        }

        public List<AchievementViewModel> GetAllAchievementsForUser(int userId)
        {
            var achievements = new List<AchievementViewModel>();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT a.id, a.name, a.description, a.category, a.requirement_value,
                        ua.id AS user_achievement_id
                    FROM tb_achievements a
                    LEFT JOIN tb_user_achievements ua
                        ON ua.achievement_id = a.id AND ua.user_id = @userId
                    WHERE a.is_active = 1
                    ORDER BY a.id;
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            achievements.Add(new AchievementViewModel
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Description = reader.GetString("description"),
                                Category = reader.GetString("category"),
                                RequirementValue = reader.GetInt32("requirement_value"),
                                IsUnlocked = !reader.IsDBNull(reader.GetOrdinal("user_achievement_id"))                                
                            });
                        }
                    }
                }
            }

            return achievements;
        }

        public async Task<List<AchievementViewModel>> UnlockAchievementsAsync(int userId, string category)
        {
            var unlocked = new List<AchievementViewModel>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string getQuery = @"
                SELECT id, name, description, requirement_value, metric_type
                FROM tb_achievements
                WHERE category = @cat AND is_active = 1;
            ";

            var getCmd = new MySqlCommand(getQuery, conn);
            getCmd.Parameters.AddWithValue("@cat", category);

            var achievementsToUnlock = new List<int>();

            using (var reader = await getCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int achievementId = reader.GetInt32(reader.GetOrdinal("id"));
                    int requirement = reader.GetInt32(reader.GetOrdinal("requirement_value"));
                    string metricType = reader.GetString(reader.GetOrdinal("metric_type"));

                    int userMetricValue = 0;

                    switch (metricType)
                    {
                        case "lessons_completed":
                            userMetricValue = await _lessonService.GetTotalLessonsCompletedAsync(userId);
                            break;

                        case "questions_answered":
                            userMetricValue = await _lessonService.GetTotalQuestionsAnsweredAsync(userId);
                            break;

                        case "achievements_earned":
                            userMetricValue = await GetUserAchievementCount(userId);
                            break;

                        case "score":
                            userMetricValue = await _lessonService.GetLastLessonScoreAsync(userId);
                            break;

                        case "user_level":
                            userMetricValue = await _lessonService.GetUserLevelAsync(userId);
                            break;

                        case "user_streak":
                            userMetricValue = await _lessonService.GetUserStreakAsync(userId);
                            break;

                        case "lesson_duration_seconds":
                            userMetricValue = await _lessonService.GetLastLessonDurationAsync(userId) < requirement ? requirement : 0;
                            break;

                        case "specific_lesson_completed":
                            userMetricValue = await _lessonService.HasCompletedLessonAsync(userId, requirement) ? requirement : 0;
                            break;

                        case "lesson_completed_time_before":
                            userMetricValue = await _lessonService.GetLastLessonCompletionTimeAsync(userId) < requirement ? requirement : 0;
                            break;

                        case "lesson_completed_time_after":
                            userMetricValue = await _lessonService.GetLastLessonCompletionTimeAsync(userId) >= requirement ? requirement : 0;
                            break;

                        case "perfect_score_on_lesson":
                            int lastLessonId = await _lessonService.GetLastCompletedLessonIdAsync(userId);
                            int lastScore = await _lessonService.GetLastLessonScoreAsync(userId);

                            // requirement_value = lessonId
                            userMetricValue = (lastLessonId == requirement && lastScore == await _lessonService.GetTotalQuestionsForLessonAsync(requirement))
                                ? requirement
                                : 0;
                            break;

                        default:
                            userMetricValue = 0;
                            break;
                    }

                    if (userMetricValue >= requirement)
                        achievementsToUnlock.Add(achievementId);
                }
            }

            // Insert + collect unlocked achievements
            foreach (var achievementId in achievementsToUnlock)
            {
                string insertQuery = @"
                    INSERT INTO tb_user_achievements (user_id, achievement_id, earned_at)
                    SELECT @u, @a, NOW()
                    WHERE NOT EXISTS (
                        SELECT 1 FROM tb_user_achievements
                        WHERE user_id = @u AND achievement_id = @a
                    );
                ";

                var insertCmd = new MySqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@u", userId);
                insertCmd.Parameters.AddWithValue("@a", achievementId);

                int rows = await insertCmd.ExecuteNonQueryAsync();

                if (rows > 0)
                {
                    // Fetch achievement details for pop-up
                    unlocked.Add(await GetAchievementByIdAsync(achievementId));
                }
            }

            return unlocked;
        }

        public async Task<AchievementViewModel> GetAchievementByIdAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT id, name, description, category
                FROM tb_achievements
                WHERE id = @id;
            ";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AchievementViewModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Description = reader.GetString(reader.GetOrdinal("description")),
                    Category = reader.GetString(reader.GetOrdinal("category"))
                };
            }

            return null;
        }

        public async Task<int> GetUserAchievementCount(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM tb_user_achievements WHERE user_id = @u", conn);
            cmd.Parameters.AddWithValue("@u", userId);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

    }
}
