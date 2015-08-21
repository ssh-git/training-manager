﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TM.UI.MVC.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TM.UI.MVC.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Argument must be positive integer.
        /// </summary>
        internal static string ArgumentOutOfRange_NegativeInteger {
            get {
                return ResourceManager.GetString("ArgumentOutOfRange_NegativeInteger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not acquire lock.
        /// </summary>
        internal static string CannotAcquireLock {
            get {
                return ResourceManager.GetString("CannotAcquireLock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Course with id = &apos;{0}&apos; not found in db.
        /// </summary>
        internal static string EntityNotFound_Course {
            get {
                return ResourceManager.GetString("EntityNotFound_Course", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Subscription for userId = &apos;{0}&apos; and course id = &apos;{1}&apos; not found in db.
        /// </summary>
        internal static string EntityNotFound_Subscription {
            get {
                return ResourceManager.GetString("EntityNotFound_Subscription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Training provider with name = &apos;{0}&apos; not found in database.
        /// </summary>
        internal static string EntityNotFound_TrainingProvider {
            get {
                return ResourceManager.GetString("EntityNotFound_TrainingProvider", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Operation not allowed. Value of &apos;{0}&apos; was set after &apos;{1}&apos; invokation..
        /// </summary>
        internal static string InvalidOperation_ValueSetAfterMethodInvoke {
            get {
                return ResourceManager.GetString("InvalidOperation_ValueSetAfterMethodInvoke", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Author sort not supported.
        /// </summary>
        internal static string NotSupported_AuthorsSort {
            get {
                return ResourceManager.GetString("NotSupported_AuthorsSort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You must select at least one specialization.
        /// </summary>
        internal static string SpecializationsNotSet {
            get {
                return ResourceManager.GetString("SpecializationsNotSet", resourceCulture);
            }
        }
    }
}