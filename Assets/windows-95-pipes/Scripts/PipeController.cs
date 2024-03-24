using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PipeController : MonoBehaviour
{
    private Color _color;

    // speed at which the pipe moves
    private const float SpawnSpeed = 0.02f;
    // minimum number of straight segments before a corner can be created
    // this is to avoid creating corners too close to each other
    private const int MinStraightSegments = 1;

    public static Vector3 CornerSize = new Vector3(1.25f, 1.25f, 1.25f);
    public static Vector3 PipeSegmentSize = new Vector3(0.75f, 0.5f, 0.75f);
    
    private Vector3 _basePosition;
    private Vector3 _currentPosition;
    private Vector3 _currentDirection = Vector3.zero;
    
    private List<GameObject> _pipeSegments = new List<GameObject>();
    
    private bool _isDone = false;

    private int _straightSegments = 0;
    
    // Start is called before the first frame update
    IEnumerator Start()
    {
        _basePosition = transform.position;
        _currentPosition = _basePosition;
        _color = new Color(Random.value, Random.value, Random.value);
        while(!_isDone)
        {
            yield return new WaitForSeconds(SpawnSpeed);

            AddSegment();
        }
    }

    void Update()
    {
        // create a new pipe segment every SpawnSpeed seconds interval
    }

    void AddSegment()
    {
        var newDirection = GetNewDirection(Vector3Int.RoundToInt(_basePosition));
        
        if(newDirection == new Vector3(0,0,0))
        {
            ReplaceLastSegment();
            _isDone = true;
            GridController.Instance.CreateNewPipe();
            return;
        }

        if (newDirection != _currentDirection)
        {
            _straightSegments = 0;
            ReplaceLastSegment();
        }
        else
        {
            _straightSegments++;
        }
        
        _currentDirection = newDirection;
        _currentPosition = _basePosition + newDirection;
        var direction = _currentPosition - _basePosition;
        _basePosition = _currentPosition;
        CreatePipeSegment(Vector3Int.RoundToInt(_currentPosition), direction);
    }
    
    void CreateCorner(Vector3Int position)
    {
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cylinder.transform.localScale = CornerSize;
        cylinder.transform.position = position;
        cylinder.GetComponent<MeshRenderer>().material.color = _color;

        GridController.Instance.AddPipe(position, gameObject);
        _pipeSegments.Add(cylinder);
    }

    void CreatePipeSegment(Vector3Int position, Vector3 direction)
    {
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.localScale = PipeSegmentSize;
        cylinder.transform.position = position;
        cylinder.transform.rotation = Quaternion.LookRotation(direction);
        // add additional 90 degrees to make the pipe face in the right direction
        cylinder.transform.Rotate(Vector3.right, 90);
        cylinder.GetComponent<Renderer>().material.color = _color;
        
        GridController.Instance.AddPipe(position, gameObject);
        _pipeSegments.Add(cylinder);
    }
    
    void ReplaceLastSegment()
    {
        if(_pipeSegments.Count > 0) {
            Destroy(_pipeSegments[^1]);
            _pipeSegments.RemoveAt(_pipeSegments.Count - 1);
        }
        CreateCorner(Vector3Int.RoundToInt(_currentPosition));
    }
    
    Vector3 GetNewDirection(Vector3Int position)
    {
        List<Vector3Int> neighbours = GridController.Instance.GetAllAvailableNeighbours(position);
        
        if (neighbours.Count == 0)
        {
            return new Vector3(0,0,0);
        }
        
        if(_currentDirection != Vector3.zero && _straightSegments < MinStraightSegments)
        {
            if(neighbours.Contains(Vector3Int.RoundToInt(_currentDirection)))
            {
                return _currentDirection;
            }
            return new Vector3(0,0,0);
        }
        
        var randomValue = Random.value;

        // in ~3 out of 4 cases, go straight, if possible
        if (randomValue < 0.75f && neighbours.Contains(Vector3Int.RoundToInt(_currentDirection)))
        {
            return _currentDirection;
        }
        
        return neighbours[Random.Range(0, neighbours.Count)];
    }
}
