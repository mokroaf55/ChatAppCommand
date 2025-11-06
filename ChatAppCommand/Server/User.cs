namespace ChatServerApp
{
    public class User
    {
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBanned { get; set; }

        public User(string userName, bool isAdmin = false)
        {
            UserName = userName;
            IsAdmin = isAdmin;
            IsBanned = false;
        }
    }
}
