using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Ink.Runtime;
using System;
using TMPro;

public class EventManager : MonoBehaviour
{
    [SerializeField] private Overworld overworld; //link back to the boss.
    [SerializeField] private OverworldAudio audio;
    [SerializeField] private FadeManager fader;
    [SerializeField] private PortraitLibrary pLibrary;
    [SerializeField] private ParticleSystem snowSystem;
    [SerializeField] private ParticleSystem rainSystem;
    [SerializeField] private ParticleSystem windSystem;
    [SerializeField] private Canvas characterCanvas; //need it to adjust layer for weather, overlay.

    //dialogue canvas members:
    [SerializeField] private GameObject eventObjects;
    [SerializeField] private CanvasGroup choiceParent;
    [SerializeField] private Image bg;
    [SerializeField] private Image overlay;
    [SerializeField] private GameObject NormalDialogueBox;
    [SerializeField] private GameObject nameBox;
    [SerializeField] private Text nameText;
    [SerializeField] private Text sentenceText;
    [SerializeField] private Text centeredText;
    [SerializeField] private Button buttonPrefab = null;
    [SerializeField] private Image speakerBoxPortrait;
    [SerializeField] private Button[] textControlButtons; //in order: auto, skip, history
    [SerializeField] private Image msgPopupImage;
    [SerializeField] private TextMeshProUGUI msgPopupText;

    //[SerializeField] private Material defaultMaterial;
    [SerializeField] private Material holoMaterial;
    [SerializeField] private Font defaultFont;
    [SerializeField] private Font robotFont;

    //dialogue box hider
    private bool hideAcceptsInput = true; //when true, you can toggle hide. false when transforming.
    private bool hideOn = false; //when true, hide the dialogue box. also, stop the text from advancing.
    [SerializeField] private CanvasGroup diaBoxGroup; //the thing we hide/show.

    //image swap speed
    private float imgFadeSpeed = 1.5f; //higher is faster. controls the speed at which char imgs are replaced/shown/hidden during events.

    //typing speed controllers
    private float textWait = 0.035f; //how many seconds to wait after a non-period character in typesentence
    private float periodWait = 0.35f; //how many seconds to wait after a period character in typesentence
    private float autoWait = 1.75f; //when auto is on, time waited when a sentence is fully written out before playing the next one  
    private bool skipOn = false; //when true, don't wait at all between textWaits, just display one after another.
    private bool autoOn = false; //when true, the player can't continue the text, but it will continue automatically.
    private bool historyOn = false; //when true, viewing history and cannot continue the story.
    private bool settingsOn = false; //when true, viewing settings and cannot continue the story.
    private bool isCentered = false; //determines whether to use sentenceText or use centeredText in typeSentence().

    [SerializeField] private GameObject settingsView; //lets player adjust vn settings, like text speed.
    [SerializeField] private Slider programGraphic;
    [SerializeField] private Text programGraphicText;

    private string currentSpeakerName; //used for pushing entries in history.
    [SerializeField] private GameObject HistoryPort; //master gameobject for the history interface.
    [SerializeField] private HistoryScroller histScroll; //used to fill/clear the content of the history interface.
    private List<HistoryEntry> historyList; 
    private int historyLimit = 15; //the max number of displays the historyList stores at a time.
    
    [SerializeField] private GameObject canProceedArrow; //visible when canProceed, invisible when cannot.
    private bool canProceed;
    private bool effectsOver; //block progress while effects are under way
    private EventHolder heldEv;
    private Story script;
    [SerializeField] private GameObject[] portraitSlots; //6 total.

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
        eventObjects.SetActive(false);
        overworld.load_part();
    }    
    IEnumerator TypeSentence(string sentence)
    {
        canProceedArrow.SetActive(false);
        while (!effectsOver)
        {
            yield return null;
        }
        
        if (isCentered)
        {
            //in this case, then we add to the text until it gets to a certain length, at which point we clear it and begin again.
            //(only clear and begin again at the start of a sentence, not during.)

            //check whether to clear centered text. Clear if either there are X newline characters or total length is greater than Y.
            if ( centeredText.text.Length - centeredText.text.Replace("\n", "").Length > 10 || centeredText.text.Length > 700)
            {
                centeredText.text = "";
            }
            string saveString  = centeredText.text;

            if (skipOn)
            {
                centeredText.text = saveString + sentence;
            }
            else
            {
                yield return new WaitForSeconds(0.05f);

                string displayString = "";
                for(int i = 0; i < sentence.Length; i++)
                {
                    if (skipOn == true)
                    {
                        //i.e., if skip is turned on while the sentence is playing.
                        centeredText.text = saveString + sentence;
                        break;
                    }
                    displayString += sentence[i];
                    centeredText.text = saveString + displayString;
                    if (i < sentence.Length - 1 && (sentence[i] == '.' || sentence[i] == '!' || sentence[i] == '?')) yield return new WaitForSeconds(periodWait);
                    else yield return new WaitForSeconds(textWait);
                }               
            }
            centeredText.text += "\n";
        }
        else
        {
            string saveString;
            if (sentence[0] == '>')
            {
                //saveString = sentenceText.text.Replace("\"", ""); // save previous sentence and remove the now-extra set of quotations
                saveString = sentenceText.text;
                sentence = sentence.Substring(1, sentence.Length - 1);
            }
            else
            {
                saveString = "";
                sentenceText.text = "";
            }
           
            if (skipOn)
            {
                sentenceText.text = saveString + sentence;
            }
            else
            {                              
                
                yield return new WaitForSeconds(0.05f);

                string displayString = saveString;
                for(int i = 0; i < sentence.Length; i++)
                {
                    if (skipOn == true)
                    {
                        //i.e., if skip is turned on while the sentence is playing.
                        sentenceText.text = saveString + sentence;
                        break;
                    }
                    displayString += sentence[i];
                    //control quotes, parentheses, or nothing.
                    if (nameText.text == "")
                    {
                        sentenceText.text = displayString;
                    }
                    else
                    {
                        //sentenceText.text = "\"" + displayString + "\"";
                        sentenceText.text = displayString;
                    }          
                    if (i < sentence.Length - 1 && (sentence[i] == '.' || sentence[i] == '!' || sentence[i] == '?')) yield return new WaitForSeconds(periodWait);
                    else yield return new WaitForSeconds(textWait);
                }
            }

        }
        if (!skipOn) audio.play_typingSound();
        yield return new WaitForSeconds(0.05f);
        
        if (autoOn == true && skipOn == false)
        {
            yield return new WaitForSeconds(autoWait);
        }
        canProceed = true;
        canProceedArrow.SetActive(true);
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

        //setup starting speaker portraits - that is, hide them all.
        for (int i = 0; i < 5; i++)
        {
            portraitSlots[i].gameObject.SetActive(false);
        }
        eventObjects.SetActive(true);

        //setup story
        script = new Story(storyText.text);

        //initialize vn elements, like text color, name, etc
        init_vn_elems();
        

        //link
        link_external_functions();

        //enable input; we can start the event now.
        effectsOver = true;
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
            overworld.set_progression(heldEv.modify_day_progression(overworld.get_progression()));
            heldEv.set_completed();
        }
        
        //start a fade
        fader.fade_from_black_cheat();
        eventObjects.SetActive(false);
        overworld.load_part();
    }

    //Helpers
    void init_vn_elems()
    {
        //return all effects to default.
        set_colour("white");
        set_name(""); //hide name box
        set_boxPortrait(-1); //hide box portrait
        set_centered(false); //do not show text in centered.
        centeredText.text = "";
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
        script.BindExternalFunction("stop_music", () =>
        {
            this.stop_music();
        });
        script.BindExternalFunction("play_music", (int id) =>
        {
            this.start_music(id);
        });
        script.BindExternalFunction("play_sound", (int id) =>
        {
            this.play_sound(id);
        });
        

        //visuals
         script.BindExternalFunction("font", (int state) => 
        {
            this.change_font(state);
        });
        script.BindExternalFunction("inside", (int wait) => 
        {
            this.inside(wait);
        });
        script.BindExternalFunction("outside", (int wait) => 
        {
            this.outside(wait);
        });
        script.BindExternalFunction("imm_bg", (int id) => 
        {
            this.imm_bg(id);
        });
        script.BindExternalFunction("bg", (int id) => 
        {
            this.set_bg(id);
        });
        script.BindExternalFunction("overlay", (int id) => 
        {
            this.set_overlay(id);
        });
        script.BindExternalFunction("snow", (int strength) => 
        {
            this.set_snow(strength);
        });
        script.BindExternalFunction("rain", (int strength) => 
        {
            this.set_rain(strength);
        });
        script.BindExternalFunction("wind", (int strength) => 
        {
            this.set_wind(strength);
        });
        script.BindExternalFunction("v_wiggle", (int id, float power, int repeats) => 
        {
            this.v_wiggle(id, power, repeats);
        });
        script.BindExternalFunction("h_wiggle", (int id, float power, int repeats) => 
        {
            this.h_wiggle(id, power, repeats);
        });
        
        script.BindExternalFunction("n", (string name) =>
        {
            this.set_name(name);
        });
        script.BindExternalFunction("c", (string colour) =>
        {
            this.set_colour(colour);
        });
        script.BindExternalFunction("p", (int pId) =>
        {
            this.set_boxPortrait(pId);
        });
        script.BindExternalFunction("p_holo", (int state) =>
        {
            this.set_boxHolo(state);
        });
        script.BindExternalFunction("center", (bool mode) =>
        {
            this.set_centered(mode);
        });
        script.BindExternalFunction("show", (int which, int index) =>
        {
            this.set_portrait_slot(which, index);
        });
        script.BindExternalFunction("holo", (int which, int state) =>
        {
            this.set_portrait_holo(which, state);
        });
        script.BindExternalFunction("speaker", (int which, int state) =>
        {
            this.set_speaker_glow(which, state);
        });
        script.BindExternalFunction("hide", (int which) =>
        {
            this.hide_portrait_slot(which);
        });
        script.BindExternalFunction("shake", (int intensity, float duration) =>
        {
            this.camera_shake(intensity, duration);
        });
        script.BindExternalFunction("msg_popup", (string name) =>
        {
            this.msg_popup(name);
        });
        script.BindExternalFunction("program", (string name, float duration) =>
        {
            this.run_program(name, duration);
        });

        //game (e.g. rel increased, set flag, etc.)
        script.BindExternalFunction("unit_state", (int unit_id, int val) =>
        {
            this.set_unit_state(unit_id, val);
        });
        script.BindExternalFunction("inc_stat", (int unit_id, int stat_id, float val) =>
        {
            this.inc_unit_stat(unit_id, stat_id, val);
        });
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
            speakerBoxPortrait.sprite = pLibrary.retrieve_boxp(speakerBoxId);
        }
    }
    void set_boxHolo(int state)
    {
        if (state == -1) speakerBoxPortrait.material = null;
        else speakerBoxPortrait.material = holoMaterial;
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

        // also set colour.
        set_colour(s);
    }
    void set_colour(string c)
    {
        switch(c)
        { 
            //r, g, b
            case "red":
                nameText.color = new Color(255f/255f, 0f, 0f);
                sentenceText.color = new Color(1f, 102f/255f, 102f/255f); break;
            case "orange":
                nameText.color = new Color(255f/255f, 128f/255f, 0f);
                sentenceText.color = new Color(255f/255f, 178f/255f, 102f/255f); break;
            case "yellow":
                nameText.color = new Color(255f/255f, 255f/255f, 51f/255f);
                sentenceText.color = new Color(1f, 1f, 153f/255f); break;
            case "green": case "Anse":
                nameText.color = new Color(51f/255f, 255f/255f, 51f/255f);
                sentenceText.color = new Color(153f/255f, 1f, 153f/255f); break;
            case "cyan":
                nameText.color = new Color(51f/255f, 255f/255f, 255f/255f);
                sentenceText.color = new Color(153f/255f, 1f, 1f); break;
            case "blue":
                nameText.color = new Color(51f/255f, 153f/255f, 255f/255f);
                sentenceText.color = new Color(153f/255f, 204f/255f, 255f/255f); break;
            case "purple": case "Friday":
                nameText.color = new Color(180f/255f, 105f/255f, 255f/255f);
                sentenceText.color = new Color(204f/255f, 153f/255f, 1f); break;
            case "pink":
                nameText.color = new Color(255f/255f, 51f/255f, 153f/255f);
                sentenceText.color = new Color(1f, 153f/255f, 204f/255f); break;
            case "grey":
                nameText.color = new Color(160f/255f, 160f/255f, 160f/255f);
                sentenceText.color = new Color(192f/255f, 192f/255f, 192f/255f); break;
            case "": default: //i.e. white
                nameText.color = new Color(224f/255f, 224f/255f, 224f/255f);
                sentenceText.color = new Color(1f, 1f, 1f); break;
        }
        
    }
    void set_centered(bool state)
    {
        isCentered = state;

        if (isCentered)
        {
            //enable centered text and disable the sentence text
            centeredText.transform.parent.gameObject.SetActive(true);
            NormalDialogueBox.SetActive(false);
        }
        else
        {
            //disable centered text and enable sentence text
            centeredText.transform.parent.gameObject.SetActive(false);
            NormalDialogueBox.SetActive(true);
        }
    }
    void change_font(int state)
    {
        if (state == -1) sentenceText.font = defaultFont;
        else sentenceText.font = robotFont;
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
        Debug.Log("EventManager.trigger_camera_shake() called.");
        //save initial position.
        Vector3 initial_position = eventObjects.transform.localPosition;

        //zoom in so we don't see the edges fraying
        eventObjects.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);

        //shake
        while (duration > 0f)
        {
            eventObjects.transform.localPosition = initial_position + (UnityEngine.Random.insideUnitSphere * intensity);
            duration -= Time.deltaTime;
            yield return null;
        }
        //reset initial position
        eventObjects.transform.localPosition = initial_position;
        eventObjects.transform.localScale = new Vector3(1f, 1f, 1f);
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
        if (skipOn == false)
        {
            StartCoroutine(handle_image_switch_fade(imgFadeSpeed, portraitSlots[whichSlot].activeSelf, whichSlot, pLibrary.retrieve_fullp(index)));
        }
        else
        {
            portraitSlots[whichSlot].GetComponent<Image>().sprite = pLibrary.retrieve_fullp(index);
            portraitSlots[whichSlot].SetActive(true);
        }

    }
    void set_portrait_holo(int whichSlot, int state)
    {
        if (state == -1) portraitSlots[whichSlot].GetComponent<Image>().material = null;
        else portraitSlots[whichSlot].GetComponent<Image>().material = holoMaterial;
    }
    void set_speaker_glow(int which, int state)
    {
        if (state == -1){}
        else return;
    }
    void hide_portrait_slot(int whichSlot)
    {
        StartCoroutine(handle_image_hide_fade(imgFadeSpeed, whichSlot));
    }
    void imm_bg(int id)
    {
        // sets the bg immediately.
        if (skipOn == false) 
        {
            fader.fade_to_black(); //automatic fading behaviour
        }
        bg.sprite = pLibrary.retrieve_eventBg(id);
        effectsOver = true;
    }
    void set_bg(int id)
    {
        if (skipOn == false) {
            fader.fade_to_black(); //automatic fading behaviour
            //bg.sprite = pLibrary.retrieve_eventBg(id);
            StartCoroutine(handle_bg_switch_fade(id));
        }
        else{
            bg.sprite = pLibrary.retrieve_eventBg(id);
        }
    }
    void set_overlay(int id)
    {
        if (id == -1) 
        {
            overlay.gameObject.SetActive(false);
            return;
        }
        

        if (skipOn == false) {
            //bg.sprite = pLibrary.retrieve_eventBg(id);
            StartCoroutine(handle_overlay(id));
        }
        else{
            overlay.sprite = pLibrary.retrieve_overlay(id);
            overlay.gameObject.SetActive(true);
        }
        
    }
    void set_snow(int strength)
    {
        if (strength == -1) 
        {
            //no snow
            snowSystem.gameObject.SetActive(false);
            return;
        }
        var emission = snowSystem.emission;
        emission.rateOverTime = (float)strength;
        
        if (rainSystem.gameObject.activeSelf) rainSystem.gameObject.SetActive(false);
        snowSystem.gameObject.SetActive(true);
    }
    void set_rain(int strength)
    {
        if (strength == -1) 
        {
            //no rain
            rainSystem.gameObject.SetActive(false);
            return;
        }
        var emission = rainSystem.emission;
        emission.rateOverTime = (float)strength;
        
        if (snowSystem.gameObject.activeSelf) snowSystem.gameObject.SetActive(false);
        rainSystem.gameObject.SetActive(true);
    }
    void set_wind(int strength)
    {
        //turns wind effects on.
        if (strength == -1) windSystem.gameObject.SetActive(false);       
        else windSystem.gameObject.SetActive(true);      
    }
    void inside(int wait)
    {
        //reorders the character canvas so that it appears before overlay and weather layers
        if (wait <= 0){
            characterCanvas.sortingLayerName = "Characters Inside Layer";
            return;
        }
        StartCoroutine(handle_character_layer_switch("Characters Inside Layer"));
    }
    void outside(int wait)
    {      
        //reorders the character canvas so that it appears behind overlay and weather layers
        if (wait <= 0){
            characterCanvas.sortingLayerName = "Characters Outside Layer";
            return;
        }
        StartCoroutine(handle_character_layer_switch("Characters Outside Layer"));
    }
    void v_wiggle(int id, float power, int repeats)
    {
        //moves the corresponding portrait up and down.
        //power is a percentage of the base amount.
        Vector3 moveAmount = new Vector3(0f, power * -1f, 0f);
        StartCoroutine(v_wiggle(id, repeats, moveAmount));
    }
    void h_wiggle(int id, float power, int repeats)
    {
        //moves the corresponding portrait side to side.
        //power is a percentage of the base amount.
        Vector3 moveAmount = new Vector3(power * 1f, 0f, 0f);
        StartCoroutine(h_wiggle(id, repeats, moveAmount));
    }
    void set_unit_state(int unit_id, int val)
    {
        //sets the availability of units in the level tree. 0: fully, 1: visible but not clickable, 2: not visible or clickable
        Carrier.Instance.get_allUnitStates()[unit_id] = val;
    }
    void inc_unit_stat(int unit_id, int stat_id, float val)
    {
        Unit u = Carrier.Instance.get_allUnitList()[unit_id];
        switch(stat_id)
        {
            case 0:
                u.set_hpMax(u.get_hpMax() + (int)val);
                break;
            case 1:
                u.set_brkMax(u.get_brkMax() + (int)val);
                break;
            case 2:
                u.set_physa(u.get_physa() + (int)val);
                break;
            case 3:
                u.set_physd(Math.Round(u.get_physd() - val, 2));
                break;
            case 4:
                u.set_maga(u.get_maga() + (int)val);
                break;
            case 5:
                u.set_magd(Math.Round(u.get_magd() - val, 2));
                break;
            case 6:
                u.set_exp(u.get_exp() + (int)val);
                break;
        }
    }
    void msg_popup(string name)
    {
        //shows a message popup that slides in, shows a short while, then fades out.
        //displays something like: 'Message request from [name]'

        //set box to its starting position
        //2620 x, 700 y
        msgPopupImage.transform.position = new Vector2(2620f, 700f);
        msgPopupImage.color = new Color(msgPopupImage.color.r, msgPopupImage.color.g, msgPopupImage.color.b, 1f);
        msgPopupText.color = new Color(msgPopupText.color.r, msgPopupText.color.g, msgPopupText.color.b, 1f);
        msgPopupText.text = "Message request from [" + name + "]";

        //coroutine:
        StartCoroutine(msg_popup_control());
    }
    void run_program(string name, float duration)
    {
        // what does this do?
        // display program progress bar
        //  -program name above bar
        //  -slide bar that goes from left to right
        //  -hold while this is going on.
        if (skipOn) return;
        effectsOver = false;
        programGraphic.gameObject.SetActive(true);
        programGraphicText.text = name + "()";
        programGraphic.value = 0f;
        StartCoroutine(programSlide(duration));
    }
    IEnumerator programSlide(float duration)
    {
        yield return null;
        // increase value of programGraphic over time
        // at end, pause.
        // then hide.
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            // so we have 100 units
            // and we have to get our value to 100 over duration seconds
            // e.g. over 5 seconds, it should increase by 20 units every sec
            elapsedTime += Time.deltaTime;
            programGraphic.value = 100f * elapsedTime / duration;

            yield return null;
        }

        // then let game continue.
        yield return new WaitForSeconds(0.5f);
        effectsOver = true;
        programGraphic.gameObject.SetActive(false);
    }


    IEnumerator msg_popup_control()
    {
        //slide in over time
        //1220 x, 700 y
        float timeElapsed = 0f;
        float slideDuration = 0.5f;
        float slideSpeed = 2600f;
        Vector2 toHere = new Vector2(1220f, 700f);
        while (timeElapsed < slideDuration)
        {
            //2f * Time.deltaTime because the dimensions of a game tile is 2 by 2.
            msgPopupImage.transform.position = Vector3.MoveTowards(msgPopupImage.transform.position, toHere, slideSpeed * Time.deltaTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        //msgPopupImage.transform.position = new Vector2(1220f, 700f);

        yield return new WaitForSeconds(3.5f);

        //wait a few seconds
        //fade out over a few more seconds
        Color objectColor = msgPopupImage.color;
        Color textColor = msgPopupText.color;
        while ( msgPopupImage.color.a > 0)
        {
            float fadeAmount = objectColor.a - (1f* Time.deltaTime);
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            textColor = new Color(textColor.r, textColor.g, textColor.b, fadeAmount);

            msgPopupText.color = textColor;
            msgPopupImage.color = objectColor;
            yield return null;
        }
    }

    IEnumerator v_wiggle(int id, int repeats, Vector3 moveAmount)
    {
        //moves the corresponding portrait by the vector amount, twice. 
        //There and back, there and back.
        GameObject obj = portraitSlots[id];
        for (int i = 0; i < repeats; i++)
        {
            float elapsedTime = 0f;
            while (elapsedTime < 0.2f)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, obj.transform.position + moveAmount, Time.deltaTime * 500);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            elapsedTime = 0f;
            while (elapsedTime < 0.2f)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, obj.transform.position - moveAmount, Time.deltaTime * 500);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
    IEnumerator h_wiggle(int id, int repeats, Vector3 moveAmount)
    {
        //moves the corresponding portrait by the vector amount, twice. 
        //There and back, there and back.
        GameObject obj = portraitSlots[id];
        for (int i = 0; i < repeats; i++)
        {
            float elapsedTime = 0f;
            while (elapsedTime < 0.5f)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, obj.transform.position + moveAmount, Time.deltaTime * 200);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            elapsedTime = 0f;
            while (elapsedTime < 0.5f)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, obj.transform.position - moveAmount, Time.deltaTime * 200);
                elapsedTime += Time.deltaTime;
                yield return null;
            }         

        }
    }

    IEnumerator handle_character_layer_switch(string s)
    {
        yield return new WaitForSeconds(1f);
        characterCanvas.sortingLayerName = s;
    }
    IEnumerator handle_bg_switch_fade(int id)
    {
        effectsOver = false;
        yield return new WaitForSeconds(1f);
        bg.sprite = pLibrary.retrieve_eventBg(id);
        effectsOver = true;
    }
    IEnumerator handle_overlay(int id)
    {
        yield return new WaitForSeconds(1f);
        overlay.sprite = pLibrary.retrieve_overlay(id);
        overlay.gameObject.SetActive(true);
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

    //audio effects
    void stop_music()
    {
        audio.stop_music();
    }
    void start_music(int id)
    {
        audio.ow_play_music(id);
    }
    void play_sound(int id)
    {
        audio.ow_play_sound(id);
    }


}
