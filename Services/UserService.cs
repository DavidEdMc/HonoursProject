using MySql.Data.MySqlClient;
using HonoursProject.Models;

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
    }
}
