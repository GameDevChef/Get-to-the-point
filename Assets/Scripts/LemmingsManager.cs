using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LemmingsManager : MonoBehaviour {

    public static LemmingsManager Instance;

    [Header("References")]
    [Space(10)]

    [SerializeField]
    public Lemming m_lemingPrefab;

    GameManager gameManager;

    List<Lemming> m_allLemmingsList = new List<Lemming>();

    [Header("Variables")]
    [Space(10)]

    [SerializeField]
    int m_maxLemingAmount;

    [SerializeField]
    float m_spawnInterval;

    int m_currentLemmingsAmount;

    float m_timer;

    void Awake()
    {
        Instance = this;      
    }

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void Tick(float _delta)
    {     
        m_timer += Time.deltaTime;
        if (m_timer > m_spawnInterval)
        {
            if (m_currentLemmingsAmount < m_maxLemingAmount)
            {
                SpawnLemming();
            }
        }
        for (int i = 0; i < m_allLemmingsList.Count; i++)
        {
            m_allLemmingsList[i].Tick(_delta);
        } 
    }

    public Lemming GetClosestUnit()
    {
        if (m_allLemmingsList.Count == 0)
            return null;

        Lemming closestLemmingInRadius = null;
        float closestDistanceSqr = .1f *.1f; 

        for (int i = 0; i < m_allLemmingsList.Count; i++)
        {           
            float distanceSqr = Vector3.SqrMagnitude(gameManager.m_MousePosition - m_allLemmingsList[i].transform.position);
            
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestLemmingInRadius = m_allLemmingsList[i];
            }
            
        }
        return closestLemmingInRadius;
    }

    void SpawnLemming()
    {
        Lemming lemmingInstance = Instantiate(m_lemingPrefab, gameManager.m_SpawnPosition, Quaternion.identity);
        lemmingInstance.Initialize();
        m_allLemmingsList.Add(lemmingInstance);
        m_currentLemmingsAmount++;
        m_timer = 0f;
    }

    public void RemoveLemming(Lemming _lemming)
    {
        m_allLemmingsList.Remove(_lemming);
    }
}
