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
    private int[,] _possibleRoutes;
    private Queue<Vector2Int> _points;
    private Stack<Vector3> _shortestRoute;
    private Camera _camera;
    
    private PlayerController _player;
    private bool _isPlayerReadyToGo;
    
    private Tile _lastHighlightedTile;
    private Tile _currentTile;
    private float _tileHeight;

    private void Awake()
    {
        _tiles = new Tile[_map.Size.x, _map.Size.y];
        _possibleRoutes = new int[_map.Size.x, _map.Size.y];
        _points = new Queue<Vector2Int>();
        _shortestRoute = new Stack<Vector3>();
        _tileHeight = _tilePrefab.localScale.y * 2 + 0.1f;
        _camera = Camera.main;
    }

    private void Start()
    {
        for (var x = 0; x < _possibleRoutes.GetLength(0); x++)
        {
            for (var y = 0; y < _possibleRoutes.GetLength(1); y++)
            {
                _possibleRoutes[x, y] = -1;
            }
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

        if (Input.GetMouseButtonDown(0) && IsTileAvailable(currentTileIndex))
        {
            FindShortestRoute(currentTileIndex);
            _player.Move(_shortestRoute);
            _isPlayerReadyToGo = false;
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

        for (var x = 0; x < _possibleRoutes.GetLength(0); x++)
        {
            for (var y = 0; y < _possibleRoutes.GetLength(1); y++)
            {
                if (!_tiles[x, y].IsObstacle)
                {
                    _possibleRoutes[x, y] = 0;
                }
            }
        }
    }
    
    private void SetPlayer()
    {
        while (true)
        {
            var x = Random.Range(0, _possibleRoutes.GetLength(0));
            var y = Random.Range(0, _possibleRoutes.GetLength(1));
            var randomIndex = new Vector2Int(x, y);

            if (_possibleRoutes[x, y] == -1)
            {
                continue;
            }
            
            var playerSetPosition = _mapIndexProvider.GetTilePosition(randomIndex);
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
        var currentIndex = _mapIndexProvider.GetIndex(playerPosition);
        _possibleRoutes[currentIndex.x, currentIndex.y] = -10;
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

                if (x < _possibleRoutes.GetLength(0) - 1)
                {
                    if (_possibleRoutes[x + 1, y] == 0)
                    {
                        _possibleRoutes[x + 1, y] = step;
                        _points.Enqueue(new Vector2Int(x + 1, y));
                        counter++;
                    }
                }

                if (x > 0)
                {
                    if (_possibleRoutes[x - 1, y] == 0)
                    {
                        _possibleRoutes[x - 1, y] = step;
                        _points.Enqueue(new Vector2Int(x - 1, y));
                        counter++;
                    }
                }
                
                if (y < _possibleRoutes.GetLength(1) - 1)
                {
                    if (_possibleRoutes[x, y + 1] == 0)
                    {
                        _possibleRoutes[x, y + 1] = step;
                        _points.Enqueue(new Vector2Int(x, y + 1));
                        counter++;
                    }
                    
                }

                if (y > 0)
                {
                    if (_possibleRoutes[x, y - 1] == 0)
                    {
                        _possibleRoutes[x, y - 1] = step;
                        _points.Enqueue(new Vector2Int(x, y - 1));
                        counter++;
                    }
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

    private void HighlightTile(Vector2Int currentTileIndex)
    {
        _currentTile = _tiles[currentTileIndex.x, currentTileIndex.y];

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
        return _possibleRoutes[currentTileIndex.x, currentTileIndex.y] > 0;
    }

    private void FindShortestRoute(Vector2Int currentTileIndex)
    {
        var currentPoint = _mapIndexProvider.GetTilePosition(currentTileIndex);
        currentPoint.y += _tileHeight;
        _shortestRoute.Push(currentPoint);
        var x = currentTileIndex.x;
        var y = currentTileIndex.y;
        
        if (_possibleRoutes[x, y] == 0)
        {
            return;
        }
        
        _tiles[x, y].SetColor(true);
        
        var step = _possibleRoutes[x, y] - 1;

        while (step >= 0)
        {
            currentPoint = _shortestRoute.Peek(); 
            currentTileIndex = _mapIndexProvider.GetIndex(currentPoint);
            x = currentTileIndex.x;
            y = currentTileIndex.y;
            
            var wasPointAdded = false;
            
            if (x < _possibleRoutes.GetLength(0) - 1 && !wasPointAdded)
            {
                if (_possibleRoutes[x + 1, y] == step || _possibleRoutes[x + 1, y] == -10)
                {
                    _tiles[x + 1, y].SetColor(true);
                    currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x + 1, y));
                    currentPoint.y += _tileHeight;
                    _shortestRoute.Push(currentPoint);
                    wasPointAdded = true;
                }
            }

            if (x > 0 && !wasPointAdded)
            {
                if (_possibleRoutes[x - 1, y] == step || _possibleRoutes[x - 1, y] == -10)
                {
                    _tiles[x - 1, y].SetColor(true);
                    currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x - 1, y));
                    currentPoint.y += _tileHeight;
                    _shortestRoute.Push(currentPoint);
                    wasPointAdded = true;
                }
            }
            
            if (y < _possibleRoutes.GetLength(1) - 1 && !wasPointAdded)
            {
                if (_possibleRoutes[x, y + 1] == step || _possibleRoutes[x, y + 1] == -10)
                {
                    _tiles[x, y + 1].SetColor(true);
                    currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x, y + 1));
                    currentPoint.y += _tileHeight;
                    _shortestRoute.Push(currentPoint);
                    wasPointAdded = true;
                }
            }
            
            if (y > 0 && !wasPointAdded)
            {
                if (_possibleRoutes[x, y - 1] == step || _possibleRoutes[x, y - 1] == -10)
                {
                    _tiles[x, y - 1].SetColor(true);
                    currentPoint = _mapIndexProvider.GetTilePosition(new Vector2Int(x, y - 1));
                    currentPoint.y += _tileHeight;
                    _shortestRoute.Push(currentPoint);
                }
            }

            step--;
        }
    }

    private void OnDisable()
    {
        _player.OnPlayerStay.RemoveAllListeners();
    }
}