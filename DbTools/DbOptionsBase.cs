using CommandLine;

namespace DbTools
{
    public class DbOptionsBase
    {
        public DbOptionsBase()
        {
        }

        public DbOptionsBase(string server, string user, string password)
        {
            Server = server;
            User = user;
            Password = password;
        }

        [Option('s', Required = true, HelpText = "The SQL Server instance to connect to.")]
        public string Server { get; set; }
        [Option('u', Required = false, HelpText = "Login user name for the server. If omitted, Windows Authentication is used.")]
        public string User { get; set; }
        [Option('p', Required = false, HelpText = "Login password.")]
        public string Password { get; set; }
    }
}
