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

        private int SafeInt(object value)
        {
            return value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private string SafeString(object value)
        {
            return value == DBNull.Value ? "" : value.ToString();
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
                            id = SafeInt(reader["id"]),
                            title = SafeString(reader["title"]),
                            description = SafeString(reader["description"]),
                            difficulty = SafeString(reader["difficulty"]),
                            order_index = SafeInt(reader["order_index"]),
                            is_active = SafeInt(reader["is_active"]) == 1
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
                                id = SafeInt(reader["id"]),
                                title = SafeString(reader["title"]),
                                description = SafeString(reader["description"]),
                                difficulty = SafeString(reader["difficulty"]),
                                order_index = SafeInt(reader["order_index"]),
                                is_active = SafeInt(reader["is_active"]) == 1
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<List<Question>> GetQuestionsForLessonAsync(int lessonId)
        {
            var questions = new List<Question>();

            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                string query = @"SELECT id, lesson_id, question_text, question_type, 
                                        time_limit_seconds, points
                                FROM tb_questions
                                WHERE lesson_id = @lessonId
                                ORDER BY id";   // fallback ordering

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@lessonId", lessonId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            questions.Add(new Question
                            {
                                id = SafeInt(reader["id"]),
                                lesson_id = SafeInt(reader["lesson_id"]),
                                question_text = SafeString(reader["question_text"]),
                                question_type = SafeString(reader["question_type"]),
                                time_limit_seconds = SafeInt(reader["time_limit_seconds"]),
                                points = SafeInt(reader["points"])
                            });
                        }
                    }
                }

                // Load options for each question
                foreach (var q in questions)
                {
                    q.Options = await GetOptionsForQuestionAsync(q.id);
                }
            }

            return questions;
        }

        private async Task<List<QuestionOption>> GetOptionsForQuestionAsync(int questionId)
        {
            var options = new List<QuestionOption>();

            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                string query = @"SELECT id, question_id, option_text, match_key, 
                                        is_correct, order_position
                                FROM tb_question_options
                                WHERE question_id = @questionId
                                ORDER BY order_position";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@questionId", questionId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            options.Add(new QuestionOption
                            {
                                id = SafeInt(reader["id"]),
                                question_id = SafeInt(reader["question_id"]),
                                option_text = SafeString(reader["option_text"]),
                                match_key = SafeString(reader["match_key"]),
                                is_correct = SafeInt(reader["is_correct"]) == 1,
                                order_position = SafeInt(reader["order_position"])
                            });
                        }
                    }
                }
            }

            return options;
        }
    }
}
