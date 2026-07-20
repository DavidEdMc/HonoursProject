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

                string query = @"SELECT id, title, description, difficulty, order_index, is_active, icon_path, content_html, is_exam
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
                            is_active = SafeInt(reader["is_active"]) == 1,
                            IconPath = SafeString(reader["icon_path"]),
                            ContentHtml = SafeString(reader["content_html"]),
                            is_exam = SafeInt(reader["is_exam"]) == 1
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

                string query = @"SELECT id, title, description, difficulty, order_index, is_active, icon_path, content_html, is_exam
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
                                is_active = SafeInt(reader["is_active"]) == 1,
                                IconPath = SafeString(reader["icon_path"]),
                                ContentHtml = SafeString(reader["content_html"]),
                                is_exam = SafeInt(reader["is_exam"]) == 1
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

        public void RecordQuestionAttempt(int userId, int questionId, int optionId, bool isCorrect, int timeTaken, int attempts)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    INSERT INTO tb_user_question_attempts
                    (user_id, question_id, selected_option_id, is_correct, time_taken_seconds, attempts)
                    VALUES (@userId, @questionId, @optionId, @isCorrect, @timeTaken, @attempts);
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@questionId", questionId);
                    cmd.Parameters.AddWithValue("@optionId", optionId);
                    cmd.Parameters.AddWithValue("@isCorrect", isCorrect ? 1 : 0);
                    cmd.Parameters.AddWithValue("@timeTaken", timeTaken);
                    cmd.Parameters.AddWithValue("@attempts", attempts);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int CalculateLessonScore(int userId, int lessonId)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT SUM(q.points)
                    FROM tb_user_question_attempts a
                    JOIN tb_questions q ON a.question_id = q.id
                    WHERE a.user_id = @userId AND q.lesson_id = @lessonId AND a.is_correct = 1;
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@lessonId", lessonId);

                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
        }

        public void SaveLessonHistory(int userId, int lessonId, int score, int durationSeconds)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    INSERT INTO tb_user_lesson_history
                    (user_id, lesson_id, score, time_taken_seconds, completed_at)
                    VALUES (@userId, @lessonId, @score, @duration, @completedAt);
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@lessonId", lessonId);
                    cmd.Parameters.AddWithValue("@score", score);
                    cmd.Parameters.AddWithValue("@duration", durationSeconds);
                    cmd.Parameters.AddWithValue("@completedAt", DateTime.UtcNow);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddXp(int userId, int xp)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    UPDATE tb_user_xp
                    SET xp = xp + @xp
                    WHERE user_id = @userId;
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@xp", xp);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateUserStats(int userId, int lessonId, int score, int durationSeconds)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    UPDATE tb_user_stats
                    SET lessons_completed = lessons_completed + 1,
                        score = score + @score,
                        fastest_lesson_seconds = LEAST(fastest_lesson_seconds, @duration),
                        last_lesson_date = @completedAt,
                        current_streak = CASE 
                            WHEN DATE(last_lesson_date) = DATE_SUB(CURDATE(), INTERVAL 1 DAY)
                            THEN current_streak + 1
                            ELSE 1
                        END,
                        highest_streak = GREATEST(highest_streak, current_streak)
                    WHERE user_id = @userId;
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@score", score);
                    cmd.Parameters.AddWithValue("@duration", durationSeconds);
                    cmd.Parameters.AddWithValue("@completedAt", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public async Task<List<int>> GetCompletedLessonIdsAsync(int userId)
        {
            var completed = new List<int>();

            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT DISTINCT lesson_id
                    FROM tb_user_lesson_history
                    WHERE user_id = @userId;
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            completed.Add(SafeInt(reader["lesson_id"]));
                        }
                    }
                }
            }

            return completed;
        }
    }
}
