using DormGrapple;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class GameField : MonoBehaviour
{
    private const int size = 8;
    private static float textureWidth;
    private static float textureHeight;
    private readonly Transform[,] backgroundTiles = new Transform[size, size];
    private readonly TileArray tiles = new TileArray(size, size);
    public GameObject BackgroundPrefab;
    public Point selectedTile { get; set; }
    public Point swappedTile { get; set; }
    public GameObject[] TilePrefabs;
    int level;
    Field field;
    public GameObject AIManager;
    private AIManager AIManagerScript;

    public GameObject GameManager;
    private GameManager gmScript;

    private LevelManager levelManager;

    private void Start()
    {
        gmScript = GameManager.GetComponent<GameManager>();

        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

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

        for(int y = 0; y < size; y++)
        {
            for(int x = 0; x < size; x++)
            {
                var cell = field.cells[x][y];

                switch((int)cell.Type)
                {
                    case 0:
                        SpawnConcreteTile(x, TileType.Apple);
                        break;
                    case 1:
                        SpawnConcreteTile(x, TileType.Cpu);
                        break;
                    case 2:
                        SpawnConcreteTile(x, TileType.Bacteria);
                        break;
                    case 3:
                        SpawnConcreteTile(x, TileType.Slippers);
                        break;
                    case 4:
                        SpawnConcreteTile(x, TileType.Web);
                        break;
                    case 5:
                        SpawnConcreteTile(x, TileType.Poison);
                        break;
                }
            }
        }
    }

    public int GetHeight()
    {
        return size;
    }

    public int GetWidth()
    {
        return size;
    }

    public IEnumerable<Tile> GetTilesRow(int index)
    {
        return tiles.GetRow(index);
    }

    private void SpawnBackgroundTiles()
    {
        for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
            {
                var position = new Vector3(x * textureWidth, y * textureHeight);
                var backgroundTile = (GameObject)Instantiate(BackgroundPrefab, position, Quaternion.identity);
                backgroundTile.transform.parent = transform;
                backgroundTiles[x, y] = backgroundTile.transform;
            }
    }

    public void FillField()
    {
        for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
            {
                if (tiles[new Point(x, y)] == null)
                    SpawnTile(x);
            }
    }

    private void SpawnTile(int column)
    {
        var position = new Vector3(column * textureWidth, size * textureHeight);
        for (var y = 0; y < size; y++)
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

    private void SpawnConcreteTile(int column, TileType type)
    {
        var position = new Vector3(column * textureWidth, size * textureHeight);
        for (var y = 0; y < size; y++)
        {
            var currentTile = new Point(column, y);
            if (tiles[currentTile] != null) continue;

            var tilePrefab = TilePrefabs.Where(x => x.GetComponent<Tile>().Type == type).FirstOrDefault();
            var tile = (GameObject)Instantiate(tilePrefab, position, Quaternion.identity);
            var tileComponent = tile.GetComponent<Tile>();
            tiles[currentTile] = tileComponent;
            tileComponent.MoveToPoint(backgroundTiles[column, y].position);
            break;
        }
    }

    public bool IsCollidedWithNeighbour(RaycastHit2D hit)
    {
        var position = GetPoint(hit.collider.transform);
        var neighbours = GetNeighbours(selectedTile);
        if (neighbours.Any(pos => pos.X == position.X && pos.Y == position.Y))
        {
            tiles[selectedTile].StopSpinning();
            swappedTile = position;
            SwapTiles(selectedTile, swappedTile);
            if (tiles.FindMatches().ToList().Any())
                return false;
        }
        else
        {
            tiles[selectedTile].StopSpinning();
            selectedTile = null;
        }

        return true;
    }

    public bool UpdateStatus(GameState state, bool isPlayerTurn)
    {
        if (state == GameState.MovedTile)
        {
            var matches = tiles.FindMatches().ToList();
            if (matches.Any())
            {
                foreach (var tile in matches)
                {
                    gmScript.DealDamage(tile);
                    tile.Remove();
                }
            }
            else
            {
                SwapTiles(selectedTile, swappedTile);
            }
            selectedTile = null;
            swappedTile = null;
        }
        else if (state == GameState.None)
        {
            var matches = tiles.FindMatches().ToList();
            if (matches.Any())
            {
                foreach (var tile in matches)
                {
                    gmScript.DealDamage(tile);
                    tile.Remove();
                }
            }
            else if (!isPlayerTurn)
            {
                var cells = ConvertNewMapToOldMap();

                var points = AIManagerScript.Move(cells);

                SwapTiles(new Point(points.Item1.Column, size - 1 - points.Item1.Row),
                    new Point(points.Item2.Column, size - 1 - points.Item2.Row));

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
        }

        return isPlayerTurn;
    }

    private List<List<ICell>> ConvertNewMapToOldMap()
    {
        for (int i = 7; i >= 0; i--)
        {
            int j = 0;
            foreach (var tile in tiles.GetRow(i))
            {
                switch (tile.Type)
                {
                    case TileType.Apple:
                        field.cells[size - 1 - i][j] = new Apple();
                        break;
                    case TileType.Cpu:
                        field.cells[size - 1 - i][j] = new Chip();
                        break;
                    case TileType.Bacteria:
                        field.cells[size - 1 - i][j] = new Bacterium();
                        break;
                    case TileType.Slippers:
                        field.cells[size - 1 - i][j] = new Slipper();
                        break;
                    case TileType.Web:
                        field.cells[size - 1 - i][j] = new CockroachTrap();
                        break;
                    case TileType.Poison:
                        field.cells[size - 1 - i][j] = new Poison();
                        break;
                }

                j++;
            }
        }

        return field.cells;
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
        for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
                if (backgroundTiles[x, y] == position)
                    return new Point(x, y);
        return new Point(-1, -1);
    }

    public void Collapse()
    {
        for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
            {
                var currentTile = new Point(x, y);
                if (tiles[currentTile] != null) continue;
                var onlyEmptyLeft = true;
                for (var i = y + 1; i < size; i++)
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

    private void SetLevel()
    {
        AIManagerScript = AIManager.GetComponent<AIManager>();

        field = new Field();

        levelManager.Init();
    }
}