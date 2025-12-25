using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ChunkedVideoDownloader : MonoBehaviour
{
    // Add this method to get file size
    private async Task<long> GetFileSize(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Head(url))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to get file size: {request.error}");
                return 0;
            }

            // Try to get Content-Length header
            string contentLength = request.GetResponseHeader("Content-Length");
            if (long.TryParse(contentLength, out long fileSize))
            {
                return fileSize;
            }

            Debug.LogWarning("Could not get Content-Length header, trying alternative method...");

            // Alternative: Try with GET request and Range header
            return await GetFileSizeWithRangeRequest(url);
        }
    }

    private async Task<long> GetFileSizeWithRangeRequest(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Range", "bytes=0-0"); // Request just first byte

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to get file size via Range: {request.error}");
                return 0;
            }

            // Get Content-Range header (e.g., "bytes 0-0/500000000")
            string contentRange = request.GetResponseHeader("Content-Range");
            if (!string.IsNullOrEmpty(contentRange))
            {
                var parts = contentRange.Split('/');
                if (parts.Length > 1 && long.TryParse(parts[1], out long totalSize))
                {
                    return totalSize;
                }
            }

            return 0;
        }
    }

    private class DownloadChunk
    {
        public int chunkIndex;
        public long startByte;
        public long endByte;
        public byte[] data;
    }

    public async Task DownloadVideoChunked(string url, string savePath, int chunkCount = 8)
    {
        // Get file size first
        long fileSize = await GetFileSize(url);

        if (fileSize <= 0)
        {
            Debug.LogError("Failed to get file size. Cannot download chunked.");
            return;
        }

        long chunkSize = fileSize / chunkCount;

        // Create all chunk tasks
        var chunkTasks = new List<Task<DownloadChunk>>();

        for (int i = 0; i < chunkCount; i++)
        {
            long start = i * chunkSize;
            long end = (i == chunkCount - 1) ? fileSize - 1 : (start + chunkSize - 1);

            chunkTasks.Add(DownloadChunkAsync(url, i, start, end));
        }

        // Wait for all chunks
        var chunks = await Task.WhenAll(chunkTasks);

        // Combine chunks
        await CombineChunks(chunks.OrderBy(c => c.chunkIndex).ToArray(), savePath);

        Debug.Log($"Chunked download complete: {Path.GetFileName(savePath)}");
    }

    private async Task<DownloadChunk> DownloadChunkAsync(string url, int index, long start, long end)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Range", $"bytes={start}-{end}");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Chunk {index} download failed: {request.error}");
                return null;
            }

            return new DownloadChunk
            {
                chunkIndex = index,
                startByte = start,
                endByte = end,
                data = request.downloadHandler.data
            };
        }
    }

    private async Task CombineChunks(DownloadChunk[] chunks, string savePath)
    {
        // Filter out null chunks (failed downloads)
        var validChunks = chunks.Where(c => c != null && c.data != null).ToArray();

        if (validChunks.Length == 0)
        {
            Debug.LogError("No valid chunks to combine");
            return;
        }

        using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
        {
            foreach (var chunk in validChunks)
            {
                await fileStream.WriteAsync(chunk.data, 0, chunk.data.Length);
            }
        }

        Debug.Log($"Combined {validChunks.Length} chunks into {Path.GetFileName(savePath)}");
    }

    // Helper method to use chunked downloading
    public async Task<bool> DownloadVideoParallelChunked(string url, string savePath)
    {
        try
        {
            await DownloadVideoChunked(url, savePath, 4); // Use 4 chunks
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Chunked download failed: {e.Message}");
            return false;
        }
    }
}