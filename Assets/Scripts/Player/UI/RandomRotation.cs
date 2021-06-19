using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(RectTransform))]
public class RandomRotation : MonoBehaviour
{
    [SerializeField] private float _rotationDegree;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private AnimationCurve _rotationSpeedCurve;

    private RectTransform _transform;
    private void OnEnable()
    {
        _transform = GetComponent<RectTransform>();
        StartCoroutine(RotationWander());
    }

    private IEnumerator RotationWander()
    {
        float _fromRotation = _transform.localEulerAngles.z;
        float _targetRotation = _transform.localEulerAngles.z;
        float lerpTime;
        float maxLerpTime;
        
        while (true)
        {
            _fromRotation = _targetRotation;
            _targetRotation = Random.Range(-_rotationDegree, _rotationDegree);
            float distance = Mathf.DeltaAngle(_fromRotation, _targetRotation);
            lerpTime = 0;
            maxLerpTime = Mathf.Abs(distance)/_rotationSpeed;
            while (lerpTime < maxLerpTime)
            {
                lerpTime += Time.deltaTime;
                _transform.localEulerAngles = new Vector3(0, 0,
                    Mathf.LerpAngle(_fromRotation, _targetRotation, _rotationSpeedCurve.Evaluate(lerpTime / maxLerpTime)));
                yield return null;
            }
            _transform.localEulerAngles = new Vector3(0, 0,_targetRotation);
        }
    }
}
