using System.Data;
using Microsoft.Data.Sqlite;

namespace Database
{
    static class Connection
    {
        private static readonly string connectionString = "Data Source=database.sqlite";
        private static readonly SqliteConnection connection;

        static Connection()
        {
            connection = new SqliteConnection(connectionString);
            connection.Open();
            InitializeSchema();
        }

        private static void InitializeSchema()
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS auth (
                    nickname TEXT PRIMARY KEY,
                    hash TEXT NOT NULL,
                    salt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS message (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    author_nickname TEXT,
                    topic TEXT NOT NULL,
                    content INTEGER NOT NULL,
                    creation_date DATETIME NOT NULL,
                    FOREIGN KEY (author_nickname) REFERENCES auth (nickname)
                );
            ";
            command.ExecuteNonQuery();

        }

        public static List<Message> GetMessages(string topic)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id, author_nickname, topic, content, creation_date FROM message WHERE topic = $topic";
            command.Parameters.AddWithValue("$topic", topic);

            using SqliteDataReader reader = command.ExecuteReader();

            List<Message> results = new List<Message>();
            while (reader.Read())
            {
                results.Add(new Message
                {
                    Id = reader.GetInt64(0),
                    AuthorNickname = reader.GetString(1),
                    Topic = reader.GetString(2),
                    Content = reader.GetString(3),
                    CreationDate = DateTime.Parse(reader.GetString(4))
                });
            }

            return results;
        }
    }

    public class Message
    {
        public long Id { get; set; }
        public string AuthorNickname { get; set; }
        public string Topic { get; set; }
        public string Content { get; set; }
        public DateTime CreationDate { get; set; }
    }

}