using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.IO;

public class Meteodata_observation_10mins_nc : MonoBehaviour
{
    private string apiKey = "eyJvcmciOiI1ZTU1NGUxOTI3NGE5NjAwMDEyYTNlYjEiLCJpZCI6ImI4N2EzMDIwOTRmZDRkYmJhOGE1NGU3YWU0ZGE3ZjViIiwiaCI6Im11cm11cjEyOCJ9"; 
    private string baseUrl = "https://api.dataplatform.knmi.nl/open-data/v1/datasets/Actuele10mindataKNMIstations/versions/2/files";

    void Start()
    {
        StartCoroutine(FetchWeatherData());
    }

    IEnumerator FetchWeatherData()
    {
        DateTime startDate = new DateTime(2023, 10, 16);
        DateTime endDate = new DateTime(2023, 10, 23); // 使用23日作为结束日期，因为循环不包括结束日期

        for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
        {
            for (int hour = 0; hour < 24; hour++)
            {
                DateTime currentTime = date.AddHours(hour);
                string fileName = GenerateFileName(currentTime);
                Debug.Log($"Attempting to download file: {fileName}");
                yield return StartCoroutine(GetFileUrl(fileName));
            }
        }
    }

    string GenerateFileName(DateTime time)
    {
        // 将时间向下舍入到最近的10分钟
        int minutes = (time.Minute / 10) * 10;
        return $"KMDS__OPER_P___10M_OBS_L2_{time:yyyyMMdd}{time.Hour:D2}{minutes:D2}.nc";
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