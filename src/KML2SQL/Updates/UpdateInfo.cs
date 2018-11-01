using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML2SQL.Updates
{
    class UpdateInfo
    {
        public DateTime LastCheckedForUpdates { get; set; }
        public DateTime LastTimeNagged { get; set; }
        public bool DontNag { get; set; }
        public string LastVersionSeen { get; set; }
    }
}
