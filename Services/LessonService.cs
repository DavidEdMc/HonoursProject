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

        public void RecordQuestionAttempt(int userId, int lessonId, int questionId, int optionId, bool isCorrect, int timeTaken, int attempts, int runId)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    INSERT INTO tb_user_question_attempts
                    (user_id, lesson_id, question_id, selected_option_id, is_correct, time_taken_seconds, attempts, run_id)
                    VALUES (@userId, @lessonId, @questionId, @optionId, @isCorrect, @timeTaken, @attempts, @runId);
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@lessonId", lessonId);
                    cmd.Parameters.AddWithValue("@questionId", questionId);
                    cmd.Parameters.AddWithValue("@optionId", optionId);
                    cmd.Parameters.AddWithValue("@isCorrect", isCorrect ? 1 : 0);
                    cmd.Parameters.AddWithValue("@timeTaken", timeTaken);
                    cmd.Parameters.AddWithValue("@attempts", attempts);
                    cmd.Parameters.AddWithValue("@runId", runId);   // NEW

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int CountCorrectAnswers(int userId, int lessonId, int runId)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT COUNT(*)
                    FROM tb_user_question_attempts a
                    JOIN tb_questions q ON a.question_id = q.id
                    WHERE a.user_id = @userId 
                    AND q.lesson_id = @lessonId 
                    AND a.run_id = @runId
                    AND a.is_correct = 1;
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@lessonId", lessonId);
                    cmd.Parameters.AddWithValue("@runId", runId);

                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
        }

        public int CountIncorrectAnswers(int userId, int lessonId, int runId)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT COUNT(*)
                    FROM tb_user_question_attempts a
                    JOIN tb_questions q ON a.question_id = q.id
                    WHERE a.user_id = @userId
                    AND q.lesson_id = @lessonId
                    AND a.run_id = @runId
                    AND a.is_correct = 0;
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@lessonId", lessonId);
                    cmd.Parameters.AddWithValue("@runId", runId);

                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
        }

        public int CalculateLessonScore(int userId, int lessonId, int runId)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT SUM(q.points)
                    FROM tb_user_question_attempts a
                    JOIN tb_questions q ON a.question_id = q.id
                    WHERE a.user_id = @userId
                    AND q.lesson_id = @lessonId
                    AND a.run_id = @runId
                    AND a.is_correct = 1;
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@lessonId", lessonId);
                    cmd.Parameters.AddWithValue("@runId", runId);

                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
        }

        public int CreateNewRun(int userId, int lessonId)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    INSERT INTO tb_user_lesson_runs (user_id, lesson_id, started_at)
                    VALUES (@userId, @lessonId, NOW());
                    SELECT LAST_INSERT_ID();
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@lessonId", lessonId);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public async Task SaveLessonHistory(int userId, int lessonId, int score, int durationSeconds)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                INSERT INTO tb_user_lesson_history (user_id, lesson_id, score, time_taken_seconds, completed_at)
                VALUES (@u, @l, @s, @t, NOW());
            ";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@l", lessonId);
            cmd.Parameters.AddWithValue("@s", score);
            cmd.Parameters.AddWithValue("@t", durationSeconds);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddXp(int userId, int xpToAdd)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            // 1. Get current XP
            string getQuery = @"
                SELECT xp
                FROM tb_user_xp
                WHERE user_id = @u;
            ";

            var getCmd = new MySqlCommand(getQuery, conn);
            getCmd.Parameters.AddWithValue("@u", userId);

            object result = await getCmd.ExecuteScalarAsync();
            int currentXp = result != null ? Convert.ToInt32(result) : 0;

            // 2. Add XP
            int newXp = currentXp + xpToAdd;

            // 3. Calculate new level
            int newLevel = (newXp / 100) + 1;

            // 4. Save XP + Level
            string updateQuery = @"
                UPDATE tb_user_xp
                SET xp = @xp, level = @lvl
                WHERE user_id = @u;
            ";

            var updateCmd = new MySqlCommand(updateQuery, conn);
            updateCmd.Parameters.AddWithValue("@xp", newXp);
            updateCmd.Parameters.AddWithValue("@lvl", newLevel);
            updateCmd.Parameters.AddWithValue("@u", userId);

            await updateCmd.ExecuteNonQueryAsync();
        }

        public void UpdateUserStats(int userId, int lessonId, int score, int durationSeconds)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                string query = @"
                    INSERT INTO tb_user_stats (
                        user_id, lessons_completed, score, fastest_lesson_seconds,
                        last_lesson_date, current_streak, highest_streak
                    )
                    VALUES (
                        @userId, 1, @score, @duration, @completedAt, 1, 1
                    )
                    ON DUPLICATE KEY UPDATE
                        lessons_completed = lessons_completed + 1,
                        score = score + @score,
                        fastest_lesson_seconds = LEAST(fastest_lesson_seconds, @duration),
                        last_lesson_date = @completedAt,
                        current_streak = CASE 
                            WHEN DATE(last_lesson_date) = DATE_SUB(CURDATE(), INTERVAL 1 DAY)
                            THEN current_streak + 1
                            ELSE 1
                        END,
                        highest_streak = GREATEST(highest_streak, current_streak);
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

        public async Task<int> GetTotalLessonsCompletedAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM tb_user_lesson_history WHERE user_id = @u", conn);
            cmd.Parameters.AddWithValue("@u", userId);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int> GetTotalQuestionsAnsweredAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM tb_user_question_attempts WHERE user_id = @u",
                conn
            );

            cmd.Parameters.AddWithValue("@u", userId);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int> GetLastLessonCompletionTimeAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT HOUR(completed_at)
                FROM tb_user_lesson_history
                WHERE user_id = @u
                ORDER BY completed_at DESC
                LIMIT 1;
            ";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);

            object result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<int> GetLastLessonScoreAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT score
                FROM tb_user_lesson_history
                WHERE user_id = @u
                ORDER BY completed_at DESC
                LIMIT 1;
            ";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);

            object result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<int> GetUserLevelAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT level
                FROM tb_user_xp
                WHERE user_id = @u;
            ";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);

            object result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 1;
        }

        public async Task<int> GetUserStreakAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT DATE(completed_at)
                FROM tb_user_lesson_history
                WHERE user_id = @u
                ORDER BY completed_at DESC;
            ";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);

            var dates = new List<DateTime>();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    dates.Add(reader.GetDateTime(0).Date);
                }
            }

            if (dates.Count == 0)
                return 0;

            int streak = 1;
            DateTime current = dates[0];

            for (int i = 1; i < dates.Count; i++)
            {
                if (dates[i] == current.AddDays(-1))
                {
                    streak++;
                    current = dates[i];
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        public async Task<int> GetLastLessonDurationAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT time_taken_seconds
                FROM tb_user_lesson_history
                WHERE user_id = @u
                ORDER BY completed_at DESC
                LIMIT 1;
            ";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);

            object result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 99999;
        }

        public async Task<bool> HasCompletedLessonAsync(int userId, int lessonId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT COUNT(*)
                FROM tb_user_lesson_history
                WHERE user_id = @u AND lesson_id = @l;
            ";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@l", lessonId);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }

        public async Task<int> GetLastCompletedLessonIdAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT lesson_id
                FROM tb_user_lesson_history
                WHERE user_id = @u
                ORDER BY completed_at DESC
                LIMIT 1;
            ";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);

            var result = await cmd.ExecuteScalarAsync();
            return result == null ? 0 : Convert.ToInt32(result);
        }

        public async Task<int> GetTotalQuestionsForLessonAsync(int lessonId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = "SELECT COUNT(*) FROM tb_questions WHERE lesson_id = @l";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@l", lessonId);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int> GetUserXpAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT xp
                FROM tb_user_xp
                WHERE user_id = @u;
            ";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);

            object result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<int> GetFirstTryCorrectCountAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(@"
                SELECT COUNT(*)
                FROM tb_user_question_attempts
                WHERE user_id = @u AND attempts = 1 AND is_correct = 1;
            ", conn);

            cmd.Parameters.AddWithValue("@u", userId);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> DidUserPerfectFirstPassAsync(int userId, int lessonId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(@"
                SELECT COUNT(*)
                FROM tb_user_question_attempts qa
                JOIN tb_questions q ON qa.question_id = q.id
                WHERE qa.user_id = @u
                AND q.lesson_id = @l
                AND qa.attempts = 1
                AND qa.is_correct = 1;
            ", conn);

            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@l", lessonId);

            int firstTryCorrect = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            int totalQuestions = await GetTotalQuestionsForLessonAsync(lessonId);

            return firstTryCorrect == totalQuestions;
        }
    }
}
