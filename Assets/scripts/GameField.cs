﻿using DormGrapple;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public enum GameState
{
    None,
    TileMoving,
    TileSelected,
    HasEmptyTiles,
    MovedTile,
    Ended
}

public class GameField : MonoBehaviour
{
    private bool isPlayerTurn = true;
    private bool isGameActive = true;
    private const int Width = 8;
    private const int Height = 8;
    private const int size = 8;
    private static float textureWidth;
    private static float textureHeight;
    private readonly Transform[,] backgroundTiles = new Transform[Width, Height];
    private readonly TileArray tiles = new TileArray(Width, Height);
    public GameObject BackgroundPrefab;
    public GameObject GameoverButton;
    public GameObject GameoverLabel;
    public GameObject VictoryLabel;
    private GameState gameState = GameState.None;
    private Point selectedTile;
    private Point swappedTile;
    public GameObject[] TilePrefabs;
    int level;
    Field field;
    public GameObject AIManager;
    private AIManager AIManagerScript;

    private void Start()
    {
        var spriteRenderer = BackgroundPrefab.GetComponent<SpriteRenderer>();
        var texture = spriteRenderer.sprite.texture;
        // Adjust to Unity units
        textureWidth = texture.width / 100f;
        textureHeight = texture.height / 100f;
        SpawnBackgroundTiles();
        GenerateMap();
        SetLevel();
    }

    private void GenerateMap()
    {
        Field field = new Field();
        field.SetField();

        for(int y = 0; y < Height; y++)
        {
            for(int x = 0; x < Width; x++)
            {
                var cell = field.cells[x][y];

                switch((int)cell.Type)
                {
                    case 0:
                        SpawnConcreteTile(x, "apple");
                        break;
                    case 1:
                        SpawnConcreteTile(x, "cpu");
                        break;
                    case 2:
                        SpawnConcreteTile(x, "bacteria");
                        break;
                    case 3:
                        SpawnConcreteTile(x, "slippers");
                        break;
                    case 4:
                        SpawnConcreteTile(x, "web");
                        break;
                    case 5:
                        SpawnConcreteTile(x, "poison");
                        break;
                }
            }
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

    private void SpawnBackgroundTiles()
    {
        for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                var position = new Vector3(x * textureWidth, y * textureHeight);
                var backgroundTile = (GameObject)Instantiate(BackgroundPrefab, position, Quaternion.identity);
                backgroundTile.transform.parent = transform;
                backgroundTiles[x, y] = backgroundTile.transform;
            }
    }

    private void FillField()
    {
        for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                if (tiles[new Point(x, y)] == null)
                    SpawnTile(x);
            }
    }

    private void SpawnTile(int column)
    {
        var position = new Vector3(column * textureWidth, Height * textureHeight);
        for (var y = 0; y < Height; y++)
        {
            var currentTile = new Point(column, y);
            if (tiles[currentTile] != null) continue;

            var tilePrefab = TilePrefabs[Random.Range(0, TilePrefabs.Length)];
            var tile = (GameObject)Instantiate(tilePrefab, position, Quaternion.identity);
            var tileComponent = tile.GetComponent<Tile>();
            tiles[currentTile] = tileComponent;
            tileComponent.MoveToPoint(backgroundTiles[column, y].position);
            break;
        }
    }

    private void SpawnConcreteTile(int column, string name)
    {
        var position = new Vector3(column * textureWidth, Height * textureHeight);
        for (var y = 0; y < Height; y++)
        {
            var currentTile = new Point(column, y);
            if (tiles[currentTile] != null) continue;

            var tilePrefab = TilePrefabs.Where(x=>x.name == name).FirstOrDefault();
            var tile = (GameObject)Instantiate(tilePrefab, position, Quaternion.identity);
            var tileComponent = tile.GetComponent<Tile>();
            tiles[currentTile] = tileComponent;
            tileComponent.MoveToPoint(backgroundTiles[column, y].position);
            break;
        }
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
                    Collapse();
                    FillField();
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
                            var position = GetPoint(hit.collider.transform);
                            var neighbours = GetNeighbours(selectedTile);
                            if (neighbours.Any(pos => pos.X == position.X && pos.Y == position.Y))
                            {
                                tiles[selectedTile].StopSpinning();
                                swappedTile = position;
                                SwapTiles(selectedTile, swappedTile);
                                if (tiles.FindMatches().ToList().Any())
                                    isPlayerTurn = false;
                            }
                            else
                            {
                                tiles[selectedTile].StopSpinning();
                                selectedTile = null;
                            }
                        }
                    }
                    break;
                }
            case GameState.MovedTile:
                {
                    var matches = tiles.FindMatches().ToList();
                    if (matches.Any())
                    {
                        foreach (var tile in matches)
                        {
                            DealDamage(tile);
                            tile.Remove();
                        }
                    }
                    else
                    {
                        SwapTiles(selectedTile, swappedTile);
                    }
                    selectedTile = null;
                    swappedTile = null;
                    break;
                }
            case (GameState.None):
                {
                    var matches = tiles.FindMatches().ToList();
                    if (matches.Any())
                    {
                        foreach (var tile in matches)
                        {
                            DealDamage(tile);
                            tile.Remove();
                        }
                    }
                    else if (!isPlayerTurn)
                    {
                        var cells = ConvertNewMapToOldMap();

                        var points = AIManagerScript.Move(cells);

                        SwapTiles(new Point(points.Item1.Column, 7 - points.Item1.Row),
                            new Point(points.Item2.Column, 7 - points.Item2.Row));

                        isPlayerTurn = true;
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                        if (hit.collider != null)
                        {
                            var position = GetPoint(hit.collider.transform);
                            selectedTile = position;
                            tiles[selectedTile].StartSpinning();
                        }
                    }
                    break;
                }
        }
    }

    private List<List<ICell>> ConvertNewMapToOldMap()
    {
        for (int i = 7; i >= 0; i--)
        {
            int j = 0;
            foreach (var tile in tiles.GetRow(i))
            {
                switch (tile.name)
                {
                    case "apple(Clone)":
                        field.cells[7 - i][j] = new Apple();
                        break;
                    case "cpu(Clone)":
                        field.cells[7 - i][j] = new Chip();
                        break;
                    case "bacteria(Clone)":
                        field.cells[7 - i][j] = new Bacterium();
                        break;
                    case "slippers(Clone)":
                        field.cells[7 - i][j] = new Slipper();
                        break;
                    case "web(Clone)":
                        field.cells[7 - i][j] = new CockroachTrap();
                        break;
                    case "poison(Clone)":
                        field.cells[7 - i][j] = new Poison();
                        break;
                }

                j++;
            }
        }

        return field.cells;
    }

    private void DealDamage(Tile tile)
    {
        if (tile.name == "apple(Clone)" || tile.name == "cpu(Clone)" || tile.name == "bacteria(Clone)")
        {
            var go = GameObject.Find("EnemyHealth");

            var hp = go.GetComponent<HP>();

            if (tile.name == "apple(Clone)")
            {
                if (!isPlayerTurn)
                    hp.Minus(5);
                else
                    hp.Minus(5 / 2);
            }
            else if (tile.name == "cpu(Clone)")
            {
                if (!isPlayerTurn)
                    hp.Minus(6);
                else
                    hp.Minus(6 / 2);
            }
            else if (tile.name == "bacteria(Clone)")
            {
                if(!isPlayerTurn)
                    hp.Minus(7);
                else
                    hp.Minus(7 / 2);
            }
        }
        else
        {
            var go = GameObject.Find("PlayerHealth");

            var hp = go.GetComponent<HP>();
            if (tile.name == "slippers(Clone)")
            {
                if(isPlayerTurn)
                    hp.Minus(5 / 2);
                else
                    hp.Minus(5);
            }
            else if (tile.name == "web(Clone)")
            {
                if(isPlayerTurn)
                    hp.Minus(6 / 2);
                else
                    hp.Minus(6);
            }
            else if (tile.name == "poison(Clone)")
            {
                if(isPlayerTurn)
                    hp.Minus(7 / 2);
                else
                    hp.Minus(7);
            }
        }
    }

    private IEnumerable<Point> GetNeighbours(Point target)
    {
        var offsets = new[] { -1, 1 };
        return offsets.SelectMany(offset => new[] { new Point(offset, 0), new Point(0, offset) })
            .Select(point => point.Sum(target));
    }

    private void SwapTiles(Point first, Point second)
    {
        var firstTile = tiles[first];
        firstTile.MoveToPoint(backgroundTiles[second.X, second.Y].position);
        tiles[second].MoveToPoint(backgroundTiles[first.X, first.Y].position);
        tiles[first] = tiles[second];
        tiles[second] = firstTile;
    }

    private Point GetPoint(Transform position)
    {
        for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
                if (backgroundTiles[x, y] == position)
                    return new Point(x, y);
        return new Point(-1, -1);
    }

    private void Collapse()
    {
        for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                var currentTile = new Point(x, y);
                if (tiles[currentTile] != null) continue;
                var onlyEmptyLeft = true;
                for (var i = y + 1; i < Height; i++)
                {
                    var nextTile = new Point(x, i);
                    if (tiles[nextTile] == null) continue;
                    tiles[currentTile] = tiles[nextTile];
                    tiles[nextTile].MoveToPoint(backgroundTiles[x, y].position);
                    tiles[nextTile] = null;
                    onlyEmptyLeft = false;
                    break;
                }
                if (onlyEmptyLeft)
                    break;
            }
    }

    private GameState GetGameState()
    {
        if (isGameActive)
        {
            var player = GameObject.Find("PlayerHealth");
            var playerHP = player.GetComponent<HP>();

            if (playerHP.GetHealth() < 1)
            {
                GameOver();
                isGameActive = false;
                return GameState.Ended;
            }
            else
            {
                var enemy = GameObject.Find("EnemyHealth");
                var enemyHP = enemy.GetComponent<HP>();

                if (enemyHP.GetHealth() < 1)
                {
                    Victory();
                    isGameActive = false;
                    return GameState.Ended;
                }
            }

            var allTiles = Enumerable.Range(0, Height)
                .SelectMany(row => tiles.GetRow(row)).ToList();
            if (allTiles.Where(tile => tile != null).Any(tile => tile.IsMoving))
                return GameState.TileMoving;
            if (allTiles.Any(tile => tile == null))
                return GameState.HasEmptyTiles;
            if (swappedTile != null)
                return GameState.MovedTile;
            if (selectedTile != null)
                return GameState.TileSelected;
            return GameState.None;
        }
        else
            return GameState.Ended;
    }

    private void SetLevel()
    {
        AIManagerScript = AIManager.GetComponent<AIManager>();

        field = new Field();

        var go = GameObject.Find("LevelManager");

        var manager = go.GetComponent<LevelManager>();

        level = manager.GetLevel();

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