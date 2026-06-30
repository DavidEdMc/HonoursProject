using MySql.Data.MySqlClient;
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

    }
}
