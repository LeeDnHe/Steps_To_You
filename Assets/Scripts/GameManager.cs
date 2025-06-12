using System.Collections;
using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameManager main;
    public string path = "playerStats.dat"; //임시 경로
    public Stat gameStat;
    
    void Awake() { //non-lazy, DDOL
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }
        
        main = this; 
        DontDestroyOnLoad(gameObject);

        LoadStat();
    }

    public void LoadStat()
    {
        gameStat = SaveIO.readStat(path);
    }

    public void SaveStat()
    {
        SaveIO.writeStat(gameStat, path);
    }
}
