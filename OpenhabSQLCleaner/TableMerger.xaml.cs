using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace OpenhabSQLCleaner
{
    /// <summary>
    /// Interaction logic for TableMerger.xaml
    /// </summary>

    public partial class TableMerger : Window
    {
        Dictionary<string, string[]> rows;
        KeyValuePair<string, string[]> selectedItem;
        readonly TaskFactory taskFactory = new TaskFactory();
        public TableMerger()
        {
            InitializeComponent();
            ConnectSQL();
        }

        private void ConnectSQL()
        {
            rows = new Dictionary<string, string[]>();
            Menus.mySqlConnection.Open();

            string query = "SELECT * FROM items order by ItemId";
            var cmd = new MySqlCommand(query, Menus.mySqlConnection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                try
                {
                    rows.Add(reader.GetString(1).ToLower(), new string[] { reader.GetString(0).PadLeft(4, '0'), "" });
                }
                catch
                {
                    rows[reader.GetString(1).ToLower()][1] = reader.GetString(0).PadLeft(4, '0');
                }
            }
            reader.Close();
            Menus.mySqlConnection?.Close();

            List<string> singles = new List<string>();
            foreach(var item in rows)
            {
                if (item.Value[1] == "")
                {
                    singles.Add(item.Key);
                }
            }
            foreach(var single in singles)
            {
                rows.Remove(single);
            }
            cbOrig.ItemsSource = rows;
            cbOrig.DisplayMemberPath = "Key";
            lbCount.Content = "Number of duplicates found: " + rows.Count;
            cbOrig.SelectedIndex = 0;
        }
        bool selected = true;
        private void CbOrig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selected)
            {
                selectedItem = (KeyValuePair<string, string[]>)(sender as ComboBox).SelectedItem;
                tblLabel.Text = selectedItem.Key + " has duplicates in: " + selectedItem.Value[0] + " and " + selectedItem.Value[1];
            }
        }

        private void BtnRunSQL_Click(object sender, RoutedEventArgs e)
        {
            if(selectedItem.Key != null)
            {
                var task = taskFactory.StartNew(() =>
                {
                    Dispatcher.Invoke(() => {
                        btnRunSQL.IsEnabled = false;
                        cbOrig.IsEnabled = false;
                        progBar.IsIndeterminate = true;
                        tblLabel.Text = "Merging " + selectedItem.Key + " " + selectedItem.Value[0] + " into " + 
                                         selectedItem.Value[1];

                    });
                    RunMerge();
                    Dispatcher.Invoke(() => {
                        btnRunSQL.IsEnabled = true;
                        cbOrig.IsEnabled = true;
                        progBar.IsIndeterminate = false;
                        tblLabel.Text = "Merging is finished!";
                        lbCount.Content = "Number of duplicates found: " + rows.Count;
                        cbOrig.SelectedIndex = 0;
                    });

                });
            }
        }

        private void RunMerge()
        {
            
            Menus.mySqlConnection.Open();

            string oldtable = selectedItem.Key + "_" + selectedItem.Value[0];
            string newtable = selectedItem.Key + "_" + selectedItem.Value[1];
            string query = "insert into OpenHAB2." + newtable + " select * from OpenHAB2." + oldtable + ";";
            var cmd = new MySqlCommand(query, Menus.mySqlConnection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            string droptable = "DROP TABLE OpenHAB2." + oldtable + ";";
            cmd = new MySqlCommand(droptable, Menus.mySqlConnection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            string deleteItem = "delete from OpenHAB2.items where ItemID = " + selectedItem.Value[0].TrimStart('0') + ";";
            cmd = new MySqlCommand(deleteItem, Menus.mySqlConnection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Menus.mySqlConnection?.Close();
            RemoveCBItem();
        }

        private void RemoveCBItem()
        {
            selected = false;
            rows.Remove(selectedItem.Key);
            Dispatcher.Invoke(() =>
            {
                cbOrig.ItemsSource = null;
                cbOrig.ItemsSource = rows;
            });
            selectedItem = new KeyValuePair<string, string[]>();
            selected = true;
        }
    }
}
