using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;


namespace AdminMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //readonly string conStr = """
        //                            DATA SOURCE=localhost:15211/xe;
        //                            DBA PRIVILEGE=SYSDBA;
        //                            PERSIST SECURITY INFO=True;
        //                            USER ID=SYS;
        //                            PASSWORD=123
        //                            """;
        public OracleConnection con;
        DataTable empDT;
        DataTable roleDT;
        string DisplayMode = "";
        List<string> createOptions = new List<string>() { 
            "USER","ROLE"
        };

        List<int> rowPerPageOptions = new List<int>() { 
            4,16,32,64,128,256,512,1024
        };

        int _rowsPerPage = 15;
        int _currentPage = 1;
        int totalPages = -1;
        int totalItems = -1;

        public MainWindow()
        {
            InitializeComponent();
            //con = new OracleConnection(conStr);
            //con.Open();
            //if (con.State != ConnectionState.Open)
            //{
            //    MessageBox.Show("Connect Failed!", "Failed", MessageBoxButton.OK);
            //    this.Close();
            //}
        }

        private void getUsersButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            totalItems = -1;
            GetUser(_currentPage, _rowsPerPage);
            GrantRoleContextItem.IsEnabled = true;
            ChangePasswordContextItem.IsEnabled = true;
        }

        private void GetUser(int page, int rowsPerPage)
        {
            try
            {
                int skip = (page - 1) * rowsPerPage;
                int take = rowsPerPage;

                OracleCommand query = con.CreateCommand();
                query.CommandText = """
                                    select USERNAME,USER_ID,PASSWORD,ACCOUNT_STATUS,LOCK_DATE,EXPIRY_DATE,DEFAULT_TABLESPACE,TEMPORARY_TABLESPACE,LOCAL_TEMP_TABLESPACE,CREATED,PROFILE,INITIAL_RSRC_CONSUMER_GROUP,EXTERNAL_NAME,PASSWORD_VERSIONS,EDITIONS_ENABLED,AUTHENTICATION_TYPE,PROXY_ONLY_CONNECT,COMMON,LAST_LOGIN,ORACLE_MAINTAINED,INHERITED,DEFAULT_COLLATION,IMPLICIT,ALL_SHARD,EXTERNAL_SHARD,PASSWORD_CHANGE_DATE,MANDATORY_PROFILE_VIOLATION,
                                    count(*) over() as "TotalItems"
                                    from dba_users
                                    order by USERNAME,USER_ID
                                    offset :Skip rows 
                                    fetch next :Take rows only
                                    """;
                query.CommandType = CommandType.Text;
                query.Parameters.Add(new OracleParameter("Skip", skip));
                query.Parameters.Add(new OracleParameter("Take", take));
                OracleDataReader datareader = query.ExecuteReader();
                empDT = new DataTable();
                empDT.Load(datareader);
                if (totalItems == -1 && empDT.Rows.Count > 0)
                {
                    totalItems = int.Parse(empDT.Rows[0]["TotalItems"].ToString());
                    totalPages = (totalItems / rowsPerPage);
                    if (totalItems % rowsPerPage == 0) totalPages = (totalItems / rowsPerPage);
                    else totalPages = (int)(totalItems / rowsPerPage) + 1;
                }
                //string str = "";
                //foreach (DataColumn column in empDT.Columns)
                //{
                //    str += $"{column.ColumnName},";
                //}
                empDT.Columns.Remove("TotalItems");
                dataGridView.ItemsSource = empDT.DefaultView;
                DisplayMode = "Users";

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

                if (_currentPage == 1)
                {
                    PrevButton.IsEnabled = false;
                }
                else
                {
                    PrevButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void PrivilegesButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)dataGridView.SelectedItems[0];
            string username = (string)row.Row.ItemArray[0];
            PrivilegesWindow screen = new(con,username,DisplayMode);
            this.Hide();
            screen.ShowDialog();
            this.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(con != null)
            {
                con.Close();
            }
        }

        private void getRolesButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            totalItems = -1;
            GetRoles(_currentPage, _rowsPerPage);
            GrantRoleContextItem.IsEnabled = false;
            ChangePasswordContextItem.IsEnabled = false;
        }

        private void GetRoles(int page, int rowsPerPage)
        {
            try
            {
                int skip = (page - 1) * rowsPerPage;
                int take = rowsPerPage;

                OracleCommand query = con.CreateCommand();
                query.CommandText = """
                                    SELECT ROLE,ROLE_ID,PASSWORD_REQUIRED,AUTHENTICATION_TYPE,COMMON,ORACLE_MAINTAINED,INHERITED,IMPLICIT,EXTERNAL_NAME,
                                    count(*) over() as "TotalItems"
                                    FROM DBA_ROLES
                                    order by ROLE,ROLE_ID
                                    offset :Skip rows 
                                    fetch next :Take rows only
                                    """;
                query.CommandType = CommandType.Text;
                query.Parameters.Add(new OracleParameter("Skip", skip));
                query.Parameters.Add(new OracleParameter("Take", take));
                OracleDataReader datareader = query.ExecuteReader();
                //OracleDataAdapter oracleDataAdapter = new(query);
                //while( datareader.Read())
                //{
                //    string ID = (string) datareader["MANV"];
                //}

                roleDT = new DataTable();
                roleDT.Load(datareader);
                if (totalItems == -1 && roleDT.Rows.Count > 0)
                {
                    totalItems = int.Parse(roleDT.Rows[0]["TotalItems"].ToString());
                    totalPages = (totalItems / rowsPerPage);
                    if (totalItems % rowsPerPage == 0) totalPages = (totalItems / rowsPerPage);
                    else totalPages = (int)(totalItems / rowsPerPage) + 1;
                }
                //string str = "";
                //foreach (DataColumn column in roleDT.Columns)
                //{
                //    str += $"{column.ColumnName},";
                //}
                roleDT.Columns.Remove("TotalItems");
                dataGridView.ItemsSource = roleDT.DefaultView;
                DisplayMode = "Roles";

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

                if (_currentPage == 1)
                {
                    PrevButton.IsEnabled = false;
                }
                else
                {
                    PrevButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void GrantRoleButton_Click(object sender, RoutedEventArgs e)
        {
            if(DisplayMode == "Roles")
            {
                MessageBox.Show("This is a role!","Can not grant role",MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                DataRowView row = (DataRowView)dataGridView.SelectedItems[0];
                string username = (string)row.Row.ItemArray[0];
                GrantRoleWindow screen = new GrantRoleWindow(con, username);
                screen.ShowDialog();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)dataGridView.SelectedItems[0];
            string name = (string)row.Row.ItemArray[0];

            OracleCommand query1 = con.CreateCommand();
            query1.CommandType = CommandType.Text;
            query1.CommandText = """
                alter session set "_oracle_script"=TRUE
                """;
            query1.ExecuteNonQuery();

            OracleCommand query = con.CreateCommand();
            query.CommandType = CommandType.StoredProcedure;
            
            if(DisplayMode == "Users")
            {
                query.CommandText = """
                delete_user
                """;
                query.Parameters.Add(new OracleParameter("username", name));
            }
            else
            {
                query.CommandText = """
                delete_role
                """;
                query.Parameters.Add(new OracleParameter("rolename", name));
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

            MessageBox.Show($"Deleted {name}", "Success", MessageBoxButton.OK);
            if(DisplayMode == "Users")
            {
                GetUser(_currentPage, _rowsPerPage);
            }
            else
            {
                GetRoles(_currentPage, _rowsPerPage);
            }
        }

        private void SelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((string)SelectorComboBox.SelectedItem == "USER") {
                UserPasswordBox.IsEnabled = true;
            }
            else if((string)SelectorComboBox.SelectedItem == "ROLE") {
                UserPasswordBox.IsEnabled = false;
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand query = con.CreateCommand();
            query.CommandType = CommandType.StoredProcedure;
            string name = NameTextBox.Text;
            if ((string)SelectorComboBox.SelectedItem == "USER")
            {
                query.CommandText = """
                create_user
                """;

                string password = UserPasswordBox.Password;

                query.Parameters.Add(new OracleParameter("username", name));
                query.Parameters.Add(new OracleParameter("password", password));

            }
            else if((string)SelectorComboBox.SelectedItem == "ROLE")
            {
                query.CommandText = """
                create_role
                """;

                query.Parameters.Add(new OracleParameter("name", name));

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

            MessageBox.Show($"Created {name}", "Success", MessageBoxButton.OK);
            if (DisplayMode == "Users")
            {
                GetUser(_currentPage, _rowsPerPage);
            }
            else
            {
                GetRoles(_currentPage, _rowsPerPage);
            }

            UserPasswordBox.Password = "";
            NameTextBox.Text = "";
        }

        private void ChangePasswordContextItem_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)dataGridView.SelectedItems[0];
            string name = (string)row.Row.ItemArray[0];
            ChangePasswordScreen screen = new ChangePasswordScreen(con, name);
            screen.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginScreen loginScreen = new LoginScreen();
            this.Hide();
            var result = loginScreen.ShowDialog();
            if (result == true)
            {
                con = loginScreen._connection;
                this.Show();
            }
            else
            {
                this.Close();
                return;
            }

            rowPerPageOptionsComboBox.ItemsSource = rowPerPageOptions;
            rowPerPageOptionsComboBox.SelectedIndex = 1;
            ViewUsersRadioButton.IsChecked = true;
            SelectorComboBox.ItemsSource = createOptions;
            SelectorComboBox.SelectedIndex = 0;

        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                if(DisplayMode == "Users")
                {
                    GetUser(_currentPage, _rowsPerPage);
                }
                else if(DisplayMode == "Roles")
                {
                    GetRoles(_currentPage, _rowsPerPage);
                }
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < totalPages)
            {
                _currentPage++;
                if (DisplayMode == "Users")
                {
                    GetUser(_currentPage, _rowsPerPage);
                }
                else if (DisplayMode == "Roles")
                {
                    GetRoles(_currentPage, _rowsPerPage);
                }
            }
        }

        private void rowPerPageOptionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _rowsPerPage = (int)rowPerPageOptionsComboBox.SelectedItem;
            _currentPage = 1;
            
            if(totalItems != -1)
            {
                if (DisplayMode == "Users")
                {
                    getUsersButton_Click(sender, e);
                }
                else if (DisplayMode == "Roles")
                {
                    getRolesButton_Click(sender, e);
                }
            }
        }
    }
}