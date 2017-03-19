﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace UWPHelper.Utilities
{
    // When you change something here, change it even it the ViewSpecificBindableClassBase down there
    public abstract class ViewSpecificClassBase<T> where T : class, new()
    {
        private static readonly Dictionary<int, T> instances = new Dictionary<int, T>();

        private static object locker;

        protected ViewSpecificClassBase()
        {
            if (this is INotifyPropertyChanged iNotifyPropertyChanged)
            {
                locker = new object();

                iNotifyPropertyChanged.PropertyChanged += async (sender, e) =>
                {
                    bool lockTaken = false;

                    try
                    {
                        Monitor.TryEnter(locker, 0, ref lockTaken);

                        if (lockTaken)
                        {
                            int callerViewId = ViewHelper.GetCurrentViewId();

                            await ViewHelper.RunOnEachViewDispatcherAsync(() =>
                            {
                                int currentViewId = ViewHelper.GetCurrentViewId();

                                if (currentViewId != callerViewId && instances.ContainsKey(currentViewId))
                                {
                                    T currentViewInstance = instances[currentViewId];
                                    PropertyInfo changedProperty = typeof(T).GetRuntimeProperty(e.PropertyName);

                                    if (changedProperty.CanRead && changedProperty.CanWrite)
                                    {
                                        changedProperty.SetValue(currentViewInstance, changedProperty.GetValue(this));
                                    }
                                }
                            });
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(locker);
                        }
                    }
                };
            }
        }

        protected static bool GetForCurrentViewIfExists(out T obj)
        {
            int currentViewId = ViewHelper.GetCurrentViewId();

            if (instances.ContainsKey(currentViewId))
            {
                obj = instances[currentViewId];
                return true;
            }

            obj = null;
            return false;
        }

        protected static T GetForCurrentView()
        {
            int currentViewId = ViewHelper.GetCurrentViewId();

            if (!instances.ContainsKey(currentViewId))
            {
                instances.Add(currentViewId, new T());
            }

            return instances[currentViewId];
        }
    }

    public abstract class ViewSpecificBindableClassBase<T> : NotifyPropertyChangedBase where T : class, new()
    {
        private static readonly Dictionary<int, T> instances = new Dictionary<int, T>();

        private static object locker;

        protected ViewSpecificBindableClassBase()
        {
            locker = new object();

            PropertyChanged += async (sender, e) =>
            {
                bool lockTaken = false;

                try
                {
                    Monitor.TryEnter(locker, 0, ref lockTaken);

                    if (lockTaken)
                    {
                        int callerViewId = ViewHelper.GetCurrentViewId();

                        await ViewHelper.RunOnEachViewDispatcherAsync(() =>
                        {
                            int currentViewId = ViewHelper.GetCurrentViewId();

                            if (currentViewId != callerViewId && instances.ContainsKey(currentViewId))
                            {
                                T currentViewInstance = instances[currentViewId];
                                PropertyInfo changedProperty = typeof(T).GetRuntimeProperty(e.PropertyName);

                                if (changedProperty.CanRead && changedProperty.CanWrite)
                                {
                                    changedProperty.SetValue(currentViewInstance, changedProperty.GetValue(this));
                                }
                            }
                        });
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(locker);
                    }
                }
            };
        }

        protected static bool GetForCurrentViewIfExists(out T obj)
        {
            int currentViewId = ViewHelper.GetCurrentViewId();

            if (instances.ContainsKey(currentViewId))
            {
                obj = instances[currentViewId];
                return true;
            }

            obj = null;
            return false;
        }

        protected static T GetForCurrentView()
        {
            int currentViewId = ViewHelper.GetCurrentViewId();

            if (!instances.ContainsKey(currentViewId))
            {
                instances.Add(currentViewId, new T());
            }

            return instances[currentViewId];
        }
    }
}