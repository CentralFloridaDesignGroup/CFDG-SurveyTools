﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CFDG.UI.windows.Export {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.1.0.0")]
    internal sealed partial class SaveDialogSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static SaveDialogSettings defaultInstance = ((SaveDialogSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new SaveDialogSettings())));
        
        public static SaveDialogSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AutoAddDate {
            get {
                return ((bool)(this["AutoAddDate"]));
            }
            set {
                this["AutoAddDate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AutomaticName {
            get {
                return ((bool)(this["AutomaticName"]));
            }
            set {
                this["AutomaticName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool OpenAfterSave {
            get {
                return ((bool)(this["OpenAfterSave"]));
            }
            set {
                this["OpenAfterSave"] = value;
            }
        }
    }
}
