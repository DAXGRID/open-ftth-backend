using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ConduitNetwork.Business.Specifications
{
    public static class GUIDHelper
    {
        public static Guid CreateDerivedGuid(Guid orig, int no, bool longRange = false)
        {
            byte[] a = orig.ToByteArray();
            byte[] b = new byte[16];

            for (int i = 0; i < 8; i++)
                b[i] = (byte)no;

            if (no > 255)
            {
                int divVal = no / 255;
                b[0] = (byte)divVal;
                b[0] += 128;
                b[1] = (byte)divVal;
            }

            if (!longRange)
            {
                // XOR
                return new Guid(BitConverter.GetBytes(BitConverter.ToUInt64(a, 0) ^ BitConverter.ToUInt64(b, 8))
                    .Concat(BitConverter.GetBytes(BitConverter.ToUInt64(a, 8) ^ BitConverter.ToUInt64(b, 0))).ToArray());
            }
            else
            {

                // AND
                for (int i = 0; i < 16; i++)
                    b[i] = (byte)no;

                int div = no / 255;
                b[0] = (byte)div;
                b[2] = (byte)div;
                b[4] = (byte)div;
                b[6] = (byte)div;
                b[8] = (byte)div;
                b[10] = (byte)div;
                b[12] = (byte)div;
                b[14] = (byte)div;

                for (int i = 0; i < 16; i++)
                    a[i] += b[i];

                return new Guid(a);
            }
        }

        public static Guid StringToGUID(string value)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            return new Guid(data);
        }
    }
}
