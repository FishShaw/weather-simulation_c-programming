using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;

public class datadownload : MonoBehaviour
{
    private string apiKey = "eyJvcmciOiI1ZTU1NGUxOTI3NGE5NjAwMDEyYTNlYjEiLCJpZCI6ImI4N2EzMDIwOTRmZDRkYmJhOGE1NGU3YWU0ZGE3ZjViIiwiaCI6Im11cm11cjEyOCJ9"; 
    private string baseUrl = "https://api.dataplatform.knmi.nl/open-data/v1/datasets/harmonie_arome_cy43_p1/versions/1.0/files";

    void Start()
    {
        StartCoroutine(FetchWeatherData());
    }

    IEnumerator FetchWeatherData()
    {
        DateTime startDate = new DateTime(2024, 7, 9, 18, 0, 0, DateTimeKind.Utc);
        
        for (int hour = 0; hour <= 3; hour++)
        {
            DateTime currentTime = startDate.AddHours(hour);
            yield return StartCoroutine(FetchHourlyData(currentTime));
            yield return new WaitForSeconds(0.5f); // 添加短暂延迟以避免请求过于频繁
        }
    }

    IEnumerator FetchHourlyData(DateTime time)
    {
        string timestamp = time.ToString("yyyyMMddHH");
        string fileName = $"HARM43_V1_P1_{timestamp}.tar";
        Debug.Log($"Fetching file: {fileName}");

        var queryParams = new Dictionary<string, string>
        {
            { "maxKeys", "1" },
            { "orderBy", "created" },
            { "sorting", "asc" },
            { "filename", fileName }
        };

        yield return StartCoroutine(ListFiles(queryParams, fileName));
    }

    IEnumerator ListFiles(Dictionary<string, string> queryParams, string fileName)
    {
        string url = baseUrl + "?" + string.Join("&", queryParams.Select(kv => 
            $"{UnityWebRequest.EscapeURL(kv.Key)}={UnityWebRequest.EscapeURL(kv.Value)}"
        ));

        Debug.Log($"Listing files with query: {url}");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Authorization", apiKey);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Unable to retrieve list of files: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
                Debug.LogError($"Full Response: {webRequest.downloadHandler.text}");
                yield break;
            }

            string jsonResponse = webRequest.downloadHandler.text;
            JObject response = JObject.Parse(jsonResponse);

            if (response.ContainsKey("error"))
            {
                Debug.LogError($"API Error: {response["error"]}");
                yield break;
            }

            var files = response["files"] as JArray;
            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    string fileNameFromResponse = file["filename"].ToString();
                    if (fileNameFromResponse == fileName)
                    {
                        yield return StartCoroutine(GetFileUrl(fileName));
                        break;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"No file found: {fileName}");
            }
        }
    }

    IEnumerator GetFileUrl(string fileName)
    {
        Debug.Log($"Getting download URL for file: {fileName}");
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
        Debug.Log($"Starting download of file: {fileName}");
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
            Debug.Log($"Successfully downloaded file to {savePath}");
        }
    }
}
