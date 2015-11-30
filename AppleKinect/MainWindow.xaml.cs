using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GestureDetector.DataSources;
using GestureDetector.Gestures.Swipe;
using Microsoft.Kinect;

namespace AppleKinect
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public KinectSensor Kinect;
        public WriteableBitmap Bitmap;
        public byte[] Pixels;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
            {
                smoothingParam.Smoothing = 0.7f;
                smoothingParam.Correction = 0.99f;
                smoothingParam.Prediction = 0.7f;
                smoothingParam.JitterRadius = 0.05f;
                smoothingParam.MaxDeviationRadius = 0.04f;
            };

            Kinect = KinectSensor.KinectSensors[0];            
            Kinect.AllFramesReady += Kinect_AllFramesReady;
            Kinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            Pixels = new byte[Kinect.ColorStream.FramePixelDataLength];
            Bitmap = new WriteableBitmap(Kinect.ColorStream.FrameWidth, Kinect.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            ImageCanvas.Source = Bitmap;
            Kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            Kinect.SkeletonStream.Enable(smoothingParam);
            Kinect.DepthStream.Enable();
            Kinect.Start();

        }

        void Kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var Frame = e.OpenColorImageFrame())
            {
                if (Frame != null)
                {
                    Frame.CopyPixelDataTo(Pixels);
                    Bitmap.WritePixels(
                        new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight),
                        Pixels, Bitmap.PixelWidth * sizeof(int), 0);
                }
            }

            using (var Frame = e.OpenSkeletonFrame())
            {
                if (Frame != null)
                {
                    var skeletons = new Skeleton[Frame.SkeletonArrayLength];
                    Frame.CopySkeletonDataTo(skeletons);
                    var skel = (from s in skeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                    if (skel != null)
                    {
                        var left_hand = skel.Joints[JointType.HandLeft];
                        var rite_hand = skel.Joints[JointType.HandRight];
                        var shoulderRight = skel.Joints[JointType.ShoulderRight];
                        var fleft = skel.Joints[JointType.FootLeft];
                        var frite = skel.Joints[JointType.FootRight];


                        Joint head = skel.Joints[JointType.Head];

                        Point mousePos = new Point((rite_hand.Position.X * 1300 + 683), (rite_hand.Position.Y * -1300 + 768));

                        //двойное нажатие левой кнопкой мыши
                        if (distance(head.Position, left_hand.Position) < 0.06f) {
                            NatiteMethods.sendMouseDoubleClick(mousePos);
                        
                        // правая кнопка мыши
                        } else if(distance(left_hand.Position, rite_hand.Position) < 0.03f) {
                            NatiteMethods.mouseLeftButtonDown(mousePos);

                        // перетаскивание
                        } else if(distance(shoulderRight.Position, left_hand.Position) < 0.03f) {
                            NatiteMethods.sendMouseRightclick(mousePos);
                        }
                        if (fleft.Position.Y <= fleft.Position.Y + 0.5f) {

                            // Get the virtual key code for the character we want to press
                            //int key = 87;
                            //uint vkKey = NatiteMethods.VkKeyScan((char) key);

                            //NatiteMethods.keybd_event(vkKey, 0, 0, 0);
                            //NatiteMethods.keybd_event(vkKey, 0, 2, 0);
                        }
                        
                        NatiteMethods.SetCursorPos((int) mousePos.X, (int) mousePos.Y);
                    }
                }
            }

        }
   
        private float distance(SkeletonPoint p1, SkeletonPoint p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) + (p1.Z - p2.Z) * (p1.Z - p2.Z);
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Kinect.Stop();
        }
    }

    class NatiteMethods {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);
        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x10;

        public static void sendMouseRightclick(Point p) {
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)p.X, (uint)p.Y, 0, (UIntPtr)0);
            Thread.Sleep(300);
        }

        public static void sendMouseDoubleClick(Point p) {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)p.X, (uint)p.Y, 0, (UIntPtr)0);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)p.X, (uint)p.Y, 0, (UIntPtr)0);
            Thread.Sleep(300);
        }

        public static void sendMouseRightDoubleClick(Point p) {
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)p.X, (uint)p.Y, 0, (UIntPtr)0);
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)p.X, (uint)p.Y, 0, (UIntPtr)0);
            Thread.Sleep(300);
        }

        public static void mouseLeftButtonDown(Point p) {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)p.X, (uint)p.Y, 0, (UIntPtr)0);
        }
        public static void mouseLeftButtonUp(Point p) {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)p.X, (uint)p.Y, 0, (UIntPtr)0);
            Thread.Sleep(300);
        }

        // For posting a keyboard event
        //[DllImport("user32.dll")]
        //public static extern void keybd_event(uint bVk, uint bScan, long dwFlags, long dwExtraInfo);
        //
        //[DllImport("user32.dll")]
        //public static extern uint VkKeyScan(char ch);
    }
}
