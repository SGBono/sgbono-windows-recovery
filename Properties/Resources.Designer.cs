﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace beforewindeploy_custom_recovery.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("beforewindeploy_custom_recovery.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;WLANProfile xmlns=&quot;http://www.microsoft.com/networking/WLAN/profile/v1&quot;&gt;
        ///	&lt;name&gt;&lt;/name&gt;
        ///	&lt;SSIDConfig&gt;
        ///		&lt;SSID&gt;
        ///			&lt;name&gt;&lt;/name&gt;
        ///		&lt;/SSID&gt;
        ///	&lt;/SSIDConfig&gt;
        ///	&lt;connectionType&gt;ESS&lt;/connectionType&gt;
        ///	&lt;connectionMode&gt;auto&lt;/connectionMode&gt;
        ///	&lt;MSM&gt;
        ///		&lt;security&gt;
        ///			&lt;authEncryption&gt;
        ///				&lt;authentication&gt;&lt;/authentication&gt;
        ///				&lt;encryption&gt;AES&lt;/encryption&gt;
        ///				&lt;useOneX&gt;false&lt;/useOneX&gt;
        ///			&lt;/authEncryption&gt;
        ///			&lt;sharedKey&gt;
        ///				&lt;keyType&gt;passPhrase&lt;/keyType&gt;
        ///				&lt;protected&gt;false&lt;/protecte [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string WiFiTemplate {
            get {
                return ResourceManager.GetString("WiFiTemplate", resourceCulture);
            }
        }
    }
}
