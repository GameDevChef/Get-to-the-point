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
    bool m_hasExited;
    
    public float MaxStoppingTime;
    private float m_currentStoppingTime;
    private List<Node> m_stoppedNodesList = new List<Node>();
    private List<FillNode> m_fillNodes = new List<FillNode>();

    public float TimeToExplode;
    private float m_currentTimeToExplode;

    public float MaxDigDownTime;
    public int DigDownRadius;
    private float m_currentDigDownTime;

    public float MaxFillTime;
    public float FillSpawnInterval;
    private float m_currentFillTime;
    private float m_currentSpawnFillTime;

    private bool m_readyToDigForward;
    public float MaxDigForwardTime;
    public int DigForwardRadius;
    private float m_currentDigForwardTime;

    public float MaxBuildTime;
    public int BuildLength;
    private float m_currentBuildTime;

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
    [Range(.01f, 1f)]
    public float DigForwardSpeedPrc;
    [Range(.01f, 1f)]
    public float BuildSpeedPrc;

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
        if (m_hasExited)
            return;
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

       
        Renderer.flipX = m_movingLeft;
    }

    private void Fill(float _delta)
    {
        if (m_currentFillTime > MaxFillTime)
        {
            ChangeAbility(ABILITY.WALKER);
            m_fillNodes.Clear();
            return;
        }
        
                   
        m_currentFillTime += _delta;

        m_currentSpawnFillTime += _delta;

        if (m_currentSpawnFillTime > FillSpawnInterval)
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

            Node spawnNode = gameManager.GetNode(m_currentNode.x + x * 3, m_currentNode.y + 3);

           

            fillNode.x = spawnNode.x;
            fillNode.y = spawnNode.y;

            m_fillNodes.Add(fillNode);
            gameManager.AddFillNode(fillNode);

           


        }
    }

    public bool ChangeAbility(ABILITY _targetAbility)
    {
        if (CurrentAbility != ABILITY.WALKER && _targetAbility != ABILITY.WALKER)
            return false;
        if (_targetAbility == CurrentAbility)
            return false;

        m_hasUmbrella = false;
        m_readyToDigForward = false;

        switch (_targetAbility)
        {
            case ABILITY.WALKER:
                m_anim.Play("Walk1");
                CurrentAbility = _targetAbility;
                m_currentLerpSpeed = m_onGround ? MaxLerpSpeed * WalkSpeedPrc : MaxLerpSpeed * FallSpeedPrc;
              
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

                m_anim.Play("Stop1");
                CurrentAbility = _targetAbility;
                m_currentStoppingTime = 0f;
                StopNodes();
                return true;

            case ABILITY.DIG_DOWN:
                if (!m_prevOnGround)
                    return false;
                m_anim.Play("Dig down");
                CurrentAbility = _targetAbility;
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
                CurrentAbility = _targetAbility;
                m_currentBuildTime = 0f;
                return true;

            case ABILITY.FILL:
                if (!m_prevOnGround)
                    return false;
                m_anim.Play("Fill");
                CurrentAbility = _targetAbility;
                m_currentBuildTime = 0f;
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
                if(Vector3.Distance(gameManager.GetWorldPositionFromNode(_centerNode), gameManager.GetWorldPositionFromNode(node)) > _radius * gameManager.UnitPerPixel)
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
                Node node = gameManager.GetNode(m_currentNode.x + x, m_currentNode.y + y);
                if (Vector3.Distance(gameManager.GetWorldPositionFromNode(m_currentNode), gameManager.GetWorldPositionFromNode(node)) > _radius * gameManager.UnitPerPixel)
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
            bool exited = gameManager.CheckIfInExitRange(m_currentNode);
            if (exited)
            {
                gameObject.SetActive(false);
                gameManager.OnLemmingExit();
                m_hasExited = true;
            }
            
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

        if (m_currentStoppingTime >= MaxStoppingTime || !CheckIfHasGround(2))
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

        if (!CheckIfHasGround(6))
        {
            ChangeAbility(ABILITY.WALKER);
            Debug.Log("no ground");
            return;
        }


        if (!m_lerp)
        {
            gameManager.AddNodesToClear(FindNodes(m_currentNode, DigDownRadius));

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

    private void DigForward(float _delta)
    {
        m_currentDigForwardTime += Time.deltaTime;

        if (m_currentDigForwardTime >= MaxDigForwardTime)
        {
            ChangeAbility(ABILITY.WALKER);
            return;
        }

        if (!CheckIfHasGround(1))
        {
            ChangeAbility(ABILITY.WALKER);
            return;
        }


        if (!m_lerp)
        {
            Node centerNode = gameManager.GetNode(m_currentNode.x, m_currentNode.y + DigForwardRadius - 1);

            gameManager.AddNodesToClear(FindNodes(centerNode, DigForwardRadius));

            int x = (m_movingLeft) ? -1 : 1; 

            Node nextSide = gameManager.GetNode(m_currentNode.x + x, m_currentNode.y);
            if (nextSide.isEmpty)
                m_targetNode = nextSide;
            else
                m_targetNode = m_currentNode;

            OnLerpEnd();
            float distance = Vector3.Distance(m_startingPosition, m_targetPosition);

            m_currentLerpSpeed = MaxLerpSpeed * DigForwardSpeedPrc;

            m_baseSpeed = m_currentLerpSpeed / distance;

        }
        else
        {
            LerpToPosition(_delta);
        }
    }

    private void Build(float _delta)
    {
        m_currentBuildTime += Time.deltaTime;

        if (m_currentBuildTime >= MaxBuildTime)
        {
            ChangeAbility(ABILITY.WALKER);
            return;
        }

        if (!m_lerp)
        {
            int x = (m_movingLeft) ? -1 : 1;

            gameManager.AddNodesToBridge(FindNodes(-x , BuildLength * x, 0, 1, 10), m_movingLeft);

            Node nextStep = gameManager.GetNode(m_currentNode.x + x, m_currentNode.y + 1);
            if (nextStep.isEmpty)
                m_targetNode = nextStep;
            else
            {
                ChangeAbility(ABILITY.WALKER);
                return;
            }
                

            OnLerpEnd();
            float distance = Vector3.Distance(m_startingPosition, m_targetPosition);

            m_currentLerpSpeed = MaxLerpSpeed * BuildSpeedPrc;

            m_baseSpeed = m_currentLerpSpeed / distance;

        }
        else
        {
            LerpToPosition(_delta);
        }
    }

    private bool CheckIfHasGround(int _height)
    {
        for (int i = 1; i <= _height; i++)
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
            m_stoppedNodesList[i].isStoppedLeft = false;
            m_stoppedNodesList[i].isStoppedRight = false;
        }
        m_stoppedNodesList.Clear();
    }

    private void StopNodes()
    {
        for (int x = 0; x < 1; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                Node node = gameManager.GetNode(m_currentNode.x + x, m_currentNode.y + y);
                node.isStoppedLeft = true;
                node.isStoppedRight = true;
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
                        m_currentLerpSpeed = MaxLerpSpeed * UmbrellFallSpeedSpeedPrc;
                    }

                    else
                    {
                        m_anim.Play("Fall1");
                        m_currentLerpSpeed = MaxLerpSpeed * FallSpeedPrc;
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
                m_currentLerpSpeed = MaxLerpSpeed * WalkSpeedPrc;
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
                            CurrentAbility = ABILITY.DIG_FORWARD;
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
                    Debug.Log("step " + stepHeight);
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
        
        m_targetNode = gameManager.GetNode(m_targetX, m_targetY);
    }

    private bool CheckIfEmpty(int _x, int _y)
    {
        Node node = gameManager.GetNode(_x, _y);
        if (node == null)
            return true;
        
        if (node.isEmpty)
        {
            if (node.isStoppedLeft == true && m_movingLeft == true)
            {
                Debug.Log("stopped l");
                return false;
            }
                
            if (node.isStoppedRight == true && m_movingLeft == false)
            {
                Debug.Log("stopped r");
                return false;
            }
            return true;
            
               
        }                  
        return false;
    }
}
