﻿using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SurrealDb.Net.Benchmarks.Models;
using SurrealDb.Net.Internals.Constants;
using SurrealDb.Net.Tests.Fixtures;

namespace SurrealDb.Net.Benchmarks.Remote;

public class ScenarioBench : BaseRemoteBenchmark
{
    private readonly SurrealDbClientGenerator[] _surrealDbClientGenerators =
        new SurrealDbClientGenerator[4];

    private ISurrealDbClient? _surrealdbHttpClient;
    private ISurrealDbClient? _surrealdbHttpClientWithHttpClientFactory;
    private ISurrealDbClient? _surrealdbWsTextClient;
    private ISurrealDbClient? _surrealdbWsBinaryClient;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        for (int index = 0; index < 4; index++)
        {
            var clientGenerator = new SurrealDbClientGenerator();
            var dbInfo = clientGenerator.GenerateDatabaseInfo();

            await CreateEcommerceTables(WsUrl, dbInfo);

            switch (index)
            {
                case 0:
                    _surrealdbHttpClient = new SurrealDbClient(
                        SurrealDbOptions
                            .Create()
                            .WithEndpoint(HttpUrl)
                            .WithNamingPolicy(NamingPolicy)
                            .Build(),
                        appendJsonSerializerContexts: GetFuncJsonSerializerContexts()
                    );
                    InitializeSurrealDbClient(_surrealdbHttpClient, dbInfo);
                    await _surrealdbHttpClient.Connect();
                    break;
                case 1:
                    _surrealdbHttpClientWithHttpClientFactory = clientGenerator.Create(
                        $"Endpoint={HttpUrl}",
                        funcJsonSerializerContexts: GetFuncJsonSerializerContexts()
                    );
                    InitializeSurrealDbClient(_surrealdbHttpClientWithHttpClientFactory, dbInfo);
                    await _surrealdbHttpClientWithHttpClientFactory.Connect();
                    break;
                case 2:
                    _surrealdbWsTextClient = new SurrealDbClient(
                        SurrealDbOptions
                            .Create()
                            .WithEndpoint(WsUrl)
                            .WithNamingPolicy(NamingPolicy)
                            .Build(),
                        appendJsonSerializerContexts: GetFuncJsonSerializerContexts()
                    );
                    InitializeSurrealDbClient(_surrealdbWsTextClient, dbInfo);
                    await _surrealdbWsTextClient.Connect();
                    break;
                case 3:
                    if (JsonSerializer.IsReflectionEnabledByDefault)
                    {
                        _surrealdbWsBinaryClient = new SurrealDbClient(
                            SurrealDbOptions
                                .Create()
                                .WithEndpoint(WsUrl)
                                .WithNamingPolicy(NamingPolicy)
                                .WithSerialization(SerializationConstants.CBOR)
                                .Build()
                        );
                        InitializeSurrealDbClient(_surrealdbWsBinaryClient, dbInfo);
                        await _surrealdbWsBinaryClient.Connect();
                    }
                    break;
            }
        }
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        foreach (var clientGenerator in _surrealDbClientGenerators!)
        {
            if (clientGenerator is not null)
                await clientGenerator.DisposeAsync();
        }

        _surrealdbHttpClient?.Dispose();
        _surrealdbHttpClientWithHttpClientFactory?.Dispose();
        _surrealdbWsTextClient?.Dispose();
        _surrealdbWsBinaryClient?.Dispose();
    }

    [Benchmark]
    public Task<List<ProductAlsoPurchased>> Http()
    {
        return BenchmarkRuns.Scenario(_surrealdbHttpClient!);
    }

    [Benchmark]
    public Task<List<ProductAlsoPurchased>> HttpWithClientFactory()
    {
        return BenchmarkRuns.Scenario(_surrealdbHttpClientWithHttpClientFactory!);
    }

    [Benchmark]
    public Task<List<ProductAlsoPurchased>> WsText()
    {
        return BenchmarkRuns.Scenario(_surrealdbWsTextClient!);
    }

    [Benchmark]
    public Task<List<ProductAlsoPurchased>> WsBinary()
    {
        return BenchmarkRuns.Scenario(_surrealdbWsBinaryClient!);
    }
}