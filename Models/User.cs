namespace HonoursProject.Models
{
    public class User
    {
        public int Id { get; set; }               // auto-increment PK
        public string Username { get; set; }      // VARCHAR
        public string Password { get; set; }      // hashed password
        public string EmailAddress { get; set; }  // VARCHAR
        public string ProfilePicture { get; set; } // URL path to image
    }
}
