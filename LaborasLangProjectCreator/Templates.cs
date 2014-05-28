using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangProjectCreator
{
    static class Templates
    {
        // {0} - file name
        // {1} - output exe name
        public static string VCXProjTemplate
        {
            get
            {
                return string.Join("\r\n",
                    @"<?xml version=""1.0"" encoding=""utf-8""?>",
                    @"<Project DefaultTargets=""Build"" ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">",
                    @"  <ItemGroup Label=""ProjectConfigurations"">",
                    @"    <ProjectConfiguration Include=""Debug|Win32"">",
                    @"      <Configuration>Debug</Configuration>",
                    @"      <Platform>Win32</Platform>",
                    @"    </ProjectConfiguration>",
                    @"    <ProjectConfiguration Include=""Release|Win32"">",
                    @"      <Configuration>Release</Configuration>",
                    @"      <Platform>Win32</Platform>",
                    @"    </ProjectConfiguration>",
                    @"  </ItemGroup>",
                    @"  <ItemGroup>",
                    @"    <None Include=""{0}"" />",
                    @"  </ItemGroup>",
                    @"  <PropertyGroup Label=""Globals"">",
                    @"    <ProjectGuid>{{576ECAB5-8EF3-4961-B9A6-A78000AAB3D7}}</ProjectGuid>",
                    @"    <Keyword>MakeFileProj</Keyword>",
                    @"    <LLOutputDir>$(SolutionDir)Binaries\$(Configuration)\</LLOutputDir>",
                    @"  </PropertyGroup>",
                    @"  <PropertyGroup Label=""Configuration"">",
                    @"    <ConfigurationType>Makefile</ConfigurationType>",
                    @"  </PropertyGroup>",
                    @"  <ImportGroup Label=""PropertySheets"" Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">",
                    @"    <Import Project=""$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"" Condition=""exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"" Label=""LocalAppDataPlatform"" />",
                    @"  </ImportGroup>",
                    @"  <ImportGroup Label=""PropertySheets"" Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">",
                    @"    <Import Project=""$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"" Condition=""exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"" Label=""LocalAppDataPlatform"" />",
                    @"  </ImportGroup>",
                    @"  <PropertyGroup Label=""UserMacros"" />",
                    @"  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">",
                    @"    <NMakeOutput>$(LLOutputDir){1}</NMakeOutput>",
                    @"    <NMakePreprocessorDefinitions>",
                    @"    </NMakePreprocessorDefinitions>",
                    @"    <NMakeBuildCommandLine>if exist ""$(NMakeOutput)"" del ""$(NMakeOutput)""",
                    @"""$(SolutionDir)..\Compiler\LaborasLangCompiler.exe"" ""$(ProjectDir){0}"" /debug /out:""$(LLOutputDir){1}""</NMakeBuildCommandLine>",
                    @"    <NMakeReBuildCommandLine>if exist ""$(NMakeOutput)"" del ""$(NMakeOutput)""",
                    @"""$(SolutionDir)..\Compiler\LaborasLangCompiler.exe"" ""$(ProjectDir){0}"" /debug /out:""$(LLOutputDir){1}""</NMakeReBuildCommandLine>",
                    @"    <NMakeCleanCommandLine>if exist ""$(NMakeOutput)"" del ""$(NMakeOutput)""</NMakeCleanCommandLine>",
                    @"    <OutDir>$(LLOutputDir)</OutDir>",
                    @"    <IntDir />",
                    @"    <ExecutablePath />",
                    @"    <IncludePath />",
                    @"    <ReferencePath />",
                    @"    <LibraryPath />",
                    @"    <LibraryWPath />",
                    @"    <SourcePath />",
                    @"    <ExcludePath />",
                    @"  </PropertyGroup>",
                    @"  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">",
                    @"    <NMakeOutput>$(LLOutputDir){1}</NMakeOutput>",
                    @"    <NMakePreprocessorDefinitions>",
                    @"    </NMakePreprocessorDefinitions>",
                    @"    <NMakeBuildCommandLine>if exist ""$(NMakeOutput)"" del ""$(NMakeOutput)""",
                    @"""$(SolutionDir)..\Compiler\LaborasLangCompiler.exe"" ""$(ProjectDir){0}"" /out:""$(LLOutputDir){1}""</NMakeBuildCommandLine>",
                    @"    <NMakeReBuildCommandLine>if exist ""$(NMakeOutput)"" del ""$(NMakeOutput)""",
                    @"""$(SolutionDir)..\Compiler\LaborasLangCompiler.exe"" ""$(ProjectDir){1}"" /out:""$(LLOutputDir){1}""</NMakeReBuildCommandLine>",
                    @"    <NMakeCleanCommandLine>if exist ""$(NMakeOutput)"" del ""$(NMakeOutput)""</NMakeCleanCommandLine>",
                    @"    <OutDir>$(LLOutputDir)</OutDir>",
                    @"    <IntDir />",
                    @"    <ExecutablePath />",
                    @"    <IncludePath />",
                    @"    <ReferencePath />",
                    @"    <LibraryPath />",
                    @"    <LibraryWPath />",
                    @"    <SourcePath />",
                    @"    <ExcludePath />",
                    @"  </PropertyGroup>",
                    @"  <ItemDefinitionGroup>",
                    @"  </ItemDefinitionGroup>",
                    @"  <Import Project=""$(VCTargetsPath)\Microsoft.Cpp.targets"" />",
                    @"  <ImportGroup Label=""ExtensionTargets"">",
                    @"  </ImportGroup>",
                    @"</Project>"
                );
            }
        }

        // {0} - project name
        public static string SLNTemplate
        {
            get
            {
                return string.Join("\r\n",
                    @"Microsoft Visual Studio Solution File, Format Version 12.00",
                    @"Project(""{{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}}"") = ""{0}"", ""{0}\{0}.vcxproj"", ""{{576ECAB5-8EF3-4961-B9A6-A78000AAB3D7}}""",
                    @"EndProject",
                    @"Global",
                    @"	GlobalSection(SolutionConfigurationPlatforms) = preSolution",
                    @"		Debug|LaborasLang = Debug|LaborasLang",
                    @"		Release|LaborasLang = Release|LaborasLang",
                    @"	EndGlobalSection",
                    @"	GlobalSection(ProjectConfigurationPlatforms) = postSolution",
                    @"		{{576ECAB5-8EF3-4961-B9A6-A78000AAB3D7}}.Debug|LaborasLang.ActiveCfg = Debug|Win32",
                    @"		{{576ECAB5-8EF3-4961-B9A6-A78000AAB3D7}}.Debug|LaborasLang.Build.0 = Debug|Win32",
                    @"		{{576ECAB5-8EF3-4961-B9A6-A78000AAB3D7}}.Release|LaborasLang.ActiveCfg = Release|Win32",
                    @"		{{576ECAB5-8EF3-4961-B9A6-A78000AAB3D7}}.Release|LaborasLang.Build.0 = Release|Win32",
                    @"	EndGlobalSection",
                    @"	GlobalSection(SolutionProperties) = preSolution",
                    @"		HideSolutionNode = FALSE",
                    @"	EndGlobalSection",
                    @"EndGlobal"
                );
            }
        }

        // {0} - file name
        public static string FiltersTemplate
        {
            get
            {
                return string.Join("\r\n",
                    @"<?xml version=""1.0"" encoding=""utf-8""?>",
                    @"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">",
                    @"  <ItemGroup>",
                    @"    <None Include=""{0}"" />",
                    @"  </ItemGroup>",
                    @"</Project>"
                );
            }
        }

        public static string UsersTemplate
        {
            get
            {
                return string.Join("\r\n",
                    @"<?xml version=""1.0"" encoding=""utf-8""?>",
                    @"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">",
                    @"  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">",
                    @"    <LocalDebuggerWorkingDirectory>$(LLOutputDir)</LocalDebuggerWorkingDirectory>",
                    @"    <DebuggerFlavor>WindowsLocalDebugger</DebuggerFlavor>",
                    @"    <LocalDebuggerDebuggerType>Mixed</LocalDebuggerDebuggerType>",
                    @"  </PropertyGroup>",
                    @"  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">",
                    @"    <LocalDebuggerDebuggerType>Mixed</LocalDebuggerDebuggerType>",
                    @"    <DebuggerFlavor>WindowsLocalDebugger</DebuggerFlavor>",
                    @"    <LocalDebuggerWorkingDirectory>$(LLOutputDir)</LocalDebuggerWorkingDirectory>",
                    @"  </PropertyGroup>",
                    @"</Project>"
                );
            }
        }
    }
}
