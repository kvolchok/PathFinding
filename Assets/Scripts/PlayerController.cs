using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public UnityEvent PlayerFinishedMoving;
    
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    [SerializeField]
    private float _speed;
    
    private Animator _animator;
    private Vector3 _startPoint;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Move(Stack<Vector3> route)
    {
        _startPoint = route.Pop();
        StartCoroutine(MoveCoroutine(route));
    }

    private IEnumerator MoveCoroutine(Stack<Vector3> route)
    {
        _animator.SetBool(IsMoving, true);
        
        while (route.Count > 0)
        {
            var nextPoint = route.Pop();
            
            var travelDistance = Vector3.Distance(_startPoint, nextPoint);
            var travelTime = travelDistance / _speed;
            var currentTime = 0f;

            while (currentTime < travelTime)
            {
                var progress = currentTime / travelTime;
                var currentPoint = Vector3.Lerp(_startPoint, nextPoint, progress);
                transform.LookAt(currentPoint);
                transform.position = currentPoint;
                currentTime += Time.deltaTime;

                yield return null;
            }

            _startPoint = nextPoint;
        }
        
        _animator.SetBool(IsMoving, false);
        PlayerFinishedMoving.Invoke();
    }
}