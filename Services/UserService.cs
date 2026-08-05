using MySqlConnector;
using HonoursProject.Models;
using BCrypt.Net;

namespace HonoursProject.Services
{
    public class UserService
    {
        private readonly DatabaseService _db;

        public UserService(DatabaseService db)
        {
            _db = db;
        }

        public User GetUserByUsername(string username)
        {
            User user = null;

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = "SELECT * FROM tb_users WHERE username = @username LIMIT 1";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                Password = reader.GetString("password"),
                                EmailAddress = reader.GetString("email_address"),
                                ProfilePicture = reader.GetString("profile_picture")
                            };
                        }
                    }
                }
            }

            return user;
        }

        public User GetUserByEmail(string email)
        {
            string query = "SELECT * FROM tb_users WHERE email_address = @email";

            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                EmailAddress = reader.GetString("email_address"),
                                Password = reader.GetString("password"),
                                ProfilePicture = reader.GetString("profile_picture")
                            };
                        }
                    }
                }
            }

            return null;
        }


        public bool VerifyPassword(string plain, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(plain, hash);
        }

        public string HashPassword(string plain)
        {
            return BCrypt.Net.BCrypt.HashPassword(plain);
        }

        public void UpdatePassword(string username, string newHash)
        {
            string query = "UPDATE tb_users SET password = @password WHERE username = @username";

            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@password", newHash);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateProfile(User user)
        {
            string query = @"UPDATE tb_users 
                            SET username = @username, 
                                email_address = @email, 
                                profile_picture = @picture 
                            WHERE id = @id";

            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@email", user.EmailAddress);
                    cmd.Parameters.AddWithValue("@picture", user.ProfilePicture);
                    cmd.Parameters.AddWithValue("@id", user.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void CreateUser(User user)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                long newUserId;

                // 1. Insert user
                string userQuery = @"
                    INSERT INTO tb_users (username, email_address, password, profile_picture)
                    VALUES (@username, @email, @password, @picture);
                ";

                using (var cmd = new MySqlCommand(userQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@email", user.EmailAddress);
                    cmd.Parameters.AddWithValue("@password", user.Password);
                    cmd.Parameters.AddWithValue("@picture", user.ProfilePicture);

                    cmd.ExecuteNonQuery();

                    // Capture the new user ID BEFORE leaving the using block
                    newUserId = cmd.LastInsertedId;
                }

                // 2. Insert XP row for the new user
                string xpQuery = @"
                    INSERT INTO tb_user_xp (user_id, xp)
                    VALUES (@userId, 0);
                ";

                using (var xpCmd = new MySqlCommand(xpQuery, conn))
                {
                    xpCmd.Parameters.AddWithValue("@userId", newUserId);
                    xpCmd.ExecuteNonQuery();
                }
            }
        }
        
        public UserStats GetStatsByUserId(int userId)
        {
            string query = "SELECT * FROM tb_user_stats WHERE user_id = @userId";

            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserStats
                            {
                                Id = reader.GetInt32("id"),
                                UserId = reader.GetInt32("user_id"),
                                LessonsCompleted = reader.GetInt32("lessons_completed"),
                                Score = reader.GetInt32("score"),
                                FastestLessonSeconds = reader.IsDBNull(reader.GetOrdinal("fastest_lesson_seconds"))
                                    ? 0
                                    : reader.GetInt32("fastest_lesson_seconds"),

                                CurrentStreak = reader.GetInt32("current_streak"),
                                HighestStreak = reader.GetInt32("highest_streak"),
                                LastLessonDate = reader.IsDBNull(reader.GetOrdinal("last_lesson_date"))
                                    ? null
                                    : reader.GetDateTime("last_lesson_date")
                            };
                        }
                    }
                }
            }

            return null;
        }

        public void UpdateStats(UserStats stats)
        {
            string query = @"UPDATE tb_user_stats 
                            SET lessons_completed = @lessons,
                                score = @score,
                                fastest_lesson_seconds = @fastest,
                                current_streak = @current,
                                highest_streak = @highest,
                                last_lesson_date = @lastDate
                            WHERE user_id = @userId";

            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@lessons", stats.LessonsCompleted);
                    cmd.Parameters.AddWithValue("@score", stats.Score);
                    cmd.Parameters.AddWithValue("@fastest", stats.FastestLessonSeconds);
                    cmd.Parameters.AddWithValue("@current", stats.CurrentStreak);
                    cmd.Parameters.AddWithValue("@highest", stats.HighestStreak);
                    cmd.Parameters.AddWithValue("@lastDate", stats.LastLessonDate);
                    cmd.Parameters.AddWithValue("@userId", stats.UserId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<LeaderboardEntry> GetLeaderboard()
        {
            string query = @"
                SELECT 
                    u.username,
                    s.score,
                    s.lessons_completed,
                    s.highest_streak,
                    s.fastest_lesson_seconds,
                    xp.level
                FROM tb_user_stats s
                JOIN tb_users u ON u.id = s.user_id
                JOIN tb_user_xp xp ON xp.user_id = u.id
                ORDER BY s.score DESC
                LIMIT 10;
            ";

            var list = new List<LeaderboardEntry>();

            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new LeaderboardEntry
                        {
                            Username = reader.GetString("username"),
                            Score = reader.GetInt32("score"),
                            LessonsCompleted = reader.GetInt32("lessons_completed"),
                            HighestStreak = reader.GetInt32("highest_streak"),
                            FastestLessonSeconds = reader.IsDBNull(reader.GetOrdinal("fastest_lesson_seconds"))
                                ? 0
                                : reader.GetInt32("fastest_lesson_seconds"),
                            Level = reader.GetInt32("level")
                        });
                    }
                }
            }

            return list;
        }
    }
}
