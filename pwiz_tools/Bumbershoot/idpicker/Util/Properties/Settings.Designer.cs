﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3615
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IDPicker.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int MinDistinctPeptides {
            get {
                return ((int)(this["MinDistinctPeptides"]));
            }
            set {
                this["MinDistinctPeptides"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NormalizeSearchScores {
            get {
                return ((bool)(this["NormalizeSearchScores"]));
            }
            set {
                this["NormalizeSearchScores"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ApplyScoreOptimization {
            get {
                return ((bool)(this["ApplyScoreOptimization"]));
            }
            set {
                this["ApplyScoreOptimization"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int MaxAmbiguousIds {
            get {
                return ((int)(this["MaxAmbiguousIds"]));
            }
            set {
                this["MaxAmbiguousIds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public int OptimizeScorePermutations {
            get {
                return ((int)(this["OptimizeScorePermutations"]));
            }
            set {
                this["OptimizeScorePermutations"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public int MinPeptideLength {
            get {
                return ((int)(this["MinPeptideLength"]));
            }
            set {
                this["MinPeptideLength"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("r-")]
        public string DecoyPrefix {
            get {
                return ((string)(this["DecoyPrefix"]));
            }
            set {
                this["DecoyPrefix"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public float MaxFdr {
            get {
                return ((float)(this["MaxFdr"]));
            }
            set {
                this["MaxFdr"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>mvh,1</string>
  <string>xcorr,1</string>
  <string>hyperscore,1</string>
  <string>ionscore,1</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection Scores {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["Scores"]));
            }
            set {
                this["Scores"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>M,15.994,1</string>\r\n  <string>C,57.0,1</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection Mods {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["Mods"]));
            }
            set {
                this["Mods"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ModsAreDistinctByDefault {
            get {
                return ((bool)(this["ModsAreDistinctByDefault"]));
            }
            set {
                this["ModsAreDistinctByDefault"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("IDPicker.log")]
        public string LogFileName {
            get {
                return ((string)(this["LogFileName"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int DebugLevel {
            get {
                return ((int)(this["DebugLevel"]));
            }
            set {
                this["DebugLevel"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int MinAdditionalPeptides {
            get {
                return ((int)(this["MinAdditionalPeptides"]));
            }
            set {
                this["MinAdditionalPeptides"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("mzML;RAW;mzXML;MGF")]
        public string SourceExtensions {
            get {
                return ((string)(this["SourceExtensions"]));
            }
            set {
                this["SourceExtensions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>&lt;RootInputDirectory&gt;</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection FastaPaths {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["FastaPaths"]));
            }
            set {
                this["FastaPaths"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>&lt;RootInputDirectory&gt;</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection SourcePaths {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["SourcePaths"]));
            }
            set {
                this["SourcePaths"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>&lt;RootInputDirectory&gt;</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection SearchPaths {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["SearchPaths"]));
            }
            set {
                this["SearchPaths"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int MinSpectraPerProtein {
            get {
                return ((int)(this["MinSpectraPerProtein"]));
            }
            set {
                this["MinSpectraPerProtein"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\")]
        public string DefaultFileOpenDirectory {
            get {
                return ((string)(this["DefaultFileOpenDirectory"]));
            }
            set {
                this["DefaultFileOpenDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\")]
        public string DefaultSpectrumSourceDirectory {
            get {
                return ((string)(this["DefaultSpectrumSourceDirectory"]));
            }
            set {
                this["DefaultSpectrumSourceDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\")]
        public string LastFileOpenDirectory {
            get {
                return ((string)(this["LastFileOpenDirectory"]));
            }
            set {
                this["LastFileOpenDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\")]
        public string LastSpectrumSourceDirectory {
            get {
                return ((string)(this["LastSpectrumSourceDirectory"]));
            }
            set {
                this["LastSpectrumSourceDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\")]
        public string LastProteinDatabaseDirectory {
            get {
                return ((string)(this["LastProteinDatabaseDirectory"]));
            }
            set {
                this["LastProteinDatabaseDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TopRankOnly {
            get {
                return ((bool)(this["TopRankOnly"]));
            }
            set {
                this["TopRankOnly"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>&lt;RootInputDirectory&gt;</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection SpectrumTableFormSettings {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["SpectrumTableFormSettings"]));
            }
            set {
                this["SpectrumTableFormSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>&lt;RootInputDirectory&gt;</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection PeptideTableFormSettings {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["PeptideTableFormSettings"]));
            }
            set {
                this["PeptideTableFormSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>&lt;RootInputDirectory&gt;</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection ProteinTableFormSettings {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["ProteinTableFormSettings"]));
            }
            set {
                this["ProteinTableFormSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection QonverterSettings {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["QonverterSettings"]));
            }
            set {
                this["QonverterSettings"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>MyriMatch MVH;StaticWeighted;False;1 Ascending Off mvh</string>
  <string>MyriMatch XCorr;StaticWeighted;False;1 Ascending Off xcorr</string>
  <string>Sequest XCorr;StaticWeighted;False;1 Ascending Off xcorr</string>
  <string>Mascot ionscore;StaticWeighted;False;1 Ascending Off ionscore</string>
  <string>Mascot ionscore-identityscore;StaticWeighted;False;1 Ascending Off ionscore;1 Descending Off identityscore</string>
  <string>X! Tandem hyperscore;StaticWeighted;False;1 Ascending Off hyperscore</string>
  <string>X! Tandem expect;StaticWeighted;False;1 Descending Off expect</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection DefaultQonverterSettings {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["DefaultQonverterSettings"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>&lt;RootInputDirectory&gt;</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection UserLayouts {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["UserLayouts"]));
            }
            set {
                this["UserLayouts"] = value;
            }
        }
    }
}
