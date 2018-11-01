using System;
using System.Linq;
using System.Collections.Generic;
using KML2SQL.Updates;

namespace KML2SQL
{
    class Settings
    {
        public string KMLFileName { get; set; }
        public string ServerName { get; set; }
        public string Login { get; set; }
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string ShapeColumnName { get; set; }
        public bool Geography { get; set; }
        public bool SRIDEnabled { get; set; }
        public string SRID { get; set; }
        public bool UseIntegratedSecurity { get; set; }
        public bool FixBrokenPolygons { get; set; }
        public UpdateInfo UpdateInfo { get; set; }
    }
}
