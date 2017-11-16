using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LemmingsManager : MonoBehaviour {

    public static LemmingsManager Instance;

    public int MaxLemmingsCount;
    int m_currentLemmingsAmount;

    public float SpawnInterval;
    public Lemming LemmingPrefab;
    float m_timer;

    private GameManager gameManager;

    private List<Lemming> m_allLemmingsList = new List<Lemming>();

 

    private void Awake()
    {
        Instance = this;      
    }


    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void Update()
    {
        if (m_currentLemmingsAmount >= MaxLemmingsCount)
            return;

        m_timer += Time.deltaTime;
        if(m_timer > SpawnInterval)
        {
            SpawnLemming();
        }
    }

    public void Tick(float _delta)
    {
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
            
            float distanceSqr = Vector3.SqrMagnitude(gameManager.MousePosition - m_allLemmingsList[i].transform.position);
            
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestLemmingInRadius = m_allLemmingsList[i];
            }
            
        }
        return closestLemmingInRadius;
    }

    private void SpawnLemming()
    {
        Lemming lemmingInstance = Instantiate(LemmingPrefab, gameManager.SpawnPosition, Quaternion.identity);
        lemmingInstance.Initialize();
        m_allLemmingsList.Add(lemmingInstance);
        m_currentLemmingsAmount++;
        m_timer = 0f;
    }

    internal void RemoveLemming(Lemming _lemming)
    {
        m_allLemmingsList.Remove(_lemming);
    }
}
