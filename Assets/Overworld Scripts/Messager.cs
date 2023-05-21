using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Messager : MonoBehaviour
{
    // the messager, on the overworld screen.
    // the button to open it is the top button of the 6-button sidebar on the left.
    // clicking on the button opens the visual component.
    // the player is locked in that until they hit the visual component's close button.
    // once they close it, that's it. All done.

    // its state is set corresponding to the current part.
    // one per part.
    // it is used for worldbuilding or character stuff.

    /*
    the visual component:
    -sender profile picture
	-sender text, which can be:
		-the sender's name, can be a username, person's real name, organization name
		-the subject title (can be an original title or can be forward)
		e.g. Friday > Fwd: Rising Housing Prices
		e.g. Yve > Labradors Check-in
	-cg image (can be anything)
		e.g. a meme, joke, spam, ads, news, an article, an advertisement (aren't you interested in this), a pamphlet sent out from a maxwell political club
	-some accompanying text along the bottom, from the character who sent it.
		e.g. Hey LMAO this is crazy^^^
	it's like a mini browser window open.
    */

    [SerializeField] private Image blocker; // to stop player from doing other things while the popup is showing.
    [SerializeField] private GameObject popup; // to enable/disable entire object.
    [SerializeField] private Text senderText; // to set the name of the sender, at the top.
    [SerializeField] private Image cgImage; // to set the image that is being shared.
    [SerializeField] private Text bottomText; // the message the sender send along with the cg.
    [SerializeField] private Button closeButton; // the button used to close the popup.

    [SerializeField] private Sprite noNotifSprite;
    [SerializeField] private Sprite yesNotifSprite;
    private bool viewed; // If false, can open. Else, cannot open.
    private float blocker_fade_anim_duration = 1f; // how long it takes for blocker alpha to fade to 60f
    private float blocker_max_alpha = 0.1f; // how long it takes for blocker alpha to fade to 60f
    private int savedPartID;

    public void validate(int partID)
    {
        // called during part setup.
        // use the partID to get information about the state we should set to:
        // -we have a thing for this part, so enable it.
        // -we don't, so disable the button.
        savedPartID = partID;
        bool validated = true;
        if (validated)
        {
            gameObject.GetComponent<Image>().sprite = yesNotifSprite;
            gameObject.GetComponent<Button>().interactable = true;
        }
        else
        {
            gameObject.GetComponent<Image>().sprite = noNotifSprite;
            gameObject.GetComponent<Button>().interactable = false;
        }
        
    }
    public void open()
    {
        // called on sidebarButton click, must already have been validated.

        // retrieve info and set fields.
        senderText.text = "Sender: ____\nRE: ____";
        //cgImage.sprite = 
        bottomText.text = "HEY HEY HEY HEY HEY HEY anyway bruh have you seen like LMAO someonme posted it on the Aventine's main forum xDDD";

        // turn on background behind that stops the player from clicking anything else except for popup close.
        // turn it on immediately, then slide blocker's alpha from 0 to 60.
        blocker.gameObject.SetActive(true);
        closeButton.interactable = false;
        gameObject.GetComponent<Image>().sprite = noNotifSprite;

        // fade in popup with a fade or animation of some kind.
        popup.SetActive(true);
        StartCoroutine(open_over_time());
    }
    IEnumerator open_over_time()
    {
        // slides blocker's alpha from 0 to 60.
        float elapsedTime = 0f;
        while (elapsedTime < blocker_fade_anim_duration)
        {
            Color newColor = new Color (0f, 0f, 0f, blocker_max_alpha * elapsedTime / blocker_fade_anim_duration);
            blocker.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // slide popup's alpha too
        //blocker.color.a = 60f/255f;

        // only enable close button once the animation has finished.
        closeButton.interactable = true;
    }
    IEnumerator close_over_time()
    {
        popup.SetActive(false);
        // slides blocker's alpha from 0 to 60.
        float elapsedTime = 0f;
        while (elapsedTime < blocker_fade_anim_duration)
        {
            Color newColor = new Color (0f, 0f, 0f, blocker_max_alpha * (1f - elapsedTime / blocker_fade_anim_duration));
            blocker.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // slide popup's alpha too
        //blocker.color.a = 60f/255f;

        // only enable close button once the animation has finished.
        blocker.gameObject.SetActive(false);
        
    }
    public void close()
    {
        // called on popup close button click.
        closeButton.interactable = false;
        StartCoroutine(close_over_time());
        
    }

}
