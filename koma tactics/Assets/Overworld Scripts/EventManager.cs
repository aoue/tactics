using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Ink.Runtime;

public class EventManager : MonoBehaviour
{
    [SerializeField] private Overworld overworld; //link back to the boss.
    [SerializeField] private OverworldAudio audio;

    [SerializeField] private Font ancientsFont;
    [SerializeField] private Font defaultFont;
    [SerializeField] private FadeManager fader;

    [SerializeField] private BackgroundManager allEventBackgrounds; //different background manager from the overworld background manager, but same class.
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
    private bool isCentered = false; //determines whether to use sentenceText or use centeredText in typeSentence().

    [SerializeField] private GameObject settingsView; //lets player adjust vn settings, like text speed.

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
    [SerializeField] private GameObject[] portraitSlots; //3 total. dimensions are 540 x 1080 | 1 : 2 ratio

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
        while (effectsOver == false)
        {
            yield return new WaitForSeconds(1f);
        }
        
        string displayString = "";
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

                foreach (char letter in sentence.ToCharArray())
                {
                    if (skipOn == true)
                    {
                        //i.e., if skip is turned on while the sentence is playing.
                        centeredText.text = saveString + sentence;
                        break;
                    }
                    displayString += letter;
                    centeredText.text = saveString + displayString;
                    yield return new WaitForSeconds(textWait);
                }               
            }
            centeredText.text += "\n";

        }
        else
        {
            if (skipOn)
            {
                sentenceText.text = sentence;            }
            else
            {
                sentenceText.text = "";
                yield return new WaitForSeconds(0.05f);               

                foreach (char letter in sentence.ToCharArray())
                {
                    if (skipOn == true)
                    {
                        //i.e., if skip is turned on while the sentence is playing.
                        sentenceText.text = sentence;
                        break;
                    }
                    displayString += letter;
                    //control quotes, parentheses, or nothing.
                    if (nameText.text == "")
                    {
                        sentenceText.text = displayString;
                    }
                    else
                    {
                        sentenceText.text = "\"" + displayString + "\"";
                    }          
                    yield return new WaitForSeconds(textWait);
                }
            }

        }
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

        //fill in roles text.
        fill_roles();

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
                audio.play_typingSound();
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
        eventObjects.SetActive(false);
        overworld.load_part();
    }


    //Helpers
    void fill_roles()
    {
        //return all effects to default.
        sentenceText.font = defaultFont; //use normal font
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

        //party
        script.BindExternalFunction("add_unit", (int id) =>
        {
            this.add_party(id);
        });
        script.BindExternalFunction("remove_unit", (int id) =>
        {
            this.remove_party(id);
        });

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
        
        script.BindExternalFunction("n", (string name) =>
        {
            this.set_name(name);
        });
        script.BindExternalFunction("p", (int pId) =>
        {
            this.set_boxPortrait(pId);
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
            speakerBoxPortrait.sprite = pLibrary.retrieve_boxp(speakerBoxId);
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
        Vector3 initial_position = eventObjects.transform.localPosition;

        //zoom in so we don't see the edges fraying
        eventObjects.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);

        //shake
        while (duration > 0f)
        {
            eventObjects.transform.localPosition = initial_position + (Random.insideUnitSphere * intensity);
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
        
        if (snowSystem.gameObject.activeSelf) rainSystem.gameObject.SetActive(false);
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
        if (wait == -1){
            characterCanvas.sortingLayerName = "Characters Inside Layer";
            return;
        }
        StartCoroutine(handle_character_layer_switch("Characters Inside Layer"));
    }
    void outside(int wait)
    {      
        //reorders the character canvas so that it appears behind overlay and weather layers
        if (wait == -1){
            characterCanvas.sortingLayerName = "Characters Outside Layer";
            return;
        }
        StartCoroutine(handle_character_layer_switch("Characters Outside Layer"));
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
    
    //party
    void add_party(int id)
    {
        Carrier.Instance.add_to_party(id);
    }
    void remove_party(int id)
    {
        Carrier.Instance.remove_from_party(id);
    }

}
