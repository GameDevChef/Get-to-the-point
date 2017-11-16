using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    public int x;
    public int y;
    public bool isEmpty;
    public bool isStopped;
}

public class GameManager : MonoBehaviour {

    public static GameManager Instance;

    public SpriteRenderer LevelRenderer;

    public Node[,] Grid;

    public Texture2D LevelTexture;
    Texture2D m_levelTextureInstance;

    public Color emptyColor;

    int m_maxX;
    int m_maxY;

    public float pixelsPerUnit = 100f;

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

    private UIButton m_currentUIButton;

    public Color presseColor;

    public Color unpressedColor;

    private void Awake()
    {
        Instance = this;
 
        CreateLevel();
        SpawnNode = GetNodeFromWorldPosition(SpawnTransform.position);
        SpawnPosition = GetWorldPositionFromNode(SpawnNode);
    }

    private void Start()
    {
        m_lemmingsManager = LemmingsManager.Instance;
        m_uiManager = UIManager.Instance;
        TargetAbility = ABILITY.UMBRELLA;
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
       
        Rect rect = new Rect(0, 0, m_maxX, m_maxY);      
        LevelRenderer.sprite = Sprite.Create(m_levelTextureInstance, rect, Vector2.zero);

    }

    private void Update()
    {
        GetMousePosition();
       // HandleMouseInput();
        CheckForUnits();
        m_uiManager.Tick();
        ClearNodes();
        HandleUnit();
        m_lemmingsManager.Tick(Time.deltaTime);
    }

    private void ClearNodes()
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

    public void AddNodesToClear(List<Node> _nodesToClear)
    {
        m_nodesToClear.AddRange(_nodesToClear);
    }

    private void HandleUnit()
    {
        if (!m_uiManager.IsOverUnit)
            return;
        if (TargetAbility == CurrentLemming.CurrentAbility)
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

    private void HandleMouseInput()
    {
        if (m_currentNode == null)
            return;

        if (Input.GetMouseButton(0))
        {
            
            if (m_currentNode == m_prevNode)
                return;

            m_prevNode = m_currentNode;

            for (int x = -6; x < 6; x++)
            {
                for (int y = -6; y < 6; y++)
                {
                    int nodeX = m_currentNode.x + x;
                    int nodeY = m_currentNode.y + y;
                    m_levelTextureInstance.SetPixel(nodeX, nodeY, emptyColor);
                    Node node = GetNode(nodeX, nodeY);
                    if (node == null)
                        continue;
                    node.isEmpty = true;
                }
            }
            m_levelTextureInstance.Apply();
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
        if (x < 0 || y < 0 || x > m_maxX || y > m_maxY)
            return null;
        return Grid[x, y];
    }

    public Node GetNodeFromWorldPosition(Vector3 _position)
    {
        int x = Mathf.RoundToInt(_position.x * pixelsPerUnit);
        int y = Mathf.RoundToInt(_position.y * pixelsPerUnit);

        return GetNode(x, y);
    }

    public Vector3 GetWorldPositionFromNode(int _x, int _y)
    {
        float x = _x / (float)pixelsPerUnit;
        float y = _y / (float) pixelsPerUnit;
        return new Vector3(x, y, 0f);
    }

    public Vector3 GetWorldPositionFromNode(Node _node)
    {
        if (_node == null)
            return -Vector3.one;

        float x = _node.x / (float)pixelsPerUnit;
        float y = _node.y / (float)pixelsPerUnit;
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
