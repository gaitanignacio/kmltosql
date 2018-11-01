using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Net.Http.Headers;
using System.Windows;
using System.Diagnostics;
using Semver;

namespace KML2SQL.Updates
{
    public static class UpdateChecker
    {
        static readonly string apiUrl = "https://api.github.com/repos/pharylon/kml2sql/releases/latest";
        static readonly string downloadUrl = "https://github.com/Pharylon/KML2SQL/releases/latest";
        public static async Task CheckForNewVersion()
        {
            var settings = SettingsPersister.Retrieve();
            if (CheckForUpdates(settings))
            {
                settings.UpdateInfo.LastCheckedForUpdates = DateTime.Now;
                var client = new HttpClient();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Add("User-Agent", "Pharylon");
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var latestVersion = await GetLatestVersion(response);
                    var thisVersion = SemVersion.Parse(GetCurrentVersion());
                    if (latestVersion > thisVersion && ShouldNag(settings, latestVersion))
                    {
                        settings.UpdateInfo.LastTimeNagged = DateTime.Now;
                        var mbResult = MessageBox.Show(
                            "A new version is availbe. Press 'Yes' to go to the download page, Cancel to skip, or 'No' to not be reminded unless an even newer version comes out.",
                            "New Version Available!",
                            MessageBoxButton.YesNoCancel);
                        if (mbResult == MessageBoxResult.Yes)
                        {
                            Process.Start(downloadUrl);
                        }
                        if (mbResult == MessageBoxResult.No)
                        {
                            settings.UpdateInfo.DontNag = true;
                        }
                    }
                }
                SettingsPersister.Persist(settings);
            }
        }

        private static async Task<SemVersion> GetLatestVersion(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);
            var latestVersionString = (string)obj["tag_name"];
            var latestVersion = SemVersion.Parse(latestVersionString);
            return latestVersion;
        }

        private static bool ShouldNag(Settings settings, SemVersion latestVersion)
        {
            bool shouldNag = false;
            var lastVersionSeen = SemVersion.Parse(settings.UpdateInfo.LastVersionSeen ?? "0.1");
            if (latestVersion > lastVersionSeen)
            {
                shouldNag = true;
                settings.UpdateInfo.LastVersionSeen = latestVersion.ToString();
            }
            return shouldNag;
        }

        private static bool CheckForUpdates(Settings settings)
        {
            if (settings == null)
            {
                return false;
            }
            if (settings.UpdateInfo == null)
            {
                settings.UpdateInfo = new UpdateInfo();
            }
            var check = 
                    settings.UpdateInfo.LastCheckedForUpdates < DateTime.Now.AddDays(-1) &&
                    settings.UpdateInfo.LastTimeNagged < DateTime.Now.AddDays(-7);
            return check;
        }

        internal static string GetCurrentVersion()
        {
            var attr = Assembly
                .GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                as AssemblyInformationalVersionAttribute[];
            return attr.First().InformationalVersion;
        }
    }
}
