using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.Data.SqlClient;

namespace Kml2Sql.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public class MapFeature
    {
        #region Properties
        /// <summary>
        /// Original SharpKML representation of placemark.
        /// </summary>
        public Placemark Placemark { get; private set; }

        /// <summary>
        /// Sql Id
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Placemar Name property, or else Id
        /// </summary>
        public string Name => Placemark?.Name;

        /// <summary>
        /// Coordinates of polygon, point, or string.
        /// </summary>
        public Vector[] Coordinates { get; private set; }

        /// <summary>
        /// Inner coordinates, if any. Only used for Polygons.
        /// </summary>
        public Vector[][] InnerCoordinates { get; private set; }

        /// <summary>
        /// Additinal Placemark data that will be entered into SQL.
        /// </summary>
        public Dictionary<string, string> Data { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Type of Placemark shape.
        /// </summary>
        public ShapeType ShapeType { get; private set; }
        #endregion

        private Kml2SqlConfig _configuration;

        internal MapFeature(Placemark placemark, int id, Kml2SqlConfig config)
        {
            Placemark = placemark;
            Id = id;
            SetGeoTypes(placemark);
            InitializeCoordinates(placemark);
            InitializeData(placemark);
            _configuration = config;
        }

        private void SetGeoTypes(Placemark placemark)
        {
            foreach (var element in placemark.Flatten())
            {
                if (element is Point)
                {
                    ShapeType = ShapeType.Point;
                }
                else if (element is Polygon)
                {
                    ShapeType = ShapeType.Polygon;
                }
                else if (element is LineString)
                {
                    ShapeType = ShapeType.LineString;
                }
            }
        }


        private void InitializeCoordinates(Placemark placemark)
        {
            switch (this.ShapeType)
            {
                case ShapeType.LineString:
                    Coordinates = InitializeLineCoordinates(placemark);
                    break;
                case ShapeType.Point:
                    Coordinates = InitializePointCoordinates(placemark);
                    break;
                case ShapeType.Polygon:
                    Vector[][] coords = InitializePolygonCoordinates(placemark);
                    Coordinates = coords[0];
                    if (coords.Length > 1)
                    {
                        InnerCoordinates = coords.Skip(1).ToArray();
                    }
                    else
                    {
                        InnerCoordinates = new Vector[0][];
                    }
                    break;
            }
        }

        private void InitializeData(Placemark placemark)
        {
            foreach (SimpleData sd in placemark.Flatten().OfType<SimpleData>())
            {
                if (sd.Name.ToLower() == "id")
                {
                    sd.Name = "placemark_sd_id";
                }
                if (sd.Name.ToLower() == "name")
                {
                    sd.Name = "placemark_sd_name";
                }
                Data.Add(sd.Name.Sanitize(), sd.Text.Sanitize());
            }
            foreach (Data data in placemark.Flatten().OfType<Data>())
            {
                if (data.Name.ToLower() == "id")
                {
                    data.Name = "placemark_data_id";
                }
                if (data.Name.ToLower() == "name")
                {
                    data.Name = "placemark_data_name";
                }
                Data.Add(data.Name.Sanitize(), data.Value.Sanitize());
            }
        }

        private static Vector[] InitializePointCoordinates(Placemark placemark)
        {
            List<Vector> coordinates = new List<Vector>();
            foreach (var point in placemark.Flatten().OfType<Point>())
            {
                Vector myVector = new Vector();
                myVector.Latitude = point.Coordinate.Latitude;
                myVector.Longitude = point.Coordinate.Longitude;
                coordinates.Add(myVector);
            }
            return coordinates.ToArray();
        }

        private static Vector[] InitializeLineCoordinates(Placemark placemark)
        {
            List<Vector> coordinates = new List<Vector>();
            foreach (LineString element in placemark.Flatten().OfType<LineString>())
            {
                LineString lineString = element;
                coordinates.AddRange(lineString.Coordinates);
            }
            return coordinates.ToArray();
        }

        private static Vector[][] InitializePolygonCoordinates(Placemark placemark)
        {
            List<List<Vector>> coordinates = new List<List<Vector>>();
            coordinates.Add(new List<Vector>());

            foreach (var polygon in placemark.Flatten().OfType<Polygon>())
            {
                coordinates[0].AddRange(polygon.OuterBoundary.LinearRing.Coordinates);
                coordinates.AddRange(polygon.InnerBoundary.Select(inner => inner.LinearRing.Coordinates.ToList()));
            }
            return coordinates.Select(c => c.ToArray()).ToArray();
        }

        internal void ReverseRingOrientation()
        {
            List<Vector> reversedCoordinates = new List<Vector>();
            for (int i = Coordinates.Length - 1; i >= 0; i--)
            {
                reversedCoordinates.Add(Coordinates[i]);
            }
            Coordinates = reversedCoordinates.ToArray();
        }

        public override string ToString()
        {
            return Name + " " + Id + " - " + ShapeType;
        }

        /// <summary>
        /// Create a SqlCommand that, when executed, will insert this object into SQL.
        /// </summary>
        /// <returns>SqlCommand</returns>
        public SqlCommand GetInsertCommand()
        {
            return MapFeatureCommandCreator.CreateCommand(this, _configuration);
        }


        internal string GetInsertQuery(bool declareVariables = true)
        {
            return MapFeatureCommandCreator.CreateCommandQuery(this, _configuration, false, declareVariables);
        }

        /// <summary>
        /// Get an insert statement that, when exectued, will insert this object into SQL. Can be used
        /// when saving text files of SQL Commands, but should be careful as commands will not be parameterized.
        /// </summary>
        /// <returns>Insert Query</returns>
        public string GetInsertQuery()
        {
            return MapFeatureCommandCreator.CreateCommandQuery(this, _configuration, false, true);
        }
    }
}
