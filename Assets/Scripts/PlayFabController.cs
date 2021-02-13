using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabController : MonoBehaviour
{
    // --------
    // PlayFab
    // --------
    public static string PlacementId { get; private set; } = "";
    public static string RewardId { get; private set; } = "";
    public static int? PlacementViewsRemaining { get; private set; } = null;
    public static double? PlacementViewsResetMinutes { get; private set; } = null;

    // --------
    // Event
    // --------
    public delegate void RewardFinishedEvent();
    public static event RewardFinishedEvent OnRewardFinished;
    public delegate void LoginSuccessEvent(LoginResult success);
    public static event LoginSuccessEvent OnLoginSuccess;

    public static PlayFabController Instance;

    private void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        PlayFabSettings.staticSettings.TitleId = "A80F6";

        var request = new LoginWithCustomIDRequest
        {
            CustomId = "GettingStartedGuide",
            CreateAccount = true,
        };
        var result = await PlayFabClientAPI.LoginWithCustomIDAsync(request);

        if(result.Error != null)
        {
            Debug.Log(result.Error.GenerateErrorReport());
        }
        else
        {
            OnLoginSuccess?.Invoke(result.Result);
        }
    }

    /// <summary>
    /// 広告配置の取得
    /// </summary>
    /// <param name="gameId">Unity Dashboard で確認</param>
    /// <param name="placementName">Unity Dashboard で確認</param>
    public async void GetAdPlacementsAsync(string gameId, string placementName)
    {
        var request = new GetAdPlacementsRequest { AppId = gameId };
        var result = await PlayFabClientAPI.GetAdPlacementsAsync(request);

        if (result.Error != null)
        {
            Debug.Log(result.Error.GenerateErrorReport());
        }
        else
        {
            var placement = result.Result.AdPlacements.Find(x => x.PlacementName == placementName);
            PlacementId = placement.PlacementId;
            RewardId = placement.RewardId;
            PlacementViewsRemaining = placement.PlacementViewsRemaining;
            PlacementViewsResetMinutes = placement.PlacementViewsResetMinutes;

            Debug.Log("GetAdPlacements Success!!");
            Debug.Log("PlacementName:" + placement.PlacementName);
            Debug.Log("RewardName:" + placement.RewardName);
            Debug.Log("PlacementViewsRemaining:" + placement.PlacementViewsRemaining);
            Debug.Log("PlacementViewsResetMinutes:" + placement.PlacementViewsResetMinutes);
        }
    }

    /// <summary>
    /// アクティビティの報告
    /// </summary>
    /// <param name="activity">PlayFabのアクティビティ</param>
    public async void ReportAdActivity(AdActivity activity)
    {
        var request = new ReportAdActivityRequest
        {
            PlacementId = PlacementId,
            RewardId = RewardId,
            Activity = activity
        };

        var result = await PlayFabClientAPI.ReportAdActivityAsync(request);
        if (result.Error != null)
        {
            Debug.Log(result.Error.GenerateErrorReport());
        }
        else
        {
            if (activity == AdActivity.End)
                RewardAdActivityAsync();
        }
    }
    /// <summary>
    /// 報酬の付与
    /// </summary>
    public async void RewardAdActivityAsync()
    {
        var request = new RewardAdActivityRequest
        {
            PlacementId = PlacementId,
            RewardId = RewardId
        };
        var result = await PlayFabClientAPI.RewardAdActivityAsync(request);
        if (result.Error != null)
        {
            if (result.Error.Error == PlayFabErrorCode.AllAdPlacementViewsAlreadyConsumed)
                Debug.Log("You have exceeded the viewing limit for video ads.");
            Debug.Log(result.Error.GenerateErrorReport());
        }
        else
        {
            Debug.Log("GrantedVirtualCurrencies:" + result.Result.RewardResults.GrantedVirtualCurrencies["MS"]);
            OnRewardFinished?.Invoke();
        }
    }
}
