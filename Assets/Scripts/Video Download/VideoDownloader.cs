using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using UnityEngine.Video;

public class VideoDownloader : MonoBehaviour
{

    public void DownLoadVideo()
    {
        //StartCoroutine(DownloadVideo("https://myanimaltransport.com/storage/app/public/200MB.mp4", "Test_Video_Download"));
        //StartCoroutine(DownloadVideo1("https://myanimaltransport.com/storage/app/public/200MB.mp4", "downloaded_Video"));
        StartCoroutine(DownloadVideo2("https://myanimaltransport.com/storage/app/public/200MB.mp4", "downloaded_Video.mp4"));
        //videoPlay("downloaded_Video" + ".mp4");
    }


    public IEnumerator DownloadVideo(string url, string fileName)
    {
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
        {
            request.downloadHandler = new DownloadHandlerFile(filePath);
            request.SendWebRequest();

            while (!request.isDone)
            {
                Debug.Log("Progress: " + (request.downloadProgress * 100) + "%");
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Downloaded to: " + filePath);
            }
            else
            {
                Debug.LogError(request.error);
            }
        }
    }


    IEnumerator DownloadVideo1(string videoUrl, string fileName)
    {
        string filePath = Path.Combine(Application.dataPath, fileName);

        Debug.Log("Start ..");
        UnityWebRequest request = UnityWebRequest.Get(videoUrl);

        // FIX: Set browser-like headers to avoid 403 Forbidden
        request.SetRequestHeader("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36");

        request.downloadHandler = new DownloadHandlerFile(filePath); // BEST FOR LARGE FILES
        request.SendWebRequest();

        while (!request.isDone)
        {
            float progress = request.downloadProgress;
            Debug.Log($"Downloading... {(progress * 100f):0}%");
            yield return null;
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Video saved at: " + filePath);
        }
        else
        {
            Debug.LogError("Download error: " + request.error);
        }
    }

    public IEnumerator DownloadVideo2(string videoURL, string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        using (UnityWebRequest request = new UnityWebRequest(videoURL))
        {
            request.downloadHandler = new DownloadHandlerFile(path);
            request.disposeDownloadHandlerOnDispose = true;

            request.SendWebRequest();

            while (!request.isDone)
            {
                Debug.Log($"Downloading: {(request.downloadProgress * 100f):0}%");
                yield return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download failed: " + request.error);
            }
            else
            {
                Debug.Log("Download completed: " + path);
                videoPlay(fileName);
            }
        }
    }

    public VideoPlayer videoPlayer;
    void videoPlay(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogError("Video file not found: " + path);
            return;
        }

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = path;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPrepared;
    }

    void OnPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

}