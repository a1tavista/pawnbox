using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace PawnBox
{
    public partial class CompilationForm : Form
    {
        public CompilationForm()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(CompilationForm_FormClosing);
        }

        void CompilationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
        }
    }
}
