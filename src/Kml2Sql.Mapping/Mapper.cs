using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Kml2Sql.Mapping
{
    /// <summary>
    /// Builds SQL queries for inserting KML files into SQL.
    /// </summary>
    public class Kml2SqlMapper
    {
        /// <summary>
        /// Configuration settings for the Mapper.
        /// </summary>
        public Kml2SqlConfig Configuration { get; private set; } = new Kml2SqlConfig();
        private IEnumerable<MapFeature> _mapFeatures;

        public Kml2SqlMapper(Stream fileStream, Kml2SqlConfig configuration) : this(fileStream)
        {
            if (configuration != null)
            {
                Configuration = configuration;
            }           
        }

        public Kml2SqlMapper(Stream fileStream)
        {
            var kml = KMLParser.Parse(fileStream);
            _mapFeatures = GetMapFeatures(kml);
        }

        private IEnumerable<MapFeature> GetMapFeatures(Kml kml)
        {
            int id = 1;
            foreach (var placemark in kml.Flatten().OfType<Placemark>())
            {

                if (HasValidElement(placemark))
                {
                    MapFeature mapFeature = new MapFeature(placemark, id, Configuration);
                    yield return mapFeature;
                }
                id++;
            }
        }

        /// <summary>
        /// Get an Enumerable set of MapFeature objects
        /// </summary>
        /// <returns>Enumerable collection of MapFeature objects</returns>
        public IEnumerable<MapFeature> GetMapFeatures()
        {
            return _mapFeatures;
        }

        /// <summary>
        /// Get SQLCommand that will create a table for MapFeature objects. Column names are based on
        /// Placemark data and Configuratiohn settings.
        /// </summary>
        /// <param name="connection">The SqlConnection</param>
        /// <param name="transaction">Any sql Transactions</param>
        /// <returns>A SqlCommand</returns>
        public SqlCommand GetCreateTableCommand(SqlConnection connection, SqlTransaction transaction = null)
        {
            var command = GetCreateTableCommand();
            command.Connection = connection;
            command.Transaction = transaction;
            return command;
        }

        /// <summary>
        /// Get SQLCommand that will create a table for MapFeature objects. Column names are based on
        /// Placemark data and Configuratiohn settings.
        /// </summary>
        /// <returns>A SqlCommand</returns>
        public SqlCommand GetCreateTableCommand()
        {
            var commandText = GetCreateTableScript();
            var command = new SqlCommand(commandText);
            command.CommandType = System.Data.CommandType.Text;
            return command;
        }

        /// <summary>
        /// Get combination of all Mapfeature inserts.
        /// </summary>
        /// <returns>string</returns>
        public string GetCombinedInsertCommands()
        {
            var sb = new StringBuilder();
            var mapFeatures = GetMapFeatures().ToArray();
            for (var i = 0; i < mapFeatures.Length; i++)
            {
                sb.Append(Environment.NewLine);
                sb.Append(mapFeatures[i].GetInsertQuery(i == 0));
            }
            return sb.ToString();
        }


        /// <summary>
        /// Get SQL query that will create a table for MapFeature objects. Column names are based on
        /// Placemark data and Configuratiohn settings.
        /// </summary>
        /// <returns>SQL Script</returns>
        public string GetCreateTableScript()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("CREATE TABLE [{0}] (", Configuration.TableName));
            sb.Append($"[{Configuration.IdColumnName}] INT NOT NULL PRIMARY KEY,");
            sb.Append($"[{Configuration.NameColumnName}] VARCHAR(255),");
            foreach (var columnName in GetColumnNames().Select(Configuration.GetColumnName))
            {
                sb.Append(String.Format("[{0}] VARCHAR(max), ", columnName));
            }
            sb.Append(String.Format("[{0}] [sys].[{1}] NOT NULL);", Configuration.PlacemarkColumnName, Configuration.GeoType));
            return sb.ToString();
        }

        private static bool HasValidElement(Placemark placemark)
        {
            return placemark.Flatten().Any(e => e is Point || e is LineString || e is Polygon);
        }

        private IEnumerable<string> GetColumnNames()
        {
            return _mapFeatures.SelectMany(x => x.Data.Keys).Distinct();
        }        
    }
}
