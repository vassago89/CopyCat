using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace CopyCat.Common.Abstracts
{
    [Serializable]
    public abstract class BinaryConfig<T> where T : class, new()
    {
        public string DefualtPath => Path.Combine(Environment.CurrentDirectory, $"{GetType().Name}.bin");

        private static BinaryFormatter _formatter = new BinaryFormatter();

        public BinaryConfig()
        {

        }

        public static T Deserialize(string filePath)
        {
            if (File.Exists(filePath) == false)
                return new T();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    return _formatter.Deserialize(stream) as T;
                }
                catch (Exception e)
                {
                    return new T();
                }
            }
        }

        public void Serialize(string filePath = null)
        {
            using (var stream = File.Create(filePath ?? DefualtPath))
            {
                _formatter.Serialize(stream, this);
            }
        }
    }
}
