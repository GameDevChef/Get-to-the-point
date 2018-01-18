using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FillNode
{
    public int x;
    public int y;
    public int timesNotMoved;
}

[System.Serializable]
public class Node
{
    public int x;
    public int y;
    public bool isEmpty;
    public bool isStoppedLeft;
    public bool isStoppedRight;
    
}

public enum GAME_STATE
{
    PLAY,
    INIT,
    EDIT
}

public class GameManager : MonoBehaviour {

    public static GameManager Instance;

    [Header("References")]
    [Space(10)]

    [SerializeField]
    GameObject m_playCanvasGO;

    [SerializeField]
    GameObject m_editCanvasGO;

    [SerializeField]
    GameObject m_initCanvasGO;

    [SerializeField]
    SpriteRenderer m_levelRenderer;

    [HideInInspector]
    public Node m_SpawnNode;

    [HideInInspector]
    public Lemming m_CurrentLemming;

    Node[,] m_grid;

    Texture2D m_levelTexture;

    LevelEditor m_levelEditor;

    LemmingsManager m_lemmingsManager;

    UIManager m_UIManager;

    Texture2D m_levelTextureInstance;

    List<Node> m_nodesToClear = new List<Node>();

    List<Node> m_nodesBridgeToAdd = new List<Node>();

    List<FillNode> m_fillNodes = new List<FillNode>();

    Node m_currentNode;

    Node m_prevNode;

    UIButton m_currentAbilityButton;

    UIButton m_currentBrushSizeButton;

    UIButton m_currentBrushColorButton;

    [Header("Variables")]
    [Space(10)]

    public  bool m_IsLevelLoaded;

    public float UnitPerPixel = 0.01f;

    [SerializeField]
    Color m_emptyColor;

    [SerializeField]
    Color m_bridgeColor;

    

    [SerializeField]
    Color m_sandColor;

    [SerializeField]
    Color m_pressedButtonColor;

    [SerializeField]
    Color m_releasedButtonColor;

    [SerializeField]
    float m_fillInterval;

    [HideInInspector]
    public Vector3 m_SpawnPosition;

    [HideInInspector]
    public Vector3 m_MousePosition;

    [HideInInspector]
    public GAME_STATE m_GameState;

    [HideInInspector]
    public Vector2 m_ExitVector;

    [HideInInspector]
    public Vector2 m_SpawnVector;

    float m_fillTimer;

    int m_maxX;

    int m_maxY;

    ABILITY m_targetAbility;

    int m_lemmingsExitCount;

    bool m_applyTexture;


    void Awake()
    {
        Instance = this;
        ChangeGameState(GAME_STATE.INIT);              
    }

    void Start()
    {
        m_levelEditor = LevelEditor.Instance;
        m_lemmingsManager = LemmingsManager.Instance;
        m_UIManager = UIManager.Instance;
    }

    public void Play(Texture2D _leveltexture)
    {       
        m_levelTexture = _leveltexture;
        ChangeGameState(GAME_STATE.PLAY);
        m_UIManager.ChangeCursorSprite(m_UIManager.m_EmptyCursorSprite);
        Camera.main.GetComponent<CameraMovement>().SetCameraBounds(_leveltexture.width, _leveltexture.height);
    }

    void CreateLevel()
    {
        m_maxX = m_levelTexture.width;
        m_maxY = m_levelTexture.height;
        m_grid = new Node[m_maxX, m_maxY];

        //m_levelTextureInstance = new Texture2D(m_maxX, m_maxY);
        //m_levelTextureInstance.filterMode = FilterMode.Point;
        //m_levelTextureInstance.SetPixels(LevelTexture.GetPixels());
        //m_levelTextureInstance.Apply();

        m_levelTextureInstance = Instantiate(m_levelTexture);

        for (int x = 0; x < m_maxX; x++)
        {
            for (int y = 0; y < m_maxY; y++)
            {
                Node node = new Node();
                node.x = x;
                node.y = y;

                Color pixelColor = m_levelTexture.GetPixel(x, y);
                node.isEmpty = (pixelColor.a == 0f /*|| CheckIfInExitRange(node)*/);
                m_grid[x, y] = node;              
            }
        }
        GenerateLevelSprite(m_levelTextureInstance);
    }

    public void GenerateLevelSprite(Texture2D _leveltexture)
    {       
        Rect rect = new Rect(0, 0, _leveltexture.width, _leveltexture.height);
        Sprite sprite = Sprite.Create(_leveltexture, rect, Vector2.zero, 100, 1, SpriteMeshType.FullRect);
        m_levelRenderer.sprite = sprite;
    }

    void Update()
    {  
        switch (m_GameState)
        {
            case GAME_STATE.EDIT:               
                m_levelEditor.HandleMouseInput();
                GetMousePosition();
                m_UIManager.Tick();
                break;
            case GAME_STATE.PLAY:
                GetMousePosition();
                m_UIManager.Tick();
                CheckForUnits();
                HandleUnit();
                HandleFillNodes();
                HandleClearNodes();
                HandleBridgeNodes();
                m_lemmingsManager.Tick(Time.deltaTime);
                if (m_applyTexture)
                {
                    m_levelTextureInstance.Apply();
                }
                break;
            case GAME_STATE.INIT:
                GetMousePosition();
                m_UIManager.Tick();
                break;
        }                 
    }

    public void ChangeGameState(GAME_STATE _gameState)
    {
        m_playCanvasGO.SetActive(false);
        m_editCanvasGO.SetActive(false);
        m_initCanvasGO.SetActive(false);

        switch (_gameState)
        {
            case GAME_STATE.PLAY:
                Debug.Log("play");
                m_GameState = _gameState;
                CreateLevel();
                SetSpawnPositions();
               
                m_playCanvasGO.SetActive(true);
                break;
            case GAME_STATE.INIT:
                Debug.Log("init");
                m_GameState = _gameState;
                m_initCanvasGO.SetActive(true);
                break;
            case GAME_STATE.EDIT:
                Debug.Log("edit");
                m_GameState = _gameState;
                m_editCanvasGO.SetActive(true);
                break;
            default:
                break;
        }
    }

    public bool CheckIfInExitRange(Node _node)
    {
        if (_node == null)
            return false;
        if (_node.x < m_ExitVector.x + 22 && _node.x > m_ExitVector.x - 22 && _node.y < m_ExitVector.y + 10 && _node.y > m_ExitVector.y - 22)
        {
            return true;
        }
        return false;
    }

    void SetSpawnPositions()
    {
        m_SpawnNode = GetNode((int)m_SpawnVector.x, (int)m_SpawnVector.y);
        m_SpawnPosition = GetWorldPositionFromNode(m_SpawnNode);                   
    }

   public void OnLemmingExit()
    {
        m_lemmingsExitCount++;
        Debug.Log(m_lemmingsExitCount);
    }

    public void AddFillNode(FillNode _fillNode)
    {
        m_fillNodes.Add(_fillNode);
        m_applyTexture = true;
    }

    void HandleFillNodes()
    {
        m_fillTimer += Time.deltaTime;

        if (m_fillTimer >= m_fillInterval)
        {
            m_fillTimer = 0f;
        }
        else
            return;

        if (m_fillNodes.Count == 0)
            return;


        for (int i = 0; i < m_fillNodes.Count; i++)
        {
            FillNode currentFillNode = m_fillNodes[i];
            Node currentNode = GetNode(currentFillNode.x, currentFillNode.y);
            Node downNode = GetNode(currentFillNode.x, currentFillNode.y - 1);

          

            if (downNode != null && downNode.isEmpty)
            {
                
                //downNode.isEmpty = false;
                m_levelTextureInstance.SetPixel(downNode.x, downNode.y, m_sandColor);
                currentFillNode.y = downNode.y;
                m_nodesToClear.Add(currentNode);
            }
            else
            {
                Node downForward = GetNode(currentFillNode.x + 1, currentNode.y - 1);


                if (downForward != null && downForward.isEmpty)
                {
                    
                    downForward.isEmpty = false;
                    m_levelTextureInstance.SetPixel(downForward.x, downForward.y, m_sandColor);
                    currentFillNode.y = downForward.y;
                    currentFillNode.x = downForward.x;
                    m_nodesToClear.Add(currentNode);
                }
                else
                {
                    Node backForward = GetNode(currentFillNode.x - 1, currentNode.y - 1);
                    if (backForward != null && backForward.isEmpty)
                    {
 

                        backForward.isEmpty = false;
                        m_levelTextureInstance.SetPixel(backForward.x, backForward.y, m_sandColor);
                        currentFillNode.y = backForward.y;
                        currentFillNode.x = backForward.x;
                        m_nodesToClear.Add(currentNode);
                    }
                    else
                    {
                        currentFillNode.timesNotMoved++;
                        GetNode(currentFillNode.x, currentFillNode.y).isEmpty = false;
                        if(currentFillNode.timesNotMoved > 10)
                        {
                            m_fillNodes.Remove(currentFillNode);
                        }
                    }
                }
            }
        }
    }
          
    void HandleClearNodes()
    {
        if (m_nodesToClear.Count == 0)
            return;

        for (int i = 0; i < m_nodesToClear.Count; i++)
        {
            m_nodesToClear[i].isEmpty = true;
            m_levelTextureInstance.SetPixel(m_nodesToClear[i].x, m_nodesToClear[i].y, m_emptyColor);

        }
        m_levelTextureInstance.Apply();
        m_nodesToClear.Clear();
    }

    void HandleBridgeNodes()
    {
        if (m_nodesBridgeToAdd.Count == 0)
            return;

        for (int i = 0; i < m_nodesBridgeToAdd.Count; i++)
        {         
            m_levelTextureInstance.SetPixel(m_nodesBridgeToAdd[i].x, m_nodesBridgeToAdd[i].y, m_bridgeColor);
        }
        m_levelTextureInstance.Apply();
        m_nodesToClear.Clear();
    }

    public void AddNodesToClear(List<Node> _nodesToClear)
    {
        m_nodesToClear.AddRange(_nodesToClear);
    }

    public void AddNodesToBridge(List<Node> _nodesToBridge, bool _movingLeft)
    {
        for (int i = 0; i < _nodesToBridge.Count; i++)
        {
            if (_movingLeft == true)
                _nodesToBridge[i].isStoppedLeft = true;
            else
                _nodesToBridge[i].isStoppedRight = true;
        }
        m_nodesBridgeToAdd.AddRange(_nodesToBridge);
    }

    void HandleUnit()
    {
        if (!m_UIManager.IsOverUnit)
            return;
       

        if (Input.GetMouseButtonDown(0))
        {
            bool canChange = m_CurrentLemming.ChangeAbility(m_targetAbility);
            m_UIManager.ChangeAbility(canChange);
        }
    }

    void CheckForUnits()
    {
        m_CurrentLemming = m_lemmingsManager.GetClosestUnit();
        if (m_CurrentLemming == null)
        {
            m_UIManager.IsOverUnit = false;
        }
        else
        {
            m_UIManager.IsOverUnit = true;
        }
    } 

    void GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        m_MousePosition = ray.GetPoint(3);
        m_MousePosition.z = 0f;
        m_currentNode = GetNodeFromWorldPosition(m_MousePosition);
    }

    public Node GetNode(int x, int y)
    {
        if (x < 0 || y < 0 || x > m_maxX - 1 || y > m_maxY - 1)
        {

            return null; 
        }
           
        return m_grid[x, y];
    }

    public Node GetNodeFromWorldPosition(Vector3 _position, bool _debug = false)
    {
       
        int x = Mathf.RoundToInt(_position.x / UnitPerPixel);
        int y = Mathf.RoundToInt(_position.y / UnitPerPixel);   

        return GetNode(x, y);
    }

    public Vector3 GetWorldPositionFromNode(int _x, int _y)
    {
        float x = _x * UnitPerPixel;
        float y = _y * UnitPerPixel;
        return new Vector3(x, y, 0f);
    }

    public Vector3 GetWorldPositionFromNode(Node _node)
    {
        if (_node == null)
            return -Vector3.one;

        float x = _node.x * UnitPerPixel;
        float y = _node.y * UnitPerPixel;
        return new Vector3(x, y, 0f);
    }

    public void ButtonPressed(UIButton _uiButton)
    {
        switch (_uiButton.m_ButtonType)
        {
            case BUTTON_TYPE.ABILITY:
                Press(m_currentAbilityButton, _uiButton);
                m_currentAbilityButton = _uiButton;
                m_currentAbilityButton.m_Image.color = m_pressedButtonColor;
                break;

            case BUTTON_TYPE.BRUSH_SIZE:
                Press(m_currentBrushSizeButton, _uiButton);
                m_currentBrushSizeButton = _uiButton;
                m_currentBrushSizeButton.m_Image.color = m_pressedButtonColor;
                break;
                
            case BUTTON_TYPE.BRUSH_COLOR:
                Press(m_currentBrushColorButton, _uiButton);
                m_currentBrushColorButton = _uiButton;
                m_currentBrushColorButton.m_Image.color = m_pressedButtonColor;
                break;
            default:
                break;
        }
    }

    void Press(UIButton _targetButton, UIButton _pressedButton)
    {
        if (_targetButton != null)
        {
            _targetButton.m_Image.color = m_releasedButtonColor;
        }
        
       
        if (m_GameState != GAME_STATE.PLAY || _pressedButton.m_ButtonType != BUTTON_TYPE.ABILITY)
            return;
        m_targetAbility = _pressedButton.ability;
    }

    public void BackToInit()
    {
        ChangeGameState(GAME_STATE.INIT);
    }
}
