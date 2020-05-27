namespace QuotesDownloader
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using TickTrader.FDK.Common;
    using TickTrader.FDK.Client;
    using System.Security.Cryptography.X509Certificates;
    using System.Net.Security;
    using TimeoutException = TickTrader.FDK.Common.TimeoutException;
    using System.Threading;
    using System.Collections.Generic;

    public partial class QuotesDownloader : Form
    {

        #region Members

        QuoteStore quoteClient;
        List<Downloader> downloadsList;
        bool isDownloaded = false;

        #endregion

        public QuotesDownloader()
        {
            downloadsList = new List<Downloader>();
            this.InitializeComponent();
            this.Text = string.Format("{0} (FDK {1})", this.Text, Library.Version);

            this.m_storageType.Items.Add("csv");
            this.m_storageType.Items.Add("hdf5");
            this.m_storageType.Items.Add("csv_zip");
            this.m_storageType.SelectedIndex = 0;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "Quotes");
            Directory.CreateDirectory(path);
            this.m_location.Text = path;
            this.m_quotesType.SelectedIndex = 0;

            var utcNow = DateTime.UtcNow;
            var to = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day);
            to = to.AddDays(-7);
            var from = to.AddDays(-7);
            this.m_dateAndTimeFrom.Value = from;
            this.m_dateAndTimeTo.Value = to;
            this.ApplyDisconnectedState();
        }

        void OnBrowse(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var status = dialog.ShowDialog();
            if (status == DialogResult.OK)
            {
                this.m_location.Text = dialog.SelectedPath;
            }
        }

        void OnPortValidating(object sender, CancelEventArgs e)
        {
            var st = this.m_port.Text;
            var port = (short)0;

            if (!short.TryParse(st, out port) || (port <= 0))
            {
                var message = string.Format("You should enter a positive number from 1 to {0}", short.MaxValue);
                this.m_toolTip.ToolTipTitle = "Invalid port number";
                this.m_toolTip.Show(message, this.m_port);
                e.Cancel = true;
            }
        }

        void OnPortKeyDown(object sender, KeyEventArgs e)
        {
            this.m_toolTip.Hide(this.m_port);
        }

        void OnLogClear(object sender, EventArgs e)
        {
            this.m_log.Items.Clear();
        }

        void OnLogSave(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "Log files|*.log"
            };

            var status = dialog.ShowDialog();
            if (status == DialogResult.OK)
            {
                using (var stream = new StreamWriter(dialog.FileName, false))
                {
                    foreach (var element in m_log.Items)
                    {
                        stream.WriteLine(element);
                    }
                }
            }
        }

        void OnConnection(object sender, EventArgs e)
        {
            if (this.quoteClient == null)
                this.Connect();
            else
                this.Disconnect();
        }

        void Connect()
        {
            try
            {
                this.Log("Quote Feed Client initialization");
                this.quoteClient = new QuoteStore("QuotesDownloader", port: Convert.ToInt32(this.m_port.Text), validateClientCertificate: (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
                this.quoteClient.LoginResultEvent += new QuoteStore.LoginResultDelegate(this.OnLogon);
                this.quoteClient.LogoutEvent += new QuoteStore.LogoutDelegate(this.OnLogout);
                this.quoteClient.DisconnectEvent += new QuoteStore.DisconnectDelegate(this.OnDisconnect);
                this.quoteClient.SymbolListResultEvent += new QuoteStore.SymbolListResultDelegate(this.OnSymbolInfo);
                this.Log("Connecting...");
                this.quoteClient.Connect(this.m_address.Text, 5000);
                this.quoteClient.Login(this.m_username.Text, this.m_password.Text, "", "", "", 5000);
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show("Connection timeout", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect();
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect();
                return;
            }

            this.ApplyConnectingState();
            quoteClient.GetSymbolList(-1);
        }

        void ApplyConnectingState()
        {
            this.m_connection.Text = "Disconnect";
            this.m_connectionParameters.Enabled = false;
        }

        void ApplyConnectedState()
        {
            this.m_download.Enabled = true;
        }

        void Disconnect()
        {
            try
            {
                this.Log("Disconnecting...");

                if (quoteClient != null)
                {
                    this.quoteClient.Disconnect("Disconnecting");
                    this.quoteClient.Dispose();
                    this.quoteClient = null;
                }

                this.Log("Quote Feed Client is disconnected");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.ApplyDisconnectedState();
        }

        void ApplyDisconnectedState()
        {
            this.m_connection.Text = "Connect";
            this.m_connectionParameters.Enabled = true;
            this.m_browse.Enabled = true;
            this.m_download.Enabled = false;
            this.m_checkedListBox.Items.Clear();
        }

        #region Client Events

        void OnLogon(QuoteStore quoteFeedClient, object sender)
        {
            if (this.InvokeRequired)
            {
                this.InvokeInPrimaryThread(OnLogon, quoteFeedClient, sender);
                return;
            }
            this.Log("Quote Feed Client is connected");
        }

        void OnDisconnect(QuoteStore quoteFeedClient, string text)
        {
            if (this.InvokeRequired)
            {
                this.InvokeInPrimaryThread(OnDisconnect, quoteFeedClient, text);
                return;
            }

            try
            {
                Console.WriteLine("Disconnected : " + text);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error : " + exception.Message);
            }
        }

        public void OnLogout(QuoteStore quoteFeedClient, LogoutInfo info)
        {
            if (this.InvokeRequired)
            {
                this.InvokeInPrimaryThread(OnLogout, quoteFeedClient, info);
                return;
            }

            try
            {
                Console.WriteLine("Logout : " + info.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error : " + exception.Message);
            }
        }

        void OnSymbolInfo(QuoteStore quoteFeedClient, object sender, string[] symbols)
        {

            if (this.InvokeRequired)
            {
                this.InvokeInPrimaryThread(OnSymbolInfo, quoteFeedClient, sender, symbols);
                return;
            }

            this.m_checkedListBox.Items.Clear();
            foreach (var symbol in symbols)
            {
                this.m_checkedListBox.Items.Add(symbol);
            }
            this.Log("Symbols information is received");
            this.ApplyConnectedState();
        }

        #endregion

        #region Form Events

        void OnDownload(object sender, EventArgs e)
        {
            if(isDownloaded)
            {
                foreach (var download in downloadsList)
                {
                    download.CancelDownload();
                }
                this.m_download.Text = "Download";
                progressBar1.Visible = false;
                this.m_browse.Enabled = true;
                this.m_checkedListBox.Enabled = true;
                this.m_quotesType.Enabled = true;
                this.m_storageType.Enabled = true;
                downloadsList.Clear();
                isDownloaded = false;
                return;
            }
            isDownloaded = true;
            this.m_download.Text = "Break";
            this.m_browse.Enabled = false;
            this.m_checkedListBox.Enabled = false;
            this.m_quotesType.Enabled = false;
            this.m_storageType.Enabled = false;
            this.progressBar1.Visible = true;
            this.progressBar1.Enabled = true;
            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = m_checkedListBox.CheckedItems.Count;
            this.progressBar1.Step = 1;
            this.progressBar1.Value = 0;
            for (int i = 0; i <  m_checkedListBox.CheckedItems.Count; i++)
            {
                DownloadQuote(m_checkedListBox.CheckedItems[i].ToString());
            }
            foreach(var download in downloadsList)
            {
                download.Start();
            }
        }

        void DownloadQuote(string quoteSymbol)
        {
            //if (this.downloader == null)
            {
                var outputType = m_storageType.SelectedItem.ToString();
                var location = this.m_location.Text;
                var symbol = quoteSymbol;
                var from = this.m_dateAndTimeFrom.Value;
                var to = this.m_dateAndTimeTo.Value;
                Downloader downloader;
                if (this.m_quotesType.SelectedIndex == 0)
                {
                    downloader = new Downloader(quoteClient, outputType, location, symbol, from, to);
                }
                else if (this.m_quotesType.SelectedIndex == 1)
                {
                    downloader = new Downloader(quoteClient, outputType, location, symbol, from, to, true);
                }
                else if (this.m_quotesType.SelectedIndex == 2)
                {
                    downloader = new Downloader(quoteClient, outputType, location, symbol, from, to, false, true);
                }
                else
                {
                    var st = this.m_quotesType.SelectedItem.ToString();
                    var match = Regex.Match(st, "^(Bid|Ask) ([^ ]+)$");
                    if (!match.Success)
                    {
                        var message = string.Format("Unexpected string format = {0}", st);
                        throw new ArgumentException(message);
                    }
                    var stPriceType = match.Groups[1].Value;
                    var priceType = ("Bid" == stPriceType) ? PriceType.Bid : PriceType.Ask;

                    var stBarPeriod = match.Groups[2].Value;

                    var barPeriod = new BarPeriod(stBarPeriod);
                    downloader = new Downloader(quoteClient, outputType, location, symbol, from, to, priceType, barPeriod);

                }

                downloader.Message += this.OnMessage;
                downloader.Finish += this.OnFinish;
                downloadsList.Add(downloader);
            }
        }

        void OnFinish(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.InvokeInPrimaryThread(OnFinish, sender, e);
                return;
            }
            if (sender != null)
            {
                ((Downloader)sender).Join();
            }
            progressBar1.PerformStep();
            if(progressBar1.Value == m_checkedListBox.CheckedItems.Count)
            {
                this.m_download.Text = "Download";
                progressBar1.Visible = false;
                this.m_browse.Enabled = true;
                this.m_checkedListBox.Enabled = true;
                this.m_quotesType.Enabled = true;
                this.m_storageType.Enabled = true;
                downloadsList.Clear();
                isDownloaded = false;
            }
        }

        void OnMessage(object sender, EventArgs<string> e)
        {
            if (this.InvokeRequired)
            {
                this.InvokeInPrimaryThread(OnMessage, sender, e);
                return;
            }

            this.Log(e.Value);
        }

        void OnClosed(object sender, FormClosedEventArgs e)
        {
            if (this.quoteClient != null)
            {
                this.quoteClient.Disconnect("QuotesDownloader");
                this.quoteClient.Dispose();
            }
        }

        void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (isDownloaded)
            {
                e.Cancel = true;
                MessageBox.Show("You should stop quotes downloading and wait for finish of background synchronization");
            }
        }

        void OnSave(object sender, EventArgs e)
        {
            Settings.Default.Save();
        }

        #endregion

        #region Helper Functions

        void Log(string text)
        {
            if (this.InvokeRequired)
            {
                this.InvokeInPrimaryThread(Log, text);
                return;
            }
            var st = string.Format("{0} - {1}", DateTime.Now, text);
            this.m_log.Items.Add(st);
            this.m_log.SelectedIndex = m_log.Items.Count - 1;
        }

        void Log(string format, params object[] arguments)
        {
            var text = string.Format(format, arguments);
            this.Log(text);
        }

        void InvokeInPrimaryThread(Action func)
        {
            this.DoInvokeInPrimaryThread(func);
        }

        void InvokeInPrimaryThread<A0>(Action<A0> func, A0 a0)
        {
            this.DoInvokeInPrimaryThread(func, a0);
        }

        void InvokeInPrimaryThread<A0, A1>(Action<A0, A1> func, A0 a0, A1 a1)
        {
            this.DoInvokeInPrimaryThread(func, a0, a1);
        }

        void InvokeInPrimaryThread<A0, A1, A2>(Action<A0, A1, A2> func, A0 a0, A1 a1, A2 a2)
        {
            this.DoInvokeInPrimaryThread(func, a0, a1, a2);
        }

        void DoInvokeInPrimaryThread(Delegate func, params object[] arguments)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(func, arguments);
            }
            else
            {
                func.DynamicInvoke(arguments);
            }
        }

        #endregion

        private void m_symbols_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void QuotesDownloader_Load(object sender, EventArgs e)
        {

        }

        private void m_toolTip_Popup(object sender, PopupEventArgs e)
        {

        }

        private void m_checkedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
