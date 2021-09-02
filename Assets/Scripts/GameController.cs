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
    public Visualizer visualizer;

    [Space]
    public Button startButton;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text songTitle;
    public Slider volumeSlider;
    static float volumeValue = 0.5f;

    [Header("Music")]
    public bool hasMusic = false;
    static public AudioClip music;

    [Header("Game")]
    public bool playingGame = false;
    public int score = 0;
    public float musicSpeed = 1f;
    public int countdownLength = 2;
    public GameObject visualisedSong;

    int resetNum = 0;
    float lastVisWidth;
    bool swapControl = false;
    bool halfTime = false;

    // Start is called before the first frame update
    void Start()
    {
        if (music != null)
        {
            songTitle.text = music.name;
            hasMusic = true;
        }
        else
        {
            if (PlayerPrefs.GetInt("HasMusic", 0) == 1)
            {
                StartCoroutine(SongLoader(PlayerPrefs.GetString("Song")));
            }
            else
            {
                hasMusic = false;
                songTitle.text = "No song yet";
            }
        }

        score = 0;

        gameObjects.SetActive(false);
        gameObjects.SetActive(false);
        menuUI.SetActive(true);

        lastVisWidth = visualizer.sequenceWidth;

        startButton.interactable = hasMusic;

        volumeValue = PlayerPrefs.GetFloat("Volume", 0.5f);
        player.audioSource.volume = volumeValue;
        volumeSlider.value = volumeValue;
    }

    // Update is called once per frame
    void Update()
    {
        startButton.interactable = hasMusic;
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

            if (character.GetComponent<PlayerCharacter>().dead)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (player.playTime >= music.length * 0.5f && !halfTime)
            {
                AddVisualisedSong();
                halfTime = true;
            }
            else if (player.playTime >= music.length)
            {
                swapControl = true;
                halfTime = false;
                player.visualizers[0] = visualizer;
                player.playTime = 0f;
            }

            character.songMovement = lastVisWidth / music.length;
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Time.timeScale += 0.25f;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            Time.timeScale -= 0.25f;
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
            hasMusic = false;
        }
        else
        {
            music = DownloadHandlerAudioClip.GetContent(AudioFile);
            music.name = songTitle.text = Path.GetFileNameWithoutExtension(path);
            hasMusic = true;
            PlayerPrefs.SetInt("HasMusic", 1);
            PlayerPrefs.SetString("Song", path);
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
    public void VolumeChange()
    {
        volumeValue = volumeSlider.value;
        player.audioSource.volume = volumeValue;
        PlayerPrefs.SetFloat("Volume", volumeValue);
    }
    void AddVisualisedSong()
    {
        GameObject.Instantiate(visualisedSong);
        float newVisLength;
        visualisedSong.transform.SetParent(gameObjects.transform);
        visualisedSong.name = $"visualisedSong {resetNum + 1}";


        visualisedSong.GetComponent<Visualizer>().musicPlayer = player;
        visualisedSong.GetComponent<Visualizer>().visualisedSongFolder = visualisedSong.transform;
        visualisedSong.GetComponent<Visualizer>().lineTemplate = visualisedSong.transform.GetChild(0).gameObject;
        visualisedSong.GetComponent<Visualizer>().boxCollider = visualisedSong.GetComponent<BoxCollider2D>();
        visualisedSong.GetComponent<Visualizer>().sequenceWidth = newVisLength = 100f * Random.Range(0.8f, 1.3f);
        visualisedSong.GetComponent<Visualizer>().squareSpacing = 0.1f;
        visualisedSong.GetComponent<Visualizer>().maxHeight = 5f * Random.Range(0.8f, 1.5f);
        visualisedSong.GetComponent<Visualizer>().numberOfSamples = 1000 / Random.Range(1, 3);
        visualisedSong.GetComponent<Visualizer>().averageSpread = 5;
        visualisedSong.GetComponent<Visualizer>().positiveAverage = true;

        visualisedSong.transform.localPosition = new Vector3(visualizer.transform.localPosition.x + (lastVisWidth / 2f) + (newVisLength / 2f), 0f, 0f);
        VisualiserDestroy(visualizer, visualisedSong.GetComponent<Visualizer>(), music.length / 2f, lastVisWidth / music.length, newVisLength / music.length);

        visualizer = visualisedSong.GetComponent<Visualizer>();
    }

    IEnumerator VisualiserDestroy(Visualizer dVisualizer, Visualizer nVisualizer, float countdown, float dMoveAmount, float nMoveAmount)
    {
        while (countdown > 0f)
        {
            if (swapControl)
            {
                countdown -= Time.deltaTime;
                dVisualizer.transform.localPosition = dVisualizer.transform.localPosition + new Vector3(Time.deltaTime * dMoveAmount, 0f, 0f);
            }
            else
            {
                nVisualizer.transform.localPosition = nVisualizer.transform.localPosition + new Vector3(Time.deltaTime * nMoveAmount, 0f, 0f);
            }
            yield return null;
        }

        GameObject.Destroy(dVisualizer.gameObject);
        swapControl = false;
    }
}
