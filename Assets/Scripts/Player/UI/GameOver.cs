using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _gameOverFadeInTime;
    [SerializeField] private AnimationCurve _fadeCurve;

    private void OnEnable()
    {
        Ship.OnGameEnd += OnGameEnd;
    }

    private void OnDisable()
    {
        Ship.OnGameEnd -= OnGameEnd;
    }
    
    private void OnGameEnd()
    {
        StopAllCoroutines();
        StartCoroutine(GameEndCo());
    }

    private IEnumerator GameEndCo()
    {
        float fadeTime = 0;

        yield return new WaitForSeconds(1);

        while (fadeTime < _gameOverFadeInTime)
        {
            fadeTime += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0, 1, _fadeCurve.Evaluate(fadeTime / _gameOverFadeInTime));
            yield return null;
        }
        
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1;
    }
}
