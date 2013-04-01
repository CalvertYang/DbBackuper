using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DbBackuper
{

    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        public ObservableCollection<CheckedListItem> Tables = new ObservableCollection<CheckedListItem>();
        #endregion
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Events

        private void Grid_Loaded_1(object sender, RoutedEventArgs e)
        {
            // Dropdownlist of databases
            cmbDatabases.ItemsSource = LoadLocalDatabases();
            cmbDatabases.SelectedIndex = 0;

            lstTables.ItemsSource = Tables;
        }

        private void cmbDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDatabases.SelectedIndex != 0)
            {
                LoadTables(cmbDatabases.SelectedItem.ToString());
            }

        }
        #endregion
       

        #region Private DRY Method
        
        private List<string> LoadLocalDatabases()
        {
            List<string> databases = new List<string>();
            databases.Add("--- Please Select ---");
            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection("Server=localhost;Integrated Security=True"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_databases";

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                   databases.Add(dr[0].ToString());
                }
            }

            return databases;
        }
        private void LoadTables(string database)
        {
            string strdb = string.Format("Server=localhost;Database={0};Integrated Security=True;", database);
            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(strdb))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_tables";

                SqlDataReader dr = cmd.ExecuteReader();
                Tables.Clear();
                while (dr.Read())
                {
                    if (dr[3].ToString().ToLower() == "table")
                    {
                        Tables.Add(new CheckedListItem { Name = dr[2].ToString(), IsChecked = false });
                    }
                }
            }
        }

        #endregion

        

        #region Models

        public class CheckedListItem : INotifyPropertyChanged
        {
            private int _id;
            private string _name;
            private bool _ischecked;


            public int Id
            {
                get
                {
                    return _id;
                }
                set
                {
                    _id = value;
                    RaisePropertyChanged("Id");
                }
            }

            public string Name
            {
                get { return _name; }
                set { _name = value; RaisePropertyChanged("Name"); }
            }

            public bool IsChecked
            {
                get { return _ischecked; }
                set { _ischecked = value; RaisePropertyChanged("IsChecked"); }
            }
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void RaisePropertyChanged(String propertyName)
            {
                if ((PropertyChanged != null))
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

        }
        #endregion

    }
}
