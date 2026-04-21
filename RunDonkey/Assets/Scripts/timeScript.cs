using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class timeScript : MonoBehaviour
{
    private float elapsedTime;
    bool runFinished = false;
    public string playerName;

    public TextMeshProUGUI timerText;
    public GameObject canvasObject;
    public TextMeshProUGUI scoreboardText;

    private void Start()
    {
        elapsedTime = 0f;
        PlayerPrefs.DeleteAll();
    }

    // timer starts counting
    private void Update()
    {
        if (!runFinished)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    // runtime found when crossing finishline
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("finishline"))
        {
            canvasObject.SetActive(true);
            runFinished = true;

            // Create list to store scores
            List<(string name, float score)> scores = new List<(string, float)>();

            int playerCount = PlayerPrefs.GetInt("playerCount", 0);

            // Load existing scores
            for (int i = 0; i < playerCount; i++)
            {
                string savedName = PlayerPrefs.GetString("playerName" + i);
                float savedScore = PlayerPrefs.GetFloat("playerScore" + i);

                scores.Add((savedName, savedScore));
            }

            // Add current player
            scores.Add((playerName, elapsedTime));

            // Sort ascending (lowest time first)
            scores = scores.OrderBy(s => s.score).ToList();

            // Keep only top 5
            if (scores.Count > 5)
            {
                scores = scores.Take(5).ToList();
            }

            // Clear old PlayerPrefs
            PlayerPrefs.DeleteKey("playerCount");

            // Save updated list
            for (int i = 0; i < scores.Count; i++)
            {
                PlayerPrefs.SetString("playerName" + i, scores[i].name);
                PlayerPrefs.SetFloat("playerScore" + i, scores[i].score);
            }

            PlayerPrefs.SetInt("playerCount", scores.Count);

            // Display scoreboard
            scoreboardText.text = "";
            foreach (var s in scores)
            {
                scoreboardText.text += s.name + ": " + s.score.ToString("F2") + "\n";
            }

            // Show current time
            timerText.text = "your time: " + elapsedTime.ToString("F2");
        }
    }
    
}