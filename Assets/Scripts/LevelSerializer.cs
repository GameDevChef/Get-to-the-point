using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

[System.Serializable]
public class LevelSave
{
    public byte[] levelTextureBytes;
    public string levelName;
    public float SpawnPosX;
    public float SpawnPosY;
    public float ExitPosX;
    public float ExitPosY;

    public LevelSave(byte[] _textureBytes, string _name,
        float _spawnPosX, float _spawnPosY, float _exitPosX, float _exitPosY)
    {
        levelTextureBytes = _textureBytes;
        levelName = _name;
        SpawnPosX = _spawnPosX;
        SpawnPosY = _spawnPosY;
        ExitPosX = _exitPosX;
        ExitPosY = _exitPosY;

    }
}

[System.Serializable]
public class PlayerData
{
    public List<LevelSave> levels;
}

public class LevelSerializer : MonoBehaviour {

    public static LevelSerializer Instance;

    [Header("References")]

    [SerializeField]
    GameObject m_savePopup;

    [SerializeField]
    InputField m_saveInputField;
    
    [SerializeField]
    Texture2D m_defaultLevelTexture;

    [SerializeField]
    Transform m_tableParent;

    [SerializeField]
    LevelTemplate m_levelTemplatePrefab;

    LevelTemplate m_currentLeveltemplate;

    LevelSave m_selectedLevelSave;

    List<LevelSave> m_levels;

    int m_selectedIndex;


    void Awake()
    {
        Instance = this;
    }

    public void OpenSavePopup(bool _state)
    {
        m_savePopup.SetActive(_state);
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(m_saveInputField.text))
            return;
        GameManager.Instance.ChangeGameState(GAME_STATE.INIT);

        LevelSave save = new LevelSave(
            LevelEditor.Instance.m_LevelTexture.EncodeToPNG(),
            m_saveInputField.text,
            GameManager.Instance.m_SpawnVector.x,
            GameManager.Instance.m_SpawnVector.y,
            GameManager.Instance.m_ExitVector.x,
            GameManager.Instance.m_ExitVector.y);

        m_levels.Add(save);
        SavePlayerData();
        InitializeTable();
    }

    public void Load()
    {
        if (m_selectedLevelSave == null)
            return;
        LevelEditor.Instance.LoadLevel(m_selectedLevelSave);
    }

    void OnEnable()
    {
        LoadPlayerData();
        InitializeTable();
    }

    void InitializeTable()
    {
        if(m_tableParent.childCount > 0)
        {
            for (int i = 0; i < m_tableParent.childCount; i++)
            {
                m_tableParent.GetChild(i).GetComponent<LevelTemplate>().Init(m_levels[i].levelName, i);
            }
        }
        if (m_tableParent.childCount >= m_levels.Count)
            return;
        for (int i = m_tableParent.childCount; i < m_levels.Count; i++)
        {
            LevelTemplate template = Instantiate(m_levelTemplatePrefab, m_tableParent);
            template.Init(m_levels[i].levelName, i);
        }
        m_tableParent.GetChild(0).GetComponent<LevelTemplate>().Select();        
    }

    public void LoadPlayerData()
    {
        if (File.Exists(Application.persistentDataPath + "/playerData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath
                + "/playerData.dat", FileMode.Open);
            PlayerData playerData = (PlayerData)bf.Deserialize(file);
            m_levels = playerData.levels;
        }

        else
        {
            CreatePlayerData();
        }
    }

    void CreatePlayerData()
    {     
        m_levels = new List<LevelSave>();
        LevelSave DefaultLevel = new LevelSave(m_defaultLevelTexture.EncodeToPNG(), "Default",
            100, 100, 800, 100);
        m_levels.Add(DefaultLevel);
        SavePlayerData();      
    }

    public void SavePlayerData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath
            + "/playerData.dat", FileMode.Create);
        PlayerData playerData = new PlayerData();
        playerData.levels = m_levels;
        bf.Serialize(file, playerData);
        file.Close();
    }

    public void SetSelectedLevel(int _index)
    {
        if(m_currentLeveltemplate != null)
        {
            m_currentLeveltemplate.Deselect();
        }
        m_currentLeveltemplate = m_tableParent.GetChild(_index).GetComponent<LevelTemplate>();
        m_selectedLevelSave = m_levels[_index];       
    }

    public void NewGame()
    {
        Caching.ClearCache();
        File.Delete(Application.persistentDataPath + "/playerData.dat");
        PlayerPrefs.DeleteAll();
    }
}
