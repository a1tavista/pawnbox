using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace PawnBox
{
    public delegate void CompilerCallback(string output);
    public delegate void LibraryCallback(TreeNode[] nodes);

    public class OneInstanceApp : WindowsFormsApplicationBase  
    {    
        public static void Run(Form form, StartupNextInstanceEventHandler startupHandler)  
        {  
            OneInstanceApp app = new OneInstanceApp();
            app.IsSingleInstance = true;
            app.MainForm = form;  
            app.StartupNextInstance += startupHandler;  
            app.Run(Environment.GetCommandLineArgs());            
        }  
    }

    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>      
        static MainForm form;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new MainForm();
            OneInstanceApp.Run(form, StartupNextInstanceHandler);
        }

        static void StartupNextInstanceHandler(object sender, StartupNextInstanceEventArgs e)  
        {
            form.OpenFile(e.CommandLine[1], true);
            form.Activate();
        }  
    }
}
