namespace SendFolderContents
{
    public interface IJob
    {
        string Path { get; set; }
        string Receiver { get; set; }
    }
}