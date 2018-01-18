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

    [Header("References")]

    [SerializeField]
    public GameObject m_loadWWWPopup;

    [SerializeField]
    InputField m_wwwAdress;

    [SerializeField]
    GameObject EditButton;

    [SerializeField]
    Sprite m_spawnSprite;

    [SerializeField]
    Sprite m_exitSprite;

    [HideInInspector]
    public Texture2D m_LevelTexture;

    GameManager m_gameManager;

    UIManager m_UIManager;

    EDIT_STATE m_editState;

    Color m_editColor;

    int m_editRadius;

    bool m_hasSpawn;

    bool m_hasExit;

    int m_currentPixelX;

    int m_currentPixelY;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        m_editState = EDIT_STATE.PAINT;
        m_gameManager = GameManager.Instance;
        m_UIManager = UIManager.Instance;
        
    }

    public void ChangeColor(Color _color)
    {
        if(m_editState != EDIT_STATE.PAINT)
        {
            ChangeEditState(EDIT_STATE.PAINT);
        }
        m_editColor = _color;
    }

    public void ChangeBrushSize(int _brushSize)
    {
        m_editRadius = _brushSize;
    }

    public void LoadLevel(LevelSave _save)
    {
        GameManager.Instance.m_IsLevelLoaded = true;
        EditButton.SetActive(true);
        m_LevelTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false, false);
        m_LevelTexture.filterMode = FilterMode.Point;
        m_LevelTexture.wrapMode = TextureWrapMode.Clamp;
        m_LevelTexture.LoadImage(_save.levelTextureBytes);

        GameManager.Instance.m_SpawnVector = new Vector2(_save.SpawnPosX, _save.SpawnPosY);
        GameManager.Instance.m_ExitVector = new Vector2(_save.ExitPosX, _save.ExitPosY);
        GameManager.Instance.GenerateLevelSprite(m_LevelTexture);
    }

    public void NewLevel(Texture2D _texture = null)
    {
        if(_texture == null)
        {
            GameManager.Instance.m_IsLevelLoaded = false;
            EditButton.SetActive(false);
        }

        m_gameManager.ChangeGameState(GAME_STATE.EDIT);
        int width = (_texture == null) ? 1000 : _texture.width;
        int height = (_texture == null) ? 400 : _texture.height;
        m_LevelTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
        m_LevelTexture.filterMode = FilterMode.Point;
        m_LevelTexture.wrapMode = TextureWrapMode.Clamp;

        Color c = new Color(0, 0, 0, 0);
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(_texture != null)
                {
                    c = _texture.GetPixel(x, y);
                }
                m_LevelTexture.SetPixel(x, y, c);
            }
        }
                   
        m_gameManager.GenerateLevelSprite(m_LevelTexture);
        m_LevelTexture.Apply();
        Camera.main.GetComponent<CameraMovement>().SetCameraBounds(width, height);
    }

    public void LoadFromWeb()
    {
        if (string.IsNullOrEmpty(m_wwwAdress.text))
            return;

        StartCoroutine(LoadFromWebCO());
    }

    IEnumerator LoadFromWebCO()
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
        m_loadWWWPopup.SetActive(_state);
    }

    public void HandleMouseInput()
    {
        if (m_UIManager.IsOverUI)
            return;
        switch (m_editState)
        {
            case EDIT_STATE.PAINT:
                Paint();
                break;
            case EDIT_STATE.SET_SPAWN:
                SetSpawnPosition(m_spawnSprite);
                break;
            case EDIT_STATE.SET_EXIT:
                SetSpawnPosition(m_exitSprite);
                break;
            default:
                break;
        }
    }

    void ChangeEditState(EDIT_STATE _targetStete)
    {
        m_editState = _targetStete;

        switch (_targetStete)
        {
            case EDIT_STATE.PAINT:
                m_UIManager.ChangeCursorSprite(m_UIManager.m_EmptyCursorSprite);
                break;
            case EDIT_STATE.SET_SPAWN:
                m_UIManager.ChangeCursorSprite(m_spawnSprite);
                break;
            case EDIT_STATE.SET_EXIT:
                m_UIManager.ChangeCursorSprite(m_exitSprite);
                break;
            default:
                break;
        }
    }

    void Paint()
    {
        if (Input.GetMouseButton(0))
        {
            int lastCurrentX = m_currentPixelX;
            int lastCurrentY = m_currentPixelY;
            GetPixelFromWorldPosition(m_gameManager.m_MousePosition);
            if (lastCurrentX == m_currentPixelX && lastCurrentY == m_currentPixelY)
                return;
            
            Vector3 center = m_gameManager.GetWorldPositionFromNode(m_currentPixelX, m_currentPixelY);

            for (int x = -m_editRadius; x <= m_editRadius; x++)
            {
                for (int y = -m_editRadius; y <= m_editRadius; y++)
                {
                    int pixelX = m_currentPixelX + x;
                    int pixelY = m_currentPixelY + y;
                    Vector3 current = m_gameManager.GetWorldPositionFromNode(pixelX, pixelY);
                    float distance = Vector3.Distance(center, current);
                    if (distance / m_gameManager.UnitPerPixel > m_editRadius)
                        continue;
                    m_LevelTexture.SetPixel(pixelX, pixelY, m_editColor);
                }
            }
            m_LevelTexture.Apply();
        }
    }

    void SetSpawnPosition(Sprite sprite)
    {
        if (sprite == m_exitSprite && m_hasExit)
            return;

        if (sprite == m_spawnSprite && m_hasSpawn)
            return;

        if (Input.GetMouseButton(0))
        {
            GetPixelFromWorldPosition(m_gameManager.m_MousePosition);

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
                        m_LevelTexture.SetPixel(pixelX, pixelY, color);
                       
                    }
                }
            }

            m_LevelTexture.Apply();  

            if(sprite == m_exitSprite)
            {
                m_hasExit = true;
                m_gameManager.m_ExitVector = new Vector2(m_currentPixelX, m_currentPixelY);
            }

            if (sprite == m_spawnSprite)
            {
                m_hasSpawn = true;
                m_gameManager.m_SpawnVector = new Vector2(m_currentPixelX, m_currentPixelY);
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
