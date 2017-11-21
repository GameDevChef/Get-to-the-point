using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour {

    private LevelEditor m_levelEditor;

    private GameManager m_gameManager;

    public ABILITY ability;

    public Image image;

    private void Start()
    {
        m_gameManager = GameManager.Instance;
        m_levelEditor = LevelEditor.Instance;
    }

    public void Press()
    {
        GameManager.Instance.ButtonPressed(this);
    }

    public void New()
    {
        m_levelEditor.NewLevel();
    }

    public void Play()
    {
        if(m_levelEditor.Leveltexture == null)
        {
            Debug.Log("no level selected");
        }
        else
        {
            m_gameManager.Play(m_levelEditor.Leveltexture);
        }
    }

    
}
