using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSet : MonoBehaviour
{
    // holds images for some purpose. Can be:
    // -bg images
    // -overlay images
    // -messager images
    // -combat frame images
    [SerializeField] private Sprite[] full;

    public Sprite get_fullImage(int i){return full[i]; }
}
