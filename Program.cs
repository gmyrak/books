using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devart.Data.SQLite;
using System.Text.RegularExpressions;
using System.IO;


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


        private String SelectOne(String sql, Dictionary<String, String> param)
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


        private int ModifyRequest(String sql, Dictionary<String, String> param)
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

        public void ShowBooks(String title)
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

        public int DeleteBook(String id)
        {
            return ModifyRequest("DELETE FROM books WHERE id=:id",
                new Dictionary<string, string> { {"id", id } });
        }

        public int UpdateBook(String id, String title)
        {
            title = title.Trim();
            if (title == "") return 0;

            return ModifyRequest("UPDATE books SET title=:title WHERE id=:id",
                new Dictionary<string, string> { { "title", title }, {"id", id } });
        }

        public String GetTitle(String id)
        {
            return SelectOne("SELECT title FROM books WHERE id=:id",
                new Dictionary<string, string> { {"id", id} });
        }


        private String AuthorId(String name) // Returns id for existing author or create new author
        {
            String personId = SelectOne("SELECT id FROM authors WHERE name=:name",
                    new Dictionary<string, string> { { "name", name } });
            if (Regex.IsMatch(personId, @"^\d+$")) return personId; // exists

            String maxId = SelectOne("SELECT MAX(id)+1 FROM authors", null);
            ModifyRequest("INSERT INTO authors(id, name) VALUES (:id, :name)",
                new Dictionary<string, string> { { "id", maxId }, { "name", name } });
            return maxId; // new author
        }

        public int InsertBook(String title, String authosr)
        {
            title = title.Trim();
            if (title == "") return 0;

            String maxId = SelectOne("SELECT MAX(id)+1 FROM books", null);

            int booksCount = ModifyRequest("INSERT INTO books(id, title) VALUES (:id, :title)",
                new Dictionary<string, string> { { "id", maxId }, { "title", title } });

            if (booksCount != 1) return booksCount;

            foreach (String au in Regex.Split(authosr, @"\s*,\s*"))
            {
                String person = au.Trim();
                if (person == "") continue;

                ModifyRequest("INSERT INTO lnk_books_authors VALUES (:book_id, :author_id)",
                    new Dictionary<string, string> { { "book_id", maxId }, { "author_id", AuthorId(person) } });
            }

            return booksCount;
        }
    }

    class Program
    {

        static String Prompt(String msg)
        {
            Console.WriteLine(msg);
            Console.Write("> ");
            return Console.ReadLine();
        }

        static String GetFullDbName(String dbName)
        {
            String dir = Directory.GetCurrentDirectory();

            while(true)
            {
                String fullName = $@"{dir}\{dbName}";
                if (File.Exists(fullName)) return fullName;

                DirectoryInfo parent = Directory.GetParent(dir);
                if (parent == null) return null;
                dir = parent.FullName;
            }

        }

        static void Main(string[] args)
        {

            Console.WriteLine("Technical task. (C) Gmyrak Dmitry\n");

            String dbName = "books.db";
            String fullDbName = GetFullDbName(dbName);
            if (fullDbName == null)
            {
                Console.WriteLine($"Database file {dbName} not found");
                Environment.Exit(1);
            }
            Console.WriteLine($"Using database file: {fullDbName}\n");

            BooksStorage bs = new BooksStorage(fullDbName);

            Boolean actionLoop = true;

            while (actionLoop)
            {
                Console.WriteLine("1 - SELECT, 2 - INSERT, 3 - UPDATE, 4 - DELETE, 5 - EXIT");
                ConsoleKeyInfo key = Console.ReadKey(true);
                
                switch (key.KeyChar)
                {
                    case '1':
                        String filter = Prompt("SELECT:\nEnter the first letters of book's title, or nothing (all books)");
                        bs.ShowBooks(filter);
                        break;

                    case '2':                     
                        String title = Prompt("INSERT:\nEnter book title");
                        String authors = Prompt("Coma separated list of authors");
                        Console.WriteLine($"{bs.InsertBook(title, authors)} was added");
                        break;

                    case '3':                        
                        String upd_id = Prompt("UPDATE:\nEnter book number for update");
                        Console.WriteLine($"> {bs.GetTitle(upd_id)}");
                        String upd_name = Prompt("Enter new book's title");
                        Console.WriteLine($"{bs.UpdateBook(upd_id, upd_name)} was updated");
                        break;

                    case '4':
                        String del_id = Prompt("DELETE:\nEnter book's number to delete");
                        Console.WriteLine($"{bs.DeleteBook(del_id)} was deleted");
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
