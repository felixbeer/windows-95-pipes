using System.Collections;
using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public Vector3Int gridSize;
    public GameObject pipePrefab;
    public GameObject camera;
    
    private Dictionary<Vector3Int, CellState> _grid = new();
    
    // this is not the cleanest way to do this, but allows access to the grid controller from pipes
    public static GridController Instance;
    
    void Awake()
    {
        Instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        CreateNewPipe();
        CenterCamera();
    }

    public void CreateNewPipe()
    {
        var cell = GetRandomFreeCell();
        if(cell == new Vector3Int(-1, -1, -1))
        {
            // no more free cells
            return;
        }
        Instantiate(pipePrefab, cell, Quaternion.identity);
        _grid[cell] = new CellState { pipe = pipePrefab };
    }
    
    Vector3Int GetRandomFreeCell()
    {
        var cell = new Vector3Int(Random.Range(0, gridSize.x), Random.Range(0, gridSize.y), Random.Range(0, gridSize.z));
        
        if(_grid.Count == gridSize.x * gridSize.y * gridSize.z)
        {
            Debug.Log("Grid is full");
            return new Vector3Int(-1, -1, -1);
        }
        
        while (_grid.ContainsKey(cell))
        {
            cell = new Vector3Int(Random.Range(0, gridSize.x), Random.Range(0, gridSize.y), Random.Range(0, gridSize.z));
        }

        return cell;
    }

    public List<Vector3Int> GetAllAvailableNeighbours(Vector3Int position)
    {
        var neighbours = new List<Vector3Int>();
        var directions = new[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1),
        };

        foreach (var direction in directions)
        {
            var neighbour = position + direction;
            
            if(neighbour.x < 0 || neighbour.x >= gridSize.x || neighbour.y < 0 || neighbour.y >= gridSize.y || neighbour.z < 0 || neighbour.z >= gridSize.z)
            {
                continue;
            }
            
            if (_grid.ContainsKey(neighbour))
            {
                continue;
            }

            neighbours.Add(direction);
        }

        return neighbours;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void AddPipe(Vector3Int position, GameObject pipe)
    {
        _grid[position] = new CellState { pipe = pipe };
    }

    public void CenterCamera()
    {
        camera.transform.position = new Vector3(gridSize.x / 2, gridSize.y / 2, -gridSize.z);
    }
}

public struct CellState
{
    [CanBeNull] public GameObject pipe;
}
