using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PawnBox
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            checkBox1.Checked = Properties.Settings.Default.ShowLines;
            checkBox2.Checked = Properties.Settings.Default.ShowLibrary;
            checkBox3.Checked = Properties.Settings.Default.UseAutoComplete;
            checkBox4.Checked = Properties.Settings.Default.SaveBeforeCompiling;
            textBox2.Text = Properties.Settings.Default.PathToCompiler;
            textBox3.Text = Properties.Settings.Default.PathToRecovery;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = Properties.Settings.Default.PathToCompiler;
            openFileDialog1.ShowDialog();
            Properties.Settings.Default.PathToCompiler = openFileDialog1.FileName;
            textBox2.Text = openFileDialog1.FileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowLines = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowLibrary = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseAutoComplete = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SaveBeforeCompiling = checkBox4.Checked;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            Properties.Settings.Default.PathToRecovery = folderBrowserDialog1.SelectedPath;
            textBox3.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FontDialog dialog = new FontDialog();
            dialog.FontMustExist = true;
            dialog.MaxSize = 72;
            dialog.MinSize = 5;
            dialog.ShowEffects = false;
            dialog.ShowColor = false;
            dialog.AllowVerticalFonts = false;
            dialog.Font = Properties.Settings.Default.Font;
            dialog.ShowDialog();
            Properties.Settings.Default.Font = dialog.Font;
        }

    }
}
