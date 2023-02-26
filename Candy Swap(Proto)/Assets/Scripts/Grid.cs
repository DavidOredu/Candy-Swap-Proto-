using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private Tile tilePrefab;
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private CandyData candyData;

    private Tile[,] grid;

    private Tile currentTile;
    private Tile clickedTile;

    private float normalAlpha = .2f;
    private float highlightedAlpha = .6f;

    public Dictionary<Vector2, Tile> tiles;

    public static event Action OnTileEnter;
    public static event Action OnTileExit;
    public static event Action OnTileClick;
    public static event Action OnTileUnclick;
    public static event Action OnTileDrag;

    private List<Tile> debugTiles = new List<Tile>();
    // Start is called before the first frame update

    void Start()
    {
        grid = new Tile[width, height];
        GenerateGrid();
    }
    private void OnEnable()
    {
        OnTileEnter += HandleTileEnter;
        OnTileExit += HandleTileExit;
        OnTileClick += HandleTileClick;
        OnTileUnclick += HandleTileUnclick;
    }
    // Update is called once per frame
    void Update()
    {
        CheckMatchingTiles();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomizeColors();
        }
    }
    private void GenerateGrid()
    {
        tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = Instantiate(this.tilePrefab, new Vector3(x, y), Quaternion.identity, transform);
                tile.positionInGrid = new Vector2Int(x, y);
                tile.tileRenderer.color = candyData.candyColors[UnityEngine.Random.Range(0, candyData.candyColors.Length)];
                tile.grid = this;

                tiles.Add(new Vector2(x, y), tile);
                grid[x, y] = tile;
            }
        }
        var position = grid[width / 2, height / 2].transform.position;
        Camera.main.transform.position = new Vector3(position.x - .5f, position.y, Camera.main.transform.position.z);
    }
    private void CheckMatchingTiles()
    {
        List<Tile> matchingTiles = new List<Tile>();

        for (int x = 0; x < 1; x++)
        {
            matchingTiles.Clear();
            for (int y = 0; y < height; y++)
            {
                if(!debugTiles.Contains(grid[x, y]))
                  debugTiles.Add(grid[x, y]);
                if(matchingTiles.Count == 0)
                {
                    matchingTiles.Add(grid[x, y]);
                    continue;
                }

                if(matchingTiles[0].GetTileColor() == grid[x, y].GetTileColor())
                {
                    matchingTiles.Add(grid[x, y]);
                }
                else
                {
                    if(matchingTiles.Count >= 3)
                    {
                        BreakTiles(matchingTiles);
                        matchingTiles.Clear();
                    }
                    else
                    {
                        matchingTiles.Clear();
                        continue;
                    }
                }
            }
            
        }
    }
    private void BreakTiles(List<Tile> tiles)
    {
        foreach (var tile in tiles)
        {
            tile.SetTileAlpha(0);
        }
    }
    private void CheckSwapTile(Tile oldTile, Tile newTile)
    {
        if (TouchingBorders(oldTile, newTile))
        {
            ChangeTilePosition(oldTile, newTile);
            OnTileUnclicked();
        }
        else
        {
            OnTileUnclicked();
        }
    }
    /// <summary>
    /// Changes two tile's relative positions.
    /// </summary>
    /// <param name="tile1"></param>
    /// <param name="tile2"></param>
    private void ChangeTilePosition(Tile tile1, Tile tile2)
    {
        var temp = tile2.positionInGrid;
        tile2.positionInGrid = tile1.positionInGrid;
        tile1.positionInGrid = temp;
        grid[tile1.positionInGrid.x, tile1.positionInGrid.y] = tile2;
        grid[temp.x, temp.y] = tile1;

        //tile1.transform.position = new Vector3(tile1.positionInGrid.x, tile1.positionInGrid.y);
        //tile2.transform.position = new Vector3(tile2.positionInGrid.x, tile2.positionInGrid.y);

        Vector3 tile1NewPosition = new Vector3(tile1.positionInGrid.x, tile1.positionInGrid.y);
        Vector3 tile2NewPosition = new Vector3(tile2.positionInGrid.x, tile2.positionInGrid.y);

        while (tile1.transform.position != tile1NewPosition && tile2.transform.position != tile2NewPosition)
        {
            tile1.transform.position = Vector3.MoveTowards(tile1.transform.position, tile1NewPosition, .1f);
            tile2.transform.position = Vector3.MoveTowards(tile2.transform.position, tile2NewPosition, .1f);
        }
    }
    /// <summary>
    /// Checks if a tile is within surrounding borders of another tile.
    /// </summary>
    /// <param name="oldTile">Tile with borders.</param>
    /// <param name="newTile">Tile to be within borders.</param>
    /// <returns>True if the NewTile is within the OldTiles's borders.</returns>
    private bool TouchingBorders(Tile oldTile, Tile newTile)
    {
        List<Tile> surroundingTiles = new List<Tile>();

        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (x != 0 && y != 0) { continue; }

                var tile = GetTileAtPosition(new Vector2(oldTile.positionInGrid.x + x, oldTile.positionInGrid.y + y));
                if (tile)
                    surroundingTiles.Add(tile);
            }
        }

        return surroundingTiles.Contains(newTile);
    }
    /// <summary>
    /// Debug function to randomize all tile colors.
    /// </summary>
    private void RandomizeColors()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y].tileRenderer.color = candyData.candyColors[UnityEngine.Random.Range(0, candyData.candyColors.Length)];
            }
        }
    }
    /// <summary>
    /// Get's a tile at a given position.
    /// </summary>
    /// <param name="pos">Position of requested tile.</param>
    /// <returns>Requested tile.</returns>
    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (tiles.TryGetValue(pos, out var tile))
            return tile;
        return null;
    }


    private void HandleTileEnter()
    {
        currentTile.SetTileHighlightAlpha(normalAlpha);
    }
    private void HandleTileExit()
    {
        if (currentTile != clickedTile && currentTile)
            currentTile.SetTileHighlightAlpha(0);
    }
    private void HandleTileClick()
    {
        if (clickedTile)
        {
            CheckSwapTile(clickedTile, currentTile);
        }
        else
        {
            clickedTile = currentTile;
        }

        currentTile.SetTileHighlightAlpha(highlightedAlpha);
    }
    private void HandleTileUnclick()
    {
        clickedTile.SetTileHighlightAlpha(0);

        clickedTile = null;
    }
    public void OnTileEntered() => OnTileEnter?.Invoke();
    public void OnTileExited() => OnTileExit?.Invoke();
    public void OnTileClicked() => OnTileClick?.Invoke();
    public void OnTileUnclicked() => OnTileUnclick?.Invoke();
    public void OnTileDragged() => OnTileDrag?.Invoke();

    public Tile GetCurrentTile() { return currentTile; }
    public Tile GetClickedTile() { return clickedTile; }
    public void SetCurrentTile(Tile tile) => currentTile = tile;
}
