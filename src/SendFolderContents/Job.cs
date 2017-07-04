namespace SendFolderContents
{
    public class Job : IJob
    {
        public string Path { get; set; }
        public string Receiver { get; set; }
    }
}