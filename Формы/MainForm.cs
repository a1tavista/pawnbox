using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using FastColoredTextBoxNS;

namespace PawnBox
{
    public partial class MainForm : Form
    {
        private FTab CurrentTab;
        private FTab CompiledTab;
        const int MRUnumber = 6;
        System.Collections.Generic.Queue<string> MRUlist = new Queue<string>();
        private Dictionary<string, Style> TextStyles = new Dictionary<string, Style>();
        private CompilationForm CForm = new CompilationForm();

        public MainForm()
        {
            InitializeComponent();

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadRecentList();
            foreach (string item in MRUlist)
            {
                ToolStripMenuItem fileRecent = new ToolStripMenuItem(item, null, RecentFile_Click);
                RecentFilesToolStripMenuItem.DropDownItems.Add(fileRecent);
            }
            if (Properties.Settings.Default.PathToCompiler.Length == 0)
            {
                Properties.Settings.Default.PathToCompiler = Application.StartupPath + @"\pawncc.exe";
                Properties.Settings.Default.Save();
            }
            ApplyOptions();
            Properties.Settings.Default.SettingsSaving += new System.Configuration.SettingsSavingEventHandler(Default_SettingsSaving);
            TextStyles.Add("Comments", new TextStyle(Brushes.Green, null, FontStyle.Regular));
            TextStyles.Add("Operators", new TextStyle(Brushes.Blue, null, FontStyle.Regular));
            TextStyles.Add("Digits", new TextStyle(Brushes.DarkBlue, null, FontStyle.Regular));
            TextStyles.Add("Strings", new TextStyle(Brushes.DimGray, null, FontStyle.Regular));
            TextStyles.Add("Params", new TextStyle(Brushes.Gray, null, FontStyle.Regular));
            ActiveControl = NewTab(String.Format("newfile{0}.pwn", mainControl.Controls.Count), "");
            this.Text = "PawnBox - " + CurrentTab.Text;
            splitContainer3.Panel2Collapsed = true;
            string[] args = System.Environment.GetCommandLineArgs();
            if (args.Length > 1)
                OpenFile(args[1], false);
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 100;
            t.Start();
            t.Tick += new EventHandler(t_Tick);
        }

        void t_Tick(object sender, EventArgs e)
        {
            IncludeLoader loader = new IncludeLoader(Path.GetDirectoryName(Properties.Settings.Default.PathToCompiler) + @"\include", IncludeLoadingComplete, this);
            Thread t = new Thread(new ThreadStart(loader.Load));
            t.Start();
            (sender as System.Windows.Forms.Timer).Stop();
        }

        void Default_SettingsSaving(object sender, CancelEventArgs e)
        {
            ApplyOptions();
        }

        void listView1_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ListViewItem item = listView1.GetItemAt(e.X, e.Y);
            mainControl.SelectedTab = CurrentTab = CompiledTab;
            ActiveControl = CompiledTab.CodeBox;
            if (item != null)
            {                             
                int line = int.Parse(item.SubItems[1].Text) - 1;
                if (CompiledTab.CodeBox.LinesCount >= line)
                    CompiledTab.CodeBox.Navigate(line);
            }
        }

        private void Lib_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                    StatusBarLabel.Text = e.Node.Text;
            
        }

        void LibraryView_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
                LibraryView.SelectedNode = LibraryView.SelectedNode.NextVisibleNode;
            else if (e.KeyCode == Keys.Up)
                LibraryView.SelectedNode = LibraryView.SelectedNode.PrevVisibleNode;
            StatusBarLabel.Text = LibraryView.SelectedNode.Text;
        }

        private void Lib_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            ActiveControl = CurrentTab;
            if (e.Button == MouseButtons.Left)
                if (e.Node.Level == 1)
                {                  
                    CurrentTab.CodeBox.InsertText(Regex.Match(e.Node.Text, @"\w+\(").Value + ");", true);
                    CurrentTab.CodeBox.SelectionStart = CurrentTab.CodeBox.SelectionStart - 2;
                    StatusBarLabel.Text = e.Node.Text;
                }
        }

        private FastColoredTextBox NewTab(string name, string fname)
        {
            FTab Tab = new FTab();
            FastColoredTextBox c = NewTextBox(name);
            mainControl.Controls.Add(Tab);            
            Tab.Controls.Add(c);
            Tab.CodeBox = c;
            Tab.Text = name;
            Tab.FileName = fname;
            mainControl.SelectedTab = Tab;
            c.ContextMenuStrip = CodeContextMenu;
            CurrentTab = Tab;
            Tab.AutoComp = LoadAutocomplete(Tab.CodeBox);
            ActiveControl = Tab.CodeBox;
            return c;
        }

        private void CloseTab(FTab Tab)
        {
            if (mainControl.TabPages.Count - 1 == 0)
                this.Close();
            else
                mainControl.TabPages.Remove(mainControl.SelectedTab);
        }

        private FastColoredTextBox NewTextBox(string name)
        {
            FastColoredTextBox FCTextBox = new FastColoredTextBox();
            FCTextBox.AutoScrollMinSize = new System.Drawing.Size(55, 15);
            FCTextBox.BackBrush = null;
            FCTextBox.Font = Properties.Settings.Default.Font;
            FCTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            FCTextBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            FCTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            FCTextBox.LeftPadding = 30;
            FCTextBox.LineInterval = 3;
            FCTextBox.Location = new System.Drawing.Point(0, 0);
            FCTextBox.Name = Name;
            FCTextBox.Paddings = new System.Windows.Forms.Padding(0);
            FCTextBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            FCTextBox.Size = new System.Drawing.Size(454, 491);
            FCTextBox.ShowLineNumbers = Properties.Settings.Default.ShowLines;
            FCTextBox.TextChanged += new EventHandler<TextChangedEventArgs>(CodeBox_TextChanged);
            FCTextBox.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(FCTextBox_TextChangedDelayed);
            return FCTextBox;
        }

        public void PB_ErrorHandler(string error)
        {
            StatusBarLabel.Text = error;
            return;
        }

        private AutocompleteMenu LoadAutocomplete(FastColoredTextBox box)
        {
            AutocompleteMenu menu = new AutocompleteMenu(box);
            menu.Items.MaximumSize = new System.Drawing.Size(400, 300);
            menu.Items.MinimumSize = new Size(350, 300);
            menu.MinFragmentLength = 3;
            menu.SearchPattern = @"[\w\.:=!<>]";
            List<string> items = new List<string>();
            foreach (TreeNode k in LibraryView.Nodes)
                foreach (TreeNode n in k.Nodes)
                    items.Add(n.Text);
            menu.Items.SetAutocompleteItems(items.ToArray());
            box.TextChanged += new EventHandler<TextChangedEventArgs>(AutoCompletemenu_Show);
            return menu;
        }

        void AutoCompletemenu_Show(object sender, EventArgs e)
        {
            if(Properties.Settings.Default.UseAutoComplete)
                CurrentTab.AutoComp.Show(false);
        }

        void CodeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Range range, full;
            if (CurrentTab.IsNowOpened)
            {
                full = range = e.ChangedRange;               
            }
            else
            {
                range = (sender as FastColoredTextBox).VisibleRange;
                full = (sender as FastColoredTextBox).Range;
            }

            range.ClearStyle(TextStyles.Values.ToArray());
            range.ClearFoldingMarkers();
            ApplyHighlight(range);

            if (CurrentTab.IsNowOpened == false)
            {
                if (CurrentTab.Changed == false)
                {
                    CurrentTab.Changed = true;
                    CurrentTab.Text = CurrentTab.Text + " *";
                }
            }
            else 
                CurrentTab.IsNowOpened = false;
            
            SelectedEventArgs sel = new SelectedEventArgs();
            CurrentTab.AutoComp.OnSelected(sel);
            
            if (sel.Item != null)
                StatusBarLabel.Text = sel.Item.Text;
        }

        void FCTextBox_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            Range full = (sender as FastColoredTextBox).Range;
            full.SetStyle(TextStyles["Comments"], @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            full.SetStyle(TextStyles["Comments"], @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
        }

        void ApplyHighlight(Range range)
        {
            range.SetFoldingMarkers("{", "}");
            range.SetFoldingMarkers(@"/\*", @"\*/");
            range.SetStyle(TextStyles["Comments"], @"//.*$", RegexOptions.Multiline);
            range.SetStyle(TextStyles["Strings"], @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'");
            range.SetStyle(TextStyles["Digits"], @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");
            range.SetStyle(TextStyles["Params"], @"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline);
            range.SetStyle(TextStyles["Operators"], @"\b(public|private|stock|function|native)\s+(?<range>\w+?)\b");
            range.SetStyle(TextStyles["Operators"], @"\b(assert|break|case|countine|default|do|else|exit|for|goto|if|return|sleep|state|switch|while|defined|sizeof|state|tagof|const|forward|native|new|operator|public|static|stock|private|function)\b|#\w+\s*[A-Za-z0-9.,=<>()_-]+");
        }

        void ApplyHighlight()
        {
            Range range = CurrentTab.CodeBox.Range;
            range.ClearStyle(TextStyles.Values.ToArray());
            range.ClearFoldingMarkers();
            range.SetStyle(TextStyles["Comments"], @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            range.SetStyle(TextStyles["Comments"], @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
            range.SetFoldingMarkers("{", "}");
            range.SetFoldingMarkers(@"/\*", @"\*/");
            range.SetStyle(TextStyles["Comments"], @"//.*$", RegexOptions.Multiline);
            range.SetStyle(TextStyles["Strings"], @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'");
            range.SetStyle(TextStyles["Digits"], @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");
            range.SetStyle(TextStyles["Params"], @"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline);
            range.SetStyle(TextStyles["Operators"], @"\b(public|private|stock|function|native)\s+(?<range>\w+?)\b");
            range.SetStyle(TextStyles["Operators"], @"\b(assert|break|case|countine|default|do|else|exit|for|goto|if|return|sleep|state|switch|while|defined|sizeof|state|tagof|const|forward|native|new|operator|public|static|stock|private|function)\b|#\w+\s*[A-Za-z0-9.,=<>()_-]+");
        }
        
        private void NewFile(object sender, EventArgs e)
        {
            CurrentTab.CodeBox = NewTab(String.Format("newfile{0}.pwn", mainControl.Controls.Count), "");
            this.Text = "PawnBox - " + CurrentTab.Text;
        }

        private void OpenFileButton(object sender, EventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.Filter = "Исходный код Pawno|*.pwn";
            if (f.ShowDialog() == DialogResult.OK)
            {
                FastColoredTextBox c = CurrentTab.CodeBox;
                if (f.FileName.Length > 0)
                {
                    if (mainControl.Controls.Count != 1 || (mainControl.Controls.Count > 0 && c.Text.Length > 0))
                        OpenFile(f.FileName, true);
                    else
                        OpenFile(f.FileName, false);
                    SaveRecentFile(f.FileName);
                }
            }
        }

        private void LoadRecentList()
        {
            MRUlist.Clear();
            try
            {
                StreamReader listToRead = new StreamReader(System.Environment.CurrentDirectory + "\\Recent.txt");
                string line;
                while ((line = listToRead.ReadLine()) != null)
                    MRUlist.Enqueue(line);
                listToRead.Close();
            }
            catch (Exception)
            {
                //throw;
            }
        }

        private void SaveRecentFile(string path)
        {
            RecentFilesToolStripMenuItem.DropDownItems.Clear();
            LoadRecentList();
            if (!(MRUlist.Contains(path)))
                MRUlist.Enqueue(path);
            while (MRUlist.Count > MRUnumber)
            {
                MRUlist.Dequeue();
            }
            foreach (string item in MRUlist)
            {
                ToolStripMenuItem fileRecent = new ToolStripMenuItem(item, null, RecentFile_Click);
                RecentFilesToolStripMenuItem.DropDownItems.Add(fileRecent);
            }
            StreamWriter stringToWrite = new StreamWriter(System.Environment.CurrentDirectory + "\\Recent.txt");
            foreach (string item in MRUlist)
            {
                stringToWrite.WriteLine(item);
            }
            stringToWrite.Flush();
            stringToWrite.Close();
        }

        private void RecentFile_Click(object sender, EventArgs e)
        {
            if(!File.Exists(sender.ToString()))
            {
                StatusBarLabel.Text = String.Format("Ошибка: не удается открыть файл {0}", sender.ToString());
                return;
            }
            FastColoredTextBox c = CurrentTab.CodeBox;
            if (mainControl.Controls.Count != 1 || (mainControl.Controls.Count > 0 && c.Text.Length > 0))
                OpenFile(sender.ToString(), true);
            else
                OpenFile(sender.ToString(), false);
        }

        public void OpenFile(string path, bool newtab)
        {
            if (Path.GetExtension(path).Contains(".pwn") || Path.GetExtension(path).Contains(".inc"))
            {
                try
                {
                    string text = File.ReadAllText(path, Encoding.Default);
                    if (newtab)
                        ActiveControl = CurrentTab.CodeBox = NewTab(Path.GetFileName(CurrentTab.FileName), path);
                    CurrentTab.FileName = path;
                    CurrentTab.CodeBox.Text = text;
                    CurrentTab.Text = Path.GetFileName(CurrentTab.FileName);
                    CurrentTab.Changed = false;
                    CurrentTab.IsNowOpened = true;
                    CurrentTab.CodeBox.GoHome();
                    this.Text = "PawnBox - " + Path.GetFileName(CurrentTab.FileName);
                    ApplyHighlight();
                }
                catch (Exception e)
                {
                    StatusBarLabel.Text = "Ошибка при чтении файла: " + e.Message;
                }
            }
        }

        private void CompileStart(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.SaveBeforeCompiling)
                SaveFile_Function(CurrentTab, false);
            StatusBarLabel.Text = String.Format("Выполняется компиляция файла {0}...", Path.GetFileName(CurrentTab.FileName));
            CurrentTab.CodeBox.ReadOnly = true;
            if (!CurrentTab.FileName.Equals(""))
            {
                if (!File.Exists(Properties.Settings.Default.PathToCompiler))
                {
                    StatusBarLabel.Text = "Ошибка: исполняемый файл компилятора не найден. Проверьте правильность пути в настройках PawnBox.";
                    return;
                }
                Compilation pawncc = new Compilation(CurrentTab.FileName, pawncc_Cancel, this);
                Thread t = new Thread(new ThreadStart(pawncc.Start));
                t.Start();
                CForm.ShowDialog(this);
            }
            else
            {
                StatusBarLabel.Text = "Ошибка: файл не существует.";
            }
        }

        private void pawncc_Cancel(string output)
        {
            CForm.Hide();
            CompiledTab = CurrentTab;
            listView1.Items.Clear();
            int errors = 0;
            ListViewItem[] items = COutput.ToTable(output, out errors);
            if (items.Length > 0)
                splitContainer3.Panel2Collapsed = false;
            else
                splitContainer3.Panel2Collapsed = true;
            listView1.Items.AddRange(items);
            CurrentTab.CodeBox.ReadOnly = false;
            if (errors == 0)
                StatusBarLabel.Text = "Компиляция успешно завершена.";
            else
                StatusBarLabel.Text = string.Format("Компиляция прервана. Ошибок: {0}, предупреждений: {1}", errors, items.Length - errors);
        }

        private void IncludeLoadingComplete(TreeNode[] nodes)
        {
            LibraryView.ShowLines = false;
            LibraryView.Nodes.AddRange(nodes);
            List<string> items = new List<string>();
            foreach (TreeNode k in LibraryView.Nodes)
                foreach (TreeNode n in k.Nodes)
                    items.Add(n.Text);
            CurrentTab.AutoComp.Items.SetAutocompleteItems(items);
        }

        private void SaveFile_Function(FTab Tab, bool SAction)
        {
            if (Tab.FileName.Length == 0 || SAction == true)
            {
                SaveFileDialog f = new SaveFileDialog();
                f.AddExtension = true;
                f.OverwritePrompt = true;
                f.ValidateNames = true;
                f.Filter = "Исходный код Pawno|*.pwn|Библиотека Pawn|*.inc";
                f.ShowDialog();
                if (f.FileName.Length > 0)
                {
                    File.WriteAllText(f.FileName, Tab.CodeBox.Text, Encoding.Default);
                    Tab.FileName = f.FileName;
                    StatusBarLabel.Text = String.Format("Файл {0} успешно сохранен.", Path.GetFileName(f.FileName));
                    Tab.Changed = false;
                    Tab.Text = Path.GetFileName(f.FileName);
                }
            }
            else
            {
                File.WriteAllText(Tab.FileName, Tab.CodeBox.Text, Encoding.Default);
                StatusBarLabel.Text = String.Format("Файл {0} успешно сохранен.", Path.GetFileName(Tab.FileName));
                Tab.Changed = false;
                Tab.Text = Path.GetFileName(Tab.FileName);
            }       
        }

        private void SaveFile_Function(FTab Tab)
        {
            string fname = Path.GetFileNameWithoutExtension(Tab.FileName) + "_" + DateTime.Now.ToString() + ".pwn";
            if (fname.Length == 0)
                fname = "$" + DateTime.Now.ToString() + "_" + (new Random(100).Next()) + "$.pwn";            
            File.WriteAllText(Properties.Settings.Default.PathToRecovery + @"\" + fname, Tab.CodeBox.Text, Encoding.Default);
        }

        private void SaveFile(object sender, EventArgs e)
        {
            SaveFile_Function(CurrentTab, false);   
        }
        
        private void SaveAsFile(object sender, EventArgs e)
        {
            SaveFile_Function(CurrentTab, true);             
        }
        
        private void перейтиКСтрокеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTab.CodeBox.ShowGoToDialog();
        }

        private void поискToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTab.CodeBox.ShowFindDialog();
        }

        private void заменаCtrlHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTab.CodeBox.ShowReplaceDialog();
        }

        void mainControl_Selected(object sender, TabControlEventArgs e)
        {
            CurrentTab = e.TabPage as FTab;
            ActiveControl = e.TabPage;
            this.Text = "PawnBox - " + CurrentTab.Text;
        }

        private void задатьАссоциациюФайловToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PawnBox.FileAssociation.Associate("Исходный код Pawn", Application.StartupPath + @"\File.ico"); 
        }

        private void параметрыToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Settings f = new Settings();
            f.ShowDialog();
        }

        private void ApplyOptions()
        {
            foreach (FTab tab in mainControl.TabPages)
            {
                tab.CodeBox.ShowLineNumbers = Properties.Settings.Default.ShowLines;
                tab.CodeBox.Font = Properties.Settings.Default.Font;
            }
            splitContainer1.Panel2Collapsed = !Properties.Settings.Default.ShowLibrary;
        }   

        void MainForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                SavingForm SaveForm = new SavingForm();
                ListBox ItemContainer = (SaveForm.Controls["FileList"] as ListBox);
                bool AnyTabChanged = false;
                foreach (FTab tab in mainControl.TabPages)
                {
                    if (tab.Changed == true)
                    {
                        AnyTabChanged = true;
                        int idx = (SaveForm.Controls["FileList"] as ListBox).Items.Add(tab.Text.Substring(0, tab.Text.Length - 2));
                        (SaveForm.Controls["FileList"] as ListBox).SetSelected(idx, true);
                    }
                }
                DialogResult result = new DialogResult();
                if (AnyTabChanged)
                {
                    result = SaveForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        SaveForm.Hide();
                        ListBox.SelectedObjectCollection Collection = ItemContainer.SelectedItems;
                        foreach (FTab tab in mainControl.TabPages)
                            if (Collection.Contains(tab.Text.Substring(0, tab.Text.Length - 2))) SaveFile_Function(tab, false);
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        SaveForm.Hide();
                    }
                    else if (result == DialogResult.No)
                    {
                        e.Cancel = false;
                        ListBox.SelectedObjectCollection Collection = ItemContainer.SelectedItems;
                        if (Directory.Exists(Properties.Settings.Default.PathToRecovery))
                        {
                            foreach (FTab tab in mainControl.TabPages)
                                if (!Collection.Contains(tab.Text.Substring(0, tab.Text.Length - 2))) SaveFile_Function(tab);
                            SaveForm.Hide();
                        }
                    }
                }
            }
        }

        void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(e.CloseReason != CloseReason.UserClosing)
            {
                foreach (FTab tab in mainControl.TabPages)
                {
                    string name = Path.GetFileName(tab.FileName);
                    File.WriteAllText(Application.StartupPath + @"\recovery\" + name, tab.CodeBox.Text);
                }
            }           
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTab.CodeBox.Cut();
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTab.CodeBox.Copy();
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTab.CodeBox.Paste();
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Application.StartupPath + @"\include\");
        }

        private void открытьПапкуСФайломToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(CurrentTab.FileName.Length > 0)
                System.Diagnostics.Process.Start(Path.GetDirectoryName(CurrentTab.FileName));
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new AboutBox2()).ShowDialog();
        }

        private void закрытьТекущуюВкладкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentTab.Changed == false)
                CloseTab(CurrentTab);
                
            DialogResult result = new DialogResult();
            result = MessageBox.Show(String.Format("Сохранить изменения в файле {0}?", CurrentTab.Text.Substring(0, CurrentTab.Text.Length - 2)), "Сохранение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                SaveFile_Function(CurrentTab, false);
                CloseTab(CurrentTab);
            }
            else if (result == DialogResult.No)
                CloseTab(CurrentTab);
            else if (result == DialogResult.Cancel)
                return;
        }
    }

    class FTab : TabPage
    {
        public string FileName = "";
        public bool Changed = false, IsNowOpened = false;
        public AutocompleteMenu AutoComp;
        public FastColoredTextBox CodeBox;
    }
}
