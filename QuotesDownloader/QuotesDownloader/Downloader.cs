using System.Windows.Forms.VisualStyles;

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
    using ICSharpCode.SharpZipLib.Zip;
    using ICSharpCode.SharpZipLib.Core;

    public class Downloader
    {
        Downloader()
        {
            this.thread = new Thread(ThreadMethod);
        }

        public Downloader(QuoteStore quoteClient, string outputType, String location, String symbol, DateTime from, DateTime to, Boolean includeLevel2 = false, bool includeVWAP = false)
            : this()
        {
            this.quoteClient = quoteClient;
            this.outputType = outputType;
            this.location = location;
            this.symbol = symbol;
            this.from = from;
            this.to = to;
            this.includeLevel2 = includeLevel2;
            this.includeVWAP = includeVWAP;
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
                if (includeVWAP)
                    throw new ArgumentException("VWAP is not supported for hdf5 and csv format.");
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
                if (includeVWAP)
                    throw new ArgumentException("VWAP is not supported for hdf5 and csv format.");
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
                        (long) quote.CreatingTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                    chunkCount++;
                    count++;
                    if (chunkCount == chunkSize)
                    {
                        H5D.setExtent(quotesSetId, new long[] {count, 2, 2});
                        H5S.close(quotesSpaceId);
                        quotesSpaceId = H5D.getSpace(quotesSetId);
                        H5S.selectHyperslab(quotesSpaceId, H5S.SelectOperator.SET, new long[] {count - chunkSize, 0, 0},
                            new long[] {chunkSize, 2, 2});

                        memSpace = H5S.create_simple(3, new long[] {chunkSize, 2, 2});
                        H5D.write(quotesSetId, quotesTypeId, memSpace, quotesSpaceId,
                            new H5PropertyListId(H5P.Template.DEFAULT), new H5Array<double>(quotesArr));

                        H5D.setExtent(dateQuotesSetId, new long[] {count});
                        H5S.close(dateQuotesSpaceId);
                        dateQuotesSpaceId = H5D.getSpace(dateQuotesSetId);
                        H5S.selectHyperslab(dateQuotesSpaceId, H5S.SelectOperator.SET, new long[] {count - chunkSize},
                            new long[] {chunkSize});
                        memSpace = H5S.create_simple(1, new long[] {chunkSize});
                        H5D.write(dateQuotesSetId, dateQuotesTypeId, memSpace, dateQuotesSpaceId,
                            new H5PropertyListId(H5P.Template.DEFAULT),
                            new H5Array<long>(datesArr));
                        chunkCount = 0;
                    }
                }
                
                if (count % chunkSize != 0)
                {
                    int delta = count % chunkSize;
                    H5D.setExtent(quotesSetId, new long[] {count, 2, 2});
                    H5S.close(quotesSpaceId);
                    quotesSpaceId = H5D.getSpace(quotesSetId);
                    H5S.selectHyperslab(quotesSpaceId, H5S.SelectOperator.SET, new long[] {count - delta, 0, 0},
                        new long[] {delta, 2, 2});

                    memSpace = H5S.create_simple(3, new long[] {delta, 2, 2});
                    H5D.write(quotesSetId, quotesTypeId, memSpace, quotesSpaceId,
                        new H5PropertyListId(H5P.Template.DEFAULT), new H5Array<double>(quotesArr));

                    H5D.setExtent(dateQuotesSetId, new long[] {count});
                    H5S.close(dateQuotesSpaceId);
                    dateQuotesSpaceId = H5D.getSpace(dateQuotesSetId);
                    H5S.selectHyperslab(dateQuotesSpaceId, H5S.SelectOperator.SET, new long[] {count - delta},
                        new long[] {delta});
                    memSpace = H5S.create_simple(1, new long[] {delta});
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
            else if (outputType == "csv_zip")
            {
                string quoteType = "Ticks";
                if (includeVWAP)
                    quoteType = "VWAP";
                if (includeLevel2)
                    quoteType = "TicksLevel2";
                string path = Path.Combine(location, $"{symbol.Replace("/","")}_{quoteType}_{from.ToString("yyyy-MM-dd")}_{to.ToString("yyyy-MM-dd")}.zip");
                using (ZipOutputStream zs = new ZipOutputStream(File.Create(path)))
                {
                    if (includeVWAP)
                        DownloadVWAPCSVNew(zs);
                    else if (includeLevel2)
                        DownloadLevel2CSVNew(enumerator, zs);
                    else
                        DownloadTicksCSVNew(enumerator, zs);
                }
                this.Log("Quotes are downloaded successfully");
            }
        }

        void DownloadTicksCSVNew(DownloadQuotesEnumerator enumerator, ZipOutputStream zipStream)
        {
            string filename = "ticks.csv";
            using (StreamWriter file = File.CreateText(filename))
            {
                DateTime lastQuoteTime = DateTime.MinValue;
                int repeatingQuotes = 0;
                for (Quote quote = enumerator.Next(-1); quote != null; quote = enumerator.Next(-1))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(quote.CreatingTime.ToString("yyyy.MM.dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                    if (quote.CreatingTime == lastQuoteTime)
                        repeatingQuotes++;
                    else
                    {
                        lastQuoteTime = quote.CreatingTime;
                        repeatingQuotes = 0;
                    }
                    if (repeatingQuotes > 0)
                        builder.Append(-repeatingQuotes);
                    builder.Append(",");
                    if (quote.Bids.Count != 0)
                        builder.Append($"{quote.Bids[0].Price},{quote.Bids[0].Volume},");
                    else
                        builder.Append(",,");
                    if (quote.Asks.Count != 0)
                        builder.Append($"{quote.Asks[0].Price},{quote.Asks[0].Volume},");
                    else
                        builder.Append(",,");
                    builder.Remove(builder.Length - 1, 1);
                    file.WriteLine(builder);
                }
            }

            FileInfo fi = new FileInfo(filename);
            ZipEntry newEntry = new ZipEntry("ticks.csv");
            newEntry.Size = fi.Length;
            zipStream.PutNextEntry(newEntry);
            byte[] buffer = new byte[4096];
            using (FileStream streamReader = File.OpenRead(filename))
            {
                StreamUtils.Copy(streamReader, zipStream, buffer);
            }
            zipStream.CloseEntry();
            File.Delete("ticks.csv");
        }
        void DownloadLevel2CSVNew(DownloadQuotesEnumerator enumerator, ZipOutputStream zipStream)
        {
            string filename = "ticks level2.csv";
            using (StreamWriter file = File.CreateText(filename))
            {
                DateTime lastQuoteTime = DateTime.MinValue;
                int repeatingQuotes = 0;
                for (Quote quote = enumerator.Next(-1); quote != null; quote = enumerator.Next(-1))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(quote.CreatingTime.ToString("yyyy.MM.dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                    if (quote.CreatingTime == lastQuoteTime)
                        repeatingQuotes++;
                    else
                    {
                        lastQuoteTime = quote.CreatingTime;
                        repeatingQuotes = 0;
                    }
                    if (repeatingQuotes > 0)
                        builder.Append(-repeatingQuotes);
                    builder.Append(",");
                    int i = 0;
                    while (i < quote.Bids.Count || i < quote.Asks.Count)
                    {
                        if (i < quote.Bids.Count)
                            builder.Append($"{quote.Bids[i].Price},{quote.Bids[i].Volume},");
                        else
                            builder.Append(",,");
                        if (i < quote.Asks.Count)
                            builder.Append($"{quote.Asks[i].Price},{quote.Asks[i].Volume},");
                        else
                            builder.Append(",,");
                        i++;
                    }
                    builder.Remove(builder.Length - 1, 1);
                    file.WriteLine(builder);
                }
            }

            FileInfo fi = new FileInfo(filename);
            ZipEntry newEntry = new ZipEntry("ticks level2.csv");
            newEntry.Size = fi.Length;
            zipStream.PutNextEntry(newEntry);
            byte[] buffer = new byte[4096];
            using (FileStream streamReader = File.OpenRead(filename))
            {
                StreamUtils.Copy(streamReader, zipStream, buffer);
            }
            zipStream.CloseEntry();

            File.Delete("ticks level2.csv");
        }
        void DownloadVWAPCSVNew(ZipOutputStream zipStream)
        {
            for (int deg = -16; deg <= 16; deg++)
            {
                DownloadQuotesEnumerator enumerator;
                try
                {
                    enumerator = quoteClient.DownloadVWAPQuotes(symbol, (short) deg, from, to, 500);
                }
                catch
                {
                    continue;
                }

                char sign = deg >= 0 ? '+' : '-';
                string filename = $"ticks vwap e{sign}{Math.Abs(deg)}.csv";
                using (StreamWriter file = File.CreateText(filename))
                {
                    DateTime lastQuoteTime = DateTime.MinValue;
                    int repeatingQuotes = 0;
                    for (Quote quote = enumerator.Next(-1); quote != null; quote = enumerator.Next(-1))
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append(quote.CreatingTime.ToString("yyyy.MM.dd HH:mm:ss.fff",
                            CultureInfo.InvariantCulture));
                        if (quote.CreatingTime == lastQuoteTime)
                            repeatingQuotes++;
                        else
                        {
                            lastQuoteTime = quote.CreatingTime;
                            repeatingQuotes = 0;
                        }

                        if (repeatingQuotes > 0)
                            builder.Append(-repeatingQuotes);
                        builder.Append(",");
                        if (quote.Bids.Count != 0)
                            builder.Append($"{quote.Bids[0].Price},{quote.Bids[0].Volume},");
                        else
                            builder.Append(",,");
                        if (quote.Asks.Count != 0)
                            builder.Append($"{quote.Asks[0].Price},{quote.Asks[0].Volume},");
                        else
                            builder.Append(",,");
                        builder.Remove(builder.Length - 1, 1);
                        file.WriteLine(builder);
                    }
                }

                FileInfo fi = new FileInfo(filename);
                ZipEntry newEntry = new ZipEntry($"ticks vwap e{(Math.Sign(deg) == -1 ? '-' : '+')}{Math.Abs(deg):d2}.csv");
                newEntry.Size = fi.Length;
                zipStream.PutNextEntry(newEntry);
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }

                zipStream.CloseEntry();
                File.Delete($"ticks vwap e{(Math.Sign(deg) == -1 ? '-' : '+')}{Math.Abs(deg):d2}.csv");
            }
        }

        void DownloadBars()
        {
            if (outputType == "csv")
            {
                DownloadBarsEnumerator enumerator = quoteClient.DownloadBars(symbol, priceType, period, from, to, -1);
                string path = Path.Combine(this.location, string.Format("{0} {1} {2} {3} {4}.csv", symbol, priceType, period, from.ToString(" yyyyMMdd"), to.ToString(" yyyyMMdd")));
                using (StreamWriter file = File.CreateText(path))
                {
                    file.WriteLine("date_time;open;close;low;high;volume");
                    for (Bar bar = enumerator.Next(-1); bar != null; bar = enumerator.Next(-1))
                        file.WriteLine(string.Format("{0};{1};{2};{3};{4};{5}", bar.From.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), bar.Open, bar.Close, bar.Low, bar.High, bar.Volume));
                }
                this.Log("Bars are downloaded successfully");
            }
            else if (outputType == "hdf5")
            {
                DownloadBarsEnumerator enumerator = quoteClient.DownloadBars(symbol, priceType, period, from, to, -1);
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
            else if (outputType == "csv_zip")
            {
                DownloadBarsEnumerator BidEnumerator = quoteClient.DownloadBars(symbol, PriceType.Bid, period, from, to, -1);
                DownloadBarsEnumerator AskEnumerator = quoteClient.DownloadBars(symbol, PriceType.Ask, period, from, to, -1);
                string path = Path.Combine(location, $"{symbol.Replace("/", "")}_{period}_{from.ToString("yyyy-MM-dd")}_{to.ToString("yyyy-MM-dd")}.zip");
                using (ZipOutputStream zs = new ZipOutputStream(File.Create(path)))
                {
                    string filename = $"{period} bid.csv";
                    using (StreamWriter file = File.CreateText(filename))
                    {
                        DateTime lastQuoteTime = DateTime.MinValue;
                        int repeatingQuotes = 0;
                        for (Bar bar = BidEnumerator.Next(-1); bar != null; bar = BidEnumerator.Next(-1))
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.Append(bar.From.ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture));
                            if (bar.From == lastQuoteTime)
                                repeatingQuotes++;
                            else
                            {
                                lastQuoteTime = bar.From;
                                repeatingQuotes = 0;
                            }
                            if (repeatingQuotes > 0)
                                builder.Append(-repeatingQuotes);
                            builder.Append(",");
                            builder.Append($"{bar.Open},{bar.High},{bar.Low},{bar.Close},{bar.Volume},");
                            builder.Remove(builder.Length - 1, 1);
                            file.WriteLine(builder);
                        }
                    }

                    FileInfo fi = new FileInfo(filename);
                    ZipEntry newEntry = new ZipEntry(filename);
                    newEntry.Size = fi.Length;
                    zs.PutNextEntry(newEntry);
                    byte[] buffer = new byte[4096];
                    using (FileStream streamReader = File.OpenRead(filename))
                    {
                        StreamUtils.Copy(streamReader, zs, buffer);
                    }
                    zs.CloseEntry();

                    File.Delete(filename);

                    filename = $"{period} ask.csv";
                    using (StreamWriter file = File.CreateText(filename))
                    {
                        DateTime lastQuoteTime = DateTime.MinValue;
                        int repeatingQuotes = 0;
                        for (Bar bar = AskEnumerator.Next(-1); bar != null; bar = AskEnumerator.Next(-1))
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.Append(bar.From.ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture));
                            if (bar.From == lastQuoteTime)
                                repeatingQuotes++;
                            else
                            {
                                lastQuoteTime = bar.From;
                                repeatingQuotes = 0;
                            }
                            if (repeatingQuotes > 0)
                                builder.Append(-repeatingQuotes);
                            builder.Append(",");
                            builder.Append($"{bar.Open},{bar.High},{bar.Low},{bar.Close},{bar.Volume},");
                            builder.Remove(builder.Length - 1, 1);
                            file.WriteLine(builder);
                        }
                    }

                    fi = new FileInfo(filename);
                    newEntry = new ZipEntry(filename);
                    newEntry.Size = fi.Length;
                    zs.PutNextEntry(newEntry);
                    buffer = new byte[4096];
                    using (FileStream streamReader = File.OpenRead(filename))
                    {
                        StreamUtils.Copy(streamReader, zs, buffer);
                    }
                    zs.CloseEntry();

                    File.Delete(filename);
                }
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

        readonly QuoteStore quoteClient;
        readonly string outputType;
        readonly string location;
        readonly string symbol;
        readonly DateTime from;
        readonly DateTime to;
        readonly Boolean includeLevel2;
        readonly Boolean includeVWAP;
        readonly PriceType priceType;
        readonly BarPeriod period;
        Thread thread;
        const int chunkSize = 32 * 4;  //32 - chunk size for 1 kB

        #endregion
    }
}
