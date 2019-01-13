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

    public void Init()
    {
        var level = this.GetLevel();

        var player = GameObject.Find("PlayerHealth");
        var playerHP = player.GetComponent<HP>();

        var enemy = GameObject.Find("EnemyHealth");
        var enemyHP = enemy.GetComponent<HP>();

        switch (level)
        {
            case 1:
                playerHP.Init(300);
                enemyHP.Init(300);
                GameObject.Find("StudentEnemy").SetActive(false);
                GameObject.Find("CleanerEnemy").SetActive(false);
                break;
            case 2:
                playerHP.Init(350);
                enemyHP.Init(300);
                GameObject.Find("GirlEnemy").SetActive(false);
                GameObject.Find("CleanerEnemy").SetActive(false);
                break;
            case 3:
                playerHP.Init(200);
                enemyHP.Init(300);
                GameObject.Find("GirlEnemy").SetActive(false);
                GameObject.Find("StudentEnemy").SetActive(false);
                break;
        }
    }
}
