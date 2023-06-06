using System.Collections.Generic;
using UnityEngine;

public class RouteController : MonoBehaviour
{
    [SerializeField]
    private MapIndexProvider _mapIndexProvider;
    [SerializeField]
    private Map _map;
    
    [SerializeField]
    private Transform _tilePrefab;
    
    private Tile[,] _tiles;
    private int[,] _distancesFromStartPoint;

    private PlayerController _player;
    private float _tileHeight;

    private void Awake()
    {
        _tiles = new Tile[_map.Size.x, _map.Size.y];
        _distancesFromStartPoint = new int[_map.Size.x, _map.Size.y];
        _tileHeight = _tilePrefab.localScale.y * 2 + 0.1f;
    }
    
    private void Start()
    {
        for (var x = 0; x < _distancesFromStartPoint.GetLength(0); x++)
        {
            for (var y = 0; y < _distancesFromStartPoint.GetLength(1); y++)
            {
                _distancesFromStartPoint[x, y] = -1;
            }
        }
    }

    public int[,] GetTilesMap()
    {
        _tiles = _map.GetTiles();

        for (var x = 0; x < _distancesFromStartPoint.GetLength(0); x++)
        {
            for (var y = 0; y < _distancesFromStartPoint.GetLength(1); y++)
            {
                if (!_tiles[x, y].IsObstacle)
                {
                    _distancesFromStartPoint[x, y] = 0;
                }
            }
        }

        return _distancesFromStartPoint;
    }

    public void FindDistances(Vector3 startPosition)
    {
        var currentIndex = _mapIndexProvider.GetIndex(startPosition);
        _distancesFromStartPoint[currentIndex.x, currentIndex.y] = -10;
        var points = new Queue<Vector2Int>();
        points.Enqueue(currentIndex);
        
        var end = 1;
        var step = 1;
        var counter = 0;

        while (points.Count != 0)
        {
            for (var i = 0; i < end; i++)
            {
                var currentPoint = points.Dequeue();
                var x = currentPoint.x;
                var y = currentPoint.y;

                if (x < _distancesFromStartPoint.GetLength(0) - 1 && _distancesFromStartPoint[x + 1, y] == 0)
                {
                    _distancesFromStartPoint[x + 1, y] = step;
                    points.Enqueue(new Vector2Int(x + 1, y));
                    counter++;
                }

                if (x > 0 && _distancesFromStartPoint[x - 1, y] == 0)
                {
                    _distancesFromStartPoint[x - 1, y] = step;
                    points.Enqueue(new Vector2Int(x - 1, y));
                    counter++;
                }
                
                if (y < _distancesFromStartPoint.GetLength(1) - 1 && _distancesFromStartPoint[x, y + 1] == 0)
                {
                    _distancesFromStartPoint[x, y + 1] = step;
                    points.Enqueue(new Vector2Int(x, y + 1));
                    counter++;
                }

                if (y > 0 && _distancesFromStartPoint[x, y - 1] == 0)
                {
                    _distancesFromStartPoint[x, y - 1] = step;
                    points.Enqueue(new Vector2Int(x, y - 1));
                    counter++;
                }
            }
            
            end = counter;
            counter = 0;
            step++;
        }
    }

    public bool IsTileFree(Vector2Int currentTileIndex)
    {
        return _distancesFromStartPoint[currentTileIndex.x, currentTileIndex.y] > 0;
    }

    public Stack<Vector3> GetShortestRoute(Vector2Int destinationTileIndex)
    {
        var currentPoint = _mapIndexProvider.GetTilePosition(destinationTileIndex);
        currentPoint.y += _map.Height;

        var shortestRoute = new Stack<Vector3>();
        shortestRoute.Push(currentPoint);
        var x = destinationTileIndex.x;
        var y = destinationTileIndex.y;

        var step = _distancesFromStartPoint[x, y] - 1;
        while (step >= 0)
        {
            currentPoint = shortestRoute.Peek(); 
            destinationTileIndex = _mapIndexProvider.GetIndex(currentPoint);
            x = destinationTileIndex.x;
            y = destinationTileIndex.y;
            
            var wasPointAdded = false;
            
            if (x < _distancesFromStartPoint.GetLength(0) - 1)
            {
                if (_distancesFromStartPoint[x + 1, y] == step || _distancesFromStartPoint[x + 1, y] == -10)
                {
                    currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x + 1, y));
                    currentPoint.y += _map.Height;
                    shortestRoute.Push(currentPoint);
                    wasPointAdded = true;
                }
            }

            if (x > 0 && !wasPointAdded)
            {
                if (_distancesFromStartPoint[x - 1, y] == step || _distancesFromStartPoint[x - 1, y] == -10)
                {
                    currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x - 1, y));
                    currentPoint.y += _map.Height;
                    shortestRoute.Push(currentPoint);
                    wasPointAdded = true;
                }
            }
            
            if (y < _distancesFromStartPoint.GetLength(1) - 1 && !wasPointAdded)
            {
                if (_distancesFromStartPoint[x, y + 1] == step || _distancesFromStartPoint[x, y + 1] == -10)
                {
                    currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x, y + 1));
                    currentPoint.y += _map.Height;
                    shortestRoute.Push(currentPoint);
                    wasPointAdded = true;
                }
            }
            
            if (y > 0 && !wasPointAdded)
            {
                if (_distancesFromStartPoint[x, y - 1] == step || _distancesFromStartPoint[x, y - 1] == -10)
                {
                    currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x, y - 1));
                    currentPoint.y += _map.Height;
                    shortestRoute.Push(currentPoint);
                }
            }

            step--;
        }
        
        return shortestRoute;
    }
}