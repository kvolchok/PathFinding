using System.Collections.Generic;
using UnityEngine;

public class HighlightingTilesController : MonoBehaviour
{
    private MapIndexProvider _mapIndexProvider;
    private Map _map;

    private Tile[,] _tiles;
    private Tile _lastHighlightedTile;
    private Tile _currentTile;

    public void Initialize(MapIndexProvider mapIndexProvider, Map map)
    {
        _mapIndexProvider = mapIndexProvider;
        _map = map;
        _tiles = _map.GetTiles();
    }

    public void HighlightTile(Vector2Int currentTileIndex, RouteController routeController)
    {
        _currentTile = _tiles[currentTileIndex.x, currentTileIndex.y];

        if (_lastHighlightedTile != null)
        {
            _lastHighlightedTile.ResetColor();
        }

        if (_currentTile != null)
        {
            _currentTile.SetColor(routeController.IsTileFree(currentTileIndex));
        }

        _lastHighlightedTile = _currentTile;
    }

    public void ResetAllHighlights()
    {
        foreach (var tile in _tiles)
        {
            tile.ResetColor();
        }
    }

    public void HighlightRoute(Stack<Vector3> route)
    {
        foreach (var position in route)
        {
            var tileIndex = _mapIndexProvider.GetIndex(position);
            _tiles[tileIndex.x, tileIndex.y].SetColor(true);
        }
    }
}