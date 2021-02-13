using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using PlayFab.ClientModels;

[RequireComponent(typeof(Button))]
public class RewardedAdsButton : MonoBehaviour, IUnityAdsListener
{

#if UNITY_IOS
    private string gameId = "3588250";
#elif UNITY_ANDROID
    private string gameId = "1486550";
#endif

    [SerializeField] Button reloadButton;
    Button rewardButton;
    Text buttonText;

    void Start()
    {
        rewardButton = GetComponent<Button>();
        buttonText = GetComponentInChildren<Text>();

        // Map the ShowRewardedVideo function to the button’s click listener:
        if (rewardButton) rewardButton.onClick.AddListener(ShowRewardedVideo);
        if (reloadButton) reloadButton.onClick.AddListener(PlayFabController_OnRewardFinished);

        // Initialize the Ads listener and service:
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, true);
    }

    private void Update()
    {
        if (PlayFabController.PlacementViewsRemaining == null
            || PlayFabController.PlacementViewsResetMinutes == null
            || PlayFabController.PlacementViewsRemaining > 0
            || PlayFabController.PlacementViewsResetMinutes <= 0)
        {
            buttonText.text = "Get Reward!!";
            rewardButton.interactable = Advertisement.IsReady(Constants.REWARD_PLACEMENT_ID);
        }
        else
        {
            buttonText.text = string.Format("Next : {0} minutes", PlayFabController.PlacementViewsResetMinutes.ToString());
            rewardButton.interactable = false;
        }
    }

    // --------
    // Implement
    // --------
    void ShowRewardedVideo()
    {
        Advertisement.Show(Constants.REWARD_PLACEMENT_ID);
    }

    // Implement IUnityAdsListener interface methods:
    public void OnUnityAdsReady(string placementId)
    {
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished)
        {
            Debug.Log("Reward Finish!!");
            // Reward the user for watching the ad to completion.
            PlayFabController.Instance.ReportAdActivity(AdActivity.End);
        }
        else if (showResult == ShowResult.Skipped)
        {
            Debug.Log("Reward Skipped...");
            // Do not reward the user for skipping the ad.
            PlayFabController.Instance.ReportAdActivity(AdActivity.Closed);
        }
        else if (showResult == ShowResult.Failed)
        {
            Debug.Log("Reward Failed...");
            PlayFabController.Instance.ReportAdActivity(AdActivity.Closed);
        }
    }

    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        Debug.Log("Reward Start!!");
        // Optional actions to take when the end-users triggers an ad.
        PlayFabController.Instance.ReportAdActivity(AdActivity.Start);
    }


    // --------
    // Event
    // --------
    private void PlayFabController_OnLoginSuccess(LoginResult success)
    {
        Debug.Log("Login Success!!");
        PlayFabController.Instance.GetAdPlacementsAsync(gameId, Constants.REWARD_PLACEMENT_NAME);
    }

    private void PlayFabController_OnRewardFinished()
    {
        // Get the latest placement every time you reward
        PlayFabController.Instance.GetAdPlacementsAsync(gameId, Constants.REWARD_PLACEMENT_NAME);
    }

    private void OnEnable()
    {
        // Add login success event
        PlayFabController.OnLoginSuccess += PlayFabController_OnLoginSuccess;
        PlayFabController.OnRewardFinished += PlayFabController_OnRewardFinished;
    }

    private void OnDisable()
    {
        // Remove login success event
        PlayFabController.OnLoginSuccess -= PlayFabController_OnLoginSuccess;
        PlayFabController.OnRewardFinished -= PlayFabController_OnRewardFinished;
    }
}
