namespace Blog.Models
{
    public class Session
    {
        public string SessionID { get; set; }
        public int UserID { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LastActivity { get; set; }
    }
}
