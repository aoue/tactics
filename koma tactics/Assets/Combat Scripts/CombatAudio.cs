using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAudio : MonoBehaviour
{
    //handles music player in the combat scene.
    //two tracks: 
    // -one for the music track playing
    // -one for sound effects caused by moves/etc
    // -one for the typing sound effect.

    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private AudioSource soundPlayer;
    [SerializeField] private AudioSource typer; //only plays text sound.

    //Play
    public void play_music(AudioClip ac)
    {
        //called from overworld on return from dungeon.       
        if (ac == null) return;
        //Debug.Log("CombatAudio.play_music() called");
        StartCoroutine(fade_to_newTrack(ac));
    }
    public void play_moveSound(AudioClip ac)
    {
        if (ac == null) return;
        soundPlayer.PlayOneShot(ac);
    }
    public void play_typingSound()
    {
        typer.Play();
    }

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




}
