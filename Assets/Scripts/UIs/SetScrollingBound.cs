using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SetScrollingBound : MonoBehaviour
{
    [SerializeField] private float minScrollBound, ContentSize;
    RectTransform rectTransform;
    [SerializeField] private QuestionManager questionManager;
   
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
           
    }

    void Update()
    {
        Vector3 pos = rectTransform.anchoredPosition;

        // Clamp the Y value between 0 and 1500
        pos.y = Mathf.Clamp(pos.y, minScrollBound, ContentSize*questionManager.getAnswersCount());

        // Apply the clamped position back
        rectTransform.anchoredPosition = pos;
    }
}
