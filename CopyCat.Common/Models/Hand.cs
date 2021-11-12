using System;
using System.Collections.Generic;
using System.Text;

namespace CopyCat.Common.Models
{
    [Serializable]
    public class Hand : Landmark
    {
        EHandLandmark Landmark { get; }

        public Hand(
            EHandLandmark landmark,
            float x,
            float y,
            float z,
            float visibility,
            float presence) : base(x, y, z, visibility, presence)
        {
            Landmark = landmark;
        }

        public IEnumerable<EHandLandmark> GetNextConnectedLandmarks()
        {
            var landmarks = new List<EHandLandmark>();

            switch ((int)Landmark)
            {
                case 0:
                    landmarks.Add((EHandLandmark)1);
                    landmarks.Add((EHandLandmark)5);
                    landmarks.Add((EHandLandmark)17);
                    break;
                case 1:
                    landmarks.Add((EHandLandmark)2);
                    break;
                case 2:
                    landmarks.Add((EHandLandmark)3);
                    break;
                case 3:
                    landmarks.Add((EHandLandmark)4);
                    break;
                case 5:
                    landmarks.Add((EHandLandmark)6);
                    landmarks.Add((EHandLandmark)9);
                    break;
                case 6:
                    landmarks.Add((EHandLandmark)7);
                    break;
                case 7:
                    landmarks.Add((EHandLandmark)8);
                    break;
                case 9:
                    landmarks.Add((EHandLandmark)10);
                    landmarks.Add((EHandLandmark)13);
                    break;
                case 10:
                    landmarks.Add((EHandLandmark)11);
                    break;
                case 11:
                    landmarks.Add((EHandLandmark)12);
                    break;
                case 13:
                    landmarks.Add((EHandLandmark)14);
                    landmarks.Add((EHandLandmark)17);
                    break;
                case 14:
                    landmarks.Add((EHandLandmark)15);
                    break;
                case 15:
                    landmarks.Add((EHandLandmark)16);
                    break;
                case 17:
                    landmarks.Add((EHandLandmark)18);
                    break;
                case 18:
                    landmarks.Add((EHandLandmark)19);
                    break;
                case 19:
                    landmarks.Add((EHandLandmark)20);
                    break;
            }

            return landmarks;
        }
    }
}
