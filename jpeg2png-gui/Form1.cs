using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace jpeg2png_gui
{
    public partial class Form1 : Form
    {
        List<string> selectedFilePaths = new List<string>();
        int currentImageIndex = 0;

        public Form1()
        {
            InitializeComponent();

            // Set the label1 default text
            label1.Text = "Ready";

            // Set dark mode colors for the form and controls
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.BackColor = Color.FromArgb(30, 30, 30);

            button1.BackColor = Color.FromArgb(28, 28, 28);
            button1.ForeColor = Color.White;
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);

            button2.BackColor = Color.FromArgb(28, 28, 28);
            button2.ForeColor = Color.White;
            button2.FlatStyle = FlatStyle.Flat;
            button2.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);
            button2.MouseEnter += Button2_MouseEnter;
            button2.MouseLeave += Button2_MouseLeave;

            comboBox1.BackColor = Color.FromArgb(28, 28, 28);
            comboBox1.ForeColor = Color.White;
            comboBox1.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox1.DrawItem += ComboBox_DrawItem;
            comboBox1.Items.AddRange(new object[] {
                "Select Weight",
                "0.0",
                "0.1",
                "0.2",
                "0.3",
                "0.4",
                "0.5 [RECOMMENDED]",
                "0.6",
                "0.7",
                "0.8",
                "0.9",
                "1.0"
            });
            comboBox1.SelectedIndex = 0;

            comboBox2.BackColor = Color.FromArgb(28, 28, 28);
            comboBox2.ForeColor = Color.White;
            comboBox2.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox2.DrawItem += ComboBox_DrawItem;
            comboBox2.Items.AddRange(new object[] {
                "Select Iterations",
                "25",
                "50",
                "75",
                "100 [RECOMMENDED]",
                "125",
                "150",
                "175",
                "200"
            });
            comboBox2.SelectedIndex = 0;

            groupBox1.BackColor = Color.FromArgb(45, 45, 48);
            groupBox1.ForeColor = Color.White;

        }

        private void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            e.DrawBackground();
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(28, 28, 28)), e.Bounds);

            if (e.Index >= 0)
            {
                string text = comboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(text, e.Font, new SolidBrush(Color.White), e.Bounds);
            }
            e.DrawFocusRectangle();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select JPEG files",
                Filter = "JPEG Files (*.jpeg;*.jpg)|*.jpeg;*.jpg",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePaths = openFileDialog.FileNames.ToList();
                DisplayImage(0);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedFilePaths.Count == 0)
            {
                MessageBox.Show("Please select files first.");
                return;
            }

            if (comboBox1.SelectedIndex == 0 || comboBox2.SelectedIndex == 0)
            {
                MessageBox.Show("Please select valid options for both weight and iterations.");
                return;
            }

            // Set label text to "Processing" before starting the processing
            label1.Text = "Processing";
            progressBar1.Value = 0; // Reset progress bar

            string weightText = comboBox1.SelectedItem.ToString().Split(' ')[0];
            string iterationsText = comboBox2.SelectedItem.ToString().Split(' ')[0];

            float weight = float.Parse(weightText);
            int iterations = int.Parse(iterationsText);

            int completed = 0;
            foreach (string filePath in selectedFilePaths)
            {
                string outputFilePath = Path.ChangeExtension(filePath, ".png");

                if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "PNG Files (*.png)|*.png",
                        DefaultExt = "png",
                        FileName = outputFilePath
                    };

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        outputFilePath = saveFileDialog.FileName;
                    }
                    else
                    {
                        continue;
                    }
                }

                string arguments = $"-o \"{outputFilePath}\" -w {weight} -i {iterations} \"{filePath}\"";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "jpeg2png.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                try
                {
                    using (Process process = Process.Start(startInfo))
                    {
                        process.WaitForExit();
                    }
                    completed++;
                    this.Text = $"Processing files... [{completed}/{selectedFilePaths.Count}]";
                    progressBar1.Value = (completed * 100) / selectedFilePaths.Count;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while processing the file: " + ex.Message);
                }
            }

            MessageBox.Show("Processing complete. All output files are saved.");
            label1.Text = "Ready"; // Revert label1 text to "Ready"
            this.Text = "jpeg2png GUI";
            progressBar1.Value = 100; // Ensure progress bar is full at completion
        }

        private void DisplayImage(int index)
        {
            if (index >= 0 && index < selectedFilePaths.Count)
            {
                try
                {
                    pictureBox1.Image = Image.FromFile(selectedFilePaths[index]);
                    currentImageIndex = index;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading the image: " + ex.Message);
                }
            }
        }

        private void Button2_MouseEnter(object sender, EventArgs e)
        {
            label1.Text = "Press Shift to set output location."; // Update label1 text on hover for button2
        }

        private void Button2_MouseLeave(object sender, EventArgs e)
        {
            label1.Text = "Ready"; // Revert label1 text when not hovering over button2
        }

        private void groupBox1_Enter(object sender, EventArgs e) { }

        private void pictureBox1_Click(object sender, EventArgs e) { }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) { }

        private void progressBar1_Click(object sender, EventArgs e) { }

        private void label1_Click(object sender, EventArgs e) { }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}