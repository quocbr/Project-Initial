using System;
using UnityEngine;

public abstract class BoosterBase
{

    public Action OnStateChanged;
    private bool isBusy;

    protected bool isDirty = true;

    public bool IsBusy
    {
        get => isBusy;
        protected set
        {
            isBusy = value;
            isDirty = true;
            OnStateChanged?.Invoke();
        }
    }

    public bool IsDirty => isDirty;

    public void InitialiseSettings( /*PUSettings settings*/)
    {
        //this.settings = settings;
    }

    public abstract void Init();

    public abstract bool Activate();

    public virtual string GetFloatingMessage()
    {
        //return settings.FloatingMessage;
        return "";
    }

    public virtual void ResetBehavior()
    {
    }

    public void SetDirty()
    {
        isDirty = true;
        OnStateChanged?.Invoke();
    }

    public void OnRedrawn()
    {
        isDirty = false;
    }

    protected BoosterInfo info;

    public void SetData(BoosterInfo info)
    {
        this.info = info;
    }
}