using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kml2Sql.Mapping
{
    /// <summary>
    /// Configuration file for generating Map Features
    /// </summary>
    public class Kml2SqlConfig
    {
        /// <summary>
        /// Table name that Placemarks will be uploaded to. Default is "KmlUpload".
        /// </summary>
        public string TableName { get; set; } = "KmlUpload";
        /// <summary>
        /// Name of the column that SqlGeography or SqlGeometry object will be stored in. Default is "Placemark".
        /// </summary>
        public string PlacemarkColumnName { get; set; } = "Placemark";
        /// <summary>
        /// Column ID name. Default is "Id".
        /// </summary>
        public string IdColumnName { get; set; } = "Id";
        /// <summary>
        /// Name of column with placemark name. Default is "Name".
        /// </summary>
        public string NameColumnName { get; set; } = "Name";
        /// <summary>
        /// Sql Polygon type that will be used. Default is "Geometry".
        /// </summary>
        public PolygonType GeoType { get; set; }
        /// <summary>
        /// Close ring of any open polygons.
        /// </summary>
        public bool FixPolygons { get; set; }
        /// <summary>
        /// Geography SRID. Not used for is PolygonType is set to Gemetry. Default is 4326.
        /// </summary>
        public int Srid { get; set; } = 4326;

        private Dictionary<string, string> ColumnNameMap = new Dictionary<string, string>();

        /// <summary>
        /// Change SQL column name of placemark data.
        /// </summary>
        /// <param name="placemarkName">Name of data from Placemark file.</param>
        /// <param name="columnName">Column name in SQL</param>
        public void MapColumnName(string placemarkName, string columnName)
        {
            ColumnNameMap.Add(placemarkName.ToLower(), columnName);
        }

        internal string GetColumnName(string placemarkName)
        {
            var key = placemarkName.ToLower();
            if (ColumnNameMap.ContainsKey(key))
            {
                return ColumnNameMap[key];
            }
            return placemarkName;
        }
    }
}
