using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyS7NetPlus.Common.Tools
{
    public class MyMessageBus
    {
        static readonly object _locker = new object();
        static ConcurrentDictionary<string, List<IMyMessageCallback>> _eventDictionary = new();

        public static void Publish<T>(string eventName, T message)
        {
            List<IMyMessageCallback> myMessageCallbackList = null;

            lock (_locker)
            {
                if (_eventDictionary.TryGetValue(eventName, out var list))
                {
                    myMessageCallbackList = new(list);
                }

            }

            if (myMessageCallbackList != null)
            {
                foreach (MyMessageCallback<T> myMessageCallback in myMessageCallbackList)
                {
                    if (myMessageCallback.SynchronizationContext != null)
                    {
                        myMessageCallback.SynchronizationContext.Post(state => myMessageCallback.Callback.Invoke((T)state), message);
                    }
                    else
                    {
                        myMessageCallback.Callback.Invoke(message);
                    }
                }
            }
            //else
            //{
            //    throw new Exception($"Publish失败，event字典中不存在key为{eventName}的数据");
            //}
        }

        public static void Subscribe<T>(string eventName, string source, Action<T> callback)
        {
            lock (_locker)
            {
                if (_eventDictionary.TryGetValue(eventName, out var list))
                {
                    // 已经存在这个event的KeyValuePair则取出value来List.Add
                    list.Add(new MyMessageCallback<T>()
                    {
                        Source = source,
                        SynchronizationContext = SynchronizationContext.Current,
                        Callback = callback
                    });
                }
                else
                {
                    // 不存在这个event的KeyValuePair，则直接在字典add KeyValuePair
                    _eventDictionary.TryAdd(eventName, new()
                    {
                        new MyMessageCallback<T>()
                        {
                            Source = source,
                            SynchronizationContext = SynchronizationContext.Current,
                            Callback = callback
                        }
                    });
                }
            }
        }

        public static void Unsubscribe(string eventName, string source)
        {
            lock (_locker)
            {
                if (_eventDictionary.TryGetValue(eventName, out var list))
                {
                    list.Where(cb => cb.Source == source)?.ToList()?.ForEach(myMessageCallback => list.Remove(myMessageCallback));

                    if (list.Count == 0)
                    {
                        _eventDictionary.TryRemove(eventName, out list);
                    }
                }
                //else
                //{
                //    throw new Exception($"Unsubscribe失败，event字典中不存在key为{eventName}的数据");
                //}
            }
        }
    }
}
