using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FriendlyRemoteClientWinForms
{
    public partial class Form1 : Form
    {
        private Queue<byte[]> _bytes = new Queue<byte[]>();

        public static byte[] ImageToByte(Bitmap image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        public Form1()
        {
            InitializeComponent();

            Thread consume = new Thread(ConsumeImageQueue);
            Thread t = new Thread(EnqueueScreenImages);
            consume.Start();
            t.Start();
        }

        private void ConsumeImageQueue()
        {
            while (true)
            {
                Thread.Sleep(40);

                if (_bytes.Count > 0)
                {
                    byte[] ba = _bytes.Dequeue();

                    richTextBox1.Text = $"{ba.Length.ToString()} number of bytes, items in queue: {_bytes.Count}";

                    using (var ms = new MemoryStream(ba))
                    {
                        var img = new Bitmap(ms);
                        if (this.pictureBox1 != null)
                            this.pictureBox1.Image = img;    
                    }
                }
            }
        }

        private void EnqueueScreenImages()
        {
            while (true)
            {
                try
                {
                    var pictureBox = this.pictureBox1;
                    this.pictureBox1.Width = ClientSize.Width;
                    this.pictureBox1.Height = ClientSize.Height;
                    var picWidth = this.pictureBox1.Width;
                    var picHeight = this.pictureBox1.Height;

                    Thread.Sleep(40);

                    var bmp = ScreenCapture.ResizeImage(ScreenCapture.CaptureDesktop(), picWidth, picHeight);
                    _bytes.Enqueue(ImageToByte(bmp, ImageFormat.Bmp));
                }
                catch (Exception e)
                {
                }
            }
        }

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // throw new System.NotImplementedException();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}