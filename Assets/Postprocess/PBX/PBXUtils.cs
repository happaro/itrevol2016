using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UnityEditor.iOS.Xcode
{
    class CommentedGUID
    {
        public static string ReadString(string line)
        {
            Match m = PBXRegex.GUID.Match(line);
            return m.Groups[1].Value;
        }

        public static string Write(string guid, GUIDToCommentMap comments)
        {
            string comment = comments[guid];
            if (comment == null)
                return guid;
            return String.Format("{0} /* {1} */", guid, comment);
        }
    }

    class GUIDToCommentMap
    {
        readonly Dictionary<string, string> m_Dict = new Dictionary<string, string>();

        public string this[string guid]
        {
            get
            {
                if (m_Dict.ContainsKey(guid))
                    return m_Dict[guid];
                return null;
            }
        }

        public void Add(string guid, string comment)
        {
            if (m_Dict.ContainsKey(guid))
                return;
            m_Dict.Add(guid, comment);
        }

        public void Remove(string guid)
        {
            m_Dict.Remove(guid);
        }
    }

    class PBXGUID
    {
        public delegate string GuidGenerator();

        // We allow changing Guid generator to make testing of PBXProject possible
        static GuidGenerator s_GUIDGenerator = DefaultGuidGenerator;

        public static string DefaultGuidGenerator()
        {
            return Guid.NewGuid().ToString("N").Substring(8).ToUpper();
        }

        public static void SetGuidGenerator(GuidGenerator generator)
        {
            s_GUIDGenerator = generator;
        }

        // Generates a GUID.
        public static string Generate()
        {
            return s_GUIDGenerator();
        }
    }

    class PBXRegex
    {
        public static string GuidRegexString = "[A-Fa-f0-9]{24}";
        const string k_CommentRegexString = "/\\*\\s+([^\\*]+)\\s+\\*/";

        public static Regex BeginSection    = new Regex("^/\\* Begin (\\w+) section \\*/$");
        public static Regex EndSection      = new Regex("^/\\* End (\\w+) section \\*/$");

        public static Regex GUID            = new Regex(String.Format("({0})", GuidRegexString));
        public static Regex GUIDComment     = new Regex(String.Format("({0}) {1}", GuidRegexString, k_CommentRegexString));
        public static Regex Key             = new Regex("(\\w+) = ");
        public static Regex KeyValue        = new Regex("(\\S+) = ([^;]+);$");
        public static Regex AnyKeyValue     = new Regex("([^=]+) = (.*);$");
        public static Regex ListHeader      = new Regex("(\\w+) = \\($");

        public static Regex BuildFile       = new Regex(String.Format("({0}).*fileRef = ({1})[^;]*; (.*)\\}};", GuidRegexString, GuidRegexString));
        public static Regex FileRef         = new Regex(String.Format("({0}).*path = ([^;]+)", GuidRegexString));
        public static Regex FileRefName     = new Regex(String.Format("name = ([^;]+)"));

        public static Regex DontNeedQuotes  = new Regex("^[\\w\\d\\./]+$");

        public static string ExtractGUID(string s)
        {
            return GUID.Match(s).Value;
        }
    }

    class PBXStream
    {
        public static string ReadSkippingEmptyLines(TextReader sr)
        {
            string line = sr.ReadLine();
            while (String.IsNullOrEmpty(line))
                line = sr.ReadLine();

            return line;
        }

        // Quotes the given string if it contains special characters. Note: if the string already
        // contains quotes, then they are escaped and the entire string quoted again
        public static string QuoteStringIfNeeded(string src)
        {
            if (PBXRegex.DontNeedQuotes.IsMatch(src))
                return src;
            return "\"" + src.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
        }

        // If the given string is quoted, removes the quotes and unescapes any quotes within the string
        public static string UnquoteString(string src)
        {
            if (!src.StartsWith("\"") || !src.EndsWith("\""))
                return src;
            return src
                   .Substring(1, src.Length - 2)
                   .Replace("\\\\", "\u569f")
                   .Replace("\\\"", "\"")
                   .Replace("\\n", "\n")
                   .Replace("\u569f", "\\"); // U+569f is a rarely used Chinese character
        }

        public delegate bool    ConditionOnString(string s);
        public delegate string  ProcessString(string s);
        public static void ReadLinesWithConditionForLastLine(TextReader sr, List<string> list, ConditionOnString cond)
        {
            string line;
            do
            {
                line = sr.ReadLine();
                list.Add(line);
            }
            while (!cond(line));
        }

        public static string ReadLinesUntilConditionIsMet(TextReader sr, List<string> list, ProcessString proc, ConditionOnString cond)
        {
            string line = sr.ReadLine();
            while (!cond(line))
            {
                list.Add(proc(line));
                line = sr.ReadLine();
            }
            return line;
        }

        public static void ReadLinesFromFile(TextReader sr, List<string> list)
        {
            while (sr.Peek() != -1)
                list.Add(sr.ReadLine());
        }
    }

    enum PBXFileType
    {
        NotBuildable,
        Framework,
        Source,
        Resource,
        CopyFile
    }

    public enum PBXSourceTree
    {
        Absolute,
        Group,
        Build,
        Developer,
        Sdk,
        Source
    };

    class FileTypeUtils
    {
        class FileTypeDesc
        {
            public FileTypeDesc(string typeName, PBXFileType type)
            {
                name = typeName;
                this.type = type;
            }

            public string name;
            public PBXFileType type;
        }

        static readonly Dictionary<string, FileTypeDesc> k_Types =
            new Dictionary<string, FileTypeDesc>
        {
            { ".a",          new FileTypeDesc("archive.ar",            PBXFileType.Framework) },
            { ".app",        new FileTypeDesc("wrapper.application",   PBXFileType.NotBuildable) },
            { ".appex",      new FileTypeDesc("wrapper.app-extension", PBXFileType.CopyFile) },
            { ".s",          new FileTypeDesc("sourcecode.asm",        PBXFileType.Source) },
            { ".c",          new FileTypeDesc("sourcecode.c.c",        PBXFileType.Source) },
            { ".cc",         new FileTypeDesc("sourcecode.cpp.cpp",    PBXFileType.Source) },
            { ".cpp",        new FileTypeDesc("sourcecode.cpp.cpp",    PBXFileType.Source) },
            { ".swift",      new FileTypeDesc("sourcecode.swift",      PBXFileType.Source) },
            { ".dll",        new FileTypeDesc("file",                  PBXFileType.NotBuildable) },
            { ".framework",  new FileTypeDesc("wrapper.framework",     PBXFileType.Framework) },
            { ".h",          new FileTypeDesc("sourcecode.c.h",        PBXFileType.NotBuildable) },
            { ".pch",        new FileTypeDesc("sourcecode.c.h",        PBXFileType.NotBuildable) },
            { ".icns",       new FileTypeDesc("image.icns",            PBXFileType.Resource) },
            { ".inc",        new FileTypeDesc("sourcecode.inc",        PBXFileType.NotBuildable) },
            { ".m",          new FileTypeDesc("sourcecode.c.objc",     PBXFileType.Source) },
            { ".mm",         new FileTypeDesc("sourcecode.cpp.objcpp", PBXFileType.Source) },
            { ".nib",        new FileTypeDesc("wrapper.nib",           PBXFileType.Resource) },
            { ".plist",      new FileTypeDesc("text.plist.xml",        PBXFileType.Resource) },
            { ".png",        new FileTypeDesc("image.png",             PBXFileType.Resource) },
            { ".rtf",        new FileTypeDesc("text.rtf",              PBXFileType.Resource) },
            { ".tiff",       new FileTypeDesc("image.tiff",            PBXFileType.Resource) },
            { ".txt",        new FileTypeDesc("text",                  PBXFileType.Resource) },
            { ".xcodeproj",  new FileTypeDesc("wrapper.pb-project",    PBXFileType.NotBuildable) },
            { ".xib",        new FileTypeDesc("file.xib",              PBXFileType.Resource) },
            { ".strings",    new FileTypeDesc("text.plist.strings",    PBXFileType.Resource) },
            { ".storyboard", new FileTypeDesc("file.storyboard",       PBXFileType.Resource) },
            { ".bundle",     new FileTypeDesc("wrapper.plug-in",       PBXFileType.Resource) },
            { ".dylib",      new FileTypeDesc("compiled.mach-o.dylib", PBXFileType.Framework) }
        };

        public static bool IsKnownExtension(string ext)
        {
            return k_Types.ContainsKey(ext);
        }

        public static PBXFileType GetFileType(string ext)
        {
            if (k_Types.ContainsKey(ext))
                return k_Types[ext].type;
            return PBXFileType.NotBuildable;
        }

        public static string GetTypeName(string ext)
        {
            if (k_Types.ContainsKey(ext))
                return k_Types[ext].name;
            return "text";
        }

        public static bool IsBuildable(string ext)
        {
            if (k_Types.ContainsKey(ext) && k_Types[ext].type != PBXFileType.NotBuildable)
                return true;
            return false;
        }

        static readonly Dictionary<PBXSourceTree, string> k_SourceTree = new Dictionary<PBXSourceTree, string>
        {
            { PBXSourceTree.Absolute,   "<absolute>" },
            { PBXSourceTree.Group,      "<group>" },
            { PBXSourceTree.Build,      "BUILT_PRODUCTS_DIR" },
            { PBXSourceTree.Developer,  "DEVELOPER_DIR" },
            { PBXSourceTree.Sdk,        "SDKROOT" },
            { PBXSourceTree.Source,     "SOURCE_ROOT" },
        };

        public static string SourceTreeDesc(PBXSourceTree tree)
        {
            return k_SourceTree[tree];
        }
    }
} // UnityEditor.iOS.Xcode
