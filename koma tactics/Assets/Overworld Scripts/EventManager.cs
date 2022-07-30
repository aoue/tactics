using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Ink.Runtime;

public class EventManager : MonoBehaviour
{
    [SerializeField] private Overworld overworld; //link back to the boss.

    [SerializeField] private GameObject shakeObject;
    [SerializeField] private Font ancientsFont;
    [SerializeField] private Font defaultFont;
    [SerializeField] private FadeManager fader;

    [SerializeField] private BackgroundManager allEventBackgrounds; //different background manager from the overworld background manager, but same class.
    [SerializeField] private PortraitLibrary pLibrary;
    
    //dialogue canvas members:
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private CanvasGroup choiceParent;
    [SerializeField] private Image bg;
    [SerializeField] private GameObject nameBox;
    [SerializeField] private Text nameText;
    [SerializeField] private Text sentenceText;
    [SerializeField] private Text centeredText;
    [SerializeField] private Button buttonPrefab = null;
    [SerializeField] private Image speakerBoxPortrait;
    [SerializeField] private Button[] textControlButtons; //in order: auto, skip, history

    //dialogue box hider
    private bool hideAcceptsInput = true; //when true, you can toggle hide. false when transforming.
    private bool hideOn = false; //when true, hide the dialogue box. also, stop the text from advancing.
    [SerializeField] private CanvasGroup diaBoxGroup; //the thing we hide/show.

    //image swap speed
    private float imgFadeSpeed = 1.5f; //higher is faster. controls the speed at which char imgs are replaced/shown/hidden during events.

    //typing speed controllers
    private float textWait = 0.035f; //how many seconds to wait after a non-period character in typesentence
    private float autoWait = 1.75f; //when auto is on, time waited when a sentence is fully written out before playing the next one
    private bool skipOn = false; //when true, don't wait at all between textWaits, just display one after another.
    private bool autoOn = false; //when true, the player can't continue the text, but it will continue automatically.
    private bool historyOn = false; //when true, viewing history and cannot continue the story.
    private bool settingsOn = false; //when true, viewing settings and cannot continue the story.
    private bool usingDefaultFont = true;
    private bool isTalking = true; //determines the mode of speech which is used when nametext is not empty. can either be true:talk [""] or false:think [()] 
    private bool isCentered = false; //determines whether to use sentenceText or use centeredText in typeSentence().

    [SerializeField] private GameObject settingsView; //lets player adjust vn settings, like text speed.

    private string currentSpeakerName; //used for pushing entries in history.
    [SerializeField] private GameObject HistoryPort; //master gameobject for the history interface.
    [SerializeField] private HistoryScroller histScroll; //used to fill/clear the content of the history interface.
    private List<HistoryEntry> historyList; 
    private int historyLimit = 15; //the max number of displays the historyList stores at a time.
    
    [SerializeField] private GameObject canProceedArrow; //visible when canProceed, invisible when cannot.
    private bool canProceed;
    private EventHolder heldEv;
    private Story script;
    [SerializeField] private GameObject[] portraitSlots; //3 total. dimensions are 540 x 1080 | 1 : 2 ratio
    private int[] portraitSlotIDs = new int[3]; //3 total, parallel to portraitSlots. used to save the current id of image in the slot, or -1 if none.

    void Update()
    {
        //toggle text control states:
        //a: auto
        //left ctrl: skip
        //h: history

        if (Input.GetKeyDown(KeyCode.A) && historyOn == false && hideOn == false && settingsOn == false)
        {
            toggle_auto();
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && historyOn == false && hideOn == false && settingsOn == false)
        {
            toggle_skip();
        }
        else if (Input.GetKeyDown(KeyCode.H) && settingsOn == false)
        {
            toggle_history();
        }
        else if (Input.GetKeyDown(KeyCode.Z) && settingsOn == false)
        {
            toggle_hide();
        }
        else if (hideOn == true && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            toggle_hide();
        }

        //press 'spacebar' or 'enter' or 'LeftClick' to continue, only if canProceed if true, we aren't showing history, and we aren't hiding dialogue box
        if (canProceed == true && historyOn == false && settingsOn == false)
        {
            if (skipOn == false && autoOn == false && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            {
                DisplayNextSentence();
            }
            else if (skipOn == true)
            {
                DisplayNextSentence();
            }
            else if (autoOn == true)
            {
                DisplayNextSentence();
            }                    
        }     
    }

    //MANAGE EVENT PREVIEW
    public static void event_hovered(float xCoord, float yCoord)
    {
    }
    public static void event_unhovered()
    {
    }

    //MANAGE EVENT SKIP/AUTO BUTTONS
    IEnumerator modify_diaBox_alpha(bool toTransparent, float speed = 1f)
    {
        //used by hide button to hide/show the dialogue box.
        float objectAlpha = diaBoxGroup.alpha;
        float currentAlpha;
        if (toTransparent) //set alpha to 0
        {
            while (diaBoxGroup.alpha > 0)
            {
                currentAlpha = objectAlpha - (speed * Time.deltaTime);
                objectAlpha = currentAlpha;
                diaBoxGroup.alpha = objectAlpha;
                yield return null;
            }
        }
        else //set alpha to 1
        {
            while (diaBoxGroup.alpha < 1)
            {
                currentAlpha = objectAlpha + (speed * Time.deltaTime);
                objectAlpha = currentAlpha;
                diaBoxGroup.alpha = objectAlpha;
                yield return null;
            }
        }
        hideAcceptsInput = true;
    }
    public void toggle_hide()
    {
        //Debug.Log("toggle hide called");
        if (hideAcceptsInput == false) return;
        hideAcceptsInput = false;
        hideOn = !hideOn;
        //we transition it to the state we want using alpha, though.
        if (hideOn == true) 
        {
            StartCoroutine(modify_diaBox_alpha(true));
        }
        else
        {
            StartCoroutine(modify_diaBox_alpha(false));
        }
    }
    public void toggle_auto()
    {
        autoOn = !autoOn;
        if (autoOn == true)
        {
            textControlButtons[0].image.color = Color.black;
        }
        else
        {
            textControlButtons[0].image.color = Color.grey;
        }
    }
    public void toggle_skip()
    {
        skipOn = !skipOn;
        if (skipOn == true)
        {
            textControlButtons[1].image.color = Color.black;
        }
        else
        {
            textControlButtons[1].image.color = Color.grey;
        }
    }   
    public void toggle_history()
    {
        //use a viewport to view the last ?? sentences.
        //they're all safely stored in historyQueue, a queue of strings.
        historyOn = !historyOn;

        if (historyOn == true)
        {
            textControlButtons[2].image.color = Color.black;
            show_history();
        }
        else
        {
            textControlButtons[2].image.color = Color.grey;
            hide_history();
        }        
    }   
    void show_history()
    {
        if (settingsOn == true) return;

        //fill history port.
        histScroll.show(historyList);
        HistoryPort.SetActive(true);
    }
    void hide_history()
    {
        HistoryPort.SetActive(false);
    }

    //SETTINGS
    public void show_settings()
    {
        if (historyOn == true) return;

        settingsOn = true;
        if (settingsOn == true)
        {
            textControlButtons[3].image.color = Color.black;
            settingsView.SetActive(true);
        }
    }
    public void hide_settings()
    {
        settingsOn = false;
        textControlButtons[3].image.color = Color.grey;
        settingsView.SetActive(false);
    }
    public void set_textWaitTime(System.Single value)
    {
        textWait = value;
        //Debug.Log("set_textWaitTime(). value = " + value + " | textWait = " + textWait);
    }

    //MANAGE EVENT RUNNING
    IEnumerator pause_before_starting_event(float duration = 1f)
    {
        //pause for a minute before starting the event. (so we can)
        yield return new WaitForSeconds(duration);
        setup_event(heldEv.get_story());
    }
    IEnumerator pause_before_ending_event(float duration = 1f)
    {
        //pause for a minute before starting the event. (so we can)
        yield return new WaitForSeconds(duration);
        //hide the dialogue canvas, i.e. return to overworld
        dialogueCanvas.SetActive(false);
        overworld.load_part();
    }    
    IEnumerator TypeSentence(string sentence)
    {
        Text typeHere = null;
        if (isCentered == true) typeHere = centeredText;
        else typeHere = sentenceText;

        if (skipOn == false)
        {
            //hide canProceed arrow
            canProceedArrow.SetActive(false);

            typeHere.text = "";
            string displayString = "";

            yield return new WaitForSeconds(0.05f);

            foreach (char letter in sentence.ToCharArray())
            {
                if (skipOn == true)
                {
                    //i.e., if skip is turned on while the sentence is playing.
                    typeHere.text = sentence;
                    break;
                }

                displayString += letter;

                //control quotes, parentheses, or nothing.
                if (nameText.text == "")
                {
                    typeHere.text = displayString;
                }
                else
                {
                    if (isTalking == true) //use quotes
                    {
                        typeHere.text = "\"" + displayString + "\"";
                    }
                    else //use parantheses
                    {
                        typeHere.text = "(" + displayString + ")";
                    }
                }

                yield return new WaitForSeconds(textWait);
            }
            //show canProceed arrow.
            canProceedArrow.SetActive(true);
        }
        else
        {
            typeHere.text = sentence;
            yield return new WaitForSeconds(0.1f);
        }
        
        if (autoOn == true && skipOn == false)
        {
            yield return new WaitForSeconds(autoWait);
        }
        canProceed = true;
    }

    public void begin_immediate(TextAsset storyText)
    {
        heldEv = null;
        setup_event(storyText);
    }
    public void begin_event(EventHolder ev)
    {
        fader.fade_to_black();
        heldEv = ev;
        StartCoroutine(pause_before_starting_event());
    }   
    void setup_event(TextAsset storyText)
    {
        //setup initial view
        nameText.text = "";
        sentenceText.text = "";

        //reset auto/skip/and history       
        autoOn = false;
        skipOn = false;
        historyOn = false;
        currentSpeakerName = "NO SPEAKER";
        if (historyList == null) historyList = new List<HistoryEntry>();
        else historyList.Clear();
        for (int i = 0; i < textControlButtons.Length; i++)
        {
            textControlButtons[i].interactable = true;
            textControlButtons[i].image.color = Color.grey;
        }

        //setup starting speaker portraits - hide them all.
        for (int i = 0; i < 3; i++)
        {
            portraitSlots[i].gameObject.SetActive(false);
            portraitSlotIDs[i] = -1;
        }
        dialogueCanvas.SetActive(true);

        //setup story
        script = new Story(storyText.text);

        //fill in roles text.
        fill_roles();

        //link
        link_external_functions();

        //enable input; we can start the event now.
        DisplayNextSentence();
    }
    void DisplayNextSentence()
    {
        //delete any remaining choice thingies, if they are there.
        int childCount = choiceParent.transform.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            GameObject.Destroy(choiceParent.transform.GetChild(i).gameObject);
        }

        canProceed = false;
        if ( script.canContinue == true)
        {
            string sentence = script.Continue().Trim();

            //if sentence is blank; then don't type sentence on it. call DisplayNextSentence again.
            if ( sentence.Length > 0)
            {
                //add name, sentence pair to history
                HistoryEntry entry = new HistoryEntry(currentSpeakerName, sentence);
                if (historyList.Count == historyLimit) //history limit is here.
                {
                    historyList.RemoveAt(0);
                }
                historyList.Add(entry);

                StartCoroutine(TypeSentence(sentence));
            }
            else
            {
                DisplayNextSentence();
            }
        }
        else if (script.currentChoices.Count > 0)
        {
            //choices setup            
            for (int i = 0; i < script.currentChoices.Count; i++)
            {
                Choice choice = script.currentChoices[i];
                Button button = CreateChoiceView(choice.text.Trim());
                button.onClick.AddListener(delegate { OnClickChoiceButton(choice); });
            }
        }
        else
        {
            //end of story.
            end_event();   
        }
    }   
    void end_event()
    {
        //do post event stuff
        if (heldEv != null)
        {
            heldEv.post_event();
            overworld.set_progression(heldEv.modify_day_progression(overworld.get_progression()));
        }
            

        //start a fade
        fader.fade_from_black_cheat();
        dialogueCanvas.SetActive(false);
        overworld.load_part();
    }


    //Helpers
    void recalibrate_portrait_positions()
    {
        //called when a portrait is hidden or called. we recalibrate the remaining portraits
        //so they're in the center in a nice way.

        //first, count how many portraits are still showing.
        int showing = 0;
        foreach(GameObject go in portraitSlots)
        {
            if (go.activeSelf == true) showing++;
        }

        List<GameObject> showingList = new List<GameObject>();
        for (int i = 0; i < portraitSlots.Length; i++)
        {
            if (portraitSlots[i].activeSelf == true) showingList.Add(portraitSlots[i]);
        }

        //position portrait slots based on number of portraits.
        switch (showing)
        {
            case 1:
                showingList[0].transform.localPosition = new Vector2(0f, showingList[0].transform.localPosition.y);
                break;
            case 2:
                showingList[0].transform.localPosition = new Vector2(-410f, showingList[0].transform.localPosition.y);
                showingList[1].transform.localPosition = new Vector2(410f, showingList[1].transform.localPosition.y);

                break;
            case 3:
                showingList[0].transform.localPosition = new Vector2(-615f, showingList[0].transform.localPosition.y);
                showingList[1].transform.localPosition = new Vector2(0f, showingList[1].transform.localPosition.y);
                showingList[2].transform.localPosition = new Vector2(615f, showingList[2].transform.localPosition.y);
                break;
        }
    }
    void fill_roles()
    {
        //return all effects to default.
        sentenceText.font = defaultFont; //use normal font
        set_name(""); //hide name box
        set_boxPortrait(-1); //hide box portrait
        set_speech(true); //use quotes for speaker
        set_centered(false); //do not show text in centered.


    }
    Button CreateChoiceView(string text)
    {
        Button choice = Instantiate(buttonPrefab) as Button;
        choice.transform.SetParent(choiceParent.transform, false);

        // Gets the text from the button prefab
        Text choiceText = choice.GetComponentInChildren<Text>();
        choiceText.text = text;

        return choice;
    }
    void OnClickChoiceButton(Choice choice)
    {
        script.ChooseChoiceIndex(choice.index);
        DisplayNextSentence();
    }

    //LINKING EXTERNAL FUNCTIONS
    void link_external_functions()
    {
        //all the storys share the same external functions. we are externalizing complexity from 
        //ink and putting it here instead.

        //music


        //visuals
        script.BindExternalFunction("bg", (int id) => 
        {
            this.set_bg(id);
        });
        script.BindExternalFunction("n", (string name) =>
        {
            this.set_name(name);
        });
        script.BindExternalFunction("p", (int pId) =>
        {
            this.set_boxPortrait(pId);
        });
        script.BindExternalFunction("talk", (bool mode) =>
        {
            this.set_speech(mode);
        });
        script.BindExternalFunction("center", (bool mode) =>
        {
            this.set_centered(mode);
        });
        script.BindExternalFunction("toggle_font", () =>
        {
            this.toggle_font();
        });
        script.BindExternalFunction("show", (int which, int index) =>
        {
            this.set_portrait_slot(which, index);
        });
        script.BindExternalFunction("hide", (int which) =>
        {
            this.hide_portrait_slot(which);
        });
        script.BindExternalFunction("shake", (int intensity, float duration) =>
        {
            this.camera_shake(intensity, duration);
        });

        //game (e.g. rel increased, set flag, etc.)

    }

    //text effects
    void set_boxPortrait(int speakerBoxId)
    {
        if (speakerBoxId == -1)
        {
            speakerBoxPortrait.gameObject.SetActive(false);
        }
        else
        {
            speakerBoxPortrait.gameObject.SetActive(true);
            speakerBoxPortrait.sprite = pLibrary.retrieve_speakerp(speakerBoxId);
        }
    }
    void set_name(string s)
    {
        if ( s == "" )
        {
            nameText.text = "";
            nameBox.SetActive(false);
        }
        else
        {
            nameBox.SetActive(true);
            nameText.text = s;
        }
        currentSpeakerName = s;
    }
    void set_speech(bool state)
    {
        isTalking = state;
    }
    void set_centered(bool state)
    {
        isCentered = state;
    }
    void toggle_font()
    {
        if (usingDefaultFont == true)
        {
            sentenceText.font = ancientsFont;
        }
        else
        {
            sentenceText.font = defaultFont;
        }
        usingDefaultFont = !usingDefaultFont;
    }

    //visual effects
    void camera_shake(int intensity, float duration)
    {
        // -currently not working.
        //intensity determines how much the camera shakes
        //duration determines how long the shake lasts.
        //at the end of the time elapsed, returns back to normal.
        StartCoroutine(trigger_camera_shake(intensity, duration));
    }
    IEnumerator trigger_camera_shake(int intensity, float duration)
    {
        //save initial position.
        Vector3 initial_position = shakeObject.transform.localPosition;

        //zoom in so we don't see the edges fraying
        shakeObject.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);

        //shake
        while (duration > 0f)
        {
            shakeObject.transform.localPosition = initial_position + (Random.insideUnitSphere * intensity);
            duration -= Time.deltaTime;
            yield return null;
        }
        //reset initial position
        shakeObject.transform.localPosition = initial_position;
        shakeObject.transform.localScale = new Vector3(1f, 1f, 1f);
    }
    void set_portrait_slot(int whichSlot, int index)
    {
        //if skip is on, then show directly.
        //two methods: 
        //1. if already showing an image: fade to half alpha, switch image, fade back to full alpha.
        //2. if not showing an image: set half alpha, switch image, show image, fade to full alpha.


        //only do the fade if:
        // - we're not skipping and
        // - the slot is empty OR the slot is same char as current slot
        //(current slot means: divide both by 100 and they give the same answer)
        if (skipOn == false && (portraitSlotIDs[whichSlot] == -1 || index / 100 != portraitSlotIDs[whichSlot] / 100) )
        {
            StartCoroutine(handle_image_switch_fade(imgFadeSpeed, portraitSlots[whichSlot].activeSelf, whichSlot, pLibrary.retrieve_fullp(index)));
        }
        else
        {
            portraitSlots[whichSlot].GetComponent<Image>().sprite = pLibrary.retrieve_fullp(index);
            portraitSlots[whichSlot].SetActive(true);
            recalibrate_portrait_positions();
        }
        portraitSlotIDs[whichSlot] = index;

    }
    void hide_portrait_slot(int whichSlot)
    {
        portraitSlotIDs[whichSlot] = -1;
        StartCoroutine(handle_image_hide_fade(imgFadeSpeed, whichSlot));
        recalibrate_portrait_positions();
    }
    void set_bg(int id)
    {
        if (skipOn == false) fader.fade_to_black(); //automatic fading behaviour
        bg.sprite = pLibrary.retrieve_eventBg(id);
    }
    IEnumerator handle_image_switch_fade(float speed, bool fadeOutFirst, int whichSlot, Sprite switchSprite)
    {
        Color objectColor = portraitSlots[whichSlot].GetComponent<Image>().color;
        float fadeAmount;

        //if fadeOutFirst is true, then first fade the image to half alpha, then assign switchSprite to portraitSlots[whichSlot].
        if (fadeOutFirst == true)
        {
            speed *= 2; //makes each img switch faster, so it matches the same total duration as the other method.
            while ( objectColor.a > 0.25f )
            {
                fadeAmount = objectColor.a - (speed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                portraitSlots[whichSlot].GetComponent<Image>().color = objectColor;
                yield return null;
            }
        }
        else
        {
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, 0.25f);
            portraitSlots[whichSlot].SetActive(true);
        }
        portraitSlots[whichSlot].GetComponent<Image>().sprite = switchSprite;

        //fade the image back to full alpha.
        while ( objectColor.a < 1f )
        {
            fadeAmount = objectColor.a + (speed * Time.deltaTime);

            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            portraitSlots[whichSlot].GetComponent<Image>().color = objectColor;
            yield return null;
        }

    }
    IEnumerator handle_image_hide_fade(float speed, int whichSlot)
    {
        Color objectColor = portraitSlots[whichSlot].GetComponent<Image>().color;
        float fadeAmount;
        while (objectColor.a > 0.25f)
        {
            fadeAmount = objectColor.a - (speed * 2 * Time.deltaTime);

            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            portraitSlots[whichSlot].GetComponent<Image>().color = objectColor;
            yield return null;
        }
        portraitSlots[whichSlot].gameObject.SetActive(false);
    }

    //sound effects

    
}
