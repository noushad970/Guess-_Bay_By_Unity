using UnityEngine;
using UnityEngine.UI;

public class TurnOnSpinWheel : MonoBehaviour
{
    [SerializeField] private GameObject spinSection, mainCam, mainCanvas;
    [SerializeField] private Button exitFromSpinWheelButton;
    [SerializeField] private Button gotoSpinWheelButton;
    private void Start()
    {
        exitFromSpinWheelButton.onClick.AddListener(disableSpinWheel);
        gotoSpinWheelButton.onClick.AddListener(enableSpinWheel);
    }
    private void enableSpinWheel()
    {
        spinSection.SetActive(true);
        mainCam.SetActive(false);
        mainCanvas.SetActive(false);
    }
    public void disableSpinWheel()
    {
        spinSection.SetActive(false);
        mainCam.SetActive(true);
        mainCanvas.SetActive(true);
    }
}
