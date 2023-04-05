using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // controls the main menu. Does things like:
    // -setting the bg
    // -showing load menu, etc

    private bool testing = true;
    
    [SerializeField] private Image bgFrame; // the main background image. We switch the sprite depending on the game state.
    [SerializeField] private Sprite[] bgs; // the main background images. We put one into the frame depending on the state of the most recent save. Like 1 per chapter, whatever.
    [SerializeField] private OverworldAudio audioManager; // controls the audio for this scene
    [SerializeField] private CanvasGroup mainButtonsGroup; // handle setting the buttons to interatable or not. For control flow.

    [SerializeField] private FadeManager fader;
    [SerializeField] private Initializer init; // handles setting or resetting things to the state that they should be at the start of the game.

    private float awakeDelay = 2f;
    private float loadDelay = 4f;
    

    void Awake()
    {
        // called when this scene is loaded.

        // retrieve most recent save OR set to defaults if there is no same at all
        int currentBgIndex = 0;   

        // set bg frame to correct image
        bgFrame.sprite = bgs[currentBgIndex];

        audioManager.ow_play_music(0);

        // fade in from darkness
        if (!testing)
        {
            mainButtonsGroup.alpha = 0f;
            mainButtonsGroup.interactable = false;
            fader.fade_from_black_cheat(awakeDelay);
            StartCoroutine(enable_main_buttons_after_delay());
        }
    }
    IEnumerator enable_main_buttons_after_delay()
    {
        yield return new WaitForSeconds(awakeDelay); 
        while (mainButtonsGroup.alpha < 1f)
        {
            mainButtonsGroup.alpha += 0.01f;
            yield return new WaitForSeconds(0.02f);
        }
        mainButtonsGroup.interactable = true;
    }

    // handle launching a new game
    public void click_continue()
    {
        // sets all variables to the loaded game's state.
        //init.reset_units();
        //init.set_units(savegamedata... etc);
    }
    public void click_new_game()
    {
        // launches a new game.

        // disable all buttons
        mainButtonsGroup.interactable = false;

        // sets all variables to the new game state.
        init.reset_units();

        // fade to black and load game
        if (!testing) { fader.fade_to_black_stay(loadDelay); StartCoroutine(load_game_after_delay()); }
        else { Carrier.Instance.new_game(); SceneManager.LoadScene(1); }
    }
    public void click_load()
    {
        // open load menu
    }
    public void click_settings()
    {
        // open settings menu
    }
    public void click_exit()
    {
        Application.Quit();
    }

    IEnumerator load_game_after_delay()
    {
        yield return new WaitForSeconds(loadDelay);
        audioManager.ow_play_music(-1);
        yield return new WaitForSeconds(1f);
        Carrier.Instance.new_game();
        SceneManager.LoadScene(1);
    }


}
