using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CFS23SaveTool
{
    public class MainForm : Form
    {
        private const string Key = "4282840";
        private const string DefaultFileName = "gamesaves.cfs23";

        private TextBox txtPath;
        private Button btnBrowse;
        private Button btnDecrypt;
        private Button btnEncrypt;
        private Label lblStatus;

        public MainForm()
        {
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "CFS23 Save Tool";
            ClientSize = new System.Drawing.Size(560, 200);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new System.Drawing.Font("Segoe UI", 9F);

            var lblInfo = new Label
            {
                Text = "Select a file (gamesaves.cfs23). Both decompression and " +
                       "compression use the same XOR key used by the game.",
                AutoSize = false,
                Size = new System.Drawing.Size(530, 40),
                Location = new System.Drawing.Point(15, 12)
            };

            txtPath = new TextBox
            {
                Location = new System.Drawing.Point(15, 60),
                Size = new System.Drawing.Size(420, 23),
                ReadOnly = true
            };

            btnBrowse = new Button
            {
                Text = "Select File...",
                Location = new System.Drawing.Point(445, 58),
                Size = new System.Drawing.Size(100, 27)
            };
            btnBrowse.Click += (s, e) => BrowseForFile();

            btnDecrypt = new Button
            {
                Text = "Decompress",
                Location = new System.Drawing.Point(15, 100),
                Size = new System.Drawing.Size(160, 32),
                Enabled = false
            };
            btnDecrypt.Click += (s, e) => ProcessFile(".json");

            btnEncrypt = new Button
            {
                Text = "Compress",
                Location = new System.Drawing.Point(185, 100),
                Size = new System.Drawing.Size(160, 32),
                Enabled = false
            };
            btnEncrypt.Click += (s, e) => ProcessFile(".cfs23");

            lblStatus = new Label
            {
                Text = "No file selected.",
                AutoSize = false,
                Size = new System.Drawing.Size(530, 40),
                Location = new System.Drawing.Point(15, 145)
            };

            Controls.Add(lblInfo);
            Controls.Add(txtPath);
            Controls.Add(btnBrowse);
            Controls.Add(btnDecrypt);
            Controls.Add(btnEncrypt);
            Controls.Add(lblStatus);
        }

        private void BrowseForFile()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Select save file";
                dlg.Filter = "Save files (*.cfs23;*.json)|*.cfs23;*.json|CFS23 file (*.cfs23)|*.cfs23|JSON file (*.json)|*.json|All files (*.*)|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtPath.Text = dlg.FileName;
                    UpdateAvailableActions(dlg.FileName);
                }
            }
        }

        private void UpdateAvailableActions(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();

            if (ext == ".cfs23")
            {
                btnDecrypt.Enabled = true;
                btnEncrypt.Enabled = false;
                lblStatus.Text = "CFS23 file selected. You can decompress it to JSON.";
            }
            else if (ext == ".json")
            {
                btnDecrypt.Enabled = false;
                btnEncrypt.Enabled = true;
                lblStatus.Text = "JSON file selected. You can compress it back to CFS23.";
            }
            else
            {
                btnDecrypt.Enabled = false;
                btnEncrypt.Enabled = false;
                lblStatus.Text = "Unsupported file type. Select a .cfs23 or .json file.";
            }
        }

        private void ProcessFile(string extension)
        {
            string source = txtPath.Text;
            if (string.IsNullOrEmpty(source) || !File.Exists(source))
            {
                lblStatus.Text = "No valid file selected.";
                return;
            }

            try
            {
                byte[] data = File.ReadAllBytes(source);
                byte[] result = XorData(data, Key);

                string output = BuildOutputPath(source, extension);
                File.WriteAllBytes(output, result);

                lblStatus.Text = "Done! Output saved to: " + output;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
            }
        }

        private static string BuildOutputPath(string source, string extension)
        {
            string dir = Path.GetDirectoryName(source);
            string name = Path.GetFileNameWithoutExtension(source);

            string candidate = Path.Combine(dir, name + extension);
            int i = 1;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(dir, name + "(" + i + ")" + extension);
                i++;
            }
            return candidate;
        }

        private static byte[] XorData(byte[] data, string key)
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return result;
        }
    }
}
