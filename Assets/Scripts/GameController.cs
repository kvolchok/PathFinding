using JetBrains.Annotations;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private MapIndexProvider _mapIndexProvider;
    [SerializeField]
    private RouteController _routeController;
    [SerializeField]
    private PlayerSetter _playerSetter;
    [SerializeField]
    private HighlightingTilesController _highlightingTilesController;

    [SerializeField]
    private LayerMask _layer;
    [SerializeField]
    private float _maxRayDistance = 100f;
    
    private Camera _camera;
    private PlayerController _player;
    private bool _isPlayerReadyToGo;

    private void Awake()
    {
        _camera = Camera.main;
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
        _highlightingTilesController.HighlightTile(currentTileIndex);

        if (Input.GetMouseButtonDown(0) && _routeController.IsTileFree(currentTileIndex))
        {
            var shortestRoute = _routeController.GetShortestRoute(currentTileIndex);
            _highlightingTilesController.HighlightRoute(shortestRoute);
            _player.Move(shortestRoute);
            _isPlayerReadyToGo = false;
        }
    }

    [UsedImplicitly]
    public void StartGame()
    {
        var distancesFromPlayer = _routeController.GetTilesMap();
        _player = _playerSetter.GetPlayer(distancesFromPlayer);
        
        _player.PlayerFinishedMoving.AddListener(_highlightingTilesController.ResetAllHighlights);
        _player.PlayerFinishedMoving.AddListener(() => _routeController.GetTilesMap());
        _player.PlayerFinishedMoving.AddListener(() => _routeController.FindDistances(_player.transform.position));
        _player.PlayerFinishedMoving.AddListener(() => _isPlayerReadyToGo = true);
        
        _routeController.FindDistances(_player.transform.position);
        _isPlayerReadyToGo = true;
    }
    
    
    private void OnDisable()
    {
        _player.PlayerFinishedMoving.RemoveAllListeners();
    }
}