using System;
using System.Collections;
using System.IO;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Networking;

internal class DownloadSystem : MonoBehaviour
{
    public void StartDownload(string fileName,Action<string> onFinished)
    {
        StartCoroutine(DoDownloadFileAndSave(fileName, onFinished));
    }
    IEnumerator DoDownloadFileAndSave(string id, Action<string> onFinished)
    {
        string webPath = PathHelper.GetWebPath(id);
        string localPath = PathHelper.GetLocalPath(id);
        using (UnityWebRequest www = new UnityWebRequest(webPath, UnityWebRequest.kHttpVerbGET))
        {
            www.certificateHandler = new BypassCertificate();
            DownloadHandlerFile dh = new DownloadHandlerFile(localPath);
            dh.removeFileOnAbort = true;
            www.downloadHandler = dh;
            var async = www.SendWebRequest();
            while (async.progress < 1f)
            {
                onFinished?.Invoke($"Downloading: {async.progress}");
                yield return null;
            }
            yield return null;
			
            if (www.isNetworkError || www.isHttpError)
            {
                onFinished?.Invoke($"Error");
                Debug.LogError(www.error);
            }
            else
            {
                onFinished?.Invoke("Finished");
                Debug.Log("File successfully downloaded and saved to " + localPath);
            }
        }
    }

    public bool WasDownloaded(string id)
    {
        string path = PathHelper.GetLocalPath(id);
        return File.Exists(path) && new FileInfo(path).Length > 0;
    }

    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            //Simply return true no matter what
            return true;
        }
    } 
}