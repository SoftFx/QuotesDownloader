namespace QuotesDownloader
{
    using System;
    using System.IO;
    using System.Threading;
    using TickTrader.FDK.Common;
    using TickTrader.FDK.Client;
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

        public Downloader(QuoteStore quoteClient, string outputType, String location, String symbol, DateTime from, DateTime to, Boolean includeLevel2)
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

        public Downloader(QuoteStore quoteClient, string outputType, String location, String symbol, DateTime from, DateTime to, PriceType priceType, BarPeriod period)
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
            if (outputType == "csv")
            {
                string path = Path.Combine(this.location, string.Format("{0}{1}{2}{3}.csv", symbol, includeLevel2 ? " level2" : "", from.ToString(" yyyyMMdd"), to.ToString(" yyyyMMdd")));
                using (StreamWriter file = File.CreateText(path))
                {
                    file.WriteLine("date_time;bid_price;bid_volume;ask_price;ask_volume");
                    for (Quote quote = enumerator.Next(-1); quote != null; quote = enumerator.Next(-1))
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append(quote.CreatingTime.ToString("yyyy-MM-dd HH:mm:ss.fff;", CultureInfo.InvariantCulture));
                        foreach (QuoteEntry entry in quote.Bids)
                            builder.AppendFormat("{0};{1};", entry.Price, entry.Volume);
                        foreach (QuoteEntry entry in quote.Asks)
                            builder.AppendFormat("{0};{1};", entry.Price, entry.Volume);
                        builder.Remove(builder.Length - 1, 1);
                        file.WriteLine(builder);
                    }
                }
                this.Log("Quotes are downloaded successfully");
            }
            else if (outputType == "hdf5")
            {
                string path = Path.Combine(this.location, string.Format("{0}{1}{2}{3}.h5", symbol, includeLevel2 ? " level2" : "", from.ToString(" yyyyMMdd"), to.ToString(" yyyyMMdd")));
                H5FileId fileId = H5F.create(path, H5F.CreateMode.ACC_TRUNC);

                H5DataTypeId quotesTypeId = new H5DataTypeId(H5T.H5Type.NATIVE_DOUBLE);
                H5DataSpaceId quotesSpaceId = H5S.create_simple(3, new long[] { 1, 2, 2 }, new long[] { (long)H5S.H5SType.UNLIMITED, 2, 2 });
                H5PropertyListId createChunkedQuotes = H5P.create(H5P.PropertyListClass.DATASET_CREATE);
                H5PropertyListId linkCreationDefaultsQuotes = H5P.create(H5P.PropertyListClass.LINK_CREATE);
                H5PropertyListId accessCreationDefaultsQuotes = H5P.create(H5P.PropertyListClass.DATASET_ACCESS);
                H5P.setChunk(createChunkedQuotes, new long[] { 1, 2, 2 });
                H5DataSetId quotesSetId = H5D.create(fileId, "Quotes", quotesTypeId, quotesSpaceId, linkCreationDefaultsQuotes, createChunkedQuotes, accessCreationDefaultsQuotes);

                H5DataTypeId dateQuotesTypeId = new H5DataTypeId(H5T.H5Type.NATIVE_LLONG);
                H5DataSpaceId dateQuotesSpaceId = H5S.create_simple(1, new long[] { 1 }, new long[] { (long)H5S.H5SType.UNLIMITED });
                H5PropertyListId createChunkedDate = H5P.create(H5P.PropertyListClass.DATASET_CREATE);
                H5PropertyListId linkCreationDefaultsDate = H5P.create(H5P.PropertyListClass.LINK_CREATE);
                H5PropertyListId accessCreationDefaultsDate = H5P.create(H5P.PropertyListClass.DATASET_ACCESS);
                H5P.setChunk(createChunkedDate, new long[] { 1 });
                H5DataSetId dateQuotesSetId = H5D.create(fileId, "DateQuotes", dateQuotesTypeId, dateQuotesSpaceId, linkCreationDefaultsDate, createChunkedDate, accessCreationDefaultsDate);

                int count = 0;
                int chunkCount = 0;
                double[,,] quotesArr = new double[chunkSize, 2, 2];
                long[] datesArr = new long[chunkSize];
                H5DataSpaceId memSpace;
                for (Quote quote = enumerator.Next(-1); quote != null; quote = enumerator.Next(-1))
                {
                    int j = 0;
                    foreach (QuoteEntry entry in quote.Bids)
                    {
                        quotesArr[chunkCount, 0, j] = entry.Price;
                        quotesArr[chunkCount, 0, j + 1] = entry.Volume;
                        j += 2;
                    }

                    j = 0;
                    foreach (QuoteEntry entry in quote.Asks)
                    {
                        quotesArr[chunkCount, 1, j] = entry.Price;
                        quotesArr[chunkCount, 1, j + 1] = entry.Volume;
                        j += 2;
                    }

                    datesArr[chunkCount] =
                        (long)quote.CreatingTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                    chunkCount++;
                    count++;
                    if (chunkCount == chunkSize)
                    {
                        H5D.setExtent(quotesSetId, new long[] { count, 2, 2 });
                        H5S.close(quotesSpaceId);
                        quotesSpaceId = H5D.getSpace(quotesSetId);
                        H5S.selectHyperslab(quotesSpaceId, H5S.SelectOperator.SET, new long[] { count - chunkSize, 0, 0 },
                            new long[] { chunkSize, 2, 2 });

                        memSpace = H5S.create_simple(3, new long[] { chunkSize, 2, 2 });
                        H5D.write(quotesSetId, quotesTypeId, memSpace, quotesSpaceId,
                            new H5PropertyListId(H5P.Template.DEFAULT), new H5Array<double>(quotesArr));

                        H5D.setExtent(dateQuotesSetId, new long[] { count });
                        H5S.close(dateQuotesSpaceId);
                        dateQuotesSpaceId = H5D.getSpace(dateQuotesSetId);
                        H5S.selectHyperslab(dateQuotesSpaceId, H5S.SelectOperator.SET, new long[] { count - chunkSize },
                            new long[] { chunkSize });
                        memSpace = H5S.create_simple(1, new long[] { chunkSize });
                        H5D.write(dateQuotesSetId, dateQuotesTypeId, memSpace, dateQuotesSpaceId,
                            new H5PropertyListId(H5P.Template.DEFAULT),
                            new H5Array<long>(datesArr));
                        chunkCount = 0;
                    }
                }

                if (count % chunkSize != 0)
                {
                    int delta = count % chunkSize;
                    H5D.setExtent(quotesSetId, new long[] { count, 2, 2 });
                    H5S.close(quotesSpaceId);
                    quotesSpaceId = H5D.getSpace(quotesSetId);
                    H5S.selectHyperslab(quotesSpaceId, H5S.SelectOperator.SET, new long[] { count - delta, 0, 0 },
                        new long[] { delta, 2, 2 });

                    memSpace = H5S.create_simple(3, new long[] { delta, 2, 2 });
                    H5D.write(quotesSetId, quotesTypeId, memSpace, quotesSpaceId,
                        new H5PropertyListId(H5P.Template.DEFAULT), new H5Array<double>(quotesArr));

                    H5D.setExtent(dateQuotesSetId, new long[] { count });
                    H5S.close(dateQuotesSpaceId);
                    dateQuotesSpaceId = H5D.getSpace(dateQuotesSetId);
                    H5S.selectHyperslab(dateQuotesSpaceId, H5S.SelectOperator.SET, new long[] { count - delta },
                        new long[] { delta });
                    memSpace = H5S.create_simple(1, new long[] { delta });
                    H5D.write(dateQuotesSetId, dateQuotesTypeId, memSpace, dateQuotesSpaceId,
                        new H5PropertyListId(H5P.Template.DEFAULT),
                        new H5Array<long>(datesArr));
                }

                H5P.close(createChunkedQuotes);
                H5P.close(linkCreationDefaultsQuotes);
                H5P.close(accessCreationDefaultsQuotes);
                H5P.close(createChunkedDate);
                H5P.close(linkCreationDefaultsDate);
                H5P.close(accessCreationDefaultsDate);
                H5S.close(quotesSpaceId);
                H5D.close(quotesSetId);
                H5S.close(dateQuotesSpaceId);
                H5D.close(dateQuotesSetId);

                H5F.close(fileId);
                this.Log("Quotes are downloaded successfully");
            }
            else
            {
                Console.WriteLine("Unsupported output type - " + outputType);
                throw new Exception("Unsupported output type - " + outputType);
            }
        }

        void DownloadBars()
        {
            DownloadBarsEnumerator enumerator = quoteClient.DownloadBars(symbol, priceType, period, from, to, -1);
            if (outputType == "csv")
            {
                string path = Path.Combine(this.location, string.Format("{0} {1} {2} {3} {4}.csv", symbol, priceType, period, from.ToString(" yyyyMMdd"), to.ToString(" yyyyMMdd")));
                using (StreamWriter file = File.CreateText(path))
                {
                    file.WriteLine("date_time;open;close;low;high;volume");
                    for (Bar bar = enumerator.Next(-1); bar != null; bar = enumerator.Next(-1))
                        file.WriteLine(string.Format("{0};{1};{2};{3};{4};{5}", bar.From.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), bar.Open, bar.Close, bar.Low, bar.High, bar.Volume));
                }
                this.Log("Bars are downloaded successfully");
            }
            else if (outputType == "hdf5")
            {
                string path = Path.Combine(this.location, string.Format("{0} {1} {2} {3} {4}.h5", symbol, priceType, period, from.ToString(" yyyyMMdd"), to.ToString(" yyyyMMdd")));
                H5FileId fileId = H5F.create(path, H5F.CreateMode.ACC_TRUNC);
               
                var barsData = new List<Bar>();
                for (Bar bar = enumerator.Next(-1); bar != null; bar = enumerator.Next(-1))
                    barsData.Add(bar);

                if (barsData.Count == 0)
                    return;

                double[,] barsArray = new double[barsData.Count, 5];
                long[,] dataBarsArray = new long[barsData.Count, 2];
                long priceTypeString = 0;
                if (priceType == PriceType.Ask)
                    priceTypeString = 1;
                for (int i=0; i< barsData.Count; i++)
                {
                    dataBarsArray[i, 0] = priceTypeString;
                    long dateTimestamp = (long)(barsData[i].From.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
                    dataBarsArray[i, 1] = dateTimestamp;
                    barsArray[i, 0] = barsData[i].Volume;
                    barsArray[i, 1] = barsData[i].Open;
                    barsArray[i, 2] = barsData[i].Close;
                    barsArray[i, 3] = barsData[i].High;
                    barsArray[i, 4] = barsData[i].Low;
                }

                H5DataTypeId barsTypeId = new H5DataTypeId(H5T.H5Type.NATIVE_DOUBLE);
                WriteDataToNewFile(fileId, "Bars", barsArray, barsData.Count, 5, barsTypeId);

                H5DataTypeId dataBarsTypeId = new H5DataTypeId(H5T.H5Type.NATIVE_LLONG);
                WriteDataToNewFile(fileId, "DataBars", dataBarsArray, barsData.Count, 2, dataBarsTypeId);

                H5F.close(fileId);
                this.Log("Bars are downloaded successfully");
            }
            else
            {
                Console.WriteLine("Unsupported output type - " + outputType);
                throw new Exception("Unsupported output type - " + outputType);
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

        readonly QuoteStore quoteClient;
        readonly string outputType;
        readonly string location;
        readonly string symbol;
        readonly DateTime from;
        readonly DateTime to;
        readonly Boolean includeLevel2;
        readonly PriceType priceType;
        readonly BarPeriod period;
        Thread thread;
        const int chunkSize = 32 * 4;  //32 - chunk size for 1 kB

        #endregion
    }
}
