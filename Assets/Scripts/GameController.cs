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
    public AudioImporter audioImporter;

    [Space]
    public Button startButton;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text songTitleText;
    public Slider volumeSlider;
    static float volumeValue = 0.5f;

    [Header("Music")]
    public bool hasMusic = false;
    static public List<AudioClip> music = new List<AudioClip>();
    public int nextSong = 0;
    [Space]
    public List<string> songList = new List<string>();

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
    string songPath = "";

    // Start is called before the first frame update
    void Start()
    {
        audioImporter.Loaded += AssignSong;

        if (music.Count != 0)
        {
            songTitleText.text = music[0].name;
            hasMusic = true;
        }
        else
        {
            int songCount = PlayerPrefs.GetInt("HasMusic", 0);
            if (songCount >= 1)
            {
                for (int k = 1; k < songCount; k++)
                {
                    songPath = PlayerPrefs.GetString("Song" + k);
                    audioImporter.Import(songPath);
                }
            }
            else
            {
                hasMusic = false;
                songTitleText.text = "No song yet";
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
    void FixedUpdate()
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

            if (player.playTime >= player.music.length * 0.5f && !halfTime)
            {
                AddVisualisedSong();
                halfTime = true;
            }
            else if (player.playTime >= player.music.length)
            {
                swapControl = true;
                halfTime = false;
                player.visualizers[0] = visualizer;
                player.playTime = 0f;
                player.music = music[nextSong];
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            player.playSpeed += 0.25f;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            player.playSpeed -= 0.25f;
        }
    }

    public void StartGame()
    {
        songTitleText.text = player.music.name;
        player.music = music[0];

        scoreText.text = score.ToString();
        songTitleText.text = player.music.name;

        gameObjects.SetActive(true);
        gameUI.SetActive(true);
        menuUI.SetActive(false);

        player.playSpeed = musicSpeed;

        player.music = music[0];
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
            songTitleText.text = "Song Load Failed";
            return;
        }
        //StartCoroutine(SongLoader(path));
        songPath = path;
        audioImporter.Import(path);
    }
    string GetSongPath()
    {
        var extensions = new[] {
            new ExtensionFilter("Sound Files", "wav", "mp3"),
            new ExtensionFilter("All Files", "*" ),
        };

        string[] path = StandaloneFileBrowser.OpenFilePanel("Import Song", "", extensions, false);
        if (path.Length < 1) return "";

        return path[0];
    }
    void AssignSong(AudioClip clip)
    {
        music.Add(clip);
        songList.Add(clip.name);
        songTitleText.text = Path.GetFileNameWithoutExtension(songPath);
        hasMusic = true;
        PlayerPrefs.SetInt("HasMusic", songList.Count);
        PlayerPrefs.SetString("Song" + (songList.Count), songPath);
    }
    //IEnumerator SongLoader(string path)
    //{
    //    UnityWebRequest AudioFile = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);
    //    yield return AudioFile.SendWebRequest();
    //    if (AudioFile.isNetworkError)
    //    {
    //        Debug.Log(AudioFile.error);
    //        Debug.Log(path);
    //        songTitle.text = "Song Load Failed";
    //        hasMusic = false;
    //    }
    //    else
    //    {
    //        music = DownloadHandlerAudioClip.GetContent(AudioFile);
    //        music.name = songTitle.text = Path.GetFileNameWithoutExtension(path);
    //        hasMusic = true;
    //        PlayerPrefs.SetInt("HasMusic", 1);
    //        PlayerPrefs.SetString("Song", path);
    //    }
    //}
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
        GameObject newSong = GameObject.Instantiate(visualisedSong);
        float newVisLength;
        newSong.transform.SetParent(gameObjects.transform);
        newSong.name = $"visualisedSong {resetNum + 1}";
        
        
        newSong.GetComponent<Visualizer>().musicPlayer = player;
        newSong.GetComponent<Visualizer>().visualisedSongFolder = newSong.transform;
        newSong.GetComponent<Visualizer>().lineTemplate = visualisedSong.transform.GetChild(0).gameObject;
        newSong.GetComponent<Visualizer>().boxCollider = visualisedSong.GetComponent<BoxCollider2D>();
        newSong.GetComponent<Visualizer>().sequenceWidth = newVisLength = 100f * Random.Range(0.8f, 1.3f);
        newSong.GetComponent<Visualizer>().squareSpacing = 0.1f;
        newSong.GetComponent<Visualizer>().maxHeight = 5f * Random.Range(0.8f, 1.5f);
        newSong.GetComponent<Visualizer>().numberOfSamples = 1000 / Random.Range(1, 3);
        newSong.GetComponent<Visualizer>().averageSpread = 5;
        newSong.GetComponent<Visualizer>().positiveAverage = true;

        nextSong = Random.Range(0, music.Count);
        
        newSong.transform.localPosition = new Vector3(visualizer.transform.localPosition.x + (lastVisWidth / 2f) + (newVisLength / 2f), 0f, 0f);
        StartCoroutine(VisualiserDestroy(visualizer, newSong.GetComponent<Visualizer>(), music[nextSong].length / 2f, lastVisWidth / music[nextSong].length));

        visualizer = newSong.GetComponent<Visualizer>();
        lastVisWidth = newVisLength;
    }

    IEnumerator VisualiserDestroy(Visualizer dVisualizer, Visualizer nVisualizer, float countdown, float moveAmount)
    {
        while (countdown > 0f)
        {
            if (swapControl)
            {
                countdown -= Time.deltaTime;
                dVisualizer.transform.localPosition = dVisualizer.transform.localPosition + new Vector3(Time.deltaTime * -moveAmount * player.playSpeed, 0f, 0f);
            }
            else
            {
                nVisualizer.transform.localPosition = nVisualizer.transform.localPosition + new Vector3(Time.deltaTime * -moveAmount * player.playSpeed, 0f, 0f);
            }
            yield return null;
        }

        GameObject.Destroy(dVisualizer.gameObject);
        swapControl = false;
    }
}
