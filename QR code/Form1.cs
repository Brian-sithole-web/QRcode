using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QRCoder;

namespace QR_code
{
    public partial class Form1 : Form
    {
        private const double LogoWidthRatio = 0.30;
        private const double CircleMarginRatio = 0.05;

        private Image uploadedLogo;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string content = textBox1.Text.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Enter text or a website address first.");
                return;
            }

            using (Bitmap qrCodeImage = CreateQrCodeBitmap(content))
            {
                Bitmap displayImage = uploadedLogo == null
                    ? new Bitmap(qrCodeImage)
                    : AddLogoToQrCode(qrCodeImage, uploadedLogo);
                ReplacePictureBoxImage(displayImage);
            }
        }

        private void uploadLogoButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Upload logo";
                openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
                openFileDialog.CheckFileExists = true;

                if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    Bitmap loadedLogo = LoadLogoCopy(openFileDialog.FileName);
                    ReplaceUploadedLogo(loadedLogo);
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("The selected file could not be opened as an image.");
                }
                catch (OutOfMemoryException)
                {
                    MessageBox.Show("The selected file could not be opened as an image.");
                }
                catch (IOException)
                {
                    MessageBox.Show("The selected file could not be opened as an image.");
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            DisposeImage(ref uploadedLogo);
            DisposeOwnedImage(pictureBox1);
            DisposeOwnedImage(logoPreviewPictureBox);
            base.OnFormClosed(e);
        }

        private static Bitmap CreateQrCodeBitmap(string content)
        {
            using (QRCodeGenerator qrCodeGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.H))
            using (QRCode qrCode = new QRCode(qrCodeData))
            {
                return qrCode.GetGraphic(10);
            }
        }

        private static Bitmap AddLogoToQrCode(Bitmap qrCodeImage, Image logo)
        {
            Bitmap result = new Bitmap(qrCodeImage.Width, qrCodeImage.Height);
            int logoSize = Math.Max(1, (int)Math.Round(qrCodeImage.Width * LogoWidthRatio));
            int margin = Math.Max(2, (int)Math.Round(logoSize * CircleMarginRatio));
            int plateSize = logoSize + (margin * 2);
            int plateX = (qrCodeImage.Width - plateSize) / 2;
            int plateY = (qrCodeImage.Height - plateSize) / 2;
            Rectangle plate = new Rectangle(plateX, plateY, plateSize, plateSize);
            Rectangle logoBounds = GetCenteredBounds(plate, logo, logoSize);

            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(qrCodeImage, 0, 0, qrCodeImage.Width, qrCodeImage.Height);

                using (SolidBrush plateBrush = new SolidBrush(Color.Black))
                {
                    graphics.FillEllipse(plateBrush, plate);
                }

                graphics.DrawImage(logo, logoBounds);
            }

            return result;
        }

        private static Rectangle GetCenteredBounds(Rectangle plate, Image logo, int maximumSide)
        {
            double scale = Math.Min(
                (double)maximumSide / logo.Width,
                (double)maximumSide / logo.Height);
            int drawWidth = Math.Max(1, (int)Math.Round(logo.Width * scale));
            int drawHeight = Math.Max(1, (int)Math.Round(logo.Height * scale));
            int x = plate.X + ((plate.Width - drawWidth) / 2);
            int y = plate.Y + ((plate.Height - drawHeight) / 2);
            return new Rectangle(x, y, drawWidth, drawHeight);
        }

        private static Bitmap LoadLogoCopy(string filePath)
        {
            using (Image imageFromFile = Image.FromFile(filePath))
            {
                return new Bitmap(imageFromFile);
            }
        }

        private void ReplaceUploadedLogo(Bitmap loadedLogo)
        {
            Image previousLogo = uploadedLogo;
            uploadedLogo = loadedLogo;

            Image previousPreview = logoPreviewPictureBox.Image;
            logoPreviewPictureBox.Image = new Bitmap(loadedLogo);

            DisposeImage(previousLogo);
            DisposeImage(previousPreview);
        }

        private void ReplacePictureBoxImage(Bitmap displayImage)
        {
            Image previousImage = pictureBox1.Image;
            pictureBox1.Image = displayImage;
            DisposeImage(previousImage);
        }

        private static void DisposeOwnedImage(PictureBox pictureBox)
        {
            Image image = pictureBox.Image;
            pictureBox.Image = null;
            DisposeImage(image);
        }

        private static void DisposeImage(Image image)
        {
            if (image != null)
            {
                image.Dispose();
            }
        }

        private static void DisposeImage(ref Image image)
        {
            DisposeImage(image);
            image = null;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
