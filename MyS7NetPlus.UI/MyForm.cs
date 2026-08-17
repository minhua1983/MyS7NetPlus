using MyS7NetPlus.Common.DataAcquisition;
using MyS7NetPlus.Common.Tool;
using MyS7NetPlus.UI.Models;
using MyS7NetPlus.UI.Repositories;
using MyS7NetPlus.UI.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Collections.Concurrent;
using System.Net;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace MyS7NetPlus.UI
{
    public partial class MyForm : Form
    {
        bool _isAllDoneBeforeFormClosing = false;

        MyForm _myFormInstance;
        MyLogger _myLogger;
        List<MyS7Context> _myS7ContextList = new();
        CancellationTokenSource _cts = new();

        ConcurrentQueue<MyS7Task> _globalSendQueue;
        Task _globalSendTask;

        ConcurrentQueue<MyPersistance> _persistQueue;
        Task _persistTask;

        TagLogRepository _tagLogRepository;
        TagLogService _tagLogService;
        AlarmLogRepository _alarmLogRepository;
        AlarmLogService _alarmLogService;

        int _persistInterval = 100;

        public MyForm(ConcurrentQueue<MyS7Task> globalSendQueue)
        {
            InitializeComponent();
            _myFormInstance = this;
            _globalSendQueue = globalSendQueue;
            _persistQueue = new();
            _myLogger = new("WinFormLogger");
            _myLogger.Logged += MyLogger_Logged;

            // load data acquisition configuration
            var dataAcquisitionFile = $"{AppDomain.CurrentDomain.BaseDirectory}\\tags.json";

            if (!File.Exists(dataAcquisitionFile))
            {
                _myLogger.Log(LogLevel.Error, "tags.json没找到，采集配置参数初始化失败");
                throw new Exception("tags.json没找到，采集配置参数初始化失败");
            }

            using StreamReader sr = new StreamReader(dataAcquisitionFile);
            var jsonContent = sr.ReadToEnd();
            var myDeviceList = JsonConvert.DeserializeObject<MyDevice[]>(jsonContent)?.ToList();
            int myS7ContextIndex = 0;
            myDeviceList?.ForEach(myDevice =>
            {
                MyS7Context myS7Context = new($"myS7Context{myS7ContextIndex.ToString()}",
                    SynchronizationContext.Current,
                    _myLogger,
                    myDevice,
                    _persistQueue);
                _myS7ContextList.Add(myS7Context);

                myDevice.MyS7Context = myS7Context;
                myDevice.GroupList.ForEach(myGroup =>
                {
                    myGroup.MyDevice = myDevice;
                    myGroup.TagList.ToList().ForEach(myTag =>
                    {
                        myTag.MyGroup = myGroup;
                    });
                });

                MyMessageBus.Subscribe("CollectedTagLogCountChanged", myS7Context.Name, (long n) => lbl_collected_tag_log_count.Text = n.ToString());

                MyMessageBus.Subscribe("PersistedTagLogCountChanged", myS7Context.Name, (long n) => lbl_persisted_tag_log_count.Text = n.ToString());

                //myS7Context.CollectedTagLogCountChanged += MyS7Context_CollectedTagLogCountChanged;
                //myS7Context.PersistedTagLogCountChanged += MyS7Context_PersistedTagLogCountChanged;

                // 不推荐下面这种实现INotifyPropertyChanged接口的写法，它在关闭winform时会提前解绑自动注册事件，这会导致界面的已入库卡住，但是实际会等待数据入库后winform在被关闭，体验很差。
                //lbl_collected_tag_log_count.DataBindings.Add("Text", myS7Context, nameof(myS7Context.CollectedTagLogCount));
                //lbl_persisted_tag_log_count.DataBindings.Add("Text", myS7Context, nameof(myS7Context.PersistedTagLogCount));
                myS7ContextIndex++;
            });



            this.FormClosing += MyForm_FormClosing;
            tc_groups.TabPages.Clear();

            _globalSendTask = Task.Run(() => SendAsync(), _cts.Token);
            _persistTask = Task.Run(() => PersistAsync(), _cts.Token);

            _tagLogRepository = new();
            _tagLogService = new(_tagLogRepository);

            _alarmLogRepository = new();
            _alarmLogService = new(_alarmLogRepository);
        }

        void MyS7Context_CollectedTagLogCountChanged(object? sender, MyEventArgs e)
        {
            _myFormInstance.Invoke(() => lbl_collected_tag_log_count.Text = e.State.ToString());
        }

        void MyS7Context_PersistedTagLogCountChanged(object? sender, MyEventArgs e)
        {
            _myFormInstance.Invoke(() => lbl_persisted_tag_log_count.Text = e.State.ToString());
        }

        async Task SendAsync()
        {
            while (true)
            {
                MyS7Task? myS7Task = null;
                if (_globalSendQueue.TryDequeue(out myS7Task))
                {
                    _myS7ContextList.Where(myS7Context => myS7Context.MyDevice.IpAddress == myS7Task.IpAddress).FirstOrDefault()?.SendQueue.Enqueue(myS7Task);

                }
                await Task.Delay(10);
            }
        }

        bool PersistTagLog(MyPersistance myPersistance)
        {
            bool result = false;
            try
            {
                MyTag? myTag = myPersistance.State as MyTag;
                TagLog tagLog = new()
                {
                    DeviceName = myTag?.MyGroup.MyDevice.Name,
                    GroupName = myTag?.MyGroup.Name,
                    TagName = myTag?.Name,
                    TagValue = myTag?.Value.ToString()!,
                    CollectedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                _tagLogService!.Insert(tagLog);
                result = true;
                myPersistance.Callback?.Invoke();

                myTag?.MyGroup.MyDevice.MyS7Context.IncreasePersistedTagLogCount();
            }
            catch (Exception ex)
            {
                _myLogger.Log(LogLevel.Error, $"PersistTagLog方法出错，{ex.Message}", ex);
            }

            return result;
        }

        TagLog CreateTagLog(MyPersistance myPersistance)
        {
            MyTag? myTag = myPersistance.State as MyTag;
            TagLog tagLog = new()
            {
                DeviceName = myTag?.MyGroup.MyDevice.Name,
                GroupName = myTag?.MyGroup.Name,
                TagName = myTag?.Name,
                TagValue = myTag?.Value.ToString()!,
                CollectedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            myTag?.MyGroup.MyDevice.MyS7Context.IncreasePersistedTagLogCount();

            return tagLog;
        }

        bool PersistAlarmLog(MyPersistance myPersistance)
        {
            bool result = false;
            try
            {
                if (myPersistance.State is (MyTag myTag, string message, long duration))
                {
                    AlarmLog alarmLog = new()
                    {
                        DeviceName = myTag?.MyGroup.MyDevice.Name,
                        GroupName = myTag?.MyGroup.Name,
                        TagName = myTag?.Name,
                        TagValue = myTag?.Value.ToString()!,
                        IsNoticed = myTag!.IsNoticed,
                        IsAlarmed = myTag!.IsAlarmed,
                        Message = message,
                        Duration = duration,
                        TriggeredAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    _alarmLogService!.Insert(alarmLog);
                    result = true;
                    myPersistance.Callback?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _myLogger.Log(LogLevel.Error, $"PersistAlarmLog方法出错，{ex.Message}", ex);
            }

            return result;
        }

        AlarmLog CreateAlarmLog(MyPersistance myPersistance)
        {

            if (myPersistance.State is (MyTag myTag, string message, long duration))
            {
                AlarmLog alarmLog = new()
                {
                    DeviceName = myTag?.MyGroup.MyDevice.Name,
                    GroupName = myTag?.MyGroup.Name,
                    TagName = myTag?.Name,
                    TagValue = myTag?.Value.ToString()!,
                    IsNoticed = myTag!.IsNoticed,
                    IsAlarmed = myTag!.IsAlarmed,
                    Message = message,
                    Duration = duration,
                    TriggeredAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                return alarmLog;
            }

            throw new Exception("CreateAlarmLog方法中myPersistance.State不是(MyTag myTag, string message, long duration)格式");
        }

        async Task PersistAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                /*
                if (_persistQueue.TryDequeue(out MyPersistance? myPersistance))
                {
                    var result = myPersistance.MyPersistanceType switch
                    {
                        MyPersistanceType.TagLog => PersistTagLog(myPersistance),
                        MyPersistanceType.AlarmLog => PersistAlarmLog(myPersistance),
                        _ => false
                    };
                }
                //*/

                List<TagLog> tagLogList = new();
                List<AlarmLog> alarmLogList = new();

                while (_persistQueue.TryDequeue(out MyPersistance? myPersistance))
                {
                    if (myPersistance.MyPersistanceType == MyPersistanceType.TagLog)
                    {
                        tagLogList.Add(CreateTagLog(myPersistance));
                    }
                    if (myPersistance.MyPersistanceType == MyPersistanceType.AlarmLog)
                    {
                        alarmLogList.Add(CreateAlarmLog(myPersistance));
                    }
                }

                _tagLogService?.BulkInsert(tagLogList);
                _alarmLogService?.BulkInsert(alarmLogList);

                await Task.Delay(_persistInterval);
            }

            while (!CheckIfPersistanceIsDone())
            {
                /*
                if (_persistQueue.TryDequeue(out MyPersistance? myPersistance))
                {
                    var result = myPersistance.MyPersistanceType switch
                    {
                        MyPersistanceType.TagLog => PersistTagLog(myPersistance),
                        MyPersistanceType.AlarmLog => PersistAlarmLog(myPersistance),
                        _ => false
                    };
                }
                //*/

                List<TagLog> tagLogList = new();
                List<AlarmLog> alarmLogList = new();

                while (_persistQueue.TryDequeue(out MyPersistance? myPersistance))
                {
                    if (myPersistance.MyPersistanceType == MyPersistanceType.TagLog)
                    {
                        tagLogList.Add(CreateTagLog(myPersistance));
                    }
                    if (myPersistance.MyPersistanceType == MyPersistanceType.AlarmLog)
                    {
                        alarmLogList.Add(CreateAlarmLog(myPersistance));
                    }
                }

                _tagLogService?.BulkInsert(tagLogList);
                _alarmLogService?.BulkInsert(alarmLogList);

                await Task.Delay(_persistInterval);
            }

            await Task.Delay(500);
        }

        bool CheckIfPersistanceIsDone()
        {
            bool result = true;

            foreach (var myS7Context in _myS7ContextList)
            {
                result = myS7Context.PersistedTagLogCount == myS7Context.CollectedTagLogCount;
                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        private async void MyForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isAllDoneBeforeFormClosing)
            {
                try
                {
                    btn_connect.Enabled = false;
                    tc_groups.Enabled = false;

                    // 取消关闭
                    e.Cancel = true;

                    //必须注销写日志事件的回调,否则_collectTask就算都变成RanToCompletion状态,但是他们在RanToCompletion状态之前调用Log事件,会驻留在底层事件队列中,就算Form1进行了Dispose,当事件从底层事件队列中取出去调用对应的回调方法时,在回调方法内部实际已经无法访问Form1对象了.
                    _myLogger.Logged -= MyLogger_Logged;

                    // _myModbusContext执行DisconnectAsync
                    foreach (var myS7Context in _myS7ContextList)
                    {
                        await myS7Context.Disconnect();
                    }

                    if (_cts != null && !_cts.IsCancellationRequested)
                    {
                        _cts.Cancel();
                    }

                    await Task.WhenAll(_persistTask);

                    List<MyDevice> myDeviceList = new();

                    // myS7Context中的事件解绑必须在_persistTask完成之后，不然已入库数字和已采集数字对不上
                    foreach (var myS7Context in _myS7ContextList)
                    {
                        MyMessageBus.Unsubscribe("CollectedTagLogCountChanged", myS7Context.Name);
                        MyMessageBus.Unsubscribe("PersistedTagLogCountChanged", myS7Context.Name);

                        //myS7Context.CollectedTagLogCountChanged -= MyS7Context_CollectedTagLogCountChanged;
                        //myS7Context.PersistedTagLogCountChanged -= MyS7Context_PersistedTagLogCountChanged;

                        myDeviceList.Add(myS7Context.MyDevice);
                    }

                    _tagLogRepository.Dispose();
                    _alarmLogRepository.Dispose();
                    _cts?.Dispose();

                    ;

                    var directoryPath = $"{AppContext.BaseDirectory}\\tags";
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    using StreamWriter streamWriter = new($"{directoryPath}\\tags_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.json");
                    await streamWriter.WriteAsync(JsonConvert.SerializeObject(myDeviceList));

                    // 标记为所有任务都处理完
                    _isAllDoneBeforeFormClosing = true;

                    //这个方法会再次触发Form1_FormClosing
                    this.Close();

                }
                catch (Exception ex)
                {
                    _myLogger.Log(LogLevel.Error, ex.Message, ex);
                }
                finally
                {

                }
            }
            else
            {
                _myLogger.Log(LogLevel.Info, "关闭WinForm");
            }
        }
        private void MyLogger_Logged(object? sender, MyLogEventArgs e)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    AddLogToListView(e);
                }));
            }
            else
            {
                AddLogToListView(e);
            }
        }

        void AddLogToListView(MyLogEventArgs e)
        {
            ListViewItem listViewItem = new(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            listViewItem.SubItems.Add(e.LogLevel.Name);
            listViewItem.SubItems.Add(e.Message);
            lv_log_message.Items.Add(listViewItem);

            //*
            if (lv_log_message.Items.Count > 10)
            {
                // bug
                lv_log_message.Items.RemoveAt(0);
            }
            //*/

            lv_log_message.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private async void btn_connect_Click(object sender, EventArgs e)
        {
            /*
            var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();
            var plc = new Plc(CpuType.S71200, ip!, 0, 1);
            //var plc = new Plc(CpuType.S71200, "192.168.71.36", 0, 1);
            try
            {
                plc.Open();

                
                //var result = plc.Read("DB1.DBW2");
                //if(result is ushort r)
                //{
                //    var data = (short)r;
                //    Console.WriteLine(data);

                //}
                
                //plc.Write("DB1.DBW2",0);
                //plc.Write(DataType.DataBlock, 1, 2, new short[] { -111, -2});
                //plc.Write(DataType.DataBlock, 1, 10, new float[] { -1.4f, -2.5f });
                //var result = plc.Read<double>("DB1.DBW2");
                //var result = plc.Read(DataType.DataBlock, 1, 2, VarType.Int, 1);

                //var result = plc.Read<bool>("DB1.DBX0.0");
                //var result = plc.Read<bool>("DB1.DBX1.0");
                //var result = plc.Read<short>("DB1.DBW2");
                //var result = plc.Read<bool>("Q0.0");


                //await plc.WriteAsync<short>("DB1.DBW2", (short)-99);
                //var result = await plc.ReadAsync<short[]>("DB1.DBW2");





                //await plc.WriteAsync<float>("DB1.DBD10", (float)-9.9);
                //var result = await plc.ReadAsync<float>("DB1.DBD10");


                //await plc.WriteAsync<bool>("DB1.DBX0.5", new bool[] { false, false, false, true });
                //var result = await plc.ReadAsync<bool>("DB1.DBX0.5", 4);

                //var result = await plc.ReadV1Async<int>("DB1.DBD20", 2);

                //await plc.WriteAsync<float>("DB1.DBD10", new float[] { -11.2f, -21.3f });
                //var result = await plc.ReadAsync<float>("DB1.DBD10", 2);




                await plc.WriteAsync<short>("DB1.DBW2", new short[] { -100, 30001, -40, -30001 });
                var result = await plc.ReadAsync<short>("DB1.DBW2", 4);

                //await plc.WriteAsync<float>("DB1.DBD10", new float[] { -3.8f, -13.7f });
                //var result = await plc.ReadAsync<float>("DB1.DBD10", 2);

                //await plc.WriteAsync<sbyte>("DB1.DBB18", new sbyte[] { -2, -100 });
                //var result = await plc.ReadAsync<sbyte>("DB1.DBB18", 2);

                //await plc.WriteAsync<int>("DB1.DBD20", new int[] { -123454321, -987656789 });
                //var result = await plc.ReadAsync<int>("DB1.DBD20", 2);

                //await plc.WriteAsync<bool>("DB1.DBX0.1", new bool[] { false, false, true });
                //var result = await plc.ReadAsync<bool>("DB1.DBX0.1", 3);
                Console.WriteLine(result?.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                plc.Close();
            }
            //*/
            btn_connect.Enabled = false;
            _myS7ContextList.ForEach(myS7Context =>
            {
                myS7Context.Connect();

                tc_groups.TabPages.Clear();

                myS7Context.MyDevice.GroupList.ForEach(g =>
                {
                    TabPage tabPage = new();
                    tabPage.Text = g.Name;
                    tc_groups.TabPages.Add(tabPage);

                    DataGridView dgv_tag_list = new();
                    dgv_tag_list.CellDoubleClick += Dgv_tag_list_CellDoubleClick;

                    dgv_tag_list.Location = new(12, 47);
                    dgv_tag_list.BorderStyle = BorderStyle.None;

                    dgv_tag_list.Dock = DockStyle.Fill;
                    // 关键：关闭底部空白新增行，消除最后那一行空行
                    dgv_tag_list.AllowUserToAddRows = false;
                    // 可选：禁止用户按Delete键删除行（工控场景常用）
                    dgv_tag_list.AllowUserToDeleteRows = false;
                    // 【最重要】关闭自动生成全部属性列
                    dgv_tag_list.AutoGenerateColumns = false;
                    // 核心：选中整行
                    dgv_tag_list.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    // false=只能选1行；true=可以按住Ctrl多选多行
                    dgv_tag_list.MultiSelect = false;

                    // load DataGridView data
                    dgv_tag_list.Columns.Clear();
                    dgv_tag_list.DataSource = g.TagList;

                    List<DataGridViewTextBoxColumn> dataGridViewTextBoxColumnlist = new();
                    dataGridViewTextBoxColumnlist.Add(new()
                    {
                        HeaderText = "点位名称",
                        Name = "Name",
                        DataPropertyName = "Name",
                        Width = 100,
                        ReadOnly = true,
                    });

                    dataGridViewTextBoxColumnlist.Add(new()
                    {
                        HeaderText = "寻址表达",
                        Name = "StartAddress",
                        DataPropertyName = "StartAddress",
                        Width = 100,
                        ReadOnly = true,
                    });

                    dataGridViewTextBoxColumnlist.Add(new()
                    {
                        HeaderText = "数据块号",
                        Name = "DbIndex",
                        DataPropertyName = "DbIndex",
                        Width = 100,
                        ReadOnly = true,
                    });

                    dataGridViewTextBoxColumnlist.Add(new()
                    {
                        HeaderText = "数值类型",
                        Name = "ValueType",
                        DataPropertyName = "ValueType",
                        Width = 100,
                        ReadOnly = true,
                    });

                    dataGridViewTextBoxColumnlist.Add(new()
                    {
                        HeaderText = "当前数值",
                        Name = "Value",
                        DataPropertyName = "Value",
                        Width = 148,
                        ReadOnly = true,
                    });

                    dgv_tag_list.Columns.AddRange(dataGridViewTextBoxColumnlist.ToArray());
                    dgv_tag_list.CellFormatting += (sender, e) =>
                    {
                        if (e.RowIndex == 0)
                        {
                            return;
                        }

                        var row = dgv_tag_list.Rows[e.RowIndex];
                        if (row.DataBoundItem is MyTag myTag)
                        {
                            var valueColumnIndex = dgv_tag_list.Columns["Value"].Index;
                            if (e.ColumnIndex == valueColumnIndex)
                            {
                                e.CellStyle!.ForeColor = (myTag.IsNoticed, myTag.IsAlarmed) switch
                                {
                                    (true, true) => Color.Red,
                                    (true, false) => Color.Orange,
                                    _ => Color.Black
                                };
                            }
                        }
                    };

                    tabPage.Controls.Add(dgv_tag_list);
                });
            });
        }

        private void Dgv_tag_list_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            var myTag = ((DataGridView)sender!).Rows[e.RowIndex].DataBoundItem as MyTag;

            MyTagForm myTagForm = new(this, _myLogger, myTag!.MyGroup.MyDevice.MyS7Context.SendQueue, myTag!, _tagLogService, _alarmLogService);
            myTagForm.FormClosed += (sender, e) => tc_groups.Enabled = true;
            myTagForm.Width = this.Width;
            myTagForm.Height = this.Height;
            myTagForm.Location = new(this.Location.X + this.Width + 0, this.Location.Y);
            // 必须！手动坐标模式
            myTagForm.StartPosition = FormStartPosition.Manual;
            myTagForm.Show();
            tc_groups.Enabled = false;
        }
    }
}
