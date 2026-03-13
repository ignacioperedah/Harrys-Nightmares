using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
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

        // Destruir banner anterior si existe
        if (this.bannerView != null)
        {
            this.bannerView.Destroy();
        }

        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        AdRequest request = new AdRequest();
        this.bannerView.LoadAd(request);
    }

    private void RequestReward()
    {
        string adUnitId = "ca-app-pub-6915944261825859/5375011766";

        // Limpiar anuncio anterior antes de cargar uno nuevo
        if (this.reward != null)
        {
            this.reward.Destroy();
            this.reward = null;
        }

        AdRequest request = new AdRequest();

        // En las versiones nuevas, se usa el método estático Load con un callback
        RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                return;
            }

            this.reward = ad;

            // Asignar los eventos actualizados al anuncio ya cargado
            this.reward.OnAdFullScreenContentClosed += HandleRewardedAdClosed;
            this.reward.OnAdFullScreenContentOpened += HandleRewardedAdOpening;
        });
    }

    public void UserChoseToWatchAd()
    {
        // IsLoaded() fue reemplazado por CanShowAd()
        if (this.reward != null && this.reward.CanShowAd())
        {
            this.reward.Show((Reward rewardAmount) =>
            {
                // Callback principal al obtener la recompensa
                HandleUserEarnedReward();
            });
        }
    }

    public void HandleRewardedAdClosed()
    {
        this.RequestReward(); // Precargar el siguiente anuncio
        gameManager.Resume();
    }

    public void HandleRewardedAdOpening()
    {
        gameManager.Pause();
    }

    public void HandleUserEarnedReward()
    {
        gameManager.VideoReward();
    }
}
