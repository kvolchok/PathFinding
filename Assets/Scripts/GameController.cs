using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private Map _map;
    [SerializeField]
    private MapIndexProvider _mapIndexProvider;
    
    [SerializeField]
    private GameObject _playerPrefab;
    [SerializeField]
    private Transform _tilePrefab;
    
    [SerializeField]
    private LayerMask _layer;

    [SerializeField]
    private float _maxRayDistance = 100f;
    
    private Tile[,] _tiles;
    private int[,] _tileMap;
    private Queue<Vector2Int> _points;
    private Stack<Vector3> _shortestRoute;
    private Vector2Int _mapsIndexDifference;
    private Camera _camera;
    
    private PlayerController _player;
    private bool _isPlayerReadyToGo;
    
    private Tile _lastHighlightedTile;
    private Tile _currentTile;
    private float _tileHeight;

    private void Awake()
    {
        _tiles = new Tile[_map.Size.x, _map.Size.y];
        _tileMap = new int[_map.Size.x + 2, _map.Size.y + 2];
        _points = new Queue<Vector2Int>();
        _shortestRoute = new Stack<Vector3>();
        _mapsIndexDifference = new Vector2Int(1, 1);
        _tileHeight = _tilePrefab.localScale.y * 2 + 0.1f;
        _camera = Camera.main;
    }

    private void Start()
    {
        for (var x = 0; x < _tileMap.GetLength(0); x++)
        {
            for (var y = 0; y < _tileMap.GetLength(1); y++)
            {
                _tileMap[x, y] = -1;
            }
        }
    }

    public void EndBuildingTiles()
    {
        SetTileMap();
        SetPlayer();
        FindPossibleRoutes();
        _isPlayerReadyToGo = true;
    }
    
    private void SetTileMap()
    {
        _tiles = _map.GetTiles();

        for (var x = 0; x < _tileMap.GetLength(0) - 2; x++)
        {
            for (var y = 0; y < _tileMap.GetLength(1) - 2; y++)
            {
                if (_tiles[x, y].IsObstacle)
                {
                    _tileMap[x + 1, y + 1] = -1;
                }
                else
                {
                    _tileMap[x + 1, y + 1] = 0;
                }
            }
        }
    }
    
    private void SetPlayer()
    {
        while (true)
        {
            var x = Random.Range(1, _tileMap.GetLength(0) - 1);
            var y = Random.Range(1, _tileMap.GetLength(1) - 1);
            var randomIndex = new Vector2Int(x, y);

            if (_tileMap[x, y] == -1)
            {
                continue;
            }
            
            var playerSetPosition = _mapIndexProvider.GetTilePosition(randomIndex - _mapsIndexDifference);
            playerSetPosition.y += _tileHeight;
                
            var player = Instantiate(_playerPrefab);
            _player = player.GetComponent<PlayerController>();
            _player.transform.SetParent(_map.transform);
            _player.transform.localPosition = playerSetPosition;
            
            _player.OnPlayerStay.AddListener(() => _shortestRoute.Clear());
            _player.OnPlayerStay.AddListener(() => _isPlayerReadyToGo = true);
            _player.OnPlayerStay.AddListener(ResetAllHighlights);
            _player.OnPlayerStay.AddListener(SetTileMap);
            _player.OnPlayerStay.AddListener(FindPossibleRoutes);
            return;
        }
    }

    private void FindPossibleRoutes()
    {
        var playerPosition = _player.transform.localPosition;
        playerPosition.y = 0;
        var currentIndex = _mapIndexProvider.GetIndex(playerPosition) + _mapsIndexDifference;
        _tileMap[currentIndex.x, currentIndex.y] = -10;
        _points.Enqueue(currentIndex);

        var end = 1;
        var step = 1;
        var counter = 0;

        while (true)
        {
            for (var i = 0; i < end; i++)
            {
                var currentPoint = _points.Dequeue();
                var x = currentPoint.x;
                var y = currentPoint.y;

                if (_tileMap[x, y + 1] == 0)
                {
                    _tileMap[x, y + 1] = step;
                    _points.Enqueue(new Vector2Int(x, y + 1));
                    counter++;
                }
                
                if (_tileMap[x, y - 1] == 0)
                {
                    _tileMap[x, y - 1] = step;
                    _points.Enqueue(new Vector2Int(x, y - 1));
                    counter++;
                }
                
                if (_tileMap[x + 1, y] == 0)
                {
                    _tileMap[x + 1, y] = step;
                    _points.Enqueue(new Vector2Int(x + 1, y));
                    counter++;
                }
                
                if (_tileMap[x - 1, y] == 0)
                {
                    _tileMap[x - 1, y] = step;
                    _points.Enqueue(new Vector2Int(x - 1, y));
                    counter++;
                }
            }

            if (_points.Count == 0)
            {
                break;
            }

            end = counter;
            counter = 0;
            step++;
        }
    }

    private void Update()
    {
        if (!_isPlayerReadyToGo)
        {
            return;
        }
        
        var mousePosition = Input.mousePosition;
        var ray = _camera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out var hitInfo, _maxRayDistance, _layer))
        {
            return;
        }
        
        var currentTileIndex = _mapIndexProvider.GetIndex(hitInfo.point);
        HighlightTile(currentTileIndex);
        currentTileIndex += _mapsIndexDifference;

        if (Input.GetMouseButtonDown(0) && IsTileAvailable(currentTileIndex))
        {
            FindShortestRoute(currentTileIndex);
            _player.Move(_shortestRoute);
            _isPlayerReadyToGo = false;
        }
    }

    private void HighlightTile(Vector2Int currentTileIndex)
    {
        _currentTile = _tiles[currentTileIndex.x, currentTileIndex.y];
        currentTileIndex += _mapsIndexDifference;

        if (_lastHighlightedTile != null)
        {
            _lastHighlightedTile.ResetColor();
        }

        if (_currentTile != null)
        {
            _currentTile.SetColor(IsTileAvailable(currentTileIndex));
        }

        _lastHighlightedTile = _currentTile;
    }

    private void ResetAllHighlights()
    {
        foreach (var tile in _tiles)
        {
            tile.ResetColor();
        }
    }

    private bool IsTileAvailable(Vector2Int currentTileIndex)
    {
        return _tileMap[currentTileIndex.x, currentTileIndex.y] > 0;
    }

    private void FindShortestRoute(Vector2Int currentTileIndex)
    {
        var currentPoint = _mapIndexProvider.GetTilePosition(currentTileIndex - _mapsIndexDifference);
        currentPoint.y += _tileHeight;
        _shortestRoute.Push(currentPoint);
        var x = currentTileIndex.x;
        var y = currentTileIndex.y;
        
        if (_tileMap[x, y] == 0)
        {
            return;
        }
        
        _tiles[x - 1, y - 1].SetColor(true);
        
        var step = _tileMap[x, y] - 1;
        while (step >= 0)
        {
            currentPoint = _shortestRoute.Peek(); 
            currentTileIndex = _mapIndexProvider.GetIndex(currentPoint) + _mapsIndexDifference;
            x = currentTileIndex.x;
            y = currentTileIndex.y;

            if (_tileMap[x, y + 1] == step || _tileMap[x, y + 1] == -10)
            {
                _tiles[x - 1, y].SetColor(true);
                currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x, y + 1) - _mapsIndexDifference);
                currentPoint.y += _tileHeight;
                _shortestRoute.Push(currentPoint);
            }
            else if (_tileMap[x, y - 1] == step || _tileMap[x, y - 1] == -10)
            {
                _tiles[x - 1, y - 2].SetColor(true);
                currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x, y - 1) - _mapsIndexDifference);
                currentPoint.y += _tileHeight;
                _shortestRoute.Push(currentPoint);
            }
            
            else if (_tileMap[x + 1, y] == step || _tileMap[x + 1, y] == -10)
            {
                _tiles[x, y - 1].SetColor(true);
                currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x + 1, y) - _mapsIndexDifference);
                currentPoint.y += _tileHeight;
                _shortestRoute.Push(currentPoint);
            }
            
            else if (_tileMap[x - 1, y] == step || _tileMap[x - 1, y] == -10)
            {
                _tiles[x - 2, y - 1].SetColor(true);
                currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x - 1, y) - _mapsIndexDifference);
                currentPoint.y += _tileHeight;
                _shortestRoute.Push(currentPoint);
            }

            step--;
        }
    }

    private void OnDisable()
    {
        _player.OnPlayerStay.RemoveAllListeners();
    }
}