namespace MyS7NetPlus.UI
{
    partial class MyForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btn_connect = new Button();
            lv_log_message = new ListView();
            ch_log_time = new ColumnHeader();
            ch_log_level = new ColumnHeader();
            ch_log_message = new ColumnHeader();
            tc_groups = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            lbl_persisted_tag_log_count = new Label();
            lbl_collected_tag_log_count = new Label();
            lbl_persist_collect_rate = new Label();
            tc_groups.SuspendLayout();
            SuspendLayout();
            // 
            // btn_connect
            // 
            btn_connect.Location = new Point(12, 12);
            btn_connect.Name = "btn_connect";
            btn_connect.Size = new Size(94, 29);
            btn_connect.TabIndex = 0;
            btn_connect.Text = "连接";
            btn_connect.UseVisualStyleBackColor = true;
            btn_connect.Click += btn_connect_Click;
            // 
            // lv_log_message
            // 
            lv_log_message.Columns.AddRange(new ColumnHeader[] { ch_log_time, ch_log_level, ch_log_message });
            lv_log_message.Location = new Point(12, 417);
            lv_log_message.Name = "lv_log_message";
            lv_log_message.Size = new Size(776, 284);
            lv_log_message.TabIndex = 3;
            lv_log_message.UseCompatibleStateImageBehavior = false;
            lv_log_message.View = View.Details;
            // 
            // ch_log_time
            // 
            ch_log_time.Text = "日志时间";
            ch_log_time.Width = 130;
            // 
            // ch_log_level
            // 
            ch_log_level.Text = "日志级别";
            // 
            // ch_log_message
            // 
            ch_log_message.Text = "日志消息";
            ch_log_message.Width = 380;
            // 
            // tc_groups
            // 
            tc_groups.Controls.Add(tabPage1);
            tc_groups.Controls.Add(tabPage2);
            tc_groups.Location = new Point(12, 47);
            tc_groups.Name = "tc_groups";
            tc_groups.SelectedIndex = 0;
            tc_groups.Size = new Size(776, 365);
            tc_groups.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(768, 332);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(768, 332);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // lbl_persisted_tag_log_count
            // 
            lbl_persisted_tag_log_count.AutoSize = true;
            lbl_persisted_tag_log_count.Location = new Point(231, 9);
            lbl_persisted_tag_log_count.Name = "lbl_persisted_tag_log_count";
            lbl_persisted_tag_log_count.RightToLeft = RightToLeft.No;
            lbl_persisted_tag_log_count.Size = new Size(18, 20);
            lbl_persisted_tag_log_count.TabIndex = 6;
            lbl_persisted_tag_log_count.Text = "0";
            lbl_persisted_tag_log_count.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lbl_collected_tag_log_count
            // 
            lbl_collected_tag_log_count.AutoSize = true;
            lbl_collected_tag_log_count.Location = new Point(231, 29);
            lbl_collected_tag_log_count.Name = "lbl_collected_tag_log_count";
            lbl_collected_tag_log_count.Size = new Size(18, 20);
            lbl_collected_tag_log_count.TabIndex = 8;
            lbl_collected_tag_log_count.Text = "0";
            lbl_collected_tag_log_count.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lbl_persist_collect_rate
            // 
            lbl_persist_collect_rate.AutoSize = true;
            lbl_persist_collect_rate.Location = new Point(141, 16);
            lbl_persist_collect_rate.Name = "lbl_persist_collect_rate";
            lbl_persist_collect_rate.Size = new Size(84, 20);
            lbl_persist_collect_rate.TabIndex = 9;
            lbl_persist_collect_rate.Text = "入库采集比";
            // 
            // MyForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 713);
            Controls.Add(lbl_persist_collect_rate);
            Controls.Add(lbl_collected_tag_log_count);
            Controls.Add(lbl_persisted_tag_log_count);
            Controls.Add(tc_groups);
            Controls.Add(lv_log_message);
            Controls.Add(btn_connect);
            Name = "MyForm";
            Text = "MyS7NetPlus demo";
            tc_groups.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_connect;
        private ListView lv_log_message;
        private ColumnHeader ch_log_time;
        private ColumnHeader ch_log_level;
        private ColumnHeader ch_log_message;
        private TabControl tc_groups;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Label lbl_persisted_tag_log_count;
        private Label lbl_collected_tag_log_count;
        private Label lbl_persist_collect_rate;
    }
}
