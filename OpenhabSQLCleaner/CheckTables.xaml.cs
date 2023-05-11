using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for CheckTables.xaml
    /// </summary>
    public partial class CheckTables : Window
    {
        TaskFactory taskFactory = new TaskFactory();
        public CheckTables()
        {
            InitializeComponent();
        }

        private void LoadData()
        {
            List<string> tables = new List<string>();
            List<string[]> rows = new List<string[]>();
            DataTable dataTable = new DataTable()
            {
                Columns = {"Id", "TableName"}
            };
            Menus.mySqlConnection.Open();

            string query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'OpenHAB2';";
            var cmd = new MySqlCommand(query, Menus.mySqlConnection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
            reader.Close();
            Menus.mySqlConnection?.Close();
            ParallelOptions pOpt = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 5,
            };
            Parallel.ForEach(tables, table =>
            {
                if (table != "items")
                {
                    MySqlConnection tmpCon = new MySqlConnection(Menus.connstring);
                    query = "SELECT * FROM OpenHAB2." + table + " order by time desc LIMIT 1;";
                    cmd = new MySqlCommand(query, tmpCon);
                    reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        dataTable.Rows.Add(new string[] { table.Split('_').Last().TrimStart('0'), table });
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            dgData.ItemsSource = dataTable.AsDataView();
                        }));
                    }
                    reader.Close();
                    tmpCon.Close();
                }
            });
            //foreach(var table in tables)
            //{
            //    if(table != "items")
            //    {
            //        query = "SELECT * FROM OpenHAB2." + table + " order by time desc LIMIT 1;";
            //        cmd = new MySqlCommand(query, Menus.mySqlConnection);
            //        reader = cmd.ExecuteReader();
            //        if(!reader.HasRows)
            //        {
            //            dataTable.Rows.Add(new string[] { table.Split('_').Last<string>().TrimStart('0'), table });
            //        }
            //        reader.Close();
            //    }
            //}
        }

        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            var task = taskFactory.StartNew(() => LoadData());
        }
    }
}
