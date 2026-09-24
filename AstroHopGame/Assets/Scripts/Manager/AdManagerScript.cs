using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdManagerScript : MonoBehaviour
{
    #region Singleton
    public static AdManagerScript instance;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Ads should persist across scene loads (menu -> game)
        }
    }
    #endregion

    #region Ad Unit IDs
    // TEST ID - Google's official test rewarded ad unit for Android.
    // Always test with this before switching to your real Ad Unit ID.
    private const string reviveAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    #endregion

    #region State
    private RewardedAd rewardedAd;
    public bool IsRewardedAdReady { get; private set; }
    #endregion

    #region Initialization
    private void Start()
    {
        InitializeAds();
    }

    private void InitializeAds()
    {
        MobileAds.Initialize(initStatus =>
        {
            if (initStatus == null)
            {
                Debug.LogError("Google Mobile Ads initialization failed.");
                return;
            }

            Debug.Log("Google Mobile Ads initialization complete.");

            // Load the first rewarded ad as soon as the SDK is ready
            LoadRewardedAd();
        });
    }
    #endregion

    #region Loading
    public void LoadRewardedAd()
    {
        // Clean up any previous ad instance before loading a new one
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        IsRewardedAdReady = false;

        Debug.Log("Loading rewarded ad...");

        var adRequest = new AdRequest();

        RewardedAd.Load(reviveAdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load: " + error);
                IsRewardedAdReady = false;
                return;
            }

            Debug.Log("Rewarded ad loaded successfully.");
            rewardedAd = ad;
            IsRewardedAdReady = true;

            RegisterEventHandlers(rewardedAd);
        });
    }
    #endregion

    #region Showing
    // Live check: don't trust a cached bool, since a loaded ad can expire in the background
    // (Google rewarded ads expire roughly 1 hour after loading if never shown).
    public bool IsAdActuallyReady()
    {
        bool ready = rewardedAd != null && rewardedAd.CanShowAd();

        Debug.Log($"[Revive] rewardedAd null? {rewardedAd == null} | CanShowAd: {(rewardedAd != null ? rewardedAd.CanShowAd().ToString() : "N/A")}");


        // Keep the cached flag in sync in case it drifted (e.g. ad expired silently)
        if (!ready && IsRewardedAdReady)
        {
            IsRewardedAdReady = false;
            LoadRewardedAd(); // Expired/unusable - fetch a fresh one right away
        }

        return ready;
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (IsAdActuallyReady())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Type}, amount: {reward.Amount}");
                onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("Tried to show rewarded ad, but it wasn't ready.");
        }
    }
    #endregion

    #region Event Handlers
    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Ad closed (whether or not the reward was earned) -> preload the next one
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad closed.");
            LoadRewardedAd();
        };

        // Ad failed to show (rare, but handle it) -> also try to preload a fresh one
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to show: " + error);
            LoadRewardedAd();
        };
    }
    #endregion
}