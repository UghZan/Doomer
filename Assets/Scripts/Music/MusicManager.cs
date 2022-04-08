using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField]  AudioClip[] music;
    [SerializeField]  AudioClip[] bossMusic;
    public AudioClip nextMusic;
    [SerializeField] AudioSource source;
    public bool interWave;
    public bool shouldChange; //locks music change, in case boss music or smth else
    public bool setNext; //if next music shouldn't change
    [SerializeField]  float changeSpeed;
    [SerializeField]  float timeInBetween;
    public static bool start = false;
    [SerializeField] PlayerStats ps;
    float time = 0;
    float dopamineMultiplier; //lower dopamine - slightly slower and quieter music
    void Start()
    {
        nextMusic = GetRandomMusic();
    }
    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            dopamineMultiplier = Mathf.Clamp(1 + ps.dopamineLevel/20, 0.9f ,1.1f);
            source.pitch = Mathf.Lerp(source.pitch, interWave ? 0.5f : dopamineMultiplier, Time.deltaTime * changeSpeed);
            source.volume = Mathf.Lerp(source.volume, interWave ? 0.5f : dopamineMultiplier, Time.deltaTime * changeSpeed);
            if (!source.isPlaying)
            {
                time += Time.deltaTime;
                if (time > timeInBetween && shouldChange)
                {
                    SwitchMusic();
                    time = 0;
                }
            }
            else
            {
                time = 0;
            }
        }
    }

    void SwitchMusic()
    {
        if(!setNext) nextMusic = GetRandomMusic();
        else setNext = false;
        source.PlayOneShot(nextMusic);
    }

    public void StopCurrent()
    {
        source.Stop();
        time = 0;
    }

    public AudioClip GetRandomMusic()
    {
        return music[Random.Range(0, music.Length)];
    }
    public AudioClip GetBossMusic(int index)
    {
        if(index == -1)
            return bossMusic[Random.Range(0, music.Length)];
        else return bossMusic[index];
    }
}
