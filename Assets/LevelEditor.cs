using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditor : MonoBehaviour {

    public static LevelEditor Instance;

    private GameManager m_gameManager;

    public Texture2D Leveltexture;

    public Color EditColor;

    private int m_currentPixelX;

    private int m_currentPixelY;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_gameManager = GameManager.Instance;
    }

    public void SaveLevel()
    {

    }

    public void LoadLevel()
    {

    }

    public void NewLevel()
    {
        m_gameManager.ChangeGameState(GAME_STATE.EDIT);
        Leveltexture = new Texture2D(1000, 200, TextureFormat.RGBA32, false, true);
        Leveltexture.filterMode = FilterMode.Point;
        Leveltexture.wrapMode = TextureWrapMode.Clamp;
        
             
        m_gameManager.GenerateLevelSprite(Leveltexture);
        Leveltexture.Apply();
    }



    public void HandleMouseInput()
    {

        if (Input.GetMouseButton(0))
        {
            Debug.Log("click");

            GetPixelFromWorldPosition(m_gameManager.MousePosition);


            for (int x = -6; x < 6; x++)
            {
                for (int y = -6; y < 6; y++)
                {
                    int pixelX = m_currentPixelX + x;
                    int pixelY = m_currentPixelY + y;
                    Leveltexture.SetPixel(pixelX, pixelY, EditColor);
                }
            }
            Leveltexture.Apply();
        }
    }

     public void GetPixelFromWorldPosition(Vector3 _position)
    {
       
        m_currentPixelX = Mathf.RoundToInt(_position.x / m_gameManager.UnitPerPixel);
        m_currentPixelY = Mathf.RoundToInt(_position.y / m_gameManager.UnitPerPixel);

    }
}
