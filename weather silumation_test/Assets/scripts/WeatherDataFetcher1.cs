using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;

public class WeatherDataFetcher1 : MonoBehaviour
{
    private string apiKey = "eyJvcmciOiI1ZTU1NGUxOTI3NGE5NjAwMDEyYTNlYjEiLCJpZCI6ImI4N2EzMDIwOTRmZDRkYmJhOGE1NGU3YWU0ZGE3ZjViIiwiaCI6Im11cm11cjEyOCJ9";
    private string datasetName = "Actuele10mindataKNMIstations";
    private string datasetVersion = "2";
    private string baseUrl = "https://api.dataplatform.knmi.nl/open-data/v1/datasets/Actuele10mindataKNMIstations/versions/2/files/KMDS__OPER_P___10M_OBS_L2_202212102330.nc/url";

    void Start()
    {
        StartCoroutine(FetchWeatherData());
    }

    IEnumerator FetchWeatherData()
    {
        Debug.Log($"Fetching latest files of {datasetName} version {datasetVersion}");

        var queryParams = new Dictionary<string, string>
        {
            { "maxKeys", "2" },
            { "orderBy", "created" },
            { "sorting", "desc" }
        };

        string url = $"{baseUrl}/datasets/{datasetName}/versions/{datasetVersion}/files";
        yield return StartCoroutine(FetchFiles(url, queryParams));
    }

    IEnumerator FetchFiles(string url, Dictionary<string, string> queryParams)
    {
        string queryString = string.Join("&", queryParams.Select(x => $"{UnityWebRequest.EscapeURL(x.Key)}={UnityWebRequest.EscapeURL(x.Value)}"));
        url += "?" + queryString;

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

            foreach (var file in response["files"])
            {
                string filename = file["filename"].ToString();
                Debug.Log($"Downloading file: {filename}");

                yield return StartCoroutine(GetFileUrl(filename));
            }
        }
    }

    IEnumerator GetFileUrl(string filename)
    {
        string url = $"{baseUrl}/datasets/{datasetName}/versions/{datasetVersion}/files/{filename}/url";

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
            yield return StartCoroutine(DownloadFile(downloadUrl, filename));
        }
    }

    IEnumerator DownloadFile(string downloadUrl, string filename)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(downloadUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Unable to download file: {webRequest.error}");
                yield break;
            }

            // 更改保存路径
            string saveDirectory = @"F:\Unity_project_thesis_digital twins\datasets";
            Directory.CreateDirectory(saveDirectory);
            string savePath = Path.Combine(saveDirectory, filename);

            File.WriteAllBytes(savePath, webRequest.downloadHandler.data);
            Debug.Log($"Successfully downloaded dataset file to {savePath}");
        }
    }
}
