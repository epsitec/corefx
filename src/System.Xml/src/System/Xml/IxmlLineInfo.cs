// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Xml
{
    /// <include file='doc\IXmlLineInfo.uex' path='docs/doc[@for="IXmlLineInfo"]/*' />
    public interface IXmlLineInfo
    {
        /// <include file='doc\IXmlLineInfo.uex' path='docs/doc[@for="IXmlLineInfo.HasLineInfo"]/*' />
        bool HasLineInfo();
        /// <include file='doc\IXmlLineInfo.uex' path='docs/doc[@for="IXmlLineInfo.LineNumber"]/*' />
        int LineNumber { get; }
        /// <include file='doc\IXmlLineInfo.uex' path='docs/doc[@for="IXmlLineInfo.LinePosition"]/*' />
        int LinePosition { get; }
    }

#if !SILVERLIGHT
    internal class PositionInfo : IXmlLineInfo
    {
        public virtual bool HasLineInfo() { return false; }
        public virtual int LineNumber { get { return 0; } }
        public virtual int LinePosition { get { return 0; } }

        public static PositionInfo GetPositionInfo(Object o)
        {
            IXmlLineInfo li = o as IXmlLineInfo;
            if (li != null)
            {
                return new ReaderPositionInfo(li);
            }
            else
            {
                return new PositionInfo();
            }
        }
    }

    internal class ReaderPositionInfo : PositionInfo
    {
        private IXmlLineInfo _lineInfo;

        public ReaderPositionInfo(IXmlLineInfo lineInfo)
        {
            _lineInfo = lineInfo;
        }

        public override bool HasLineInfo()
        {
            return _lineInfo.HasLineInfo();
        }

        public override int LineNumber
        {
            get
            {
                return _lineInfo.LineNumber;
            }
        }

        public override int LinePosition
        {
            get
            {
                return _lineInfo.LinePosition;
            }
        }
    }
#endif
}// namespace
