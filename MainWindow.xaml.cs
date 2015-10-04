﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.BodyBasics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        private const double HandSize = 30;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as closed
        /// </summary>
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as opened
        /// </summary>
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as in lasso (pointer) position
        /// </summary>
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper1 = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// definition of bones
        /// </summary>
        private List<Tuple<JointType, JointType>> bones;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// List of colors for each body tracked
        /// </summary>
        private List<Pen> bodyColors;
        public double window_width;
        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;
        private int[] shot = new int[7];
        private int[] hats = new int[7];
        bool fl;
        int num_of_bodies;
        private bool bell;
        private Random rnd;
        private int secs;
        private int count;
        private Random rand;
        System.Media.SoundPlayer player, gun, gun1, music;
        bool flag1, flag2, rot1, rot2;
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            flag1 = true;
            flag2 = true;
            rot1 = true;
            rot2 = true;
            rnd = new Random();
            secs = rnd.Next(250, 350);
            count = 0;
            Console.WriteLine(secs.ToString());
            bell = false;
            player = new System.Media.SoundPlayer();
            player.SoundLocation = @"D:\CS\KinectApp\HackathonGla15-master\sounds\Bell-sound.wav";
            player.Load();
            gun = new System.Media.SoundPlayer();
            gun.SoundLocation = @"D:\CS\KinectApp\HackathonGla15-master\sounds\WesternRicochet-SoundBible.com-1725886901.wav";
            gun.Load();
            music = new System.Media.SoundPlayer();
            music.SoundLocation = @"D:\CS\KinectApp\HackathonGla15-master\sounds\Cowboy_Theme-Pavak-1711860633.wav";
            //player.Play();
            music.Load();
            music.Play();

            shot[0] = 1;
            shot[1] = 1;
            shot[2] = 1;
            shot[3] = 1;
            shot[4] = 1;
            shot[5] = 1;
            shot[6] = 1;
            fl = false;
            window_width = 500;
            //asd
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // get the coordinate mapper
            this.coordinateMapper1 = this.kinectSensor.CoordinateMapper;

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // get size of joint space
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            // populate body colors, one for each BodyIndex
            this.bodyColors = new List<Pen>();

            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>

        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            if (count == secs-1)
                music.Stop();
            if (count == secs)
            {
                bell = true;
                Console.WriteLine(count.ToString());
                player.Play();
                count=secs+1;
            }
            else
                count++;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    num_of_bodies = 0;
                    // Draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                    int penIndex = 0;
                    foreach (Body body in this.bodies)
                    {
                        Pen drawPen = this.bodyColors[penIndex++];

                        if (body.IsTracked)
                        {
                            this.DrawClippedEdges(body, dc);

                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                            
                            // convert the joint points to depth (display) space
                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            foreach (JointType jointType in joints.Keys)
                            {
                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = joints[jointType].Position;
                                if (position.Z < 0)
                                {
                                    position.Z = InferredZPositionClamp;
                                }

                                DepthSpacePoint depthSpacePoint = this.coordinateMapper1.MapCameraPointToDepthSpace(position);
                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                            }
                            Console.WriteLine("SHOT");
                            for(int i=0;i<shot.Length;i++)
                            {
                                Console.WriteLine(i + " - " + shot[i]);
                            }
                            foreach (Body i in bodies)
                            {
                                if (i.IsTracked)
                                {
                                    num_of_bodies++;
                                }
                            }
                            Console.WriteLine("num_of_bodies: " + num_of_bodies);
                            //if ((shot[Array.IndexOf(bodies, body)] == 1) && (num_of_bodies > 1))
                            if (shot[Array.IndexOf(bodies, body)] == 1)
                                {
                                    if (fl)
                                    {
                                    shot[Array.IndexOf(bodies, body)] = 0;
                                    fl = false;
                                    continue;
                                    }
                                //if (jointPoints[JointType.HandTipLeft]>=jointPoints[JointType.SpineMid])
                                if (((jointPoints[JointType.HandTipLeft].Y <= jointPoints[JointType.SpineMid].Y) ||
                                    (jointPoints[JointType.HandTipRight].Y <= jointPoints[JointType.SpineMid].Y)) && bell)
                                {
                                    if (jointPoints[JointType.Neck].X > window_width / 2)
                                    {
                                        //hat2.Visibility = Visibility.Hidden;
                                        vest2.Visibility = Visibility.Hidden;
                                        pants2.Visibility = Visibility.Hidden;
                                        
                                        if (rot1)
                                        {
                                            DoubleAnimation da = new DoubleAnimation();
                                            da.From = 0;
                                            da.To = 100;
                                            da.Duration = new Duration(TimeSpan.FromSeconds(1));
                                            //da.RepeatBehavior = RepeatBehavior.Forever;
                                            RotateTransform rt = new RotateTransform();
                                            hat2.RenderTransform = rt;

                                            rt.CenterX = Canvas.GetLeft(hat2);
                                            rt.CenterY = Canvas.GetTop(hat2) - hat2.ActualHeight / 10;
                                            rot1 = false;
                                            rt.BeginAnimation(RotateTransform.AngleProperty, da);
                                        }
                                        //hat2.Visibility = Visibility.Hidden;
                                        if (flag1)
                                        {
                                            gun.Play();
                                            flag1 = false;
                                        }
                                    }
                                    if (jointPoints[JointType.Neck].X <= window_width / 2)
                                    {
                                        pants1.Visibility = Visibility.Hidden;
                                        vest1.Visibility = Visibility.Hidden;
                                        
                                        if (rot2)
                                        {
                                            DoubleAnimation da = new DoubleAnimation();
                                            da.From = 0;
                                            da.To = 100;
                                            da.By = -10;
                                            da.Duration = new Duration(TimeSpan.FromSeconds(2));
                                            //da.RepeatBehavior = RepeatBehavior.Forever;
                                            RotateTransform rt = new RotateTransform();
                                            rt.CenterX = Canvas.GetLeft(hat);
                                            rt.CenterY = Canvas.GetTop(hat) - hat.ActualHeight / 10;
                                            hat.RenderTransform = rt;
                                            rt.BeginAnimation(RotateTransform.AngleProperty, da);
                                            rot2 = false;
                                         }
                                        //hat2.Visibility = Visibility.Hidden;
                                        if (flag2)
                                        {
                                            gun.Play();
                                            flag2 = false;
                                        }
                                    }
                                    //fl = true;
                                    for (int i=0;i<bodies.Length;i++)
                                    {
                                        if (bodies[i] == body)
                                            continue;
                                        else
                                        {
                                            shot[i] = 0;
                                        }
                                        
                                    }
                                    Console.WriteLine("BAM");
                                    Console.WriteLine(Array.IndexOf(bodies, body));
                                    Console.WriteLine(shot[Array.IndexOf(bodies, body)]);
                                }
                                if (jointPoints[JointType.Neck].X > window_width / 2)
                                {
                                    Canvas.SetLeft(hat, jointPoints[JointType.Head].X + (jointPoints[JointType.ShoulderRight].X - jointPoints[JointType.ShoulderLeft].X) / 2);
                                    Canvas.SetTop(hat, jointPoints[JointType.Head].Y + (jointPoints[JointType.Head].Y - jointPoints[JointType.Neck].Y)*2);
                                    Canvas.SetTop(vest1, jointPoints[JointType.SpineShoulder].Y);
                                    Canvas.SetLeft(vest1, jointPoints[JointType.ShoulderLeft].X + (jointPoints[JointType.ShoulderRight].X - jointPoints[JointType.ShoulderLeft].X));
                                    Canvas.SetLeft(pants1, jointPoints[JointType.ShoulderLeft].X +(jointPoints[JointType.ShoulderRight].X - jointPoints[JointType.ShoulderLeft].X)/4 );
                                    Canvas.SetTop(pants1, jointPoints[JointType.SpineMid].Y);

                                }
                                if (jointPoints[JointType.Neck].X<window_width/2)
                                {
                                    Canvas.SetLeft(hat2, jointPoints[JointType.Head].X);
                                    Canvas.SetTop(hat2, jointPoints[JointType.Head].Y + (jointPoints[JointType.Head].Y - jointPoints[JointType.ShoulderLeft].Y)*2);
                                    Canvas.SetTop(vest2, jointPoints[JointType.SpineShoulder].Y+ (jointPoints[JointType.Head].Y - jointPoints[JointType.Neck].Y));
                                    Canvas.SetLeft(vest2, jointPoints[JointType.ShoulderLeft].X );
                                    Canvas.SetLeft(pants2, jointPoints[JointType.ShoulderLeft].X - (jointPoints[JointType.ShoulderRight].X - jointPoints[JointType.ShoulderLeft].X) / 2);
                                    Canvas.SetTop(pants2, jointPoints[JointType.SpineMid].Y);
                                }

                                //Console.WriteLine("Elbow Left");
                                //Console.WriteLine(jointPoints[JointType.ElbowLeft]);
                                //Console.WriteLine("HandTipLeft");
                                //Console.WriteLine(jointPoints[JointType.HandTipLeft]);
                                //foreach (JointType p in jointPoints.Keys)
                                //{
                                //   Console.WriteLine(p);
                                //}
                                this.DrawBody(joints, jointPoints, dc, drawPen);
                                this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                                this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                            }
                        }
                    }

                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="drawingPen">specifies color to draw a specific body</param>
        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }

        }

        /// <summary>
        /// Draws one bone of a body (joint to joint)
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="jointType0">first joint of bone to draw</param>
        /// <param name="jointType1">second joint of bone to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// /// <param name="drawingPen">specifies color to draw a specific bone</param>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        /// <summary>
        /// Draws a hand symbol if the hand is tracked: red circle = closed, green circle = opened; blue circle = lasso
        /// </summary>
        /// <param name="handState">state of the hand</param>
        /// <param name="handPosition">position of the hand</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
                    break;
            }
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping body data
        /// </summary>
        /// <param name="body">body to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }
    }
}
