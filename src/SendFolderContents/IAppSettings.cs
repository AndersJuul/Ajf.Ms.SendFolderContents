using System;
using System.Collections.Generic;

namespace SendFolderContents
{
    public interface IAppSettings
    {
        string Revision { get; set; }
        string Description { get; set; }
        IEnumerable<IJob> Jobs { get; set; }
        int[] Hours { get; set; }
        int[] Minutes { get; set; }
        TimeSpan MinSleep { get; set; }
    }
}