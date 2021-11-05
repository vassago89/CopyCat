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

        public VideoViewModel(PoseTracker poseTracker)
        {
            ShowCommand = new DelegateCommand(() =>
            {
                Task.Run(() =>
                {
                    var capture = new VideoCapture();
                    capture.Open("yoga.mp4");
                    Mat mat = new Mat();
                    while (capture.Read(mat))
                    {
                        poseTracker.Get(mat);
                        foreach (var pose in poseTracker.Poses.Values)
                        {
                            foreach (var landmark in pose.GetNextConnectedLandmarks())
                                mat.Line(pose.Point, poseTracker.Poses[landmark].Point, Scalar.Red, 2);
                        }

                        var source = mat.ToBitmapSource();
                        source.Freeze();
                        Source = source;
                    }
                });
            });
        }
    }
}
