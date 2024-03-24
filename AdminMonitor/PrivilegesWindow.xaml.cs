using FsCheck;
using FsCheck.Experimental;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AdminMonitor
{
    /// <summary>
    /// Interaction logic for PrivilegesWindow.xaml
    /// </summary>
    public partial class PrivilegesWindow : Window
    {
        OracleConnection _con;
        string _username;
        DataTable table;
        BindingList<string> _operationsList = new BindingList<string>()
        {
            "SELECT","DELETE","INSERT","UPDATE"
        };
        BindingList<Table> _tablesList = new BindingList<Table>();
        int _rowsPerPage = 15;
        int _currentPage = 1;
        int totalPages = -1;
        int totalItems = -1;
        List<int> rowPerPageOptions = new List<int>() {
            4,16,32,64,128,256,512,1024
        };

        public PrivilegesWindow(OracleConnection con, string username, string displayMode)
        {
            InitializeComponent();
            _con = con;
            _username = username;
            this.Title = $"{username}'s privileges";
            ObjectNameTextBlock.Text = _username;
            if(displayMode == "Roles")
            {
                GrantOptionCheckBox.IsEnabled = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OperationsComboBox.ItemsSource = _operationsList;
            OperationsComboBox.SelectedIndex = 0;
            rowPerPageOptionsComboBox.ItemsSource = rowPerPageOptions;

            rowPerPageOptionsComboBox.SelectedIndex = 1;
            TablePrivsRadioButton.IsChecked = true;
            
            LoadTableNames();
            //LoadColumnPrivileges();
        }

        private void LoadTableNames()
        {
            OracleCommand query = _con.CreateCommand();
            query.CommandText = """
                SELECT
                  table_name, owner
                FROM
                  all_tables
                ORDER BY
                  table_name, owner
                """;

            query.CommandType = CommandType.Text;

            OracleDataReader dr = query.ExecuteReader();
            try
            {
                while (dr.Read())
                {
                    string tableName = (string)dr["table_name"];
                    string tableOwner = (string)dr["owner"];
                    _tablesList.Add(new Table()
                    {
                        table_name = tableName,
                        table_owner = tableOwner
                    });
                }
                TableNameComboBox.ItemsSource = _tablesList;
            }
            finally { 
                dr.Close();
                TableNameComboBox.SelectedIndex = 0;
            }
        }

        private void LoadPrivileges(int page, int rowsPerPage)
        {
            int skip = (page - 1) * rowsPerPage;
            int take = rowsPerPage;

            OracleCommand query = _con.CreateCommand();
            if (TablePrivsRadioButton.IsChecked == true)
            {
                query.CommandText = """
                select GRANTEE,OWNER,TABLE_NAME,GRANTOR,PRIVILEGE,GRANTABLE,HIERARCHY,COMMON,TYPE,INHERITED, count(*) over() as "TotalItems"
                from dba_tab_privs  
                where grantee = :username1 
                        or grantee in (select granted_role 
                                        from dba_role_privs 
                                        connect by prior granted_role = grantee 
                                        start with grantee = :username2) 
                order by 1,2,3,4
                offset :Skip rows 
                fetch next :Take rows only
                """;
            }else if(ColumnPrivsRadioButton.IsChecked == true)
            {
                query.CommandText = """
                Select grantee, owner, table_name, column_name, grantor, privilege,grantable, count(*) over() as "TotalItems"
                from DBA_COL_PRIVS
                where grantee =  :username1
                        or grantee in (select granted_role 
                                        from dba_role_privs 
                                        connect by prior granted_role = grantee 
                                        start with grantee = :username2)
                order by 1,2,3,4
                offset :Skip rows 
                fetch next :Take rows only
                """;
            }
            
            query.CommandType = CommandType.Text;
            query.Parameters.Add(new OracleParameter("username1", _username));
            query.Parameters.Add(new OracleParameter("username2", _username));
            query.Parameters.Add(new OracleParameter("Skip", skip));
            query.Parameters.Add(new OracleParameter("Take", take));
            OracleDataReader dr = query.ExecuteReader();
            try
            {
                //OracleDataAdapter oracleDataAdapter = new(getEmps);
                //while( empDR.Read())
                //{
                //    string ID = (string) empDR["MANV"];

                //}
                table = new DataTable();
                table.Load(dr);
                if (totalItems == -1 && table.Rows.Count > 0)
                {
                    totalItems = int.Parse(table.Rows[0]["TotalItems"].ToString());
                    totalPages = (totalItems / rowsPerPage);
                    if (totalItems % rowsPerPage == 0) totalPages = (totalItems / rowsPerPage);
                    else totalPages = (int)(totalItems / rowsPerPage) + 1;
                }
                table.Columns.Remove("TotalItems");
                dataGridView.ItemsSource = table.DefaultView;
            }
            finally { dr.Close(); }

            PageCountTextBox.Text = $" {_currentPage}/{totalPages} ";
            TotalItemDisplayTextBox.Text = $" of {totalItems} item(s).";          

            if (_currentPage == totalPages)
            {
                NextButton.IsEnabled = false;
            }
            else
            {
                NextButton.IsEnabled = true;
            }

            if(_currentPage == 1)
            {
                PrevButton.IsEnabled = false;
            }
            else
            {
                PrevButton.IsEnabled = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _con.Close();
        }

        private void GrantButton_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand query = _con.CreateCommand();
            query.CommandText = """
                grant_privilege
                """;
            query.CommandType = CommandType.StoredProcedure;

            string operation = (string)OperationsComboBox.SelectedItem;
            Table table = (Table)TableNameComboBox.SelectedItem;
            List<Column> columns = (List<Column>)ColumnNamesComboBox.ItemsSource;
            string columnList = "";
            foreach(Column column in columns)
            {
                if(column.isChecked)
                {
                    columnList += $"{column.ColumnName},";
                }
            }
            if(columnList.Length > 0)
            {
                columnList = columnList.Remove(columnList.Length - 1);
            }

            string grantOption = "";
            if (GrantOptionCheckBox.IsChecked == true)
            {
                grantOption = " WITH GRANT OPTION";
            }

            query.Parameters.Add(new OracleParameter("Operation", operation));
            query.Parameters.Add(new OracleParameter("Owner", table.table_owner));
            query.Parameters.Add(new OracleParameter("Table", table.table_name));
            query.Parameters.Add(new OracleParameter("Schema", _username));
            query.Parameters.Add(new OracleParameter("columnList", columnList));
            query.Parameters.Add(new OracleParameter("grantOption", grantOption));
            try
            {
                query.ExecuteNonQuery();
            }catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            var resultScreen = new ResultWindow(_con,operation,table.table_owner,table.table_name,_username,columnList);
            resultScreen.ShowDialog();

            LoadPrivileges(_currentPage,_rowsPerPage);
        }

        private void TableNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OracleCommand query = _con.CreateCommand();
            query.CommandText = """
                select * from all_tab_columns where TABLE_NAME = :tableName
                """;
            query.CommandType = CommandType.Text;

            Table table = (Table)TableNameComboBox.SelectedItem;
            query.CommandType = CommandType.Text;
            query.Parameters.Add(new OracleParameter("tableName", table.table_name));
            //query.Parameters.Add(new OracleParameter("tableOwner", table.table_owner));

            OracleDataReader dr = query.ExecuteReader();
            List<Column> columns = new List<Column>();
            try
            {
                while (dr.Read())
                {
                    string columnName = (string)dr["COLUMN_NAME"];
                    columns.Add(new Column()
                    {
                        ColumnName = columnName,
                        isChecked = false
                    });
                }
                ColumnNamesComboBox.ItemsSource = columns;
                dr.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(),"Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { dr.Close(); }
        }

        private void OperationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((string)OperationsComboBox.SelectedItem == "INSERT" || (string)OperationsComboBox.SelectedItem == "DELETE")
            {
                ColumnNamesComboBox.IsEnabled = false;
            }
            else
            {
                ColumnNamesComboBox.IsEnabled = true;
            }
        }

        private void RevokePriv_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)dataGridView.SelectedItems[0];
            string grantee = (string)row.Row.ItemArray[0];
            OracleCommand query = _con.CreateCommand();
            query.CommandType = CommandType.StoredProcedure;
            if (grantee == _username)
            {
                query.CommandText = """
                revoke_privilege
                """;
                string operation = (string)row.Row.ItemArray[4];
                string tableName = (string)row.Row.ItemArray[2];
                string tableOwner = (string)row.Row.ItemArray[1];

                query.Parameters.Add(new OracleParameter("username", _username));
                query.Parameters.Add(new OracleParameter("operation", operation));
                query.Parameters.Add(new OracleParameter("tableOwner", tableOwner));
                query.Parameters.Add(new OracleParameter("tableName", tableName));
            }
            else
            {
                query.CommandText = """
                revoke_role
                """;

                query.Parameters.Add(new OracleParameter("username", _username));
                query.Parameters.Add(new OracleParameter("rolename", grantee));
            }

            try
            {
                query.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Revoked successfully!", "Success", MessageBoxButton.OK);
            LoadPrivileges(_currentPage, _rowsPerPage);
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadPrivileges(_currentPage, _rowsPerPage);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < totalPages)
            {
                _currentPage++;
                LoadPrivileges(_currentPage, _rowsPerPage);
            }
        }

        private void TablePrivsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            totalItems = -1;
            LoadPrivileges(_currentPage, _rowsPerPage);
        }

        private void ColumnPrivsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            totalItems = -1;
            LoadPrivileges(_currentPage, _rowsPerPage);
        }

        private void rowPerPageOptionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _rowsPerPage = (int)rowPerPageOptionsComboBox.SelectedItem;
            _currentPage = 1;
            if(totalItems != -1)
            {
                LoadPrivileges(_currentPage, _rowsPerPage);
            }
        }
    }

    internal class Table
    {
        public string table_name { get; set; }
        public string table_owner { get; set; }
    }
    internal class Column
    {
        public string ColumnName { get; set; }
        public bool isChecked { get; set; }
    }
}
