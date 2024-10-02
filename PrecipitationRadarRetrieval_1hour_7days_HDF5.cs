using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;

public class PrecipitationRadarRetrieval_1hour : MonoBehaviour
{
    private string apiKey = "eyJvcmciOiI1ZTU1NGUxOTI3NGE5NjAwMDEyYTNlYjEiLCJpZCI6ImI4N2EzMDIwOTRmZDRkYmJhOGE1NGU3YWU0ZGE3ZjViIiwiaCI6Im11cm11cjEyOCJ9"; 
    //Gridded files of radar reflectivities at 1500 m over the Netherlands and surrounding area measured by two radars in Herwijnen and Den Helder. Time interval is 5 minutes. See data set radar_tar_refl_composites/1.0 for an archive that goes back to 2008.
    private string baseUrl = "https://api.dataplatform.knmi.nl/open-data/v1/datasets/radar_reflectivity_composites/versions/2.0/files";

    void Start()
    {
        StartCoroutine(FetchWeatherData());
    }

    IEnumerator FetchWeatherData()
    {
        DateTime startDate = new DateTime(2023, 10, 16, 0, 0, 0, DateTimeKind.Utc);
        
        for (int day = 0; day < 7; day++)
        {
            DateTime currentDate = startDate.AddDays(day);
            for (int hour = 0; hour < 24; hour++)
            {
                DateTime currentTime = currentDate.AddHours(hour);
                yield return StartCoroutine(FetchHourlyData(currentTime));
            }
        }
    }

    IEnumerator FetchHourlyData(DateTime time)
    {
        // 将时间转换为API所需的格式
        string timestamp = time.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
        Debug.Log($"Fetching file for {timestamp}");

        // 设置查询参数
        var queryParams = new Dictionary<string, string>
        {
            { "maxKeys", "1" },
            { "orderBy", "created" },
            { "sorting", "asc" },
            { "begin", timestamp },
            { "end", time.AddMinutes(5).ToString("yyyy-MM-ddTHH:mm:ss+00:00") }
        };

        // 调用ListFiles方法获取文件列表
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

            string baseDirectory = @"F:\Unity_project_thesis_digital twins\datasets";
            string scriptName = this.GetType().Name; // 获取脚本的名称
            string saveDirectory = Path.Combine(baseDirectory, scriptName);
            Directory.CreateDirectory(saveDirectory);
            string savePath = Path.Combine(saveDirectory, fileName);

            File.WriteAllBytes(savePath, webRequest.downloadHandler.data);
            Debug.Log($"Successfully downloaded dataset file to {savePath}");
        }
    }
}
