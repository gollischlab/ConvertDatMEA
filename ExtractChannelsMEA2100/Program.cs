using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// Fernando Rozenblit, 2018

namespace ExtractChannels2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Error handling from https://stackoverflow.com/questions/20359923/unhandled-exception-swallowed-except-in-debugger
            // For UI events (set the unhandled exception mode to force all Windows Forms
            // errors to go through our handler)
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(delegate (object s, ThreadExceptionEventArgs e) {
                ExceptionLogger(e.Exception);
            });
            // Non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(delegate (object sender, UnhandledExceptionEventArgs e) {
                ExceptionLogger(e.ExceptionObject as Exception);
            });

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new MainWindow());
            } catch (Exception e)
            {
                ExceptionLogger(e);
                throw;
            }
        }

        public static void ExceptionLogger(Exception e)
        {
            string msg = "An error occurred:\n";
            msg = msg + e.Message + "\n\nStack Trace:\n" + e.StackTrace;
            if (MessageBox.Show(msg, "ExtractChannels Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop) == DialogResult.Abort)
                Application.Exit();
        }
    }
}
