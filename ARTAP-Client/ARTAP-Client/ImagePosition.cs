using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    public class ImagePosition
    {
        #region Static Variables

        /// <summary>
        /// Used within this class to assign all instances unique id's
        /// </summary>
        private static int nextId = 0;

        #endregion

        #region Fields

        /// <summary>
        /// Unique identifier for references over the network
        /// </summary>
        public readonly int ID;

        /// <summary>
        /// Position of user in world space at the time of this object's creation
        /// </summary>
        public readonly float[] Position;

        /// <summary>
        /// Direction user was facing at the time of this object's creation
        /// </summary>
        public readonly float[] Forward;

        /// <summary>
        /// Upward direction relative to user at the time of this object's creation
        /// </summary>
        public readonly float[] Up;

        /// <summary>
        /// Time since application launch of this object's creation
        /// </summary>
        public readonly float TimeCreated;

        #endregion

        #region Constructor

        public ImagePosition(int ID, float[] Position, float[] Forward, float[] Up, float TimeCreated)
        {
            this.ID = ID;
            this.Position = Position;
            this.Forward = Forward;
            this.Up = Up;
            this.TimeCreated = TimeCreated;
        }

        #endregion

        //To byte array of length 44
        public byte[] ToByteArray()
        {
            byte[] idBytes = BitConverter.GetBytes(ID);
            byte[] positionBytes = VectorToBytes(Position);
            byte[] forwardBytes = VectorToBytes(Forward);
            byte[] upBytes = VectorToBytes(Up);
            byte[] timeCreatedBytes = BitConverter.GetBytes(TimeCreated);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(idBytes);
                Array.Reverse(positionBytes);
                Array.Reverse(forwardBytes);
                Array.Reverse(upBytes);
                Array.Reverse(timeCreatedBytes);
            }
            byte[] finalArray = new byte[44];
            Array.Copy(idBytes, 0, finalArray, 0, 4);
            Array.Copy(positionBytes, 0, finalArray, 4, 12);
            Array.Copy(forwardBytes, 0, finalArray, 16, 12);
            Array.Copy(upBytes, 0, finalArray, 28, 12);
            Array.Copy(timeCreatedBytes, 0, finalArray, 40, 4);
            return finalArray;
        }

        public static ImagePosition FromByteArray(byte[] bytes)
        {
            byte[] idBytes = new byte[4];
            byte[] positionBytes = new byte[12];
            byte[] forwardBytes = new byte[12];
            byte[] upBytes = new byte[12];
            byte[] timeCreatedBytes = new byte[4];
            Buffer.BlockCopy(bytes, 0, idBytes, 0, 4);
            Buffer.BlockCopy(bytes, 4, positionBytes, 0, 12);
            Buffer.BlockCopy(bytes, 16, forwardBytes, 0, 12);
            Buffer.BlockCopy(bytes, 28, upBytes, 0, 12);
            Buffer.BlockCopy(bytes, 40, timeCreatedBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(idBytes);
                Array.Reverse(positionBytes);
                Array.Reverse(forwardBytes);
                Array.Reverse(upBytes);
                Array.Reverse(timeCreatedBytes);
            }
            int id = BitConverter.ToInt32(idBytes, 0);
            float[] position = VectorFromBytes(positionBytes);
            float[] forward = VectorFromBytes(forwardBytes);
            float[] up = VectorFromBytes(upBytes);
            float timeCreated = BitConverter.ToSingle(timeCreatedBytes, 0);
            return new ImagePosition(id, position, forward, up, timeCreated);
        }

        //return byte array of size 12
        private byte[] VectorToBytes(float[] vect)
        {
            byte[] buff = new byte[sizeof(float) * 3];
            Buffer.BlockCopy(BitConverter.GetBytes(vect[0]), 0, buff, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(vect[1]), 0, buff, 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(vect[2]), 0, buff, 2 * sizeof(float), sizeof(float));
            return buff;
        }

        private static float[] VectorFromBytes(byte[] bytes)
        {
            float x = BitConverter.ToSingle(bytes, 0 * sizeof(float));
            float y = BitConverter.ToSingle(bytes, 1 * sizeof(float));
            float z = BitConverter.ToSingle(bytes, 2 * sizeof(float));
            return new float[]{x, y, z};
        }

        public double GetForwardAngle()
        {
            //get angle relative to z axis
            double val = Math.Atan((Forward[0] / Forward[2]));
            if(Forward[2] < 0)
            {
                val += Math.PI;
            }
            if (val < 0)
                val += 2 * Math.PI;
            return val;
        }

        public bool IsHere(ImagePosition pos)
        {
            float[] posToCheck = pos.Position;
            return (Position[0] - 2.5f < posToCheck[0] && Position[0] + 2.5f > posToCheck[0] &&
                    Position[1] - 2.5f < posToCheck[1] && Position[1] + 2.5f > posToCheck[1] &&
                    Position[2] - 2.5f < posToCheck[2] && Position[2] + 2.5f > posToCheck[2]);
        }
    }
}
