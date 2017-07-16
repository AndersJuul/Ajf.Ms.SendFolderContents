using System;
using System.Collections.Generic;

namespace SendFolderContents
{
    public interface IAppSettings
    {
        IEnumerable<IJob> Jobs { get; set; }
        int[] Hours { get; set; }
        int[] Minutes { get; set; }
        TimeSpan MinSleep { get; set; }
    }
}