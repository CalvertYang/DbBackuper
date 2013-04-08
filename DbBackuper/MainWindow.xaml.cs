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
// Source: http://msdn.microsoft.com/en-us/library/ms162129%28v=sql.105%29
using Smo=Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.SqlEnum;
using System.Collections.Specialized;

namespace DbBackuper
{

    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        
        private const string LOCAL_CONNSTRING_NO_DATABASE = "Server=localhost;Integrated Security=True;";
        private const string LOCAL_FORMAT_WITH_DATABASE = "Server=localhost;Integrated Security=True;Database={0};";
        private const string REMOTE_FORMAT_NO_DATABASE = "Server={0};User ID={1};Password={2};";
        private const string REMOTE_FORMAT_WITH_DATABASE = "Server={0};User ID={1};Password={2};Database={3};";
        public ObservableCollection<CheckedListItem> _tables = new ObservableCollection<CheckedListItem>();
        private string _source_connstring;
        private string _target_connstring;
        private DateTime _from;
        private DateTime _to;

        #endregion

        #region Contructor & Destructor
        /// <summary>
        /// 建構子
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // 檢查是否有記憶 帳號 密碼
            chkSourceRemember.IsChecked = Properties.Settings.Default.IsRememberSource;
            chkTargetRemember.IsChecked = Properties.Settings.Default.IsRememberTarget;
            
            if (Properties.Settings.Default.IsRememberSource)
            {
                txtSourceLocation.Text = Properties.Settings.Default.SourceLocation;
                txtSourceAccount.Text = Properties.Settings.Default.SourceAccount;
                pwdSource.Password = Properties.Settings.Default.SourcePassword;
               
            }
            if (Properties.Settings.Default.IsRememberTarget)
            {
                txtTargetLocation.Text = Properties.Settings.Default.TargetLocation;
                txtTargetAccount.Text = Properties.Settings.Default.TargetAccount;
                pwdTarget.Password = Properties.Settings.Default.TargetPassword;
            }

            if (String.IsNullOrEmpty(txtSourceLocation.Text) && String.IsNullOrEmpty(txtSourceAccount.Text) && String.IsNullOrEmpty(pwdSource.Password))
            {
                cmbSourceSwitcher.SelectedIndex = 0;
            }
            else
            {
                cmbSourceSwitcher.SelectedIndex = 1;
            }

            if (String.IsNullOrEmpty(txtTargetLocation.Text) && String.IsNullOrEmpty(txtTargetAccount.Text) && String.IsNullOrEmpty(pwdTarget.Password))
            {
                cmbTargetSwitcher.SelectedIndex = 0;
            }
            else
            {
                cmbTargetSwitcher.SelectedIndex = 1;
            }

        }
        /// <summary>
        /// 解構子
        /// </summary>
        ~MainWindow() 
        {
        }
        #endregion
       
        #region Control Events

        #region Description
        /* Description
         * N[sequence]: Normal Control 
         * P: Wizard Page
         * */
        #endregion

        // N1: 初始化，載入資料
        private void Grid_Loaded_1(object sender, RoutedEventArgs e)
        {
            
            
        }

        #region Page-1
        // P1: Validate "Source" connection 
        private void SourceValidateConn_Click(object sender, RoutedEventArgs e)
        {
            DealSourceConnectionString();
            RunValidateConnection(this._source_connstring, "source");
           
        }
        // P1: Checkbox privode remember information
        private void SourceRemember_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsRememberSource = Convert.ToBoolean(chkSourceRemember.IsChecked);
            if (!Convert.ToBoolean(chkSourceRemember.IsChecked))
            {
                Properties.Settings.Default.SourceAccount = "";
                Properties.Settings.Default.SourceLocation = "";
                Properties.Settings.Default.SourcePassword = "";
            }
            // When validate or commit next and run success save correct current information.
            Properties.Settings.Default.Save();
        }
        // P1: 切換local/remote
        private void SourceSwitch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (imgSourceStatus != null)
            {
                imgSourceStatus.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        #endregion

        #region Page-2
        // P2-1: Combobox Select Event
        private void SourceDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSourceDatabases.SelectedIndex != 0)
            {
                string format = this._source_connstring + "Database={0};";
                string conn = string.Format(format,cmbSourceDatabases.SelectedItem.ToString());
                LoadTables(conn);
            }

        }
        #endregion

        #region Page-3
        // P3: Show/Hide control for set login information
        private void TargetSwitch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtTargetAccount != null)
            {
                if ((cmbTargetSwitcher.SelectedItem as ComboBoxItem).Tag.ToString() == "Hide")
                {
                    txtTargetAccount.Visibility = System.Windows.Visibility.Collapsed;
                    txtTargetLocation.Visibility = System.Windows.Visibility.Collapsed;
                    lbTargetAccount.Visibility = System.Windows.Visibility.Collapsed;
                    lbTargetPassword.Visibility = System.Windows.Visibility.Collapsed;
                    lbTargetSummaryErrorMsg.Visibility = System.Windows.Visibility.Collapsed;
                    pwdTarget.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    txtTargetAccount.Visibility = System.Windows.Visibility.Visible;
                    txtTargetLocation.Visibility = System.Windows.Visibility.Visible;
                    lbTargetAccount.Visibility = System.Windows.Visibility.Visible;
                    lbTargetPassword.Visibility = System.Windows.Visibility.Visible;
                    lbTargetSummaryErrorMsg.Visibility = System.Windows.Visibility.Visible;
                    pwdTarget.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
        // P3:  Validate "Target" connection 
        private void TargetValidateConn_Click(object sender, RoutedEventArgs e)
        {
            DealTargetConnectionString();
            RunValidateConnection(this._target_connstring,"target");
        }
        // P3: Checkbox privode remember information
        private void TargetRemember_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsRememberTarget = Convert.ToBoolean(chkTargetRemember.IsChecked);
            if (!Convert.ToBoolean(chkTargetRemember.IsChecked))
            {
                Properties.Settings.Default.TargetLocation = "";
                Properties.Settings.Default.TargetAccount = "";
                Properties.Settings.Default.TargetPassword = "";
            }
            // When validate or commit next and run success save correct current information.
            Properties.Settings.Default.Save();
        }
        #endregion

        #region Page-4
        // P4: show/hide specify date range
        private void BackupDateRange_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)chkBackupDateRange.IsChecked)
            {
                lbFrom.Visibility = System.Windows.Visibility.Visible;
                dpFrom.Visibility = System.Windows.Visibility.Visible;
                lbTo.Visibility = System.Windows.Visibility.Visible;
                dpTo.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lbFrom.Visibility = System.Windows.Visibility.Collapsed;
                dpFrom.Visibility = System.Windows.Visibility.Collapsed;
                lbTo.Visibility = System.Windows.Visibility.Collapsed;
                dpTo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        #endregion

        #region Page-5
        // P5: 
        private void RunBackup_Click(object sender, RoutedEventArgs e)
        {
            string msgtext = "Are you sure continue?";
            string caption = " Dialog";

            MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
            MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

            MessageBoxResult rsltMessageBox = MessageBox.Show(msgtext, caption, btnMessageBox, icnMessageBox);

            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    RunBackup();
                    break;

                case MessageBoxResult.No:
                    /* ... */
                    break;

                case MessageBoxResult.Cancel:
                    /* ... */
                    break;
            }
        }
        #endregion
        #region Wizard Events
        // Close
        private void Wizard_Cancelled(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        // NEXT
        private void Wizard_Commit(object sender, AvalonWizard.WizardPageConfirmEventArgs e)
        {
            switch (e.Page.Name)
            {
                case "first":
                    bool source_local_condition = ((cmbSourceSwitcher.SelectedItem as ComboBoxItem).Content.ToString() == "Local"); 
                    bool source_remote_condition = ((cmbSourceSwitcher.SelectedItem as ComboBoxItem).Content.ToString() == "Remote") 
                        && (!String.IsNullOrEmpty(txtSourceAccount.Text) && !String.IsNullOrEmpty(txtSourceLocation.Text) 
                        && !String.IsNullOrEmpty(pwdSource.Password));
                    bool source_pass = source_local_condition || source_remote_condition;

                    if (source_pass)
                    {
                        DealSourceConnectionString();
                        RunValidateConnection(this._source_connstring, "source");

                        cmbSourceDatabases.ItemsSource = LoadDatabases(this._source_connstring);
                        cmbSourceDatabases.SelectedIndex = 0;

                        lstSourceTables.ItemsSource = _tables;
                    }
                    else
                    {
                        MessageBox.Show("Information not completed!!!");
                        e.Cancel = true;
                    }

                    
                    break;
                case "second":
                    if (_tables.Where(t => t.IsChecked == true).ToList().Count() < 1)
                    {
                        MessageBox.Show("Please choose more than one.");
                        e.Cancel = true;
                    }
                    

                    break;
                case "third":
                    bool target_local_condition = ((cmbTargetSwitcher.SelectedItem as ComboBoxItem).Content.ToString() == "Local");
                    bool target_remote_condition = ((cmbTargetSwitcher.SelectedItem as ComboBoxItem).Content.ToString() == "Remote")
                                && (!String.IsNullOrEmpty(txtTargetAccount.Text) && !String.IsNullOrEmpty(txtTargetLocation.Text) 
                                && !String.IsNullOrEmpty(pwdTarget.Password));
                    bool target_pass = target_local_condition || target_remote_condition;
                    if (target_pass)
                    {
                        DealTargetConnectionString();
                        RunValidateConnection(this._target_connstring, "source");
                        txtBackupDatabaseName.Text = cmbSourceDatabases.SelectedItem.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Information not completed!!!");
                        e.Cancel = true;
                    }
                    break;
                case "fourth":
                    
                    if (String.IsNullOrEmpty(txtBackupDatabaseName.Text))
                    {
                        MessageBox.Show("Please enter database name for backup");
                        e.Cancel = true;
                    }
                    if ((bool)chkBackupDateRange.IsChecked)
                    {
                        if (dpFrom.SelectedDate != null && dpTo.SelectedDate != null)
                        {
                            this._from = (DateTime)dpFrom.SelectedDate;
                            this._to = (DateTime)dpTo.SelectedDate;
                        }
                        else
                        {
                            MessageBox.Show("Please select date range");
                            e.Cancel = true;
                        }
                    }

                    break;
                case "fifth":
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Threads
        public void bgwValidateConnection_DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            List<string> result = e.Argument as List<string>;
            ConnectionStatus s = new ConnectionStatus();
            s.IsConnected = IsServerConnected(result[0]);
            s.Whois = result[1];
            e.Result = s;
           
            
        }
        private void bgwValidateConnection_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ConnectionStatus status = (ConnectionStatus)e.Result;
            
            switch (status.Whois)
            { 
                case "target":
                    if (status.IsConnected)
                    {
                        string uri = String.Format(@"pack://application:,,,/DbBackuper;component/Images/tick.png");
                        imgTargetStatus.Source = new BitmapImage(new Uri(uri));
                        if (wizard.CurrentPage.Name == "third")
                            imgTargetStatus.Visibility = System.Windows.Visibility.Visible;
                        else
                            imgTargetStatus.Visibility = System.Windows.Visibility.Collapsed;
                       
                        if ((cmbTargetSwitcher.SelectedItem as ComboBoxItem).Content.ToString() == "Remote")
                        {
                            Properties.Settings.Default.TargetLocation = txtTargetLocation.Text.Trim();
                            Properties.Settings.Default.TargetAccount = txtTargetAccount.Text.Trim();
                            Properties.Settings.Default.TargetPassword = pwdTarget.Password.Trim();
                        }
                        else
                        {
                            Properties.Settings.Default.TargetLocation = "";
                            Properties.Settings.Default.TargetAccount = "";
                            Properties.Settings.Default.TargetPassword = "";
                        }
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        string uri = String.Format(@"pack://application:,,,/DbBackuper;component/Images/error.png");
                        imgTargetStatus.Source = new BitmapImage(new Uri(uri));
                        imgTargetStatus.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case "source":
                    if (status.IsConnected)
                    {
                        string uri = String.Format(@"pack://application:,,,/DbBackuper;component/Images/tick.png");
                        imgSourceStatus.Source = new BitmapImage(new Uri(uri));
                        if (wizard.CurrentPage.Name == "first")
                            imgSourceStatus.Visibility = System.Windows.Visibility.Visible;
                        else
                            imgSourceStatus.Visibility = System.Windows.Visibility.Collapsed;

                        if ((cmbSourceSwitcher.SelectedItem as ComboBoxItem).Content.ToString() == "Remote")
                        {
                            Properties.Settings.Default.SourceLocation = txtSourceLocation.Text.Trim();
                            Properties.Settings.Default.SourceAccount = txtSourceAccount.Text.Trim();
                            Properties.Settings.Default.SourcePassword = pwdSource.Password.Trim();
                        }
                        else
                        {
                            Properties.Settings.Default.SourceLocation = "";
                            Properties.Settings.Default.SourceAccount = "";
                            Properties.Settings.Default.SourcePassword = "";
                        }
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        string uri = String.Format(@"pack://application:,,,/DbBackuper;component/Images/error.png");
                        imgSourceStatus.Source = new BitmapImage(new Uri(uri));
                        imgSourceStatus.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
            }
        }
        #endregion
       
        #endregion
       

        #region Private DRY Method
        
        private List<string> LoadDatabases(string connstring)
        {
            List<string> databases = new List<string>();
            databases.Add("--- Please Select ---");

            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(connstring))
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
        private void LoadTables(string connstring)
        {
            using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(connstring))
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
        private void RunValidateConnection(string connstring, string from)
        {
            if (!String.IsNullOrEmpty(connstring))
            {
                BackgroundWorker bgw = new BackgroundWorker();
                bgw.DoWork += bgwValidateConnection_DoWorkHandler;
                bgw.RunWorkerCompleted += bgwValidateConnection_RunWorkerCompleted;
                bgw.WorkerReportsProgress = true;
                // [0]: connection string [1]: target/source
                List<string> args = new List<string>();
                args.Add(connstring);
                args.Add(from);
                bgw.RunWorkerAsync(args);
            }
            else
            {
                MessageBox.Show("Connection Information can't be empty");
            }
        }
        private void DealSourceConnectionString()
        {
            switch ((cmbSourceSwitcher.SelectedItem as ComboBoxItem).Content.ToString())
            {
                case "Local":
                    this._source_connstring = LOCAL_CONNSTRING_NO_DATABASE;
                    break;
                case "Remote":
                    string format = REMOTE_FORMAT_NO_DATABASE;
                    // TODO: Validate here.
                    string server = txtSourceLocation.Text.Trim();
                    string account = txtSourceAccount.Text.Trim();
                    string pwd = pwdSource.Password;
                    this._source_connstring = string.Format(format, server, account, pwd);
                    break;
            }
        }
        private void DealTargetConnectionString()
        {
            switch ((cmbTargetSwitcher.SelectedItem as ComboBoxItem).Content.ToString())
            {
                case "Local":
                    this._target_connstring = LOCAL_CONNSTRING_NO_DATABASE;
                    break;
                case "Remote":
                    string format = REMOTE_FORMAT_NO_DATABASE;
                    // TODO: Validate here.
                    string server = txtTargetLocation.Text.Trim();
                    string account = txtTargetAccount.Text.Trim();
                    string pwd = pwdTarget.Password;
                    this._target_connstring = string.Format(format, server, account, pwd);
                    break;
            }
        }
        private void RunBackup()
        { 
            //Step1.  Get All Table Sechma
            //Smo.Server server = new Smo.Server(@"(localdb)\v11.0");
            //Smo.Database database = new Smo.Database();
            //database = server.Databases["TEST"];
            //Smo.Table table = database.Tables["Categories", @"PPP"];

            Smo.Server srv = new Smo.Server();
            // really you would get these from config or elsewhere:
            //srv.ConnectionContext.Login = "foo";
            //srv.ConnectionContext.Password = "bar";
            srv.ConnectionContext.ServerInstance = @"(localdb)\v11.0";
            string dbName = "TEST";

            Smo.Database db = new Smo.Database();
            db = srv.Databases[dbName];

            StringBuilder sb = new StringBuilder();

            foreach (Smo.Table tbl in db.Tables)
            {
                Smo.ScriptingOptions options = new Smo.ScriptingOptions();
                options.ClusteredIndexes = true;
                options.Default = true;
                options.DriAll = true;
                options.Indexes = true;
                options.IncludeHeaders = true;

                StringCollection coll = tbl.Script(options);
                foreach (string str in coll)
                {
                    sb.Append(str);
                    sb.Append(Environment.NewLine);
                }
            }
            System.IO.StreamWriter fs = System.IO.File.CreateText("c:\\temp\\output.txt");
            fs.Write(sb.ToString());
            fs.Close();

            //Step2.  Build Table & Save FK Table 
            //Step3.  SQL Statement



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
        public struct ConnectionStatus
        {
            public bool IsConnected;
            public string Whois;
        }
        #endregion

       

        

        

       
    }
    
    
}
