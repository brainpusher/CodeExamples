using System;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public Action<Vector3Int> onDirectionChanged = delegate(Vector3Int direction) {  };
    
    public bool IsCanMove { get; set; }
    
    private  bool _isDragging;

    private Vector2 _startTouch;
    private Vector2 _swipeDelta;
    
    private Vector3Int _direction = Vector3Int.zero;
    
    private void Update()
    {
        if (!IsCanMove) return;
        
#if UNITY_EDITOR
        StandaloneInput();
#elif UNITY_ANDROID || UNITY_IOS
        MobileInput();
#endif       
        _swipeDelta = Vector2.zero;
        if (_isDragging)
        {
            if (Input.touches.Length > 0)
                _swipeDelta = Input.touches[0].position - _startTouch;
            else if (Input.GetMouseButton(0))
            {
                _swipeDelta = (Vector2)Input.mousePosition - _startTouch;
            }
        }
        
        if (_swipeDelta.magnitude > 50)
        {
            float x = _swipeDelta.x;
            float y = _swipeDelta.y;
            
            if(Mathf.Abs(x) > Mathf.Abs(y))
                _direction = x > 0 ? Vector3Int.right : Vector3Int.left;
            else
                _direction = y > 0 ? Vector3Int.forward : Vector3Int.back;

            
            onDirectionChanged?.Invoke(_direction);
            
            Reset();
        }
    }

    private void Reset()
    {
        _startTouch = Vector2.zero;
        _swipeDelta = Vector2.zero;
        
        _isDragging = false;
    }

    private void StandaloneInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _startTouch = Input.mousePosition;
        } 
        else if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            Reset();
        }
    }

    private void MobileInput()
    {
        if (Input.touches.Length > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                _isDragging = true;
                _startTouch = Input.touches[0].position;
            } 
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                _isDragging = false;
                Reset();
            }

        }
    }
}