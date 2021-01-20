using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.FDK.Common;
using TickTrader.FDK.Client;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace QuotesDownloader
{
    public static class ConsoleDownloader
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFile(string fileName, int desiredAccess, int shareMode, IntPtr securityAttributes, [MarshalAs(UnmanagedType.I4)]int creationDisposition, [MarshalAs(UnmanagedType.I4)]int flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetStdHandle(int nStdHandle, IntPtr handle);

        private delegate void SignalHandler(ConsoleSignal consoleSignal);

        private enum ConsoleSignal
        {
            CtrlC = 0,
            CtrlBreak = 1,
            Close = 2,
            LogOff = 5,
            Shutdown = 6
        }

        [DllImport("Kernel32", EntryPoint = "SetConsoleCtrlHandler")]
        private static extern bool SetSignalHandler(SignalHandler handler, bool add);

        public static void ConsoleMain(string[] args)
        {
            if (AllocConsole())
            {
                var ptr = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, 3, 0, IntPtr.Zero);
                if (!SetStdHandle(-11, ptr))
                    throw new Exception("Failed to set handle.");

                var newOut = new StreamWriter(Console.OpenStandardOutput());
                newOut.AutoFlush = true;
                Console.SetOut(newOut);
                Console.SetError(newOut);

                try
                {
                    bool help = false;

                    string address = "ttlive.fxopen.com";
                    string login = "100";
                    string password = "TTqfdeppmhDR";
                    int port = 5042;

                    var to = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
                    to = to.AddDays(-7);
                    var from = to.AddDays(-7);
                    string symbol = "EURUSD";
                    string periodicity = "H1";
                    PriceType priceType = PriceType.Bid;
                    bool bars = false;
                    bool ticks = false;
                    bool level2 = false;
                    bool vwap = false;
                    bool verbose = false;
                    string outputType = "csv";

                    var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    path = Path.Combine(path, "Quotes");
                    path = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    Directory.CreateDirectory(path);
                    string location = path;

                    var options = new OptionSet()
                        {
                            { "a|address=", v => address = v },
                            { "u|username=", v => login = v },
                            { "w|password=", v => password = v },
                            { "p|port=", v => port = int.Parse(v) },
                            { "h|?|help",   v => help = v != null },
                            { "t|dateto=", v => to = DateTime.Parse(v) },
                            { "f|datefrom=", v => from = DateTime.Parse(v) },
                            { "s|symbol=", v => symbol = v },
                            { "d|periodicity=", v => periodicity = v },
                            { "o|output=", v => outputType = v },
                            { "l|location=", v => location = v },
                            { "v|verbose=", v => verbose = bool.Parse(v) },
                            { "r|request=", v =>
                                {
                                    switch (v.ToLowerInvariant())
                                    {
                                        case "bids":
                                            priceType = PriceType.Bid;
                                            bars = true;
                                            break;
                                        case "asks":
                                            priceType = PriceType.Ask;
                                            bars = true;
                                            break;
                                        case "ticks":
                                            ticks = true;
                                            break;
                                        case "level2":
                                            level2 = true;
                                            break;
                                        case "vwap":
                                            vwap = true;
                                            break;
                                        default:
                                            throw new Exception("Unknown request type: " + v);
                                    }
                                }
                            },
                        };

                    try
                    {
                        options.Parse(args);
                    }
                    catch (OptionException e)
                    {
                        Console.Error.WriteLine(e.Message);
                        Console.WriteLine("Try `QuotesDownloader --help' for more information.");
                        return;
                    }

                    if (help)
                    {
                        Console.WriteLine("QuotesDownloader usage:");
                        options.WriteOptionDescriptions(Console.Out);
                        Console.ReadKey();
                        return;
                    }

                    var client = new QuoteStore("QuotesDownloader", port: port, validateClientCertificate: (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);

                    client.Connect(address, -1);
                    client.Login(login, password, "", "", "", -1);
                    if (verbose)
                    {
                        Console.WriteLine("The Quote Client is connected.");
                        Console.WriteLine("Start downloading...");
                    }
                    List<Task> tasks = new List<Task>();
                    foreach (var _symbol in ParseSymbols(symbol))
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            Console.WriteLine($"Downloading symbol {_symbol}");
                            Downloader downloader;
                            if (bars)
                                downloader = new Downloader(client, outputType, location, _symbol, from, to, priceType, new BarPeriod(periodicity));
                            else
                                downloader = new Downloader(client, outputType, location, _symbol, from, to, level2, vwap);
                            SetSignalHandler(signal =>
                            {
                                downloader?.CancelDownload();
                            }, true);
                            downloader.Start();
                            while (!downloader.IsFinished)
                                Thread.Sleep(50);
                            Console.WriteLine($"Symbol {_symbol} downloaded successful.");
                        }));
                    }
                    Task.WaitAll(tasks.ToArray());
                    if (verbose)
                        Console.WriteLine("Downloading finished successfully.");
                    client.Disconnect("Disconnecting");
                    client.Dispose();
                    if (verbose)
                        Console.WriteLine("The Quote Client is disconnected.");
                    Console.ReadLine();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error : " + ex.Message);
                    Console.ReadLine();
                }
            }
            FreeConsole();
        }

        private static List<string> ParseSymbols(string symbols)
        {
            return symbols?.Split('|').ToList();
        }
    }
}
