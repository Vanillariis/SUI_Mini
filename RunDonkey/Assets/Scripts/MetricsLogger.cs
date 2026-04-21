using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement; // For scene change tracking

public class MetricsLogger : MonoBehaviour
{
    // FPS / Metrics logging
    public float logInterval = 1f; // Time interval between each metrics log (in seconds)
    private float timer = 0f; // Accumulates time for FPS calculation
    private int frameCount = 0; // Counts frames per log interval

    private List<string> logLines = new List<string>(); // Holds all log lines for CSV export
    private string filePath; // Path where the CSV file is saved

    private static MetricsLogger instance; // Singleton to ensure only one logger exists across scenes

    void Awake()
    {
        // Ensure only one instance of the logger persists across scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to scene change events
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void Start()
    {
        // Save file in Unity persistent data folder (works on Quest + PC)
        string directoryPath = Application.persistentDataPath;

        // Create unique timestamped file
        filePath = Path.Combine(
            directoryPath,
            $"Metrics_Log_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv"
        );

        // CSV header (clean version)
        logLines.Add("Time (s),FPS,FrameTime(ms),Scene");

        // Initial log entry
        logLines.Add($"{Time.time:F2},0,0,{SceneManager.GetActiveScene().name}");
    }

    void Update()
    {
        frameCount++;
        timer += Time.unscaledDeltaTime;

        if (timer >= logInterval)
        {
            // FPS calculation
            float fps = (timer > 0f) ? (frameCount / timer) : 0f;

            // Frame time in ms
            float frameTimeMs = (frameCount > 0)
                ? (timer / frameCount) * 1000f
                : 0f;

            float timeSinceStart = Time.time;

            // Log row
            logLines.Add(
                $"{timeSinceStart:F2}," +
                $"{fps:F2}," +
                $"{frameTimeMs:F2}," +
                $"{SceneManager.GetActiveScene().name}"
            );

            // Reset counters
            timer = 0f;
            frameCount = 0;
        }
    }

    // Logs scene transitions (only scene info now)
    void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        float timeSinceStart = Time.time;

        logLines.Add(
            $"{timeSinceStart:F2},0,0,{newScene.name}"
        );
    }

    // Save log when application quits
    void OnApplicationQuit()
    {
        SaveLog();
    }

    // Write CSV file
    public void SaveLog()
    {
        File.WriteAllLines(filePath, logLines);
        Debug.Log("Metrics log saved to: " + filePath);
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
}