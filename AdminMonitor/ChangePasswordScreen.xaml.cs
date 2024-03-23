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
using System.Xml.Linq;

namespace AdminMonitor
{
    /// <summary>
    /// Interaction logic for ChangePasswordScreen.xaml
    /// </summary>
    public partial class ChangePasswordScreen : Window
    {
        string _username = string.Empty;
        OracleConnection _conn;
        public ChangePasswordScreen(OracleConnection con,string username)
        {
            InitializeComponent();
            _conn = con;
            _username = username;
            UsernameTextBlock.Text = _username;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand query = _conn.CreateCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = """
                change_password
                """;

            string password = NewPasswordBox.Password;
            query.Parameters.Add(new OracleParameter("username", _username));
            query.Parameters.Add(new OracleParameter("password", password));

            try
            {
                query.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Changed password of {_username}", "Success", MessageBoxButton.OK);
            DialogResult = true;
        }
    }
}
