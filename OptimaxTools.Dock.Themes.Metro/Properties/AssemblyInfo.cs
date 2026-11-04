using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

// In order to begin building localizable applications, set 
// <UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
// inside a <PropertyGroup>.  For example, if you are using US english
// in your source files, set the <UICulture> to en-US.  Then uncomment
// the NeutralResourceLanguage attribute below.  Update the "en-US" in
// the line below to match the UICulture setting in the project file.

// [assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]
[assembly: ThemeInfo(
	ResourceDictionaryLocation.None, // where theme specific resource dictionaries are located
									 // (used if a resource is not found in the page, 
									 // or application resource dictionaries)
	ResourceDictionaryLocation.SourceAssembly) // where the generic resource dictionary is located
											  // (used if a resource is not found in the page, 
											  // app, or any theme specific resource dictionaries)
]

[assembly: XmlnsDefinition("http://schemas.optimax.com/wpf/xaml/dock", "OptimaxTools.Dock.Themes")]
[assembly: XmlnsPrefix("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "OptimaxTools.Dock.Themes")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "OptimaxTools.Dock.Themes")]

// OptimaxTools.Dock is a fork of Dirkster.AvalonDock 5.0.1 (https://github.com/Dirkster99/AvalonDock), itself a fork of
// Xceed's AvalonDock, under the Microsoft Public License (see LICENSE). The solution's Directory.Build.props turns off
// the attributes the SDK would generate, so the identity is declared here.
[assembly: System.Reflection.AssemblyTitle("OptimaxTools.Dock.Themes.Metro")]
[assembly: System.Reflection.AssemblyDescription("The Metro theme of OptimaxTools.Dock")]
[assembly: System.Reflection.AssemblyConfiguration("")]
[assembly: System.Reflection.AssemblyCompany("Optimax Corporation")]
[assembly: System.Reflection.AssemblyProduct("OptimaxTools.Dock")]
[assembly: System.Reflection.AssemblyCopyright("Based on Dirkster.AvalonDock 5.0.1 and Xceed AvalonDock, Microsoft Public License (MS-PL)")]
[assembly: System.Reflection.AssemblyTrademark("")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersion("1.0.0, from Dirkster.AvalonDock 5.0.1")]
