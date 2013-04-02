using AvalonWizard;
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
        public ObservableCollection<CheckedListItem> _tables = new ObservableCollection<CheckedListItem>();
        #endregion

        #region Contructor & Destructor
        /// <summary>
        /// 建構子
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 解構子
        /// </summary>
        ~MainWindow() 
        {
        }
        #endregion
       
        #region Control Events
        // 1: 初始化，載入資料
        private void Grid_Loaded_1(object sender, RoutedEventArgs e)
        {
            // Dropdownlist of databases
            cmbDatabases.ItemsSource = LoadLocalDatabases();
            cmbDatabases.SelectedIndex = 0;

            lstTables.ItemsSource = _tables;
        }

        private void cmbDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDatabases.SelectedIndex != 0)
            {
                LoadTables(cmbDatabases.SelectedItem.ToString());
            }

        }
        

        private void wizard_Cancelled(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void wizard_Commit(object sender, AvalonWizard.WizardPageConfirmEventArgs e)
        {
            switch (e.Page.Name)
            { 
                case "first":
                    if (_tables.Select(x => x.IsChecked == true).Count() < 1)
                    {
                        MessageBox.Show("Tables has no select.");
                        e.Cancel = true;
                    }
                    break;
                default:
                    break;
            }
        }
        private void btnValidateConn_Click(object sender, RoutedEventArgs e)
        {
            string connstring = "";
            if (cmbSwitcher.SelectedItem.ToString() == "local")
            {
                connstring = "Server=localhost;Integrated Security=True";
            }
            {
                connstring = string.Format("Server={0};User ID={1};Password={2}", txtRemote.Text.Trim(), txtAccount.Text.Trim(), pwd.Password.Trim());
            }
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += bgwValidateConnection_DoWorkHandler;
            bgw.RunWorkerCompleted += bgwValidateConnection_RunWorkerCompleted;
            bgw.WorkerReportsProgress = true;
            bgw.RunWorkerAsync(connstring);
        }

        private void btnSourceValidateConn_Click(object sender, RoutedEventArgs e)
        {
            string connstring = string.Format("Server={0};User ID={1};Password={2}", txtSource.Text.Trim(), txtSourceAccount.Text.Trim(), pwd.Password.Trim());
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += bgwValidateConnection_DoWorkHandler;
            bgw.RunWorkerCompleted += bgwValidateConnection_RunWorkerCompleted;
            bgw.WorkerReportsProgress = true;
            bgw.RunWorkerAsync(connstring);
        }

        public void bgwValidateConnection_DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            e.Result = IsServerConnected(e.Argument.ToString());
        }

        private void bgwValidateConnection_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result)
            {
                string uri = String.Format(@"pack://application:,,,/DbBackuper;component/Images/tick.png");
                imgStatus.Source = new BitmapImage(new Uri(uri));
                imgStatus.Visibility = System.Windows.Visibility.Visible;

            }
            else
            {
                string uri = String.Format(@"pack://application:,,,/DbBackuper;component/Images/error.png");
                imgStatus.Source = new BitmapImage(new Uri(uri));
                imgStatus.Visibility = System.Windows.Visibility.Visible;
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
                _tables.Clear();
                while (dr.Read())
                {
                    if (dr[3].ToString().ToLower() == "table")
                    {
                        _tables.Add(new CheckedListItem { Name = dr[2].ToString(), IsChecked = false });
                    }
                }
            }
        }
        private static bool IsServerConnected(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
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

        private void cmbSourceSwitcher_Selected_1(object sender, RoutedEventArgs e)
        {

        }

       

        

    }
}
