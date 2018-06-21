namespace QuotesDownloader
{
    using System;
    using System.Windows.Forms;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
                ConsoleDownloader.ConsoleMain(args);
            else
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new QuotesDownloader());
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "QuotesDownloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }
    }
}
