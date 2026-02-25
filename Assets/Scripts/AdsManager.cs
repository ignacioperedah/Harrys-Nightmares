using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;


public class AdsManager : MonoBehaviour
{
    
    string gameId = "4438521";
    public string surfacingId = "Banner_Android";
    public string rewardVideo = "Rewarded_Android";
    bool testMode = true;
    public bool rewardOn = false;

    // Start is called before the first frame update
    public void Start()
    {
        Advertisement.Initialize(gameId, testMode);
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        StartCoroutine(ShowBannerWhenInitialized());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Advertisement.isShowing)
        {
            rewardOn = true;
        }
        else
        {
            rewardOn = false;
        }
    }

    public void hideBanner()
    {
        Advertisement.Banner.Hide();
    }

    public void showBanner()
    {
        Advertisement.Banner.Show(surfacingId);
    }

    IEnumerator ShowBannerWhenInitialized()
    {
        while (!Advertisement.isInitialized)
        {
            yield return new WaitForSeconds(0.5f);
        }
        Advertisement.Banner.Show(surfacingId);

    }

    public void ShowRewardedAd()
    {
        rewardOn = true;
        Advertisement.Show(rewardVideo);
    }
}