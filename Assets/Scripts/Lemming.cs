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
    DIG_DOWN,
    DIG_FORWARD,
    BUILD,
    FILL
}

public class Lemming : MonoBehaviour {

    [Header("References")]

    [SerializeField]
    SpriteRenderer m_renderer;

    Animator m_anim;

    GameManager m_gamemanager;

    LemmingsManager m_lemingsManager;

    List<Node> m_stoppedNodesList = new List<Node>();

    List<FillNode> m_fillNodes = new List<FillNode>();

    Node m_currentNode;

    Node m_targetNode;

    [Header("Variables")]
    [Space(10)]

    [Header("Ability max speed %")]

    [SerializeField]
    public float m_maxMoveSpeed;

    [Range(.01f, 1f)]
    [SerializeField]
    float m_fallSpeedPrc;

    [Range(.01f, 1f)]
    [SerializeField]
    float m_walkSpeedPrc;

    [Range(.01f, 1f)]
    [SerializeField]
    float m_umbrellFallSpeedPrc;

    [Range(.01f, 1f)]
    [SerializeField]
    float m_digDownSpeedPrc;

    [Range(.01f, 1f)]
    [SerializeField]
    float m_digForwardSpeedPrc;

    [Range(.01f, 1f)]
    [SerializeField]
    float m_buildSpeedPrc;

    [Header("Stopper Ability")]
    [SerializeField]
    float m_maxStoppingSpeed;
    
    float m_currentStoppingTime;

    [Header("Builder Ability")]

    [SerializeField]
    float m_maxBuildTime;

    [SerializeField]
    int m_buildLength;

    float m_currentBuildTime;

    [Header("Dig Down Ability")]

    [SerializeField]
    float m_maxDigDowmTime;

    [SerializeField]
    int m_digDownRadius;

    float m_currentDigDownTime;

    [Header("Dig Forward Ability")]

    [SerializeField]
    float m_maxDigForwardTime;

    [SerializeField]
    int m_digForwardRadius;

    float m_currentDigForwardTime;

    bool m_readyToDigForward;

    [Header("Explode Ability")]

    [SerializeField]
    float m_timeToExplode;

    float m_currentTimeToExplode;

    [Header("Filler Ability")]

    [SerializeField]
    float m_maxFillTime;

    [SerializeField]
    float m_fillSpawnInterval;

    float m_currentFillTime;

    float m_currentSpawnFillTime;

    Vector3 m_startingPosition;

    Vector3 m_targetPosition;

    ABILITY m_currentAbility;

    bool m_move;

    float m_t;

    bool m_movingLeft;

    bool m_onGround;

    bool m_prevOnGround;

    int m_framesInAir;

    bool m_isInitialized;

    bool m_hasExited;
     
    int m_targetX;

    int m_targetY;

    float m_baseSpeed;

    float m_currentMoveSpeed;

    bool m_hasUmbrella;

   
    void Awake()
    {
        m_anim = GetComponent<Animator>();      
    }

    public void Initialize()
    {
        m_gamemanager = GameManager.Instance;
        m_lemingsManager = LemmingsManager.Instance;
        m_currentNode = m_gamemanager.m_SpawnNode;
        transform.position = m_gamemanager.m_SpawnPosition;
        m_isInitialized = true;
        m_currentMoveSpeed = m_maxMoveSpeed * m_fallSpeedPrc;
        m_currentAbility = ABILITY.WALKER;
    }

    public void Tick(float _delta)
    {
        if (m_hasExited)
            return;
        if (m_currentNode == null)
            return;
        if (!m_isInitialized)
            return;

        switch (m_currentAbility)
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
            case ABILITY.DIG_FORWARD:
                DigForward(_delta);
                break;
            case ABILITY.BUILD:
                Build(_delta);
                break;
            case ABILITY.FILL:
                Fill(_delta);
                break;
            default:
                break;
        }       
        m_renderer.flipX = m_movingLeft;
    }

    void Fill(float _delta)
    {
        if (m_currentFillTime > m_maxFillTime)
        {
            ChangeAbility(ABILITY.WALKER);
            m_fillNodes.Clear();
            return;
        }                         
        m_currentFillTime += _delta;

        m_currentSpawnFillTime += _delta;

        if (m_currentSpawnFillTime > m_fillSpawnInterval)
        {
            m_currentSpawnFillTime = 0f;

            if (m_fillNodes.Count > 0)
            {
                if (m_fillNodes[m_fillNodes.Count - 1].timesNotMoved > 0)
                {
                    ChangeAbility(ABILITY.WALKER);
                    m_fillNodes.Clear();
                    return;
                }
            }

            FillNode fillNode = new FillNode();
            int x = (m_movingLeft) ? -1 : 1;
            Node spawnNode = m_gamemanager.GetNode(m_currentNode.x + x * 3, m_currentNode.y + 3);         

            fillNode.x = spawnNode.x;
            fillNode.y = spawnNode.y;

            m_fillNodes.Add(fillNode);
            m_gamemanager.AddFillNode(fillNode);           
        }
    }

    public bool ChangeAbility(ABILITY _targetAbility)
    {
        if (m_currentAbility != ABILITY.WALKER && _targetAbility != ABILITY.WALKER)
            return false;
        if (_targetAbility == m_currentAbility)
            return false;

        m_hasUmbrella = false;
        m_readyToDigForward = false;

        switch (_targetAbility)
        {
            case ABILITY.WALKER:
                m_anim.Play("Walk1");
                m_currentAbility = _targetAbility;
                m_currentMoveSpeed = m_onGround ? m_maxMoveSpeed * m_walkSpeedPrc : m_maxMoveSpeed * m_fallSpeedPrc;
               
                return true;

            case ABILITY.EXPLODE:
                m_currentTimeToExplode = 0f;
                m_anim.Play("Explode");
                m_currentAbility = _targetAbility;
                return true;

            case ABILITY.UMBRELLA:
                if (m_onGround)
                    return false;
                m_hasUmbrella = true;               
                m_anim.Play("Umbrella");
                m_currentAbility = _targetAbility;
                return true;

            case ABILITY.STOPPER:
                if (!m_prevOnGround)
                    return false;
                Debug.Log("stop");
                m_anim.Play("Stop1");
                m_currentAbility = _targetAbility;
                m_currentStoppingTime = 0f;
                StopNodes();
                return true;

            case ABILITY.DIG_DOWN:
                if (!m_prevOnGround)
                    return false;
                m_anim.Play("Dig down");
                m_currentAbility = _targetAbility;
                m_currentDigDownTime = 0f;              
                return true;

            case ABILITY.DIG_FORWARD:
                if (!m_prevOnGround)
                    return false;
                m_readyToDigForward = true;               
                return true;

            case ABILITY.BUILD:
                if (!m_prevOnGround)
                    return false;
                m_anim.Play("Build");
                m_currentAbility = _targetAbility;
                m_currentBuildTime = 0f;
                return true;

            case ABILITY.FILL:
                if (!m_prevOnGround)
                    return false;
                m_anim.Play("Fill");
                m_currentAbility = _targetAbility;
                m_currentBuildTime = 0f;
                return true;

            default:
                return false;
        }
    }

    List<Node> FindNodes(Node _centerNode, int _radius)
    {
        List<Node> returnList = new List<Node>();
        for (int x = -_radius; x < _radius; x++)
        {
            for (int y = -_radius; y < _radius; y++)
            {
                Node node = m_gamemanager.GetNode(_centerNode.x + x, _centerNode.y + y);
                if (node == null)
                    continue;
                if (node.isEmpty)
                    continue;
                if(Vector3.Distance(m_gamemanager.GetWorldPositionFromNode(_centerNode), m_gamemanager.GetWorldPositionFromNode(node)) > _radius * m_gamemanager.UnitPerPixel)
                {
                    continue;
                }
                returnList.Add(node);
            }
        }
        return returnList;
    }

    List<Node> FindNodes(int _minX, int _maxX, int _minY, int _maxY, int _radius)
    {
        List<Node> returnList = new List<Node>();

        if(_maxX < _minX)
        {
            int temp = _maxX;
            _maxX = _minX;
            _minX = temp;
        }

        for (int x = _minX; x < _maxX; x++)
        {
            for (int y = _minY; y < _maxY; y++)
            {
                Node node = m_gamemanager.GetNode(m_currentNode.x + x, m_currentNode.y + y);
                if (Vector3.Distance(m_gamemanager.GetWorldPositionFromNode(m_currentNode), m_gamemanager.GetWorldPositionFromNode(node)) > _radius * m_gamemanager.UnitPerPixel)
                {
                    continue;
                }
                returnList.Add(node);
            }
        }
        return returnList;
    }

    void Explode(float _delta)
    {
        
        m_currentTimeToExplode += _delta;
        if(m_currentTimeToExplode >= m_timeToExplode)
        {       
            m_gamemanager.AddNodesToClear(FindNodes(m_currentNode, 10));
            m_lemingsManager.RemoveLemming(this);
            gameObject.SetActive(false);
        }
    }

    void MoveToPosition(float _delta)
    {
        m_t += _delta * m_baseSpeed;
        if (m_t > 1)
        {
            m_t = 1;
            m_move = false;
            m_currentNode = m_targetNode;
            bool exited = m_gamemanager.CheckIfInExitRange(m_currentNode);
            if (exited)
            {
                gameObject.SetActive(false);
                m_gamemanager.OnLemmingExit();
                m_hasExited = true;
            }
            
        }
        Vector3 targetPosition = Vector3.Lerp(m_startingPosition, m_targetPosition, m_t);
        transform.position = targetPosition;
    }

    void Walker(float _delta)
    {
        if (!m_move)
        {
            PathFind();
            OnEndMove();

            float distance = Vector3.Distance(m_startingPosition, m_targetPosition); 
            m_baseSpeed = m_currentMoveSpeed / distance;
            if (m_hasUmbrella)
                m_baseSpeed /= 2f;
        }
        else
        {
            MoveToPosition(_delta);
        }
    }

    void OnEndMove()
    {
        m_startingPosition = transform.position;
        m_move = true;
        m_t = 0f;
        Vector3 targetPosition = m_gamemanager.GetWorldPositionFromNode(m_targetNode);
        m_targetPosition = targetPosition;      
    }

    void Stopper(float _delta)
    {
        m_currentStoppingTime += _delta;

        if (m_currentStoppingTime >= m_maxStoppingSpeed || !CheckIfHasGround(2))
        {
            ChangeAbility(ABILITY.WALKER);
            EndStoppingNodes();
        }
    }

    void DigDown(float _delta)
    {
        m_currentDigDownTime += Time.deltaTime;

        if (m_currentDigDownTime >= m_maxDigDowmTime)
        {
            ChangeAbility(ABILITY.WALKER);
            return;
        }

        if (!CheckIfHasGround(6))
        {
            ChangeAbility(ABILITY.WALKER);
            return;
        }

        if (!m_move)
        {
            m_gamemanager.AddNodesToClear(FindNodes(m_currentNode, m_digDownRadius));

            Node nextDown = m_gamemanager.GetNode(m_currentNode.x, m_currentNode.y - 1);
            if (nextDown.isEmpty)
                m_targetNode = nextDown;
            else
                m_targetNode = m_currentNode;

            OnEndMove();
            float distance = Vector3.Distance(m_startingPosition, m_targetPosition);
       
            m_currentMoveSpeed = m_maxMoveSpeed * m_digDownSpeedPrc;

            m_baseSpeed = m_currentMoveSpeed / distance;         
        }
        else
        {
            MoveToPosition(_delta);
        }
    }

    void DigForward(float _delta)
    {
        m_currentDigForwardTime += Time.deltaTime;

        if (m_currentDigForwardTime >= m_maxDigForwardTime)
        {
            ChangeAbility(ABILITY.WALKER);
            return;
        }

        if (!CheckIfHasGround(1))
        {
            ChangeAbility(ABILITY.WALKER);
            return;
        }


        if (!m_move)
        {
            Node centerNode = m_gamemanager.GetNode(m_currentNode.x, m_currentNode.y + m_digForwardRadius - 1);

            m_gamemanager.AddNodesToClear(FindNodes(centerNode, m_digForwardRadius));

            int x = (m_movingLeft) ? -1 : 1; 

            Node nextSide = m_gamemanager.GetNode(m_currentNode.x + x, m_currentNode.y);
            if (nextSide.isEmpty)
                m_targetNode = nextSide;
            else
                m_targetNode = m_currentNode;

            OnEndMove();
            float distance = Vector3.Distance(m_startingPosition, m_targetPosition);

            m_currentMoveSpeed = m_maxMoveSpeed * m_digForwardSpeedPrc;

            m_baseSpeed = m_currentMoveSpeed / distance;

        }
        else
        {
            MoveToPosition(_delta);
        }
    }

    void Build(float _delta)
    {
        m_currentBuildTime += Time.deltaTime;

        if (m_currentBuildTime >= m_maxBuildTime)
        {
            ChangeAbility(ABILITY.WALKER);
            return;
        }

        if (!m_move)
        {
            int x = (m_movingLeft) ? -1 : 1;

            m_gamemanager.AddNodesToBridge(FindNodes(-x , m_buildLength * x, 0, 1, 10), m_movingLeft);

            Node nextStep = m_gamemanager.GetNode(m_currentNode.x + x, m_currentNode.y + 1);
            if (nextStep.isEmpty)
                m_targetNode = nextStep;
            else
            {
                ChangeAbility(ABILITY.WALKER);
                return;
            }
                
            OnEndMove();
            float distance = Vector3.Distance(m_startingPosition, m_targetPosition);

            m_currentMoveSpeed = m_maxMoveSpeed * m_buildSpeedPrc;

            m_baseSpeed = m_currentMoveSpeed / distance;

        }
        else
        {
            MoveToPosition(_delta);
        }
    }

    bool CheckIfHasGround(int _height)
    {
        for (int i = 1; i <= _height; i++)
        {
            Node nextDown = m_gamemanager.GetNode(m_currentNode.x, m_currentNode.y - i);
            if (CheckIfEmpty(nextDown.x, nextDown.y))
            {
                return false;
            }       
        }
        return true;
    }

    void EndStoppingNodes()
    {
        for (int i = 0; i < m_stoppedNodesList.Count; i++)
        {
            m_stoppedNodesList[i].isStoppedLeft = false;
            m_stoppedNodesList[i].isStoppedRight = false;
        }
        m_stoppedNodesList.Clear();
    }

    void StopNodes()
    {
        for (int x = 0; x < 1; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                Node node = m_gamemanager.GetNode(m_currentNode.x + x, m_currentNode.y + y);
                node.isStoppedLeft = true;
                node.isStoppedRight = true;
                m_stoppedNodesList.Add(node);
            }          
        }
    }

    void PathFind()
    {
        m_prevOnGround = m_onGround;
        m_targetX = m_currentNode.x;
        m_targetY = m_currentNode.y;

        bool isNextDownAir = CheckIfEmpty(m_targetX, m_targetY - 1);

        if (isNextDownAir) // can go down case
        {
            m_framesInAir++;
            if (m_onGround)
            {              
                if (m_framesInAir >= 4) // been on ground and falling more thern 4 frames
                {
                    m_onGround = false;
                    if (m_hasUmbrella)
                    {
                        m_anim.Play("Umbrella");
                        m_currentMoveSpeed = m_maxMoveSpeed * m_umbrellFallSpeedPrc;
                    }

                    else
                    {
                        m_anim.Play("Fall1");
                        m_currentMoveSpeed = m_maxMoveSpeed * m_fallSpeedPrc;
                    }                                        
                }
            }
            m_targetY--;
        }
        else // has ground under
        {
            if(m_framesInAir >= 200 && !m_hasUmbrella) // death case
            {
                ChangeAbility(ABILITY.EXPLODE);
                return;
            }
            m_framesInAir = 0;
            m_onGround = true;
            if (!m_prevOnGround) // first frame on ground
            {
                if (m_hasUmbrella)
                {
                    ChangeAbility(ABILITY.WALKER);
                }
                m_anim.Play("Walk1");
                m_currentMoveSpeed = m_maxMoveSpeed * m_walkSpeedPrc;
            }
                
            int x = (m_movingLeft) ? -1 : 1;
            bool isNextSideAir = CheckIfEmpty(m_targetX + x, m_targetY);
            if (isNextSideAir) // can go sideways
            {
                m_targetX += x;
            }
            else // looking for a step
            {
                bool canStep = false;
                int stepHeight = 0;
                for (int i = 1; i < 4; i++)
                {
                    if (i >= 1) // dig forward if can
                    {
                        if (m_readyToDigForward)
                        {
                            m_readyToDigForward = false;
                            m_currentDigForwardTime = 0f;
                            m_anim.Play("Dig forward");
                            m_currentAbility = ABILITY.DIG_FORWARD;
                            return;
                        }
                    }
                    bool isStepAir = CheckIfEmpty(m_targetX + x, m_targetY + i);
                    if (isStepAir)
                    {
                        
                        stepHeight = i;
                        canStep = true;
                        break;
                    }
                }
                if (canStep) // step forward
                {

                    m_targetY += stepHeight;
                    m_targetX += x;

                }
                else // turn around
                {
                    m_movingLeft = !m_movingLeft;
                    
                    int x2 = (m_movingLeft) ? -1 : 1;
                    m_targetX += x2;
                }
            }
        }        
        m_targetNode = m_gamemanager.GetNode(m_targetX, m_targetY);
    }

    bool CheckIfEmpty(int _x, int _y)
    {
        Node node = m_gamemanager.GetNode(_x, _y);
        if (node == null)
            return true;
        
        if (node.isEmpty)
        {
            if (node.isStoppedLeft == true && m_movingLeft == true)
            {                
                return false;
            }
                
            if (node.isStoppedRight == true && m_movingLeft == false)
            {

                return false;
            }

            if (_y < m_currentNode.y && (node.isStoppedLeft || node.isStoppedRight))
                return false;

            return true;                           
        }                  
        return false;
    }
}
