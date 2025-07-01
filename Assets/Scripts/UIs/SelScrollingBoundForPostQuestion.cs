using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelScrollingBoundForPostQuestion : MonoBehaviour
{

    RectTransform rectTransform;
    [SerializeField] private Button AddAnotherQuestion;
    [SerializeField] GameObject[] inputfields;
    int primaryInputFieldIndex = 4;
    void Start()
    {
        AddAnotherQuestion.onClick.AddListener(OnClickAddInput);
        rectTransform = GetComponent<RectTransform>();

    }
    private void OnEnable()
    {
        primaryInputFieldIndex = 4;
        AddInputField(primaryInputFieldIndex);
    }
    private void OnClickAddInput()
    {
        if(primaryInputFieldIndex<inputfields.Length)
        AddInputField(++primaryInputFieldIndex);
        else
        {
            Debug.Log("Maximum input fields reached");
        }
    }
    void AddInputField(int x)
    {
        for(int i = 0; i < inputfields.Length; i++)
        {
           
                inputfields[i].SetActive(false);
                
        }
        for (int i = 0; i < x; i++)
        {
            inputfields[i].SetActive(true);
        }
    }
    void Update()
    {
        Vector3 pos = rectTransform.anchoredPosition;

        // Clamp the Y value between 0 and 1500
        pos.y = Mathf.Clamp(pos.y, -20, primaryInputFieldIndex*40);

        // Apply the clamped position back
        rectTransform.anchoredPosition = pos;
    }
}
