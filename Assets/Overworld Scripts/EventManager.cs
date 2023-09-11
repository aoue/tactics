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
    [SerializeField] private PortraitLibrary pLibrary;
    [SerializeField] private ParticleSystem snowSystem;
    [SerializeField] private ParticleSystem rainSystem;
    [SerializeField] private ParticleSystem windSystem;
    [SerializeField] private Canvas characterCanvas; //need it to adjust layer for weather, overlay.

    //dialogue canvas members:
    [SerializeField] private GameObject eventObjects;
    [SerializeField] private CanvasGroup choiceParent;
    [SerializeField] private Image bg;
    [SerializeField] private Image fadeTapestry;
    [SerializeField] private Image thinkingframe;
    [SerializeField] private GameObject NormalDialogueBox;
    [SerializeField] private GameObject nameBox;
    [SerializeField] private Text nameText;
    [SerializeField] private Text sentenceText;
    [SerializeField] private Button buttonPrefab = null;
    [SerializeField] private Image speakerBoxPortrait;
    [SerializeField] private Image continuePopup;

    // Switch popups
    [SerializeField] private CanvasGroup bgSwitchPopup;
    [SerializeField] private Text bgSwitchPopupText;
    [SerializeField] private CanvasGroup musicSwitchPopup;
    [SerializeField] private Text musicSwitchPopupText;

    //[SerializeField] private Material defaultMaterial;
    [SerializeField] private Material holoMaterial;

    //dialogue box hider
    private bool hideAcceptsInput = true; //when true, you can toggle hide. false when transforming.
    private bool hideOn = false; //when true, hide the dialogue box. also, stop the text from advancing.
    [SerializeField] private CanvasGroup diaBoxGroup; //the thing we hide/show.

    //image swap speed
    private float imgFadeSpeed = 1.5f; //higher is faster. controls the speed at which char imgs are replaced/shown/hidden during events.

    //typing speed controllers
    private float speakerglow_anim_duration = 0.25f;
    private float autoWait = 1f; //how many seconds to wait before proceeding when auto mode is on.
    private float textWait = 0.035f; //how many seconds to wait after a non-period character in typesentence
    private float periodWait = 0.35f; //how many seconds to wait after a period character in typesentence
    private bool historyOn = false; //when true, viewing history and cannot continue the story.
    private bool settingsOn = false; //when true, viewing settings and cannot continue the story.
    private bool fastOn = false; //when true, text displays instantly. press crtl to toggle.
    private bool autoOn = false; //when true, text displays instantly. press crtl to toggle.

    [SerializeField] private GameObject settingsView; //lets player adjust vn settings, like text speed.
    [SerializeField] private Slider programGraphic;
    [SerializeField] private Text programGraphicText;

    private string currentSpeakerName; //used for pushing entries in history.
    [SerializeField] private GameObject HistoryPort; //master gameobject for the history interface.
    [SerializeField] private HistoryScroller histScroll; //used to fill/clear the content of the history interface.
    private List<HistoryEntry> historyList; 
    private int historyLimit = 20; //the max number of displays the historyList stores at a time.
    
    //button preset colors
    Color InactiveButtonColor = new Color(140f/255f, 140f/255f, 140f/255f, 1f);
    Color activeButtonColor = new Color(0.23f, 0.23f, 0.23f, 1f);
    [SerializeField] private Button[] diaControlButtons; //5 total. Needed to control their visual states.
    
    private bool canProceed;
    private bool effectsOver; //block progress while effects are under way
    private EventHolder heldEv;
    private Story script;
    [SerializeField] private GameObject[] portraitSlots; //6 total.

    void Update()
    {
        if (!effectsOver) return;

        //press 'spacebar' or 'enter' to continue, only if canProceed if true, we aren't showing history, and we aren't hiding dialogue box
        if (canProceed == true && historyOn == false && settingsOn == false)
        {
            if (!hideOn && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || autoOn == true || fastOn == true))
            {
                DisplayNextSentence();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            toggle_auto();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            toggle_fast();
        }
        
        if (Input.GetKeyDown(KeyCode.H) && settingsOn == false)
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

        
    }

    // MANAGE EVENT SKIP/AUTO BUTTONS
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
        hide_history();
        historyOn = false;
        //we transition it to the state we want using alpha, though.
        if (hideOn == true) 
        {
            StartCoroutine(modify_diaBox_alpha(true));
            diaControlButtons[0].GetComponent<Image>().color = activeButtonColor;
        }
        else
        {
            StartCoroutine(modify_diaBox_alpha(false));
            diaControlButtons[0].GetComponent<Image>().color = InactiveButtonColor;
        }
    }
    public void toggle_auto()
    {
        autoOn = !autoOn;
        if (fastOn) toggle_fast();

        //adjust button visuals
        if (autoOn)
        {
            diaControlButtons[1].GetComponent<Image>().color = activeButtonColor;
        }
        else
        {
            diaControlButtons[1].GetComponent<Image>().color = InactiveButtonColor;
        }
    }
    public void toggle_fast()
    {
        fastOn = !fastOn;
        if (autoOn) toggle_auto();
        //adjust button visuals
        if (fastOn)
        {
            diaControlButtons[2].GetComponent<Image>().color = activeButtonColor;
        }
        else
        {
            diaControlButtons[2].GetComponent<Image>().color = InactiveButtonColor;
        }
    }
    public void toggle_history()
    {
        //use a viewport to view the last ?? sentences.
        //they're all safely stored in historyQueue, a queue of strings.
        historyOn = !historyOn;

        if (historyOn == true)
        {
            show_history();
            diaControlButtons[3].GetComponent<Image>().color = activeButtonColor;
        }
        else
        {
            hide_history();
            diaControlButtons[3].GetComponent<Image>().color = InactiveButtonColor;
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
    void toggle_settings()
    {

    }


    // SETTINGS
    public void show_settings()
    {
        if (historyOn == true) return;

        settingsOn = true;
        if (settingsOn == true)
        {
            settingsView.SetActive(true);
        }
    }
    public void hide_settings()
    {
        settingsOn = false;
        settingsView.SetActive(false);
    }
    public void set_textWaitTime(System.Single value)
    {
        textWait = value;
        //Debug.Log("set_textWaitTime(). value = " + value + " | textWait = " + textWait);
    }


    // MANAGE EVENT RUNNING
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
        StartCoroutine(fadeObjectOut(continuePopup, 0.3f, 1f));
        continuePopup.gameObject.GetComponent<Button>().interactable = false;
        while (!effectsOver)
        {
            yield return new WaitForSeconds(0.2f);
        }
        
        string saveString;
        if (sentence[0] == '>')
        {
            saveString = sentenceText.text;
            sentence = sentence.Substring(1, sentence.Length - 1);
        }
        else
        {
            saveString = "";
            sentenceText.text = "";
        }
        string displayString = saveString;
                                    
        yield return new WaitForSeconds(0.05f);
        

        for(int i = 0; i < sentence.Length; i++)
        {
            if (fastOn)
            {
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
                sentenceText.text = displayString;
            }          
            if (i < sentence.Length - 1 && (sentence[i] == '.' || sentence[i] == '!' || sentence[i] == '?')) yield return new WaitForSeconds(periodWait);
            else yield return new WaitForSeconds(textWait);
        }
        if (fastOn)
        {
            sentenceText.text = saveString + sentence;
        }
        else
        {
            audio.play_typingSound();
            StartCoroutine(fadeObjectIn(continuePopup, 0.3f, 1f));
            
            continuePopup.gameObject.GetComponent<Button>().interactable = true;
        }
        if (autoOn)
        {
            yield return new WaitForSeconds(autoWait);
        }
        yield return new WaitForSeconds(0.05f);
        canProceed = true;
    }
    IEnumerator fadeObjectIn(Image obj, float duration, float maxAlpha)
    {
        while (!effectsOver) {
            yield return new WaitForSeconds(0.1f);
        }
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color newAlphaColor = new Color(obj.color.r, obj.color.g, obj.color.b, Math.Min(maxAlpha, elapsedTime / duration));
            obj.color = newAlphaColor;
            yield return null;
        }
        Color c = new Color(obj.color.r, obj.color.g, obj.color.b, Math.Min(maxAlpha, 1f));
        obj.color = c;
    }
    IEnumerator fadeObjectOut(Image obj, float duration, float maxAlpha)
    {
        while (!effectsOver) {
            yield return new WaitForSeconds(0.1f);
        }
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color newAlphaColor = new Color(obj.color.r, obj.color.g, obj.color.b, Math.Min(maxAlpha, 1f - elapsedTime / duration));
            obj.color = newAlphaColor;
            yield return null;
        }
        Color c = new Color(obj.color.r, obj.color.g, obj.color.b, 0f);
        obj.color = c;
    }

    public void begin_immediate(TextAsset storyText)
    {
        heldEv = null;
        setup_event(storyText);
    }
    public void begin_event(EventHolder ev)
    {
        //fader.fade_to_black();
        heldEv = ev;
        StartCoroutine(pause_before_starting_event());
    }   
    void setup_event(TextAsset storyText)
    {
        //setup initial view
        nameText.text = "";
        sentenceText.text = "";

        //reset history
        historyOn = false;
        currentSpeakerName = "NO SPEAKER";
        if (historyList == null) historyList = new List<HistoryEntry>();
        else historyList.Clear();

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
        //delete any remaining choice options
        int childCount = choiceParent.transform.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            GameObject.Destroy(choiceParent.transform.GetChild(i).gameObject);
        }

        canProceed = false;
        if (script.canContinue == true)
        {
            string sentence = script.Continue().Trim();
            //if sentence is blank; then don't type sentence on it. call DisplayNextSentence again.
            if ( sentence.Length > 0)
            {
                //add name, sentence pair to history
                HistoryEntry entry = new HistoryEntry(currentSpeakerName, sentence, sentenceText.color, nameText.color);
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

        //handle fade
        independent_from_black_fade();

        eventObjects.SetActive(false);
        overworld.load_part();
    }
    public void press_continuePopup()
    {
        DisplayNextSentence();
    }


    // HELPERS
    void init_vn_elems()
    {
        //return all effects to default.
        set_colour("white");
        set_name(""); //hide name box
        set_boxPortrait(-1); //hide box portrait
        set_speaker_glow(-1);
        fastOn = false;
        settingsOn = false;
        historyOn = false;
        autoOn = false;

        Color initFadeTapestry = new Color(0f, 0f, 0f, 1f);
        fadeTapestry.color = initFadeTapestry;
        fadeTapestry.gameObject.SetActive(true);

        continuePopup.gameObject.SetActive(true);

        Color initAlphaZero = new Color(0f, 0f, 0f, 0f);
        thinkingframe.color = initAlphaZero;
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


    // INDEPENDENT FADING
    public void independent_from_black_fade()
    {
        Color initFadeTapestry = new Color(0f, 0f, 0f, 1f);
        fadeTapestry.color = initFadeTapestry;
        fadeTapestry.gameObject.SetActive(true);
        fadeTapestry.raycastTarget = true;
        StartCoroutine(fade_from_black());
    }
    IEnumerator fade_from_black()
    {
        // hide textbox
        yield return new WaitForSeconds(1f);
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color newAlpha = new Color(0f, 0f, 0f, 1f - elapsedTime / duration);
            fadeTapestry.color = newAlpha;
            yield return null;
        }
        bgSwitchPopup.alpha = 0f;
        fadeTapestry.raycastTarget = false;
    }

    public void independent_to_black_fade()
    {
        Color initFadeTapestry = new Color(0f, 0f, 0f, 0f);
        fadeTapestry.color = initFadeTapestry;
        fadeTapestry.gameObject.SetActive(true);
        fadeTapestry.raycastTarget = true;
        StartCoroutine(fade_to_black());
    }
    IEnumerator fade_to_black()
    {
        // hide textbox
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color newAlpha = new Color(0f, 0f, 0f, elapsedTime / duration);
            fadeTapestry.color = newAlpha;
            yield return null;
        }
        bgSwitchPopup.alpha = 0f;
        yield return new WaitForSeconds(1f);
        fadeTapestry.raycastTarget = false;
    }


    // EXTERNAL FUNCTIONS
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
        script.BindExternalFunction("bg", (int id, float duration, string popupText) => 
        {
            this.set_bg(id, duration, popupText);
        });
        script.BindExternalFunction("thinkbg", (int id) => 
        {
            this.set_thinkbg(id);
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
        script.BindExternalFunction("show", (int which, int index) =>
        {
            this.set_portrait_slot(which, index);
        });
        script.BindExternalFunction("holo", (int which, int state) =>
        {
            this.set_portrait_holo(which, state);
        });
        script.BindExternalFunction("speakerglow", (int which) =>
        {
            this.set_speaker_glow(which);
        });
        script.BindExternalFunction("hide", (int which) =>
        {
            this.hide_portrait_slot(which);
        });
        script.BindExternalFunction("shake", (int intensity, float duration) =>
        {
            this.camera_shake(intensity, duration);
        });
        script.BindExternalFunction("program", (string name, float duration) =>
        {
            this.run_program(name, duration);
        });
    }


    // TEXT EFFECTS
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
            // nameBox.SetActive(true);
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


    // BACKGROUND EFFECTS
    void set_bg(int id, float duration, string popupText)
    {
        StartCoroutine(handle_bg_switch_fade(id, duration, popupText));
    }
    IEnumerator handle_bg_switch_fade(int id, float duration, string popupText)
    {
        // hide textbox
        effectsOver = false;
        NormalDialogueBox.SetActive(false);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            if (fadeTapestry.color.a < 1f)
            {
                Color newAlpha = new Color(0f, 0f, 0f, elapsedTime / duration);
                fadeTapestry.color = newAlpha;
            }
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        bg.sprite = pLibrary.retrieve_eventBg(id);

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color newAlpha = new Color(0f, 0f, 0f, 1f - elapsedTime / duration);
            fadeTapestry.color = newAlpha;
            yield return null;
        }

        StartCoroutine(show_popup(bgSwitchPopup));
        effectsOver = true;
        NormalDialogueBox.SetActive(true);
    }

    void set_thinkbg(int id)
    {
        //if id is -1, fade the thinkbg out.
        //otherwise, fade it in.
        if (id == -1) StartCoroutine(fadeObjectOut(thinkingframe, 1f, 160f/255f));
        else StartCoroutine(fadeObjectIn(thinkingframe, 1f, 160f/255f));
    }


    // CHARACTER PORTRAIT EFFECTS
    void set_portrait_slot(int whichSlot, int index)
    {
        StartCoroutine(portrait_slot_show_anim(imgFadeSpeed, portraitSlots[whichSlot].activeSelf, whichSlot, pLibrary.retrieve_fullp(index)));
    }
    IEnumerator portrait_slot_show_anim(float speed, bool fadeOutFirst, int whichSlot, Sprite switchSprite)
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

    void hide_portrait_slot(int whichSlot)
    {
        StartCoroutine(portrait_slot_hide_anim(imgFadeSpeed, whichSlot));
    }
    IEnumerator portrait_slot_hide_anim(float speed, int whichSlot)
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

    void set_speaker_glow(int whichSlot)
    {
        StartCoroutine(run_speakerglow_anim(whichSlot)); 
    }
    IEnumerator run_speakerglow_anim(int whichSlot)
    {
        float elapsedTime = 0f;
        Color compareDark = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color compareWhite = new Color(1f, 1f, 1f, 1f);
        while (elapsedTime < speakerglow_anim_duration)
        {
            if (whichSlot == -1)
            {
                //so the final colour values are 1f, 1f, 1f
                //we will be starting from potentially 1f or 0.5f
                // -at 1f, don't make any change.
                // -at 0.5f, incrementally increase from 0.5f to 1f over the duration.
                float value = 0.5f + (elapsedTime/speakerglow_anim_duration) / 2;
                Color inc_lit = new Color(value, value, value, 1f);

                // light up all
                for(int i = 0; i < portraitSlots.Length; i++)
                {
                    if (colors_equal(portraitSlots[i].GetComponent<Image>().color, compareWhite)) continue;
                    else portraitSlots[i].GetComponent<Image>().color = inc_lit;
                }
            }
            else
            {
                // put all portrait slots to dark, except for whichSlot which is set to white
                //we will be starting from potentially 1f or 0.5f
                // -if at 1f, incrementally decrease from 1f to 0.5f over the duration.
                // -if at 0.5f, don't make any change.

                //after one fifth of the duration, the value should be 0.9f
                float dark_value = 1f - ((elapsedTime/speakerglow_anim_duration) / 2);
                Color inc_darkened = new Color(dark_value, dark_value, dark_value, 1f);

                // darken all but whichSlot
                for(int i = 0; i < portraitSlots.Length; i++)
                {
                    if (colors_equal(portraitSlots[i].GetComponent<Image>().color, compareDark)) continue;
                    if (i != whichSlot) portraitSlots[i].GetComponent<Image>().color = inc_darkened;
                }

                // and at the same time, light up whichSlot
                if (!colors_equal(portraitSlots[whichSlot].GetComponent<Image>().color, compareWhite))
                {
                    float light_value = 0.5f + (elapsedTime/speakerglow_anim_duration) / 2;
                    Color inc_lit = new Color(light_value, light_value, light_value, 1f);
                    portraitSlots[whichSlot].GetComponent<Image>().color = inc_lit;
                }
                
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // set to exact values
        if (whichSlot == -1)
        {
            // light up everything
            for(int i = 0; i < portraitSlots.Length; i++)
            {
                portraitSlots[i].GetComponent<Image>().color = compareWhite;
            }
        }
        else
        {
            // darken all but whichSlot
            for(int i = 0; i < portraitSlots.Length; i++)
            {
                if (i != whichSlot) portraitSlots[i].GetComponent<Image>().color = compareDark;
                else portraitSlots[whichSlot].GetComponent<Image>().color = compareWhite;
            }
        }
    }
    bool colors_equal(Color c1, Color c2)
    {
        // used to compare colors. Returns true if colors are equal, false otherwise.
        return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b;
    }

    void set_portrait_holo(int whichSlot, int state)
    {
        if (state == -1) portraitSlots[whichSlot].GetComponent<Image>().material = null;
        else portraitSlots[whichSlot].GetComponent<Image>().material = holoMaterial;
    }
    void set_boxHolo(int state)
    {
        if (state == -1) speakerBoxPortrait.material = null;
        else speakerBoxPortrait.material = holoMaterial;
    }


    // CAMERA AND MOVEMENT EFFECTS
    void v_wiggle(int id, float power, int repeats)
    {
        //moves the corresponding portrait up and down.
        //power is a percentage of the base amount.
        Vector3 moveAmount = new Vector3(0f, -Math.Abs(power), 0f);
        StartCoroutine(v_wiggle(id, repeats, moveAmount));
    }
    IEnumerator v_wiggle(int id, int repeats, Vector3 moveAmount)
    {
        //moves the corresponding portrait by the vector amount, twice. 
        //There and back, there and back.
        effectsOver = false;
        GameObject obj = portraitSlots[id];
        for (int i = 0; i < repeats; i++)
        {
            float yDest = obj.transform.position.y + moveAmount.y;
            while (obj.transform.position.y > yDest)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, obj.transform.position + moveAmount, Time.deltaTime * 500);
                yield return null;
            }
            yDest = obj.transform.position.y - moveAmount.y;
            while (obj.transform.position.y < yDest)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, obj.transform.position - moveAmount, Time.deltaTime * 500);
                yield return null;
            }
        }
        effectsOver = true;
    }
    void h_wiggle(int id, float power, int repeats)
    {
        //moves the corresponding portrait side to side.
        //power is a percentage of the base amount.
        Vector3 moveAmount = new Vector3(-Math.Abs(power), 0f, 0f);
        StartCoroutine(h_wiggle(id, repeats, moveAmount));
    }
    IEnumerator h_wiggle(int id, int repeats, Vector3 moveAmount)
    {
        //moves the corresponding portrait by the vector amount, twice. 
        //There and back, there and back.
        effectsOver = false;
        GameObject obj = portraitSlots[id];

        for (int i = 0; i < repeats; i++)
        {
            float xDest = obj.transform.position.x + moveAmount.x;
            while (obj.transform.position.x > xDest)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, obj.transform.position + moveAmount, Time.deltaTime * 200);
                yield return null;
            }
            xDest = obj.transform.position.x - moveAmount.x;
            while (obj.transform.position.x < xDest)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, obj.transform.position - moveAmount, Time.deltaTime * 200);
                yield return null;
            }
        }
        effectsOver = true;
    }
    
    void camera_shake(int intensity, float duration)
    {
        StartCoroutine(trigger_camera_shake(intensity, duration));
    }
    IEnumerator trigger_camera_shake(int intensity, float duration)
    {
        //zoom in so we don't see the edges fraying
        //eventObjects.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
        Vector3 initial_position = eventObjects.transform.localPosition;

        //shake
        while (duration > 0f)
        {
            //eventObjects.transform.localPosition = intensity * (initial_position - UnityEngine.Random.insideUnitSphere);
            float offset = UnityEngine.Random.Range(0, intensity / 100f);
            eventObjects.transform.localScale = new Vector3(1f + offset, 1f + offset, 1f);
            duration -= Time.deltaTime;
            yield return null;
        }

        //reset initial position
        eventObjects.transform.localPosition = initial_position;
        eventObjects.transform.localScale = new Vector3(1f, 1f, 1f);
    }
    
    void run_program(string name, float duration)
    {
        // what does this do?
        // display program progress bar
        //  -program name above bar
        //  -slide bar that goes from left to right
        //  -hold while this is going on.
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

    // WEATHER EFFECTS
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
        if (strength == -1) windSystem.gameObject.SetActive(false);       
        else windSystem.gameObject.SetActive(true);

        if (strength == -1) 
        {
            //no wind
            windSystem.gameObject.SetActive(false);
            return;
        }
        var emission = windSystem.emission;
        emission.rateOverTime = (float)strength;
        windSystem.maxParticles = strength;
        windSystem.gameObject.SetActive(true);
    }

    // AUDIO EFFECTS
    void stop_music()
    {
        audio.stop_music();
    }
    void start_music(int id)
    {
        musicSwitchPopupText.text = get_song_title(id);
        audio.ow_play_music(id);
        StartCoroutine(show_popup(musicSwitchPopup));
    }
    void play_sound(int id)
    {
        audio.ow_play_sound(id);
    }
    string get_song_title(int id)
    {
        switch(id){
            case 0:
                return "track0";
            case 1:
                return "track1";
            case 2:
                return "track2";
            case 3:
                return "track3";
            case 4:
                return "track4";
            case 5:
                return "track5";
        }
        return "Track_Not_Found";
    }
    IEnumerator show_popup(CanvasGroup popUp)
    {
        float duration = 1f;
        while (!effectsOver) {
            yield return new WaitForSeconds(0.1f);
        }
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            popUp.alpha = 2*elapsedTime / duration;
            yield return null;
        }
        yield return new WaitForSeconds(1f);

        while (!effectsOver) {
            yield return new WaitForSeconds(0.1f);
        }
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            popUp.alpha = 1f - elapsedTime / duration;
            yield return null;
        }
        popUp.alpha = 0f;
    }


}


        
