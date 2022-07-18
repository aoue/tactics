using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private AudioSource soundPlayer;
    [SerializeField] private AudioSource typer; //only plays text sound.

    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private AudioClip passTimeSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip nightMusic;
    [SerializeField] private AudioClip[] musicArr;
    [SerializeField] private AudioClip[] soundArr;

    //set when we go into a dungeon. 
    private AudioClip dungeonTheme; 
    private AudioClip combatTheme;

    //HELPERS
    IEnumerator fade_to_newTrack(AudioClip toPlay, float fadeOutTime = 1.5f)
    {
        //fades out from currently playing track and switches to new track.
        //only for music player - that's the only thing that needs to fade.
        if (musicPlayer.isPlaying)
        {
            float startVolume = musicPlayer.volume;
            while (musicPlayer.volume > 0)
            {
                musicPlayer.volume -= startVolume * Time.deltaTime / fadeOutTime;
                yield return null;
            }
            musicPlayer.Stop();
            musicPlayer.volume = startVolume;
        }       
        musicPlayer.clip = toPlay;       
        musicPlayer.Play();
    }

    //GENERAL MUSIC FUNCTIONS - OUTSIDE INK
    public void play_passTime()
    {
        if (passTimeSound == null) return;
        soundPlayer.PlayOneShot(passTimeSound);
    }
    public void play_moveSound(AudioClip ac)
    {
        if (ac == null)
        {
            //Debug.Log("no move sound");
            return;
        }
        soundPlayer.PlayOneShot(ac);
    }
    public void stop_moveSound()
    {
        soundPlayer.Stop();
    }
    public void play_nightMusic()
    {
        //called from overworld on return from dungeon.
        StartCoroutine(fade_to_newTrack(nightMusic));
    }
    public void play_background_music(AudioClip ac)
    {
        //called by overworld at the start of days.
        if (ac == null) return;
        StartCoroutine(fade_to_newTrack(ac));
    }

    //SPECIFIC MUSIC FUNCTIONS - OUTSIDE INK
    public void play_buttonSound()
    {
        soundPlayer.PlayOneShot(buttonClickSound);
    }
    public void play_typingSound()
    {
        typer.Play();
    }
    public void play_walkingSound()
    {
        soundPlayer.clip = walkingSound;
        soundPlayer.Play();
    }
    public void stop_soundPlayer()
    {
        soundPlayer.Stop();
    }

    //GENERAL MUSIC FUNCTIONS - INSIDE INK
    public void play_loop(int whichTrack)
    {
        //plays indicated track from musicArr. looped.

        //first stop if another track was playing.
        StartCoroutine(fade_to_newTrack(musicArr[whichTrack]));
    }
    public void play_once(int whichTrack)
    {
        //plays indicated sound from soundArr. not looped.
        soundPlayer.PlayOneShot(soundArr[whichTrack]);
    }
    public void stop_playing()
    {
        //stops musicPlayer.
        musicPlayer.Stop();
    }

}
