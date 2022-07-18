using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Used to control the camera.
    //the camera size can be controlled with mousewheel scrool.
    //the camera can be moved with wasd or pushing the edge of the screen with the mouse.

    //camera size
    [SerializeField] private Camera cam;
    int maxSize; //+/- in either direction
    int minSize; //+/- in either direction

    //camera boundaries
    int topBorder;
    int bottomBorder;
    int rightBorder;
    int leftBorder;

    float cameraSpeed = 5.0f;

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
        //called through combatGrid to move the camera to a specific position.
        //the camera jumps to this position.
        transform.position = toHere;
    }

    void Update()
    {
        //cycle camera side by pressing 'q'

        cam.orthographicSize = Mathf.Min(maxSize, Mathf.Max(minSize, cam.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * cameraSpeed));

        

        //move camera position with mouse or WASD
        var VPmousePosition = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
        float horizontalInput = 0f;
        float verticalInput = 0f;
        
        if (VPmousePosition.x <= 0.01f || Input.GetKey(KeyCode.A))
        {
            //Debug.Log("Left");
            horizontalInput = -1f;
        }
        else if (VPmousePosition.x >= 0.99f || Input.GetKey(KeyCode.D))
        {
            //Debug.Log("Right");
            horizontalInput = 1f;
        }

        if (VPmousePosition.y <= 0.01f || Input.GetKey(KeyCode.S))
        {
            //Debug.Log("Down");
            verticalInput = -1f;
        }
        else if (VPmousePosition.y >= 0.99f || Input.GetKey(KeyCode.W))
        {
            //Debug.Log("Up");
            verticalInput = 1f;
        }

        Vector3 targetPosition = new Vector3(transform.position.x + horizontalInput, transform.position.y + verticalInput, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);

        
    }

}
