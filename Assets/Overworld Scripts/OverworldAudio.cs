using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldAudio : CombatAudio
{
    //controls audio for the overworld scene.
    //plays music, sounds, and the typing beep.

    //the overworld audio is also the audio library. it has all the music tracks and sound effects held.

    [SerializeField] private AudioClip[] musicList;
    [SerializeField] private AudioClip[] soundList;

    public void ow_play_music(int index)
    {
        if (index == -1)
        {
            stop_music();
            return;
        }
        play_music(musicList[index]);
    }
    public void ow_play_sound(int index)
    {
        play_sound(soundList[index]);
    }

}
