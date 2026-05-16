# SQLite Manager

## Overview

SQLite Manager is a WPF desktop application written in C# (.NET) that allows users to create, open, edit, and manage SQLite databases through a graphical interface.

The project is intended for educational purposes and for working with lightweight local databases without needing external database tools.

---

## Features

- Create new SQLite database files (.db)
- Open existing SQLite databases
- Display table data in a grid view
- Add and delete rows
- Edit data directly in the UI
- Save changes to the database file

---

## Technology Stack

- C# (.NET 8 / .NET 9)
- WPF (Windows Presentation Foundation)
- SQLite (System.Data.SQLite)
- ADO.NET

---

## How It Works

The application uses ADO.NET with SQLite to interact with database files. Data is loaded into a DataTable using SQLiteDataAdapter and displayed in a WPF DataGrid. Changes made in the UI are synchronized back to the database using SQLiteCommandBuilder.

---

## Database Schema

By default, the application works with a single table:

```sql
CREATE TABLE data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT,
    value TEXT
);

```
## Running the Project

### Requirements

- .NET SDK 8 or 9
- Windows OS (WPF is Windows-only)

---

### Run in Development Mode

```bash
dotnet run
```

### Notes
- The application was created during the learning process of C#
- It is not intended to replace full database management systems.
- Works only on Windows due to WPF dependency.
- For deployment, it is recommended to use the published single-file executable.
- You can find ready to use .exe file in realises
