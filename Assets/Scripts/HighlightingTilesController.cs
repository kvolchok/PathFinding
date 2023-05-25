using System.Collections.Generic;
using UnityEngine;

public class HighlightingTilesController : MonoBehaviour
{
    [SerializeField]
    private MapIndexProvider _mapIndexProvider;
    [SerializeField]
    private Map _map;
    [SerializeField]
    private RouteController _routeController;

    private Tile[,] _tiles;
    private Tile _lastHighlightedTile;
    private Tile _currentTile;

    private void Awake()
    {
        _tiles = _map.GetTiles();
    }

    public void HighlightTile(Vector2Int currentTileIndex)
    {
        _currentTile = _tiles[currentTileIndex.x, currentTileIndex.y];

        if (_lastHighlightedTile != null)
        {
            _lastHighlightedTile.ResetColor();
        }

        if (_currentTile != null)
        {
            _currentTile.SetColor(_routeController.IsTileFree(currentTileIndex));
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