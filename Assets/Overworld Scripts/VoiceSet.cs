using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceSet : MonoBehaviour
{
    // Holds all the voice clips for a given character.
    // called by voice manager.

    [SerializeField] private AudioClip exampleSound;
    // [SerializeField] private AudioClip[] sounds; 

    public AudioClip get_sound(string mood)
    {
        // in future, switch based on mood to find a random sound that matches the mood.
        // but for now, just return the character's basic sound, i guess.

        return exampleSound;
    }
}
