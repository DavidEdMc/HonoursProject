using HonoursProject.Models;
using MySql.Data.MySqlClient;

namespace HonoursProject.Services
{
    public class AchievementService
    {
        private readonly DatabaseService _db;

        public AchievementService(DatabaseService db)
        {
            _db = db;
        }

        public List<AchievementViewModel> GetAllAchievementsForUser(int userId)
        {
            var achievements = new List<AchievementViewModel>();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = "SELECT * FROM tb_achievements ORDER BY id";

                using (var cmd = new MySqlCommand(query, conn))
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
                            IsUnlocked = false // default until we add logic
                        });
                    }
                }
            }

            return achievements;
        }
    }
}
