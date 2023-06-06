using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    [SerializeField] 
    private MapIndexProvider _mapIndexProvider;
    [SerializeField] 
    private Map _map;

    [SerializeField]
    private GameObject _obstacleTilePrefab;
    [SerializeField]
    private GameObject _simpleTilePrefab;

    private Camera _camera;
    private Tile _currentTile;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Start()
    {
        BuildTileMap();
    }

    private void Update()
    {
        var mousePosition = Input.mousePosition;
        var ray = _camera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out var hitInfo) || _currentTile == null)
        {
            return;
        }
        
        // Получаем индекс и позицию тайла по позиции курсора
        var tileIndex = _mapIndexProvider.GetIndex(hitInfo.point);
        var tilePosition = _mapIndexProvider.GetTilePosition(tileIndex);
        _currentTile.transform.localPosition = tilePosition;

        // Проверяем, доступно ли место для постройки тайла
        var isAvailable = _map.IsCellAvailable(tileIndex);
        // Задаем тайлу соответствующий цвет
        _currentTile.SetColor(isAvailable);
            
        // Если место недоступно для постройки - выходим из метода
        if (!isAvailable)
        {
            return;
        }
            
        // Если нажата ЛКМ - устанавливаем тайл 
        if (Input.GetMouseButtonDown(0))
        {
            _map.SetTile(tileIndex, _currentTile);
            _currentTile.ResetColor();
            _currentTile = null;
        }
    }
    
    public void StartPlacingTile(GameObject tilePrefab)
    {
        if (_currentTile != null)
        {
            Destroy(_currentTile.gameObject);
            return;
        }

        var tileObject = Instantiate(tilePrefab);
        _currentTile = tileObject.GetComponent<Tile>();
        
        if (tilePrefab == _obstacleTilePrefab)
        {
            _currentTile.SetAsObstacle();
        }
        
        _currentTile.transform.SetParent(_map.transform);
    }
    
    private void BuildTileMap()
    {
        for (var y = _map.Size.y - 1; y >= 0; y--)
        {
            for (var x = _map.Size.x - 1; x >= 0; x--)
            {
                var randomNumber = Random.Range(0, 10);
                if (randomNumber >= 7)
                {
                    var tileObject = Instantiate(_obstacleTilePrefab);
                    _currentTile = tileObject.GetComponent<Tile>();
                    _currentTile.SetAsObstacle();
                }
                else
                {
                    var tileObject = Instantiate(_simpleTilePrefab);
                    _currentTile = tileObject.GetComponent<Tile>();
                }
                
                _currentTile.transform.SetParent(_map.transform);
                var tileIndex = new Vector2Int(y, x);
                _map.SetTile(tileIndex, _currentTile);
                
                var tilePosition = _mapIndexProvider.GetTilePosition(tileIndex);
                _currentTile.transform.localPosition = tilePosition;
                _currentTile = null;
            }
        }
    }
}