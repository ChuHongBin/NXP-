namespace Camerasimulation
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.groupBox_Preview = new System.Windows.Forms.GroupBox();
            this.labal_pictureBox_Preview = new System.Windows.Forms.PictureBox();
            this.listBox_ImageList = new System.Windows.Forms.ListBox();
            this.button_Save = new System.Windows.Forms.Button();
            this.checkedListBox_ShownOptions = new System.Windows.Forms.CheckedListBox();
            this.groupBox_ImageList = new System.Windows.Forms.GroupBox();
            this.groupBox_ShownOptions = new System.Windows.Forms.GroupBox();
            this.button_Play = new System.Windows.Forms.Button();
            this.groupBox_ImageInform = new System.Windows.Forms.GroupBox();
            this.label_ImageThreshold = new System.Windows.Forms.Label();
            this.label_ImageCount = new System.Windows.Forms.Label();
            this.label_ImageSize = new System.Windows.Forms.Label();
            this.label_ImageRodeType = new System.Windows.Forms.Label();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.timer_Play = new System.Windows.Forms.Timer(this.components);
            this.label_CarBattVol = new System.Windows.Forms.Label();
            this.label_CarRodeType = new System.Windows.Forms.Label();
            this.label_CarAimSpeed = new System.Windows.Forms.Label();
            this.groupBox_CarInform = new System.Windows.Forms.GroupBox();
            this.Speed_ = new System.Windows.Forms.Label();
            this.label_Left_K = new System.Windows.Forms.Label();
            this.label_Right_K = new System.Windows.Forms.Label();
            this.label_Middle_K = new System.Windows.Forms.Label();
            this.label_down_bias = new System.Windows.Forms.Label();
            this.label_up_bias = new System.Windows.Forms.Label();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenu_Open = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenu_Clear = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_SaveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenu_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.帮助ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStrip_Verion = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenu_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.StripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.StripStatusLabel_StandPoint = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripStatusLabel_RealPoint = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripStatusLabel1_codition = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_Space = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBox_Preview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.labal_pictureBox_Preview)).BeginInit();
            this.groupBox_ImageList.SuspendLayout();
            this.groupBox_ShownOptions.SuspendLayout();
            this.groupBox_ImageInform.SuspendLayout();
            this.groupBox_CarInform.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_Preview
            // 
            this.groupBox_Preview.Controls.Add(this.labal_pictureBox_Preview);
            resources.ApplyResources(this.groupBox_Preview, "groupBox_Preview");
            this.groupBox_Preview.Name = "groupBox_Preview";
            this.groupBox_Preview.TabStop = false;
            // 
            // labal_pictureBox_Preview
            // 
            this.labal_pictureBox_Preview.BackColor = System.Drawing.Color.White;
            this.labal_pictureBox_Preview.Cursor = System.Windows.Forms.Cursors.Arrow;
            resources.ApplyResources(this.labal_pictureBox_Preview, "labal_pictureBox_Preview");
            this.labal_pictureBox_Preview.Name = "labal_pictureBox_Preview";
            this.labal_pictureBox_Preview.TabStop = false;
            this.labal_pictureBox_Preview.Click += new System.EventHandler(this.pictureBox_Preview_Click);
            this.labal_pictureBox_Preview.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox_Preview_DragDrop);
            this.labal_pictureBox_Preview.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox_Preview_DragEnter);
            this.labal_pictureBox_Preview.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox_Preview_Paint);
            this.labal_pictureBox_Preview.MouseEnter += new System.EventHandler(this.pictureBox_Preview_MouseEnter);
            this.labal_pictureBox_Preview.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_Preview_MouseMove);
            // 
            // listBox_ImageList
            // 
            resources.ApplyResources(this.listBox_ImageList, "listBox_ImageList");
            this.listBox_ImageList.FormattingEnabled = true;
            this.listBox_ImageList.Name = "listBox_ImageList";
            this.listBox_ImageList.SelectedIndexChanged += new System.EventHandler(this.listBox_ImageList_SelectedIndexChanged);
            // 
            // button_Save
            // 
            resources.ApplyResources(this.button_Save, "button_Save");
            this.button_Save.Name = "button_Save";
            this.button_Save.UseVisualStyleBackColor = true;
            this.button_Save.Click += new System.EventHandler(this.button_Save_Click);
            // 
            // checkedListBox_ShownOptions
            // 
            this.checkedListBox_ShownOptions.CheckOnClick = true;
            resources.ApplyResources(this.checkedListBox_ShownOptions, "checkedListBox_ShownOptions");
            this.checkedListBox_ShownOptions.FormattingEnabled = true;
            this.checkedListBox_ShownOptions.Items.AddRange(new object[] {
            resources.GetString("checkedListBox_ShownOptions.Items"),
            resources.GetString("checkedListBox_ShownOptions.Items1"),
            resources.GetString("checkedListBox_ShownOptions.Items2"),
            resources.GetString("checkedListBox_ShownOptions.Items3"),
            resources.GetString("checkedListBox_ShownOptions.Items4"),
            resources.GetString("checkedListBox_ShownOptions.Items5"),
            resources.GetString("checkedListBox_ShownOptions.Items6"),
            resources.GetString("checkedListBox_ShownOptions.Items7")});
            this.checkedListBox_ShownOptions.Name = "checkedListBox_ShownOptions";
            this.checkedListBox_ShownOptions.SelectedIndexChanged += new System.EventHandler(this.checkedListBox_ShownOptions_SelectedIndexChanged);
            // 
            // groupBox_ImageList
            // 
            this.groupBox_ImageList.Controls.Add(this.listBox_ImageList);
            resources.ApplyResources(this.groupBox_ImageList, "groupBox_ImageList");
            this.groupBox_ImageList.Name = "groupBox_ImageList";
            this.groupBox_ImageList.TabStop = false;
            // 
            // groupBox_ShownOptions
            // 
            this.groupBox_ShownOptions.Controls.Add(this.checkedListBox_ShownOptions);
            resources.ApplyResources(this.groupBox_ShownOptions, "groupBox_ShownOptions");
            this.groupBox_ShownOptions.Name = "groupBox_ShownOptions";
            this.groupBox_ShownOptions.TabStop = false;
            // 
            // button_Play
            // 
            resources.ApplyResources(this.button_Play, "button_Play");
            this.button_Play.Name = "button_Play";
            this.button_Play.UseVisualStyleBackColor = true;
            this.button_Play.Click += new System.EventHandler(this.button_Play_Click);
            // 
            // groupBox_ImageInform
            // 
            this.groupBox_ImageInform.Controls.Add(this.label_ImageThreshold);
            this.groupBox_ImageInform.Controls.Add(this.label_ImageCount);
            this.groupBox_ImageInform.Controls.Add(this.label_ImageSize);
            this.groupBox_ImageInform.Controls.Add(this.label_ImageRodeType);
            resources.ApplyResources(this.groupBox_ImageInform, "groupBox_ImageInform");
            this.groupBox_ImageInform.Name = "groupBox_ImageInform";
            this.groupBox_ImageInform.TabStop = false;
            // 
            // label_ImageThreshold
            // 
            resources.ApplyResources(this.label_ImageThreshold, "label_ImageThreshold");
            this.label_ImageThreshold.Name = "label_ImageThreshold";
            // 
            // label_ImageCount
            // 
            resources.ApplyResources(this.label_ImageCount, "label_ImageCount");
            this.label_ImageCount.Name = "label_ImageCount";
            // 
            // label_ImageSize
            // 
            resources.ApplyResources(this.label_ImageSize, "label_ImageSize");
            this.label_ImageSize.Name = "label_ImageSize";
            // 
            // label_ImageRodeType
            // 
            resources.ApplyResources(this.label_ImageRodeType, "label_ImageRodeType");
            this.label_ImageRodeType.Name = "label_ImageRodeType";
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // timer_Play
            // 
            this.timer_Play.Tick += new System.EventHandler(this.timer_Play_Tick);
            // 
            // label_CarBattVol
            // 
            resources.ApplyResources(this.label_CarBattVol, "label_CarBattVol");
            this.label_CarBattVol.Name = "label_CarBattVol";
            // 
            // label_CarRodeType
            // 
            resources.ApplyResources(this.label_CarRodeType, "label_CarRodeType");
            this.label_CarRodeType.Name = "label_CarRodeType";
            // 
            // label_CarAimSpeed
            // 
            resources.ApplyResources(this.label_CarAimSpeed, "label_CarAimSpeed");
            this.label_CarAimSpeed.Name = "label_CarAimSpeed";
            // 
            // groupBox_CarInform
            // 
            this.groupBox_CarInform.Controls.Add(this.Speed_);
            this.groupBox_CarInform.Controls.Add(this.label_Left_K);
            this.groupBox_CarInform.Controls.Add(this.label_Right_K);
            this.groupBox_CarInform.Controls.Add(this.label_Middle_K);
            this.groupBox_CarInform.Controls.Add(this.label_down_bias);
            this.groupBox_CarInform.Controls.Add(this.label_up_bias);
            this.groupBox_CarInform.Controls.Add(this.label_CarAimSpeed);
            this.groupBox_CarInform.Controls.Add(this.label_CarRodeType);
            this.groupBox_CarInform.Controls.Add(this.label_CarBattVol);
            resources.ApplyResources(this.groupBox_CarInform, "groupBox_CarInform");
            this.groupBox_CarInform.Name = "groupBox_CarInform";
            this.groupBox_CarInform.TabStop = false;
            // 
            // Speed_
            // 
            resources.ApplyResources(this.Speed_, "Speed_");
            this.Speed_.Name = "Speed_";
            // 
            // label_Left_K
            // 
            resources.ApplyResources(this.label_Left_K, "label_Left_K");
            this.label_Left_K.Name = "label_Left_K";
            // 
            // label_Right_K
            // 
            resources.ApplyResources(this.label_Right_K, "label_Right_K");
            this.label_Right_K.Name = "label_Right_K";
            // 
            // label_Middle_K
            // 
            resources.ApplyResources(this.label_Middle_K, "label_Middle_K");
            this.label_Middle_K.Name = "label_Middle_K";
            // 
            // label_down_bias
            // 
            resources.ApplyResources(this.label_down_bias, "label_down_bias");
            this.label_down_bias.Name = "label_down_bias";
            // 
            // label_up_bias
            // 
            resources.ApplyResources(this.label_up_bias, "label_up_bias");
            this.label_up_bias.Name = "label_up_bias";
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.文件ToolStripMenuItem,
            this.帮助ToolStripMenuItem});
            resources.ApplyResources(this.menuStrip, "menuStrip");
            this.menuStrip.Name = "menuStrip";
            // 
            // 文件ToolStripMenuItem
            // 
            this.文件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenu_Open,
            this.ToolStripMenu_Clear,
            this.toolStripSeparator1,
            this.ToolStripMenuItem_SaveAll,
            this.toolStripSeparator2,
            this.ToolStripMenu_Exit});
            this.文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            resources.ApplyResources(this.文件ToolStripMenuItem, "文件ToolStripMenuItem");
            // 
            // ToolStripMenu_Open
            // 
            this.ToolStripMenu_Open.Name = "ToolStripMenu_Open";
            resources.ApplyResources(this.ToolStripMenu_Open, "ToolStripMenu_Open");
            this.ToolStripMenu_Open.Click += new System.EventHandler(this.ToolStripMenu_Open_Click);
            // 
            // ToolStripMenu_Clear
            // 
            this.ToolStripMenu_Clear.Name = "ToolStripMenu_Clear";
            resources.ApplyResources(this.ToolStripMenu_Clear, "ToolStripMenu_Clear");
            this.ToolStripMenu_Clear.Click += new System.EventHandler(this.ToolStripMenu_Clear_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // ToolStripMenuItem_SaveAll
            // 
            this.ToolStripMenuItem_SaveAll.Name = "ToolStripMenuItem_SaveAll";
            resources.ApplyResources(this.ToolStripMenuItem_SaveAll, "ToolStripMenuItem_SaveAll");
            this.ToolStripMenuItem_SaveAll.Click += new System.EventHandler(this.ToolStripMenuItem_SaveAll_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // ToolStripMenu_Exit
            // 
            this.ToolStripMenu_Exit.Name = "ToolStripMenu_Exit";
            resources.ApplyResources(this.ToolStripMenu_Exit, "ToolStripMenu_Exit");
            this.ToolStripMenu_Exit.Click += new System.EventHandler(this.ToolStripMenu_Exit_Click);
            // 
            // 帮助ToolStripMenuItem
            // 
            this.帮助ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStrip_Verion,
            this.toolStripSeparator4,
            this.ToolStripMenu_Help});
            this.帮助ToolStripMenuItem.Name = "帮助ToolStripMenuItem";
            resources.ApplyResources(this.帮助ToolStripMenuItem, "帮助ToolStripMenuItem");
            // 
            // ToolStrip_Verion
            // 
            this.ToolStrip_Verion.Name = "ToolStrip_Verion";
            resources.ApplyResources(this.ToolStrip_Verion, "ToolStrip_Verion");
            this.ToolStrip_Verion.Click += new System.EventHandler(this.ToolStrip_Verion_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // ToolStripMenu_Help
            // 
            this.ToolStripMenu_Help.Name = "ToolStripMenu_Help";
            resources.ApplyResources(this.ToolStripMenu_Help, "ToolStripMenu_Help");
            this.ToolStripMenu_Help.Click += new System.EventHandler(this.ToolStripMenu_Help_Click);
            // 
            // StripProgressBar
            // 
            resources.ApplyResources(this.StripProgressBar, "StripProgressBar");
            this.StripProgressBar.Name = "StripProgressBar";
            // 
            // StripStatusLabel_StandPoint
            // 
            this.StripStatusLabel_StandPoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.StripStatusLabel_StandPoint.Name = "StripStatusLabel_StandPoint";
            resources.ApplyResources(this.StripStatusLabel_StandPoint, "StripStatusLabel_StandPoint");
            // 
            // StripStatusLabel_RealPoint
            // 
            this.StripStatusLabel_RealPoint.Name = "StripStatusLabel_RealPoint";
            resources.ApplyResources(this.StripStatusLabel_RealPoint, "StripStatusLabel_RealPoint");
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StripProgressBar,
            this.toolStripProgressBar1,
            this.StripStatusLabel1_codition,
            this.toolStripStatusLabel_Space,
            this.StripStatusLabel_StandPoint,
            this.StripStatusLabel_RealPoint});
            resources.ApplyResources(this.statusStrip, "statusStrip");
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.SizingGrip = false;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            resources.ApplyResources(this.toolStripProgressBar1, "toolStripProgressBar1");
            // 
            // StripStatusLabel1_codition
            // 
            this.StripStatusLabel1_codition.Name = "StripStatusLabel1_codition";
            resources.ApplyResources(this.StripStatusLabel1_codition, "StripStatusLabel1_codition");
            this.StripStatusLabel1_codition.Click += new System.EventHandler(this.toolStripStatusLabel1_Click_1);
            // 
            // toolStripStatusLabel_Space
            // 
            this.toolStripStatusLabel_Space.Name = "toolStripStatusLabel_Space";
            resources.ApplyResources(this.toolStripStatusLabel_Space, "toolStripStatusLabel_Space");
            this.toolStripStatusLabel_Space.Spring = true;
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.groupBox_ImageInform);
            this.Controls.Add(this.groupBox_CarInform);
            this.Controls.Add(this.button_Play);
            this.Controls.Add(this.groupBox_ShownOptions);
            this.Controls.Add(this.groupBox_ImageList);
            this.Controls.Add(this.button_Save);
            this.Controls.Add(this.groupBox_Preview);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Activated += new System.EventHandler(this.FormMain_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.Layout += new System.Windows.Forms.LayoutEventHandler(this.FormMain_Layout);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.groupBox_Preview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.labal_pictureBox_Preview)).EndInit();
            this.groupBox_ImageList.ResumeLayout(false);
            this.groupBox_ShownOptions.ResumeLayout(false);
            this.groupBox_ImageInform.ResumeLayout(false);
            this.groupBox_ImageInform.PerformLayout();
            this.groupBox_CarInform.ResumeLayout(false);
            this.groupBox_CarInform.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox_Preview;
        private System.Windows.Forms.ListBox listBox_ImageList;
        private System.Windows.Forms.Button button_Save;
        private System.Windows.Forms.CheckedListBox checkedListBox_ShownOptions;
        private System.Windows.Forms.GroupBox groupBox_ImageList;
        private System.Windows.Forms.GroupBox groupBox_ShownOptions;
        private System.Windows.Forms.Button button_Play;
        private System.Windows.Forms.GroupBox groupBox_ImageInform;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Timer timer_Play;
        private System.Windows.Forms.Label label_ImageSize;
        private System.Windows.Forms.Label label_ImageRodeType;
        private System.Windows.Forms.Label label_CarBattVol;
        private System.Windows.Forms.Label label_CarRodeType;
        private System.Windows.Forms.Label label_CarAimSpeed;
        private System.Windows.Forms.GroupBox groupBox_CarInform;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem 文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenu_Open;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenu_Clear;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_SaveAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenu_Exit;
        private System.Windows.Forms.ToolStripMenuItem 帮助ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolStrip_Verion;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenu_Help;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.Label label_ImageCount;
        private System.Windows.Forms.Label label_ImageThreshold;
        private System.Windows.Forms.ToolStripProgressBar StripProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel StripStatusLabel_StandPoint;
        private System.Windows.Forms.ToolStripStatusLabel StripStatusLabel_RealPoint;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.Label label_up_bias;
        private System.Windows.Forms.Label label_down_bias;
        private System.Windows.Forms.PictureBox labal_pictureBox_Preview;
        private System.Windows.Forms.Label label_Middle_K;
        private System.Windows.Forms.Label label_Left_K;
        private System.Windows.Forms.Label label_Right_K;
        private System.Windows.Forms.Label Speed_;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Space;
        private System.Windows.Forms.ToolStripStatusLabel toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel StripStatusLabel1_codition;
    }
}

