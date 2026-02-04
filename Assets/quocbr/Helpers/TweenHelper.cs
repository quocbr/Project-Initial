using System;
using DG.Tweening;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace quocbr.Helpers
{
    public class TweenHelper : MonoBehaviour
    {
        public static void DoPunch(Transform objTransform, Action onCompleteAction = null)
        {
            // Debug.Log($"TweenHelper: PopupScaleSquence - {objTransform.name}");
            objTransform.gameObject.SetActive(true);

            Vector3 originalScale = objTransform.localScale;

            Sequence seq = DOTween.Sequence();
            seq.Append(objTransform.DOScale(originalScale * 1.1f, 0.15f));
            seq.Append(objTransform.DOScale(originalScale * .9f, 0.2f));
            seq.Append(objTransform.DOScale(originalScale, 0.15f))
                .OnComplete(() =>
                {
                    onCompleteAction?.Invoke();
                });
        }

        public static void DoPunch(Transform objTransform, float targetScale, Action onCompleteAction = null)
        {
            // Debug.Log($"TweenHelper: PopupScaleSquence - {objTransform.name}");
            objTransform.gameObject.SetActive(true);

            Vector3 originalScale = Vector3.one * targetScale;
            objTransform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(objTransform.DOScale(originalScale * 1.2f, 0.3f));
            seq.Append(objTransform.DOScale(originalScale * .8f, 0.3f));
            seq.Append(objTransform.DOScale(originalScale, 0.3f))
                .OnComplete(() =>
                {
                    onCompleteAction?.Invoke();
                });
        }

        public static void PunchHide(Transform objTransform)
        {
            Vector3 originalScale = objTransform.localScale;

            Sequence seq = DOTween.Sequence();
            seq.Append(objTransform.DOScale(originalScale * 1.1f, 0.1f));
            seq.Append(objTransform.DOScale(0, 0.3f));
            seq.OnComplete(() =>
            {
                objTransform.gameObject.SetActive(false);
            });
        }
        
        public static Tween Wait1Second(Action onComplete) 
        {
            // DOTween: run `onComplete` after 1 second
            return DOVirtual.DelayedCall(1f, () => onComplete?.Invoke(), ignoreTimeScale: false);
        }
        
        public static Tween WaitSomeSeconds(float seconds, Action onComplete) 
        {
            // DOTween: run `onComplete` after `seconds`
            return DOVirtual.DelayedCall(seconds, () => onComplete?.Invoke(), ignoreTimeScale: false);
        }

        public static void PopupScaleSquence(Transform transform1, Action onCompleteAction)
        {
            throw new NotImplementedException();
        }
    }
}