using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryScroller : MonoBehaviour
{
    //uses a vertical layout group
    [SerializeField] private Scrollbar sendToBottom; //used to force scroller to start at the bottom
    [SerializeField] private GameObject content; //the place where all the prefabs are instantiated.
    [SerializeField] private GameObject entryPrefab; //prefab it makes in content.
    //each entry prefab has 2 text fields: name and sentence
    bool bruh = false;
    void Update()
    {
        if (bruh)
        {
            sendToBottom.value = 0f;
            bruh = false;
        }
    }

    public void show(List<HistoryEntry> he)
    {
        clear();
        for (int i = 0; i < he.Count; i++)
        {
            GameObject entry = Instantiate(entryPrefab) as GameObject;
            entry.GetComponent<Text>().text = he[i].name;
            entry.transform.GetChild(0).GetComponent<Text>().text = he[i].sentence;

            entry.transform.SetParent(content.transform, false);
        }

        //finally, add a dummy entry - but make it invisible.
        //this is for alignment
        GameObject lastEntry = Instantiate(entryPrefab) as GameObject;
        lastEntry.GetComponent<Text>().text = "";
        lastEntry.transform.GetChild(0).GetComponent<Text>().text = "";

        lastEntry.transform.SetParent(content.transform, false);

        //finally, force us to be at the bottom of the scollview so the player sees the most recent sentence said.
        bruh = true;
    }
    public void clear()
    {
        //delete all entries in content
        int childCount = content.transform.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            GameObject.Destroy(content.transform.GetChild(i).gameObject);
        }
    }
    

}
