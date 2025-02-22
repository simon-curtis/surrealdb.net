﻿using SurrealDb.Net.Exceptions;
using SurrealDb.Net.Models.Response;

namespace SurrealDb.Net.LiveQuery.Tests;

public class KillTests
{
    [Fact]
    public async Task ShouldNotBeSupportedOnHttpProtocol()
    {
        const string url = "http://127.0.0.1:8000";

        Func<Task> func = async () =>
        {
            await using var surrealDbClientGenerator = new SurrealDbClientGenerator();
            var dbInfo = surrealDbClientGenerator.GenerateDatabaseInfo();

            using var client = surrealDbClientGenerator.Create(url);
            await client.SignIn(new RootAuth { Username = "root", Password = "root" });
            await client.Use(dbInfo.Namespace, dbInfo.Database);

            var liveQueryUuid = Guid.NewGuid();

            await client.Kill(liveQueryUuid);
        };

        await func.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task ShouldKillActiveLiveQueryOnWsProtocol()
    {
        const string url = "ws://127.0.0.1:8000/rpc";

        Func<Task> func = async () =>
        {
            await using var surrealDbClientGenerator = new SurrealDbClientGenerator();
            var dbInfo = surrealDbClientGenerator.GenerateDatabaseInfo();

            using var client = surrealDbClientGenerator.Create(url);
            await client.SignIn(new RootAuth { Username = "root", Password = "root" });
            await client.Use(dbInfo.Namespace, dbInfo.Database);

            var response = await client.Query("LIVE SELECT * FROM test;");

            if (response.FirstResult is not SurrealDbOkResult okResult)
                throw new Exception("Expected a SurrealDbOkResult");

            var liveQueryUuid = okResult.GetValue<Guid>();

            await client.Kill(liveQueryUuid);
        };

        await func.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldFailToKillInexistantLiveQueryOnWsProtocol()
    {
        const string url = "ws://127.0.0.1:8000/rpc";

        Func<Task> func = async () =>
        {
            await using var surrealDbClientGenerator = new SurrealDbClientGenerator();
            var dbInfo = surrealDbClientGenerator.GenerateDatabaseInfo();

            using var client = surrealDbClientGenerator.Create(url);
            await client.SignIn(new RootAuth { Username = "root", Password = "root" });
            await client.Use(dbInfo.Namespace, dbInfo.Database);

            var liveQueryUuid = Guid.NewGuid();

            await client.Kill(liveQueryUuid);
        };

        await func.Should()
            .ThrowAsync<SurrealDbException>()
            .WithMessage(
                "There was a problem with the database: Can not execute KILL statement using id '$id'"
            );
    }
}
