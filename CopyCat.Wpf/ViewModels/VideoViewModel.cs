using CopyCat.Common.Events;
using CopyCat.Common.Models;
using CopyCat.Common.Modules;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CopyCat.Wpf.ViewModels
{
    class VideoViewModel : BindableBase
    {
        public DelegateCommand ShowCommand { get; }

        private BitmapSource _source;
        public BitmapSource Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public VideoViewModel(EventBusMediator eventBusMediator, PoseTracker poseTracker)
        {
            ShowCommand = new DelegateCommand(() =>
            {
                Task.Run(() =>
                {
                    var capture = new VideoCapture();
                    capture.Open("yoga.mp4");
                    //capture.Open("wiper.mp4");
                    Mat mat = new Mat();
                    //eventBusMediator.Subscribe<PosePackage>((package) =>
                    //{

                    //});
                    while (capture.Read(mat))
                    {
                        poseTracker.Get(mat.Cols, mat.Rows, mat.Clone().Data);
                        foreach (var current in Enum.GetValues(typeof(EPoseLandmark)))
                        {
                            var pose = poseTracker.Poses[(EPoseLandmark)current];
                            if (pose.Presence == 0)
                                continue;

                            foreach (var next in pose.GetNextConnectedLandmarks())
                            {
                                var nextPose = poseTracker.Poses[next];

                                mat.Line(
                                    new Point(pose.X * mat.Width, pose.Y * mat.Height),
                                    new Point(nextPose.X * mat.Width, nextPose.Y * mat.Height),
                                    Scalar.Red,
                                    2);
                            }
                        }

                        foreach (var current in Enum.GetValues(typeof(EHandLandmark)))
                        {
                            var hand = poseTracker.Left[(EHandLandmark)current];
                            var currentPoint = new Point(hand.X * mat.Width, hand.Y * mat.Height);

                            foreach (var next in hand.GetNextConnectedLandmarks())
                            {
                                var nextHand = poseTracker.Left[next];
                                var nextPoint = new Point(nextHand.X * mat.Width, nextHand.Y * mat.Height);

                                if (currentPoint == nextPoint)
                                    continue;

                                mat.Line(
                                    currentPoint,
                                    nextPoint,
                                    Scalar.Yellow,
                                    2);
                            }
                        }

                        foreach (var current in Enum.GetValues(typeof(EHandLandmark)))
                        {
                            var hand = poseTracker.Right[(EHandLandmark)current];
                            var currentPoint = new Point(hand.X * mat.Width, hand.Y * mat.Height);

                            foreach (var next in hand.GetNextConnectedLandmarks())
                            {
                                var nextHand = poseTracker.Right[next];
                                var nextPoint = new Point(nextHand.X * mat.Width, nextHand.Y * mat.Height);

                                if (currentPoint == nextPoint)
                                    continue;

                                mat.Line(
                                    currentPoint,
                                    nextPoint,
                                    Scalar.Green,
                                    2);
                            }
                        }

                        eventBusMediator.Publish(poseTracker.Poses);
                        //eventBusMediator.Publish(poseTracker.Left);
                        //eventBusMediator.Publish(poseTracker.Right);

                        var source = mat.ToBitmapSource();
                        source.Freeze();
                        Source = source;
                    }
                });
            });
        }
    }
}
