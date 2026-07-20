namespace HonoursProject.Models
{
    public class LessonProgressViewModel
    {
        public Lesson Lesson { get; set; }

        // Whether the user has completed this lesson
        public bool IsCompleted { get; set; }

        // Whether this lesson is unlocked (computed in controller)
        public bool IsUnlocked { get; set; }
    }
}
