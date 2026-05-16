using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace SqliteManager
{
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        private SQLiteConnection connection;
        private SQLiteDataAdapter adapter;
        private DataTable table;

        private string currentDatabasePath = "";
        private string currentTable = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        
        // СОЗДАНИЕ БД
        

        private void CreateDb_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "SQLite Database (*.db)|*.db";

            if (dialog.ShowDialog() == true)
            {
                SQLiteConnection.CreateFile(dialog.FileName);

                currentDatabasePath = dialog.FileName;

                connection =
                    new SQLiteConnection($"Data Source={currentDatabasePath}");

                connection.Open();

                connection.Close();

                MessageBox.Show("База данных создана.");

                LoadTables();
            }
        }

        
        // ОТКРЫТИЕ БД
        

        private void OpenDb_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "SQLite Database (*.db)|*.db";

            if (dialog.ShowDialog() == true)
            {
                currentDatabasePath = dialog.FileName;

                if (connection != null)
                {
                    connection.Close();
                }

                connection =
                    new SQLiteConnection($"Data Source={currentDatabasePath}");

                connection.Open();

                LoadTables();
            }
        }

        
        // ЗАГРУЗКА ТАБЛИЦ
        

        private void LoadTable(string tableName)
        {
            try
            {
                if (connection == null)
                    return;

                string query = $"SELECT * FROM [{tableName}]";

                adapter = new SQLiteDataAdapter(query, connection);

                SQLiteCommandBuilder builder =
                    new SQLiteCommandBuilder(adapter);

                table = new DataTable();

                adapter.Fill(table);

                DataGridMain.ItemsSource = table.DefaultView;
            }
            catch (Exception ex)
            {
                SqlErrorText.Text = ex.Message;
            }
        }


        private void LoadTables()
        {
            try
            {
                TablesCombo.Items.Clear();

                if (connection == null)
                    return;

                string sql =
                    "SELECT name FROM sqlite_master WHERE type='table'";

                SQLiteCommand cmd =
                    new SQLiteCommand(sql, connection);

                SQLiteDataReader reader =
                    cmd.ExecuteReader();

                while (reader.Read())
                {
                    TablesCombo.Items.Add(reader["name"].ToString());
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

   

        
        // ВЫБОР ТАБЛИЦЫ
        


        private void TablesCombo_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (TablesCombo.SelectedItem != null)
            {
                LoadTable(TablesCombo.SelectedItem.ToString());
            }
        }        

        
        // ПЕРЕЗАГРУЗКА ТАБЛИЦЫ
        

        private void ReloadTable()
        {
            try
            {
                using var conn =
                    new SQLiteConnection($"Data Source={currentDatabasePath}");

                conn.Open();

                string sql = $"SELECT * FROM [{currentTable}]";

                adapter = new SQLiteDataAdapter(sql, conn);

                SQLiteCommandBuilder builder =
                    new SQLiteCommandBuilder(adapter);

                table = new DataTable();

                adapter.Fill(table);

                DataGridMain.ItemsSource = table.DefaultView;

                conn.Close();
            }
            catch (Exception ex)
            {
                SqlErrorText.Text = ex.Message;
            }
        }

        
        // ВЫПОЛНЕНИЕ SQL
        

        private void ExecuteSql_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string query = SqlEditor.Text;

                SqlErrorText.Text = "";

                using var conn =
                    new SQLiteConnection($"Data Source={currentDatabasePath}");

                conn.Open();

                using var cmd =
                    new SQLiteCommand(query, conn);

                if (query.Trim().ToUpper().StartsWith("SELECT"))
                {
                    var sqlAdapter =
                        new SQLiteDataAdapter(cmd);

                    DataTable resultTable =
                        new DataTable();

                    sqlAdapter.Fill(resultTable);

                    SqlResultGrid.ItemsSource =
                        resultTable.DefaultView;

                    MainTabs.SelectedIndex = 1;
                }
                else
                {
                    int affected =
                        cmd.ExecuteNonQuery();

                    SqlErrorText.Text =
                        $"Выполнено. Изменено строк: {affected}";

                    ReloadTable();
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                SqlErrorText.Text = ex.Message;
            }
        }

        
        // ОЧИСТКА SQL
        

        private void ClearSql_Click(
            object sender,
            RoutedEventArgs e)
        {
            SqlEditor.Text = "";
        }

        
        // ПОИСК
        

        private void Search_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (table == null)
                    return;

                string filter = SearchBox.Text;

                if (string.IsNullOrWhiteSpace(filter))
                    return;

                string rowFilter = "";

                foreach (DataColumn column in table.Columns)
                {
                    if (rowFilter != "")
                        rowFilter += " OR ";

                    rowFilter +=
                        $"Convert([{column.ColumnName}], 'System.String') LIKE '%{filter}%'";
                }

                table.DefaultView.RowFilter = rowFilter;
            }
            catch (Exception ex)
            {
                SqlErrorText.Text = ex.Message;
            }
        }

        
        // СБРОС ФИЛЬТРА
        

        private void ClearFilter_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (table != null)
            {
                table.DefaultView.RowFilter = "";
            }

            SearchBox.Text = "";
        }

        
        // ДОБАВЛЕНИЕ СТРОКИ
        

        private void AddRow_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (table == null)
                return;

            table.Rows.Add(table.NewRow());
        }

        
        // УДАЛЕНИЕ СТРОКИ
        

        private void DeleteRow_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DataGridMain.SelectedItem is DataRowView row)
            {
                row.Delete();
            }
        }

        
        // СОХРАНЕНИЕ
        

        private void SaveChanges_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (adapter != null && table != null)
                {
                    SQLiteCommandBuilder builder =
                        new SQLiteCommandBuilder(adapter);

                    adapter.Update(table);

                    MessageBox.Show("Изменения сохранены.");
                }
            }
            catch (Exception ex)
            {
                SqlErrorText.Text = ex.Message;
            }
        }
        
        // СОЗДАНИЕ ТАБЛИЦЫ
        

        private void CreateTable_Click(
            object sender,
            RoutedEventArgs e)
        {
            string tableName =
                Microsoft.VisualBasic.Interaction.InputBox(
                    "Введите имя таблицы:",
                    "Создание таблицы");

            if (string.IsNullOrWhiteSpace(tableName))
                return;

            try
            {
                using var conn =
                    new SQLiteConnection($"Data Source={currentDatabasePath}");

                conn.Open();

                string sql =
                    $"CREATE TABLE [{tableName}] (Id INTEGER PRIMARY KEY AUTOINCREMENT)";

                using var cmd =
                    new SQLiteCommand(sql, conn);

                cmd.ExecuteNonQuery();

                conn.Close();

                LoadTables();
            }
            catch (Exception ex)
            {
                SqlErrorText.Text = ex.Message;
            }
        }

        
        // ДОБАВЛЕНИЕ КОЛОНКИ
        

        private void AddColumn_Click(
            object sender,
            RoutedEventArgs e)
        {
            string columnName =
                Microsoft.VisualBasic.Interaction.InputBox(
                    "Введите имя колонки:",
                    "Добавление колонки");

            if (string.IsNullOrWhiteSpace(columnName))
                return;

            try
            {
                using var conn =
                    new SQLiteConnection($"Data Source={currentDatabasePath}");

                conn.Open();

                string sql =
                    $"ALTER TABLE [{currentTable}] ADD COLUMN [{columnName}] TEXT";

                using var cmd =
                    new SQLiteCommand(sql, conn);

                cmd.ExecuteNonQuery();

                conn.Close();

                ReloadTable();
            }
            catch (Exception ex)
            {
                SqlErrorText.Text = ex.Message;
            }
        }

        
        // ПЕРЕИМЕНОВАНИЕ КОЛОНКИ
        

        private void RenameColumn_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "SQLite ограниченно поддерживает переименование колонок.");
        }

        
        // УДАЛЕНИЕ КОЛОНКИ
        

        private void DeleteColumn_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "SQLite ограниченно поддерживает удаление колонок.");
        }

        
        // ПКМ ПО ТАБЛИЦЕ
        

        private void DataGridMain_MouseRightButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid)
            {
                ContextMenu menu =
                    new ContextMenu();

                MenuItem copyCell =
                    new MenuItem();

                copyCell.Header =
                    "Скопировать ячейку";

                copyCell.Click += (s, ev) =>
                {
                    if (grid.CurrentCell.Item is DataRowView row)
                    {
                        string column =
                            grid.CurrentCell.Column.Header.ToString();

                        Clipboard.SetText(
                            row[column]?.ToString() ?? "");
                    }
                };

                MenuItem copyRow =
                    new MenuItem();

                copyRow.Header =
                    "Скопировать строку";

                copyRow.Click += (s, ev) =>
                {
                    if (grid.SelectedItem is DataRowView row)
                    {
                        string text =
                            string.Join("\t",
                                row.Row.ItemArray);

                        Clipboard.SetText(text);
                    }
                };

                MenuItem deleteRow =
                    new MenuItem();

                deleteRow.Header =
                    "Удалить строку";

                deleteRow.Click += (s, ev) =>
                {
                    if (grid.SelectedItem is DataRowView row)
                    {
                        row.Delete();
                    }
                };

                menu.Items.Add(copyCell);
                menu.Items.Add(copyRow);
                menu.Items.Add(new Separator());
                menu.Items.Add(deleteRow);

                grid.ContextMenu = menu;
            }
        }

        
        // СВЯЗИ
        

        private void ShowRelations_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "Редактор связей будет добавлен позже.");
        }

        
        // ОБНОВЛЕНИЕ
        

        private void ReloadTable_Click(
            object sender,
            RoutedEventArgs e)
        {
            ReloadTable();
        }
    }
}