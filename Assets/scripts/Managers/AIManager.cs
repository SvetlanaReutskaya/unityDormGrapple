using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private IBot bot;

    void Start()
    {
        var go = GameObject.Find("LevelManager");

        var managerScript = go.GetComponent<LevelManager>();

        var level = managerScript.GetLevel();

        if (level == 1 || level == 2)
        {
            bot = new SimpleBot();
        }
        else
        {
            bot = new AdvancedBot();
        }
    }

    public System.Tuple<Position, Position> Move(List<List<ICell>> cells)
    {
        return bot.Move(cells);
    }
}
