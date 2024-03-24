using FsCheck.Experimental;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for GrantRoleWindow.xaml
    /// </summary>
    public partial class GrantRoleWindow : Window
    {
        public OracleConnection _con { get; set; }
        DataTable __RoleDT = new DataTable();
        public string username { get; set; }

        List<int> rowPerPageOptions = new List<int>() {
            4,16,32,64,128,256,512,1024
        };

        int _rowsPerPage = 15;
        int _currentPage = 1;
        int totalPages = -1;
        int totalItems = -1;
        public GrantRoleWindow(OracleConnection con, string username)
        {
            InitializeComponent();
            _con = con;
            this.username = username;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rowPerPageOptionsComboBox.ItemsSource = rowPerPageOptions;
            rowPerPageOptionsComboBox.SelectedIndex = 1;
        }

        private void GetRoles(int page, int rowsPerPage)
        {
            try
            {
                int skip = (page - 1) * rowsPerPage;
                int take = rowsPerPage;

                OracleCommand query = _con.CreateCommand();
                query.CommandText = """
                                    SELECT ROLE,ROLE_ID,PASSWORD_REQUIRED,AUTHENTICATION_TYPE,COMMON,ORACLE_MAINTAINED,INHERITED,IMPLICIT,EXTERNAL_NAME,
                                    count(*) over() as "TotalItems"
                                    FROM DBA_ROLES
                                    order by ROLE, ROLE_ID
                                    offset :Skip rows 
                                    fetch next :Take rows only
                                    """;
                query.CommandType = CommandType.Text;
                query.Parameters.Add(new OracleParameter("Skip", skip));
                query.Parameters.Add(new OracleParameter("Take", take));
                OracleDataReader datareader = query.ExecuteReader();

                __RoleDT = new DataTable();
                __RoleDT.Load(datareader);
                if (totalItems == -1 && __RoleDT.Rows.Count > 0)
                {
                    totalItems = int.Parse(__RoleDT.Rows[0]["TotalItems"].ToString());
                    totalPages = (totalItems / rowsPerPage);
                    if (totalItems % rowsPerPage == 0) totalPages = (totalItems / rowsPerPage);
                    else totalPages = (int)(totalItems / rowsPerPage) + 1;
                }
                __RoleDT.Columns.Remove("TotalItems");
                dataGridView.ItemsSource = __RoleDT.DefaultView;

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

        private void GrantRole_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)dataGridView.SelectedItems[0];
            string rolename = (string)row.Row.ItemArray[0];
            string adminOption = " ";

            OracleCommand query = _con.CreateCommand();
            query.CommandText = """
                grant_role
                """;
            query.CommandType = CommandType.StoredProcedure;
            
            query.Parameters.Add(new OracleParameter("username", username));
            query.Parameters.Add(new OracleParameter("rolename", rolename));
            query.Parameters.Add(new OracleParameter("adminOption", adminOption));

            try
            {
                query.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                return;
            }

            var resultScreen = new ResultWindow(_con, username, rolename);
            resultScreen.ShowDialog();
            DialogResult = true;
        }

        private void GrantRoleWithOption_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)dataGridView.SelectedItems[0];
            string rolename = (string)row.Row.ItemArray[0];
            string adminOption = "WITH ADMIN OPTION";

            OracleCommand query = _con.CreateCommand();
            query.CommandText = """
                grant_role
                """;
            query.CommandType = CommandType.StoredProcedure;
            
            query.Parameters.Add(new OracleParameter("username", username));
            query.Parameters.Add(new OracleParameter("rolename", rolename));
            query.Parameters.Add(new OracleParameter("adminOption", adminOption));

            try
            {
                query.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                return;
            }

            var resultScreen = new ResultWindow(_con, username, rolename);
            resultScreen.ShowDialog();
            DialogResult = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                GetRoles(_currentPage, _rowsPerPage);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < totalPages)
            {
                _currentPage++;
                GetRoles(_currentPage, _rowsPerPage);
            }
        }

        private void rowPerPageOptionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _rowsPerPage = (int)rowPerPageOptionsComboBox.SelectedItem;
            _currentPage = 1;
            totalItems = -1;
            GetRoles(_currentPage, _rowsPerPage);
        }
    }
}
