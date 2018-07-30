using System;
using System.Timers;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using YeelightAPI;

namespace YeelightDesktop
{
    class Program
    {
        static private Bitmap screenCapture;
        static private Color screenColor = new Color();
        static private int scaleFactor;
        static private string deviceAddress;
        static private Device yeelightDevice = new Device("");

        static void Main(string[] args)
        {
            try
            {
                deviceAddress = args[0];
            } catch
            {
                Console.Write("Enter Yeelight device IP or hostname: ");
                deviceAddress = Console.ReadLine();
            }

            try
            {
                scaleFactor = int.Parse(args[1]);
            }
            catch {
                scaleFactor = 4;
            }

            InitiateYeelightDevice(deviceAddress);

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(UpdateLights);
            timer.Interval = 1200;
            timer.Enabled = true;

            Console.ReadLine();
        }

        private static void InitiateYeelightDevice (string deviceAddress)
        {
            int delay = 500;
            yeelightDevice = new Device(deviceAddress);
            yeelightDevice.Connect();
            yeelightDevice.SetPower(true);
            yeelightDevice.SetRGBColor(255, 255, 255, delay);
            Thread.Sleep(delay);
            yeelightDevice.SetRGBColor(0, 0, 255, delay);
            Thread.Sleep(delay);
            yeelightDevice.SetRGBColor(255, 255, 255, delay);
        }

        private static void UpdateLights(object s, ElapsedEventArgs e)
        {
            screenCapture = GetScreen();
            screenColor = GetColor(screenCapture);
            SendYeelightData(screenColor);
        }

        static private Bitmap GetScreen ()
        {
            Rectangle screenSize = Screen.PrimaryScreen.Bounds;
            Bitmap bitmap = new Bitmap(screenSize.Width, screenSize.Height);
            Size size = new Size(bitmap.Width, bitmap.Height);
            Graphics screenImage = Graphics.FromImage(bitmap);
            screenImage.CopyFromScreen(0, 0, 0, 0, size);
            screenImage.Dispose();
            return bitmap;
        }

        static private Color GetColor (Bitmap bitmap)
        {
            int totalRed = 0;
            int totalGreen = 0;
            int totalBlue = 0;
            int totalPixelArea = (bitmap.Height / scaleFactor) * (bitmap.Width / scaleFactor);

            for (int y = 0; y < bitmap.Height; y += scaleFactor)
            {
                for (int x = 0; x < bitmap.Width; x += scaleFactor)
                {
                    Color currentPixel = bitmap.GetPixel(x, y);
                    totalRed += currentPixel.R;
                    totalGreen += currentPixel.G;
                    totalBlue += currentPixel.B;
                }
            }

            bitmap.Dispose();

            int averageRed = totalRed / totalPixelArea;
            int averageGreen = totalGreen / totalPixelArea;
            int averageBlue = totalBlue / totalPixelArea;

            return Color.FromArgb(averageRed,averageGreen,averageBlue);
        }

        static private void SendYeelightData(Color color)
        {
            yeelightDevice.SetRGBColor(
                    color.R,
                    color.G,
                    color.B,
                    1000
                );
            Console.WriteLine("R: " + color.R);
            Console.WriteLine("G: " + color.G);
            Console.WriteLine("B: " + color.B);
            Console.WriteLine("");
        }
    }
}
