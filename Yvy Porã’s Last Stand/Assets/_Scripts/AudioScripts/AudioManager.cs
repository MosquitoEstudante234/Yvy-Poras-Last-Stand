using UnityEngine.Audio;
using System;
using UnityEngine;
using Unity.VisualScripting;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;
    public AudioSource Audiogamer;


    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }


        //DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

        }
    }


    public void Update()
    {

    }
    public void ChangeAudiogamer(AudioClip music)
    {
        Audiogamer.Stop();
        Audiogamer.clip = music;
        Audiogamer.Play();
    }

    public void Start()
    {
        // Play("Fase1");
        //AudioManager.instance.Play("Nome");
    }


    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + name + "not found!");
            return;
        }
        s.source.Play();

    }


}
