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

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            var setting = new DotNetCleanSettings()
                .SetConfiguration(Configuration);
            DotNetClean(setting);
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
            DotNetPack(packsetting);
        });

}
