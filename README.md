# Описание приложения

## Интерфейс пользователя

Приложение предоставляет простой консольный интерфейс для работы с библиотекой. 
В меню верхнего уровня присутствуют 5 операций. Нажатие соответствующей цифры сразу же активирует функцию (без нажатия ENTER).

`1 - SELECT, 2 - INSERT, 3 - UPDATE, 4 - DELETE, 5 - EXIT`

### 1 - SELECT
Дополнительно предлагается ввести первые буквы названия книги. Выводимые результаты будут соответственно отфильтрованы. Если ввести пустую строку (просто ENTER) - показываются все книги. В результатах присуствуют колонки `id` - идентификатор книги, `title` - название книги. Идентификатор потребуется для использования в других операциях (UPDATE, DELETE).

### 2 - INSERT
Добавить новую книгу в библиотеку. Сначала предлагается вверсти название книги, потом автора. Можно ввести несколько авторов, разделяя их имена запятыми. Каждый из этих авторов будет связан с книгой. Если имя автора уже есть в таблице - используется существующий идентификатор, в противном случае добавляется новый автор.

### 3 - UPDATE
Редактирование названия книги. Требуется ввести идентификатор книги для редактирования, потом новое название.

### 4 - DELETE
Удаление книги. Требуется ввести идентификатор книги. Вместе с книгой каскадно удаляются и все записи из связующей таблицы.

### 5 - EXIT
Выход из приложения.

## База данных
В файловой базе данных SQLite хранятся данные о книгах и авторах.

Таблицы:
* books
* authors
* lnk_books_authors

```sql
CREATE TABLE [books](
  [id] INTEGER PRIMARY KEY AUTOINCREMENT, 
  [title] VARCHAR NOT NULL);
  
CREATE TABLE [authors](
  [id] INTEGER PRIMARY KEY AUTOINCREMENT, 
  [name] VARCHAR);
  
CREATE TABLE [lnk_books_authors](
  [book_id] INTEGER REFERENCES [books]([id]) ON DELETE CASCADE, 
  [author_id] INTEGER REFERENCES [authors]([id]) ON DELETE CASCADE, 
  PRIMARY KEY([book_id], [author_id]));  
```  

Таблица `lnk_books_authors` служит для связи "многие ко многим" между книгами и авторами.


## Файл базы данных

