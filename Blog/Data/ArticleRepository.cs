using Blog.Models;
using Microsoft.Data.Sqlite;
using System.Drawing.Printing;
using System.Xml.Linq;

namespace Blog.Data
{
    /// <summary>
    /// Implementation of <see cref="IArticleRepository"/> using SQLite as a persistence solution.
    /// </summary>
    public class ArticleRepository : IArticleRepository
    {
        private readonly string _connectionString;

        public ArticleRepository(DatabaseConfig _config)
        {
            _connectionString = _config.DefaultConnectionString ?? throw new ArgumentNullException("Connection string not found");
        }

        /// <summary>
        /// Creates the necessary tables for this application if they don't exist already.
        /// Should be called once when starting the service.
        /// </summary>
        public void EnsureCreated()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"CREATE TABLE IF NOT EXISTS Article(
	                Id INTEGER PRIMARY KEY NOT NULL ,
	                AuthorName VARCHAR(50) NOT NULL,
	                AuthorEmail VARCHAR(100) NOT NULL, 
	                Title VARCHAR(100) NOT NULL,
	                Content TEXT,
	                PublishedDate TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Comment(
	                Id INTEGER PRIMARY KEY NOT NULL,
	                ArticleId INTEGER NOT NULL,
	                Content TEXT NOT NULL,
	                PublishedDate TEXT NOT NULL,
	                FOREIGN KEY (ArticleId) REFERENCES Article(Id)
                );

                CREATE TABLE IF NOT EXISTS User(
	                Id INTEGER PRIMARY KEY NOT NULL,
	                Username VARCHAR(50) NOT NULL,
	                Email VARCHAR(100) NOT NULL,
	                Password  VARCHAR(100) NOT NULL,
                    Salt NVARCHAR(50) NOT NULL,
	                DateOfBirth  NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Sessions(
	                SessionID VARCHAR(64) PRIMARY KEY NOT NULL,
	                UserID INT NOT NULL,
	                CreatedAt DATETIME  NOT NULL,
	                LastActivity  DATETIME  NOT NULL, 
                    FOREIGN KEY (UserID) REFERENCES User(Id)
                );";
                command.ExecuteNonQuery();
            }
        }

        public IEnumerable<Article> GetAll()
        {
            var articles = new List<Article>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"SELECT * FROM Article";
                
                using var reader = command.ExecuteReader();
                while(reader.Read())
                {
                    var article = new Article
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        AuthorName = reader.GetString(reader.GetOrdinal("AuthorName")),
                        AuthorEmail = reader.GetString(reader.GetOrdinal("AuthorEmail")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Content = reader.GetString(reader.GetOrdinal("Content")),
                        PublishedDate = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("PublishedDate")))
                    };
                    articles.Add(article);
                }
            }
            return articles;
        }

        public int GetTotalArticles()
        {
            int totalArticles = 0;
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"SELECT COUNT(*) FROM Article";
                totalArticles = Convert.ToInt32(command.ExecuteScalar());
            }
            return totalArticles;
        }

        public IEnumerable<Article> GetByDateRange(DateTimeOffset startDate, DateTimeOffset endDate, int pageNumber, int pageSize)
        {
            var articles = new List<Article>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"SELECT * FROM Article
                WHERE PublishedDate >= $startDate AND PublishedDate <= $endDate
                ORDER BY PublishedDate DESC
                LIMIT $pageSize OFFSET $offset";


                command.Parameters.AddWithValue("$startDate", startDate.ToString("o"));
                command.Parameters.AddWithValue("$endDate", endDate.ToString("o"));
                command.Parameters.AddWithValue("$pageSize", pageSize);
                command.Parameters.AddWithValue("$offset", (pageNumber - 1) * pageSize);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var article = new Article
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        AuthorName = reader.GetString(reader.GetOrdinal("AuthorName")),
                        AuthorEmail = reader.GetString(reader.GetOrdinal("AuthorEmail")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Content = reader.GetString(reader.GetOrdinal("Content")),
                        PublishedDate = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("PublishedDate")))
                    };
                    articles.Add(article);
                }
            }
            return articles;
        }

        public Article? GetById(int id)
        {
            Article? article = null;
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"SELECT * FROM Article
                WHERE Id = $id";

                command.Parameters.AddWithValue("$id", id);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    article = new Article
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        AuthorName = reader.GetString(reader.GetOrdinal("AuthorName")),
                        AuthorEmail = reader.GetString(reader.GetOrdinal("AuthorEmail")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Content = reader.GetString(reader.GetOrdinal("Content")),
                        PublishedDate = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("PublishedDate")))
                    };
                }
            }
            return article;
        }

        public Article Create(Article article)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO Article (AuthorName, AuthorEmail, Title, Content, PublishedDate)
                    VALUES ($AuthorName, $AuthorEmail, $Title, $Content, $PublishedDate);
                    SELECT last_insert_rowid();
                ";

                command.Parameters.AddWithValue("$AuthorName", article.AuthorName);
                command.Parameters.AddWithValue("$AuthorEmail", article.AuthorEmail);
                command.Parameters.AddWithValue("$Title", article.Title);
                command.Parameters.AddWithValue("$Content", article.Content);
                command.Parameters.AddWithValue("$PublishedDate", article.PublishedDate.ToString("o"));

                var newId = (long)command.ExecuteScalar();
                article.Id = (int)newId;
            }
            return article;
        }

        public void AddComment(Comment comment)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO Comment (ArticleId, Content, PublishedDate)
                    VALUES ($ArticleId, $Content, $PublishedDate);
                    SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("$ArticleId", comment.ArticleId);
                command.Parameters.AddWithValue("$Content", comment.Content);
                command.Parameters.AddWithValue("$PublishedDate", comment.PublishedDate.ToString("o"));
                var newId = (long)command.ExecuteScalar();
                comment.Id = (int)newId;
            }
        }

        public IEnumerable<Comment> GetCommentsByArticleId(int articleId)
        {
            var comments = new List<Comment>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"SELECT * FROM Comment
                WHERE ArticleId = $articleId";

                command.Parameters.AddWithValue("$articleId", articleId);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var comment = new Comment()
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        ArticleId = reader.GetInt32(reader.GetOrdinal("ArticleId")),
                        Content = reader.GetString(reader.GetOrdinal("Content")),
                        PublishedDate = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("PublishedDate")))
                    };
                    comments.Add(comment);
                }
            }
            return comments;
        }

        public User CreateUser(User user)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO User (Username, Email, Password, Salt, DateOfBirth )
                    VALUES ($Username, $Email, $Password,$Salt, $DateOfBirth);
                    SELECT last_insert_rowid();
                ";

                command.Parameters.AddWithValue("$Username", user.Username);
                command.Parameters.AddWithValue("$Email", user.Email);
                command.Parameters.AddWithValue("$Password", user.Password);
                command.Parameters.AddWithValue("$Salt", user.Salt);
                command.Parameters.AddWithValue("$DateOfBirth", user.DateOfBirth.ToString("o"));

                var newId = (long)command.ExecuteScalar();
                user.Id = (int)newId;
            }
            return user;
        }

        public User? GetUserByUsername(string username)
        {
            User? user = null;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"SELECT * FROM User
                WHERE Username = $username";

                command.Parameters.AddWithValue("$username", username);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    user = new User
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Password = reader.GetString(reader.GetOrdinal("Password")),
                        Salt = (byte[])reader["Salt"],
                        DateOfBirth = DateTime.Parse(reader.GetString(reader.GetOrdinal("DateOfBirth")))
                    };
                }
            }
            return user;

        }

        public Session? GetSession(string sessionID)
        {
            Session? session = null;
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"SELECT * FROM Sessions
                WHERE SessionID = $sessionID";

                command.Parameters.AddWithValue("$sessionID", sessionID);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    session = new Session
                    {
                        SessionID = reader.GetString(reader.GetOrdinal("SessionID")),
                        UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                        CreatedAt = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                        LastActivity = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("LastActivity")))
                    };
                }
            }
            return session;
        }

        public void CreateSession(Session session)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO Sessions (SessionID, UserID, CreatedAt, LastActivity)
                    VALUES ($SessionID, $UserID, $CreatedAt, $LastActivity);
                ";
                command.Parameters.AddWithValue("$SessionID", session.SessionID);
                command.Parameters.AddWithValue("$UserID", session.UserID);
                command.Parameters.AddWithValue("$CreatedAt", session.CreatedAt.ToString("o"));
                command.Parameters.AddWithValue("$LastActivity", session.LastActivity.ToString("o"));
                command.ExecuteNonQuery();
            }
        }

        public void RemoveSession(string sessionID)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"
                    DELETE FROM Sessions
                    WHERE SessionID = $SessionID;
                ";
                command.Parameters.AddWithValue("$SessionID", sessionID);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateSessionActivity(Session session)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"
                    UPDATE Sessions
                    SET LastActivity = $LastActivity
                    WHERE SessionID = $SessionID;
                ";
                command.Parameters.AddWithValue("$SessionID", session.SessionID);
                command.Parameters.AddWithValue("$LastActivity", session.LastActivity.ToString("o"));
                command.ExecuteNonQuery();
            }
        }

        public User? GetUserById(int userId)
        {
            User? user = null;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                @"SELECT * FROM User
                WHERE Id = $userId";

                command.Parameters.AddWithValue("$userId", userId);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    user = new User
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Password = reader.GetString(reader.GetOrdinal("Password")),
                        Salt = (byte[])reader["Salt"],
                        DateOfBirth = DateTime.Parse(reader.GetString(reader.GetOrdinal("DateOfBirth")))
                    };
                }
            }
            return user;
        }
    }
}
