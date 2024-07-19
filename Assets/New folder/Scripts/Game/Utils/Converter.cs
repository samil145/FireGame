using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utils
{
    public static class Converter
    {
        public static byte[] FromStringToByteArray(string str)
        {
            if (str == null)
                return null;
            byte[] bytes = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }
            return bytes;
        }

        public static string FromByteArrayToString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                chars[i] = (char)bytes[i];
            }
            return new string(chars);
        }

        public static byte[] FromShortToByteArray(short number) => FromLongToByteArray(number)[6..8];

        public static short FromByteArrayToShort(byte[] bytes) => (short)FromByteArrayToLong(bytes);

        public static byte[] FromIntToByteArray(int number) => FromLongToByteArray(number)[4..8];

        public static int FromByteArrayToInt(byte[] bytes) => (int)FromByteArrayToLong(bytes);

        public static byte[] FromLongToByteArray(long number)
        {
            int size = sizeof(long);
            byte[] bytes = new byte[size];
            for (int i = size - 1; i >= 0; i--)
            {
                bytes[i] = (byte)number;
                number = number >> 8;
            }
            return bytes;
        }

        public static long FromByteArrayToLong(byte[] bytes)
        {
            int size = bytes.Length;
            long result = 0;
            for (int i = 0; i < size; i++)
            {
                result |= bytes[i];
                if (i != size - 1)
                    result = result << 8;
            }
            return result;
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object result = binForm.Deserialize(memStream);

            return result;
        }
    }
}

