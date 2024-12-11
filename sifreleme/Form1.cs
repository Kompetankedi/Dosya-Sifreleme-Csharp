using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace sifreleme
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private bool isDragging = false;
        private Point mouseOffset;
        private Point formPosition;
        private string filePath;
        private string password;
        private void Form1_Load(object sender, EventArgs e)
        {
            CenterToScreen();
        }
        private byte[] GenerateKey(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
        private byte[] EncryptData(byte[] data, string password)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GenerateKey(password);
                aes.GenerateIV();

                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }

                    return ms.ToArray();
                }
            }
        }


        private byte[] DecryptData(byte[] encryptedData, string password)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GenerateKey(password);

                using (var ms = new MemoryStream(encryptedData))
                {
                    byte[] iv = new byte[aes.IV.Length];
                    ms.Read(iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var result = new MemoryStream())
                        {
                            cs.CopyTo(result);
                            return result.ToArray();
                        }
                    }
                }
            }
        }


        private void btnSelect_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    textBox1.Text = filePath;
                }
            }
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            password = txtPassword.Text;
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(filePath))
            {
                MessageBox.Show("Lütfen dosya seçin ve şifre girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                byte[] encryptedData = EncryptData(data, password);
                File.WriteAllBytes(filePath, encryptedData);
                MessageBox.Show("Dosya başarıyla şifrelendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şifreleme sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            password = txtPassword.Text;
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(filePath))
            {
                MessageBox.Show("Lütfen dosya seçin ve şifre girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                byte[] decryptedData = DecryptData(encryptedData, password);
                File.WriteAllBytes(filePath, decryptedData);
                MessageBox.Show("Dosya başarıyla çözüldü.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (CryptographicException)
            {
                MessageBox.Show("Yanlış şifre.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şifre çözme sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button3_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            mouseOffset = e.Location;
            formPosition = Location;
        }

        private void button3_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging)
            {
                Cursor = Cursors.Default;
            }
            else
            {
                Cursor = Cursors.SizeAll;
            }
        }

        private void button3_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.Button == MouseButtons.Left)
            {
                Point currentPosition = PointToScreen(e.Location);
                Location = new Point(currentPosition.X - mouseOffset.X, currentPosition.Y - mouseOffset.Y);
            }
        }
    }
}
