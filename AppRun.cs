using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

namespace RainbowPawno
{
    class AppRun
    {
        private bool IsStarted;
        private bool requestInitialOwnership = true;
        private Mutex m = new Mutex(requestInitialOwnership, "RainbowPawnoMutex", out IsStarted);
        
        static public void Startup()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm()); 
            
        }
    }
}
