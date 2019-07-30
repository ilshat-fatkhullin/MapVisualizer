using System.Collections.Generic;
using UnityEngine.Events;

public class Tilemap
{
    public Tile CenterTile
    {
        get { return centerTile; }

        private set
        {
            if (centerTile != value)
            {
                centerTile = value;
                OnCenterTileChanged.Invoke();
            }
        }
    }

    private Tile centerTile;

    public UnityEvent OnCenterTileChanged;

    private Queue<Tile> tilesToVisualize;

    private Queue<Tile> tilesToRemove;

    private List<Tile> visualizedTiles;

    public Tilemap(Tile centerTile)
    {
        tilesToVisualize = new Queue<Tile>();
        tilesToRemove = new Queue<Tile>();
        visualizedTiles = new List<Tile>();
        OnCenterTileChanged = new UnityEvent();

        UpdateCenterTile(centerTile);
    }

    public void UpdateCenterTile(Tile centerTile)
    {
        CenterTile = centerTile;

        tilesToVisualize.Clear();

        for (int x = centerTile.X - 1; x <= centerTile.X + 1; x++)
            for (int y = centerTile.Y - 1; y <= centerTile.Y + 1; y++)
            {
                if (visualizedTiles.Find(t => t.X == x && t.Y == y) != null)
                    continue;
                tilesToVisualize.Enqueue(new Tile(x, y, centerTile.Zoom));
            }

        for (int i = 0; i < visualizedTiles.Count; i++)
        {
            Tile tile = visualizedTiles[i];

            if (tile.X >= centerTile.X - 1 && tile.X <= centerTile.X + 1 &&
                tile.Y >= centerTile.Y - 1 && tile.Y <= centerTile.Y + 1)
                continue;

            visualizedTiles.RemoveAt(i);
            i--;
            tilesToRemove.Enqueue(tile);            
        }
    }

    public bool HasTilesToVisualize()
    {
        return tilesToVisualize.Count > 0;
    }

    public Tile DequeueTileToVisualize()
    {
        Tile tile = tilesToVisualize.Dequeue();
        visualizedTiles.Add(tile);
        return tile;
    }

    public bool HasTilesToRemove()
    {
        return tilesToRemove.Count > 0;
    }

    public Tile DequeueTileToRemove()
    {
        return tilesToRemove.Dequeue();
    }
}