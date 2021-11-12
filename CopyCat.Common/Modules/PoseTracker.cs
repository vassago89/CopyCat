using CopyCat.Common.Events;
using CopyCat.Common.Framework;
using CopyCat.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace CopyCat.Common.Modules
{
    [Serializable]
    public class PosePackage : IEvent
    {
        public Dictionary<EPoseLandmark, Pose> Poses { get; }

        public Pose this[EPoseLandmark landmark]
        {
            get => Poses[landmark];
            set => Poses[landmark] = value;
        }

        public PosePackage()
        {
            Poses = new Dictionary<EPoseLandmark, Pose>();
        }
    }

    [Serializable]
    public class HandPackage : IEvent
    {
        public Dictionary<EHandLandmark, Hand> Hands { get; }

        public Hand this[EHandLandmark landmark]
        {
            get => Hands[landmark];
            set => Hands[landmark] = value;
        }

        public HandPackage()
        {
            Hands = new Dictionary<EHandLandmark, Hand>();
        }
    }

    [Serializable]
    public class LeftHandPackage : HandPackage
    {
    }

    [Serializable]
    public class RightHandPackage : HandPackage
    {
    }

    public class PoseTracker : IDisposable
    {
        [DllImport("pose_landmark.dll")]
        public static extern void StartRun();

        [DllImport("pose_landmark.dll")]
        public static extern IntPtr GetLandmarks(int cols, int rows, IntPtr data);

        [DllImport("pose_landmark.dll")]
        public static extern void Release();

        public PosePackage Poses { get; }
        public LeftHandPackage Left { get; }
        public RightHandPackage Right { get; }

        private Dictionary<EPoseLandmark, Predicter> Predicts { get; }

        public PoseTracker()
        {
            Poses = new PosePackage();
            Left = new LeftHandPackage();
            Right = new RightHandPackage();

            Predicts = new Dictionary<EPoseLandmark, Predicter>();

            foreach (var pose in Enum.GetValues(typeof(EPoseLandmark)))
                Predicts[(EPoseLandmark)pose] = new Predicter();

            StartRun();
        }

        public bool Get(int cols, int rows, IntPtr dataPtr)
        {
            //var intPtr = GetLandmarks(mat.Cols, mat.Rows, mat.Clone().Data);
            var intPtr = GetLandmarks(cols, rows, dataPtr);
            if (IntPtr.Zero == intPtr)
                return false;

            var array = new float[165 + 105 + 105 + 478];

            Marshal.Copy(intPtr, array, 0, array.Length);

            int index = 0;
            foreach (var pose in Enum.GetValues(typeof(EPoseLandmark)))
            {
                var predicted = Predicts[(EPoseLandmark)pose].Predict(array[index], array[index + 1], array[index + 2]);

                //Poses[(EPoseLandmark)pose] = new Pose(
                //    (EPoseLandmark)pose,
                //    predicted.X,
                //    predicted.Y,
                //    predicted.Z,
                //    array[index + 3],
                //    array[index + 4]);

                Poses[(EPoseLandmark)pose] = new Pose(
                    (EPoseLandmark)pose,
                    array[index],
                    array[index + 1],
                    array[index + 2],
                    array[index + 3],
                    array[index + 4]);

                index += 5;
            }

            foreach (var hand in Enum.GetValues(typeof(EHandLandmark)))
            {
                Left[(EHandLandmark)hand] = new Hand(
                    (EHandLandmark)hand,
                    array[index],
                    array[index + 1],
                    array[index + 2],
                    array[index + 3],
                    array[index + 4]);

                index += 5;
            }

            foreach (var hand in Enum.GetValues(typeof(EHandLandmark)))
            {
                Right[(EHandLandmark)hand] = new Hand(
                    (EHandLandmark)hand,
                    array[index],
                    array[index + 1],
                    array[index + 2],
                    array[index + 3],
                    array[index + 4]);

                index += 5;
            }

            //for (int i = 0; i < 468; i++)
            //foreach (var face in Enum.GetValues(typeof(EHandLandmark)))
            //{
            //    Right[(EHandLandmark)hand] = new Hand(
            //        (EHandLandmark)hand,
            //        array[index],
            //        array[index + 1],
            //        array[index + 2],
            //        array[index + 3],
            //        array[index + 4]);

            //    index += 5;
            //}

            return true;
        }

        public void Dispose()
        {
            //Release();
        }

        class Predicter
        {
            class Filter
            {
                private bool _initialized;

                private float _q = 0.1f;
                private float _r = 1.0f;
                private float[] _prevs = new float[6];

                public float _p;
                public float _k;
                public float _value;

                public Filter()
                {

                }

                public float Predict(float value)
                {
                    if (_initialized == false)
                    {
                        _initialized = true;
                        _value = value;

                        for (var i = 0; i < _prevs.Length; i++)
                            _prevs[i] = _value;

                        return value;
                    }

                    _k = (_p + _q) / (_p + _q + _r);
                    _p = _r * (_p + _q) / (_r + _p + _q);

                    _prevs[0] = _value + (value - _value) * _k;
                    for (var i = 1; i < _prevs.Length; i++)
                        _prevs[i] = _prevs[i] * 0.1f + _prevs[i - 1] * (1f - 0.1f);

                    _value = _prevs[_prevs.Length - 1];

                    return _value;
                }
            }

            private Filter _x;
            private Filter _y;
            private Filter _z;

            public Predicter()
            {
                _x = new Filter();
                _y = new Filter();
                _z = new Filter();
            }

            public (float X, float Y, float Z) Predict(float x, float y, float z)
            {
                return (_x.Predict(x), _y.Predict(y), _z.Predict(z));
            }
        }
    }
}
