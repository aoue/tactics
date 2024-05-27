using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class CombatDialoguer : MonoBehaviour
{
    //responsible for handling combat dialogue events.
    // either at the start of the mission, or start of rounds. 

    //constant label keys:
    // 0: start of mission.
    // (-2): end of mission; victory
    // (-3): end of mission; defeat
    // otherwise, use the round number, starting at 0.

    [SerializeField] private Canvas dialoguerCanvas; //master. We need it to return control to it later.
    [SerializeField] private CombatGrid cGrid; //master. We need it to return control to it later.
    [SerializeField] private CameraController cammy; //ref to to the combat scene's camera. For jumping.
    [SerializeField] private PortraitLibrary pLibrary; //

    // [SerializeField] private GameObject nameBox;
    // [SerializeField] private Text nameText;

    [SerializeField] private Text sentenceText;
    [SerializeField] private Image cgFrame; //an image frame that shows up in the middle of the screen. 
    [SerializeField] private Image portrait; //for showing the speaker's (box) portrait.
    private float textWait = 0.02f;
    private bool fastOn = false;

    private Story script;
    private bool canProceed;
    private int label;

    void Update()
    {
        //use spacebar to continue.
        if (canProceed && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || fastOn))
        {
            canProceed = false;
            displayNextSentence();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            fastOn = !fastOn;
        }
    }

    public void play_event(TextAsset storyText, int labelIndex)
    {
        //plays the scene matching labelIndex in story.
        cammy.lock_camera();

        //hide unit informer, tile informer, and active portrait
        cGrid.hide_informers();
        fastOn = false;
       
        script = new Story(storyText.text);
        script.ResetState();
        label = labelIndex;
        script.variablesState["label"] = labelIndex;
        //Debug.Log("label = " + script.variablesState["label"]);

        link_external_functions();

        //set portrait, cgframe to 0. (not name, there must always be a name in this dialogues.)
        portrait.gameObject.SetActive(true);
        cgFrame.gameObject.SetActive(false);

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
            if (fastOn)
            {
                sentenceText.text = sentence;
                yield return new WaitForSeconds(0.05f);
                break;
            }

            displayString += letter;
            sentenceText.text = displayString;
            yield return new WaitForSeconds(textWait);
        }
        if (!fastOn) { cGrid.play_typing(); }
        
        canProceed = true;
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
            case -2:
                //win.
                cGrid.show_mission_summary(true);
                break;
            case -3:
                //loss.
                cGrid.show_mission_summary(false);
                break;
            default:
                //normal events
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
        script.BindExternalFunction("cg", (int which) =>
        {
            this.set_cg(which);
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
        return; // disabled
        //sets name.
        // if (name == "") nameBox.gameObject.SetActive(false);
        // else
        // {
        //     nameText.text = name;
        //     nameBox.gameObject.SetActive(true);
        // }
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
    void set_cg(int which)
    {
        if (which == -1)
        {
            cgFrame.enabled = false;
        }
        else
        {
            cgFrame.sprite = pLibrary.retrieve_combatImage(which);
            cgFrame.enabled = true;
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
