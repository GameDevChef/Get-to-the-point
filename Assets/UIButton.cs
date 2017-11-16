using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour {

    public ABILITY ability;

    public Image image;

    public void Press()
    {
        GameManager.Instance.ButtonPressed(this);
    }
}
