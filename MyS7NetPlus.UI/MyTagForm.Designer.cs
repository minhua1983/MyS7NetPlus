namespace MyS7NetPlus.UI
{
    partial class MyTagForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tc_tag_info = new TabControl();
            tp_tag_config = new TabPage();
            cb_tag_value = new CheckBox();
            btn_tag_update = new Button();
            tb_tag_low_dead_band = new TextBox();
            tb_tag_low_threshold = new TextBox();
            tb_tag_high_dead_band = new TextBox();
            cb_tag_boolean_threshold = new CheckBox();
            tb_tag_high_threshold = new TextBox();
            cb_tag_need_to_monitor = new CheckBox();
            tb_tag_value = new TextBox();
            tb_tag_value_type = new TextBox();
            tb_tag_db_index = new TextBox();
            tb_tag_start_address = new TextBox();
            tb_tag_data_type = new TextBox();
            lbl_tag_data_type = new Label();
            tb_tag_name = new TextBox();
            lbl_tag_low_dead_band = new Label();
            lbl_tag_low_threshold = new Label();
            lbl_tag_high_dead_band = new Label();
            lbl_tag_high_threshold = new Label();
            lbl_tag_boolean_threshold = new Label();
            lbl_tag_need_to_monitor = new Label();
            lbl_tag_value = new Label();
            lbl_tag_db_index = new Label();
            lbl_tag_start_address = new Label();
            lbl_tag_value_type = new Label();
            lbl_tag_name = new Label();
            tp_tag_value_history = new TabPage();
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            tp_tag_alarm_history = new TabPage();
            lv_alarm_message = new ListView();
            ch_triggered_at = new ColumnHeader();
            ch_is_alarmed = new ColumnHeader();
            ch_message = new ColumnHeader();
            tc_tag_info.SuspendLayout();
            tp_tag_config.SuspendLayout();
            tp_tag_value_history.SuspendLayout();
            tp_tag_alarm_history.SuspendLayout();
            SuspendLayout();
            // 
            // tc_tag_info
            // 
            tc_tag_info.Controls.Add(tp_tag_config);
            tc_tag_info.Controls.Add(tp_tag_value_history);
            tc_tag_info.Controls.Add(tp_tag_alarm_history);
            tc_tag_info.Location = new Point(12, 12);
            tc_tag_info.Name = "tc_tag_info";
            tc_tag_info.SelectedIndex = 0;
            tc_tag_info.Size = new Size(776, 689);
            tc_tag_info.TabIndex = 0;
            // 
            // tp_tag_config
            // 
            tp_tag_config.Controls.Add(cb_tag_value);
            tp_tag_config.Controls.Add(btn_tag_update);
            tp_tag_config.Controls.Add(tb_tag_low_dead_band);
            tp_tag_config.Controls.Add(tb_tag_low_threshold);
            tp_tag_config.Controls.Add(tb_tag_high_dead_band);
            tp_tag_config.Controls.Add(cb_tag_boolean_threshold);
            tp_tag_config.Controls.Add(tb_tag_high_threshold);
            tp_tag_config.Controls.Add(cb_tag_need_to_monitor);
            tp_tag_config.Controls.Add(tb_tag_value);
            tp_tag_config.Controls.Add(tb_tag_value_type);
            tp_tag_config.Controls.Add(tb_tag_db_index);
            tp_tag_config.Controls.Add(tb_tag_start_address);
            tp_tag_config.Controls.Add(tb_tag_data_type);
            tp_tag_config.Controls.Add(lbl_tag_data_type);
            tp_tag_config.Controls.Add(tb_tag_name);
            tp_tag_config.Controls.Add(lbl_tag_low_dead_band);
            tp_tag_config.Controls.Add(lbl_tag_low_threshold);
            tp_tag_config.Controls.Add(lbl_tag_high_dead_band);
            tp_tag_config.Controls.Add(lbl_tag_high_threshold);
            tp_tag_config.Controls.Add(lbl_tag_boolean_threshold);
            tp_tag_config.Controls.Add(lbl_tag_need_to_monitor);
            tp_tag_config.Controls.Add(lbl_tag_value);
            tp_tag_config.Controls.Add(lbl_tag_db_index);
            tp_tag_config.Controls.Add(lbl_tag_start_address);
            tp_tag_config.Controls.Add(lbl_tag_value_type);
            tp_tag_config.Controls.Add(lbl_tag_name);
            tp_tag_config.Location = new Point(4, 29);
            tp_tag_config.Name = "tp_tag_config";
            tp_tag_config.Padding = new Padding(3);
            tp_tag_config.Size = new Size(768, 656);
            tp_tag_config.TabIndex = 0;
            tp_tag_config.Text = "点位配置";
            tp_tag_config.UseVisualStyleBackColor = true;
            // 
            // cb_tag_value
            // 
            cb_tag_value.AutoSize = true;
            cb_tag_value.Location = new Point(337, 193);
            cb_tag_value.Name = "cb_tag_value";
            cb_tag_value.Size = new Size(18, 17);
            cb_tag_value.TabIndex = 25;
            cb_tag_value.UseVisualStyleBackColor = true;
            // 
            // btn_tag_update
            // 
            btn_tag_update.Location = new Point(337, 396);
            btn_tag_update.Name = "btn_tag_update";
            btn_tag_update.Size = new Size(125, 29);
            btn_tag_update.TabIndex = 24;
            btn_tag_update.Text = "更新点位";
            btn_tag_update.UseVisualStyleBackColor = true;
            btn_tag_update.Click += btn_tag_update_Click;
            // 
            // tb_tag_low_dead_band
            // 
            tb_tag_low_dead_band.Location = new Point(337, 363);
            tb_tag_low_dead_band.Name = "tb_tag_low_dead_band";
            tb_tag_low_dead_band.Size = new Size(125, 27);
            tb_tag_low_dead_band.TabIndex = 23;
            // 
            // tb_tag_low_threshold
            // 
            tb_tag_low_threshold.Location = new Point(337, 333);
            tb_tag_low_threshold.Name = "tb_tag_low_threshold";
            tb_tag_low_threshold.Size = new Size(125, 27);
            tb_tag_low_threshold.TabIndex = 22;
            // 
            // tb_tag_high_dead_band
            // 
            tb_tag_high_dead_band.Location = new Point(337, 304);
            tb_tag_high_dead_band.Name = "tb_tag_high_dead_band";
            tb_tag_high_dead_band.Size = new Size(125, 27);
            tb_tag_high_dead_band.TabIndex = 21;
            // 
            // cb_tag_boolean_threshold
            // 
            cb_tag_boolean_threshold.AutoSize = true;
            cb_tag_boolean_threshold.Location = new Point(337, 252);
            cb_tag_boolean_threshold.Name = "cb_tag_boolean_threshold";
            cb_tag_boolean_threshold.Size = new Size(18, 17);
            cb_tag_boolean_threshold.TabIndex = 20;
            cb_tag_boolean_threshold.UseVisualStyleBackColor = true;
            // 
            // tb_tag_high_threshold
            // 
            tb_tag_high_threshold.Location = new Point(337, 275);
            tb_tag_high_threshold.Name = "tb_tag_high_threshold";
            tb_tag_high_threshold.Size = new Size(125, 27);
            tb_tag_high_threshold.TabIndex = 19;
            // 
            // cb_tag_need_to_monitor
            // 
            cb_tag_need_to_monitor.AutoSize = true;
            cb_tag_need_to_monitor.Location = new Point(337, 223);
            cb_tag_need_to_monitor.Name = "cb_tag_need_to_monitor";
            cb_tag_need_to_monitor.Size = new Size(18, 17);
            cb_tag_need_to_monitor.TabIndex = 18;
            cb_tag_need_to_monitor.UseVisualStyleBackColor = true;
            // 
            // tb_tag_value
            // 
            tb_tag_value.Location = new Point(337, 187);
            tb_tag_value.Name = "tb_tag_value";
            tb_tag_value.Size = new Size(125, 27);
            tb_tag_value.TabIndex = 17;
            // 
            // tb_tag_value_type
            // 
            tb_tag_value_type.Location = new Point(337, 158);
            tb_tag_value_type.Name = "tb_tag_value_type";
            tb_tag_value_type.Size = new Size(125, 27);
            tb_tag_value_type.TabIndex = 16;
            // 
            // tb_tag_db_index
            // 
            tb_tag_db_index.Location = new Point(337, 129);
            tb_tag_db_index.Name = "tb_tag_db_index";
            tb_tag_db_index.Size = new Size(125, 27);
            tb_tag_db_index.TabIndex = 15;
            // 
            // tb_tag_start_address
            // 
            tb_tag_start_address.Location = new Point(337, 100);
            tb_tag_start_address.Name = "tb_tag_start_address";
            tb_tag_start_address.Size = new Size(125, 27);
            tb_tag_start_address.TabIndex = 14;
            // 
            // tb_tag_data_type
            // 
            tb_tag_data_type.Location = new Point(337, 71);
            tb_tag_data_type.Name = "tb_tag_data_type";
            tb_tag_data_type.Size = new Size(125, 27);
            tb_tag_data_type.TabIndex = 13;
            // 
            // lbl_tag_data_type
            // 
            lbl_tag_data_type.AutoSize = true;
            lbl_tag_data_type.Location = new Point(244, 74);
            lbl_tag_data_type.Name = "lbl_tag_data_type";
            lbl_tag_data_type.Size = new Size(69, 20);
            lbl_tag_data_type.TabIndex = 12;
            lbl_tag_data_type.Text = "点位区域";
            // 
            // tb_tag_name
            // 
            tb_tag_name.Location = new Point(337, 41);
            tb_tag_name.Name = "tb_tag_name";
            tb_tag_name.Size = new Size(125, 27);
            tb_tag_name.TabIndex = 11;
            // 
            // lbl_tag_low_dead_band
            // 
            lbl_tag_low_dead_band.AutoSize = true;
            lbl_tag_low_dead_band.Location = new Point(244, 366);
            lbl_tag_low_dead_band.Name = "lbl_tag_low_dead_band";
            lbl_tag_low_dead_band.Size = new Size(69, 20);
            lbl_tag_low_dead_band.TabIndex = 10;
            lbl_tag_low_dead_band.Text = "下线死区";
            // 
            // lbl_tag_low_threshold
            // 
            lbl_tag_low_threshold.AutoSize = true;
            lbl_tag_low_threshold.Location = new Point(244, 336);
            lbl_tag_low_threshold.Name = "lbl_tag_low_threshold";
            lbl_tag_low_threshold.Size = new Size(69, 20);
            lbl_tag_low_threshold.TabIndex = 9;
            lbl_tag_low_threshold.Text = "下限阈值";
            // 
            // lbl_tag_high_dead_band
            // 
            lbl_tag_high_dead_band.AutoSize = true;
            lbl_tag_high_dead_band.Location = new Point(244, 307);
            lbl_tag_high_dead_band.Name = "lbl_tag_high_dead_band";
            lbl_tag_high_dead_band.Size = new Size(69, 20);
            lbl_tag_high_dead_band.TabIndex = 8;
            lbl_tag_high_dead_band.Text = "上线死区";
            // 
            // lbl_tag_high_threshold
            // 
            lbl_tag_high_threshold.AutoSize = true;
            lbl_tag_high_threshold.Location = new Point(244, 278);
            lbl_tag_high_threshold.Name = "lbl_tag_high_threshold";
            lbl_tag_high_threshold.Size = new Size(69, 20);
            lbl_tag_high_threshold.TabIndex = 7;
            lbl_tag_high_threshold.Text = "上限阈值";
            // 
            // lbl_tag_boolean_threshold
            // 
            lbl_tag_boolean_threshold.AutoSize = true;
            lbl_tag_boolean_threshold.Location = new Point(244, 249);
            lbl_tag_boolean_threshold.Name = "lbl_tag_boolean_threshold";
            lbl_tag_boolean_threshold.Size = new Size(69, 20);
            lbl_tag_boolean_threshold.TabIndex = 6;
            lbl_tag_boolean_threshold.Text = "布尔阈值";
            // 
            // lbl_tag_need_to_monitor
            // 
            lbl_tag_need_to_monitor.AutoSize = true;
            lbl_tag_need_to_monitor.Location = new Point(244, 220);
            lbl_tag_need_to_monitor.Name = "lbl_tag_need_to_monitor";
            lbl_tag_need_to_monitor.Size = new Size(69, 20);
            lbl_tag_need_to_monitor.TabIndex = 5;
            lbl_tag_need_to_monitor.Text = "需要监控";
            // 
            // lbl_tag_value
            // 
            lbl_tag_value.AutoSize = true;
            lbl_tag_value.Location = new Point(244, 190);
            lbl_tag_value.Name = "lbl_tag_value";
            lbl_tag_value.Size = new Size(69, 20);
            lbl_tag_value.TabIndex = 4;
            lbl_tag_value.Text = "当前数值";
            // 
            // lbl_tag_db_index
            // 
            lbl_tag_db_index.AutoSize = true;
            lbl_tag_db_index.Location = new Point(244, 132);
            lbl_tag_db_index.Name = "lbl_tag_db_index";
            lbl_tag_db_index.Size = new Size(69, 20);
            lbl_tag_db_index.TabIndex = 3;
            lbl_tag_db_index.Text = "数据块号";
            // 
            // lbl_tag_start_address
            // 
            lbl_tag_start_address.AutoSize = true;
            lbl_tag_start_address.Location = new Point(244, 103);
            lbl_tag_start_address.Name = "lbl_tag_start_address";
            lbl_tag_start_address.Size = new Size(69, 20);
            lbl_tag_start_address.TabIndex = 2;
            lbl_tag_start_address.Text = "寻址表达";
            // 
            // lbl_tag_value_type
            // 
            lbl_tag_value_type.AutoSize = true;
            lbl_tag_value_type.Location = new Point(244, 161);
            lbl_tag_value_type.Name = "lbl_tag_value_type";
            lbl_tag_value_type.Size = new Size(69, 20);
            lbl_tag_value_type.TabIndex = 1;
            lbl_tag_value_type.Text = "数值类型";
            // 
            // lbl_tag_name
            // 
            lbl_tag_name.AutoSize = true;
            lbl_tag_name.Location = new Point(244, 44);
            lbl_tag_name.Name = "lbl_tag_name";
            lbl_tag_name.Size = new Size(69, 20);
            lbl_tag_name.TabIndex = 0;
            lbl_tag_name.Text = "点位名称";
            // 
            // tp_tag_value_history
            // 
            tp_tag_value_history.Controls.Add(formsPlot1);
            tp_tag_value_history.Location = new Point(4, 29);
            tp_tag_value_history.Name = "tp_tag_value_history";
            tp_tag_value_history.Padding = new Padding(3);
            tp_tag_value_history.Size = new Size(768, 656);
            tp_tag_value_history.TabIndex = 1;
            tp_tag_value_history.Text = "数值曲线";
            tp_tag_value_history.UseVisualStyleBackColor = true;
            // 
            // formsPlot1
            // 
            formsPlot1.Location = new Point(3, 6);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(759, 644);
            formsPlot1.TabIndex = 0;
            // 
            // tp_tag_alarm_history
            // 
            tp_tag_alarm_history.Controls.Add(lv_alarm_message);
            tp_tag_alarm_history.Location = new Point(4, 29);
            tp_tag_alarm_history.Name = "tp_tag_alarm_history";
            tp_tag_alarm_history.Padding = new Padding(3);
            tp_tag_alarm_history.Size = new Size(768, 656);
            tp_tag_alarm_history.TabIndex = 2;
            tp_tag_alarm_history.Text = "报警记录";
            tp_tag_alarm_history.UseVisualStyleBackColor = true;
            // 
            // lv_alarm_message
            // 
            lv_alarm_message.Columns.AddRange(new ColumnHeader[] { ch_triggered_at, ch_is_alarmed, ch_message });
            lv_alarm_message.Location = new Point(6, 6);
            lv_alarm_message.Name = "lv_alarm_message";
            lv_alarm_message.Size = new Size(756, 644);
            lv_alarm_message.TabIndex = 0;
            lv_alarm_message.UseCompatibleStateImageBehavior = false;
            lv_alarm_message.View = View.Details;
            // 
            // ch_triggered_at
            // 
            ch_triggered_at.Text = "触发时间";
            ch_triggered_at.Width = 130;
            // 
            // ch_is_alarmed
            // 
            ch_is_alarmed.Text = "是否报警";
            // 
            // ch_message
            // 
            ch_message.Text = "报警信息";
            ch_message.Width = 380;
            // 
            // MyTagForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 713);
            Controls.Add(tc_tag_info);
            Name = "MyTagForm";
            Text = "点位信息";
            tc_tag_info.ResumeLayout(false);
            tp_tag_config.ResumeLayout(false);
            tp_tag_config.PerformLayout();
            tp_tag_value_history.ResumeLayout(false);
            tp_tag_alarm_history.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tc_tag_info;
        private TabPage tp_tag_config;
        private TabPage tp_tag_value_history;
        private TabPage tp_tag_alarm_history;
        private Label lbl_tag_name;
        private Label lbl_tag_start_address;
        private Label lbl_tag_value_type;
        private Label lbl_tag_value;
        private Label lbl_tag_db_index;
        private Label lbl_tag_low_threshold;
        private Label lbl_tag_high_dead_band;
        private Label lbl_tag_high_threshold;
        private Label lbl_tag_boolean_threshold;
        private Label lbl_tag_need_to_monitor;
        private Label lbl_tag_low_dead_band;
        private Label lbl_tag_data_type;
        private TextBox tb_tag_name;
        private TextBox tb_tag_db_index;
        private TextBox tb_tag_start_address;
        private TextBox tb_tag_data_type;
        private TextBox tb_tag_value_type;
        private CheckBox cb_tag_boolean_threshold;
        private TextBox tb_tag_high_threshold;
        private CheckBox cb_tag_need_to_monitor;
        private TextBox tb_tag_value;
        private Button btn_tag_update;
        private TextBox tb_tag_low_dead_band;
        private TextBox tb_tag_low_threshold;
        private TextBox tb_tag_high_dead_band;
        private CheckBox cb_tag_value;
        private ScottPlot.WinForms.FormsPlot formsPlot1;
        private ListView lv_alarm_message;
        private ColumnHeader ch_triggered_at;
        private ColumnHeader ch_is_alarmed;
        private ColumnHeader ch_message;
    }
}