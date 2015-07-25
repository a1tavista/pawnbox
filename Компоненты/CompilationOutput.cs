using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace PawnBox
{
    static class COutput
    {
        public static ListViewItem[] ToTable(string str, out int err)
        {
            err = 0;
            List<ListViewItem> result = new List<ListViewItem>();
            string[] outp = Regex.Split(str, "\n");
            int i = 0;
            foreach (string o in outp)
            {
                if (Regex.IsMatch(o, " : error|warning"))
                {
                    string[] res = Regex.Split(o, " : ");
                    char[] tr = {'(', ')'};
                    res[0] = Regex.Match(res[0], @"(\(\d+\))").Groups[0].Value.Trim(tr);
                    res[1] = res[1].Trim();
                    if(Regex.IsMatch(res[1], "error"))
                        err++;
                    ListViewItem item = new ListViewItem((++i).ToString());
                    item.SubItems.Add(res[0]);
                    item.SubItems.Add(res[1]);
                    result.Add(item);
                }
            }
            return result.ToArray();
        }
    }
}
