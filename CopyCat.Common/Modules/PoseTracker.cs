using CopyCat.Common.Models;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CopyCat.Common.Modules
{
    public class PoseTracker : IDisposable
    {
        [DllImport("pose_landmark.dll")]
        public static extern void StartRun();

        [DllImport("pose_landmark.dll")]
        public static extern IntPtr GetLandmarks(int cols, int rows, IntPtr data);

        [DllImport("pose_landmark.dll")]
        public static extern void Release();

        public Dictionary<EPoseLandmark, Pose> Poses { get; }

        public PoseTracker()
        {
            Poses = new Dictionary<EPoseLandmark, Pose>();
            StartRun();
        }

        public bool Get(Mat mat)
        {
            var intPtr = GetLandmarks(mat.Cols, mat.Rows, mat.Clone().Data);
            if (IntPtr.Zero == intPtr)
                return false;

            var array = new float[165];

            Marshal.Copy(intPtr, array, 0, array.Length);

            int index = 0;
            foreach (var pose in Enum.GetValues(typeof(EPoseLandmark)))
            {
                Poses[(EPoseLandmark)pose] = new Pose(
                    (EPoseLandmark)pose,
                    array[index],
                    array[index + 1],
                    array[index + 2],
                    array[index + 3],
                    array[index + 4],
                    new Point(array[index] * mat.Width, array[index + 1] * mat.Height));

                index += 5;
            }

            return true;
        }

        public void Dispose()
        {
            //Release();
        }
    }
}
