using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    int levelNum = 0;

	void Start () {
        DontDestroyOnLoad(this);
    }

    public void SetLevel(int number)
    {
        levelNum = number;
    }

    public int GetLevel()
    {
        return levelNum;
    }
}
