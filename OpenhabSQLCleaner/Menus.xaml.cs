using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

namespace OpenhabSQLCleaner
{
    /// <summary>
    /// Interaction logic for Menus.xaml
    /// </summary>
    public partial class Menus : Window
    {
        public static MySqlConnection mySqlConnection;
        private MySecrets mySecrets = new MySecrets();
        public static string connstring;
        public Menus()
        {
            InitializeComponent(); 
            connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", mySecrets.GetSecrets[0], mySecrets.GetSecrets[1], mySecrets.GetSecrets[2], mySecrets.GetSecrets[3]);
            mySqlConnection = new MySqlConnection(connstring);
        }
        private void BtnTableMerger_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            TableMerger window = new TableMerger();
            window.ShowDialog();
            Show();
        }

        private void BtnCleanSQLtables_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            CleanSQLTables window = new CleanSQLTables();
            window.ShowDialog();
            Show();
        }

        private void BtnCheckTables_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            CheckTables window = new CheckTables();
            window.ShowDialog();
            Show();
        }

        private void btnRefTest_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
