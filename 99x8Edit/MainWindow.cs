﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace _99x8Edit 
{
    // Main window
    public partial class MainWindow : Form
    {
        private Machine dataSource;
        private PCGEditor PCGWin;
        private Map mapWin;
        private Sprites spriteWin;
        private Peek peekWin;
        private About aboutWin;
        private String currentFile = @"";
        public MainWindow()
        {
            InitializeComponent();
        }
        public bool UndoEnable
        {
            set
            {
                btnUndo.Enabled = value;
            }
        }
        public bool RedoEnable
        {
            set
            {
                btnRedo.Enabled = value;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize VDP settings
            dataSource = new Machine();
            dataSource.Initialize();
            Stream s = new MemoryStream(Properties.Resources._default);
            try
            {
                dataSource.LoadAllSettings(new BinaryReader(s));    // Init by resource
            }
            finally
            {
                s.Close();
            }
            // Undo/Redo            
            MementoCaretaker.Instance.Initialize(this, dataSource);
            // Export menu
            foreach (String item in dataSource.exportTypeList)
            {
                comboExportType.Items.Add(item);   // Code it since items may be expanded
            }
            comboExportType.SelectedIndex = 0;
            // Editors
            PCGWin = new PCGEditor(dataSource);
            mapWin = new Map(dataSource);
            spriteWin = new Sprites(dataSource);
            // Check drag and drop of files
            String[] args = System.Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                // See the last args since many files may have been dropped
                String dnd_path = args[args.Length - 1];
                this.loadFile(dnd_path);
            }
            // Open PCG editor as default
            PCGWin.Show();
        }
        //----------------------------------------------------------------------
        // Controls
        private void btnPCGWin_Click(object sender, EventArgs e)
        {
            if (PCGWin.IsDisposed)
            {
                PCGWin = new PCGEditor(dataSource);
            }
            if (PCGWin.Visible)
            {
                PCGWin.Hide();
            }
            else
            {
                PCGWin.Show();
                PCGWin.BringToFront();
            }
        }
        private void btnMapWin_MouseClick(object sender, MouseEventArgs e)
        {
            if (mapWin.IsDisposed)
            {
                mapWin = new Map(dataSource);
            }
            if (mapWin.Visible)
            {
                mapWin.Hide();
            }
            else
            {
                mapWin.Show();
                mapWin.BringToFront();
            }
        }
        private void btnSpritesWin_Click(object sender, EventArgs e)
        {
            if (spriteWin.IsDisposed)
            {
                spriteWin = new Sprites(dataSource);
            }
            if (spriteWin.Visible)
            {
                spriteWin.Hide();
            }
            else
            {
                spriteWin.Show();
                spriteWin.BringToFront();
            }
        }
        private void btnSavePCG_Click(object sender, EventArgs e)
        {
            String dir = Path.GetDirectoryName(currentFile);
            if (dir == null)
            {
                String target = this.saveDialog(dir);
                if (target != null)
                {
                    this.saveFile(target);      // Save and update current file
                }
            }
            else
            {
                this.saveFile(currentFile);
            }
        }
        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            String dir = Path.GetDirectoryName(currentFile);
            if (dir == null)
            {
                dir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
            String target = this.saveDialog(dir);
            if (target != null)
            {
                this.saveFile(target);
            }
        }
        private void btnLoadPCG_Click(object sender, EventArgs e)
        {
            String dir = Path.GetDirectoryName(currentFile);
            if (dir == null)
            {
                dir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = dir;
            dlg.Filter = "VDP File(*.vdp)|*.vdp";
            dlg.FilterIndex = 1;
            dlg.Title = "Load settings";
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.loadFile(dlg.FileName);
            }
        }
        private void btnPCGExport_Click(object sender, EventArgs e)
        {
            String dir = Path.GetDirectoryName(currentFile);
            if (dir == null)
            {
                dir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
            String ext = dataSource.exportTypeExt[comboExportType.SelectedIndex];
            SaveFileDialog dlg = new SaveFileDialog();
            String filter = ext + " files(*" + ext + ")|*" + ext;
            dlg.FileName = "";
            dlg.InitialDirectory = dir;
            dlg.Filter = filter;
            dlg.FilterIndex = 1;
            dlg.Title = "Save to";
            dlg.RestoreDirectory = true;
            dlg.OverwritePrompt = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    dataSource.ExportPCG((Machine.ExportType)comboExportType.SelectedIndex, dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnExportMap_Click(object sender, EventArgs e)
        {
            String dir = Path.GetDirectoryName(currentFile);
            if (dir == null)
            {
                dir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
            String ext = dataSource.exportTypeExt[comboExportType.SelectedIndex];
            SaveFileDialog dlg = new SaveFileDialog();
            String filter = ext + " files(*" + ext + ")|*" + ext;
            dlg.FileName = "";
            dlg.InitialDirectory = dir;
            dlg.Filter = filter;
            dlg.FilterIndex = 1;
            dlg.Title = "Save to";
            dlg.RestoreDirectory = true;
            dlg.OverwritePrompt = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    dataSource.ExportMap((Machine.ExportType)comboExportType.SelectedIndex, dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnExportSprites_Click(object sender, EventArgs e)
        {
            String dir = Path.GetDirectoryName(currentFile);
            if (dir == null)
            {
                dir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
            String ext = dataSource.exportTypeExt[comboExportType.SelectedIndex];
            SaveFileDialog dlg = new SaveFileDialog();
            String filter = ext + " files(*" + ext + ")|*" + ext;
            dlg.FileName = "";
            dlg.InitialDirectory = dir;
            dlg.Filter = filter;
            dlg.FilterIndex = 1;
            dlg.Title = "Save to";
            dlg.RestoreDirectory = true;
            dlg.OverwritePrompt = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    dataSource.ExportSprites((Machine.ExportType)comboExportType.SelectedIndex, dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnUndo_Click(object sender, EventArgs e)
        {
            // Undo
            MementoCaretaker.Instance.Undo();
            // Update UI
            PCGWin.ChangeOccuredByHost();
            mapWin.ChangeOccuredByHost();
            spriteWin.ChangeOccuredByHost();
        }
        private void btnRedo_Click(object sender, EventArgs e)
        {
            // Redo
            MementoCaretaker.Instance.Redo();
            // Update UI
            PCGWin.ChangeOccuredByHost();
            mapWin.ChangeOccuredByHost();
            spriteWin.ChangeOccuredByHost();
        }
        String peekPath = "";
        private void btnPeek_Click(object sender, EventArgs e)
        {
            if (peekWin != null)
            {
                if (peekWin.IsDisposed == false)
                {
                    if (peekWin.Visible)
                    {
                        peekWin.Hide();
                    }
                    else
                    {
                        peekWin.Show();
                        peekWin.BringToFront();
                    }
                    return;
                }
            }
            // Create window
            String dir = Path.GetDirectoryName(peekPath);
            if (dir == null)
            {
                dir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = dir;
            dlg.Filter = "Rom(*.rom)|*.rom|All files(*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.Title = "Open rom or other file to peek in";
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                peekWin = new Peek(dlg.FileName);
                peekWin.Show();
                peekPath = dlg.FileName;
            }
        }
        private void btnAbout_Click(object sender, EventArgs e)
        {
            if (aboutWin != null)
            {
                if (aboutWin.IsDisposed == false)
                {
                    aboutWin.Show();
                    aboutWin.BringToFront();
                    return;
                }
            }
            aboutWin = new About();
            aboutWin.Show();
        }
        //----------------------------------------------------------------------
        // Utilities
        private void saveFile(String path)
        {
            // Save and update current file path if OK
            BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create));
            try
            {
                dataSource.SaveAllSettings(br);
                currentFile = path;
                PCGWin.CurrentFile = currentFile;
                spriteWin.CurrentFile = currentFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                currentFile = "";
                PCGWin.CurrentFile = currentFile;
                spriteWin.CurrentFile = currentFile;
            }
            finally
            {
                br.Close();
            }
        }
        private String saveDialog(String dir)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "";
            dlg.InitialDirectory = dir;
            dlg.Filter = "VDP File(*.vdp)|*.vdp";
            dlg.FilterIndex = 1;
            dlg.Title = "Save settings";
            dlg.RestoreDirectory = true;
            dlg.OverwritePrompt = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
            }
            return null;
        }
        private void loadFile(String path)
        {
            BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open));
            try
            {
                dataSource.LoadAllSettings(br);
                currentFile = path;
                PCGWin.CurrentFile = currentFile;
                spriteWin.CurrentFile = currentFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                currentFile = "";
                PCGWin.CurrentFile = "";
                spriteWin.CurrentFile = "";
            }
            finally
            {
                br.Close();
            }
            // Update UI
            PCGWin.ChangeOccuredByHost();
            mapWin.ChangeOccuredByHost();
            spriteWin.ChangeOccuredByHost();
            // Clear mementos
            MementoCaretaker.Instance.Clear();
        }
    }
}
