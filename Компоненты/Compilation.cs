using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PawnBox
{

    class Compilation
    {
        private string cFile = "";
        private string[] output = new string[2];
        private Form form;
        private Process p = new Process();
        private CompilerCallback callback;

        public Compilation(string file, CompilerCallback deleg, Form parent) 
        {
            cFile = file;
            callback = deleg;
            form = parent;
        }

        public void Start()
        {
            p.StartInfo.FileName = Properties.Settings.Default.PathToCompiler;
            p.StartInfo.Arguments = String.Format("\"{0}\"", cFile);
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;

            p.Start();
            output[0] = p.StandardOutput.ReadToEnd();
            output[1] = p.StandardError.ReadToEnd();

            string amx = Path.GetDirectoryName(Properties.Settings.Default.PathToCompiler) + @"\" + Path.ChangeExtension(Path.GetFileName(cFile), ".amx");
            File.Delete(Path.ChangeExtension(cFile, ".amx"));
            if (File.Exists(amx))
                File.Move(amx, Path.ChangeExtension(cFile, ".amx"));
            File.Delete(amx);
          
            if (callback != null)
                form.Invoke(callback, output[1] + output[0]);      
        }
    }
}
