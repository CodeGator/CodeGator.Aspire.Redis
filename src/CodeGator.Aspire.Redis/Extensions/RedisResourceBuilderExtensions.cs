
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Aspire.Hosting;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// This class contains extension methods related to the <see cref="IResourceBuilder{RedisResource}"/>
/// type.
/// </summary>
public static partial class RedisResourceBuilderExtensions
{
    /// <summary>
    /// This method adds an <c>Aspire</c> command to clear the <c>Redis</c> cache.
    /// </summary>
    /// <param name="builder">The builder to use for the operation.</param>
    /// <returns>The value of the <paramref name="builder"/> parameter, for chaining
    /// method calls together, fluent style.</returns>
    public static IResourceBuilder<RedisResource> WithClearCommand(
        this IResourceBuilder<RedisResource> builder
        )
    {
        builder.WithCommand(
            name: "clear-cache",
            displayName: "Clear Cache",
            executeCommand: context => OnRunClearCacheCommandAsync(builder, context),
            commandOptions: new CommandOptions
            {
                IconName = "AnimalRabbitOff",
                IconVariant = IconVariant.Filled,
                UpdateState = OnUpdateResourceState,
                ConfirmationMessage = "Are you sure you want to clear the cache?",
                Description = "This command will clear all cached data in the Redis database.",
            }
        );

        return builder;
    }


    private static async Task<ExecuteCommandResult> OnRunClearCacheCommandAsync(
        IResourceBuilder<RedisResource> builder,
        ExecuteCommandContext context
        )
    {
        var connectionString = await builder.Resource.GetConnectionStringAsync() ??
            throw new InvalidOperationException(
                $"Unable to get the '{context.ResourceName}' connection string.");

        await using var connection = ConnectionMultiplexer.Connect(connectionString);
        var database = connection.GetDatabase();
        await database.ExecuteAsync("FLUSHALL");

        return CommandResults.Success();
    }


    private static ResourceCommandState OnUpdateResourceState(
        UpdateCommandStateContext context
        )
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<RedisResource>>();

        ZLogUpdatingResourceState(logger, context.ResourceSnapshot);

        return context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
            ? ResourceCommandState.Enabled
            : ResourceCommandState.Disabled;
    }


    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Debug,
        Message = "Updating resource state: {ResourceSnapshot}")]
    private static partial void ZLogUpdatingResourceState(
        ILogger logger,
        object? resourceSnapshot
        );
}
