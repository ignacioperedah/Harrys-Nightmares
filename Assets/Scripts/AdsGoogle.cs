using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds;
using System;

public class AdsGoogle : MonoBehaviour
{
    private BannerView bannerView;
    private RewardedAd reward;

    public GameManager gameManager;

    

    // Start is called before the first frame update
    public void Start()
    {
        MobileAds.Initialize(initStatus => { });

        this.RequestReward();
        this.RequestBanner();
    }

    private void RequestBanner()
    {
        string adUnitId = "ca-app-pub-6915944261825859/3981445176";

        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        AdRequest request = new AdRequest.Builder().Build();
        this.bannerView.LoadAd(request);
    }

    private void RequestReward()
    {
        string adUnitId = "ca-app-pub-6915944261825859/5375011766";


        this.reward = new RewardedAd(adUnitId);

        this.reward.OnAdClosed += HandleRewardedAdClosed;

        this.reward.OnUserEarnedReward += HandleUserEarnedReward;

        this.reward.OnAdOpening += HandleRewardedAdOpening;

        AdRequest request = new AdRequest.Builder().Build();
        this.reward.LoadAd(request);

    }

    public void UserChoseToWatchAd()
    {
        if (this.reward.IsLoaded())
        {
            this.reward.Show();
        }
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        this.RequestReward();
        gameManager.Resume();

    }
    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        gameManager.Pause();
    }

    public void HandleUserEarnedReward(object sender, Reward args2)
    {
        gameManager.VideoReward();
    }
}
