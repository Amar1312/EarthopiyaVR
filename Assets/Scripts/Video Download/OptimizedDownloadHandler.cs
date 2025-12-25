using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class OptimizedDownloadHandler : DownloadHandlerScript
{
    private FileStream fileStream;
    private long totalBytes = 0;
    private long downloadedBytes = 0;

    public OptimizedDownloadHandler(string path, bool append = false) : base()
    {
        // Use FileOptions.WriteThrough and larger buffer for better performance
        fileStream = new FileStream(
            path,
            append ? FileMode.Append : FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920, // 80KB buffer
            useAsync: true
        );
    }

    protected override void ReceiveContentLengthHeader(ulong contentLength)
    {
        totalBytes = (long)contentLength;
    }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || dataLength == 0)
            return false;

        fileStream.Write(data, 0, dataLength);
        downloadedBytes += dataLength;

        return true;
    }

    protected override void CompleteContent()
    {
        fileStream?.Close();
        fileStream?.Dispose();
    }

    public float GetProgress()
    {
        if (totalBytes <= 0) return 0;
        return (float)downloadedBytes / totalBytes;
    }
}