using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject homePanel, forYouPanel, PostAQuestionPanel, GamePanel,appPurchaseStore;
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
        appPurchaseStore.SetActive(false);
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
    public void enableAppPurchaseStore()
    {
        disableAll();
        appPurchaseStore.SetActive(true);

    }
    public void enableCommunityPage()
    {
        Debug.Log("Community Page is not implemented yet.");
    }
    public void enableContactPage()
    {
        Debug.Log("Contact Page is not implemented yet.");
    }
}
