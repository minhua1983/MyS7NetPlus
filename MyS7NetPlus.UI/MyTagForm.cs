using MyS7NetPlus.Common.DataAcquisitions;
using MyS7NetPlus.Common.Tools;
using MyS7NetPlus.UI.Models;
using MyS7NetPlus.UI.Services;
using NLog;
using S7.Net.Types;
using ScottPlot;
using System.Collections.Concurrent;

namespace MyS7NetPlus.UI
{
    public partial class MyTagForm : Form
    {
        Form _myForm;
        MyLogger _myLogger;
        MyTag _myTag;
        ConcurrentQueue<MyS7Task> _sendQueue;
        TagLogService _tagLogService;
        AlarmLogService _alarmLogService;
        Queue<TagLog> _tagLogQueue;
        int _maxTagLogCountInChart = 300;
        System.Windows.Forms.Timer _uiTimer;
        List<AlarmLog> _alarmLogList;

        public MyTagForm(Form myForm, MyLogger myLogger, ConcurrentQueue<MyS7Task> myS7TaskQueue, MyTag myTag, TagLogService tagLogService, AlarmLogService alarmLogService)
        {
            _myForm = myForm;
            _myLogger = myLogger;
            _myTag = myTag;
            _sendQueue = myS7TaskQueue;
            _tagLogService = tagLogService;
            _alarmLogService = alarmLogService;
            _tagLogQueue = new();

            // 初始化UI绘图定时器，1000ms刷新一次画面
            _uiTimer = new System.Windows.Forms.Timer();
            _uiTimer.Interval = 1000;
            _uiTimer.Tick += Timer_Tick;
            _uiTimer.Start();

            InitializeComponent();

            this.FormClosing += MyTagForm_FormClosing;

            tb_tag_name.DataBindings.Add("Text", myTag, nameof(myTag.Name));
            tb_tag_data_type.DataBindings.Add("Text", myTag, nameof(myTag.DataType));
            tb_tag_start_address.DataBindings.Add("Text", myTag, nameof(myTag.StartAddress));
            tb_tag_db_index.DataBindings.Add("Text", myTag, nameof(myTag.DbIndex));
            tb_tag_value_type.DataBindings.Add("Text", myTag, nameof(myTag.ValueType));

            Binding valueBanding;
            if (myTag.ValueType.ToUpper() == "BOOLEAN")
            {
                valueBanding = new("Checked", myTag, nameof(myTag.Value));
                cb_tag_value.DataBindings.Add(valueBanding);
                tb_tag_value.Visible = false;
                tb_tag_value.Enabled = false;
            }
            else
            {
                valueBanding = new("Text", myTag, nameof(myTag.Value));
                tb_tag_value.DataBindings.Add(valueBanding);
                cb_tag_value.Visible = false;
                cb_tag_value.Enabled = false;
            }


            Binding needToMonitorBinding = new("Checked", myTag, nameof(myTag.NeedToMonitor));
            cb_tag_need_to_monitor.DataBindings.Add(needToMonitorBinding);


            cb_tag_boolean_threshold.DataBindings.Add("Checked", myTag, nameof(myTag.BooleanThreshold));
            tb_tag_high_threshold.DataBindings.Add("Text", myTag, nameof(myTag.HighThreshold));
            tb_tag_high_dead_band.DataBindings.Add("Text", myTag, nameof(myTag.HighDeadBand));
            tb_tag_low_threshold.DataBindings.Add("Text", myTag, nameof(myTag.LowThreshold));
            tb_tag_low_dead_band.DataBindings.Add("Text", myTag, nameof(myTag.LowDeadBand));

            tb_tag_name.Enabled = false;
            tb_tag_data_type.Enabled = false;
            tb_tag_start_address.Enabled = false;
            tb_tag_db_index.Enabled = false;
            tb_tag_value_type.Enabled = false;

            tb_tag_value.Enabled = true;
            cb_tag_need_to_monitor.Enabled = true;

            cb_tag_boolean_threshold.Enabled = false;
            tb_tag_high_threshold.Enabled = false;
            tb_tag_high_dead_band.Enabled = false;
            tb_tag_low_threshold.Enabled = false;
            tb_tag_low_dead_band.Enabled = false;

            Task.Delay(1000).ContinueWith(task =>
            {
                // ContinueWith时已经回到UI线程，即默认执行了ConfigureAwait(true)来回到UI线程，所以不用invoke
                if (myTag.ValueType.ToUpper() == "BOOLEAN")
                {
                    cb_tag_value.DataBindings.Remove(valueBanding);
                }
                else
                {
                    tb_tag_value.DataBindings.Remove(valueBanding);
                }

                cb_tag_need_to_monitor.DataBindings.Remove(needToMonitorBinding);
            });

            RenderAlarmLogList();
        }

        void RenderAlarmLogList()
        {
            // lv_alarm_message
            _alarmLogList = _alarmLogService.Selects(_myTag.MyGroup.MyDevice.Name,
                 _myTag.MyGroup.Name,
                 _myTag.Name,
                 DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds(),
                 DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                 100);

            _alarmLogList.ForEach(alarmLog =>
            {
                ListViewItem listViewItem = new(DateTimeOffset.FromUnixTimeMilliseconds(alarmLog.TriggeredAt).UtcDateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                listViewItem.SubItems.Add(alarmLog.IsAlarmed.ToString());
                listViewItem.SubItems.Add(alarmLog.Message);
                lv_alarm_message.Items.Add(listViewItem);

                //*
                if (lv_alarm_message.Items.Count > 100)
                {
                    // bug
                    lv_alarm_message.Items.RemoveAt(0);
                }
                //*/

                lv_alarm_message.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);
            });

            ContextMenuStrip contextMenuStrip = new();
            ToolStripMenuItem toolStripMenuItem = new("导出csv文件");
            toolStripMenuItem.Click += (sender, e) => ExportFileForAlarmLog();
            contextMenuStrip.Items.Add(toolStripMenuItem);
            lv_alarm_message.ContextMenuStrip = contextMenuStrip;
        }

        void ExportFileForAlarmLog()
        {
            using SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV文件(*.csv)|*.csv";
            saveFileDialog.FileName = $"AlarmLog_{System.DateTime.Now.AddHours(-24):yyyyMMddHHmmss}_{System.DateTime.Now:yyyyMMddHHmmss}.csv";
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            //带BOM，Excel中文不乱码
            var encoding = new System.Text.UTF8Encoding(true);
            using var streamWriter = new StreamWriter(saveFileDialog.FileName, false, encoding);

            //写表头
            streamWriter.WriteLine("Id,DeviceName,GroupName,TagName,TagValue,TriggeredAt,IsNoticed,IsAlarmed,Message");

            foreach (var alarmLog in _alarmLogList)
            {
                //简单转义：消息里面有双引号就替换，防止csv崩掉
                string msg = alarmLog.Message?.Replace("\"", "\"\"") ?? "";
                //字段用双引号包起来，防止内容带逗号换行
                streamWriter.WriteLine($"{alarmLog.Id},{alarmLog.DeviceName},{alarmLog.GroupName},{alarmLog.TagName},{alarmLog.TagValue},\"{ DateTimeOffset.FromUnixTimeMilliseconds(alarmLog.TriggeredAt).UtcDateTime.ToLocalTime().ToString("yyyy年MM月dd日 HH:mm:ss.fff")}\",{alarmLog.IsNoticed},{alarmLog.IsAlarmed},\"{msg}\"");
            }

            MessageBox.Show("导出成功");
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var tagLogList = _tagLogService.Selects(
                 _myTag.MyGroup.MyDevice.Name,
                 _myTag.MyGroup.Name,
                 _myTag.Name,
                 DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds(),
                 DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                 300
             );

            tagLogList.ForEach(tagLog =>
            {
                if (_tagLogQueue.Count > _maxTagLogCountInChart)
                {
                    _tagLogQueue.Dequeue();
                }

                _tagLogQueue.Enqueue(tagLog);
            });

            // 把队列数据取出来
            var tempTagLogList = _tagLogQueue.ToList();
            _tagLogQueue.Clear();

            double[] xs = tempTagLogList.Select(tagLog => DateTimeOffset.FromUnixTimeMilliseconds(tagLog.CollectedAt).LocalDateTime.ToOADate()).ToArray();
            double[] ys;

            if (_myTag.ValueType.ToUpper() == "BOOLEAN")
            {
                ys = tempTagLogList.Select(tagLog => tagLog.TagValue!.ToUpper() == "TRUE" ? 1d : 0d).ToArray();
            }
            else
            {
                ys = tempTagLogList.Select(tagLog => double.Parse(tagLog.TagValue!)).ToArray();
            }

            var fontName = "Microsoft YaHei";
            formsPlot1.Plot.Axes.Title.Label.FontName = fontName;       //图表标题
            formsPlot1.Plot.Axes.Bottom.Label.FontName = fontName;     //X轴
            formsPlot1.Plot.Axes.Left.Label.FontName = fontName;       //Y轴
            formsPlot1.Plot.Axes.Right.Label.FontName = fontName;      //右Y轴（如有）
            formsPlot1.Plot.Legend.FontName = fontName;                //图例

            formsPlot1.Plot.Title($"{_myTag.Name}实时趋势");
            formsPlot1.Plot.XLabel("采集时间");
            formsPlot1.Plot.YLabel(_myTag.Name);
            formsPlot1.Plot.Clear();
            // 核心正确API：ScatterLine，专门数组画连续折线
            var curve = formsPlot1.Plot.Add.ScatterLine(xs, ys);
            curve.LineWidth = 1.6f;
            curve.Color = Colors.DarkBlue;

            // X轴显示时间
            formsPlot1.Plot.Axes.DateTimeTicksBottom();
            formsPlot1.Plot.Grid.IsVisible = true;
            formsPlot1.Plot.Font.Automatic();

            formsPlot1.Refresh();
        }

        private void MyTagForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _uiTimer.Stop();
            _uiTimer.Dispose();
        }

        private async void btn_tag_update_Click(object? sender, EventArgs e)
        {
            if (_myTag.NeedToMonitor != cb_tag_need_to_monitor.Checked)
            {
                _myTag.NeedToMonitor = cb_tag_need_to_monitor.Checked;
            }

            if (_myTag.Value.ToString() != tb_tag_value.Text)
            {
                try
                {
                    object value = _myTag.ValueType.ToUpper() == "BOOLEAN" ? cb_tag_value.Checked : tb_tag_value.Text;

                    MyS7Task myS7Task = new()
                    {
                        MyS7TaskType = MyS7TaskType.WriteAsync,
                        IpAddress = _myTag.MyGroup.MyDevice.IpAddress,
                        //TaskCompletionSource = new(),
                        StartAddress = _myTag.StartAddress,
                        ValueType = _myTag.ValueType,
                        Value = value
                    };

                    _ = await MyS7Context.GetMyS7TaskResult(_sendQueue, myS7Task);

                    _myLogger.Log(LogLevel.Info, $"更新任务已经下发 ({_myTag.Value} -> {value})，请耐心等待任务执行结果");
                    MessageBox.Show($"更新任务已经下发 ({_myTag.Value} -> {value})，请耐心等待任务执行结果");

                    tc_tag_info.SelectedIndex = 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    _myLogger.Log(LogLevel.Error, $"发生错误:{ex.Message}", ex);
                }
            }
        }
    }
}
