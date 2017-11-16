using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ABILITY
{
    WALKER,
    STOPPER,
    UMBRELLA,
    EXPLODE,
    DIG_DOWN
}

public class Lemming : MonoBehaviour {

    Node m_currentNode;
    Node m_targetNode;

    Vector3 m_startingPosition;
    Vector3 m_targetPosition;
    bool m_lerp;
    float m_t;
    bool m_movingLeft;
    bool m_onGround;
    bool m_prevOnGround;
    int m_framesInAir;
    bool m_isInitialized;

    public float MaxStoppingTime;
    private float m_currentStoppingTime;
    private List<Node> m_stoppedNodesList = new List<Node>();

    public float TimeToExplode;
    private float m_currentTimeToExplode;

    public float MaxDigDownTime;
    public int DigDownRadius;
    private float m_currentDigDownTime;
   // private bool m_isDiggingDown;

    private bool m_hasUmbrella;
     

    int m_targetX;
    int m_targetY;

    float m_baseSpeed;
    public float MaxLerpSpeed;
    [Range(.01f, 1f)]
    public float FallSpeedPrc;
    [Range(.01f, 1f)]
    public float WalkSpeedPrc;
    [Range(.01f, 1f)]
    public float UmbrellFallSpeedSpeedPrc;
    [Range(.01f, 1f)]
    public float DigDownSpeedPrc;
    private float m_currentLerpSpeed;


    public SpriteRenderer Renderer;

    Animator m_anim;

    GameManager gameManager;

    LemmingsManager lemmingsManager;

    public ABILITY CurrentAbility;

    private void Awake()
    {
        m_anim = GetComponent<Animator>();      
    }

    public void Initialize()
    {
        gameManager = GameManager.Instance;
        lemmingsManager = LemmingsManager.Instance;
        m_currentNode = gameManager.SpawnNode;
        transform.position = gameManager.SpawnPosition;
        m_isInitialized = true;
        m_currentLerpSpeed = MaxLerpSpeed * FallSpeedPrc;
        CurrentAbility = ABILITY.WALKER;
    }

    public void Tick(float _delta)
    {
        if (m_currentNode == null)
            return;
        if (!m_isInitialized)
            return;

        switch (CurrentAbility)
        {
            case ABILITY.WALKER:           
                Walker(_delta);
                break;
            case ABILITY.STOPPER:
                Stopper(_delta);           
                break;
            case ABILITY.UMBRELLA:
                Walker(_delta);
                break;
            case ABILITY.EXPLODE:
                Explode(_delta);
                break;
            case ABILITY.DIG_DOWN:
                DigDown(_delta);
                break;
            default:
                break;
        }

       
        Renderer.flipX = m_movingLeft;
    }

    public bool ChangeAbility(ABILITY _targetAbility)
    {
        m_hasUmbrella = false;
        //m_isDiggingDown = false;

        switch (_targetAbility)
        {
            case ABILITY.WALKER:
                m_anim.Play("Walk");
                CurrentAbility = _targetAbility;
                return true;
            case ABILITY.EXPLODE:
                m_currentTimeToExplode = 0f;
                m_anim.Play("Explode");
                CurrentAbility = _targetAbility;
                return true;
            case ABILITY.UMBRELLA:
                if (m_onGround)
                    return false;
                m_hasUmbrella = true;               
                m_anim.Play("Umbrella");
                CurrentAbility = _targetAbility;
                return true;
            case ABILITY.STOPPER:
                if (!m_prevOnGround)
                    return false;
                m_anim.Play("Stop");
                CurrentAbility = _targetAbility;
                m_currentStoppingTime = 0f;
                StopNodes();
                return true;

            case ABILITY.DIG_DOWN:
                if (!m_prevOnGround)
                    return false;
                m_anim.Play("Dig down");
                //m_isDiggingDown = true;
                CurrentAbility = _targetAbility;
                m_currentDigDownTime = 0f;              
                return true;

            default:
                return false;
        }
    }

    private List<Node> FindNodes(Node _centerNode, int _radius)
    {
        List<Node> returnList = new List<Node>();
        for (int x = -_radius; x < _radius; x++)
        {
            for (int y = -_radius; y < _radius; y++)
            {
                Node node = gameManager.GetNode(_centerNode.x + x, _centerNode.y + y);
                if (node == null)
                    continue;
                if (node.isEmpty)
                    continue;
                if(Vector3.Distance(gameManager.GetWorldPositionFromNode(_centerNode), gameManager.GetWorldPositionFromNode(node)) > _radius / gameManager.pixelsPerUnit)
                {
                    continue;
                }
                returnList.Add(node);
            }
        }
        return returnList;
    }

    private List<Node> FindNodes(int _minX, int _maxX, int _minY, int _maxY, int _radius)
    {
        List<Node> returnList = new List<Node>();
        for (int x = _minX; x < _maxX; x++)
        {
            for (int y = _minY; y < _maxY; y++)
            {
                Node node = gameManager.GetNode(m_currentNode.x + x, m_currentNode.y + y);
                if (Vector3.Distance(gameManager.GetWorldPositionFromNode(m_currentNode), gameManager.GetWorldPositionFromNode(node)) > _radius / gameManager.pixelsPerUnit)
                {
                    continue;
                }
                returnList.Add(node);
            }
        }
        return returnList;
    }

    private void Explode(float _delta)
    {
        
        m_currentTimeToExplode += _delta;
        if(m_currentTimeToExplode >= TimeToExplode)
        {
            Debug.Log("claer");        
            gameManager.AddNodesToClear(FindNodes(m_currentNode, 10));
            lemmingsManager.RemoveLemming(this);
            gameObject.SetActive(false);
        }
    }

    private void LerpToPosition(float _delta)
    {
        m_t += _delta * m_baseSpeed;
        if (m_t > 1)
        {
            m_t = 1;
            m_lerp = false;
            m_currentNode = m_targetNode;
        }
        Vector3 targetPosition = Vector3.Lerp(m_startingPosition, m_targetPosition, m_t);
        transform.position = targetPosition;
    }

    private void Walker(float _delta)
    {
        if (!m_lerp)
        {
            PathFind();

            OnLerpEnd();

            float distance = Vector3.Distance(m_startingPosition, m_targetPosition);
            m_baseSpeed = m_currentLerpSpeed / distance;
            if (m_hasUmbrella)
                m_baseSpeed /= 2f;
        }
        else
        {
            LerpToPosition(_delta);
        }
    }

    private void OnLerpEnd()
    {
        m_startingPosition = transform.position;
        m_lerp = true;
        m_t = 0f;
        Vector3 targetPosition = gameManager.GetWorldPositionFromNode(m_targetNode);
        m_targetPosition = targetPosition;      
    }

    private void Stopper(float _delta)
    {
        m_currentStoppingTime += _delta;
        if(m_currentStoppingTime >= MaxStoppingTime)
        {
            ChangeAbility(ABILITY.WALKER);
            EndStoppingNodes();
        }
    }

    private void DigDown(float _delta)
    {
        m_currentDigDownTime += Time.deltaTime;

        if (m_currentDigDownTime >= MaxDigDownTime)
        {
            ChangeAbility(ABILITY.WALKER);
            return;
        }

        if (!CheckIfHasGround(7))
        {
            ChangeAbility(ABILITY.WALKER);
            Debug.Log("no ground");
            return;
        }


        if (!m_lerp)
        {
            gameManager.AddNodesToClear(FindNodes(m_currentNode, 5));

            Node nextDown = gameManager.GetNode(m_currentNode.x, m_currentNode.y - 1);
            if (nextDown.isEmpty)
                m_targetNode = nextDown;
            else
                m_targetNode = m_currentNode;

            OnLerpEnd();
            float distance = Vector3.Distance(m_startingPosition, m_targetPosition);
       
            m_currentLerpSpeed = MaxLerpSpeed * DigDownSpeedPrc;

            m_baseSpeed = m_currentLerpSpeed / distance;
            
        }
        else
        {
            LerpToPosition(_delta);
        }
    }

    private bool CheckIfHasGround(int _height)
    {
        for (int i = 1; i < _height; i++)
        {
            Node nextDown = gameManager.GetNode(m_currentNode.x, m_currentNode.y - i);
            if(nextDown == null)
            {
                return false;
            }
            if (!nextDown.isEmpty)
            {
                return true;
            }          
        }
        return false;
    }

    private void EndStoppingNodes()
    {
        for (int i = 0; i < m_stoppedNodesList.Count; i++)
        {
            m_stoppedNodesList[i].isStopped = false;
        }
        m_stoppedNodesList.Clear();
    }

    private void StopNodes()
    {
        for (int x = -4; x < 4; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                Node node = gameManager.GetNode(m_currentNode.x + x, m_currentNode.y + y);
                node.isStopped = true;
                m_stoppedNodesList.Add(node);
            }
        }
    }

    private void PathFind()
    {
        m_prevOnGround = m_onGround;
        m_targetX = m_currentNode.x;
        m_targetY = m_currentNode.y;

        bool isNextDownAir = CheckIfEmpty(m_targetX, m_targetY - 1);
        if (isNextDownAir)
        {
            m_framesInAir++;
            if (m_onGround)
            {              
                if (m_framesInAir >= 4)
                {
                    m_onGround = false;
                    if (m_hasUmbrella)
                    {
                        m_anim.Play("Umbrella");
                        m_currentLerpSpeed = MaxLerpSpeed * UmbrellFallSpeedSpeedPrc;
                    }

                    else
                    {
                        m_anim.Play("Fall");
                        m_currentLerpSpeed = MaxLerpSpeed * FallSpeedPrc;
                    }
                        
                    
                }
            }
            
            
            
            m_targetY--;
        }
        else
        {
            if(m_framesInAir >= 200 && !m_hasUmbrella)
            {
                ChangeAbility(ABILITY.EXPLODE);
                return;
            }
            m_framesInAir = 0;
            m_onGround = true;
            if (!m_prevOnGround)
            {
                if (m_hasUmbrella)
                {
                    ChangeAbility(ABILITY.WALKER);
                }
                m_anim.Play("Walk");
                m_currentLerpSpeed = MaxLerpSpeed * WalkSpeedPrc;
            }
                
            int x = (m_movingLeft) ? -1 : 1;
            bool isNexSideAir = CheckIfEmpty(m_targetX + x, m_targetY);
            if (isNexSideAir)
            {
                m_targetX += x;
            }
            else
            {
                bool canStep = false;
                int stepHeight = 0;
                for (int i = 1; i < 4; i++)
                {
                    bool isStepAir = CheckIfEmpty(m_targetX, m_targetY + i);
                    if (isStepAir)
                    {
                        stepHeight = i;
                        canStep = true;
                        break;
                    }
                }
                if (canStep)
                {
                    m_targetY += stepHeight;
                    m_targetX += x;

                }
                else
                {
                    m_movingLeft = !m_movingLeft;
                    
                    int x2 = (m_movingLeft) ? -1 : 1;
                    m_targetX += x2;
                }
            }
        }
        
        m_targetNode = gameManager.GetNode(m_targetX, m_targetY);
    }

    private bool CheckIfEmpty(int _x, int _y)
    {
        Node node = gameManager.GetNode(_x, _y);
        if (node == null)
            return true;
        
        if (node.isEmpty)
        {
            if (node.isStopped)
                return false;
            return true;
        }                  
        return false;
    }
}
