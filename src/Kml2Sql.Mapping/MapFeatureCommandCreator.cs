using SharpKml.Base;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kml2Sql.Mapping
{
    internal static class MapFeatureCommandCreator
    {
        internal static SqlCommand CreateCommand(MapFeature mapFeature, Kml2SqlConfig config)
        {
            var query = CreateCommandQuery(mapFeature, config);
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Id", mapFeature.Id);
            sqlCommand.Parameters.AddWithValue("@Name", GetNameParam(mapFeature));
            foreach (KeyValuePair<string, string> simpleData in mapFeature.Data)
            {
                sqlCommand.Parameters.AddWithValue("@" + config.GetColumnName(simpleData.Key), simpleData.Value);
            }
            return sqlCommand;
        }

        private static object GetNameParam(MapFeature mapFeature)
        {
            if (!String.IsNullOrWhiteSpace(mapFeature.Name))
            {
                return mapFeature.Name;
            }
            return DBNull.Value;
        }

        internal static string CreateCommandQuery(
            MapFeature mapFeature, 
            Kml2SqlConfig config, 
            bool useParameters = true, 
            bool declareVariables = true)
        {
            var columnNames = mapFeature.Data.Keys.Select(x => config.GetColumnName(x)).ToArray();
            string columnText = GetColumnText(columnNames);
            string parameters = GetParameters(mapFeature, useParameters, columnNames);
            StringBuilder query = new StringBuilder();
            query.Append(ParseCoordinates(mapFeature, config, declareVariables));
            query.Append(string.Format($"INSERT INTO {config.TableName}({config.IdColumnName}, {config.NameColumnName}, {columnText} {config.PlacemarkColumnName})"));
            query.Append(Environment.NewLine);
            query.Append($"VALUES({parameters} @placemark);");
            return query.ToString();
        }

        private static string GetColumnText(string[] columnNames)
        {
            string columnText = "";
            if (columnNames.Length > 0)
            {
                columnText = string.Join(", ", columnNames) + ",";
            }
            return columnText;
        }

        private static string GetParameters(MapFeature mapFeature, bool useParameters, string[] columnNames)
        {
            string parameters;
            if (useParameters)
            {
                var joined = string.Join(", ", columnNames.Select(x => "@" + x));
                parameters = $"@Id, @Name, {joined}";
                if (columnNames.Length > 0)
                {
                    parameters += ", ";
                }
            }
            else
            {
                var joinedData = string.Join(", ", mapFeature.Data.Values.Select(x=> "'" + x.Replace("'","") + "'"));
                parameters = $"{mapFeature.Id}, '{mapFeature.Name.Replace("'","''")}', {joinedData}";
                if (mapFeature.Data.Values.Count > 0)
                {
                    parameters += ", ";
                }
            }
            return parameters;
        }

        private static string ParseCoordinates(MapFeature mapFeature, Kml2SqlConfig config, bool declareVariables)
        {
            StringBuilder commandString = new StringBuilder();
            if (config.GeoType == PolygonType.Geography)
            {
                commandString.Append(ParseCoordinatesGeography(mapFeature, config, declareVariables));
                if (declareVariables)
                {
                    commandString.Append("DECLARE @placemark geography;" + Environment.NewLine);
                }                
                commandString.Append("SET @placemark = @validGeo;" + Environment.NewLine);
            }
            else
            {
                commandString.Append(ParseCoordinatesGeometry(mapFeature, config, declareVariables));
                if (declareVariables)
                {
                    commandString.Append("DECLARE @placemark geometry;" + Environment.NewLine);
                }                
                commandString.Append("SET @placemark = @validGeom;" + Environment.NewLine);
            }
            return commandString.ToString();
        }

        private static string ParseCoordinatesGeometry(MapFeature mapFeature, Kml2SqlConfig config, bool declareVariables)
        {
            switch (mapFeature.ShapeType)
            {
                case ShapeType.Polygon: return CreatePolygon(mapFeature, config, declareVariables);
                case ShapeType.LineString: return CreateLineString(mapFeature, config, declareVariables);
                case ShapeType.Point: return CreatePoint(mapFeature, config, declareVariables);
                default: throw new Exception("Unsupported shape type!");
            }
        }

        private static string CreatePoint(MapFeature mapFeature, Kml2SqlConfig config, bool declareVariables)
        {
            var sb = new StringBuilder();
            if (declareVariables)
            {
                sb.Append(@"DECLARE @validGeom geometry;" + Environment.NewLine);
            }            
            sb.Append("SET @validGeom = geometry::STPointFromText('POINT (");
            sb.Append(mapFeature.Coordinates[0].Longitude + " " + mapFeature.Coordinates[0].Latitude);
            sb.Append(@")', " + config.Srid + @");" + Environment.NewLine);
            return sb.ToString();
        }

        private static string CreateLineString(MapFeature mapFeature, Kml2SqlConfig config, bool declareVariables)
        {
            var sb = new StringBuilder();
            if (declareVariables)
            {
                sb.Append("DECLARE @validGeom geometry;" + Environment.NewLine);
            }            
            sb.Append("SET @validGeom = geometry::STLineFromText('LINESTRING (");
            foreach (Vector coordinate in mapFeature.Coordinates)
            {
                sb.Append(coordinate.Longitude + " " + coordinate.Latitude + ", ");
            }
            sb.Remove(sb.Length - 2, 2).ToString();
            sb.Append(@")', " + config.Srid + @");");
            return sb.ToString();
        }

        private static string CreatePolygon(MapFeature mapFeature, Kml2SqlConfig config, bool declareVariables)
        {
            var sb = new StringBuilder();
            if (declareVariables)
            {
                sb.Append("DECLARE @geom geometry;" + Environment.NewLine);
            }            
            sb.Append("SET @geom = geometry::STPolyFromText('POLYGON((");
            sb.Append(GetOuterRingSql(mapFeature.Coordinates, config));
            foreach (Vector[] innerCoordinates in mapFeature.InnerCoordinates)
            {
                sb.Append(GetInnerRingSql(innerCoordinates, config));
            }
            sb.Append(@"))', " + config.Srid + @").MakeValid();" + Environment.NewLine);
            if (declareVariables)
            {
                sb.Append("DECLARE @validGeom geometry;" + Environment.NewLine);
            }            
            sb.Append("SET @validGeom = @geom.MakeValid().STUnion(@geom.STStartPoint());");
            return sb.ToString();
        }

        private static string GetOuterRingSql(Vector[] coordinates, Kml2SqlConfig config)
        {
            List<string> outerCoordSql = coordinates.Select(GetVectorSql).ToList();
            if (config.FixPolygons && RingInvalid(coordinates))
            {
                outerCoordSql.Add(GetVectorSql(coordinates[0]));
            }
            var joined = string.Join(", ", outerCoordSql);
            return joined;
        }

        private static string GetInnerRingSql(Vector[] innerCoordinates, Kml2SqlConfig config)
        {
            var sb = new StringBuilder();
            sb.Append("), (");
            var coordSql = innerCoordinates.Select(GetVectorSql).ToList();
            if (config.FixPolygons && RingInvalid(innerCoordinates))
            {
                coordSql.Add(GetVectorSql(innerCoordinates[0]));
            }
            sb.Append(string.Join(", ", coordSql));
            return sb.ToString();
        }

        public static string GetVectorSql(Vector coordinate)
        {
            return $"{coordinate.Longitude} {coordinate.Latitude}";
        }

        private static bool RingInvalid(Vector[] coordinates)
        {
            return coordinates.First().Latitude != coordinates.Last().Latitude ||
                coordinates.First().Longitude != coordinates.Last().Longitude;
        }

        private static string ParseCoordinatesGeography(MapFeature mapFeature, Kml2SqlConfig config, bool declare)
        {
            StringBuilder commandString = new StringBuilder();
            commandString.Append(ParseCoordinatesGeometry(mapFeature, config, declare));
            if (declare)
            {
                commandString.Append("DECLARE @validGeo geography;");
            }            
            commandString.Append("SET @validGeo = geography::STGeomFromText(@validGeom.STAsText(), " + config.Srid + @").MakeValid();");
            return commandString.ToString();
        }
    }
}
