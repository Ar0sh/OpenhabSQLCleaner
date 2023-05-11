using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;

namespace OpenhabSQLCleaner
{
    public partial class CleanSQLTables : Window
    {
        readonly TaskFactory taskFactory = new TaskFactory();
        public CleanSQLTables()
        {
            InitializeComponent();
        }

        private void ConnectSQL()
        {
            DataTable errors = new DataTable()
            { 
                Columns = { "Error" }
            };
            Menus.mySqlConnection.Open();
            string query = "SELECT * FROM items";
            var cmd = new MySqlCommand(query, Menus.mySqlConnection);
            var reader = cmd.ExecuteReader();
            List<string> rows = new List<string>();
            while(reader.Read())
            {
                rows.Add(reader.GetString(1).ToLower() + "_" + reader.GetString(0).PadLeft(4, '0'));
            }
            reader.Close();
            int i = 1;
            var pOpt = new ParallelOptions() 
            {
                MaxDegreeOfParallelism = 5,
            };
            Parallel.ForEach(rows, pOpt, row =>
            {
                MySqlConnection tmpCon = new MySqlConnection(Menus.connstring);
                tmpCon.Open();
                try
                {
                    string query2 = $"delete from {row} where time<DATE_SUB(NOW(),INTERVAL 1 YEAR)";
                    var cmd2 = new MySqlCommand(query2, tmpCon);
                    cmd2.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Debug.WriteLine(ex.Message, "ERROR");
                    errors.Rows.Add(ex.Message);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        dgError.ItemsSource = errors.AsDataView();
                    }));
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    progBar.Value = ((double)i / rows.Count()) * 100;
                    progLabel.Content = row + ": Deleted data older than 1 year.";
                }));
                Debug.WriteLine(row, "DONE");
                i++;
                tmpCon.Close();
                Thread.Sleep(100);
            });
            Menus.mySqlConnection?.Close();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var task = taskFactory.StartNew(() => ConnectSQL());
        }
    }
}
