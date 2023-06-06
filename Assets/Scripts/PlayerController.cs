using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public UnityEvent OnPlayerFinishedMoving;
    
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    [SerializeField]
    private float _speed;
    
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Move(Stack<Vector3> route)
    {
        StartCoroutine(MoveCoroutine(route));
    }

    private IEnumerator MoveCoroutine(Stack<Vector3> route)
    {
        _animator.SetBool(IsMoving, true);
        route.Pop();
        
        while (route.Count > 0)
        {
            var nextPoint = route.Pop();
            
            var distanceToNextPoint = Vector3.Distance(transform.position, nextPoint);
            transform.LookAt(nextPoint);
            var travelDistance = 0f;

            while (travelDistance <= distanceToNextPoint)
            {
                travelDistance = _speed * Time.deltaTime;
                var currentPoint = Vector3.MoveTowards(transform.position, nextPoint, travelDistance);
                transform.position = currentPoint;
                distanceToNextPoint -= travelDistance;

                yield return null;
            }

            transform.position = nextPoint;
            yield return null;
        }
        
        _animator.SetBool(IsMoving, false);
        OnPlayerFinishedMoving.Invoke();
    }
}