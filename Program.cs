﻿using System;
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

        private int modify(String sql)
        {
            SQLiteCommand command = dbConnect.CreateCommand();
            command.CommandText = sql;
            return command.ExecuteNonQuery();
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

            String prompt = "1 - SELECT, 2 - INSERT, 3 - UPDATE, 4 - DELETE";


            while (true)
            {
                Console.WriteLine(prompt);
                ConsoleKeyInfo key = Console.ReadKey(false);
                
                switch (key.KeyChar)
                {
                    case '1': 
                        Console.WriteLine("first");
                        break;

                    case '2':
                        Console.WriteLine("second");
                        break;

                    default:
                        Console.WriteLine("Other");
                        break;

                }

            }



            //bs.showBooks("Т");
            //bs.select($"SELECT * FROM books WHERE title LIKE '%'");
                        
            Console.WriteLine("Bye!");
        }
    }
}
