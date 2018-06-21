namespace QuotesDownloader
{
    using System;
    using System.IO;
    using System.Threading;
    using TickTrader.FDK.Common;
    using TickTrader.FDK.QuoteStore;
    using System.Text;
    using System.Globalization;
    using HDF5DotNet;
    using System.Collections.Generic;

    public class Downloader
    {
        Downloader()
        {
            this.thread = new Thread(ThreadMethod);
        }

        public Downloader(Client quoteClient, string outputType, String location, String symbol, DateTime from, DateTime to, Boolean includeLevel2)
            : this()
        {
            this.quoteClient = quoteClient;
            this.outputType = outputType;
            this.location = location;
            this.symbol = symbol;
            this.from = from;
            this.to = to;
            this.includeLevel2 = includeLevel2;
        }

        public Downloader(Client quoteClient, string outputType, String location, String symbol, DateTime from, DateTime to, PriceType priceType, BarPeriod period)
            : this()
        {
            this.quoteClient = quoteClient;
            this.outputType = outputType;
            this.location = location;
            this.symbol = symbol;
            this.from = from;
            this.to = to;
            this.priceType = priceType;
            this.period = period;
        }

        #region Properties

        public bool IsFinished
        {
            get
            {
                return this.thread == null;
            }
        }

        #endregion

        #region Control Methods

        public void Start()
        {
            this.thread.Start();
        }

        public void Join()
        {
            var thread = this.thread;
            if (thread != null)
            {
                thread.Join();
            }
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs<string>> Message;
        public event EventHandler<EventArgs> Finish;

        #endregion

        #region Helper Methods

        void ThreadMethod()
        {
            try
            {
                var from = this.from;
                var to = this.to;
                Directory.CreateDirectory(this.location);

                if (period == null)
                    DownloadQuotes();
                else
                    DownloadBars();
            }
            catch (Exception ex)
            {
                this.Log(ex.Message);
            }
            this.RaiseFinish();
            this.thread = null;
        }

        void DownloadQuotes()
        {
            QuoteDepth marketDepth = includeLevel2 ? QuoteDepth.Level2 : QuoteDepth.Top;
            DownloadQuotesEnumerator enumerator = quoteClient.DownloadQuotes(symbol, marketDepth, from, to, -1);
            string path = Path.Combine(this.location, string.Format("{0}{1}{2}{3}.txt", symbol, includeLevel2 ? " level2" : "", from.ToString(" yyyyMMdd"), to.ToString(" yyyyMMdd")));
            if (outputType == "text")
            {
                using (StreamWriter file = File.CreateText(path))
                {
                    for (Quote quote = enumerator.Next(-1); quote != null; quote = enumerator.Next(-1))
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append(quote.CreatingTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                        builder.Append(" bid ");
                        foreach (QuoteEntry entry in quote.Bids)
                            builder.AppendFormat("{0} {1} ", entry.Price, entry.Volume);
                        builder.Append(" ask ");
                        foreach (QuoteEntry entry in quote.Asks)
                            builder.AppendFormat("{0} {1} ", entry.Price, entry.Volume);
                        file.WriteLine(builder);
                    }
                }
                this.Log("Quotes are downloaded successfully");
            }
            else
            {
                H5FileId fileId = H5F.create(path, H5F.CreateMode.ACC_TRUNC);
               
                var quotesData = new List<Quote>();
                for (Quote quote = enumerator.Next(-1); quote != null; quote = enumerator.Next(-1))
                    quotesData.Add(quote);

                if (quotesData.Count == 0)
                    return;

                double[,,] quotesArray = new double[quotesData.Count, 2, quotesData[0].Bids.Count * 2];
                string[] dateQuotesArray = new string[quotesData.Count];
                for (int i = 0; i < quotesData.Count; i++)
                {
                    dateQuotesArray[i] = quotesData[i].CreatingTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                    int j = 0;
                    foreach (QuoteEntry entry in quotesData[i].Bids)
                    {
                        quotesArray[i, 0, j] = entry.Price;
                        quotesArray[i, 0, j+1] = entry.Volume;
                        j += 2;
                    }
                    j = 0;
                    foreach (QuoteEntry entry in quotesData[i].Asks)
                    {
                        quotesArray[i, 1, j] = entry.Price;
                        quotesArray[i, 1, j + 1] = entry.Volume;
                        j += 2;
                    }
                }
                H5DataTypeId quotesTypeId = new H5DataTypeId(H5T.H5Type.NATIVE_DOUBLE);
                H5DataSpaceId quotesSpaceId = H5S.create_simple(3, new long[] { quotesData.Count, 2, quotesData[0].Asks.Count * 2 }, new long[] { (long)H5S.H5SType.UNLIMITED, 2, (long)H5S.H5SType.UNLIMITED });
                H5PropertyListId createChunked = H5P.create(H5P.PropertyListClass.DATASET_CREATE);
                H5PropertyListId linkCreationDefaults = H5P.create(H5P.PropertyListClass.LINK_CREATE);
                H5PropertyListId accessCreationDefaults = H5P.create(H5P.PropertyListClass.DATASET_ACCESS);
                H5P.setChunk(createChunked, new long[] { 1, 2, 2 });
                H5DataSetId quotesSetId = H5D.create(fileId, "Quotes", quotesTypeId, quotesSpaceId, linkCreationDefaults, createChunked, accessCreationDefaults);

                H5D.setExtent(quotesSetId, new long[] { quotesData.Count, 2, quotesData[0].Asks.Count * 2 });
                H5DataSpaceId newSpace = H5D.getSpace(quotesSetId);
                H5S.selectHyperslab(newSpace, H5S.SelectOperator.SET, new long[] { 0, 0, 0 }, new long[] { quotesData.Count, 2, quotesData[0].Asks.Count * 2 });
                H5D.write(quotesSetId, quotesTypeId, new H5Array<double>(quotesArray));

                H5P.close(createChunked);
                H5P.close(linkCreationDefaults);
                H5P.close(accessCreationDefaults);
                H5S.close(newSpace);
                H5S.close(quotesSpaceId);
                H5D.close(quotesSetId);

                H5DataTypeId dateQuotesTypeId = H5T.create(H5T.CreateClass.STRING, 8);
                H5DataSpaceId dateQuotesSpaceId = H5S.create_simple(1, new long[] { quotesData.Count }, new long[] { (long)H5S.H5SType.UNLIMITED });
                createChunked = H5P.create(H5P.PropertyListClass.DATASET_CREATE);
                linkCreationDefaults = H5P.create(H5P.PropertyListClass.LINK_CREATE);
                accessCreationDefaults = H5P.create(H5P.PropertyListClass.DATASET_ACCESS);
                H5P.setChunk(createChunked, new long[] { 1 });
                H5DataSetId dateQuotesSetId = H5D.create(fileId, "DateQuotes", dateQuotesTypeId, dateQuotesSpaceId, linkCreationDefaults, createChunked, accessCreationDefaults);

                H5D.setExtent(dateQuotesSetId, new long[] { quotesData.Count });
                newSpace = H5D.getSpace(dateQuotesSetId);
                H5S.selectHyperslab(newSpace, H5S.SelectOperator.SET, new long[] { 0 }, new long[] { quotesData.Count });
                H5D.write(dateQuotesSetId, dateQuotesTypeId, new H5Array<string>(dateQuotesArray));

                H5P.close(createChunked);
                H5P.close(linkCreationDefaults);
                H5P.close(accessCreationDefaults);
                H5S.close(newSpace);
                H5S.close(dateQuotesSpaceId);
                H5D.close(dateQuotesSetId);
                H5T.close(dateQuotesTypeId);

                H5F.close(fileId);
                this.Log("Quotes are downloaded successfully");
            }
        }

        void DownloadBars()
        {
            DownloadBarsEnumerator enumerator = quoteClient.DownloadBars(symbol, priceType, period, from, to, -1);
            string path = Path.Combine(this.location, string.Format("{0} {1} {2} {3}.h5", symbol, period, from.ToString(" yyyyMMdd"), to.ToString(" yyyyMMdd")));
            if (outputType == "text")
            {
                using (StreamWriter file = File.CreateText(path))
                {
                    for (Bar bar = enumerator.Next(-1); bar != null; bar = enumerator.Next(-1))
                        file.WriteLine(string.Format("{0} {1} {2} {3} {4} {5}", bar.From.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), bar.Open, bar.Close, bar.Low, bar.High, bar.Volume));
                }
                this.Log("Bars are downloaded successfully");
            }
            else
            {
                H5FileId fileId = H5F.create(path, H5F.CreateMode.ACC_TRUNC);
               
                var barsData = new List<Bar>();
                for (Bar bar = enumerator.Next(-1); bar != null; bar = enumerator.Next(-1))
                    barsData.Add(bar);

                if (barsData.Count == 0)
                    return;

                double[,] barsArray = new double[barsData.Count, 5];
                string[,] dataBarsArray = new string[barsData.Count, 2];
                string priceTypeString = "Bid";
                if (priceType == PriceType.Ask)
                    priceTypeString = "Ask";
                for (int i=0; i< barsData.Count; i++)
                {
                    dataBarsArray[i, 0] = priceTypeString;
                    dataBarsArray[i, 1] = barsData[i].From.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                    barsArray[i, 0] = barsData[i].Volume;
                    barsArray[i, 1] = barsData[i].Open;
                    barsArray[i, 2] = barsData[i].Close;
                    barsArray[i, 3] = barsData[i].High;
                    barsArray[i, 4] = barsData[i].Low;
                }

                H5DataTypeId barsTypeId = new H5DataTypeId(H5T.H5Type.NATIVE_DOUBLE);
                WriteDataToNewFile(fileId, "Bars", barsArray, barsData.Count, 5, barsTypeId);

                H5DataTypeId dataBarsTypeId = H5T.create(H5T.CreateClass.STRING, 8);
                WriteDataToNewFile(fileId, "DataBars", dataBarsArray, barsData.Count, 2, dataBarsTypeId);

                H5F.close(fileId);
                this.Log("Bars are downloaded successfully");
            }
        }

        void WriteDataToNewFile<T>(H5FileId fileId, string datasetName, T[,] barsData, long count, long parametersNumber, H5DataTypeId datatypeId)
        {
            H5DataSpaceId dataspaceId = H5S.create_simple(2, new long[] { count, parametersNumber }, new long[] { (long)H5S.H5SType.UNLIMITED, parametersNumber });
            H5PropertyListId createChunked = H5P.create(H5P.PropertyListClass.DATASET_CREATE);
            H5PropertyListId linkCreationDefaults = H5P.create(H5P.PropertyListClass.LINK_CREATE);
            H5PropertyListId accessCreationDefaults = H5P.create(H5P.PropertyListClass.DATASET_ACCESS);
            H5P.setChunk(createChunked, new long[] { 1, parametersNumber });
            H5DataSetId datasetId = H5D.create(fileId, datasetName, datatypeId, dataspaceId, linkCreationDefaults, createChunked, accessCreationDefaults);

            H5D.setExtent(datasetId, new long[] { count, parametersNumber });
            H5DataSpaceId newSpace = H5D.getSpace(datasetId);
            H5S.selectHyperslab(newSpace, H5S.SelectOperator.SET, new long[] { 0, 0 }, new long[] {count, parametersNumber });
            H5D.write(datasetId, datatypeId, new H5Array<T>(barsData));

            H5P.close(createChunked);
            H5P.close(linkCreationDefaults);
            H5P.close(accessCreationDefaults);
            H5S.close(newSpace);
            H5S.close(dataspaceId);
            H5D.close(datasetId);
        }

        void Log(string text)
        {
            var eh = this.Message;

            if (eh != null)
            {
                var e = new EventArgs<string>(text);
                eh(this, e);
            }
        }

        void Log(string format, params object[] arguments)
        {
            var text = string.Format(format, arguments);
            this.Log(text);
        }

        void RaiseFinish()
        {
            var eh = this.Finish;
            if (eh != null)
            {
                eh(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Members

        readonly Client quoteClient;
        readonly string outputType;
        readonly string location;
        readonly string symbol;
        readonly DateTime from;
        readonly DateTime to;
        readonly Boolean includeLevel2;
        readonly PriceType priceType;
        readonly BarPeriod period;
        Thread thread;

        #endregion
    }
}
