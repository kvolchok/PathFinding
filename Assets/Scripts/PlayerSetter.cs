using UnityEngine;

public class PlayerSetter : MonoBehaviour
{
    [SerializeField]
    private GameObject _playerPrefab;
    
    private MapIndexProvider _mapIndexProvider;
    private Map _map;

    public void Initialize(MapIndexProvider mapIndexProvider, Map map)
    {
        _mapIndexProvider = mapIndexProvider;
        _map = map;
    }

    public PlayerController GetPlayer(int[,] tilesMap)
    {
        while (true)
        {
            var x = Random.Range(0, tilesMap.GetLength(0));
            var y = Random.Range(0, tilesMap.GetLength(1));
            var randomIndex = new Vector2Int(x, y);

            if (tilesMap[x, y] == -1)
            {
                continue;
            }
            
            var playerSetPosition = _mapIndexProvider.GetTilePosition(randomIndex);
            playerSetPosition.y += _map.Height;

            return SpawnPlayer(playerSetPosition);
        }
    }

    private PlayerController SpawnPlayer(Vector3 setPosition)
    {
        var playerObject = Instantiate(_playerPrefab);
        var player = playerObject.GetComponent<PlayerController>();
        player.transform.SetParent(_map.transform);
        player.transform.localPosition = setPosition;

        return player;
    }
}