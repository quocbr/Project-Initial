using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PopupPlugin
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class BasePopup : MonoBehaviour
    {
        /// <summary>
        /// Optional Animator show popup controller
        /// </summary>
        [HideInInspector] public Animator animator;

        /// <summary>
        /// Optional animation show
        /// </summary>
        public AnimationClip showAnimationClip;

        /// <summary>
        /// Optional animation hide
        /// </summary>
        public AnimationClip hideAnimationClip;

        private Action hideCompletedCallback;

        private Action InitAction;

        protected bool isShowed;
        private int mSortOrder;
        private Transform mTransform;
        private bool overlay;
        private Stack<BasePopup> refStacks;
        private Action showCompletedCallback;

        public bool IsShowed => isShowed;

        public virtual void Awake()
        {
            isShowed = false;
            animator = GetComponent<Animator>();
            mTransform = transform;
            mSortOrder = mTransform.GetSiblingIndex();
            refStacks = PopupManagerAbi.Ins.popupStacks;
            if (animator == null || showAnimationClip == null || hideAnimationClip == null)
            {
                //BPDebug.LogMessage("Chưa gán Animator hoặc showAnimationClip, hideAnimationClip  cho popup " + GetType().ToString(), true);
            }
        }

        protected void Show(Action showCompletedCallback = null, bool overlay = true,
            Action hideCompletedCallback = null)
        {
            //GlobalEventManager.Instance.OnPopupOpen(this);
            //Trường hợp chỉ tạo popup duy nhất.
            if (isShowed)
            {
                Reshow();
                int topSortOrder = refStacks.Peek().SortOrder();
                //Nếu đã bị các popup khác đè lên
                if (refStacks.Count > 1 && SortOrder() != topSortOrder)
                {
                    // Đẩy popup này lên trên cùng và sắp xếp lại sortOrder cho toàn stack
                    MoveElementToTopStack(ref refStacks, SortOrder());
                }

                return;
            }

            this.showCompletedCallback = showCompletedCallback;
            this.hideCompletedCallback = hideCompletedCallback;

            float waitLastPopupHide = 0;
            this.overlay = overlay;
            isShowed = true;

            if (!overlay && refStacks.Count > 0)
                ForceHideAllCurrent(ref waitLastPopupHide);

            if (!refStacks.Contains(this))
                refStacks.Push(this);

            if (refStacks.Count > 0)
                ChangeSortOrder(refStacks.Peek().SortOrder() + 1);

            if (waitLastPopupHide != 0)
                StartCoroutine(RunMethod(waitLastPopupHide, AnimateShow));
            else
                AnimateShow();

            PopupManagerAbi.Ins.OnPopupOpen(this);
        }

        protected void Queue(Action action)
        {
            InitAction = action;
            PopupManagerAbi.Ins.OderPopup(this);
        }

        public void ActivePopup()
        {
            InitAction?.Invoke();
            Show();
        }

        public void Reshow()
        {
            if (animator != null && showAnimationClip != null)
            {
                animator.Play(showAnimationClip.name, -1, 0.0f);
                float showAnimationDuration = GetAnimationClipDuration(showAnimationClip);
                StartCoroutine(RunMethod(showAnimationDuration, OnShowFinish));
            }

            PopupManagerAbi.Ins.ChangeTransparentOrder(mTransform, true);
        }

        public virtual void Hide()
        {
            //GlobalEventManager.Instance.OnPopupClose(this);           
            if (!isShowed)
                return;
            isShowed = false;
            AnimateHide();
        }

        public void Hide(Action hideCompletedCallback)
        {
            //GlobalEventManager.Instance.OnPopupClose(this);
            this.hideCompletedCallback = hideCompletedCallback;
            if (!isShowed)
                return;
            isShowed = false;
            AnimateHide();
        }

        public void OnCloseClick()
        {
            Hide();
        }


        private IEnumerator RunMethod(float delay, Action action)
        {
            yield return new WaitForSecondsRealtime(delay);
            action();
        }

        private void AnimateShow()
        {
            if (animator != null && showAnimationClip != null)
            {
                float showAnimationDuration = GetAnimationClipDuration(showAnimationClip);
                StartCoroutine(RunMethod(showAnimationDuration, OnShowFinish));
                animator.Play(showAnimationClip.name);
            }
            else
            {
                OnShowFinish();
            }

            PopupManagerAbi.Ins.ChangeTransparentOrder(mTransform, true);
        }

        private void OnShowFinish()
        {
            showCompletedCallback?.Invoke();
        }

        private void AnimateHide()
        {
            if (animator != null && hideAnimationClip != null)
            {
                animator.Play(hideAnimationClip.name);
                float hideAnimationDuration = GetAnimationClipDuration(hideAnimationClip);
                StartCoroutine(RunMethod(hideAnimationDuration, Destroy));
            }
            else
            {
                Destroy();
            }

            PopupManagerAbi.Ins.OnPopupClose(this);
        }

        private void Destroy()
        {
            if (refStacks.Contains(this))
                refStacks.Pop();

            if (gameObject.activeSelf)
                DestroyImmediate(gameObject);

            hideCompletedCallback?.Invoke();
            PopupManagerAbi.Ins.ChangeTransparentOrder(mTransform, false);
            PopupManagerAbi.Ins.ResetOrder();
        }

        public int SortOrder()
        {
            return mSortOrder;
        }

        public void ChangeSortOrder(int newSortOrder = -1)
        {
            if (newSortOrder != -1)
            {
                mTransform.SetSiblingIndex(newSortOrder);
                mSortOrder = newSortOrder;
            }
        }

        private void ForceHideAllCurrent(ref float waitTime)
        {
            while (refStacks.Count > 0)
            {
                BasePopup bp = refStacks.Pop();
                waitTime += bp.GetAnimationClipDuration(hideAnimationClip);
                bp.Hide();
            }
        }

        private float GetAnimationClipDuration(AnimationClip clip)
        {
            if (animator != null && clip != null)
            {
                RuntimeAnimatorController rac = animator.runtimeAnimatorController;
                for (int i = 0; i < rac.animationClips.Length; i++)
                {
                    if (rac.animationClips[i].Equals(clip))
                        return rac.animationClips[i].length;
                }
            }

            return 0;
        }

        private void MoveElementToTopStack(ref Stack<BasePopup> stack, int order)
        {
            Stack<BasePopup> tempStack = new Stack<BasePopup>();
            BasePopup foundPopup = null;
            int minSortOrder = 0;
            while (refStacks.Count > 0)
            {
                BasePopup bp = refStacks.Pop();
                if (bp.SortOrder() != order)
                {
                    tempStack.Push(bp);
                    minSortOrder = bp.SortOrder();
                }
                else
                {
                    foundPopup = bp;
                }
            }

            while (tempStack.Count > 0)
            {
                BasePopup bp = tempStack.Pop();
                bp.ChangeSortOrder(minSortOrder++);
                stack.Push(bp);
            }

            if (foundPopup != null)
            {
                foundPopup.ChangeSortOrder(minSortOrder);
                stack.Push(foundPopup);
            }
        }
    }
}