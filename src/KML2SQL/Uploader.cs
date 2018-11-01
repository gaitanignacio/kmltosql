using Kml2Sql.Mapping;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML2SQL
{
    public class Uploader
    {
        public IProgress<ProgressReoprt> OnProgressChange { get; set; }

        public Kml2SqlMapper Mapper { get; private set; }

        public Uploader(FileStream stream, Kml2SqlConfig configuration)
        {
            Mapper = new Kml2SqlMapper(stream, configuration);
        }

        public Uploader(string filePath, Kml2SqlConfig configuration)
        {
            using (var stream = File.OpenRead(filePath))
            {
                Mapper = new Kml2SqlMapper(stream, configuration);
            }
        }

        public Uploader(FileStream stream) : this(stream, null) { }

        public Uploader(string filePath) : this(filePath, null) { }

        public Uploader(FileStream stream, Kml2SqlConfig configuration, Progress<ProgressReoprt> onChange) 
            : this(stream, configuration)
        {
            OnProgressChange = onChange;
        }

        public Uploader(string fileStream, Kml2SqlConfig configuration, Progress<ProgressReoprt> onChange)
            : this(fileStream, configuration)
        {
            OnProgressChange = onChange;
        }

        public void Upload(string connectionString, bool dropExistingTable)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                Upload(conn, dropExistingTable);
            }
        }

        public void Upload(SqlConnection connection, bool dropExistingTable)
        {
            SqlCommand sqlCommand;
            try
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }
                if (dropExistingTable)
                {
                    ReportProgress("Dropping Table", 0);
                    try
                    {
                        DropTable(connection);
                    }
                    catch
                    {

                    }                    
                }
                ReportProgress("Creating Table", 0);
                CreateTable(connection);
                var mapFeatures = Mapper.GetMapFeatures().ToArray();
                for (var i = 0; i < mapFeatures.Length; i++)
                {
                    ReportProgress(GetProgressMessage(mapFeatures[i]), GetPercentage(i + 1, mapFeatures.Length));
                    sqlCommand = mapFeatures[i].GetInsertCommand();
                    sqlCommand.Connection = connection;
                    sqlCommand.ExecuteNonQuery();                
                }
                ReportProgress("Done!", 100);
            }
            catch (Exception ex)
            {
                if (OnProgressChange != null)
                {
                    OnProgressChange.Report(new ProgressReoprt() { Exception = ex });
                }
                throw;
            }

        }

        private static string GetProgressMessage(MapFeature mf)
        {
            var message = "Uploading Placemark " + mf.Id;
            if (mf.Name != mf.Id.ToString())
            {
                message += $" ({mf.Name})";
            }
            return message;
        }

        public string GetScript()
        {
            var sb = new StringBuilder();
            sb.Append(Mapper.GetCreateTableScript());
            sb.Append(Mapper.GetCombinedInsertCommands());
            if (OnProgressChange != null)
            {
                ReportProgress("Done!", 100);
            }
            return sb.ToString();
        }

        private static int GetPercentage(int current, int total)
        {
            var percentage = ((double)current / total) * 100;
            percentage = Math.Floor(percentage);
            return Math.Min((int)percentage, 99);
        }

        public void CreateTable(SqlConnection connection)
        {
            var tableCommand = Mapper.GetCreateTableCommand(connection);
            tableCommand.ExecuteNonQuery();
        }


        public void DropTable(SqlConnection connection)
        {
            string dropCommandString = String.Format("DROP TABLE {0};", Mapper.Configuration.TableName);
            var dropCommand = new SqlCommand(dropCommandString, connection);
            dropCommand.CommandType = System.Data.CommandType.Text;
            dropCommand.ExecuteNonQuery();    
        }

        private void ReportProgress(string message, int percentage)
        {
            if (OnProgressChange != null)
            {
                var report = new ProgressReoprt();
                report.Message = message;
                report.PercentDone = percentage;
                OnProgressChange.Report(report);
            }
        }

    }


    public class ProgressReoprt
    {
        public string Message { get; set; }
        public int PercentDone { get; set; }
        public Exception Exception { get; set; }
    }
}
