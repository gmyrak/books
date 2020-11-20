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

        private int modify(String sql)
        {
            SQLiteCommand command = dbConnect.CreateCommand();
            command.CommandText = sql;
            return command.ExecuteNonQuery();
        }

        public void showBooks(String title)
        {
            String sql =
                $@"select b.id, b.title, GROUP_CONCAT(a.name, ', ') authors from books b
                left join lnk_books_authors l on b.id = l.book_id
                left join authors a on l.author_id = a.id
                where b.title like '{title}%'
                group by b.id, b.title";

            SQLiteCommand command = dbConnect.CreateCommand();
            command.CommandText = sql;

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"{reader.GetString(0)}\t{reader.GetString(1)} // {reader.GetString(2)}");
                }
            }
        }

        public int deleteBook(String id)
        {
            return modify($"DELETE FROM books WHERE id={id}");
        }
    }

    class Program
    {

        static String prompt(String msg)
        {
            Console.WriteLine(msg);
            Console.Write("> ");
            return Console.ReadLine();
        }

        static void Main(string[] args)
        {

            BooksStorage bs = new BooksStorage(@"d:\Projects\books\database.db");

            Boolean actionLoop = true;

            while (actionLoop)
            {
                Console.WriteLine("1 - SELECT, 2 - INSERT, 3 - UPDATE, 4 - DELETE, 5 - EXIT");
                ConsoleKeyInfo key = Console.ReadKey(true);
                
                switch (key.KeyChar)
                {
                    case '1':
                        Console.WriteLine("SELECT:");
                        String filter = prompt("Enter the first letters of book's title, or nothing (all books)");
                        bs.showBooks(filter);
                        break;

                    case '2':
                        Console.WriteLine("INSERT:");
                        String title = prompt("Enter book title");
                        String authors = prompt("Coma separated list of authors");
                        break;

                    case '3':
                        Console.WriteLine("UPDATE:");
                        String upd_id = prompt("Enter book number for update");
                        String upd_name = prompt("Enter new book's title");
                        break;

                    case '4':
                        Console.WriteLine("DELETE:");
                        String del_id = prompt("Enter book's number to delete");
                        Console.WriteLine($"{bs.deleteBook(del_id)} was deleted");
                        break;

                    case '5':
                        actionLoop = false;
                        break;                             

                    default:
                        Console.WriteLine("Incorrect choice");
                        break;

                }
                Console.WriteLine();

            }

                        
            Console.WriteLine("Bye!");
        }
    }
}
