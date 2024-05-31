using System.Collections;
using UnityEngine;
using System;

public class SawTrap : Trap
{
    [SerializeField] private bool closedLoop;
    [SerializeField] private Transform rootParentTransform;
    [SerializeField] private Vector2[] sawPoints;
    [SerializeField] private float sawSpeed;
    [SerializeField] private float sawRotationSpeed = 6;
    [SerializeField] private Transform movingElement;
    [SerializeField] private Transform rotatingElement;

    private Vector3 _rotateAroundAxis = Vector3.zero;
    private float _rotationSpeed;
    
    private void Start()
    {
        _rotateAroundAxis = rootParentTransform.rotation * Vector3.right;
        _rotationSpeed = 360f / sawRotationSpeed;
    }

    public override void ActivateTrap()
    {
        StartCoroutine(MoveSawBlade());
    }

    public override void DeactivateTrap()
    {
        StopCoroutine(MoveSawBlade());
    }
    
    private IEnumerator MoveSawBlade()
    {
        int pointIndex = 0;
        while (true)
        {
            Vector3 targetPoint = new Vector3(sawPoints[pointIndex].x, movingElement.localPosition.y, sawPoints[pointIndex].y);
            float sign = Vector3.Dot(Vector3.forward,targetPoint - movingElement.localPosition);
            
            while (movingElement.localPosition != targetPoint)
            {
                movingElement.localPosition = Vector3.MoveTowards(movingElement.localPosition, targetPoint, sawSpeed * Time.deltaTime);
                rotatingElement.RotateAround(movingElement.position, _rotateAroundAxis * sign, _rotationSpeed * Time.deltaTime);
                yield return null;
            }
            pointIndex = (pointIndex + 1) % sawPoints.Length;
            if (!closedLoop && pointIndex == 0)
            {
                Array.Reverse(sawPoints);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < sawPoints.Length; i++)
        {
            Vector3 pos = new Vector3(sawPoints[i].x, 0, sawPoints[i].y);
            Gizmos.DrawSphere(pos,0.3f);
        }
    }
}
