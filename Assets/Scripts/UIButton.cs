using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BUTTON_TYPE
{
    ABILITY,
    BRUSH_SIZE,
    BRUSH_COLOR
}

public class UIButton : MonoBehaviour {

    LevelEditor m_levelEditor;

    GameManager m_gameManager;

    [Header("Variables")]

    public BUTTON_TYPE m_ButtonType;

    public ABILITY ability;

    public Image m_Image;

    [SerializeField]
    int BrushSize;

    void Start()
    {
        m_levelEditor = LevelEditor.Instance;
        m_gameManager = GameManager.Instance;
    }

    public void Press()
    {
        m_gameManager.ButtonPressed(this);
    }

    public void New()
    {
        m_levelEditor.NewLevel();
    }

    public void Edit()
    {
        m_levelEditor.NewLevel(m_levelEditor.m_LevelTexture);
    }

    public void Play()
    {
        if(m_levelEditor.m_LevelTexture == null)
        {
            LevelSerializer.Instance.Load();
        }
        {
            m_gameManager.Play(m_levelEditor.m_LevelTexture);
        }
    }

    public void ChangeColor()
    {
        GameManager.Instance.ButtonPressed(this);
        LevelEditor.Instance.ChangeColor(GetComponent<Image>().color);       
    }

    public void ChangeBrushSize()
    {
        GameManager.Instance.ButtonPressed(this);
        LevelEditor.Instance.ChangeBrushSize(BrushSize);
    }

    public void SetSpawn()
    {       
        m_levelEditor.StartSettingSpawn();
    }

    public void SetExit()
    {
        m_levelEditor.StartSettingExit();
    }

    public void BackToInit()
    {
        GameManager.Instance.BackToInit();
    }

    public void OpenLoadPopup(bool _state)
    {
        m_levelEditor.OpenLoadPopup(_state);
    }

    public void OpenSavePopup(bool _state)
    {
        LevelSerializer.Instance.OpenSavePopup(_state);
    }

    public void LoadFromWeb()
    {
        m_levelEditor.LoadFromWeb();
    }   
}
