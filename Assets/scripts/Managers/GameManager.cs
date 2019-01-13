using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject PlayerHealth;
    public GameObject EnemyHealth;
    private GameState gameState = GameState.None;

    private bool isPlayerTurn = true;
    private bool isGameActive = true;

    public GameObject GameoverButton;
    public GameObject GameoverLabel;
    public GameObject VictoryLabel;

    public GameObject GameField;
    private GameField gf;

    private void Start()
    {
        gf = GameField.GetComponent<GameField>();
    }

    private void Update()
    {
        gameState = GetGameState();
        switch (gameState)
        {
            case (GameState.Ended):
                return;
            case (GameState.TileMoving):
                return;
            case GameState.HasEmptyTiles:
                {
                    gf.Collapse();
                    gf.FillField();
                    break;
                }
            case GameState.TileSelected:
                {
                    if (isPlayerTurn)
                    {
                        if (!Input.GetMouseButtonDown(0)) return;
                        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                        if (hit.collider != null)
                        {
                            isPlayerTurn =  gf.IsCollidedWithNeighbour(hit);
                        }
                    }
                    break;
                }
            case GameState.MovedTile:
                {
                    break;
                }
            case (GameState.None):
                {
                    break;
                }
        }

        isPlayerTurn = gf.UpdateStatus(gameState, isPlayerTurn);
    }

    private GameState GetGameState()
    {
        if (isGameActive)
        {
            var playerHP = PlayerHealth.GetComponent<HP>();

            if (playerHP.GetHealth() < 1)
            {
                GameOver();
                isGameActive = false;
                return GameState.Ended;
            }
            else
            {
                var enemyHP = EnemyHealth.GetComponent<HP>();

                if (enemyHP.GetHealth() < 1)
                {
                    Victory();
                    isGameActive = false;
                    return GameState.Ended;
                }
            }

            var allTiles = Enumerable.Range(0, gf.GetHeight())
                .SelectMany(row => gf.GetTilesRow(row)).ToList();
            if (allTiles.Where(tile => tile != null).Any(tile => tile.IsMoving))
                return GameState.TileMoving;
            if (allTiles.Any(tile => tile == null))
                return GameState.HasEmptyTiles;
            if (gf.swappedTile != null)
                return GameState.MovedTile;
            if (gf.selectedTile != null)
                return GameState.TileSelected;
            return GameState.None;
        }
        else
            return GameState.Ended;
    }

    public void DealDamage(Tile tile)
    {
        if (tile.Owner == TileOwner.Player)
        {
            var hp = EnemyHealth.GetComponent<HP>();

            if (!isPlayerTurn)
                hp.Minus(tile.Damage);
            else
                hp.Minus(tile.Damage / 2);
        }
        else
        {
            var hp = PlayerHealth.GetComponent<HP>();

            if (isPlayerTurn)
                hp.Minus(tile.Damage / 2);
            else
                hp.Minus(tile.Damage);
        }
    }

    private void GameOver()
    {
        GameoverLabel.SetActive(true);
        GameoverButton.SetActive(true);
    }

    private void Victory()
    {
        VictoryLabel.SetActive(true);
        GameoverButton.SetActive(true);
    }
}
