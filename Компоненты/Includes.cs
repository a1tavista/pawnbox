using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PawnBox
{
    class IncludeLoader
    {
        private string incpath;
        private LibraryCallback callback;
        private Form form;

        public IncludeLoader(string path, LibraryCallback cback, Form parent) 
        {
            incpath = path;
            callback = cback;
            form = parent;
        }

        public void Load()
        {
            List<string> Includes = System.IO.Directory.GetFiles(incpath).ToList();
            List<TreeNode> nodes = new List<TreeNode>();
            List<TreeNode> func = new List<TreeNode>();
            foreach (string s in Includes)
            {
                func.Clear();
                List<string> content = new List<string>();
                if(Path.GetExtension(s) == ".inc")
                    content = File.ReadAllLines(s).ToList();
                content.Sort();
                foreach (string content_string in content)
                    if (content_string.StartsWith("native") || content_string.StartsWith("stock"))
                        func.Add(new TreeNode(Regex.Match(content_string, @"\b\w+\(.*\)").Value));
                nodes.Add(new TreeNode(Path.GetFileName(s), func.ToArray()));
                
            }
            form.Invoke(callback, (object)nodes.ToArray());
        }

    }
}
