using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Notepad
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_VSCROLL = 0x115;
        private const int SB_LINEUP = 0;
        private const int SB_LINEDOWN = 1;

        private Stack<TextMemento> mementoStack = new Stack<TextMemento>();
        private Stack<TextMemento> redoStack = new Stack<TextMemento>();
        private string currentFilePath = null;
        private string lastSavedText = string.Empty;

        private Command _openCommand;
        private Command _saveCommand;

        public Form1()
        {
            InitializeComponent();
            this.Resize += Form1_Resize;

            _openCommand = new OpenFileCommand(this);
            _saveCommand = new SaveFileCommand(this);

            openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;

            openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;

            UpdateFormTitle();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _openCommand.Execute();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _saveCommand.Execute();
        }

        public void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовий документ (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(openFileDialog.FileName))
                    {
                        textBox1.Text = sr.ReadToEnd();
                        currentFilePath = openFileDialog.FileName;
                        UpdateFormTitle();
                        mementoStack.Clear();
                        lastSavedText = textBox1.Text;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при відкритті файлу: " + ex.Message);
                }
            }
        }

        public void SaveFile()
        {
            if (currentFilePath != null)
            {
                SaveToFile(currentFilePath);
                UpdateFormTitle();
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Текстовий документ (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = saveFileDialog.FileName;
                    SaveToFile(currentFilePath);
                    UpdateFormTitle();
                }
            }
        }

        private void SaveToFile(string filePath)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.Write(textBox1.Text);
                    lastSavedText = textBox1.Text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при збереженні файлу: " + ex.Message);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            textBox1.Size = new System.Drawing.Size(ClientSize.Width, ClientSize.Height);
        }

        private void UpdateFormTitle()
        {
            if (currentFilePath != null)
            {
                string fileName = Path.GetFileName(currentFilePath);
                if (IsTextChanged())
                {
                    this.Text = $"{fileName} (не збережено)";
                }
                else
                {
                    this.Text = fileName;
                }
            }
            else
            {
                this.Text = "Без імені";
            }
        }

        private bool IsTextChanged()
        {
            if (lastSavedText == string.Empty)
                return false;

            return textBox1.Text != lastSavedText;
        }
    }

    public abstract class Command
    {
        public abstract void Execute();
    }

    public class OpenFileCommand : Command
    {
        private readonly Form1 _form;

        public OpenFileCommand(Form1 form)
        {
            _form = form;
        }

        public override void Execute()
        {
            _form.OpenFile();
        }
    }

    public class SaveFileCommand : Command
    {
        private readonly Form1 _form;

        public SaveFileCommand(Form1 form)
        {
            _form = form;
        }

        public override void Execute()
        {
            _form.SaveFile();
        }
    }

    public class TextMemento
    {
        public string Text { get; }

        public TextMemento(string text)
        {
            Text = text;
        }
    }
}
