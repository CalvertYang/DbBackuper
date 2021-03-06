﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DbBackuper
{
    public class ConnectionConfig
    {
        private const string IP_PATTEN = @"([2]([5][0-5]|[0-4][0-9])|[0-1]?[0-9]{1,2})(\.([2]([5][0-5]|[0-4][0-9])|[0-1]?[0-9]{1,2})){3}";
        private const string URL_PATTEN = @"([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,6}";
        private const string LOCAL_PATTEN = @"([l|L][o|O][c|C][a|A][l|L]|\([l|L][o|O][c|C][a|A][l|L][d|D][b|B]\)\\[\w._\\-]*)";
        private const string DB_PATTEN = @"^([A-Za-z]*)";
        private string _server;
        private string _userid;
        private string _pwd;
        private string _db;
        private bool _loginSecurity;

        public string Server
        {
            get { return _server; }
            set
            {
                Regex rexIp = new Regex(IP_PATTEN);
                Regex rexUrl = new Regex(URL_PATTEN);
                Regex rexLocal = new Regex(LOCAL_PATTEN);
                if (rexIp.IsMatch(value) || rexUrl.IsMatch(value) || rexLocal.IsMatch(value))
                {
                    _server = value;
                }
            }
        }
        public string UserId {
            get { return _userid; }
            set { if (!string.IsNullOrEmpty(value)) _userid = value; }
        }
        public string Password
        {
            get { return _pwd; }
            set { if(!string.IsNullOrEmpty(value)) _pwd = value; }
        }
        public bool LoginSecurity
        {
            get { return _loginSecurity; }
            set { if (value != null) _loginSecurity = value; }
        }
        public string Database
        {
            get { return _db; }
            set
            {
                Regex rexDb = new Regex(DB_PATTEN);
                if (rexDb.IsMatch(value))
                {
                    _db = value;
                }
            }
        }
        public ConnectionConfig()
        {
            this.Server = @"local";
            this.LoginSecurity = true;
        }

        public string ConnectionString()
        {
            string conn = "";
            // Local Connection string
            
            if (this.LoginSecurity)
            {
                if (String.IsNullOrEmpty(this.Server))
                {
                    this.Server = @"local";
                }

                if (string.IsNullOrEmpty(this.Database))
                {
                    
                    conn = string.Format("Server={0};Integrated Security=True;", this.Server);
                }
                else // When database has value
                {
                    conn = string.Format("Server={0};Integrated Security=True;Database={1};", this.Server, this.Database);
                }
            }
            else // Remote Connection string.
            {
                if (string.IsNullOrEmpty(this.Database))
                {
                    conn = string.Format("Server={0};User ID={1};Password={2};", this.Server, this.UserId, this.Password);
                }
                else
                {
                    conn = string.Format("Server={0};User ID={1};Password={2};Database={3};", this.Server, this.UserId, this.Password,this.Database);
                }
            }

            return conn;
        }

    }
}
