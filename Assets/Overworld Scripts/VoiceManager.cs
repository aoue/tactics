using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    // Holds and returns voice clips for the EventManager.
    // Takes in a label of the format [character]-[mood]
    //  e.g. friday-happy
    // and then randomly returns one voice clip that is tagged that way.

    // structure:
    // has a dictionary binding labels to functions
    // has lists to match each character mood.

    [SerializeField] private AudioSource voicePlayer;
    [SerializeField] private VoiceSet[] characterVoiceSets;

    

    public void play_voice(string label)
    {
        // get the voice from the corresponding voiceset + part 2 of the label
        AudioClip v = get_voice(label);

        // play the voiceclip on the voice player.
        voicePlayer.PlayOneShot(v);
    }

    private AudioClip get_voice(string label)
    {
        // split label on '-'
        // first part tells us which character; switch statement
        // second part is passed in to the character's voice set
        string[] labelParts = label.Split('-');
        string character = labelParts[0];
        string mood = labelParts[1];
        switch(character)
        {
            case "friday":
                return characterVoiceSets[1].get_sound(mood);
            case "anse":
                return characterVoiceSets[2].get_sound(mood);
            default:
                return characterVoiceSets[0].get_sound(mood);
        }


        
    }

}
