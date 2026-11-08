using System.Collections;
using UnityEngine;
#if UNITY_ANDROID
using Google.Play.Review;
#elif UNITY_IPHONE
using UnityEngine.iOS;
#endif
public class AppRating : MonoBehaviour
{
#if UNITY_ANDROID
    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo;
    private Coroutine _coroutine;

    private void Start()
    {
        _coroutine = StartCoroutine(InitReview());
    }

    public void RateAndReview()
    {
        Debug.Log("Open Rate and review1");
        StartCoroutine(LaunchReview());
    }

    private IEnumerator InitReview(bool force = false)
    {
        if (_reviewManager == null) _reviewManager = new ReviewManager();

        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            if (force) DirectlyOpen();
            yield break;
        }

        _playReviewInfo = requestFlowOperation.GetResult();
    }

    public IEnumerator LaunchReview()
    {
        Debug.Log("Open Rate and review2");
        if (_playReviewInfo == null)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            yield return StartCoroutine(InitReview(true));
        }

        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null;
        VariableSystem.IsRateDone = true;
        //SettingControll.instance.RemoveRateButton();
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            DirectlyOpen();
            yield break;
        }
        //UIManager.Ins.CloseUI<RateUI>();
        Debug.Log("Open Rate and review3");
    }

    private void DirectlyOpen()
    {
        Debug.Log("Open Rate and review4");
        Application.OpenURL("http://play.google.com/store/apps/details?id=" + Application.identifier);
        //Application.OpenURL("https://play.google.com/store/apps/details?id=com.avatarlife.avatarstory.mystory.myworld.mywonder");
    }
#elif UNITY_IPHONE

    public void RateAndReview()
    {
        VariableSystem.IsRateDone = true;
        Device.RequestStoreReview();
    }
#endif
}
