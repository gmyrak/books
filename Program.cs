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

        private void select(String sql)
        {
            SQLiteCommand command = dbConnect.CreateCommand();
            command.CommandText = sql;

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                int fieldCount = reader.FieldCount;

                while (reader.Read())
                {
                    for (int i = 0; i < fieldCount; i++)
                        Console.Write(reader.GetString(i) + "\t");
                    
                    Console.WriteLine();
                }
            }
        }

        private void modify(String sql)
        {
            SQLiteCommand command = dbConnect.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public void showBooks(String title)
        {
            select($"SELECT * FROM books WHERE title LIKE '{title}%'");
        }


    }

    class Program
    {
        static void Main(string[] args)
        {

            BooksStorage bs = new BooksStorage(@"c:\Projects\books\database.db");

            bs.showBooks("Т");
            //bs.select($"SELECT * FROM books WHERE title LIKE '%'");

            Console.WriteLine("Hello!");
        }
    }
}
