using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda.Analyzers.Autofac;
using Mutagen.Bethesda.Analyzers.GitHub.Args;
using Mutagen.Bethesda.Analyzers.Skyrim;
using Mutagen.Bethesda.Environments.DI;

namespace Mutagen.Bethesda.Analyzers.GitHub;

public static class RunDiscussionsCreator
{
    public static async Task<int> Run(CreateDiscussionsCommand cmd)
    {
        var container = GetContainer(cmd);

        var getTopics = container.Resolve<GetTopicDefinitions>();

        var topicDefinitions = cmd.Directory is null
            ? getTopics.FromRegistered()
            : getTopics.FromDirectory(cmd.Directory);

        var createDiscussions = container.Resolve<CreateGitHubDiscussions>();
        await createDiscussions.CreateFromTopicDefinition(topicDefinitions);

        return 0;
    }

    private static IContainer GetContainer(CreateDiscussionsCommand cmd)
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(new FileSystem()).As<IFileSystem>();
        builder.RegisterInstance(new GameReleaseInjection(cmd.GameRelease)).AsImplementedInterfaces();
        builder.RegisterModule<MainModule>();
        builder.RegisterModule<SkyrimAnalyzerModule>();
        builder.RegisterType<GetTopicDefinitions>().AsSelf();
        builder.RegisterType<CreateGitHubDiscussions>().AsSelf();
        builder.RegisterInstance(cmd).As<CreateDiscussionsCommand>();

        return builder.Build();
    }
}
