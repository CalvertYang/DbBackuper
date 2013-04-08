using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbBackuper
{
    public class SechmaModel
    {
        public string TableName { get; set; }
        public string Scripts { get; set; }
        public List<ForeignKey> Foreignkeys { get; set; }
        public string IndexScripts { get; set; }
        public bool IsExistsOnTarget { get; set; }
    }

    public class ForeignKey
    {
        public string Table { get; set; }
        public string Column { get; set; }
    }
}
