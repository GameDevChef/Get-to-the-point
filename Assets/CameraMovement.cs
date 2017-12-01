using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    bool IsMouseMovement;

    float m_cameraHalfWidth;
    float m_cameraHalfHeight;


    float minCamX = 2.4f;
    float minCamY = 0.64f;
    float maxCamX;
    float maxCamY;

    bool boundsSet;

    private void Start()
    {       
        m_cameraHalfWidth = (float)Screen.width / (float)Screen.height  * Camera.main.orthographicSize;
        Debug.Log(Screen.width / Screen.height);
     
        m_cameraHalfHeight = Camera.main.orthographicSize;
    }

    public void SetCameraBounds(int _pixelWidth, int _pixelHeight)
    {
        float unitWidth = _pixelWidth * 0.01f;
        float unitHeight = _pixelHeight * 0.01f;

        maxCamY = unitHeight - m_cameraHalfHeight;
        maxCamX = unitWidth - m_cameraHalfWidth;

        boundsSet = true;
    }

    private void Update()
    {
        

        if (!boundsSet)
            return;

        Vector3 move = Vector3.zero;

        if (!IsMouseMovement)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            move.x = h;
            move.y = v;

            move = transform.position + move * Time.deltaTime;

            move.x = Mathf.Clamp(move.x, minCamX, maxCamX);
            move.y = Mathf.Clamp(move.y, minCamY, maxCamY);

            transform.position = move;
            return;
        }


       

        if (UIManager.Instance.IsOverUI)
            return;

        if (Input.mousePosition.y < 380 && transform.position.y > minCamY)
        {
            move = Vector3.down;
        }

        else if (Input.mousePosition.y > Screen.height - 100 && transform.position.y < maxCamY)
        {
            move = Vector3.up;
        }

        else if (Input.mousePosition.x < 100 && transform.position.x > minCamX)
        {
            move = Vector3.left;
        }

        else if (Input.mousePosition.x > Screen.width - 100 && transform.position.x < maxCamX)
        {
            move = Vector3.right;
        }

      

        transform.position += move * Time.deltaTime;

      

        
        
    }
}
