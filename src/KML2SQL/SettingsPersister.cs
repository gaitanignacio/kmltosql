using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;

namespace KML2SQL
{
    static class SettingsPersister
    {
        private static object _sync = new object();
        private static string FileName = Path.Combine(Utility.GetApplicationFolder(), "KML2SQL.settings");

        public static void Persist(Settings settings)
        {
            lock (_sync)
            {
                var settingsText = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
                File.WriteAllText(FileName, settingsText);
            }            
        }
        public static Settings Retrieve()
        {
            lock (_sync)
            {
                if (File.Exists(FileName))
                {
                    try
                    {
                        var settingsText = File.ReadAllText(FileName);
                        var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(settingsText);
                        return settings;
                    }
                    catch
                    {
                        File.Delete(FileName);
                    }                    
                }
                return null;
            }
        }

    }
}