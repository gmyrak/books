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
                ModifyRequest("PRAGMA foreign_keys = ON", null);
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

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read()) return reader.GetString(0);
                else return "";
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
                $@"select b.id, b.title, GROUP_CONCAT(a.name, ', ') au from books b
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
                new Dictionary<string, string> { { "id", id } });
        }

        public int UpdateBook(String id, String title)
        {
            title = title.Trim();
            if (title == "") return 0;

            return ModifyRequest("UPDATE books SET title=:title WHERE id=:id",
                new Dictionary<string, string> { { "title", title }, { "id", id } });
        }

        public String GetTitle(String book_id)
        {
            return SelectOne("SELECT title FROM books WHERE id=:id",
                new Dictionary<string, string> { { "id", book_id } });
        }

        public String GetAuthors(String book_id)
        {
            String sql =
               $@"select GROUP_CONCAT(a.name, ', ') au from books b
                left join lnk_books_authors l on b.id = l.book_id
                left join authors a on l.author_id = a.id
                where b.id = :id";
                
            return SelectOne(sql, new Dictionary<string, string> { { "id", book_id } });
        }

        private String GetAuthorId(String name)
        {
            return SelectOne("SELECT id FROM authors WHERE name=:name",
                    new Dictionary<string, string> { { "name", name } });
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

        private int CreateLink(String book_id, String author_id)
        {
            return ModifyRequest("INSERT INTO lnk_books_authors VALUES (:book_id, :author_id)",
                new Dictionary<string, string> { { "book_id", book_id }, { "author_id", author_id } });
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
                String person = CleanName(au);
                if (person == "") continue;
                CreateLink(maxId, AuthorId(person));
            }

            return booksCount;
        }

        public void UpdateAuthors(String book_id, String authorsList)
        {

            int add = 0;
            int del = 0; 

            foreach (String au in Regex.Split(authorsList, @"\s*,\s*"))
            {
                String person = CleanName(au);
                if (person == "") continue;

                if (Regex.IsMatch(au, @"^-")) // author for delete
                {
                    del += ModifyRequest("DELETE FROM lnk_books_authors WHERE book_id = :book_id AND author_id = :author_id",
                        new Dictionary<string, string> { { "book_id", book_id }, { "author_id", GetAuthorId(person) } });
                }
                else
                {
                    add += CreateLink(book_id, AuthorId(person));
                }
            }
            Console.WriteLine($"Authors add: {add}; delete: {del}");
        }

        public static String CleanName(String name)
        {
            // removes [+-;=,.] at the beginning and duplicate spaces between words
            return String.Join(" ", Regex.Split(Regex.Replace(name.Trim(), @"^[+-;=,.]+", "").Trim(), @"\s+"));            
        }
    }

    class UserInterface
    {
        private BooksStorage db;
        public UserInterface(BooksStorage database)
        {
            this.db = database;
        }

        public void MainMenu()
        {
            Boolean actionLoop = true;

            while (actionLoop)
            {
                Console.WriteLine("1 - SELECT, 2 - INSERT, 3 - UPDATE, 4 - DELETE, 5 - EXIT");

                switch (Console.ReadKey(true).KeyChar)
                {
                    case '1':
                        ShowBooks();
                        break;
                    case '2':
                        AddBook();
                        break;
                    case '3':
                        UpdateBook();
                        break;
                    case '4':
                        DeleteBook();
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
        }

        public void ShowBooks()
        {
            String filter = Prompt("SELECT:\nEnter the first letters of book's title, or nothing (all books)");
            db.ShowBooks(filter);
        }

        public void AddBook()
        {
            String title = Prompt("INSERT:\nEnter book title");
            if (title == "") return;
            String authors = Prompt("Coma separated list of authors");
            Console.WriteLine($"{db.InsertBook(title, authors)} was added");
        }
        public void UpdateBook()
        {
            String updId = Prompt("UPDATE:\nEnter book number for update");
            String title = db.GetTitle(updId);
            if (title == "")
            {
                Console.WriteLine("Book not found");
                return;
            }
            Console.WriteLine($"Title: {title}");
            String updName = Prompt("Enter new book's title");
            Console.WriteLine($"{db.UpdateBook(updId, updName)} Book name was updated");

            Console.WriteLine($"Authors: {db.GetAuthors(updId)}");
            String updAuthors = Prompt("Coma separater lisn for add (<name>) or delete ( -<name> )");
            db.UpdateAuthors(updId, updAuthors);
        }

        public void DeleteBook()
        {
            String del_id = Prompt("DELETE:\nEnter book's number to delete");
            Console.WriteLine($"{db.DeleteBook(del_id)} was deleted");
        }

        public static String Prompt(String msg)
        {
            Console.WriteLine(msg);
            Console.Write("> ");
            return Console.ReadLine();
        }

    }

    class Program
    {

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

            Console.WriteLine("Library management system (C) Gmyrak Dmitry\n");

            String dbName = "books.db";
            String fullDbName = GetFullDbName(dbName);
            if (fullDbName == null)
            {
                Console.WriteLine($"Database file {dbName} not found");
                Environment.Exit(1);
            }
            Console.WriteLine($"Using database file: {fullDbName}\n");

            BooksStorage bs = new BooksStorage(fullDbName);
            UserInterface ui = new UserInterface(bs);
            ui.MainMenu();

            Console.WriteLine("Bye!");
        }
    }
}
