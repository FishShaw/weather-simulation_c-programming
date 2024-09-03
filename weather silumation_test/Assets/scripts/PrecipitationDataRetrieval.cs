using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;

public class PrecipitationDataRetrieval : MonoBehaviour
{
    private string apiKey = "eyJvcmciOiI1ZTU1NGUxOTI3NGE5NjAwMDEyYTNlYjEiLCJpZCI6ImI4N2EzMDIwOTRmZDRkYmJhOGE1NGU3YWU0ZGE3ZjViIiwiaCI6Im11cm11cjEyOCJ9"; 
    private string baseUrl = "https://api.dataplatform.knmi.nl/open-data/v1/datasets/radar_reflectivity_composites/versions/2.0/files";

    void Start()
    {
        StartCoroutine(FetchWeatherData());
    }

    IEnumerator FetchWeatherData()
    {
        // 设置开始和结束时间
        DateTime startTime = new DateTime(2024, 8, 31, 12, 0, 0, DateTimeKind.Utc);
        DateTime endTime = new DateTime(2024, 8, 31, 13, 0, 0, DateTimeKind.Utc);

        string startTimestamp = startTime.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
        string endTimestamp = endTime.ToString("yyyy-MM-ddTHH:mm:ss+00:00");

        Debug.Log($"Fetching files from {startTimestamp} to {endTimestamp}");

        var queryParams = new Dictionary<string, string>
        {
            { "maxKeys", "1000" }, // 增加最大键值以确保获取所有文件
            { "orderBy", "created" },
            { "sorting", "asc" }, // 按时间升序排列
            { "begin", startTimestamp },
            { "end", endTimestamp }
        };

        yield return StartCoroutine(ListFiles(queryParams));
    }

    IEnumerator ListFiles(Dictionary<string, string> queryParams)
    {
        string url = baseUrl;
        string queryString = "?" + string.Join("&", queryParams.Select(kv => 
            $"{UnityWebRequest.EscapeURL(kv.Key)}={UnityWebRequest.EscapeURL(kv.Value)}"
        ));
        url += queryString;

        Debug.Log($"Full URL being requested: {url}");

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Authorization", apiKey);
            yield return webRequest.SendWebRequest();

            Debug.Log($"Response Code: {webRequest.responseCode}");
            Debug.Log($"Full Response: {webRequest.downloadHandler.text}");

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Unable to retrieve list of files: {webRequest.error}");
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
                Debug.Log($"Number of files found: {files.Count}");
                foreach (var file in files)
                {
                    string fileName = file["filename"].ToString();
                    Debug.Log($"Processing file: {fileName}");
                    yield return StartCoroutine(GetFileUrl(fileName));
                }
            }
            else
            {
                Debug.LogWarning("No files found for the specified date range.");
                Debug.Log($"Response content: {jsonResponse}");
            }
        }
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

            string saveDirectory = @"F:\Unity_project_thesis_digital twins\datasets";//储存路径
            System.IO.Directory.CreateDirectory(saveDirectory);
            string savePath = System.IO.Path.Combine(saveDirectory, fileName);

            System.IO.File.WriteAllBytes(savePath, webRequest.downloadHandler.data);
            Debug.Log($"Successfully downloaded dataset file to {savePath}");
        }
    }
}
