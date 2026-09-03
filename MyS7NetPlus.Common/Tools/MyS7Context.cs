using MyS7NetPlus.Common.Communications;
using MyS7NetPlus.Common.DataAcquisitions;
using NLog;
using S7.Net;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reflection;

namespace MyS7NetPlus.Common.Tools
{
    //public class MyS7Context : IDisposable, INotifyPropertyChanged    //不推荐这种方式，winform UI的已入库label会卡，不流畅，且关闭winform时，已入库label和已采集label瞬间不动了了，体验没CollectedTagLogCountChanged和PersistedTagLogCountChanged事件流畅
    public class MyS7Context : IDisposable
    {
        private bool disposedValue;

        string _name;
        MyLogger _myLogger;
        SynchronizationContext _uiContext;
        MyDevice _myDevice;
        ConcurrentQueue<MyS7Task> _sendQueue;
        ConcurrentQueue<MyPersistance> _persistQueue;
        Plc _plc;
        CancellationTokenSource _cts;
        Task _collectTask;
        Task _sendTask;

        bool _isDeviceOffline = false;
        int _maxErrorTimes = 5;
        int _currentErrorTimes = 0;
        int _dataAcquisitionInterval = 100;
        int _readBytesTimeout = 2000;

        //public event PropertyChangedEventHandler PropertyChanged;

        long _collectedTagLogCount = 0;
        long _persistedTagLogCount = 0;

        public event EventHandler<MyEventArgs> CollectedTagLogCountChanged;
        public event EventHandler<MyEventArgs> PersistedTagLogCountChanged;

        public MyDevice MyDevice
        {
            get => _myDevice;
            set
            {
                _myDevice = value;
            }
        }

        public ConcurrentQueue<MyS7Task> SendQueue
        {
            get => _sendQueue;
            set
            {
                _sendQueue = value;
            }
        }

        public long CollectedTagLogCount
        {
            get => Volatile.Read(ref _collectedTagLogCount);
        }

        public long PersistedTagLogCount
        {
            get => Volatile.Read(ref _persistedTagLogCount);
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public MyS7Context(string name, SynchronizationContext? uiContext, MyLogger myLogger, MyDevice myDevice, ConcurrentQueue<MyPersistance> persistQueue)
        {
            _name = name;
            _myLogger = myLogger;
            _uiContext = uiContext!;
            _myDevice = myDevice;
            _sendQueue = new();
            _persistQueue = persistQueue;
            Initialize();
        }

        public static string GetLocalIp()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
                .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .ToList().FirstOrDefault() ?? "127.0.0.1";
        }

        //protected virtual void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new(propertyName));
        //}

        public long IncreaseCollectedTagLogCount()
        {
            var result = Interlocked.Increment(ref _collectedTagLogCount);
            //OnPropertyChanged(nameof(CollectedTagLogCount));
            //CollectedTagLogCountChanged?.Invoke(this, new(result));
            MyMessageBus.Publish("CollectedTagLogCountChanged", result);
            return result;
        }

        public long IncreasePersistedTagLogCount()
        {
            var result = Interlocked.Increment(ref _persistedTagLogCount);
            //OnPropertyChanged(nameof(PersistedTagLogCount));
            //PersistedTagLogCountChanged?.Invoke(this, new(result));
            MyMessageBus.Publish("PersistedTagLogCountChanged", result);
            return result;
        }

        public virtual void Initialize()
        {
            if (_myDevice != null)
            {
                _myLogger.Log(LogLevel.Info, "tags.json数据加载成功");
                // init plc
                //_plc = new Plc(CpuType.S71200, _myDevice!.IpAddress, 0, 1);
                _plc = new Plc(CpuType.S71200, GetLocalIp(), 0, 1);
                _myLogger.Log(LogLevel.Info, "初始化plc成功");

                // init cts
                _cts = new();
            }
        }

        public void Connect()
        {
            _cts = new();
            _plc.Open();
            _myLogger.Log(LogLevel.Warn, "打开plc连接");

            // start _collectTask
            _collectTask = Task.Run(async () => await CollectAsync(), _cts.Token);
            _myLogger.Log(LogLevel.Warn, "启动_collectTask任务");

            // start _sendTask
            _sendTask = Task.Run(async () => await SendAsync(), _cts.Token);
            _myLogger.Log(LogLevel.Warn, "启动_sendTask任务");
        }

        public async Task DisconnectAsync()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            List<Task> tasks = new List<Task>();
            if (_collectTask != null)
            {
                tasks.Add(_collectTask);
            }
            if (_sendTask != null)
            {
                tasks.Add(_sendTask);
            }

            await Task.WhenAll(tasks);

            _cts?.Dispose();
            _plc.Close();

            _myLogger.Log(LogLevel.Warn, "关闭plc连接");
        }

        public async Task<T[]> ReadAsync<T>(string address, ushort count) where T : struct
        {
            _myLogger.Log(LogLevel.Info, "尝试读取数据");
            var result = await _plc.ReadAsync<T>(address, count);
            _myLogger.Log(LogLevel.Info, "数据读取成功");
            return result;
        }

        public async Task WriteAsync<T>(string address, T[] values) where T : struct
        {
            _myLogger.Log(LogLevel.Info, "尝试写入数据");
            await _plc.WriteAsync<T>(address, values);
            _myLogger.Log(LogLevel.Info, "数据写入成功");
        }

        public static async Task<object> GetMyS7TaskResult(ConcurrentQueue<MyS7Task> sendQueue, MyS7Task myS7Task, CancellationToken? cancellationToken = null)
        {
            TaskCompletionSource<object> tcs = new();
            CancellationTokenSource cts = new(2000);

            var externalCancellationToken = cancellationToken ?? CancellationToken.None;
            if (externalCancellationToken != CancellationToken.None)
            {
                // 关联cancellationToken和cts.Token，这样一旦外部cancellationToken超时被cancel了，cts也会被同步超时cancel
                CancellationTokenSource.CreateLinkedTokenSource(cts.Token, externalCancellationToken);
            }

            var ctr = cts.Token.Register(() =>
            {
                tcs.SetException(new TimeoutException());
                //cts.Dispose();
            });

            object result = null;

            try
            {
                //var bytes = await _plc.ReadBytesAsync(g.DataType, g.DbIndex, g.ByteOffset, g.ByteCount);

                myS7Task.TaskCompletionSource = tcs;
                sendQueue.Enqueue(myS7Task);
                result = await myS7Task.TaskCompletionSource.Task;
            }
            catch (Exception e)
            {
                //myLogger.Log(LogLevel.Error, $"出错:{e.Message}", e);
                throw e;
            }
            finally
            {
                cts.Dispose();
                ctr.Dispose();
            }

            return result;
        }

        async Task CollectAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                if (!_isDeviceOffline)
                {
                    // plc is online
                    try
                    {
                        foreach (var g in _myDevice.GroupList)
                        {
                            MyAddress myAddress = new(g.StartAddress);
                            g.DataType = myAddress.DataType;
                            g.DbIndex = myAddress.DbIndex;
                            g.ByteOffset = myAddress.ByteOffset;
                            g.BitOffset = myAddress.BitOffset;

                            byte[] bytes = null;

                            /*
                            TaskCompletionSource<object> tcs = new();
                            CancellationTokenSource cts = new(2000);
                            var ctr = cts.Token.Register(() =>
                            {
                                tcs.SetException(new TimeoutException());
                                //cts.Dispose();
                            });

                            

                            try
                            {
                                _myLogger.Log(LogLevel.Info, $"尝试批量读取数据group:{g.Name}");

                                //var bytes = await _plc.ReadBytesAsync(g.DataType, g.DbIndex, g.ByteOffset, g.ByteCount);
                                
                                MyS7Task myS7Task = new MyS7Task()
                                {
                                    MyS7TaskType = MyS7TaskType.ReadBytesAsync,
                                    IpAddress = _myDevice.IpAddress,
                                    TaskCompletionSource = tcs,
                                    StartAddress = g.StartAddress,
                                    ByteCount = g.ByteCount,
                                };
                                _sendQueue.Enqueue(myS7Task);
                                bytes = (byte[])await myS7Task.TaskCompletionSource.Task;

                                _myLogger.Log(LogLevel.Info, $"批量读取数据成功group:{g.Name}");
                            }
                            catch (Exception e)
                            {
                                _myLogger.Log(LogLevel.Error, $"批量读取数据出错:{e.Message}", e);
                                throw e;
                            }
                            finally
                            {
                                cts.Dispose();
                                ctr.Dispose();
                            }
                            //*/

                            _myLogger.Log(LogLevel.Info, $"尝试批量读取数据group:{g.Name}");

                            MyS7Task myS7Task = new()
                            {
                                MyS7TaskType = MyS7TaskType.ReadBytesAsync,
                                IpAddress = _myDevice.IpAddress,
                                //TaskCompletionSource = tcs,
                                StartAddress = g.StartAddress,
                                ByteCount = g.ByteCount,
                            };

                            bytes = (byte[])await GetMyS7TaskResult(_sendQueue, myS7Task);

                            _myLogger.Log(LogLevel.Info, $"批量读取数据成功group:{g.Name}");

                            if (bytes != null)
                            {
                                SetTagValues(bytes, g);
                            }
                        }
                        _currentErrorTimes = 0;
                    }
                    catch (Exception ex)
                    {
                        _currentErrorTimes++;
                        _myLogger.Log(LogLevel.Error, $"批量读取数据出错（{ex.Message}），连续错误次数 {_currentErrorTimes}/{_maxErrorTimes}", ex);
                        if (_currentErrorTimes >= _maxErrorTimes)
                        {
                            _isDeviceOffline = true;
                            _myLogger.Log(LogLevel.Info, "连续出错次数到达上线，判定plc离线");
                        }
                    }

                    await Task.Delay(_dataAcquisitionInterval);
                }
                else
                {
                    // plc is offline
                    _plc.Close();
                    _plc = null;
                    _myLogger.Log(LogLevel.Warn, "关闭plc连接");
                    _currentErrorTimes = 0;

                    // 等待时间太短的话，前面的socket对象没完全释放的话，下面的.Open方法就会卡死
                    await Task.Delay(10000);

                    _plc = new Plc(CpuType.S71200, _myDevice!.IpAddress, 0, 1);
                    _plc.Open();
                    _myLogger.Log(LogLevel.Warn, "开打plc连接");
                    _isDeviceOffline = false;
                }
            }
        }

        void SetTagValues(byte[] bytes, MyGroup g)
        {
            foreach (var tag in g.TagList)
            {
                MyAddress myAddress = new(tag.StartAddress);
                tag.DataType = myAddress.DataType;
                tag.DbIndex = myAddress.DbIndex;
                tag.ByteOffset = myAddress.ByteOffset;
                tag.BitOffset = myAddress.BitOffset;
                tag.ByteCount = myAddress.ByteCount;

                if (_uiContext != null)
                {
                    if (_uiContext == SynchronizationContext.Current)
                    {
                        // 当前线程是ui线程
                        SetValue(bytes, tag);
                    }
                    else
                    {
                        // 当前线程不是ui线程
                        _uiContext.Send(state => SetValue(bytes, tag), null);
                    }
                }
                else
                {
                    // ui线程之前传入为null
                    SetValue(bytes, tag);
                }
            }
        }

        void SetValue(byte[] bytes, MyTag tag)
        {
            var tempBytes = new byte[tag.ByteCount];
            Array.Copy(bytes, tag.ByteOffset - tag.MyGroup.ByteOffset, tempBytes, 0, tag.ByteCount);

            if (tag.ValueType.ToUpper() == "BOOLEAN")
            {
                var boolByte = PlcEx.ToValue<byte>(tempBytes);
                tag.Value = (boolByte & (1 << tag.BitOffset)) > 0;
            }
            else
            {
                tag.Value = tag.ValueType.ToUpper() switch
                {
                    "SBYTE" => PlcEx.ToValue<sbyte>(tempBytes),
                    "BYTE" => PlcEx.ToValue<byte>(tempBytes),
                    "INT16" => PlcEx.ToValue<short>(tempBytes),
                    "UINT16" => PlcEx.ToValue<ushort>(tempBytes),
                    "INT32" => PlcEx.ToValue<int>(tempBytes),
                    "UINT32" => PlcEx.ToValue<uint>(tempBytes),
                    "SINGLE" => PlcEx.ToValue<float>(tempBytes),
                    _ => new Exception($"SetTagValues方法读取tag.ValueType:{tag.ValueType}时异常，不支持{tag.ValueType}")
                };
            }

            IncreaseCollectedTagLogCount();

            _persistQueue.Enqueue(new()
            {
                MyPersistanceType = MyPersistanceType.TagLog,
                State = tag
            });

            //Interlocked.Increment(ref _collectedTagLogCount);
            //CollectedTagLogCountChanged?.Invoke(this, new(_collectedTagLogCount));


            if (tag.NeedToMonitor)
            {
                Action<MyTag> action = tag.ValueType.ToUpper() switch
                {
                    "BOOLEAN" => MonitorBooleanValue,
                    _ => MonitorNumberValue
                };
                action.Invoke(tag);
            }
        }

        void MonitorBooleanValue(MyTag myTag)
        {
            dynamic value = myTag.Value;
            dynamic booleanThreshold = myTag.BooleanThreshold;

            var isInvalid = booleanThreshold != bool.Parse(value.ToString());

            MonitorValue(myTag, isInvalid);
        }

        void MonitorNumberValue(MyTag myTag)
        {
            dynamic value = myTag.Value;
            dynamic highShreshold = myTag.HighThreshold;
            dynamic lowShreshold = myTag.LowThreshold;
            dynamic highDeadBand = myTag.HighDeadBand;
            dynamic lowDeadBand = myTag.LowDeadBand;


            var isInvalid = value >= highShreshold
                || value <= lowShreshold
                || value >= highShreshold - highDeadBand && myTag.IsAlarmed
                || value <= lowShreshold + lowDeadBand && myTag.IsAlarmed;

            MonitorValue(myTag, isInvalid);
        }

        void MonitorValue(MyTag myTag, bool isInvalid)
        {
            if (isInvalid)
            {
                // 非法值
                if (!myTag.IsAlarmed)
                {
                    // 还没报警
                    if (!myTag.IsNoticed)
                    {
                        // 还没预警
                        if ((DateTime.UtcNow - myTag.LastNoticed).TotalMilliseconds >= myTag.OnDelay)
                        {
                            // 还没报警，还没预警，不在OnDelay时间范围内，进行预警
                            myTag.IsNoticed = true;
                            myTag.LastNoticed = DateTime.UtcNow;
                            var message = $"{myTag.Name}触发预警，当前值{myTag.Value}";

                            // 记录日志
                            _myLogger.Log(LogLevel.Warn, message);
                            // 入UI _persistQueue队列然后消费写入db
                            _persistQueue.Enqueue(new()
                            {
                                MyPersistanceType = MyPersistanceType.AlarmLog,
                                State = (myTag, message, 0L)
                            });
                        }
                        else
                        {
                            // 还没报警，还没预警，在OnDelay时间范围内，这种情况不存在，不用处理
                        }
                    }
                    else
                    {
                        // 已经预警
                        if ((DateTime.UtcNow - myTag.LastNoticed).TotalMilliseconds >= myTag.OnDelay)
                        {
                            // 还没报警，已经预警，不在OnDelay时间范围内，进行报警
                            myTag.IsAlarmed = true;
                            myTag.LastAlarmed = DateTime.UtcNow;
                            var duration = new DateTimeOffset(myTag.LastAlarmed).ToUnixTimeMilliseconds() - new DateTimeOffset(myTag.LastNoticed).ToUnixTimeMilliseconds();
                            myTag.LastNoticed = DateTime.UtcNow;
                            var message = $"{myTag.Name}触发报警，当前值{myTag.Value}";

                            // 记录日志
                            _myLogger.Log(LogLevel.Warn, message);
                            // 入UI _persistQueue队列然后消费写入db
                            _persistQueue.Enqueue(new()
                            {
                                MyPersistanceType = MyPersistanceType.AlarmLog,
                                State = (myTag, message, duration)
                            });
                        }
                        else
                        {
                            // 还没报警，已经预警，在OnDelay时间范围内，这种情况正常
                            //myTag.LastNoticed = DateTime.Now;
                        }
                    }
                }
                else
                {
                    // 已经报警
                    myTag.LastAlarmed = DateTime.UtcNow;
                    myTag.LastNoticed = DateTime.UtcNow;
                }
            }
            else
            {
                if ((DateTime.UtcNow - myTag.LastAlarmed).TotalMilliseconds >= myTag.OffDelay && myTag.IsAlarmed)
                {
                    myTag.IsAlarmed = false;
                    myTag.LastNoticed = DateTime.UtcNow;
                    var duration = new DateTimeOffset(myTag.LastNoticed).ToUnixTimeMilliseconds() - new DateTimeOffset(myTag.LastAlarmed).ToUnixTimeMilliseconds();
                    var message = $"{myTag.Name}取消报警，当前值{myTag.Value}";

                    // 记录日志
                    _myLogger.Log(LogLevel.Warn, message);
                    // 入UI _persistQueue队列然后消费写入db
                    _persistQueue.Enqueue(new()
                    {
                        MyPersistanceType = MyPersistanceType.AlarmLog,
                        State = (myTag, message, duration)
                    });
                }

                if ((DateTime.UtcNow - myTag.LastNoticed).TotalMilliseconds >= myTag.OffDelay && myTag.IsNoticed)
                {
                    myTag.IsNoticed = false;
                    var duration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - new DateTimeOffset(myTag.LastNoticed).ToUnixTimeMilliseconds();
                    var message = $"{myTag.Name}取消预警，当前值{myTag.Value}";

                    // 记录日志
                    _myLogger.Log(LogLevel.Warn, message);
                    // 入UI _persistQueue队列然后消费写入db
                    _persistQueue.Enqueue(new()
                    {
                        MyPersistanceType = MyPersistanceType.AlarmLog,
                        State = (myTag, message, duration)
                    });
                }
            }
        }

        async Task SendAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                MyS7Task? myS7Task = null;
                if (_sendQueue.TryDequeue(out myS7Task))
                {
                    object result = null;
                    try
                    {
                        if (myS7Task.MyS7TaskType == MyS7TaskType.ReadTagsFromMemory)
                        {
                            result = _myDevice;
                        }

                        if (myS7Task.MyS7TaskType == MyS7TaskType.ReadAsync)
                        {
                            result = myS7Task.ValueType.ToUpper() switch
                            {
                                "BOOLEAN" => (await _plc.ReadAsync<bool>(myS7Task.StartAddress, 1))[0],
                                "SBYTE" => (await _plc.ReadAsync<sbyte>(myS7Task.StartAddress, 1))[0],
                                "BYTE" => (await _plc.ReadAsync<byte>(myS7Task.StartAddress, 1))[0],
                                "INT16" => (await _plc.ReadAsync<short>(myS7Task.StartAddress, 1))[0],
                                "UINT16" => (await _plc.ReadAsync<ushort>(myS7Task.StartAddress, 1))[0],
                                "INT32" => (await _plc.ReadAsync<int>(myS7Task.StartAddress, 1))[0],
                                "UINT32" => (await _plc.ReadAsync<uint>(myS7Task.StartAddress, 1))[0],
                                "SINGLE" => (await _plc.ReadAsync<float>(myS7Task.StartAddress, 1))[0],
                                _ => throw new Exception($"不支持当前MyS7Task的ValueType:{myS7Task.ValueType}")
                            };
                        }

                        if (myS7Task.MyS7TaskType == MyS7TaskType.WriteAsync)
                        {
                            if (myS7Task.ValueType.ToUpper() == "BOOLEAN") await _plc.WriteAsync<bool>(myS7Task.StartAddress, new bool[] { bool.Parse(myS7Task.Value!.ToString()!) });
                            if (myS7Task.ValueType.ToUpper() == "SBYTE") await _plc.WriteAsync<sbyte>(myS7Task.StartAddress, new sbyte[] { sbyte.Parse(myS7Task.Value!.ToString()!) });
                            if (myS7Task.ValueType.ToUpper() == "BYTE") await _plc.WriteAsync<byte>(myS7Task.StartAddress, new byte[] { byte.Parse(myS7Task.Value!.ToString()!) });
                            if (myS7Task.ValueType.ToUpper() == "INT16") await _plc.WriteAsync<short>(myS7Task.StartAddress, new short[] { short.Parse(myS7Task.Value!.ToString()!) });
                            if (myS7Task.ValueType.ToUpper() == "UINT16") await _plc.WriteAsync<ushort>(myS7Task.StartAddress, new ushort[] { ushort.Parse(myS7Task.Value!.ToString()!) });
                            if (myS7Task.ValueType.ToUpper() == "INT32") await _plc.WriteAsync<int>(myS7Task.StartAddress, new int[] { int.Parse(myS7Task.Value!.ToString()!) });
                            if (myS7Task.ValueType.ToUpper() == "UINT32") await _plc.WriteAsync<uint>(myS7Task.StartAddress, new uint[] { uint.Parse(myS7Task.Value!.ToString()!) });
                            if (myS7Task.ValueType.ToUpper() == "SINGLE") await _plc.WriteAsync<float>(myS7Task.StartAddress, new float[] { float.Parse(myS7Task.Value!.ToString()!) });
                        }

                        if (myS7Task.MyS7TaskType == MyS7TaskType.ReadBytesAsync)
                        {
                            MyAddress myAddress = null;
                            if (myS7Task.StartAddress != null)
                            {
                                myAddress = new(myS7Task.StartAddress);
                            }
                            result = await _plc.ReadBytesAsync(myAddress!.DataType, myAddress.DbIndex, myAddress.ByteOffset, myS7Task.ByteCount);
                        }

                        myS7Task.TaskCompletionSource.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        _myLogger.Log(LogLevel.Error, $"错误:{ex.Message}", ex);
                        //myS7Task.TaskCompletionSource.SetException(ex);
                    }
                }

                await Task.Delay(10);
            }
        }

        protected virtual async Task Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer

                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                }

                await Task.WhenAll(_collectTask, _sendTask);

                _cts.Dispose();
                ((IDisposable)_plc).Dispose();
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MyS7Context()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~MyS7Context()
        {
            Dispose(disposing: false);
        }
    }
}
