using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour {

    public static UIManager Instance;

    public Sprite EmptyCursorSprite;
    public Sprite ActiveCursorSprite;

    public SpriteRenderer CursorRenderer;
    public Transform CursorTransform;

    private LemmingsManager m_lemmingsManager;
    private GameManager m_gameManager;

    public bool IsOverUnit;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_gameManager = GameManager.Instance;
        m_lemmingsManager = LemmingsManager.Instance;
        Cursor.visible = false;
    }

    public void Tick()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
        CursorTransform.position = m_gameManager.MousePosition;
        if (IsOverUnit)
        {
            CursorRenderer.sprite = ActiveCursorSprite;
        }
        else
        {
            CursorRenderer.sprite = EmptyCursorSprite;
        }
    }

    internal void ChangeAbility(bool _canChange)
    {
        StartCoroutine(ChangeCursorColorCO(_canChange));
    }

    private IEnumerator ChangeCursorColorCO(bool _canChange)
    {
        Color color = (_canChange) ? Color.green : Color.red;     
        CursorRenderer.color = color;
        Debug.Log(color);
        while(CursorRenderer.color != Color.white)
        {
            CursorRenderer.color = Color.Lerp(CursorRenderer.color, Color.white, Time.deltaTime);
            yield return null;
            if(CursorRenderer.color.b >= .9f)
            {
                CursorRenderer.color = Color.white;
            }        
        }
    }
}
