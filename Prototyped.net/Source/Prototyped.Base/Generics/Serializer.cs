using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Prototyped.Base.Generics
{
    /// <summary>
    /// This class is a example of a generic serializer.
    /// </summary>
    public class Serializer
    {
        #region Encoding

        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">Unicode Byte Array to be converted to String</param>
        /// <returns>String converted from Unicode Byte Array</returns>
        public static string UTF8ByteArrayToString(byte[] characters)
        {
            var encoding = new UTF8Encoding();
            var constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization
        /// </summary>
        /// <param name="pXmlString"></param>
        /// <returns></returns>
        public static Byte[] StringToUTF8ByteArray(string pXmlString)
        {
            var encoding = new UTF8Encoding();
            var byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }

        #endregion

        #region Serialiser Logic

        /// <summary>
        /// Serialize an object into an XML string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T obj)
        {
            try
            {
                string xmlString = null;
                var memoryStream = new MemoryStream();
                var xs = new XmlSerializer(typeof(T));
                var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                xs.Serialize(xmlTextWriter, obj);
                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                xmlString = UTF8ByteArrayToString(memoryStream.ToArray()); return xmlString;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Reconstruct an object from an XML string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string xml)
        {
            var xs = new XmlSerializer(typeof(T));
            var memoryStream = new MemoryStream(StringToUTF8ByteArray(xml));
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            return (T)xs.Deserialize(memoryStream);
        }

        #endregion
    }
}
