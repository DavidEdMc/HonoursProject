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
            string query = @"INSERT INTO tb_users (username, email_address, password, profile_picture)
                            VALUES (@username, @email, @password, @picture)";

            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@email", user.EmailAddress);
                    cmd.Parameters.AddWithValue("@password", user.Password);
                    cmd.Parameters.AddWithValue("@picture", user.ProfilePicture);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
