using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTemplate : MonoBehaviour {

    public Image m_Image;
    public Text m_Text;
    public int index;

    public void Init(string _name, int _index)
    {
        m_Text.text = _name;
        index = _index;
    }


    public void Select()
    {
        
        LevelSerializer.Instance.SetSelectedLevel(index);
        m_Image.color = Color.green;

    }

    public void Deselect()
    {
        m_Image.color = new Color(0f, 0f, 0f, 0f);
    }


}
