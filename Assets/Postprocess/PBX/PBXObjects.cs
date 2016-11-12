using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityEditor.iOS.Xcode
{
    using KnownProperties = SortedDictionary<string, PropertyType>;

    abstract class PBXObjectBase
    {
        public string guid;

        public void ReadFromSection(string curLine, TextReader sr)
        {
            guid = CommentedGUID.ReadString(curLine);
            ReadFromSectionImpl(curLine, sr);
        }

        public      abstract void WriteToSection(TextWriter sw, GUIDToCommentMap comments);
        protected   abstract void ReadFromSectionImpl(string curLine, TextReader srs);
    }

    class PBXBuildFile : PBXObjectBase
    {
        public string postfix;
        public string fileRef;

        protected override void ReadFromSectionImpl(string curLine, TextReader sr)
        {
            Match m = PBXRegex.BuildFile.Match(curLine);
            fileRef = m.Groups[2].Value;
            postfix = m.Groups[3].Value;
        }

        public override void WriteToSection(TextWriter sw, GUIDToCommentMap comments)
        {
            sw.WriteLine(
                "\t\t{0} = {{isa = PBXBuildFile; fileRef = {1}; {2}}};",
                CommentedGUID.Write(guid, comments),
                CommentedGUID.Write(fileRef, comments),
                postfix);
        }

        public static PBXBuildFile CreateFromFile(string fileRefGUID, bool weak,
                                                  string compileFlags)
        {
            var buildFile = new PBXBuildFile();
            buildFile.guid = PBXGUID.Generate();
            buildFile.fileRef = fileRefGUID;
            buildFile.postfix = "";
            if (weak || !string.IsNullOrEmpty(compileFlags))
            {
                string data = "";
                if (weak)
                    data += "ATTRIBUTES = (Weak, ); ";
                if (!string.IsNullOrEmpty(compileFlags))
                    data += "COMPILER_FLAGS = \"" + compileFlags.Replace("\"", "\\\"") + "\"; ";
                buildFile.postfix = "settings = {" + data + "}; ";
            }

            return buildFile;
        }
    }

    class PBXFileReference : PBXObjectBase
    {
        public string text;
        public string path;
        public string name;

        protected override void ReadFromSectionImpl(string curLine, TextReader sr)
        {
            Match m = PBXRegex.AnyKeyValue.Match(curLine.Trim());
            text = m.Groups[2].Value;

            m = PBXRegex.FileRef.Match(curLine);
            name = path = PBXStream.UnquoteString(m.Groups[2].Value.Trim());
            if (PBXRegex.FileRefName.IsMatch(curLine))
                name = PBXStream.UnquoteString(PBXRegex.FileRefName.Match(curLine).Groups[1].Value);
        }

        public override void WriteToSection(TextWriter sw, GUIDToCommentMap comments)
        {
            sw.WriteLine("\t\t{0} = {1};", CommentedGUID.Write(guid, comments), text);
        }

        public static PBXFileReference CreateFromFile(string path, string projectFileName,
			PBXSourceTree tree)
        {
            string guid = PBXGUID.Generate();

            var fileRef = new PBXFileReference();
            fileRef.guid = guid;

            fileRef.path    = path;
            fileRef.name    = projectFileName;
            fileRef.text    = String.Format("{{isa = PBXFileReference; lastKnownFileType = {0}; name = {1}; path = {2}; sourceTree = {3}; }}",
                                            FileTypeUtils.GetTypeName(Path.GetExtension(fileRef.name)),
                                            PBXStream.QuoteStringIfNeeded(fileRef.name),
                                            PBXStream.QuoteStringIfNeeded(fileRef.path),
                                            PBXStream.QuoteStringIfNeeded(FileTypeUtils.SourceTreeDesc(tree)));
            return fileRef;
        }
    }

    enum PropertyType
    {
        Regular,
        RegularList,
        CommentedGuid,
        CommentedGuidList,
        BuildPropertiesList,
        ProjectAttributeList,
        ProjectReferenceList,
        None
    }

    abstract class PBXObject : PBXObjectBase
    {
        protected List<string> m_BadLines = new List<string>();
        protected abstract KnownProperties knownProperties { get; }

        protected Dictionary<string, string> m_Properties = new Dictionary<string, string>();

        PropertyType GetPropertyTypeForLine(string line)
        {
            string trimmed = line.Trim();
            Match m = PBXRegex.Key.Match(trimmed);
            if (!m.Success)
                return PropertyType.None;
            string key = m.Groups[1].Value.Trim();
            if (!knownProperties.ContainsKey(key))
                return PropertyType.None;
            return knownProperties[key];
        }

        protected void ReadRegularProperty(string curLine)
        {
            Match m = PBXRegex.KeyValue.Match(curLine.Trim());
            string key = m.Groups[1].Value.Trim();
            m_Properties[key] = PBXStream.UnquoteString(m.Groups[2].Value.Trim());
        }

        protected void ReadCommentedGuidProperty(string curLine)
        {
            Match m = PBXRegex.KeyValue.Match(curLine.Trim());
            string key = m.Groups[1].Value.Trim();
            m_Properties[key] = CommentedGUID.ReadString(m.Groups[2].Value.Trim());
        }

        // return new curLine
        protected virtual string ReadProperty(PropertyType prop, string curLine, TextReader sr)
        {
            if (prop == PropertyType.Regular)
                ReadRegularProperty(curLine);
            else if (prop == PropertyType.CommentedGuid)
                ReadCommentedGuidProperty(curLine);
            return curLine;
        }

        protected override void ReadFromSectionImpl(string curLine, TextReader sr)
        {
            // TODO: the implementation works but is not most elegant
            curLine = sr.ReadLine();

            while (curLine.Trim() != "};")
            {
                PropertyType propType = GetPropertyTypeForLine(curLine);
                if (propType == PropertyType.None)
                    m_BadLines.Add(curLine);
                else
                    curLine = ReadProperty(propType, curLine, sr); // TODO: static analysis: "Value assigned is not used in any execution path"
                curLine = sr.ReadLine();
            }
        }

        protected void WriteRegularProperty(string prop, TextWriter sw)
        {
            sw.WriteLine("\t\t\t{0} = {1};", prop, PBXStream.QuoteStringIfNeeded(m_Properties[prop]));
        }

        protected void WriteCommentedGuidProperty(string prop, TextWriter sw, GUIDToCommentMap comments)
        {
            sw.WriteLine("\t\t\t{0} = {1};", prop, CommentedGUID.Write(m_Properties[prop], comments));
        }

        protected virtual void WriteProperty(PropertyType propType, string prop, TextWriter sw, GUIDToCommentMap comments)
        {
            if (propType == PropertyType.Regular)
                WriteRegularProperty(prop, sw);
            else if (propType == PropertyType.CommentedGuid)
                WriteCommentedGuidProperty(prop, sw, comments);
        }

        void WritePropertyWrapper(string prop, HashSet<string> processed, TextWriter sw, GUIDToCommentMap comments)
        {
            if (processed.Contains(prop))
                return;
            if (!knownProperties.ContainsKey(prop))
                return;
            WriteProperty(knownProperties[prop], prop, sw, comments);
            processed.Add(prop);
        }

        protected virtual IEnumerable<string> GetPropertyNames()
        {
            return m_Properties.Keys;
        }

        public override void WriteToSection(TextWriter sw, GUIDToCommentMap comments)
        {
            // TODO: the implementation works but is not elegant

            var processedProperties = new HashSet<string>();
            var allProps = new List<string>();
            allProps.AddRange(GetPropertyNames());
            allProps.Sort();

            sw.WriteLine("\t\t{0} = {{", CommentedGUID.Write(guid, comments));
            WritePropertyWrapper("isa", processedProperties, sw, comments); // always the first
            foreach (var prop in allProps)
                WritePropertyWrapper(prop, processedProperties, sw, comments);
            foreach (var line in m_BadLines)
                sw.WriteLine(line);
            sw.WriteLine("\t\t};");
        }
    }

    abstract class GUIDListBase : PBXObject
    {
        protected abstract string mainListName { get; }

        protected List<string> mainList { get { return m_ListProperties[mainListName]; } }
        protected Dictionary<string, List<string>> m_ListProperties = new Dictionary<string, List<string>>();

        public void AddGUID(string aGuid)
        {
            mainList.Add(aGuid);
        }

        public void RemoveGUID(string aGuid)
        {
            mainList.RemoveAll(x => x == aGuid);
        }

        protected string ReadRegularListProperty(string curLine, TextReader sr)
        {
            Match m = PBXRegex.ListHeader.Match(curLine.Trim());
            string key = m.Groups[1].Value.Trim();
            var list = new List<string>();
            curLine = sr.ReadLine();
            while (curLine.Trim() != ");")
            {
                list.Add(curLine.Trim().TrimEnd(",".ToArray()));
                curLine = sr.ReadLine();
            }
            m_ListProperties[key] = list;
            return curLine;
        }

        protected string ReadCommentedGuidListProperty(string curLine, TextReader sr)
        {
            Match m = PBXRegex.ListHeader.Match(curLine.Trim());
            string key = m.Groups[1].Value.Trim();
            var list = new List<string>();
            curLine = sr.ReadLine();
            while (curLine.Trim() != ");")
            {
                list.Add(CommentedGUID.ReadString(curLine));
                curLine = sr.ReadLine();
            }
            m_ListProperties[key] = list;
            return curLine;
        }

        // return new curLine
        protected override string ReadProperty(PropertyType prop, string curLine, TextReader sr)
        {
            if (prop == PropertyType.Regular)
                ReadRegularProperty(curLine);
            else if (prop == PropertyType.CommentedGuid)
                ReadCommentedGuidProperty(curLine);
            else if (prop == PropertyType.RegularList)
                curLine = ReadRegularListProperty(curLine, sr);
            else if (prop == PropertyType.CommentedGuidList)
                curLine = ReadCommentedGuidListProperty(curLine, sr);
            return curLine;
        }

        protected void WriteRegularListProperty(string prop, TextWriter sw)
        {
            sw.WriteLine("\t\t\t{0} = (", prop);
            var list = m_ListProperties[prop];
            foreach (string v in list)
                sw.WriteLine("\t\t\t\t{0},", v);
            sw.WriteLine("\t\t\t);");
        }

        protected void WriteCommentedGuidListProperty(string prop, TextWriter sw, GUIDToCommentMap comments)
        {
            sw.WriteLine("\t\t\t{0} = (", prop);
            var list = m_ListProperties[prop];
            foreach (string v in list)
                sw.WriteLine("\t\t\t\t{0},", CommentedGUID.Write(v, comments));
            sw.WriteLine("\t\t\t);");
        }

        protected override void WriteProperty(PropertyType propType, string prop, TextWriter sw, GUIDToCommentMap comments)
        {
            if (propType == PropertyType.Regular)
                WriteRegularProperty(prop, sw);
            else if (propType == PropertyType.CommentedGuid)
                WriteCommentedGuidProperty(prop, sw, comments);
            else if (propType == PropertyType.RegularList)
                WriteRegularListProperty(prop, sw);
            else if (propType == PropertyType.CommentedGuidList)
                WriteCommentedGuidListProperty(prop, sw, comments);
        }

        protected override IEnumerable<string> GetPropertyNames()
        {
            return m_Properties.Keys.Concat(m_ListProperties.Keys);
        }
    }

    class XCConfigurationList : GUIDListBase
    {
        protected override string mainListName { get { return "buildConfigurations"; } }

        public List<string> buildConfig { get { return mainList; } }

        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "buildConfigurations", PropertyType.CommentedGuidList },
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }

        public static XCConfigurationList Create()
        {
            var res = new XCConfigurationList();
            res.guid = PBXGUID.Generate();

            res.m_Properties["isa"] = "XCConfigurationList";
            res.m_ListProperties["buildConfigurations"] = new List<string>();
            res.m_Properties["defaultConfigurationIsVisible"] = "0";

            return res;
        }
    }

    class PBXGroup : GUIDListBase
    {
        protected override string mainListName { get { return "children"; } }

        public List<string> children { get { return mainList; } }

        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "name", PropertyType.Regular },
            { "children", PropertyType.CommentedGuidList },
            { "path", PropertyType.Regular },
            { "sourceTree", PropertyType.Regular },
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }

        public string name
        {
            get
            {
                if (m_Properties.ContainsKey("path"))
                    return m_Properties["path"];
                if (m_Properties.ContainsKey("name"))
                    return m_Properties["name"];
                return null;
            }
            set
            {
                if (m_Properties.ContainsKey("path"))
                    m_Properties["path"] = value;
                m_Properties["name"] = value;
            }
        }

        public static PBXGroup Create(string name)
        {
            var gr = new PBXGroup();
            gr.guid = PBXGUID.Generate();

            gr.m_Properties["isa"] = "PBXGroup";
            gr.m_Properties["path"] = name;
            gr.m_Properties["sourceTree"] = "<group>";
            gr.m_ListProperties["children"] = new List<string>();

            return gr;
        }
    }

    class PBXVariantGroup : PBXGroup
    {
    }

    class PBXNativeTarget : GUIDListBase
    {
        protected override string mainListName { get { return "buildPhases"; } }

        public List<string> phase { get { return mainList; } }

        public string buildConfigList
        {
            get { return m_Properties["buildConfigurationList"]; }
            set { m_Properties["buildConfigurationList"] = value; } // guid
        }

        public string name { get { return m_Properties["name"]; } }
        public List<string> dependencies { get { return m_ListProperties["dependencies"]; } }

        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "buildPhases", PropertyType.CommentedGuidList },
            { "buildRules", PropertyType.CommentedGuidList },
            { "dependencies", PropertyType.CommentedGuidList },
            { "name", PropertyType.Regular },
            { "productName", PropertyType.Regular },
            { "productType", PropertyType.Regular },
            { "productReference", PropertyType.CommentedGuid },
            { "buildConfigurationList", PropertyType.CommentedGuid },
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }

        public static PBXNativeTarget Create(string name, string productRef, string productType, string buildConfigList)
        {
            var res = new PBXNativeTarget();
            res.guid = PBXGUID.Generate();
            res.m_Properties["isa"] = "PBXNativeTarget";
            res.m_Properties["buildConfigurationList"] = buildConfigList;
            res.m_ListProperties["buildPhases"] = new List<string>();
            res.m_ListProperties["buildRules"] = new List<string>();
            res.m_ListProperties["dependencies"] = new List<string>();
            res.m_Properties["name"] = name;
            res.m_Properties["productName"] = name;
            res.m_Properties["productReference"] = productRef;
            res.m_Properties["productType"] = productType;
            return res;
        }
    }

    class FileGUIDListBase : GUIDListBase
    {
        protected override string mainListName { get { return "files"; } }

        public List<string> file { get { return mainList; } }

        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "buildActionMask", PropertyType.Regular },
            { "files", PropertyType.CommentedGuidList },
            { "runOnlyForDeploymentPostprocessing", PropertyType.Regular }
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }
    }

    class PBXSourcesBuildPhase : FileGUIDListBase
    {
        public static PBXSourcesBuildPhase Create()
        {
            var res = new PBXSourcesBuildPhase();
            res.guid = PBXGUID.Generate();
            res.m_Properties["isa"] = "PBXSourcesBuildPhase";
            res.m_Properties["buildActionMask"] = "2147483647";
            res.m_ListProperties["files"] = new List<string>();
            res.m_Properties["runOnlyForDeploymentPostprocessing"] = "0";
            return res;
        }
    }

    class PBXFrameworksBuildPhase : FileGUIDListBase
    {
        public static PBXFrameworksBuildPhase Create()
        {
            var res = new PBXFrameworksBuildPhase();
            res.guid = PBXGUID.Generate();
            res.m_Properties["isa"] = "PBXFrameworksBuildPhase";
            res.m_Properties["buildActionMask"] = "2147483647";
            res.m_ListProperties["files"] = new List<string>();
            res.m_Properties["runOnlyForDeploymentPostprocessing"] = "0";
            return res;
        }
    }

    class PBXResourcesBuildPhase : FileGUIDListBase
    {
        public static PBXResourcesBuildPhase Create()
        {
            var res = new PBXResourcesBuildPhase();
            res.guid = PBXGUID.Generate();
            res.m_Properties["isa"] = "PBXResourcesBuildPhase";
            res.m_Properties["buildActionMask"] = "2147483647";
            res.m_ListProperties["files"] = new List<string>();
            res.m_Properties["runOnlyForDeploymentPostprocessing"] = "0";
            return res;
        }
    }

    class PBXCopyFilesBuildPhase : FileGUIDListBase
    {
        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "buildActionMask", PropertyType.Regular },
            { "dstPath", PropertyType.Regular },
            { "dstSubfolderSpec", PropertyType.Regular },
            { "runOnlyForDeploymentPostprocessing", PropertyType.Regular },
            { "files", PropertyType.CommentedGuidList },
            { "name", PropertyType.Regular }
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }

        public string name
        {
            get
            {
                if (m_Properties.ContainsKey("name"))
                    return m_Properties["name"];
                return null;
            }
        }

        // name may be null
        public static PBXCopyFilesBuildPhase Create(string name, string subfolderSpec)
        {
            var res = new PBXCopyFilesBuildPhase();
            res.guid = PBXGUID.Generate();
            res.m_Properties["isa"] = "PBXCopyFilesBuildPhase";
            res.m_Properties["buildActionMask"] = "2147483647";
            res.m_Properties["dstPath"] = "";
            res.m_Properties["dstSubfolderSpec"] = subfolderSpec;
            res.m_ListProperties["files"] = new List<string>();
            res.m_Properties["runOnlyForDeploymentPostprocessing"] = "0";
            if (name != null)
                res.m_Properties["name"] = name;
            return res;
        }
    }

    class PBXShellScriptBuildPhase : GUIDListBase
    {
        protected override string mainListName { get { return "files"; } }

        public List<string> file { get { return mainList; } }

        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "buildActionMask", PropertyType.Regular },
            { "files", PropertyType.CommentedGuidList },
            { "inputPaths", PropertyType.RegularList },
            { "outputPaths", PropertyType.RegularList },
            { "shellPath", PropertyType.Regular },
            { "shellScript", PropertyType.Regular },
            { "runOnlyForDeploymentPostprocessing", PropertyType.Regular },
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }
    }

    class BuildConfigEntry
    {
        public string       name;
        public List<string> val = new List<string>();

        public static string ExtractValue(string src)
        {
            return PBXStream.UnquoteString(src.Trim().TrimEnd(','));
        }

        public void Read(string curLine, TextReader sr)
        {
            val.Clear();

            if (PBXRegex.ListHeader.IsMatch(curLine))
            {
                Match m = PBXRegex.ListHeader.Match(curLine);
                name = PBXStream.UnquoteString(m.Groups[1].Value);
                PBXStream.ReadLinesUntilConditionIsMet(sr, val, ExtractValue, s => s.Trim() == ");");
            }
            else
            {
                Match m = PBXRegex.KeyValue.Match(curLine);
                name = PBXStream.UnquoteString(m.Groups[1].Value);
                AddValue(PBXStream.UnquoteString(m.Groups[2].Value));
            }
        }

        public void Write(TextWriter sw, GUIDToCommentMap comments)
        {
            if (val.Count == 0)
            {
            }
            else if (val.Count == 1)
            {
                sw.WriteLine("\t\t\t\t{0} = {1};", PBXStream.QuoteStringIfNeeded(name), PBXStream.QuoteStringIfNeeded(val[0]));
            }
            else
            {
                sw.WriteLine("\t\t\t\t{0} = (", PBXStream.QuoteStringIfNeeded(name));
                foreach (string s in val)
                    sw.WriteLine("\t\t\t\t\t{0},", PBXStream.QuoteStringIfNeeded(s));
                sw.WriteLine("\t\t\t\t);");
            }
        }

        public void AddValue(string aVal)
        {
            val.Add(ExtractValue(aVal));
        }

        public static BuildConfigEntry FromNameValue(string name, string value)
        {
            var ret = new BuildConfigEntry();
            ret.name = name;
            ret.AddValue(value);
            return ret;
        }
    }

    class XCBuildConfiguration : PBXObject
    {
        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "buildSettings", PropertyType.BuildPropertiesList },
            { "name", PropertyType.Regular },
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }

        public SortedDictionary<string, BuildConfigEntry> entry = new SortedDictionary<string, BuildConfigEntry>();
        public string name { get { return m_Properties["name"]; } }

        protected string ReadBuildPropertiesListProperty(string curLine, TextReader sr)
        {
            if (curLine.Trim() != "buildSettings = {")
                return curLine;
            curLine = sr.ReadLine();
            while (curLine.Trim() != "};")
            {
                var val = new BuildConfigEntry();
                val.Read(curLine, sr);
                entry[val.name] = val;

                curLine = sr.ReadLine();
            }
            return curLine;
        }

        protected override string ReadProperty(PropertyType prop, string curLine, TextReader sr)
        {
            if (prop == PropertyType.Regular)
                ReadRegularProperty(curLine);
            else if (prop == PropertyType.CommentedGuid)
                ReadCommentedGuidProperty(curLine);
            else if (prop == PropertyType.BuildPropertiesList)
                curLine = ReadBuildPropertiesListProperty(curLine, sr);
            return curLine;
        }

        protected void WriteBuildPropertiesListProperty(string prop, TextWriter sw, GUIDToCommentMap comments)
        {
            sw.WriteLine("\t\t\tbuildSettings = {");
            foreach (BuildConfigEntry e in entry.Values)
                e.Write(sw, comments);
            sw.WriteLine("\t\t\t};");
        }

        protected override void WriteProperty(PropertyType propType, string prop, TextWriter sw, GUIDToCommentMap comments)
        {
            if (propType == PropertyType.Regular)
                WriteRegularProperty(prop, sw);
            else if (propType == PropertyType.CommentedGuid)
                WriteCommentedGuidProperty(prop, sw, comments);
            else if (propType == PropertyType.BuildPropertiesList)
                WriteBuildPropertiesListProperty(prop, sw, comments);
        }

        public void SetProperty(string aName, string value)
        {
            entry[aName] = BuildConfigEntry.FromNameValue(aName, value);
        }

        public void AddProperty(string aName, string value)
        {
            if (entry.ContainsKey(aName))
                entry[aName].AddValue(value);
            else
                SetProperty(aName, value);
        }

        public void UpdateProperties(string aName, string[] addValues, string[] removeValues)
        {
            if (entry.ContainsKey(aName))
            {
                var valSet = new HashSet<string>(entry[aName].val);

                if (removeValues != null) valSet.ExceptWith(removeValues);
                if (addValues != null) valSet.UnionWith(addValues);

                entry[aName].val = new List<string>(valSet);
            }
        }

        protected override IEnumerable<string> GetPropertyNames()
        {
            return m_Properties.Keys.Concat(new List<string>{"buildSettings"});
        }

        // name should be either release or debug
        public static XCBuildConfiguration Create(string name)
        {
            var res = new XCBuildConfiguration();
            res.guid = PBXGUID.Generate();
            res.m_Properties["isa"] = "XCBuildConfiguration";
            res.m_Properties["name"] = name;
            return res;
        }
    }

    class PBXContainerItemProxy : GUIDListBase
    {
        protected override string mainListName { get { return "none"; } }

        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "containerPortal", PropertyType.CommentedGuid }, // guid
            { "proxyType", PropertyType.Regular },
            { "remoteGlobalIDString", PropertyType.Regular },
            { "remoteInfo", PropertyType.Regular },
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }

        public static PBXContainerItemProxy Create(string containerRef, string proxyType,
                                                   string remoteGlobalGUID, string remoteInfo)
        {
            var res = new PBXContainerItemProxy();
            res.guid = PBXGUID.Generate();
            res.m_Properties["isa"] = "PBXContainerItemProxy";
            res.m_Properties["containerPortal"] = containerRef;
            res.m_Properties["proxyType"] = proxyType;
            res.m_Properties["remoteGlobalIDString"] = remoteGlobalGUID;
            res.m_Properties["remoteInfo"] = remoteInfo;
            return res;
        }
    }

    class PBXReferenceProxy : GUIDListBase
    {
        protected override string mainListName { get { return "none"; } }

        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "path", PropertyType.Regular },
            { "fileType", PropertyType.Regular },
            { "sourceTree", PropertyType.Regular },
            { "remoteRef", PropertyType.CommentedGuid }, // guid
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }

        public string path { get { return m_Properties["path"]; } }

        public static PBXReferenceProxy Create(string path, string fileType,
                                               string remoteRef, string sourceTree)
        {
            var res = new PBXReferenceProxy();
            res.guid = PBXGUID.Generate();
            res.m_Properties["isa"] = "PBXReferenceProxy";
            res.m_Properties["path"] = path;
            res.m_Properties["fileType"] = fileType;
            res.m_Properties["remoteRef"] = remoteRef;
            res.m_Properties["sourceTree"] = sourceTree;
            return res;
        }
    }

    class PBXTargetDependency : PBXObject
    {
        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "target", PropertyType.CommentedGuid },
            { "targetProxy", PropertyType.CommentedGuid }
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }

        public static PBXTargetDependency Create(string target, string targetProxy)
        {
            var res = new PBXTargetDependency();
            res.guid = PBXGUID.Generate();
            res.m_Properties["isa"] = "PBXTargetDependency";
            res.m_Properties["target"] = target;
            res.m_Properties["targetProxy"] = targetProxy;
            return res;
        }
    }

    class ProjectReference
    {
        public string group;      // guid
        public string projectRef; // guid

        public static ProjectReference Create(string group, string projectRef)
        {
            var res = new ProjectReference();
            res.group = group;
            res.projectRef = projectRef;
            return res;
        }

        public void Read(string curLine, TextReader sr)
        {
            if (curLine.Trim() != "{")
                throw new Exception("Wrong entry passed to ProjectReference.Read");

            curLine = sr.ReadLine();
            while (curLine.Trim() != "}")
            {
                Match m = PBXRegex.KeyValue.Match(curLine.Trim());
                if (m.Success)
                {
                    string key = m.Groups[1].Value;
                    string value = m.Groups[2].Value;

                    if (key == "ProductGroup")
                        group = CommentedGUID.ReadString(value);
                    else if (key == "ProjectRef")
                        projectRef = CommentedGUID.ReadString(value);
                }
                curLine = sr.ReadLine();
            }
        }

        public void Write(TextWriter sw, GUIDToCommentMap comments)
        {
            sw.WriteLine("\t\t\t\t{");
            sw.WriteLine("\t\t\t\t\tProductGroup = {0};", CommentedGUID.Write(@group, comments));
            sw.WriteLine("\t\t\t\t\tProjectRef = {0};", CommentedGUID.Write(projectRef, comments));
            sw.WriteLine("\t\t\t\t}");
        }
    }

    class PBXProjectObject : GUIDListBase
    {
        protected override string mainListName { get { return "targets"; } }

        static readonly KnownProperties k_KnownProps = new KnownProperties
        {
            { "isa", PropertyType.Regular },
            { "attributes", PropertyType.ProjectAttributeList },
            { "buildConfigurationList", PropertyType.CommentedGuid },
            { "compatibilityVersion", PropertyType.Regular },
            { "developmentRegion", PropertyType.Regular },
            { "hasScannedForEncodings", PropertyType.Regular },
            { "knownRegions", PropertyType.RegularList },
            { "mainGroup", PropertyType.CommentedGuid },
            { "projectDirPath", PropertyType.Regular },
            { "projectRoot", PropertyType.Regular },
            { "targets", PropertyType.CommentedGuidList },
            { "projectReferences", PropertyType.ProjectReferenceList },
        };

        protected override KnownProperties knownProperties { get { return k_KnownProps; } }

        public List<ProjectReference> projectReferences = new List<ProjectReference>();
        protected List<string> m_AttributeLines = new List<string>();
        public string mainGroup { get { return m_Properties["mainGroup"]; } }
        public List<string> targets { get { return m_ListProperties["targets"]; } }

        public string buildConfigList
        {
            get { return m_Properties["buildConfigurationList"]; }
            set { m_Properties["buildConfigurationList"] = value; }
        }

        protected string ReadProjectAttributeProperty(string curLine, TextReader sr)
        {
            if (!curLine.Contains("= {"))
                return curLine;

            m_AttributeLines.Clear();
            m_AttributeLines.Add(curLine);
            int nesting = 1;

            while (nesting > 0)
            {
                curLine = sr.ReadLine();
                if (curLine.Contains("= {"))
                    nesting += 1;
                else if (curLine.Trim() == "};")
                    nesting -= 1;
                m_AttributeLines.Add(curLine);
            }
            return curLine;
        }

        protected string ReadProjectReferenceList(string curLine, TextReader sr)
        {
            if (curLine.Trim() != "projectReferences = (")
                return curLine;
            curLine = sr.ReadLine();

            while (curLine.Trim() != ");")
            {
                if (curLine.Trim() == "{")
                {
                    var newRef = new ProjectReference();
                    newRef.Read(curLine, sr);
                    projectReferences.Add(newRef);
                }
                curLine = sr.ReadLine();
            }
            return curLine;
        }

        protected override string ReadProperty(PropertyType prop, string curLine, TextReader sr)
        {
            if (prop == PropertyType.Regular)
                ReadRegularProperty(curLine);
            else if (prop == PropertyType.CommentedGuid)
                ReadCommentedGuidProperty(curLine);
            else if (prop == PropertyType.RegularList)
                curLine = ReadRegularListProperty(curLine, sr);
            else if (prop == PropertyType.CommentedGuidList)
                curLine = ReadCommentedGuidListProperty(curLine, sr);
            else if (prop == PropertyType.ProjectAttributeList)
                curLine = ReadProjectAttributeProperty(curLine, sr);
            else if (prop == PropertyType.ProjectReferenceList)
                curLine = ReadProjectReferenceList(curLine, sr);
            return curLine;
        }

        protected void WriteProjectAttributeListProperty(string prop, TextWriter sw, GUIDToCommentMap comments)
        {
            foreach (string line in m_AttributeLines)
                sw.WriteLine(line);
        }

        protected void WriteProjectReferenceListProperty(string prop, TextWriter sw, GUIDToCommentMap comments)
        {
            if (projectReferences.Count == 0)
                return;

            sw.WriteLine("\t\t\tprojectReferences = (");
            foreach (var projRef in projectReferences)
            {
                projRef.Write(sw, comments);
            }
            sw.WriteLine("\t\t\t);");
        }

        protected override void WriteProperty(PropertyType propType, string prop, TextWriter sw, GUIDToCommentMap comments)
        {
            if (propType == PropertyType.Regular)
                WriteRegularProperty(prop, sw);
            else if (propType == PropertyType.CommentedGuid)
                WriteCommentedGuidProperty(prop, sw, comments);
            else if (propType == PropertyType.RegularList)
                WriteRegularListProperty(prop, sw);
            else if (propType == PropertyType.CommentedGuidList)
                WriteCommentedGuidListProperty(prop, sw, comments);
            else if (propType == PropertyType.ProjectAttributeList)
                WriteProjectAttributeListProperty(prop, sw, comments);
            else if (propType == PropertyType.ProjectReferenceList)
                WriteProjectReferenceListProperty(prop, sw, comments);
        }

        protected override IEnumerable<string> GetPropertyNames()
        {
            return m_Properties.Keys.Concat(m_ListProperties.Keys).Concat(new List<string>{"projectReferences", "attributes"});
        }

        public void AddReference(string productGroup, string projectRef)
        {
            projectReferences.Add(ProjectReference.Create(productGroup, projectRef));
        }
    }
} // namespace UnityEditor.iOS.Xcode
