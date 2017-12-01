using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour {

    private LevelEditor m_levelEditor;

    private GameManager m_gameManager;

    public ABILITY ability;

    public Image m_Image;

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
            m_gameManager.Play(m_levelEditor.DefaultTexture);
        }
        else
        {
            m_gameManager.Play(m_levelEditor.Leveltexture);
        }
    }

    public void ChangeColor()
    {
   
        
        LevelEditor.Instance.ChangeColor(m_Image.color);
        
    }

    public void SetSpawn()
    {
        LevelEditor.Instance.StartSettingSpawn();
    }

    public void SetExit()
    {
        LevelEditor.Instance.StartSettingExit();
    }

    public void OpenLoadPopup(bool _state)
    {
        LevelEditor.Instance.OpenLoadPopup(_state);
    }

    public void LoadFromWeb()
    {
        LevelEditor.Instance.LoadFromWeb();
    }



    
}
