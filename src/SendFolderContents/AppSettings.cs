using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Ajf.Nuget.Logging;

namespace SendFolderContents
{
    public class AppSettings :ServiceSettingsFromConfigFile, IAppSettings
    {
        public AppSettings()
        {
            Hours = ConfigurationManager.AppSettings["Hours"].Split(',').Select(x => Convert.ToInt32(x)).ToArray();
            Minutes = ConfigurationManager.AppSettings["Minutes"].Split(',').Select(x => Convert.ToInt32(x)).ToArray();

            MinSleep =TimeSpan.FromMinutes( Convert.ToInt32(ConfigurationManager.AppSettings["MinSleep"]));
            Jobs = ConfigurationManager
                .AppSettings["Jobs"]
                .Split('|')
                .Select(x =>
                {
                    var parts = x.Split(';');
                    return new Job
                    {
                        Path = parts[0],
                        Receiver = parts[1]
                    };
                }).ToArray();
        }

        public IEnumerable<IJob> Jobs { get; set; }
        public int[] Hours { get; set; }
        public int[] Minutes { get; set; }
        public TimeSpan MinSleep { get; set; }
    }
}