using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject homePanel, forYouPanel, PostAQuestionPanel, GamePanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void disableAll()
    {
        homePanel.SetActive(false);
        forYouPanel.SetActive(false);  
        GamePanel.SetActive(false);
        PostAQuestionPanel.SetActive(false);
    }
    public void enableHome()
    {
        disableAll();
        homePanel.SetActive(true);
    }
    public void enableForYou()
    {
        disableAll();
        forYouPanel.SetActive(true);
    }
    public void enablePosting()
    {
        disableAll();
        PostAQuestionPanel.SetActive(true);
    }
    public void enableGame()
    {
        disableAll();
        GamePanel.SetActive(true);
    }
}
