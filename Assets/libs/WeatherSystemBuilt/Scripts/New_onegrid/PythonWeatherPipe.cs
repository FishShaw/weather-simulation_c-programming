using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Text;
using WeatherSystem.SingleGrid;
using System.Linq;
using Wander;

namespace WeatherSystem.SingleGrid
{
    [Serializable]
    public class WeatherDataJson
    {
        public float[] wind_u;
        public float[] wind_v;
        public float[] wind_speed;
        public float[] wind_direction;
        public float[] rain;
    }

    public class Pipe
    {
        private NamedPipeClientStream pipeClient;
        private string pipeName;
        private const int MAX_BUFFER_SIZE = 131072;

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
                throw new IndexOutOfRangeException($"Sum of id and msg cannot exceed {MAX_BUFFER_SIZE - 1} in length.");
            }
            
            byte[] idBuffer = Encoding.UTF8.GetBytes(messageId);
            byte[] contentBuffer = Encoding.UTF8.GetBytes(content);
            
            pipeClient.Write(idBuffer, 0, idBuffer.Length);
            pipeClient.Write(new byte[] { 0 }, 0, 1);  // add separator
            pipeClient.Write(contentBuffer, 0, contentBuffer.Length);
            pipeClient.Write(new byte[] { 0 }, 0, 1);  // add end
            pipeClient.Flush();
        }

        public (string, string) Read()
        {
            try 
            {
                byte[] buffer = new byte[MAX_BUFFER_SIZE];
                List<byte> messageBytes = new List<byte>();
                
                // read the message id until the separator
                while (true)
                {
                    int b = pipeClient.ReadByte();
                    if (b == 0 || b == -1) break;
                    messageBytes.Add((byte)b);
                }
                
                string messageId = Encoding.UTF8.GetString(messageBytes.ToArray());
                
                // read the message content until the separator
                messageBytes.Clear();
                while (true)
                {
                    int b = pipeClient.ReadByte();
                    if (b == 0 || b == -1) break;
                    messageBytes.Add((byte)b);
                }
                
                string content = Encoding.UTF8.GetString(messageBytes.ToArray());
                
                return (messageId, content);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading message: {e.Message}");
                throw;
            }
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
        private const int GRID_SIZE = 64;

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
                var terrainBuilder = FindFirstObjectByType<TerrainBuilder>();
                if (terrainBuilder != null)
                {
                    gridMapper.Initialize(terrainBuilder.originRDX, terrainBuilder.originRDY);
                }
                List<Vector2> coordinates = gridMapper.Generate64x64Grid();
                
                //divide the coordinates into chunks
                const int CHUNK_SIZE = 1024;
                int totalChunks = (coordinates.Count + CHUNK_SIZE - 1) / CHUNK_SIZE;
                
                for (int i = 0; i < coordinates.Count; i += CHUNK_SIZE)
                {
                    int count = Math.Min(CHUNK_SIZE, coordinates.Count - i);
                    var chunk = coordinates.GetRange(i, count);
                    
                    string chunkMsg = string.Join("|", chunk.Select(coord => $"{coord.x},{coord.y}"));
                    string chunkHeader = $"{i}/{coordinates.Count}";
                    
                    await Task.Run(() =>
                    {
                        pipe.Write("COORDS_CHUNK", $"{chunkHeader}:{chunkMsg}");
                    });
                    
                    var (mid, msg) = await Task.Run(() => pipe.Read());
                    
                    if (mid == "ERROR")
                    {
                        Debug.LogError($"Server error: {msg}");
                        isConnected = false;
                        return null;
                    }
                    else if (mid != "CHUNK_ACK")
                    {
                        Debug.LogError($"Unexpected response type: {mid}");
                        return null;
                    }
                }
                
                await Task.Run(() =>
                {
                    pipe.Write("COORDS_END", "");
                });

                var (messageId, content) = await Task.Run(() => pipe.Read());
                
                if (messageId != "WEATHER")
                {
                    Debug.LogError($"Unexpected final response: {messageId}");
                    return null;
                }

                // 添加日志来检查接收到的原始数据
                Debug.Log($"Received content length: {content.Length}");
                Debug.Log($"First 100 chars: {content.Substring(0, Math.Min(100, content.Length))}");

                try 
                {
                    var weatherData = JsonUtility.FromJson<WeatherDataJson>(content);
                    if (weatherData == null)
                    {
                        Debug.LogError("Weather data deserialization returned null");
                        return null;
                    }
                    
                    // 验证数组长度
                    int expectedLength = 16 * GRID_SIZE * GRID_SIZE;
                    if (weatherData.wind_u?.Length != expectedLength ||
                        weatherData.wind_v?.Length != expectedLength ||
                        weatherData.wind_speed?.Length != expectedLength ||
                        weatherData.wind_direction?.Length != expectedLength ||
                        weatherData.rain?.Length != expectedLength)
                    {
                        Debug.LogError($"Invalid array lengths in weather data. Expected {expectedLength}");
                        return null;
                    }
                    
                    var result = new Dictionary<int, WeatherData_new>();
                    
                    for (int hour = 0; hour < 16; hour++)
                    {
                        var hourData = new WeatherData_new();
                        int offset = hour * GRID_SIZE * GRID_SIZE;
                        
                        for (int y = 0; y < GRID_SIZE; y++)
                        for (int x = 0; x < GRID_SIZE; x++)
                        {
                            int i = offset + y * GRID_SIZE + x;
                            hourData.WindU[x, y] = weatherData.wind_u[i];
                            hourData.WindV[x, y] = weatherData.wind_v[i];
                            hourData.WindSpeed[x, y] = weatherData.wind_speed[i];
                            hourData.WindDirection[x, y] = weatherData.wind_direction[i];
                            hourData.Rain[x, y] = weatherData.rain[i];
                        }
                        
                        result[hour] = hourData;
                    }
                    
                    return result;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error deserializing weather data: {e.Message}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error requesting weather data: {e.Message}");
                return null;
            }
        }

        private void OnDestroy()
        {
            pipe?.Close();
        }
    }
}