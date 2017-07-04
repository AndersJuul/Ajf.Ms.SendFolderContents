using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SendFolderContents
{
    public class AppSettings : IAppSettings
    {
        public AppSettings()
        {
            Revision = ConfigurationManager.AppSettings["Revision"];
            Description = ConfigurationManager.AppSettings["Description"];
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

        public string Revision { get; set; }
        public string Description { get; set; }
        public IEnumerable<IJob> Jobs { get; set; }
        public int[] Hours { get; set; }
        public int[] Minutes { get; set; }
        public TimeSpan MinSleep { get; set; }
    }
}