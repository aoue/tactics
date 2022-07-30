using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class CombatDialoguer : MonoBehaviour
{
    //responsible for handling combat dialogue events.
    //controls:
    // -jumping to labels in missions at the appropriate time
    // -controlling text, display, and ui

    //linked functions:
    // -play music (todo)
    // -set name (implemented)
    // -set mini box portrait (todo)
    // -jump camera (todo)
    // -slide camera (todo)

    //constant label keys:
    // 0: start of mission.
    // (-2): end of mission; victory
    // (-3): end of mission; defeat
    // otherwise, use the round number, starting at 0.

    [SerializeField] private Canvas dialoguerCanvas; //master. We need it to return control to it later.
    [SerializeField] private CombatGrid cGrid; //master. We need it to return control to it later.
    [SerializeField] private CameraController cammy; //ref to to the combat scene's camera. For jumping.
    [SerializeField] private PortraitLibrary pLibrary; //

    [SerializeField] private Text nameText;
    [SerializeField] private Text sentenceText;
    [SerializeField] private Image portrait;
    private float textWait = 0.02f;

    private Story script;
    private bool canProceed;
    private int label;

    void Update()
    {
        //use spacebar to continue.
        if (canProceed && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButton(0)))
        {
            canProceed = false;
            displayNextSentence();
        }
    }

    bool linked = false;
    public void play_event(TextAsset storyText, int labelIndex)
    {
        //plays the scene matching labelIndex in story.
        cammy.lock_camera();

        //hide unit informer, tile informer, and active portrait
        cGrid.hide_informers();
       
        script = new Story(storyText.text);
        script.ResetState();
        label = labelIndex;
        script.variablesState["label"] = labelIndex;
        //Debug.Log("label = " + script.variablesState["label"]);

        link_external_functions();
               
        dialoguerCanvas.enabled = true;
        displayNextSentence();
    }
    void displayNextSentence()
    {
        //displays the next sentence in the story, or, if the story has ended, calls end_story().
        canProceed = false;
        if (script.canContinue == true)
        {
            string sentence = script.Continue().Trim();

            //if sentence is blank; then don't type sentence on it. call DisplayNextSentence again.
            if (sentence.Length > 0)
            {
                StartCoroutine(typeSentence(sentence));
            }
            else
            {
                cGrid.play_typing();
                displayNextSentence();
            }
        }
        else
        {
            end_story();
        }
    }
    IEnumerator typeSentence(string sentence)
    {
        string displayString = "";
        foreach(char letter in sentence.ToCharArray())
        {
            displayString += letter;
            sentenceText.text = "\"" + displayString + "\"";
            yield return new WaitForSeconds(textWait);
        }
        canProceed = true;
        cGrid.play_typing();
    }

    void end_story()
    {
        //clear ui, back to:
        //if start of mission or start of round event, then enable gameplay.
        //otherwise, it's the end of the mission and we should load back away.
        dialoguerCanvas.enabled = false;

        //call next_turn()
        
        switch (label)
        {
            /*
            case 0:
                //beginning of mission: send the player back, and ensure the round still hasn't started yet.
                //they need the chance to deploy their units.
                cammy.unlock_camera();
                cGrid.post_mission_begin_dialogue();
                break;
            */
            case -2:
                //win.
                cGrid.end_mission_win();
                break;
            case -3:
                //loss.
                cGrid.end_mission_loss();               
                break;
            default:
                cammy.unlock_camera();
                cGrid.post_mission_begin_dialogue();
                break;
        }
    }

    //linked functions
    void link_external_functions()
    {
        script.BindExternalFunction("play_music", (int which) =>
        {
            this.play_music(which);
        });
        script.BindExternalFunction("n", (string name) =>
        {
            this.set_name(name);
        });
        script.BindExternalFunction("p", (int which) =>
        {
            this.set_portrait(which);
        });
        script.BindExternalFunction("jump", (int x, int y) =>
        {
            this.jump_camera(x, y);
        });
        script.BindExternalFunction("slide", (int x, int y) =>
        {
            this.slide_camera(x, y);
        });
    }
    void play_music(int which)
    {
        //sets music and starts playing it.
        cGrid.play_music_track(which);
    }
    void set_name(string name)
    {
        //sets name.
        nameText.text = name;
    }
    void set_portrait(int which)
    {
        //sets the single portrait slot to the corresponding image.
        //-1 to hide.

        if (which == -1)
        {
            portrait.enabled = false;
        }
        else
        {
            portrait.sprite = pLibrary.retrieve_boxp(which);
            portrait.enabled = true;
        }

    }
    void jump_camera(int x, int y)
    {
        //jumps camera to the corresponding coordinates
        Vector3 dest = cGrid.get_pos_from_coords(x, y) + new Vector3(0f, 0f, -10f);
        cammy.jump_to(dest);
    }
    void slide_camera(int x, int y)
    {
        Vector3 dest = cGrid.get_pos_from_coords(x, y) + new Vector3(0f, 0f, -10f);
        cammy.slide_to(dest, x, y);
    }
    

    

    

}
