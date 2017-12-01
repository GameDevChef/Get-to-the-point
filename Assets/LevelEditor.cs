using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EDIT_STATE
{
    PAINT,
    SET_SPAWN,
    SET_EXIT
}

public class LevelEditor : MonoBehaviour {

    public static LevelEditor Instance;

    private GameManager m_gameManager;

    private UIManager m_UIManager;

    public GameObject m_LoadPopup;

    public InputField m_wwwAdress;

    public EDIT_STATE editState;

    public Texture2D Leveltexture;

    public Texture2D DefaultTexture;

    public Color EditColor;

    public Sprite SpawnSprite;


    public Sprite ExitSprite;

    bool hasSpawn;

    bool hasExit;

    private int m_currentPixelX;

    private int m_currentPixelY;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        editState = EDIT_STATE.PAINT;
        m_gameManager = GameManager.Instance;
        m_UIManager = UIManager.Instance;
        
    }

    public void SaveLevel()
    {

    }

    internal void ChangeColor(Color color)
    {
        if(editState != EDIT_STATE.PAINT)
        {
            ChangeEditState(EDIT_STATE.PAINT);
        }
        EditColor = color;
    }

    public void LoadLevel()
    {

    }

    public void NewLevel(Texture2D _texture = null)
    {
        m_gameManager.ChangeGameState(GAME_STATE.EDIT);
        int width = (_texture == null) ? 1000 : _texture.width;
        int height = (_texture == null) ? 400 : _texture.height;
        Leveltexture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
        Leveltexture.filterMode = FilterMode.Point;
        Leveltexture.wrapMode = TextureWrapMode.Clamp;

        Color c = new Color(0, 0, 0, 0);

        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(_texture != null)
                {
                    c = _texture.GetPixel(x, y);
                }
                Leveltexture.SetPixel(x, y, c);
            }
        }
                   
        m_gameManager.GenerateLevelSprite(Leveltexture);
        Leveltexture.Apply();
        Camera.main.GetComponent<CameraMovement>().SetCameraBounds(width, height);
    }

    internal void LoadFromWeb()
    {
        if (string.IsNullOrEmpty(m_wwwAdress.text))
            return;

        StartCoroutine(LoadFromWebCO());
    }

    private IEnumerator LoadFromWebCO()
    {
        
        WWW www = new WWW(m_wwwAdress.text);

        yield return www;

        if(www.texture == null)
        {
            Debug.Log("no texture");
            
            
        }
        else
        {
            Texture2D texture = www.texture;

            NewLevel(www.texture);
        }
    }

    public void OpenLoadPopup(bool _state)
    {
        Debug.Log(_state);
        m_LoadPopup.SetActive(_state);
    }

    public void HandleMouseInput()
    {
        switch (editState)
        {
            case EDIT_STATE.PAINT:
                Paint();
                break;
            case EDIT_STATE.SET_SPAWN:
                SetSpawnPosition(SpawnSprite);
                break;
            case EDIT_STATE.SET_EXIT:
                SetSpawnPosition(ExitSprite);
                break;
            default:
                break;
        }


    }

    void ChangeEditState(EDIT_STATE _targetStete)
    {
        editState = _targetStete;

        switch (_targetStete)
        {
            case EDIT_STATE.PAINT:
                m_UIManager.ChangeCursorSprite(m_UIManager.EmptyCursorSprite);
                break;
            case EDIT_STATE.SET_SPAWN:
                m_UIManager.ChangeCursorSprite(SpawnSprite);
                break;
            case EDIT_STATE.SET_EXIT:
                m_UIManager.ChangeCursorSprite(ExitSprite);
                break;
            default:
                break;
        }
    }

    void Paint()
    {
        if (Input.GetMouseButton(0))
        {

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

    void SetSpawnPosition(Sprite sprite)
    {
        if (sprite == ExitSprite && hasExit)
            return;

        if (sprite == SpawnSprite && hasSpawn)
            return;

        if (Input.GetMouseButton(0))
        {
            GetPixelFromWorldPosition(m_gameManager.MousePosition);
            Debug.Log(m_currentPixelX + " " + m_currentPixelY);


            if (m_currentPixelX < 10 || m_currentPixelX > 490 || m_currentPixelY < 10 || m_currentPixelY > 190)
                return;

            Texture2D texture = sprite.texture;
            int halfHeight = Mathf.RoundToInt(texture.height / 2);
            int halfWidth = Mathf.RoundToInt(texture.width / 2);


            for (int x = - halfWidth; x < halfWidth; x++)
            {
                for (int y = - halfHeight; y < halfHeight; y++)
                {
                    int pixelX = m_currentPixelX + x;
                    int pixelY = m_currentPixelY + y;
                    Color color = texture.GetPixel(x + halfWidth, y + halfHeight);
                    if (color.a > 0)
                    {
                        Leveltexture.SetPixel(pixelX, pixelY, color);
                       
                    }

                }
            }
            Leveltexture.Apply();  

            if(sprite == ExitSprite)
            {
                hasExit = true;
                m_gameManager.ExitVector = new Vector2(m_currentPixelX, m_currentPixelY);
            }

            if (sprite == SpawnSprite)
            {
                hasSpawn = true;
                m_gameManager.SpawnVector = new Vector2(m_currentPixelX, m_currentPixelY);
            }
        }
    }

   

    public void GetPixelFromWorldPosition(Vector3 _position)
    {
       
        m_currentPixelX = Mathf.RoundToInt(_position.x / m_gameManager.UnitPerPixel);
        m_currentPixelY = Mathf.RoundToInt(_position.y / m_gameManager.UnitPerPixel);

    }

    public void StartSettingSpawn()
    {
        ChangeEditState(EDIT_STATE.SET_SPAWN);

    }

    public void StartSettingExit()
    {
        ChangeEditState(EDIT_STATE.SET_EXIT);
    }
}
