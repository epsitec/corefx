// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.Versioning;

#if !SILVERLIGHT
#if !HIDE_XSL
using System.Xml.Xsl.Runtime;
#endif
#endif

namespace System.Xml
{
#if !SILVERLIGHT
    public enum XmlOutputMethod
    {
        Xml = 0,    // Use Xml 1.0 rules to serialize
        Html = 1,    // Use Html rules specified by Xslt specification to serialize
        Text = 2,    // Only serialize text blocks
        AutoDetect = 3,    // Choose between Xml and Html output methods at runtime (using Xslt rules to do so)
    }
#endif

    /// <summary>
    /// Three-state logic enumeration.
    /// </summary>
    internal enum TriState
    {
        Unknown = -1,
        False = 0,
        True = 1,
    };

    internal enum XmlStandalone
    {
        // Do not change the constants - XmlBinaryWriter depends in it
        Omit = 0,
        Yes = 1,
        No = 2,
    }

    // XmlWriterSettings class specifies basic features of an XmlWriter.
    public sealed class XmlWriterSettings
    {
        //
        // Fields
        //

#if ASYNC || FEATURE_NETCORE
        private bool _useAsync;
#endif

        // Text settings
        private Encoding _encoding;

#if FEATURE_LEGACYNETCF
        private bool dontWriteEncodingTag;
#endif

        private bool _omitXmlDecl;
        private NewLineHandling _newLineHandling;
        private string _newLineChars;
        private TriState _indent;
        private string _indentChars;
        private bool _newLineOnAttributes;
        private bool _closeOutput;
        private NamespaceHandling _namespaceHandling;

        // Conformance settings
        private ConformanceLevel _conformanceLevel;
        private bool _checkCharacters;
        private bool _writeEndDocumentOnClose;

#if !SILVERLIGHT
        // Xslt settings
        private XmlOutputMethod _outputMethod;
        private List<XmlQualifiedName> _cdataSections = new List<XmlQualifiedName>();
        private bool _doNotEscapeUriAttributes;
        private bool _mergeCDataSections;
        private string _mediaType;
        private string _docTypeSystem;
        private string _docTypePublic;
        private XmlStandalone _standalone;
        private bool _autoXmlDecl;
#endif

        // read-only flag
        private bool _isReadOnly;

        //
        // Constructor
        //
        public XmlWriterSettings()
        {
            Initialize();
        }

        //
        // Properties
        //

#if ASYNC || FEATURE_NETCORE
        public bool Async
        {
            get
            {
                return _useAsync;
            }
            set
            {
                CheckReadOnly("Async");
                _useAsync = value;
            }
        }
#endif

        // Text
        public Encoding Encoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                CheckReadOnly("Encoding");
                _encoding = value;
            }
        }

#if FEATURE_LEGACYNETCF
        internal bool DontWriteEncodingTag
        {
            get
            {
                return dontWriteEncodingTag;
            }
            set
            {
                CheckReadOnly("DontWriteEncodingTag");
                dontWriteEncodingTag = value;
            }
        }
#endif

        // True if an xml declaration should *not* be written.
        public bool OmitXmlDeclaration
        {
            get
            {
                return _omitXmlDecl;
            }
            set
            {
                CheckReadOnly("OmitXmlDeclaration");
                _omitXmlDecl = value;
            }
        }

        // See NewLineHandling enum for details.
        public NewLineHandling NewLineHandling
        {
            get
            {
                return _newLineHandling;
            }
            set
            {
                CheckReadOnly("NewLineHandling");

                if ((uint)value > (uint)NewLineHandling.None)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _newLineHandling = value;
            }
        }

        // Line terminator string. By default, this is a carriage return followed by a line feed ("\r\n").
        public string NewLineChars
        {
            get
            {
                return _newLineChars;
            }
            set
            {
                CheckReadOnly("NewLineChars");

                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _newLineChars = value;
            }
        }

        // True if output should be indented using rules that are appropriate to the output rules (i.e. Xml, Html, etc).
        public bool Indent
        {
            get
            {
                return _indent == TriState.True;
            }
            set
            {
                CheckReadOnly("Indent");
                _indent = value ? TriState.True : TriState.False;
            }
        }

        // Characters to use when indenting. This is usually tab or some spaces, but can be anything.
        public string IndentChars
        {
            get
            {
                return _indentChars;
            }
            set
            {
                CheckReadOnly("IndentChars");

                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _indentChars = value;
            }
        }

        // Whether or not indent attributes on new lines.
        public bool NewLineOnAttributes
        {
            get
            {
                return _newLineOnAttributes;
            }
            set
            {
                CheckReadOnly("NewLineOnAttributes");
                _newLineOnAttributes = value;
            }
        }

        // Whether or not the XmlWriter should close the underlying stream or TextWriter when Close is called on the XmlWriter.
        public bool CloseOutput
        {
            get
            {
                return _closeOutput;
            }
            set
            {
                CheckReadOnly("CloseOutput");
                _closeOutput = value;
            }
        }


        // Conformance
        // See ConformanceLevel enum for details.
        public ConformanceLevel ConformanceLevel
        {
            get
            {
                return _conformanceLevel;
            }
            set
            {
                CheckReadOnly("ConformanceLevel");

                if ((uint)value > (uint)ConformanceLevel.Document)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _conformanceLevel = value;
            }
        }

        // Whether or not to check content characters that they are valid XML characters.
        public bool CheckCharacters
        {
            get
            {
                return _checkCharacters;
            }
            set
            {
                CheckReadOnly("CheckCharacters");
                _checkCharacters = value;
            }
        }

        // Whether or not to remove duplicate namespace declarations
        public NamespaceHandling NamespaceHandling
        {
            get
            {
                return _namespaceHandling;
            }
            set
            {
                CheckReadOnly("NamespaceHandling");
                if ((uint)value > (uint)(NamespaceHandling.OmitDuplicates))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _namespaceHandling = value;
            }
        }

        //Whether or not to auto complete end-element when close/dispose
        public bool WriteEndDocumentOnClose
        {
            get
            {
                return _writeEndDocumentOnClose;
            }
            set
            {
                CheckReadOnly("WriteEndDocumentOnClose");
                _writeEndDocumentOnClose = value;
            }
        }

#if !SILVERLIGHT
        // Specifies the method (Html, Xml, etc.) that will be used to serialize the result tree.
        public XmlOutputMethod OutputMethod
        {
            get
            {
                return _outputMethod;
            }
            internal set
            {
                _outputMethod = value;
            }
        }
#endif

        //
        // Public methods
        //
        public void Reset()
        {
            CheckReadOnly("Reset");
            Initialize();
        }

        // Deep clone all settings (except read-only, which is always set to false).  The original and new objects
        // can now be set independently of each other.
        public XmlWriterSettings Clone()
        {
            XmlWriterSettings clonedSettings = MemberwiseClone() as XmlWriterSettings;

#if !SILVERLIGHT
            // Deep clone shared settings that are not immutable
            clonedSettings._cdataSections = new List<XmlQualifiedName>(_cdataSections);
#endif

            clonedSettings._isReadOnly = false;
            return clonedSettings;
        }

        //
        // Internal properties
        //
#if !SILVERLIGHT
        // Set of XmlQualifiedNames that identify any elements that need to have text children wrapped in CData sections.
        internal List<XmlQualifiedName> CDataSectionElements
        {
            get
            {
                Debug.Assert(_cdataSections != null);
                return _cdataSections;
            }
        }

        // Used in Html writer to disable encoding of uri attributes
        public bool DoNotEscapeUriAttributes
        {
            get
            {
                return _doNotEscapeUriAttributes;
            }
            set
            {
                CheckReadOnly("DoNotEscapeUriAttributes");
                _doNotEscapeUriAttributes = value;
            }
        }

        internal bool MergeCDataSections
        {
            get
            {
                return _mergeCDataSections;
            }
            set
            {
                CheckReadOnly("MergeCDataSections");
                _mergeCDataSections = value;
            }
        }

        // Used in Html writer when writing Meta element.  Null denotes the default media type.
        internal string MediaType
        {
            get
            {
                return _mediaType;
            }
            set
            {
                CheckReadOnly("MediaType");
                _mediaType = value;
            }
        }

        // System Id in doc-type declaration.  Null denotes the absence of the system Id.
        internal string DocTypeSystem
        {
            get
            {
                return _docTypeSystem;
            }
            set
            {
                CheckReadOnly("DocTypeSystem");
                _docTypeSystem = value;
            }
        }

        // Public Id in doc-type declaration.  Null denotes the absence of the public Id.
        internal string DocTypePublic
        {
            get
            {
                return _docTypePublic;
            }
            set
            {
                CheckReadOnly("DocTypePublic");
                _docTypePublic = value;
            }
        }

        // Yes for standalone="yes", No for standalone="no", and Omit for no standalone.
        internal XmlStandalone Standalone
        {
            get
            {
                return _standalone;
            }
            set
            {
                CheckReadOnly("Standalone");
                _standalone = value;
            }
        }

        // True if an xml declaration should automatically be output (no need to call WriteStartDocument)
        internal bool AutoXmlDeclaration
        {
            get
            {
                return _autoXmlDecl;
            }
            set
            {
                CheckReadOnly("AutoXmlDeclaration");
                _autoXmlDecl = value;
            }
        }

        // If TriState.Unknown, then Indent property was not explicitly set.  In this case, the AutoDetect output
        // method will default to Indent=true for Html and Indent=false for Xml.
        internal TriState IndentInternal
        {
            get
            {
                return _indent;
            }
            set
            {
                _indent = value;
            }
        }

        internal bool IsQuerySpecific
        {
            get
            {
                return _cdataSections.Count != 0 || _docTypePublic != null ||
                       _docTypeSystem != null || _standalone == XmlStandalone.Yes;
            }
        }
#endif

#if !SILVERLIGHT
        internal XmlWriter CreateWriter(string outputFileName)
        {
            if (outputFileName == null)
            {
                throw new ArgumentNullException("outputFileName");
            }

            // need to clone the settigns so that we can set CloseOutput to true to make sure the stream gets closed in the end
            XmlWriterSettings newSettings = this;
            if (!newSettings.CloseOutput)
            {
                newSettings = newSettings.Clone();
                newSettings.CloseOutput = true;
            }

            FileStream fs = null;
            try
            {
                // open file stream
#if !ASYNC
                fs = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
#else
                fs = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.Read, 0x1000, _useAsync);
#endif

                // create writer
                return newSettings.CreateWriter(fs);
            }
            catch
            {
                if (fs != null)
                {
                    fs.Dispose();
                }
                throw;
            }
        }
#endif

        internal XmlWriter CreateWriter(Stream output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            XmlWriter writer;

            // create raw writer
#if SILVERLIGHT
            Debug.Assert(Encoding.UTF8.WebName == "utf-8");
            if (this.Encoding.WebName == "utf-8") { // Encoding.CodePage is not supported in Silverlight
                // create raw UTF-8 writer
                if (this.Indent) {
                    writer = new XmlUtf8RawTextWriterIndent(output, this);
                }
                else {
                    writer = new XmlUtf8RawTextWriter(output, this);
                }
            }
            else {
                // create raw writer for other encodings
                if (this.Indent) {
                    writer = new XmlEncodedRawTextWriterIndent(output, this);
                }
                else {
                    writer = new XmlEncodedRawTextWriter(output, this);
                }
            }
#else
            Debug.Assert(Encoding.UTF8.WebName == "utf-8");
            if (this.Encoding.WebName == "utf-8")
            { // Encoding.CodePage is not supported in Silverlight
                // create raw UTF-8 writer
                switch (this.OutputMethod)
                {
                    case XmlOutputMethod.Xml:
                        if (this.Indent)
                        {
                            writer = new XmlUtf8RawTextWriterIndent(output, this);
                        }
                        else
                        {
                            writer = new XmlUtf8RawTextWriter(output, this);
                        }
                        break;
                    case XmlOutputMethod.Html:
                        if (this.Indent)
                        {
                            writer = new HtmlUtf8RawTextWriterIndent(output, this);
                        }
                        else
                        {
                            writer = new HtmlUtf8RawTextWriter(output, this);
                        }
                        break;
                    case XmlOutputMethod.Text:
                        writer = new TextUtf8RawTextWriter(output, this);
                        break;
                    case XmlOutputMethod.AutoDetect:
                        writer = new XmlAutoDetectWriter(output, this);
                        break;
                    default:
                        Debug.Assert(false, "Invalid XmlOutputMethod setting.");
                        return null;
                }
            }
            else
            {
                // Otherwise, create a general-purpose writer than can do any encoding
                switch (this.OutputMethod)
                {
                    case XmlOutputMethod.Xml:
                        if (this.Indent)
                        {
                            writer = new XmlEncodedRawTextWriterIndent(output, this);
                        }
                        else
                        {
                            writer = new XmlEncodedRawTextWriter(output, this);
                        }
                        break;
                    case XmlOutputMethod.Html:
                        if (this.Indent)
                        {
                            writer = new HtmlEncodedRawTextWriterIndent(output, this);
                        }
                        else
                        {
                            writer = new HtmlEncodedRawTextWriter(output, this);
                        }
                        break;
                    case XmlOutputMethod.Text:
                        writer = new TextEncodedRawTextWriter(output, this);
                        break;
                    case XmlOutputMethod.AutoDetect:
                        writer = new XmlAutoDetectWriter(output, this);
                        break;
                    default:
                        Debug.Assert(false, "Invalid XmlOutputMethod setting.");
                        return null;
                }
            }

            // Wrap with Xslt/XQuery specific writer if needed; 
            // XmlOutputMethod.AutoDetect writer does this lazily when it creates the underlying Xml or Html writer.
            if (this.OutputMethod != XmlOutputMethod.AutoDetect)
            {
                if (this.IsQuerySpecific)
                {
                    // Create QueryOutputWriter if CData sections or DocType need to be tracked
                    writer = new QueryOutputWriter((XmlRawWriter)writer, this);
                }
            }
#endif // !SILVERLIGHT

            // wrap with well-formed writer
            writer = new XmlWellFormedWriter(writer, this);

#if ASYNC
            if (_useAsync)
            {
                writer = new XmlAsyncCheckWriter(writer);
            }
#endif

            return writer;
        }

        internal XmlWriter CreateWriter(TextWriter output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            XmlWriter writer;

            // create raw writer
#if SILVERLIGHT
            if (this.Indent) {
                writer = new XmlEncodedRawTextWriterIndent(output, this);
            }
            else {
                writer = new XmlEncodedRawTextWriter(output, this);
            }
#else 
            switch (this.OutputMethod)
            {
                case XmlOutputMethod.Xml:
                    if (this.Indent)
                    {
                        writer = new XmlEncodedRawTextWriterIndent(output, this);
                    }
                    else
                    {
                        writer = new XmlEncodedRawTextWriter(output, this);
                    }
                    break;
                case XmlOutputMethod.Html:
                    if (this.Indent)
                    {
                        writer = new HtmlEncodedRawTextWriterIndent(output, this);
                    }
                    else
                    {
                        writer = new HtmlEncodedRawTextWriter(output, this);
                    }
                    break;
                case XmlOutputMethod.Text:
                    writer = new TextEncodedRawTextWriter(output, this);
                    break;
                case XmlOutputMethod.AutoDetect:
                    writer = new XmlAutoDetectWriter(output, this);
                    break;
                default:
                    Debug.Assert(false, "Invalid XmlOutputMethod setting.");
                    return null;
            }

            // XmlOutputMethod.AutoDetect writer does this lazily when it creates the underlying Xml or Html writer.
            if (this.OutputMethod != XmlOutputMethod.AutoDetect)
            {
                if (this.IsQuerySpecific)
                {
                    // Create QueryOutputWriter if CData sections or DocType need to be tracked
                    writer = new QueryOutputWriter((XmlRawWriter)writer, this);
                }
            }
#endif //SILVERLIGHT

            // wrap with well-formed writer
            writer = new XmlWellFormedWriter(writer, this);

#if ASYNC
            if (_useAsync)
            {
                writer = new XmlAsyncCheckWriter(writer);
            }
#endif
            return writer;
        }

        internal XmlWriter CreateWriter(XmlWriter output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            return AddConformanceWrapper(output);
        }


        internal bool ReadOnly
        {
            get
            {
                return _isReadOnly;
            }
            set
            {
                _isReadOnly = value;
            }
        }

        private void CheckReadOnly(string propertyName)
        {
            if (_isReadOnly)
            {
                throw new XmlException(Res.Xml_ReadOnlyProperty, this.GetType().Name + '.' + propertyName);
            }
        }

        //
        // Private methods
        //
        private void Initialize()
        {
            _encoding = Encoding.UTF8;
            _omitXmlDecl = false;
            _newLineHandling = NewLineHandling.Replace;
            _newLineChars = Environment.NewLine; // "\r\n" on Windows, "\n" on Unix
            _indent = TriState.Unknown;
            _indentChars = "  ";
            _newLineOnAttributes = false;
            _closeOutput = false;
            _namespaceHandling = NamespaceHandling.Default;
            _conformanceLevel = ConformanceLevel.Document;
            _checkCharacters = true;
            _writeEndDocumentOnClose = true;

#if !SILVERLIGHT
            _outputMethod = XmlOutputMethod.Xml;
            _cdataSections.Clear();
            _mergeCDataSections = false;
            _mediaType = null;
            _docTypeSystem = null;
            _docTypePublic = null;
            _standalone = XmlStandalone.Omit;
            _doNotEscapeUriAttributes = false;
#endif

#if ASYNC || FEATURE_NETCORE
            _useAsync = false;
#endif
            _isReadOnly = false;
        }

        private XmlWriter AddConformanceWrapper(XmlWriter baseWriter)
        {
            ConformanceLevel confLevel = ConformanceLevel.Auto;
            XmlWriterSettings baseWriterSettings = baseWriter.Settings;
            bool checkValues = false;
            bool checkNames = false;
            bool replaceNewLines = false;
            bool needWrap = false;

            if (baseWriterSettings == null)
            {
                // assume the V1 writer already do all conformance checking; 
                // wrap only if NewLineHandling == Replace or CheckCharacters is true
                if (_newLineHandling == NewLineHandling.Replace)
                {
                    replaceNewLines = true;
                    needWrap = true;
                }
                if (_checkCharacters)
                {
                    checkValues = true;
                    needWrap = true;
                }
            }
            else
            {
                if (_conformanceLevel != baseWriterSettings.ConformanceLevel)
                {
                    confLevel = this.ConformanceLevel;
                    needWrap = true;
                }
                if (_checkCharacters && !baseWriterSettings.CheckCharacters)
                {
                    checkValues = true;
                    checkNames = confLevel == ConformanceLevel.Auto;
                    needWrap = true;
                }
                if (_newLineHandling == NewLineHandling.Replace &&
                     baseWriterSettings.NewLineHandling == NewLineHandling.None)
                {
                    replaceNewLines = true;
                    needWrap = true;
                }
            }

            XmlWriter writer = baseWriter;

            if (needWrap)
            {
                if (confLevel != ConformanceLevel.Auto)
                {
                    writer = new XmlWellFormedWriter(writer, this);
                }
                if (checkValues || replaceNewLines)
                {
                    writer = new XmlCharCheckingWriter(writer, checkValues, checkNames, replaceNewLines, this.NewLineChars);
                }
            }

#if !SILVERLIGHT
            if (this.IsQuerySpecific && (baseWriterSettings == null || !baseWriterSettings.IsQuerySpecific))
            {
                // Create QueryOutputWriterV1 if CData sections or DocType need to be tracked
                writer = new QueryOutputWriterV1(writer, this);
            }
#endif

            return writer;
        }

        //
        // Internal methods
        //

#if !SILVERLIGHT

#if !HIDE_XSL
        /// <summary>
        /// Serialize the object to BinaryWriter.
        /// </summary>
        internal void GetObjectData(XmlQueryDataWriter writer) {
            // Encoding encoding;
            // NOTE: For Encoding we serialize only CodePage, and ignore EncoderFallback/DecoderFallback.
            // It suffices for XSLT purposes, but not in the general case.
            Debug.Assert(Encoding.Equals(Encoding.GetEncoding(Encoding.CodePage)), "Cannot serialize encoding correctly");
            writer.Write(Encoding.CodePage);
            // bool omitXmlDecl;
            writer.Write(OmitXmlDeclaration);
            // NewLineHandling newLineHandling;
            writer.Write((sbyte)NewLineHandling);
            // string newLineChars;
            writer.WriteStringQ(NewLineChars);
            // TriState indent;
            writer.Write((sbyte)IndentInternal);
            // string indentChars;
            writer.WriteStringQ(IndentChars);
            // bool newLineOnAttributes;
            writer.Write(NewLineOnAttributes);
            // bool closeOutput;
            writer.Write(CloseOutput);
            // ConformanceLevel conformanceLevel;
            writer.Write((sbyte)ConformanceLevel);
            // bool checkCharacters;
            writer.Write(CheckCharacters);
            // XmlOutputMethod outputMethod;
            writer.Write((sbyte)outputMethod);
            // List<XmlQualifiedName> cdataSections;
            writer.Write(cdataSections.Count);
            foreach (XmlQualifiedName qname in cdataSections) {
                writer.Write(qname.Name);
                writer.Write(qname.Namespace);
            }
            // bool mergeCDataSections;
            writer.Write(mergeCDataSections);
            // string mediaType;
            writer.WriteStringQ(mediaType);
            // string docTypeSystem;
            writer.WriteStringQ(docTypeSystem);
            // string docTypePublic;
            writer.WriteStringQ(docTypePublic);
            // XmlStandalone standalone;
            writer.Write((sbyte)standalone);
            // bool autoXmlDecl;
            writer.Write(autoXmlDecl);
            // bool isReadOnly;
            writer.Write(ReadOnly);
        }

        /// <summary>
        /// Deserialize the object from BinaryReader.
        /// </summary>
        internal XmlWriterSettings(XmlQueryDataReader reader) {
            // Encoding encoding;
            Encoding = Encoding.GetEncoding(reader.ReadInt32());
            // bool omitXmlDecl;
            OmitXmlDeclaration = reader.ReadBoolean();
            // NewLineHandling newLineHandling;
            NewLineHandling = (NewLineHandling)reader.ReadSByte(0, (sbyte)NewLineHandling.None);
            // string newLineChars;
            NewLineChars = reader.ReadStringQ();
            // TriState indent;
            IndentInternal = (TriState)reader.ReadSByte((sbyte)TriState.Unknown, (sbyte)TriState.True);
            // string indentChars;
            IndentChars = reader.ReadStringQ();
            // bool newLineOnAttributes;
            NewLineOnAttributes = reader.ReadBoolean();
            // bool closeOutput;
            CloseOutput = reader.ReadBoolean();
            // ConformanceLevel conformanceLevel;
            ConformanceLevel = (ConformanceLevel)reader.ReadSByte(0, (sbyte)ConformanceLevel.Document);
            // bool checkCharacters;
            CheckCharacters = reader.ReadBoolean();
            // XmlOutputMethod outputMethod;
            outputMethod = (XmlOutputMethod)reader.ReadSByte(0, (sbyte)XmlOutputMethod.AutoDetect);
            // List<XmlQualifiedName> cdataSections;
            int length = reader.ReadInt32();
            cdataSections = new List<XmlQualifiedName>(length);
            for (int idx = 0; idx < length; idx++) {
                cdataSections.Add(new XmlQualifiedName(reader.ReadString(), reader.ReadString()));
            }
            // bool mergeCDataSections;
            mergeCDataSections = reader.ReadBoolean();
            // string mediaType;
            mediaType = reader.ReadStringQ();
            // string docTypeSystem;
            docTypeSystem = reader.ReadStringQ();
            // string docTypePublic;
            docTypePublic = reader.ReadStringQ();
            // XmlStandalone standalone;
            Standalone = (XmlStandalone)reader.ReadSByte(0, (sbyte)XmlStandalone.No);
            // bool autoXmlDecl;
            autoXmlDecl = reader.ReadBoolean();
            // bool isReadOnly;
            ReadOnly = reader.ReadBoolean();
        }
#else
        internal void GetObjectData(object writer) { }
        internal XmlWriterSettings(object reader) { }
#endif

#endif
    }
}
