using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StandardGrid
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            
        }

        

        private void MainForm_Load(object sender, EventArgs e)
        {
            ScaleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            ScaleComboBox.Items.Add("1:10000");
            ScaleComboBox.Items.Add("1:50000");
            ScaleComboBox.Items.Add("1:100000");
            ScaleComboBox.Items.Add("1:500000");
            ScaleComboBox.SelectedIndex = 0;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void InputButton_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog = new OpenFileDialog();
            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.InputTextBox.Text = OpenFileDialog.FileName;
            }
        }

        private void OutputButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FolderBrowserDialog = new FolderBrowserDialog();
            if (FolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.OutputTextBox.Text = FolderBrowserDialog.SelectedPath;
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            GridCreater Creater = new GridCreater();
            Creater.ImagePath = this.InputTextBox.Text;
            Creater.TargetFolder = this.OutputTextBox.Text;
            Creater.Scale = this.ScaleComboBox.Text;
            Creater.Create();
        }

        

       

        

        
    }
}
