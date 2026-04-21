using UnityEngine;
using System.IO;

public class CSVLogger : MonoBehaviour
{
    private string filePath;

    void Awake()
    {
        string sessionID = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        filePath = Path.Combine(Application.persistentDataPath, $"log_{sessionID}.csv");

        // Header
        File.WriteAllText(filePath, "Time,Type,Message\n");
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        float time = Time.time;

        // Clean message so CSV doesn't break
        string cleanMessage = logString
            .Replace(",", ";")
            .Replace("\n", " ");

        string line = $"{time:F2},{type},{cleanMessage}";
        File.AppendAllText(filePath, line + "\n");
    }
}