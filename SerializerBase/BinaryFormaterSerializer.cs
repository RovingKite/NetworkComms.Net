﻿//  Copyright 2011 Marc Fletcher, Matthew Dean
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SerializerBase
{
    /// <summary>
    /// Serializer that uss .Net built in BinaryFormatter
    /// </summary>
    public class BinaryFormaterSerializer : ArraySerializer
    {
        static BinaryFormaterSerializer instance;
        static object locker = new object();

        /// <summary>
        /// Singleton instance of serializer
        /// </summary>
        public static BinaryFormaterSerializer Instance
        {
            get
            {
                lock (locker)
                    if (instance == null)
                        instance = new BinaryFormaterSerializer();

                return instance;
            }
        }

        private BinaryFormaterSerializer() { }

        #region ISerialize Members

        /// <summary>
        /// Serializes objectToSerialize to a byte array using compression provided by compressor
        /// </summary>
        /// <typeparam name="T">Type paramter of objectToSerialize</typeparam>
        /// <param name="objectToSerialise">Object to serialize.  Must be marked Serializable</param>
        /// <param name="compressor">The compression provider to use</param>
        /// <returns>The serialized and compressed bytes of objectToSerialize</returns>
        public override byte[] SerialiseDataObject<T>(T objectToSerialise, ICompress compressor)
        {
            var baseRes = base.SerialiseDataObject<T>(objectToSerialise, compressor);

            if (baseRes != null)
                return baseRes;

            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream mem = new MemoryStream())
            {
                formatter.Serialize(mem, objectToSerialise);
                mem.Seek(0, 0);
                return compressor.CompressDataStream(mem);
            }        
        }

        /// <summary>
        /// Deserializes data object held as compressed bytes in receivedObjectBytes using compressor and BinaryFormatter
        /// </summary>
        /// <typeparam name="T">Type parameter of the resultant object</typeparam>
        /// <param name="receivedObjectBytes">Byte array containing serialized and compressed object</param>
        /// <param name="compressor">Compression provider to use</param>
        /// <returns>The deserialized object</returns>
        public override T DeserialiseDataObject<T>(byte[] receivedObjectBytes, ICompress compressor)
        {
            var baseRes = base.DeserialiseDataObject<T>(receivedObjectBytes, compressor);

            if (!Equals(baseRes, default(T)))
                return baseRes;

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            compressor.DecompressToStream(receivedObjectBytes, stream);
            stream.Seek(0,0);

            return (T)formatter.Deserialize(stream);
        }

        #endregion
    }
}
