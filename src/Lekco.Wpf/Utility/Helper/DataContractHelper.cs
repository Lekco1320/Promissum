using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Lekco.Wpf.Utility.Helper
{
    /// <summary>
    /// A helper class for coping with objects assigned <see cref="DataContractAttribute"/>.
    /// </summary>
    public static class DataContractHelper
    {
        /// <summary>
        /// Deserilize XML file to object.
        /// </summary>
        /// <typeparam name="T">Real type of the object.</typeparam>
        /// <param name="fileName">The file's name.</param>
        /// <returns>Deserilized object.</returns>
        public static T DeserilizeFromFile<T>(string fileName)
        {
            using var stream = new FileStream(fileName, FileMode.Open);
            var serializer = new DataContractSerializer(typeof(T));
            var result = (T?)serializer.ReadObject(stream) ?? throw new InvalidDataContractException("Invalid data or file has been destoryed.");
            return result;
        }

        /// <summary>
        /// Serilize the object to XML file.
        /// </summary>
        /// <typeparam name="T">Real type of the object.</typeparam>
        /// <param name="fileName">The file's name.</param>
        public static void SerilizeToFile<T>(this T obj, string fileName)
        {
            using var writer = XmlWriter.Create(fileName);
            var serializer = new DataContractSerializer(typeof(T));
            serializer.WriteObject(writer, obj);
        }

        /// <summary>
        /// Serilize the object to XML file.
        /// </summary>
        /// <typeparam name="T">Real type of the object.</typeparam>
        /// <param name="fileName">The file's name.</param>
        /// <param name="namespaces">All known namespaces to assigned in the top of XML.</param>
        public static void SerilizeToFile<T>(this T obj, string fileName, IDictionary<string, string> namespaces)
        {
            using var writer = XmlWriter.Create(fileName);
            var serializer = new DataContractSerializer(typeof(T));
            serializer.WriteStartObject(writer, obj);
            foreach (var pair in namespaces)
            {
                writer.WriteAttributeString("xmlns", pair.Key, null, pair.Value);
            }
            serializer.WriteObjectContent(writer, obj);
            serializer.WriteEndObject(writer);
        }
    }
}
