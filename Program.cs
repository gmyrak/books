using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devart.Data.SQLite;


namespace books
{
    class BooksStorage
    {
        private SQLiteConnection dbConnect;
        public BooksStorage(String file)
        {
            dbConnect = new SQLiteConnection();
            dbConnect.ConnectionString = $"Data Source={file};";
            try
            {
                dbConnect.Open();
            } catch
            {
                Console.WriteLine($"Database File: {file} not found");
                Environment.Exit(1);
            }
        }

        public void select(String sql)
        {
            SQLiteCommand command = dbConnect.CreateCommand();
            command.CommandText = sql;

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                int fieldCount = reader.FieldCount;

                //for (int i = 0; i < fieldCount; i++)
                //    Console.Write(reader.GetString(i) + "\t");
                //Console.WriteLine();

                while (reader.Read())
                {
                    for (int i = 0; i < fieldCount; i++)
                        Console.Write(reader.GetString(i) + "\t");
                    
                    Console.WriteLine();
                }
            }
        }

        public void modify(String sql)
        {
            SQLiteCommand command = dbConnect.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            BooksStorage bs = new BooksStorage(@"c:\Projects\books\database.db");

            bs.select("SELECT * FROM books");

            bs.modify("INSERT INTO authors(name) VALUES ('Агата Кристи')");

            Console.WriteLine("Hello!");
        }
    }
}
