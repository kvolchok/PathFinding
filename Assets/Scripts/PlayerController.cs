using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    public UnityEvent OnPlayerStay;
    
    [SerializeField]
    private float _speed;
    
    private Animator _animator;
    
    private MapIndexProvider _mapIndexProvider;
    private Vector3 _startPoint;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // По умолчанию у персонажа проигрывается анимация покоя.
    // Для того, чтобы запустить анимацию ходьбы - передавайте в параметр аниматора IsMoving значение true:
    // _animator.SetBool(IsMoving, true);
    // Для того, чтобы запустить анимацию покоя - передавайте в параметр аниматора IsMoving значение false:
    // _animator.SetBool(IsMoving, false);
    
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
        OnPlayerStay.Invoke();
    }
}