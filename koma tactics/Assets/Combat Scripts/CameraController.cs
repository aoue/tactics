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
    private bool canMove;
    private float topBorder;
    private float bottomBorder;
    private float rightBorder;
    private float leftBorder;

    void Update()
    {
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

        float xDest = transform.position.x + horizontalInput;
        xDest = Math.Min(rightBorder, xDest);
        xDest = Math.Max(leftBorder, xDest);

        float yDest = transform.position.y + verticalInput;
        yDest = Math.Min(topBorder, yDest);
        yDest = Math.Max(bottomBorder, yDest);

        Vector3 targetPosition = new Vector3(xDest, yDest, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, cameraSpeed * Time.deltaTime);
    }

    public void lock_camera() { canMove = false; }
    public void unlock_camera() { canMove = true; }
    public bool get_canMove() { return canMove; }

    public void setup(int xborder, int yborder)
    {
        //called at the start of a mission by combatGrid, who sets the values of 
        //the borders based on the map size.

        //e.g. map dimensions are x=7 by y=12
        //y limits coords: 0, 12
        //x limits coords: 0, 22

        //you can go 3 tiles away from the border in any direction
        topBorder = (2 * (xborder - 1));
        bottomBorder = 2;
        rightBorder = (2 * (yborder - 1));
        leftBorder = 2;

        //set camera size
        maxSize = 12;
        minSize = 6;

        //set starting position (in the middle)
        float xcenter = (2 * (yborder - 1)) / 2;
        float ycenter = (2 * (xborder - 1)) / 2;
        Vector3 startpos = new Vector3(xcenter, ycenter, - 10f);
        transform.localPosition = startpos;
    }
    public void jump_to(Vector3 toHere)
    {
        //only called when the camera is locked.
        //called to move the camera to a specific position.

        float real_x = Mathf.Min(rightBorder, toHere.x);
        real_x = Mathf.Max(leftBorder, real_x);

        float real_y = Mathf.Min(topBorder, toHere.y);
        real_y = Mathf.Max(bottomBorder, real_y);

        Vector3 real_pos = new Vector3(real_x, real_y, -10f);
        transform.position = toHere;
    }
    public void slide_to(Vector3 toHere, int x, int y, bool duringEnemyTurn = false, float slideSpeed = 2f)
    {
        //only called when the camera is locked.
        //called to slide the camera to a specific position over time.
        float slideDuration = (float)Math.Sqrt(Math.Pow(Math.Abs(transform.position.x - x), 2) + Math.Pow(Math.Abs(transform.position.y - y), 2));       

        StartCoroutine(slide_camera(toHere, slideDuration, duringEnemyTurn, slideSpeed));
    }
    IEnumerator slide_camera(Vector3 toHere, float slideDuration, bool duringEnemyTurn, float slideSpeed)
    {
        canMove = false;
        float timeElapsed = 0f;
        while (timeElapsed < slideDuration && !canMove)
        {
            //2f * Time.deltaTime because the dimensions of a game tile is 2 by 2.
            transform.position = Vector3.MoveTowards(transform.position, toHere, slideSpeed * Time.deltaTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        if (!canMove) transform.position = toHere;
        if (!duringEnemyTurn) canMove = true;
    }
  
}
