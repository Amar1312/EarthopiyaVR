using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Security.Cryptography;
using System;

public class VideoDownloadManager : MonoBehaviour
{
    [System.Serializable]
    public class VideoDownloadItem
    {
        public string url;
        public string savePath;
        public string fileName;
        public bool isDownloaded;
        public bool isVerified;
        public float progress;
        public string status;
        public int retryCount;
        public long expectedFileSize; // Optional: Server should provide this
        public string expectedHash; // Optional: For integrity check
    }

    public List<VideoDownloadItem> downloadQueue = new List<VideoDownloadItem>();
    private int maxConcurrentDownloads = 3;
    private int maxRetryAttempts = 3;
    private List<Task> activeDownloadTasks = new List<Task>();

    void Start()
    {
        InitializeDownloadQueue();
        StartParallelDownloads();
    }

    void InitializeDownloadQueue()
    {
        for (int i = 1; i <= 6; i++)
        {
            string fileName = $"video_{i}.mp4";
            string savePath = Path.Combine(Application.persistentDataPath, fileName);

            downloadQueue.Add(new VideoDownloadItem
            {
                url = "https://myanimaltransport.com/storage/app/public/200MB.mp4",
                savePath = savePath,
                fileName = fileName,
                isDownloaded = false,
                isVerified = false,
                progress = 0f,
                status = "Pending",
                retryCount = 0,
                expectedFileSize = 0, // Set this if you know file sizes
                expectedHash = "" // Set this for hash verification
            });
        }
    }

    public async void StartParallelDownloads()
    {
        // First, check existing files for integrity
        await CheckExistingFiles();

        // Start downloading pending files
        var pendingDownloads = downloadQueue.Where(d => !d.isDownloaded || !d.isVerified).ToList();

        while (pendingDownloads.Count > 0)
        {
            var availableSlots = maxConcurrentDownloads - activeDownloadTasks.Count;
            var toDownload = pendingDownloads.Take(availableSlots).ToList();

            foreach (var item in toDownload)
            {
                if (item.retryCount >= maxRetryAttempts)
                {
                    item.status = $"Max retries ({maxRetryAttempts}) exceeded";
                    pendingDownloads.Remove(item);
                    continue;
                }

                item.status = "Downloading";
                var task = DownloadVideoWithResumeAndRetry(item);
                activeDownloadTasks.Add(task);
                pendingDownloads.Remove(item);
            }

            if (activeDownloadTasks.Count > 0)
            {
                await Task.WhenAny(activeDownloadTasks);
                activeDownloadTasks.RemoveAll(t => t.IsCompleted);
            }
            else
            {
                await Task.Yield();
            }

            pendingDownloads = downloadQueue.Where(d => !d.isDownloaded || !d.isVerified).ToList();
        }

        Debug.Log("All downloads completed and verified!");
    }

    private async Task CheckExistingFiles()
    {
        foreach (var item in downloadQueue)
        {
            if (File.Exists(item.savePath))
            {
                item.status = "Verifying existing file...";

                // Verify file integrity
                bool isFileValid = await VerifyFileIntegrity(item);

                if (isFileValid)
                {
                    item.isDownloaded = true;
                    item.isVerified = true;
                    item.progress = 1f;
                    item.status = "Already downloaded and verified";
                    Debug.Log($"File already exists and is valid: {item.fileName}");
                }
                else
                {
                    // File is corrupted, delete it
                    item.isDownloaded = false;
                    item.isVerified = false;
                    item.progress = 0f;
                    item.status = "File corrupted, will redownload";
                    Debug.LogWarning($"File corrupted, deleting: {item.fileName}");

                    try
                    {
                        File.Delete(item.savePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to delete corrupted file: {e.Message}");
                    }
                }
            }
        }
    }

    private async Task<bool> VerifyFileIntegrity(VideoDownloadItem item)
    {
        try
        {
            // Method 1: Check file size (if expected size is known)
            if (item.expectedFileSize > 0)
            {
                FileInfo fileInfo = new FileInfo(item.savePath);
                if (fileInfo.Length != item.expectedFileSize)
                {
                    Debug.LogWarning($"File size mismatch: {item.fileName}. Expected: {item.expectedFileSize}, Actual: {fileInfo.Length}");
                    return false;
                }
            }

            // Method 2: Check if file can be opened/read (basic corruption check)
            try
            {
                using (FileStream fs = new FileStream(item.savePath, FileMode.Open, FileAccess.Read))
                {
                    // Try to read the end of the file
                    fs.Seek(-100, SeekOrigin.End);
                    byte[] buffer = new byte[100];
                    await fs.ReadAsync(buffer, 0, 100);

                    // Additional: Check for MP4 headers
                    if (Path.GetExtension(item.savePath).ToLower() == ".mp4")
                    {
                        // Check for MP4 signature (basic check)
                        fs.Seek(4, SeekOrigin.Begin);
                        byte[] header = new byte[4];
                        fs.Read(header, 0, 4);
                        string headerStr = System.Text.Encoding.ASCII.GetString(header);

                        // MP4 files typically have 'ftyp' at position 4
                        if (!headerStr.Contains("ftyp"))
                        {
                            Debug.LogWarning($"MP4 file may be corrupted: {item.fileName}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"File read error (corrupted): {item.fileName} - {e.Message}");
                return false;
            }

            // Method 3: Hash verification (most reliable but slower)
            if (!string.IsNullOrEmpty(item.expectedHash))
            {
                string actualHash = await CalculateFileHash(item.savePath);
                if (actualHash != item.expectedHash)
                {
                    Debug.LogWarning($"Hash mismatch: {item.fileName}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error verifying file: {e.Message}");
            return false;
        }
    }

    private async Task<string> CalculateFileHash(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            byte[] hashBytes = await Task.Run(() => md5.ComputeHash(stream));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public async Task DownloadVideoWithResumeAndRetry(VideoDownloadItem item)
    {
        bool downloadSuccess = false;

        while (!downloadSuccess && item.retryCount < maxRetryAttempts)
        {
            try
            {
                downloadSuccess = await AttemptDownload(item);

                if (!downloadSuccess)
                {
                    item.retryCount++;
                    if (item.retryCount < maxRetryAttempts)
                    {
                        item.status = $"Retrying ({item.retryCount}/{maxRetryAttempts})...";
                        Debug.LogWarning($"Retry {item.retryCount} for {item.fileName}");
                        await Task.Delay(1000 * item.retryCount); // Exponential backoff
                    }
                }
            }
            catch (Exception e)
            {
                item.retryCount++;
                item.status = $"Error: {e.Message}";
                Debug.Log($"Download error for {item.fileName}: {e.Message}");

                if (item.retryCount < maxRetryAttempts)
                {
                    await Task.Delay(1000 * item.retryCount);
                }
            }
        }

        if (!downloadSuccess)
        {
            item.status = $"Failed after {maxRetryAttempts} attempts";
            Debug.Log($"Failed to download {item.fileName} after {maxRetryAttempts} attempts");
        }
    }

    private async Task<bool> AttemptDownload(VideoDownloadItem item)
    {
        string tempPath = item.savePath + ".tmp";
        long downloadedBytes = 0;
        long totalBytes = 0;

        // Check for existing temp file
        if (File.Exists(tempPath))
        {
            downloadedBytes = new FileInfo(tempPath).Length;
        }

        // Clean up if temp file is suspiciously small after multiple attempts
        if (downloadedBytes < 1024 && item.retryCount > 0)
        {
            try { File.Delete(tempPath); } catch { }
            downloadedBytes = 0;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(item.url))
        {
            if (downloadedBytes > 0)
            {
                request.SetRequestHeader("Range", $"bytes={downloadedBytes}-");
            }

            request.downloadHandler = new DownloadHandlerFile(tempPath, downloadedBytes > 0);
            var operation = request.SendWebRequest();

            // Get total file size
            while (!operation.isDone)
            {
                if (totalBytes == 0)
                {
                    string contentLength = request.GetResponseHeader("Content-Length");
                    string contentRange = request.GetResponseHeader("Content-Range");

                    if (!string.IsNullOrEmpty(contentRange))
                    {
                        var parts = contentRange.Split('/');
                        if (parts.Length > 1 && long.TryParse(parts[1], out long total))
                        {
                            totalBytes = total;
                        }
                    }
                    else if (!string.IsNullOrEmpty(contentLength) && long.TryParse(contentLength, out long length))
                    {
                        totalBytes = length;
                    }
                }

                if (totalBytes > 0)
                {
                    long currentDownloaded = downloadedBytes + (long)request.downloadedBytes;
                    item.progress = Mathf.Clamp01((float)currentDownloaded / totalBytes);
                }
                else
                {
                    item.progress = operation.progress;
                }

                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.responseCode == 416) // Already complete
                {
                    item.status = "Verifying...";

                    // Move temp to final
                    if (File.Exists(tempPath))
                    {
                        if (File.Exists(item.savePath)) File.Delete(item.savePath);
                        File.Move(tempPath, item.savePath);
                    }

                    // Verify the downloaded file
                    bool isVerified = await VerifyFileIntegrity(item);
                    if (isVerified)
                    {
                        item.isDownloaded = true;
                        item.isVerified = true;
                        item.progress = 1f;
                        item.status = "Completed";
                        return true;
                    }
                    else
                    {
                        item.status = "Download complete but verification failed";
                        return false;
                    }
                }
                else
                {
                    item.status = $"Failed: {request.error}";
                    return false;
                }
            }

            // Download completed, verify file
            item.status = "Verifying download...";

            // Move to final location
            try
            {
                if (File.Exists(item.savePath)) File.Delete(item.savePath);
                File.Move(tempPath, item.savePath);

                // Verify integrity
                bool isVerified = await VerifyFileIntegrity(item);
                if (isVerified)
                {
                    item.isDownloaded = true;
                    item.isVerified = true;
                    item.progress = 1f;
                    item.status = "Completed";
                    Debug.Log($"Download complete and verified: {item.fileName}");
                    return true;
                }
                else
                {
                    // Delete corrupted file
                    if (File.Exists(item.savePath)) File.Delete(item.savePath);
                    item.status = "Verification failed";
                    Debug.LogWarning($"Verification failed for {item.fileName}");
                    return false;
                }
            }
            catch (Exception e)
            {
                item.status = $"File error: {e.Message}";
                Debug.LogError($"File operation failed for {item.fileName}: {e.Message}");
                return false;
            }
        }
    }

    // Manual retry for a specific item
    public async Task RetryDownload(VideoDownloadItem item)
    {
        if (item.isDownloaded && item.isVerified)
        {
            Debug.Log($"File {item.fileName} is already downloaded and verified");
            return;
        }

        item.retryCount = 0;
        item.isDownloaded = false;
        item.isVerified = false;
        item.progress = 0f;
        item.status = "Retrying...";

        await DownloadVideoWithResumeAndRetry(item);
    }

    // Verify all downloaded files
    public async Task VerifyAllDownloads()
    {
        foreach (var item in downloadQueue)
        {
            if (item.isDownloaded && !item.isVerified)
            {
                item.status = "Verifying...";
                bool isValid = await VerifyFileIntegrity(item);

                if (isValid)
                {
                    item.isVerified = true;
                    item.status = "Verified";
                }
                else
                {
                    item.isDownloaded = false;
                    item.status = "Corrupted, needs redownload";
                }
            }
        }
    }

    // UI Helper Methods
    public float GetOverallProgress()
    {
        if (downloadQueue.Count == 0) return 0f;
        float totalProgress = downloadQueue.Sum(item => item.progress);
        return totalProgress / downloadQueue.Count;
    }

    public int GetCompletedCount()
    {
        return downloadQueue.Count(item => item.isDownloaded && item.isVerified);
    }

    public int GetFailedCount()
    {
        return downloadQueue.Count(item =>
            item.retryCount >= maxRetryAttempts &&
            (!item.isDownloaded || !item.isVerified));
    }
}