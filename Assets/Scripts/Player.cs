using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    static bool Playing;
    static bool Paused;
    static bool Stopped;

    public float playTime = 0f;
    public float playSpeed = 1f;

    [Space]
    public AudioClip music;
    public AudioSource audioSource;
    public Visualizer[] visualizers;

    [Space]
    public bool muteOnDrag = true;

    [Space]
    public bool disableControls = false;

    Vector3[] initialPos;
    float[] moveAmounts;
    float lastVolume = 1f;


    // Start is called before the first frame update
    void Start()
    {
        Playing = false;
        Paused = false;
        Stopped = true;

        audioSource.clip = music;

        initialPos = new Vector3[visualizers.Length];
        moveAmounts = new float[visualizers.Length];
        for (int k = 0; k < visualizers.Length; k++)
        {
            initialPos[k] = visualizers[k].transform.localPosition;
            moveAmounts[k] = visualizers[k].sequenceWidth / music.length;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (disableControls)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Stop();
        }

        audioSource.pitch = playSpeed;
    }
    public void Play()
    {
        if (Stopped)
        {
            audioSource.Play();
            Playing = true;
            Stopped = false;
            StartCoroutine(PlayMusic());
            return;
        }
        Playing = true;
        Paused = false;
        Stopped = false;
        audioSource.Play();
    }
    public void Pause()
    {
        Playing = false;
        Paused = true;
        Stopped = false;
        audioSource.Pause();
    }
    public void PlayPause()
    {
        if (Playing)
        {
            Pause();
        }
        else
        {
            Play();
        }
    }
    public void Stop()
    {
        Playing = false;
        Paused = false;
        Stopped = true;
        StopCoroutine(PlayMusic());
        audioSource.Stop();
        playTime = 0f;

        for (int k = 0; k < visualizers.Length; k++)
        {
            visualizers[k].transform.localPosition = initialPos[k];
        }
    }
    IEnumerator PlayMusic()
    {
        do
        {
            if (Playing)
            {
                playTime += Time.deltaTime * playSpeed;
                for (int k = 0; k < visualizers.Length; k++)
                {
                    visualizers[k].transform.localPosition =
                        new Vector3(
                            -playTime * moveAmounts[k],
                            visualizers[k].transform.localPosition.y,
                            visualizers[k].transform.localPosition.z);
                }
            }
            yield return null;
        } while (playTime < music.length);
    }
    public void OnDragStart()
    {
        lastVolume = audioSource.volume;
        if (muteOnDrag)
        { audioSource.volume = 0f; }
    }
    public void OnVisualizerDrag(int index)
    {
        playTime = (visualizers[index].transform.localPosition.x + initialPos[index].x) / visualizers[index].sequenceWidth;
        playTime *= music.length;
        playTime = Mathf.Abs(playTime);

        audioSource.time = playTime;

        int i = 0;
        foreach(Visualizer v in visualizers)
        {
            v.transform.localPosition =
                        new Vector3(
                            -playTime * moveAmounts[i],
                            v.transform.localPosition.y,
                            v.transform.localPosition.z);
            i++;
        }
    }
    public void OnDragEnd()
    {
        if (muteOnDrag)
        { audioSource.volume = lastVolume; }
    }
}
