using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devart.Data.SQLite;
using System.Text.RegularExpressions;


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
                Console.WriteLine($"Can't open database file: {file}");
                Environment.Exit(1);
            }
        }

        public void showBooks(String title)
        {
            String sql =
                $@"select b.id, b.title, GROUP_CONCAT(a.name, ', ') authors from books b
                left join lnk_books_authors l on b.id = l.book_id
                left join authors a on l.author_id = a.id
                where b.title like :title
                group by b.id, b.title";

            SQLiteCommand command = dbConnect.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add("title", $"{title.Trim()}%");

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"{reader.GetString(0)}\t{reader.GetString(1)} - {reader.GetString(2)}");
                }
            }
        }

        public int deleteBook(String id)
        {
            return modifyRequest("DELETE FROM books WHERE id=:id",
                new Dictionary<string, string> { {"id", id } });
        }

        public int updateBook(String id, String title)
        {
            title = title.Trim();
            if (title == "") return 0;

            return modifyRequest("UPDATE books SET title=:title WHERE id=:id",
                new Dictionary<string, string> { { "title", title }, {"id", id } });
        }

        public String getTitle(String id)
        {
            return selectOne("SELECT title FROM books WHERE id=:id",
                new Dictionary<string, string> { {"id", id} });
        }

        private String selectOne(String sql, Dictionary<String, String> param)
        {
            SQLiteCommand command = dbConnect.CreateCommand();
            command.CommandText = sql;
            if (param != null)
            {
                foreach (var pair in param)
                {
                    command.Parameters.Add(pair.Key, pair.Value);
                }
            }

            try
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    return reader.GetString(0);
                }
            }
            catch
            {
                return "Error";
            }
        }


        private int modifyRequest(String sql, Dictionary<String, String> param)
        {
            SQLiteCommand command = dbConnect.CreateCommand();
            command.CommandText = sql;
            if (param != null)
            {
                foreach (var pair in param)
                {
                    command.Parameters.Add(pair.Key, pair.Value);
                }
            }
            try
            {
                return command.ExecuteNonQuery();
            }
            catch
            {
                return 0;
            }
            
        }

        public int insertBook(String title, String authosr)
        {
            title = title.Trim();
            if (title == "") return 0;

            String maxId = selectOne("SELECT MAX(id)+1 FROM books", null);

            int booksCount = modifyRequest("INSERT INTO books(id, title) VALUES (:id, :title)",
                new Dictionary<string, string> { { "id", maxId }, { "title", title } });

            if (booksCount != 1) return booksCount;

            foreach (String au in Regex.Split(authosr, @"\s*,\s*"))
            {
                String person = au.Trim();
                if (person == "") continue;

            }

            return booksCount;
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

            Console.WriteLine("Devart Technical task (C) Gmyrak Dmitry\n");

            
            BooksStorage bs = new BooksStorage(@"d:\Projects\books\database.db");

            Boolean actionLoop = true;

            while (actionLoop)
            {
                Console.WriteLine("1 - SELECT, 2 - INSERT, 3 - UPDATE, 4 - DELETE, 5 - EXIT");
                ConsoleKeyInfo key = Console.ReadKey(true);
                
                switch (key.KeyChar)
                {
                    case '1':
                        String filter = prompt("SELECT:\nEnter the first letters of book's title, or nothing (all books)");
                        bs.showBooks(filter);
                        break;

                    case '2':                     
                        String title = prompt("INSERT:\nEnter book title");
                        String authors = prompt("Coma separated list of authors");
                        Console.WriteLine($"{bs.insertBook(title, authors)} was added");
                        break;

                    case '3':                        
                        String upd_id = prompt("UPDATE:\nEnter book number for update");
                        Console.WriteLine($"> {bs.getTitle(upd_id)}");
                        String upd_name = prompt("Enter new book's title");
                        Console.WriteLine($"{bs.updateBook(upd_id, upd_name)} was updated");
                        break;

                    case '4':
                        String del_id = prompt("DELETE:\nEnter book's number to delete");
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
