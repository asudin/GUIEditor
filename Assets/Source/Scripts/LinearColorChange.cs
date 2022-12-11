using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinearColorChange : MonoBehaviour
{
    [SerializeField] private Color _targetColor;
    [SerializeField] private Color _originalColor;
    [SerializeField] private float _duration;
    [field: SerializeField] private bool _isChanging { get; set; }

    private Light _target;
    private float _runningTime;

    private void Start()
    {
        _target = GetComponent<Light>();
    }

    private void Update()
    {
        StartCoroutine(TargetColorChange());
    }

    private void ChangeColors()
    {
        if (_target.color != _originalColor)
            _target.color = _originalColor;
        else
            _target.color = _targetColor;
    }

    private IEnumerator TargetColorChange()
    {
        float waitingSeconds = 10f;
        var waitingTime = new WaitForSeconds(waitingSeconds);
        while (_runningTime <= _duration)
        {
            _runningTime += Time.deltaTime;

            float normalizedRunningTime = _runningTime / _duration;

            _target.color = Color.Lerp(_originalColor, _targetColor, normalizedRunningTime);

            yield return waitingTime;
        }
        _runningTime = 0;
    }
}
