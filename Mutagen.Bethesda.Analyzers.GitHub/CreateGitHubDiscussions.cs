using Mutagen.Bethesda.Analyzers.GitHub.Args;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;
using Octokit.GraphQL.Model;

namespace Mutagen.Bethesda.Analyzers.GitHub;

public class CreateGitHubDiscussions(CreateDiscussionsCommand arguments)
{
    public async Task CreateFromTopicDefinition(IEnumerable<TopicDefinition> topics)
    {
        var connection = new Connection(new ProductHeaderValue("CreateDiscussions", "1.0"), arguments.Token);
        foreach (var topic in topics)
        {
            var mutation = new Mutation()
                .CreateDiscussion(new Arg<CreateDiscussionInput>(new CreateDiscussionInput
                {
                    RepositoryId = new ID("R_kgDOEiIB_Q"),
                    Title = topic.Title,
                    Body = $"""
                        # Context
                        Bethesda Path:
                        ``
                        Mutagen Path:
                        ``

                        # Description
                        {topic.MessageFormat}

                        # Effects

                        """,
                    CategoryId = new ID("DIC_kwDOEiIB_c4B_Kgc"),
                }))
                .Select(x => x.Discussion.Id)
                .Compile();

            var discussionId = await connection.Run(mutation, []);

            var addLabel = new Mutation()
                .AddLabelsToLabelable(new Arg<AddLabelsToLabelableInput>(new AddLabelsToLabelableInput
                {
                    LabelableId = discussionId,
                    LabelIds =
                    [
                        new ID(topic.Severity switch
                        {
                            Severity.None => "LA_kwDOEiIB_c8AAAACBIQ8ag",
                            Severity.Suggestion => "MDU6TGFiZWwyNDI3MzQ1NDc4",
                            Severity.Warning => "MDU6TGFiZWwyNDI3MzMyNzYy",
                            Severity.Error => "MDU6TGFiZWwyNDI3MzU0Njg0",
                            Severity.CTD => "MDU6TGFiZWwyNDI3MzMyNzU2",
                            _ => throw new ArgumentOutOfRangeException()
                        })
                    ],
                }))
                .Select(x => x.ClientMutationId)
                .Compile();

            await connection.Run(addLabel, []);
        }
    }
}
