namespace QuotesDownloader
{
    partial class QuotesDownloader
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label1;
            this.m_from = new System.Windows.Forms.Label();
            this.m_dateAndTimeFrom = new System.Windows.Forms.DateTimePicker();
            this.m_to = new System.Windows.Forms.Label();
            this.m_dateAndTimeTo = new System.Windows.Forms.DateTimePicker();
            this.m_browse = new System.Windows.Forms.Button();
            this.m_location = new System.Windows.Forms.Label();
            this.m_download = new System.Windows.Forms.Button();
            this.m_connection = new System.Windows.Forms.Button();
            this.m_groups = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.m_checkedListBox = new System.Windows.Forms.CheckedListBox();
            this.m_storageType = new System.Windows.Forms.ComboBox();
            this.m_quotesType = new System.Windows.Forms.ComboBox();
            this.m_log = new System.Windows.Forms.ListBox();
            this.m_LogContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.m_logContextMenuClearItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_logContextMenuSaveItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_connectionParameters = new System.Windows.Forms.GroupBox();
            this.m_port = new System.Windows.Forms.TextBox();
            this.m_portLabel = new System.Windows.Forms.Label();
            this.m_save = new System.Windows.Forms.Button();
            this.m_password = new System.Windows.Forms.TextBox();
            this.m_username = new System.Windows.Forms.TextBox();
            this.m_address = new System.Windows.Forms.TextBox();
            this.m_passwordLabel = new System.Windows.Forms.Label();
            this.m_usernameLabel = new System.Windows.Forms.Label();
            this.m_addressLabel = new System.Windows.Forms.Label();
            this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
            label1 = new System.Windows.Forms.Label();
            this.m_groups.SuspendLayout();
            this.m_LogContextMenu.SuspendLayout();
            this.m_connectionParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(561, 160);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(86, 17);
            label1.TabIndex = 1;
            label1.Text = "Output type:";
            // 
            // m_from
            // 
            this.m_from.AutoSize = true;
            this.m_from.Location = new System.Drawing.Point(784, 39);
            this.m_from.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_from.Name = "m_from";
            this.m_from.Size = new System.Drawing.Size(40, 17);
            this.m_from.TabIndex = 0;
            this.m_from.Text = "From";
            // 
            // m_dateAndTimeFrom
            // 
            this.m_dateAndTimeFrom.CustomFormat = "MMMM/dd/yyyy hh:mm:ss tt";
            this.m_dateAndTimeFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_dateAndTimeFrom.Location = new System.Drawing.Point(263, 17);
            this.m_dateAndTimeFrom.Margin = new System.Windows.Forms.Padding(4);
            this.m_dateAndTimeFrom.Name = "m_dateAndTimeFrom";
            this.m_dateAndTimeFrom.Size = new System.Drawing.Size(265, 22);
            this.m_dateAndTimeFrom.TabIndex = 1;
            // 
            // m_to
            // 
            this.m_to.AutoSize = true;
            this.m_to.Location = new System.Drawing.Point(195, 57);
            this.m_to.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_to.Name = "m_to";
            this.m_to.Size = new System.Drawing.Size(25, 17);
            this.m_to.TabIndex = 2;
            this.m_to.Text = "To";
            // 
            // m_dateAndTimeTo
            // 
            this.m_dateAndTimeTo.CustomFormat = "MMMM/dd/yyyy hh:mm:ss tt";
            this.m_dateAndTimeTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_dateAndTimeTo.Location = new System.Drawing.Point(263, 49);
            this.m_dateAndTimeTo.Margin = new System.Windows.Forms.Padding(4);
            this.m_dateAndTimeTo.Name = "m_dateAndTimeTo";
            this.m_dateAndTimeTo.Size = new System.Drawing.Size(265, 22);
            this.m_dateAndTimeTo.TabIndex = 3;
            // 
            // m_browse
            // 
            this.m_browse.Location = new System.Drawing.Point(621, 162);
            this.m_browse.Margin = new System.Windows.Forms.Padding(4);
            this.m_browse.Name = "m_browse";
            this.m_browse.Size = new System.Drawing.Size(100, 28);
            this.m_browse.TabIndex = 4;
            this.m_browse.Text = "Browse";
            this.m_browse.UseVisualStyleBackColor = true;
            this.m_browse.Click += new System.EventHandler(this.OnBrowse);
            // 
            // m_location
            // 
            this.m_location.AutoSize = true;
            this.m_location.Location = new System.Drawing.Point(784, 162);
            this.m_location.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_location.Name = "m_location";
            this.m_location.Size = new System.Drawing.Size(0, 17);
            this.m_location.TabIndex = 5;
            // 
            // m_download
            // 
            this.m_download.Location = new System.Drawing.Point(621, 101);
            this.m_download.Margin = new System.Windows.Forms.Padding(4);
            this.m_download.Name = "m_download";
            this.m_download.Size = new System.Drawing.Size(100, 28);
            this.m_download.TabIndex = 6;
            this.m_download.Text = "Download";
            this.m_download.UseVisualStyleBackColor = true;
            this.m_download.Click += new System.EventHandler(this.OnDownload);
            // 
            // m_connection
            // 
            this.m_connection.Location = new System.Drawing.Point(621, 39);
            this.m_connection.Margin = new System.Windows.Forms.Padding(4);
            this.m_connection.Name = "m_connection";
            this.m_connection.Size = new System.Drawing.Size(100, 28);
            this.m_connection.TabIndex = 8;
            this.m_connection.Text = "Connect";
            this.m_connection.UseVisualStyleBackColor = true;
            this.m_connection.Click += new System.EventHandler(this.OnConnection);
            // 
            // m_groups
            // 
            this.m_groups.Controls.Add(this.button1);
            this.m_groups.Controls.Add(this.progressBar1);
            this.m_groups.Controls.Add(this.m_checkedListBox);
            this.m_groups.Controls.Add(this.m_storageType);
            this.m_groups.Controls.Add(label1);
            this.m_groups.Controls.Add(this.m_quotesType);
            this.m_groups.Controls.Add(this.m_to);
            this.m_groups.Controls.Add(this.m_dateAndTimeTo);
            this.m_groups.Controls.Add(this.m_dateAndTimeFrom);
            this.m_groups.Location = new System.Drawing.Point(589, 15);
            this.m_groups.Margin = new System.Windows.Forms.Padding(4);
            this.m_groups.Name = "m_groups";
            this.m_groups.Padding = new System.Windows.Forms.Padding(4);
            this.m_groups.Size = new System.Drawing.Size(835, 196);
            this.m_groups.TabIndex = 10;
            this.m_groups.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(537, 31);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(111, 42);
            this.button1.TabIndex = 12;
            this.button1.Text = "select all";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(156, 86);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(180, 28);
            this.progressBar1.TabIndex = 11;
            this.progressBar1.Visible = false;
            this.progressBar1.Click += new System.EventHandler(this.progressBar1_Click);
            // 
            // m_checkedListBox
            // 
            this.m_checkedListBox.CheckOnClick = true;
            this.m_checkedListBox.FormattingEnabled = true;
            this.m_checkedListBox.Location = new System.Drawing.Point(656, 11);
            this.m_checkedListBox.Margin = new System.Windows.Forms.Padding(4);
            this.m_checkedListBox.Name = "m_checkedListBox";
            this.m_checkedListBox.Size = new System.Drawing.Size(159, 72);
            this.m_checkedListBox.TabIndex = 10;
            this.m_checkedListBox.SelectedIndexChanged += new System.EventHandler(this.m_checkedListBox_SelectedIndexChanged);
            // 
            // m_storageType
            // 
            this.m_storageType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_storageType.FormattingEnabled = true;
            this.m_storageType.Location = new System.Drawing.Point(656, 150);
            this.m_storageType.Margin = new System.Windows.Forms.Padding(4);
            this.m_storageType.Name = "m_storageType";
            this.m_storageType.Size = new System.Drawing.Size(160, 24);
            this.m_storageType.TabIndex = 2;
            this.m_storageType.SelectedIndexChanged += new System.EventHandler(this.m_storageType_SelectedIndexChanged);
            // 
            // m_quotesType
            // 
            this.m_quotesType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_quotesType.FormattingEnabled = true;
            this.m_quotesType.Items.AddRange(new object[] {
            "Ticks",
            "Level2",
            "VWAP",
            "Bid S1",
            "Bid S10",
            "Bid M1",
            "Bid M5",
            "Bid M15",
            "Bid M30",
            "Bid H1",
            "Bid H4",
            "Bid D1",
            "Bid W1",
            "Bid MN1",
            "Ask S1",
            "Ask S10",
            "Ask M1",
            "Ask M5",
            "Ask M15",
            "Ask M30",
            "Ask H1",
            "Ask H4",
            "Ask D1",
            "Ask W1",
            "Ask MN1"});
            this.m_quotesType.Location = new System.Drawing.Point(655, 111);
            this.m_quotesType.Margin = new System.Windows.Forms.Padding(4);
            this.m_quotesType.Name = "m_quotesType";
            this.m_quotesType.Size = new System.Drawing.Size(160, 24);
            this.m_quotesType.TabIndex = 0;
            this.m_quotesType.SelectedIndexChanged += new System.EventHandler(this.m_quotesType_SelectedIndexChanged);
            // 
            // m_log
            // 
            this.m_log.ContextMenuStrip = this.m_LogContextMenu;
            this.m_log.FormattingEnabled = true;
            this.m_log.HorizontalScrollbar = true;
            this.m_log.ItemHeight = 16;
            this.m_log.Location = new System.Drawing.Point(16, 218);
            this.m_log.Margin = new System.Windows.Forms.Padding(4);
            this.m_log.Name = "m_log";
            this.m_log.ScrollAlwaysVisible = true;
            this.m_log.Size = new System.Drawing.Size(1407, 372);
            this.m_log.TabIndex = 11;
            // 
            // m_LogContextMenu
            // 
            this.m_LogContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.m_LogContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_logContextMenuClearItem,
            this.m_logContextMenuSaveItem});
            this.m_LogContextMenu.Name = "m_LogContextMenu";
            this.m_LogContextMenu.Size = new System.Drawing.Size(113, 52);
            // 
            // m_logContextMenuClearItem
            // 
            this.m_logContextMenuClearItem.Name = "m_logContextMenuClearItem";
            this.m_logContextMenuClearItem.Size = new System.Drawing.Size(112, 24);
            this.m_logContextMenuClearItem.Text = "Clear";
            this.m_logContextMenuClearItem.Click += new System.EventHandler(this.OnLogClear);
            // 
            // m_logContextMenuSaveItem
            // 
            this.m_logContextMenuSaveItem.Name = "m_logContextMenuSaveItem";
            this.m_logContextMenuSaveItem.Size = new System.Drawing.Size(112, 24);
            this.m_logContextMenuSaveItem.Text = "Save";
            this.m_logContextMenuSaveItem.Click += new System.EventHandler(this.OnLogSave);
            // 
            // m_connectionParameters
            // 
            this.m_connectionParameters.Controls.Add(this.m_port);
            this.m_connectionParameters.Controls.Add(this.m_portLabel);
            this.m_connectionParameters.Controls.Add(this.m_save);
            this.m_connectionParameters.Controls.Add(this.m_password);
            this.m_connectionParameters.Controls.Add(this.m_username);
            this.m_connectionParameters.Controls.Add(this.m_address);
            this.m_connectionParameters.Controls.Add(this.m_passwordLabel);
            this.m_connectionParameters.Controls.Add(this.m_usernameLabel);
            this.m_connectionParameters.Controls.Add(this.m_addressLabel);
            this.m_connectionParameters.Location = new System.Drawing.Point(16, 6);
            this.m_connectionParameters.Margin = new System.Windows.Forms.Padding(4);
            this.m_connectionParameters.Name = "m_connectionParameters";
            this.m_connectionParameters.Padding = new System.Windows.Forms.Padding(4);
            this.m_connectionParameters.Size = new System.Drawing.Size(565, 204);
            this.m_connectionParameters.TabIndex = 12;
            this.m_connectionParameters.TabStop = false;
            this.m_connectionParameters.Text = "Connection parameters";
            // 
            // m_port
            // 
            this.m_port.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QuotesDownloader.Settings.Default, "Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.m_port.Location = new System.Drawing.Point(144, 57);
            this.m_port.Margin = new System.Windows.Forms.Padding(4);
            this.m_port.Name = "m_port";
            this.m_port.Size = new System.Drawing.Size(132, 22);
            this.m_port.TabIndex = 10;
            this.m_port.Text = global::QuotesDownloader.Settings.Default.Port;
            this.m_port.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnPortKeyDown);
            this.m_port.Validating += new System.ComponentModel.CancelEventHandler(this.OnPortValidating);
            // 
            // m_portLabel
            // 
            this.m_portLabel.AutoSize = true;
            this.m_portLabel.Location = new System.Drawing.Point(68, 65);
            this.m_portLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_portLabel.Name = "m_portLabel";
            this.m_portLabel.Size = new System.Drawing.Size(34, 17);
            this.m_portLabel.TabIndex = 9;
            this.m_portLabel.Text = "Port";
            // 
            // m_save
            // 
            this.m_save.Location = new System.Drawing.Point(144, 156);
            this.m_save.Margin = new System.Windows.Forms.Padding(4);
            this.m_save.Name = "m_save";
            this.m_save.Size = new System.Drawing.Size(100, 28);
            this.m_save.TabIndex = 8;
            this.m_save.Text = "Save";
            this.m_save.UseVisualStyleBackColor = true;
            this.m_save.Click += new System.EventHandler(this.OnSave);
            // 
            // m_password
            // 
            this.m_password.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QuotesDownloader.Settings.Default, "Password", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.m_password.Location = new System.Drawing.Point(144, 121);
            this.m_password.Margin = new System.Windows.Forms.Padding(4);
            this.m_password.Name = "m_password";
            this.m_password.Size = new System.Drawing.Size(375, 22);
            this.m_password.TabIndex = 7;
            this.m_password.Text = global::QuotesDownloader.Settings.Default.Password;
            this.m_password.UseSystemPasswordChar = true;
            // 
            // m_username
            // 
            this.m_username.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QuotesDownloader.Settings.Default, "Username", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.m_username.Location = new System.Drawing.Point(144, 89);
            this.m_username.Margin = new System.Windows.Forms.Padding(4);
            this.m_username.Name = "m_username";
            this.m_username.Size = new System.Drawing.Size(375, 22);
            this.m_username.TabIndex = 6;
            this.m_username.Text = global::QuotesDownloader.Settings.Default.Username;
            // 
            // m_address
            // 
            this.m_address.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QuotesDownloader.Settings.Default, "Address", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.m_address.Location = new System.Drawing.Point(144, 25);
            this.m_address.Margin = new System.Windows.Forms.Padding(4);
            this.m_address.Name = "m_address";
            this.m_address.Size = new System.Drawing.Size(375, 22);
            this.m_address.TabIndex = 5;
            this.m_address.Text = global::QuotesDownloader.Settings.Default.Address;
            // 
            // m_passwordLabel
            // 
            this.m_passwordLabel.AutoSize = true;
            this.m_passwordLabel.Location = new System.Drawing.Point(32, 129);
            this.m_passwordLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_passwordLabel.Name = "m_passwordLabel";
            this.m_passwordLabel.Size = new System.Drawing.Size(69, 17);
            this.m_passwordLabel.TabIndex = 3;
            this.m_passwordLabel.Text = "Password";
            // 
            // m_usernameLabel
            // 
            this.m_usernameLabel.AutoSize = true;
            this.m_usernameLabel.Location = new System.Drawing.Point(29, 97);
            this.m_usernameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_usernameLabel.Name = "m_usernameLabel";
            this.m_usernameLabel.Size = new System.Drawing.Size(73, 17);
            this.m_usernameLabel.TabIndex = 2;
            this.m_usernameLabel.Text = "Username";
            // 
            // m_addressLabel
            // 
            this.m_addressLabel.AutoSize = true;
            this.m_addressLabel.Location = new System.Drawing.Point(43, 33);
            this.m_addressLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_addressLabel.Name = "m_addressLabel";
            this.m_addressLabel.Size = new System.Drawing.Size(60, 17);
            this.m_addressLabel.TabIndex = 1;
            this.m_addressLabel.Text = "Address";
            // 
            // m_toolTip
            // 
            this.m_toolTip.ToolTipTitle = "Invalid port number";
            this.m_toolTip.Popup += new System.Windows.Forms.PopupEventHandler(this.m_toolTip_Popup);
            // 
            // QuotesDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1440, 603);
            this.Controls.Add(this.m_connectionParameters);
            this.Controls.Add(this.m_log);
            this.Controls.Add(this.m_connection);
            this.Controls.Add(this.m_download);
            this.Controls.Add(this.m_location);
            this.Controls.Add(this.m_browse);
            this.Controls.Add(this.m_from);
            this.Controls.Add(this.m_groups);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "QuotesDownloader";
            this.Text = "SoftFX Quotes Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnClosed);
            this.Load += new System.EventHandler(this.QuotesDownloader_Load);
            this.m_groups.ResumeLayout(false);
            this.m_groups.PerformLayout();
            this.m_LogContextMenu.ResumeLayout(false);
            this.m_connectionParameters.ResumeLayout(false);
            this.m_connectionParameters.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label m_from;
        private System.Windows.Forms.DateTimePicker m_dateAndTimeFrom;
        private System.Windows.Forms.Label m_to;
        private System.Windows.Forms.DateTimePicker m_dateAndTimeTo;
        private System.Windows.Forms.Button m_browse;
        private System.Windows.Forms.Label m_location;
        private System.Windows.Forms.Button m_download;
        private System.Windows.Forms.Button m_connection;
        private System.Windows.Forms.GroupBox m_groups;
        private System.Windows.Forms.ListBox m_log;
        private System.Windows.Forms.ContextMenuStrip m_LogContextMenu;
        private System.Windows.Forms.ToolStripMenuItem m_logContextMenuClearItem;
        private System.Windows.Forms.ToolStripMenuItem m_logContextMenuSaveItem;
        private System.Windows.Forms.ComboBox m_quotesType;
        private System.Windows.Forms.GroupBox m_connectionParameters;
        private System.Windows.Forms.TextBox m_password;
        private System.Windows.Forms.TextBox m_username;
        private System.Windows.Forms.TextBox m_address;
        private System.Windows.Forms.Label m_passwordLabel;
        private System.Windows.Forms.Label m_usernameLabel;
        private System.Windows.Forms.Label m_addressLabel;
        private System.Windows.Forms.Button m_save;
        private System.Windows.Forms.TextBox m_port;
        private System.Windows.Forms.Label m_portLabel;
        private System.Windows.Forms.ToolTip m_toolTip;
        private System.Windows.Forms.ComboBox m_storageType;
        private System.Windows.Forms.CheckedListBox m_checkedListBox;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
    }
}
