using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        public LoginScreen()
        {
            InitializeComponent();
        }
        public OracleConnection _connection;

        List<string> privileges = new List<string>() { 
            "","SYSDBA","SYSOPER"
        };
        void Encrypt(string password, string username,string server, string privilege)
        {
            var passwordInBytes = Encoding.UTF8.GetBytes(password);
            var entropy = new byte[20];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }
            var cypherText = ProtectedData.Protect(passwordInBytes, entropy, DataProtectionScope.CurrentUser);
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["username"].Value = username;
            config.AppSettings.Settings["DABPRIVILEGE"].Value = (string)DBAPrivilegeComboBox.SelectedItem;
            config.AppSettings.Settings["DATASOURCE"].Value = DataSourceTextBox.Text;
            config.AppSettings.Settings["password"].Value = Convert.ToBase64String(cypherText);
            config.AppSettings.Settings["entropy"].Value = Convert.ToBase64String(entropy);
            config.AppSettings.Settings["isPasswordRemmembered"].Value = "1";
            config.Save(ConfigurationSaveMode.Minimal);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string password = PasswordBox.Password;
            string username = UsernameTextBox.Text;
            string server = DataSourceTextBox.Text;
            string privilege = DBAPrivilegeComboBox.Text;

            string conStr = $"""
                                    DATA SOURCE={server};
                                    DBA PRIVILEGE={privilege};
                                    PERSIST SECURITY INFO=True;
                                    USER ID={username};
                                    PASSWORD={password}
                                    """;
            
            try
            {
                _connection = new OracleConnection(conStr);
                _connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            if (_connection.State == ConnectionState.Open)
            {
                MessageBox.Show( "Logged in successfully!", "Success", MessageBoxButton.OK);
                if (RemembermeCheckBox.IsChecked == true)
                {
                    Encrypt(password, username,server,privilege);
                }
                else
                {
                    config.AppSettings.Settings["isPasswordRemmembered"].Value = "0";
                    config.AppSettings.Settings["username"].Value = " ";
                    config.AppSettings.Settings["password"].Value = " ";
                    config.AppSettings.Settings["entropy"].Value = " ";
                    config.AppSettings.Settings["DABPRIVILEGE"].Value = (string)DBAPrivilegeComboBox.SelectedItem;
                    config.AppSettings.Settings["DATASOURCE"].Value = DataSourceTextBox.Text;
                    config.Save(ConfigurationSaveMode.Minimal);

                }
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Wrong credential!", "Log in failed", MessageBoxButton.OK);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings["isPasswordRemmembered"].Value == "1")
            {
                UsernameTextBox.Text = config.AppSettings.Settings["username"].Value;
                var cypherText = Convert.FromBase64String(ConfigurationManager.AppSettings["password"]);
                var entropy = Convert.FromBase64String(ConfigurationManager.AppSettings["entropy"]);
                var decryptedPassword = ProtectedData.Unprotect(cypherText, entropy, DataProtectionScope.CurrentUser);
                var realPassword = Encoding.UTF8.GetString(decryptedPassword);


                PasswordBox.Password = realPassword;
                RemembermeCheckBox.IsChecked = true;
            }

            DataSourceTextBox.Text = config.AppSettings.Settings["DATASOURCE"].Value;
            DBAPrivilegeComboBox.ItemsSource = privileges;
            DBAPrivilegeComboBox.SelectedItem = privileges.Single(x => x == config.AppSettings.Settings["DABPRIVILEGE"].Value);
        }
    }
}
