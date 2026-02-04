using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PopupPlugin
{
    public class PopupManagerAbi : MonoBehaviour
    {
        private static PopupManagerAbi mInstance;

        public Canvas canvas;
        public bool usingDefaultTransparent = true;
        public Image transparent;
        public Transform parent;
        private int defaultSortingOrder;
        private bool hasPopupShowing;
        private Transform mTransparentTrans;
        private Queue<BasePopup> popupQueue = new Queue<BasePopup>();
        public Stack<BasePopup> popupStacks = new Stack<BasePopup>();

        public static PopupManagerAbi Ins =>
            //if (mInstance == null) {
            //	mInstance = LoadResource<PopupManagerAbi> ("PopupManager");
            //}
            mInstance;

        private void Awake()
        {
            mInstance = this;
            mTransparentTrans = transparent.transform;
            defaultSortingOrder = canvas.sortingOrder;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            EvtPopupClose += HandlePopupClose;
            //GlobalEventManager.Instance.SceneChanged += HandleChangeScene;
            hasPopupShowing = false;
        }

        private void OnDestroy()
        {
            EvtPopupClose -= HandlePopupClose;
        }

        public static T CreateNewInstance<T>()
        {
            T result = Ins.CheckInstancePopupPrebab<T>();
            return result;
        }

        public T CheckInstancePopupPrebab<T>()
        {
            Type type = typeof(T);
            GameObject go = Instantiate(Resources.Load("Popup/" + type.Name), parent) as GameObject;
            T result = go.GetComponent<T>();
            return result;
        }

        private bool IsOfType<T>(object value)
        {
            return value is T;
        }

        public void ChangeTransparentOrder(Transform topPopupTransform, bool active)
        {
            if (active)
            {
                mTransparentTrans.SetSiblingIndex(topPopupTransform.GetSiblingIndex() - 1);
                transparent.gameObject.SetActive(true && usingDefaultTransparent);
                hasPopupShowing = true;
            }
            else
            {
                if (parent.childCount >= 2)
                {
                    mTransparentTrans.SetSiblingIndex(parent.childCount - 2);
                    hasPopupShowing = true;
                }
                else
                {
                    transparent.gameObject.SetActive(false);
                    hasPopupShowing = false;
                }
            }
            //Debug.Log("hasPopupShowing: "+ hasPopupShowing);
        }

        public PopupManagerAbi Preload()
        {
            return mInstance;
        }

        public bool SequenceHidePopup()
        {
            if (popupStacks.Count > 0)
                popupStacks.Peek().Hide();
            else
            {
                transparent.gameObject.SetActive(false);
                hasPopupShowing = false;
            }

            return popupStacks.Count > 0;
        }

        public void CloseAllPopup()
        {
            for (int i = 0; i < popupStacks.Count; i++)
            {
                BasePopup popup = popupStacks.Peek();
                if (popup != null)
                    popup.Hide();
            }

            transparent.gameObject.SetActive(false);
        }

        public static T LoadResource<T>(string name)
        {
            GameObject go = (GameObject)Instantiate(Resources.Load(name));
            go.name = string.Format("[{0}]", name);
            DontDestroyOnLoad(go);
            return go.GetComponent<T>();
        }

        public void SetSortingOrder(int order)
        {
            canvas.sortingOrder = order;
        }

        public void ResetOrder()
        {
            canvas.sortingOrder = defaultSortingOrder;
        }

        public void OderPopup(BasePopup popup)
        {
            if (!hasPopupShowing)
            {
                popup.ActivePopup();
            }
            else
            {
                popup.gameObject.SetActive(false);
                popupQueue.Enqueue(popup);
            }
        }

        #region Popup Events

        public delegate void PopupEvent(BasePopup popup);

        public event PopupEvent EvtPopupOpen;
        public event PopupEvent EvtPopupClose;

        #endregion

        #region Event Methods

        public void OnPopupOpen(BasePopup popup)
        {
            EvtPopupOpen?.Invoke(popup);
        }

        public void OnPopupClose(BasePopup popup)
        {
            EvtPopupClose?.Invoke(popup);
        }

        #endregion

        #region Handle Events

        private void HandlePopupClose(BasePopup popup)
        {
            if (popupQueue.Count > 0)
            {
                BasePopup nextPopup = popupQueue.Dequeue();
                nextPopup.gameObject.SetActive(true);
                nextPopup.ActivePopup();
            }
        }

        private void HandleChangeScene(string scene)
        {
            canvas.worldCamera = Camera.main;
        }

        #endregion
    }
}