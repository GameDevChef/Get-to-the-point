using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour {

    public static UIManager Instance;

    [Header("References")]

    public Sprite m_EmptyCursorSprite;

    [SerializeField]
    Sprite m_activeCursorSprite;

    [SerializeField]
    SpriteRenderer m_cursoRenderer;
    
    [SerializeField]
    Transform m_cursorTransform;

    LemmingsManager m_lemmingsManager;

    GameManager m_gameManager;

    Sprite m_defaultSprite;

    [Header("Variables")]

    [HideInInspector]
    public bool IsOverUnit;

    [HideInInspector]
    public bool IsOverUI;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        m_gameManager = GameManager.Instance;
        m_lemmingsManager = LemmingsManager.Instance;
        Cursor.visible = false;
        m_defaultSprite = m_EmptyCursorSprite;
    }

    public void Tick()
    {
        IsOverUI = EventSystem.current.IsPointerOverGameObject();
        if (IsOverUI)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
        m_cursorTransform.position = m_gameManager.m_MousePosition;
        if (IsOverUnit)
        {
            m_cursoRenderer.sprite = m_activeCursorSprite;
        }
        else
        {
            m_cursoRenderer.sprite = m_defaultSprite;
        }
    }

    public void ChangeAbility(bool _canChange)
    {
        StartCoroutine(ChangeCursorColorCO(_canChange));
    }

    IEnumerator ChangeCursorColorCO(bool _canChange)
    {
        Color color = (_canChange) ? Color.green : Color.red;     
        m_cursoRenderer.color = color;

        while(m_cursoRenderer.color != Color.white)
        {
            m_cursoRenderer.color = Color.Lerp(m_cursoRenderer.color, Color.white, Time.deltaTime);
            yield return null;
            if(m_cursoRenderer.color.b >= .9f)
            {
                m_cursoRenderer.color = Color.white;
            }        
        }
    }

    public void ChangeCursorSprite(Sprite _sprite)
    {
        m_defaultSprite = _sprite;
    }
}
