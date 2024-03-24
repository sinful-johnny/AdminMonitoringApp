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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AdminMonitor
{
    /// <summary>
    /// Interaction logic for ResultWindow.xaml
    /// </summary>
    public partial class ResultWindow : Window
    {
        string DisplayMode = "";
        string name = string.Empty;
        string rolename = string.Empty;
        string operation = string.Empty;
        string tableOwner = string.Empty;
        string tableName = string.Empty;
        string columnList = string.Empty;
        OracleConnection _con;
        DataTable table;
        public ResultWindow(OracleConnection con,string operation, string tableOwner,string table,string schema, string columnList)
        {
            InitializeComponent();
            if(columnList != null && columnList != " ")
            {
                DisplayMode = "ColumnPrivs";
            }
            else
            {
                DisplayMode = "TablePrivs";
            }
            _con = con;
            this.operation = operation;
            this.tableOwner = tableOwner;
            this.tableName = table;
            this.name = schema;
            this.columnList = columnList;
        }
        public ResultWindow(OracleConnection con,string username, string rolename)
        {
            InitializeComponent();

            DisplayMode = "Role";

            _con = con;
            this.name =username;
            this.rolename = rolename;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(DisplayMode == "TablePrivs")
            {
                GetTablePrivs();
            }
            else if(DisplayMode == "ColumnPrivs")
            {
                GetColPrivs();
            }
            else if(DisplayMode == "Role")
            {
                GetRoles();
            }
        }

        private void GetRoles()
        {
            OracleCommand query = _con.CreateCommand();
            query.CommandType = CommandType.Text;
            query.CommandText = """
                                        select *
                                        from dba_role_privs
                                        where   grantee = :username
                                                and granted_role = :rolename
                                        """;
            query.Parameters.Add(new OracleParameter("username", name));
            query.Parameters.Add(new OracleParameter("rolename", rolename));

            try
            {
                OracleDataReader dr = query.ExecuteReader();
                table = new DataTable();
                table.Load(dr);
                dataGridView.ItemsSource = table.DefaultView;
                dr.Close();
            }
            finally { }
        }

        private void GetColPrivs()
        {
            OracleCommand query = _con.CreateCommand();
            query.CommandType = CommandType.Text;
            if (operation == "SELECT")
            {
                tableName = $"{name}_{tableName}_{operation}";
                query.CommandText = """
                                        select GRANTEE, OWNER, TABLE_NAME, GRANTOR, PRIVILEGE, GRANTABLE, HIERARCHY, COMMON, TYPE, INHERITED
                                        from dba_tab_privs
                                        where   grantee = :username
                                                and OWNER = :tableName
                                                and TABLE_NAME = :tableName
                                                and PRIVILEGE = :operation
                                        """;
            }
            else
            {
                query.CommandText = """
                                        Select grantee, owner, table_name, column_name, grantor, privilege,grantable, count(*) over() as "TotalItems"
                                        from DBA_COL_PRIVS
                                        where   grantee =  :username
                                                and OWNER = :tableOwner
                                                and TABLE_NAME = :tableName
                                                and PRIVILEGE = :operation
                                        """;
            }

            query.Parameters.Add(new OracleParameter("username", name));
            query.Parameters.Add(new OracleParameter("tableOwner", tableOwner));
            query.Parameters.Add(new OracleParameter("tableName", tableName));
            query.Parameters.Add(new OracleParameter("operation", operation));

            try
            {
                OracleDataReader dr = query.ExecuteReader();
                table = new DataTable();
                table.Load(dr);
                dataGridView.ItemsSource = table.DefaultView;
                dr.Close();
            }
            finally { }
        }

        private void GetTablePrivs()
        {
            OracleCommand query = _con.CreateCommand();
            query.CommandType = CommandType.Text;
            query.CommandText = """
                                        select GRANTEE, OWNER, TABLE_NAME, GRANTOR, PRIVILEGE, GRANTABLE, HIERARCHY, COMMON, TYPE, INHERITED
                                        from dba_tab_privs
                                        where   grantee = :username
                                                and OWNER = :tableOwner
                                                and TABLE_NAME = :tableName
                                                and PRIVILEGE = :operation
                                        """;

            query.Parameters.Add(new OracleParameter("username", name));
            query.Parameters.Add(new OracleParameter("tableOwner", tableOwner));
            query.Parameters.Add(new OracleParameter("tableName", tableName));
            query.Parameters.Add(new OracleParameter("operation", operation));

            try
            {
                OracleDataReader dr = query.ExecuteReader();
                table = new DataTable();
                table.Load(dr);
                dataGridView.ItemsSource = table.DefaultView;
                dr.Close();
            }
            finally { }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
