using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Text;
using WeatherSystem.SingleGrid;

namespace WeatherSystem.SingleGrid
{
    public class Pipe
    {
        private NamedPipeClientStream pipeClient;
        private string pipeName;
        private const int MAX_BUFFER_SIZE = 65536;

        public Pipe(string name, bool server)
        {
            pipeName = name;
            if (!server)
            {
                pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
                pipeClient.Connect(10000);
            }
            else
            {
                throw new Exception("Server mode not supported in C# client");
            }
        }

        public void Write(string messageId, string content)
        {
            if (messageId.Length + content.Length > MAX_BUFFER_SIZE - 1)
            {
                throw new IndexOutOfRangeException("Sum of id and msg cannot exceed 65536 in length.");
            }

            byte[] idBuffer = Encoding.UTF8.GetBytes(messageId);
            byte[] contentBuffer = Encoding.UTF8.GetBytes(content);
            
            pipeClient.Write(idBuffer, 0, idBuffer.Length);
            pipeClient.Write(contentBuffer, 0, contentBuffer.Length);
        }

        public (string, string) Read()
        {
            byte[] buffer = new byte[MAX_BUFFER_SIZE];
            
            int idBytesRead = pipeClient.Read(buffer, 0, MAX_BUFFER_SIZE);
            string messageId = Encoding.UTF8.GetString(buffer, 0, idBytesRead);
            
            int msgBytesRead = pipeClient.Read(buffer, 0, MAX_BUFFER_SIZE);
            string content = Encoding.UTF8.GetString(buffer, 0, msgBytesRead);
            
            return (messageId, content);
        }

        public void Close()
        {
            pipeClient?.Close();
            pipeClient?.Dispose();
        }
    }

    public class PythonWeatherPipe : MonoBehaviour
    {
        private static PythonWeatherPipe instance;
        private Pipe pipe;
        private bool isConnected;
        private const string PIPE_NAME = "KNMI_Interop_";
        private bool isConnecting = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private async Task<bool> TryConnect()
        {
            if (isConnecting) return false;
            
            try
            {
                isConnecting = true;
                pipe = new Pipe(PIPE_NAME, false);
                isConnected = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to connect to Python server: {e.Message}");
                await Task.Delay(1000);
                return false;
            }
            finally
            {
                isConnecting = false;
            }
        }

        private async void Start()
        {
            await Task.Delay(100);
            if (!isConnected)
            {
                while (!await TryConnect())
                {
                    await Task.Delay(1000);
                }
            }
        }

        public async Task<Dictionary<int, WeatherData_new>> RequestWeatherData(Vector3 worldPosition)
        {
            if (!isConnected)
            {
                Debug.LogWarning("Pipe not connected, attempting to reconnect...");
                if (!await TryConnect())
                {
                    return null;
                }
            }

            try
            {
                var gridMapper = GetComponent<WeatherCoordinates>();
                var (lat, lon) = gridMapper.WorldToGeographic(worldPosition);
                
                await Task.Run(() =>
                {
                    pipe.Write("COORDS", $"{lat},{lon}");
                });
                
                var (mid, msg) = await Task.Run(() => pipe.Read());

                if (mid == "ERROR")
                {
                    Debug.LogError($"Server error: {msg}");
                    isConnected = false;
                    return null;
                }

                if (!mid.StartsWith("WEATHER"))
                {
                    Debug.LogError($"Unexpected message type: {mid}");
                    return null;
                }

                return ParseWeatherData(msg);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to request weather data: {e.Message}");
                return null;
            }
        }

        private Dictionary<int, WeatherData_new> ParseWeatherData(string msg)
        {
            var result = new Dictionary<int, WeatherData_new>();
            
            try
            {
                string[] entries = msg.Split('#');
                foreach (string entry in entries)
                {
                    if (string.IsNullOrEmpty(entry)) continue;
                    
                    string[] parts = entry.Split(':');
                    if (parts.Length != 2) continue;

                    if (!int.TryParse(parts[0], out int hour)) continue;
                    
                    string[] values = parts[1].Split(',');
                    if (values.Length != 5) continue;  // 现在是5个值

                    if (float.TryParse(values[0], out float windU) &&
                        float.TryParse(values[1], out float windV) &&
                        float.TryParse(values[2], out float windSpeed) &&
                        float.TryParse(values[3], out float windDirection) &&
                        float.TryParse(values[4], out float rain))
                    {
                        result[hour] = new WeatherData_new
                        {
                            WindU = windU,
                            WindV = windV,
                            WindSpeed = windSpeed,
                            WindDirection = windDirection,
                            Rain = rain
                        };
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse weather data: {e.Message}");
                return new Dictionary<int, WeatherData_new>();
            }
        }

        private void OnDestroy()
        {
            if (pipe != null)
            {
                pipe.Close();
                isConnected = false;
            }
        }
    }
}