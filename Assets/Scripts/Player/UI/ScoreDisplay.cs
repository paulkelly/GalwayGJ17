using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private AnimationCurve _dilateCurve;
    [SerializeField] private float _dilateValueDefault;
    [SerializeField] private float _dilateValueMax;
    [SerializeField] private float _dilateTime;
    
    private TMP_Text _text;
    private Material _material;
    
    private bool _dilateInProgress;
    private float _dilateProgress;

    private void OnEnable()
    {
        _text = GetComponent<TMP_Text>();
        _material = _text.fontMaterial;
        ScoreTracker.OnScoreUpdated += ScoreTrackerOnOnScoreUpdated;
        SetScore(ScoreTracker.Score);
        _material.SetFloat(ShaderUtilities.ID_FaceDilate,_dilateValueDefault);   
    }

    private void OnDisable()
    {
        ScoreTracker.OnScoreUpdated -= ScoreTrackerOnOnScoreUpdated;
    }

    private void ScoreTrackerOnOnScoreUpdated(int value)
    {
        SetScore(value);
        _dilateInProgress = true;
        _dilateProgress = 0;
    }

    private void SetScore(int value)
    {
        _text.text = string.Format("{0:n0}", value);
    }

    private void Update()
    {
        if (!_dilateInProgress) return;
        if (_dilateProgress < _dilateTime)
        {
            _dilateProgress += Time.deltaTime;
            float dilate = _dilateCurve.Evaluate(Mathf.Clamp01(_dilateProgress / _dilateTime));
            _material.SetFloat(ShaderUtilities.ID_FaceDilate,Mathf.Lerp(_dilateValueDefault, _dilateValueMax, dilate));   
        }
        else
        {
            _dilateInProgress = false;
            _material.SetFloat(ShaderUtilities.ID_FaceDilate,_dilateValueDefault);   
        }
    }
}
