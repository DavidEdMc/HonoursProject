using MySql.Data.MySqlClient;
using HonoursProject.Models;

namespace HonoursProject.Services
{
    public class LessonService
    {
        private readonly DatabaseService _db;

        public LessonService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<List<Lesson>> GetAllLessonsAsync()
        {
            var lessons = new List<Lesson>();

            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                string query = @"SELECT id, title, description, difficulty, order_index, is_active
                                FROM tb_lessons
                                ORDER BY order_index";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lessons.Add(new Lesson
                        {
                            id = Convert.ToInt32(reader["id"]),
                            title = reader["title"].ToString(),
                            description = reader["description"].ToString(),
                            difficulty = reader["difficulty"].ToString(),
                            order_index = Convert.ToInt32(reader["order_index"]),
                            is_active = Convert.ToInt32(reader["is_active"]) == 1
                        });
                    }
                }
            }

            return lessons;
        }

        public async Task<Lesson?> GetLessonByIdAsync(int id)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                string query = @"SELECT id, title, description, difficulty, order_index, is_active
                                FROM tb_lessons
                                WHERE id = @id";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Lesson
                            {
                                id = Convert.ToInt32(reader["id"]),
                                title = reader["title"].ToString(),
                                description = reader["description"].ToString(),
                                difficulty = reader["difficulty"].ToString(),
                                order_index = Convert.ToInt32(reader["order_index"]),
                                is_active = Convert.ToInt32(reader["is_active"]) == 1
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
