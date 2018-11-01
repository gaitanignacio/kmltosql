using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML2SQL;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.Diagnostics;
using System.Text;
using System.Configuration;
using Kml2Sql.Mapping;
using System.Data.SqlClient;
using System.Linq;

namespace KML2SQLTests
{
    [TestClass]
    public class Tests
    {
        //====================================================================================================
        //
        // Yes, I know these aren't real tests. Sorry. I wasn't really doing TDD at the time I wrote this.
        //
        //===================================================================================================

        string _tablePrefix = "Kml2SqlTest";
        string connectionString;

        [TestInitialize]
        public void InitializeTests()
        {
            connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ToString();
        }

        private void Upload(string fileName, string tableName, PolygonType geoType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Kml2SqlConfig config = GetConfig(tableName, geoType);
                var uploader = new Uploader(fileName, config);
                uploader.Upload(connection, true);
            }

        }

        private Kml2SqlConfig GetConfig(string tableName, PolygonType geoType)
        {
            return new Kml2SqlConfig()
            {
                GeoType = geoType,
                TableName = _tablePrefix + tableName,
                FixPolygons = true,
            };
        }

        private void Upload(string fileName, Kml2SqlConfig config)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var uploader = new Uploader(fileName, config);
                uploader.Upload(connection, true);
            }

        }

        [TestMethod]
        public void CheckNPA()
        {
            Upload(@"TestData\npa.kml", "NPA", PolygonType.Geography);
        }

        [TestMethod]
        public void TestGetScript()
        {
            using (var stream = File.OpenRead(@"TestData\npa.kml"))
            {
                var mapper = new Kml2SqlMapper(stream);
                mapper.Configuration.TableName = _tablePrefix + "NPA";
                mapper.Configuration.GeoType = PolygonType.Geography;
                var places = mapper.GetMapFeatures();
                var query = places.First().GetInsertQuery();
                Assert.IsNotNull(query);
                Assert.IsTrue(query.Length > 0);
            }
        }

        [TestMethod]
        public void BasicKML()
        {
            Upload( @"TestData\Basic.kml", "Basic", PolygonType.Geography);
        }

        [TestMethod]
        public void ConfigChanges()
        {
            var config = GetConfig("BasicConfig", PolygonType.Geometry);
            config.IdColumnName = "MyId";
            config.NameColumnName = "MyName";
            Upload(@"TestData\Basic.kml", config);
        }

        [TestMethod]
        public void BasicKMLGeometry()
        {
            Upload(@"TestData\Basic.kml", "BasicGeom", PolygonType.Geometry);
        }

        [TestMethod]
        public void CheckNPAGeometry()
        {
            Upload(@"TestData\npa.kml", "NPAGeom", PolygonType.Geometry);
        }

        [TestMethod]
        public void SchoolTest()
        {
            Upload( @"TestData\school.kml", "School", PolygonType.Geography);
        }

        [TestMethod]
        public void SchoolTestGeometry()
        {
            Upload(@"TestData\school.kml", "SchoolGeom", PolygonType.Geometry);
        }

        [TestMethod]
        public void GoogleSample()
        {
            Upload(@"TestData\KML_Samples.kml", "Google", PolygonType.Geometry);
        }

        [TestMethod]
        public void SenicLandmarks()
        {
            Upload(@"TestData\Scenic_Landmarks.kml", "ScenicLandmarks", PolygonType.Geometry);
        }

        //[TestMethod]
        //public void UsZips()
        //{
        //    myUploader.Upload(@"TestData\us_zips.kml", "Zips", 4326, GeoType.Geometry, true);
        //}

        //[TestMethod]
        //public void BasicKmlOnMySql()
        //{
        //    myUploader = new MapUploader("192.168.0.202", "test", "root", passwordList[1], "placemark", @"TestData\Basic.kml", myInfo.Table, 4326, true);
        //    myUploader.Upload();
        //}
    }
}
