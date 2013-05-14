﻿using System;

namespace Kamilla.Network.Logging
{
    public class NetworkLogWrapper
    {
        public readonly string Name;
        public readonly string FileExtension;
        public readonly NetworkLogFlags Flags;
        public readonly byte[] ReqHeader;
        public int ReqHeaderLength { get { return ReqHeader.Length; } }

        public readonly string FileFilter;
        public bool IsReadOnly { get { return (Flags & NetworkLogFlags.ReadOnly) != 0; } }

        private readonly Type m_type;

        public NetworkLogWrapper(NetworkLogAttribute attr, Type type)
        {
            m_type = type;

            this.FileExtension = attr.FileExtension;
            this.Flags = attr.Flags;
            this.ReqHeader = attr.HeaderBytes;

            using (var dummy = this.Activate(NetworkLogMode.Abstract))
                this.Name = dummy.Name;

            this.FileFilter = this.Name + " (*." + this.FileExtension + ")|*." + this.FileExtension;
        }

        public bool Fits(byte[] header)
        {
            if (ReqHeader.Length > header.Length)
                return false;

            for (int i = 0; i < ReqHeader.Length; ++i)
            {
                if (ReqHeader[i] != header[i])
                    return false;
            }

            return true;
        }

        public NetworkLog Activate(NetworkLogMode mode)
        {
            if (IsReadOnly)
            {
                if (mode == NetworkLogMode.Writing)
                    throw new NotSupportedException("This dump reading class does not support writing data.");

                mode = NetworkLogMode.Abstract;
            }

            try
            {
                return (NetworkLog)Activator.CreateInstance(m_type, mode);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: [NetworkLogWrapper] Failed to activate a new instance of {0} with mode {1}: {2}",
                    m_type, mode, e);

                return null;
            }
        }
    }
}
