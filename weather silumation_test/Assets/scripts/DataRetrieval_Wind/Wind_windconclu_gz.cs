using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.IO;

public class Wind_windconclu_gz : MonoBehaviour
{
    private string apiKey = "eyJvcmciOiI1ZTU1NGUxOTI3NGE5NjAwMDEyYTNlYjEiLCJpZCI6ImI4N2EzMDIwOTRmZDRkYmJhOGE1NGU3YWU0ZGE3ZjViIiwiaCI6Im11cm11cjEyOCJ9"; 
    private string baseUrl = "https://api.dataplatform.knmi.nl/open-data/v1/datasets/windgegevens/versions/1.0/files";
    private string targetFileName = "kis_tow_202310.gz";

    void Start()
    {
        StartCoroutine(FetchWindData());
    }

    IEnumerator FetchWindData()
    {
        //debug information
        Debug.Log($"Attempting to download file: {targetFileName}");
        yield return StartCoroutine(GetFileUrl(targetFileName));
    }

    IEnumerator GetFileUrl(string fileName)
    {
        string url = $"{baseUrl}/{fileName}/url";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Authorization", apiKey);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Unable to get file URL: {webRequest.error}");
                yield break;
            }

            string jsonResponse = webRequest.downloadHandler.text;
            JObject response = JObject.Parse(jsonResponse);

            if (response.ContainsKey("error"))
            {
                Debug.LogError($"API Error: {response["error"]}");
                yield break;
            }

            string downloadUrl = response["temporaryDownloadUrl"].ToString();
            yield return StartCoroutine(DownloadFile(downloadUrl, fileName));
        }
    }

    IEnumerator DownloadFile(string downloadUrl, string fileName)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(downloadUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Unable to download file: {webRequest.error}");
                yield break;
            }

            string baseDirectory = @"F:\Unity_project_thesis_digital twins\datasets";
            string scriptName = this.GetType().Name;
            string saveDirectory = Path.Combine(baseDirectory, scriptName);
            Directory.CreateDirectory(saveDirectory);
            string savePath = Path.Combine(saveDirectory, fileName);

            File.WriteAllBytes(savePath, webRequest.downloadHandler.data);
            Debug.Log($"Successfully downloaded dataset file to {savePath}");
        }
    }
}