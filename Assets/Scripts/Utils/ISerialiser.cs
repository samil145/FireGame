namespace Utils
{
    interface ICustomSerialiser<T> where T : ICustomSerialiser<T>
    {
        byte[] Serialize();
        T Deserialize(byte[] bytes);

        public static T Fabricate(byte[] bytes) 
        {
            T temp = default(T);
            return temp.Deserialize(bytes);
        }
    }
}