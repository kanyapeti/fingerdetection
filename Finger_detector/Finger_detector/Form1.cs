using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math;
using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Finger_detector
{
    public partial class Form1 : Form
    {
        String filename;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
    
            //A szűrő után atszínezni az élek között fehérre égy ugyanúgy detektálni, mint most.
            Bitmap colorFiltered = ColorFiltering((Bitmap)Bitmap.FromFile(filename));

           // Bitmap colorFiltered = (Bitmap)Bitmap.FromFile(filename);

            int howMany = RectangleCounting(colorFiltered);

           label1.Text = howMany.ToString();

            pictureBox2.Image = colorFiltered; 
            
        }

        //Segédobjektumokkal való feldolgozás.
        private void ProcessImage(Bitmap bitmap)
        {

            ColorFiltering colorFilter = new ColorFiltering();

            //Ezzel a szürkés-fehér objektumokat ki lehet szűrni.
            //rgb(197, 121, 76)
            colorFilter.Red = new IntRange(170, 220);
            colorFilter.Green = new IntRange(100, 140);
            colorFilter.Blue = new IntRange(50, 100);
            
            //Itt az előbb meghatározott objektumokon kívűl mindent feketére fest -> könnyebb detektálhatóság.
            colorFilter.FillOutsideRange = true;

            colorFilter.ApplyInPlace(bitmap);
            
            // step 2 - locating objects
            BlobCounter blobCounter = new BlobCounter();

            //Ezzel mondom meg, hogy szűrje ki.
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 5;
            blobCounter.MinWidth = 5;

            blobCounter.ProcessImage(bitmap);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            // and to picture box
            pictureBox1.Image = bitmap;

            UpdatePictureBoxPosition();
        }

        private void UpdatePictureBoxPosition()
        {
            int imageWidth;
            int imageHeight;

            if (pictureBox1.Image == null)
            {
                imageWidth = 320;
                imageHeight = 240;
            }
            else
            {
                imageWidth = pictureBox1.Image.Width;
                imageHeight = pictureBox1.Image.Height;
            }

            pictureBox1.SuspendLayout();
            pictureBox1.Size = new Size(imageWidth + 2, imageHeight + 2);
            pictureBox1.ResumeLayout();
        }

        public Bitmap CuttingBefore(Bitmap bitmap)
        {
            

            for (int i = 0; i < 120; i++)
                for (int j = 0; j < bitmap.Height; j++)
                    bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));

            return bitmap;

        }

        public Bitmap CuttingAfter(Bitmap bitmap)
        {
            for (int i = 180; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                    bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));

            return bitmap;
        }
        public Bitmap ColorFiltering(Bitmap bitmap)
        {

            Thread[] szalak = new Thread[4];
         
                szalak[0] = new Thread(delegate () {
                    lock (bitmap)
                    {
                        for (int i = 180; i < 180+((bitmap.Width-180)/4); i++)
                            for (int j = 0; j < bitmap.Height; j++)
                                bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                     }
                });
           
                szalak[1] = new Thread(delegate () {
                lock (bitmap)
                {
                    for (int i = 180 + ((bitmap.Width - 180) / 4); i < 180 + 2*((bitmap.Width - 180) / 4); i++)
                        for (int j = 0; j < bitmap.Height; j++)
                            bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                });

                szalak[2] = new Thread(delegate () {
                    lock (bitmap)
                    {
                        for (int i = 180 + 2 * ((bitmap.Width - 180) / 4); i < 180 + 3 * ((bitmap.Width - 180) / 4); i++)
                            for (int j = 0; j < bitmap.Height; j++)
                                bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                });
         
                szalak[3] = new Thread(delegate (){
                lock (bitmap)
                {
                    for (int i = 180 + 3 * ((bitmap.Width - 180) / 4); i < bitmap.Width; i++)
                        for (int j = 0; j < bitmap.Height; j++)
                            bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                });
            

            foreach (Thread t in szalak)
                t.Start();
            foreach (Thread t in szalak)
                t.Join();

            Thread[] threads = new Thread[3];



            threads[0] = new Thread(delegate () {
                lock (bitmap)
                {
                    for (int i = 40; i < 80; i++)
                        for (int j = 0; j < bitmap.Height; j++)
                            bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                }
            });

            threads[1] = new Thread(delegate () {
                lock (bitmap)
                {
                    for (int i = 80; i < 120; i++)
                        for (int j = 0; j < bitmap.Height; j++)
                            bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                }
            });

            threads[2] = new Thread(delegate () {
                lock (bitmap)
                {
                    for (int i = 0; i < 40; i++)
                        for (int j = 0; j < bitmap.Height; j++)
                            bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                }
            });

            foreach (Thread t in threads)
                t.Start();
            foreach (Thread t in threads)
                t.Join();

            ColorFiltering colorFilter = new ColorFiltering();

            //rgb(197, 121, 76)
            colorFilter.Red = new IntRange(160, 230);
            colorFilter.Green = new IntRange(90, 150);
            colorFilter.Blue = new IntRange(40, 110);
            //colorFilter.FillColor=
            colorFilter.FillOutsideRange = true;

            colorFilter.ApplyInPlace(bitmap);

            pictureBox1.Image = bitmap;

            return bitmap;
        }

        public int RectangleCounting(Bitmap bitmap)
        {
            BlobCounter bc = new BlobCounter();
            // process binary image
            bc.FilterBlobs = true;
            //Ettől kisebb négyzeteket ne számoljon, mert az csak hibát okozhat.
            bc.MinHeight = 20;
            bc.MinWidth = 20;
            //bc.MaxHeight=
            bc.ProcessImage(bitmap);
            Rectangle[] rects = bc.GetObjectsRectangles();

            return rects.Count();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filename=openFileDialog1.FileName;
                    pictureBox2.Image = (Bitmap)Bitmap.FromFile(filename);
                }
                catch
                {
                    MessageBox.Show("Failed loading selected image file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
