using UnityEngine;

public class PlayerSetter : MonoBehaviour
{
    [SerializeField]
    private MapIndexProvider _mapIndexProvider;
    [SerializeField]
    private Map _map;
    
    [SerializeField]
    private GameObject _playerPrefab;
    [SerializeField]
    private Transform _tilePrefab;

    private float _tileHeight;
    private PlayerController _player;

    private void Awake()
    {
        _tileHeight = _tilePrefab.localScale.y * 2 + 0.1f;
    }

    public PlayerController GetPlayer(int[,] intTileMap)
    {
        while (true)
        {
            var x = Random.Range(0, intTileMap.GetLength(0));
            var y = Random.Range(0, intTileMap.GetLength(1));
            var randomIndex = new Vector2Int(x, y);

            if (intTileMap[x, y] == -1)
            {
                continue;
            }
            
            var playerSetPosition = _mapIndexProvider.GetTilePosition(randomIndex);
            playerSetPosition.y += _tileHeight;
                
            SpawnPlayer();
            _player.transform.localPosition = playerSetPosition;
            
            return _player;
        }
    }

    private void SpawnPlayer()
    {
        var player = Instantiate(_playerPrefab);
        _player = player.GetComponent<PlayerController>();
        _player.transform.SetParent(_map.transform);
    }
}