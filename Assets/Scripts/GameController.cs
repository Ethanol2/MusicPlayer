using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using SFB;

public class GameController : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public PlayerCharacter character;
    public GameObject gameObjects;
    public GameObject gameUI;
    public GameObject menuUI;

    [Space]
    public Button startButton;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text songTitle;

    [Header("Music")]
    public bool hasMusic = false;
    public AudioClip music;

    [Header("Game")]
    public bool playingGame = false;
    public int score = 0;
    public float musicSpeed = 1f;
    public int countdownLength = 2;


    // Start is called before the first frame update
    void Start()
    {
        songTitle.text = "No song yet";

        gameObjects.SetActive(false);
        gameObjects.SetActive(false);
        menuUI.SetActive(true);

        startButton.enabled = hasMusic;
    }

    // Update is called once per frame
    void Update()
    {
        startButton.enabled = hasMusic;
        if (playingGame)
        {
            if (character.transform.localPosition.x > 0f)
            {
                score += 5;
            }
            else
            {
                score += 1;
            }

            scoreText.text = score.ToString();

            if (character.transform.localPosition.y < -10f)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    public void StartGame()
    {
        songTitle.text = music.name;
        player.music = music;

        scoreText.text = score.ToString();

        gameObjects.SetActive(true);
        gameUI.SetActive(true);
        menuUI.SetActive(false);

        player.playSpeed = musicSpeed;

        player.music = music;
        StartCoroutine(Countdown(2));
    }
    public void OpenMenu()
    {
        gameObjects.SetActive(false);
        gameObjects.SetActive(false);
        menuUI.SetActive(true);

        playingGame = false;
    }
    public void LoadSong()
    {
        string path = GetSongPath();
        if (path == "")
        {
            songTitle.text = "Song Load Failed";
            return;
        }
        StartCoroutine(SongLoader(path));
    }
    string GetSongPath()
    {
        var extensions = new[] {
            new ExtensionFilter("Sound Files", "wav"),
            new ExtensionFilter("All Files", "*" ),
        };

        string[] path = StandaloneFileBrowser.OpenFilePanel("Import Song", "", extensions, false);
        if (path.Length < 1) return "";

        return path[0];
    }
    IEnumerator SongLoader(string path)
    {
        UnityWebRequest AudioFile = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);
        yield return AudioFile.SendWebRequest();
        if (AudioFile.isNetworkError)
        {
            Debug.Log(AudioFile.error);
            Debug.Log(path);
            songTitle.text = "Song Load Failed";
        }
        else
        {
            music = DownloadHandlerAudioClip.GetContent(AudioFile);
            music.name = songTitle.text = Path.GetFileNameWithoutExtension(path);
            hasMusic = true;
        }
    }

    IEnumerator Countdown(float length)
    {
        float tempDTime = Time.deltaTime;
        Time.timeScale = 0f;
        for (float k = 0; k < length; k += tempDTime)
        {
            yield return null;
        }
        Time.timeScale = 1f;
        player.Play();

        playingGame = true;
    }
}
