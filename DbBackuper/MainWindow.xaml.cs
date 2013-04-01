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
        /// <summary>
        /// 本地端有掛載的所有資料庫。
        /// </summary>
        private BindingList<DatabaseItem> _dataDatabases = new BindingList<DatabaseItem>();
        // private ObservableCollection<CheckedListItem> TopicList;
        #endregion
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded_1(object sender, RoutedEventArgs e)
        {
            Initialze();
        }

        #region Private DRY Method

        private void Initialze() {
            // Dropdownlist of databases
            cmbDatabases.ItemsSource = LoadLocalDatabases();
            cmbDatabases.SelectedIndex = 0;



        }
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
        private List<string> LoadTables(string database)
        {
            List<string> tables = new List<string>();
            tables.Add("--- Please Select ---");
            string strdb = string.Format("Server=localhost;Database={0};Integrated Security=True;", database);
            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(strdb))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_tables";

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    if(dr[3].ToString().ToLower() == "table")
                    tables.Add(dr[2].ToString());
                }
            }

            return tables;
        }

        #endregion

        #region Events
        private void cmbDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DbContext db = new DbContext("Server=localhost;Integrated Security=True");
            if(cmbDatabases.SelectedIndex != 0)
            {
                // cmbTables.ItemsSource = LoadTables(cmbDatabases.SelectedItem.ToString());
            }
            
        }
        #endregion

        #region Models
        
        // For ComboboxItem of Database
        public class DatabaseItem : INotifyPropertyChanged
        {
            private string _dbname;

            public string DatabaseName
            {
                get
                {
                    return _dbname;
                }
                set
                {
                    _dbname = value;
                    RaisePropertyChanged("DatabaseName");
                }
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
