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

    public SpriteRenderer LevelRenderer;

    public Node[,] Grid;

    public Texture2D LevelTexture;
    Texture2D m_levelTextureInstance;

    public Color emptyColor;
    public Color bridgeColor;

    int m_maxX;
    int m_maxY;

    public float UnitPerPixel = 0.01f;

    public Vector3 MousePosition;
    Node m_currentNode;
    Node m_prevNode;

    public Transform SpawnTransform;
    public Vector3 SpawnPosition;
    public Node SpawnNode;

    public Lemming CurrentLemming;

    private LemmingsManager m_lemmingsManager;

    private ABILITY TargetAbility;

    private UIManager m_uiManager;

    private List<Node> m_nodesToClear = new List<Node>();

    private List<Node> m_nodesBridgeToAdd = new List<Node>();

    private UIButton m_currentUIButton;

    public Color presseColor;

    public Color unpressedColor;

    public float FillInterval;

    private float m_fillTimer;

    public Color fillColor;

    private LevelEditor m_levelEditor;

    private List<FillNode> m_fillNodes = new List<FillNode>();
    private bool m_applyTexture;

    public GAME_STATE GameState;

    public GameObject PlayCanvasObj;

    public GameObject EditCanvasObj;

    public GameObject InitCanvasObj;

    private void Awake()
    {
        Instance = this;
        ChangeGameState(GAME_STATE.INIT);
               
    }

    private void Start()
    {
        m_levelEditor = LevelEditor.Instance;
        m_lemmingsManager = LemmingsManager.Instance;
        m_uiManager = UIManager.Instance;
    }



    public void Play(Texture2D _leveltexture)
    {
        LevelTexture = _leveltexture;
        ChangeGameState(GAME_STATE.PLAY);
        
    }



    private void CreateLevel()
    {
        m_maxX = LevelTexture.width;
        m_maxY = LevelTexture.height;
        Grid = new Node[m_maxX, m_maxY];

        //m_levelTextureInstance = new Texture2D(m_maxX, m_maxY);
        //m_levelTextureInstance.filterMode = FilterMode.Point;
        //m_levelTextureInstance.SetPixels(LevelTexture.GetPixels());
        //m_levelTextureInstance.Apply();

        m_levelTextureInstance = Instantiate(LevelTexture);

        for (int x = 0; x < m_maxX; x++)
        {
            for (int y = 0; y < m_maxY; y++)
            {
                Node node = new Node();
                node.x = x;
                node.y = y;

                Color pixelColor = LevelTexture.GetPixel(x, y);
                node.isEmpty = (pixelColor.a == 0f);
                Grid[x, y] = node;
                
            }
        }

        GenerateLevelSprite(m_levelTextureInstance);

    }

    public void GenerateLevelSprite(Texture2D _leveltexture)
    {       
        Rect rect = new Rect(0, 0, _leveltexture.width, _leveltexture.height);
        Debug.Log(_leveltexture.width + " " + _leveltexture.height);
        Sprite sprite = Sprite.Create(_leveltexture, rect, Vector2.zero, 100, 1, SpriteMeshType.Tight);
   

        LevelRenderer.sprite = sprite;
    }

    private void Update()
    {  

        switch (GameState)
        {
            case GAME_STATE.EDIT:
                
                m_levelEditor.HandleMouseInput();
                GetMousePosition();
                m_uiManager.Tick();
                break;
            case GAME_STATE.PLAY:
                GetMousePosition();
                m_uiManager.Tick();
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
                m_uiManager.Tick();
                break;

        } 
        
           
    }

    public void ChangeGameState(GAME_STATE _gameState)
    {
        PlayCanvasObj.SetActive(false);
        EditCanvasObj.SetActive(false);
        InitCanvasObj.SetActive(false);

        switch (_gameState)
        {
            case GAME_STATE.PLAY:
                Debug.Log("play");
                GameState = _gameState;
                CreateLevel();
                SpawnNode = GetNodeFromWorldPosition(SpawnTransform.position);
                SpawnPosition = GetWorldPositionFromNode(SpawnNode);
                PlayCanvasObj.SetActive(true);
                break;
            case GAME_STATE.INIT:
                Debug.Log("init");
                GameState = _gameState;
                InitCanvasObj.SetActive(true);
                break;
            case GAME_STATE.EDIT:
                Debug.Log("edit");
                GameState = _gameState;
                EditCanvasObj.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void AddFillNode(FillNode _fillNode)
    {
        m_fillNodes.Add(_fillNode);
        m_applyTexture = true;
    }

    private void HandleFillNodes()
    {
        m_fillTimer += Time.deltaTime;

        if (m_fillTimer >= FillInterval)
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
                m_levelTextureInstance.SetPixel(downNode.x, downNode.y, fillColor);
                currentFillNode.y = downNode.y;
                m_nodesToClear.Add(currentNode);
            }
            else
            {
                Node downForward = GetNode(currentFillNode.x + 1, currentNode.y - 1);


                if (downForward != null && downForward.isEmpty)
                {
                    
                    downForward.isEmpty = false;
                    m_levelTextureInstance.SetPixel(downForward.x, downForward.y, fillColor);
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
                        m_levelTextureInstance.SetPixel(backForward.x, backForward.y, fillColor);
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
          
    private void HandleClearNodes()
    {
        if (m_nodesToClear.Count == 0)
            return;

        for (int i = 0; i < m_nodesToClear.Count; i++)
        {
            m_nodesToClear[i].isEmpty = true;
            m_levelTextureInstance.SetPixel(m_nodesToClear[i].x, m_nodesToClear[i].y, emptyColor);

        }
        m_levelTextureInstance.Apply();
        m_nodesToClear.Clear();
    }

    private void HandleBridgeNodes()
    {
        if (m_nodesBridgeToAdd.Count == 0)
            return;

        for (int i = 0; i < m_nodesBridgeToAdd.Count; i++)
        {         
            m_levelTextureInstance.SetPixel(m_nodesBridgeToAdd[i].x, m_nodesBridgeToAdd[i].y, bridgeColor);
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

   

    private void HandleUnit()
    {
        if (!m_uiManager.IsOverUnit)
            return;
       

        if (Input.GetMouseButtonDown(0))
        {
            bool canChange = CurrentLemming.ChangeAbility(TargetAbility);
            m_uiManager.ChangeAbility(canChange);
        }
    }

    private void CheckForUnits()
    {
        CurrentLemming = m_lemmingsManager.GetClosestUnit();
        if (CurrentLemming == null)
        {
            m_uiManager.IsOverUnit = false;
        }
        else
        {
            m_uiManager.IsOverUnit = true;
        }
    }

   

    void GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        MousePosition = ray.GetPoint(3);
        MousePosition.z = 0f;
        m_currentNode = GetNodeFromWorldPosition(MousePosition);
    }

    public Node GetNode(int x, int y)
    {
        if (x < 0 || y < 0 || x > m_maxX - 1 || y > m_maxY - 1)
        {

            return null; 
        }
           
        return Grid[x, y];
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

    internal void ButtonPressed(UIButton _uiButton)
    {
        if(m_currentUIButton != null)
        {
            m_currentUIButton.image.color = unpressedColor;
        }
        m_currentUIButton = _uiButton;
        m_currentUIButton.image.color = presseColor;
        TargetAbility = _uiButton.ability;

    }
}
