using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FriendlyRemoteClientWinForms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Thread t = new Thread(() => Application.Run(new ReceiverForm()));
            Thread t2 = new Thread(() => Application.Run(new Form1()));
            t.Start();
            t2.Start();
        }
    }
}