using System;
using System.Collections.Generic;
using System.Text;

namespace CopyCat.Common.Models
{
    [Serializable]
    public class Pose : Landmark
    {
        EPoseLandmark Landmark { get; }

        public Pose(
            EPoseLandmark landmark,
            float x,
            float y,
            float z,
            float visibility,
            float presence) : base (x, y, z, visibility, presence)
        {
            Landmark = landmark;
        }

        public IEnumerable<EPoseLandmark> GetNextConnectedLandmarks()
        {
            var landmarks = new List<EPoseLandmark>();

            switch ((int)Landmark)
            {
                case 0:
                    landmarks.Add((EPoseLandmark)1);
                    landmarks.Add((EPoseLandmark)4);
                    break;
                case 1:
                    landmarks.Add((EPoseLandmark)2);
                    break;
                case 2:
                    landmarks.Add((EPoseLandmark)3);
                    break;
                case 3:
                    landmarks.Add((EPoseLandmark)7);
                    break;
                case 4:
                    landmarks.Add((EPoseLandmark)5);
                    break;
                case 5:
                    landmarks.Add((EPoseLandmark)6);
                    break;
                case 6:
                    landmarks.Add((EPoseLandmark)8);
                    break;
                case 9:
                    landmarks.Add((EPoseLandmark)10);
                    break;
                case 11:
                    landmarks.Add((EPoseLandmark)12);
                    landmarks.Add((EPoseLandmark)13);
                    landmarks.Add((EPoseLandmark)23);
                    break;
                case 12:
                    landmarks.Add((EPoseLandmark)14);
                    landmarks.Add((EPoseLandmark)24);
                    break;
                case 13:
                    landmarks.Add((EPoseLandmark)15);
                    break;
                case 14:
                    landmarks.Add((EPoseLandmark)16);
                    break;
                case 15:
                    landmarks.Add((EPoseLandmark)17);
                    landmarks.Add((EPoseLandmark)19);
                    landmarks.Add((EPoseLandmark)21);
                    break;
                case 16:
                    landmarks.Add((EPoseLandmark)18);
                    landmarks.Add((EPoseLandmark)20);
                    landmarks.Add((EPoseLandmark)22);
                    break;
                case 17:
                    landmarks.Add((EPoseLandmark)19);
                    break;
                case 18:
                    landmarks.Add((EPoseLandmark)20);
                    break;
                case 23:
                    landmarks.Add((EPoseLandmark)24);
                    landmarks.Add((EPoseLandmark)25);
                    break;
                case 24:
                    landmarks.Add((EPoseLandmark)26);
                    break;
                case 25:
                    landmarks.Add((EPoseLandmark)27);
                    break;
                case 26:
                    landmarks.Add((EPoseLandmark)28);
                    break;
                case 27:
                    landmarks.Add((EPoseLandmark)29);
                    landmarks.Add((EPoseLandmark)31);
                    break;
                case 28:
                    landmarks.Add((EPoseLandmark)30);
                    landmarks.Add((EPoseLandmark)32);
                    break;
                case 29:
                    landmarks.Add((EPoseLandmark)31);
                    break;
                case 30:
                    landmarks.Add((EPoseLandmark)32);
                    break;
            }

            return landmarks;
        }
    }
}
