using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraController : MonoBehaviour
{
    //Used to control the camera.
    //the camera size can be controlled with mousewheel scrool.
    //the camera can be moved with wasd or pushing the edge of the screen with the mouse.

    //camera speed
    private const float cameraSpeed = 20f;
    private const float cameraBorderDetection = 0.02f; //smaller means it detects closer to the screen borders.

    //camera size ; set in setup()
    int maxSize; //+/- in either direction
    int minSize; //+/- in either direction
    
    [SerializeField] private Camera cam;
    

    //camera boundaries (todo)
    bool canMove;
    int topBorder;
    int bottomBorder;
    int rightBorder;
    int leftBorder;


    void Update()
    {
        //cycle camera side by pressing 'q'

        if (!canMove) return;

        cam.orthographicSize = Mathf.Min(maxSize, Mathf.Max(minSize, cam.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * cameraSpeed));

        //move camera position with mouse or WASD
        var VPmousePosition = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (VPmousePosition.x <= cameraBorderDetection|| Input.GetKey(KeyCode.A))
        {
            //Debug.Log("Left");
            horizontalInput = -1f;
        }
        else if (VPmousePosition.x >= 1f - cameraBorderDetection || Input.GetKey(KeyCode.D))
        {
            //Debug.Log("Right");
            horizontalInput = 1f;
        }

        if (VPmousePosition.y <= cameraBorderDetection || Input.GetKey(KeyCode.S))
        {
            //Debug.Log("Down");
            verticalInput = -1f;
        }
        else if (VPmousePosition.y >= 1f - cameraBorderDetection || Input.GetKey(KeyCode.W))
        {
            //Debug.Log("Up");
            verticalInput = 1f;
        }

        Vector3 targetPosition = new Vector3(transform.position.x + horizontalInput, transform.position.y + verticalInput, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, cameraSpeed * Time.deltaTime);
    }

    public void lock_camera() { canMove = false; }
    public void unlock_camera() { canMove = true; }

    public void setup(int xborder, int yborder)
    {
        //called at the start of a mission by combatGrid, who sets the values of 
        //the borders based on the map size.

        //set borders
        maxSize = 20;
        minSize = 6;

        //set starting position
        Vector3 startpos = new Vector3(2 * ((xborder - 1) / 2), 2 * ((yborder - 1) / 2), - 10f);
        transform.position = startpos;

    }
    public void jump_to(Vector3 toHere)
    {
        if (canMove) return;
        //only called when the camera is locked.
        //called to move the camera to a specific position.
        transform.position = toHere;
    }
    public void slide_to(Vector3 toHere, int x, int y)
    {
        if (canMove) return;
        //only called when the camera is locked.
        //called to slide the camera to a specific position over time.
        float slideDuration = (float)Math.Sqrt(Math.Pow(Math.Abs(transform.position.x - x), 2) + Math.Pow(Math.Abs(transform.position.y - y), 2));
        StartCoroutine(slide_camera(toHere, slideDuration));
    }
    IEnumerator slide_camera(Vector3 toHere, float slideDuration)
    {
        float timeElapsed = 0f;
        while (timeElapsed < slideDuration && !canMove)
        {
            //2f * Time.deltaTime because the dimensions of a game tile is 2 by 2.
            transform.position = Vector3.MoveTowards(transform.position, toHere, 2f * Time.deltaTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        if (!canMove) transform.position = toHere;
    }

    

}
