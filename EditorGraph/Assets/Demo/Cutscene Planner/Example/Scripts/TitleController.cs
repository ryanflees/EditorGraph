using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class TitleController : MonoBehaviour
{
    public Text titleText;
    public CanvasGroup titleCanvasGroup { get { return GetComponent<CanvasGroup>(); } }

    private float _targetAlpha;
    private bool _inProgress;
    private float _speed;
    // Use this for initialization
    void Start()
    {
        titleCanvasGroup.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (_inProgress)
        {
            if (titleCanvasGroup.alpha < _targetAlpha)
            {
                titleCanvasGroup.alpha += Time.deltaTime * _speed;
                if (titleCanvasGroup.alpha >= _targetAlpha)
                {
                    titleCanvasGroup.alpha = _targetAlpha;
                    _inProgress = false;
                }
            }
            else if (titleCanvasGroup.alpha > _targetAlpha)
            {
                titleCanvasGroup.alpha -= Time.deltaTime * _speed;
                if (titleCanvasGroup.alpha <= _targetAlpha)
                {
                    titleCanvasGroup.alpha = _targetAlpha;
                    _inProgress = false;
                }
            }
        }
    }

    public void Show(float fadeTime, string text)
    {
        titleCanvasGroup.alpha = 0;
        _targetAlpha = 1;
        titleText.text = text;

        if (fadeTime == 0)
        {
            titleCanvasGroup.alpha = _targetAlpha;
            _inProgress = false;
        }
        else
        {
            _speed = 1f / fadeTime;
            _inProgress = true;
        }
        
        
    }
    public void Hide(float fadeTime)
    {
        titleCanvasGroup.alpha = 1;
        _targetAlpha = 0;

        if (fadeTime == 0)
        {
            titleCanvasGroup.alpha = _targetAlpha;
            _inProgress = false;
        }
        else
        {
            _speed = 1f / fadeTime;
            _inProgress = true;
        }
    }
}
