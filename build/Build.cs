using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Parameter("github package registry username")]
    public string GithubUsername { get; set; }
    [Parameter("github package registry password")]
    public string GithubToken { get; set; }
    [Parameter("github package url")]
    public string GithubPacakgeUrl { get; set; }
    [Parameter("nuget package version suffix")]
    public string PackageVersionSuffix { get; set; } = string.Format("alpha.{0}", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));

    [Solution] readonly Solution Solution;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            var setting = new DotNetCleanSettings()
                .SetConfiguration(Configuration);
            DotNetClean(setting);
            EnsureCleanDirectory(RootDirectory / "dist");
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            var setting = new DotNetRestoreSettings();
            DotNetRestore(setting);
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var buildsetting = new DotNetBuildSettings()
                .SetConfiguration(Configuration);
            DotNetBuild(buildsetting);
        });
    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var outputdir = RootDirectory / "dist" / Configuration;
            var packsetting = new DotNetPackSettings()
                .SetConfiguration(Configuration)
                .SetOutputDirectory(outputdir)
                ;
            if(!string.IsNullOrEmpty(PackageVersionSuffix))
            {
                packsetting = packsetting.SetVersionSuffix(PackageVersionSuffix);
            }
            DotNetPack(packsetting);
        });

    Target GithubPush => _ => _
        .DependsOn(Pack)
        .Requires(() => Configuration == "Release")
        .Requires(() => string.IsNullOrEmpty(GithubToken))
        .Executes(() =>
        {
            string nugetconfig = string.Format(@"
<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
    <packageSources>
        <clear />
        <add key=""github"" value=""{0}"" />
    </packageSources>
    <packageSourceCredentials>
        <github>
            <add key=""Username"" value=""{1}"" />
            <add key=""ClearTextPassword"" value=""{2}"" />
        </github>
    </packageSourceCredentials>
</configuration>
", GithubPacakgeUrl, GithubUsername, GithubToken);
            EnsureExistingDirectory(RootDirectory / "tmp");
            System.IO.File.WriteAllText(RootDirectory / "tmp" / "NuGet.config", nugetconfig);
            try
            {
                foreach (var pkg in System.IO.Directory.EnumerateFiles(RootDirectory / "dist" / Configuration, "*.nupkg").Concat(System.IO.Directory.EnumerateFiles(RootDirectory / "dist" / Configuration, "*.snupkg")))
                {
                    var setting = new NuGetPushSettings()
                        .EnableForceEnglishOutput()
                        .SetSource("github")
                        .SetConfigFile(RootDirectory / "tmp" / "NuGet.config")
                        .SetTargetPath(pkg)
                        .SetVerbosity(NuGetVerbosity.Normal)
                        ;
                    NuGetTasks.NuGetPush(setting);
                }
            }
            finally
            {
                EnsureCleanDirectory(RootDirectory / "tmp");
            }
        });

}
