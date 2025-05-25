using System.Collections.ObjectModel;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace Terces.Tests;

public class ManualSecretRotatorTests
{
    [Fact]
    public async Task SkipsWhenSecretUninitialized()
    {
        var time = new FakeTimeProvider();
        var store = new InMemorySecretStore(time);
        
        var rotator = new ManualSecretRotator(time);
        var resource = new ResourceConfiguration()
        {
            Name = "test-secret",
            StoreName = "test-store",
            ExpirationDays = 90,
            ContentType = "text/plain",
            StrategyType = ManualSecretRotator.StrategyType
        };
        var context = new OperationContext
        {
            Stores = new Dictionary<string, ISecretStore>() {
            { "test-store", store }},
            Force = false,
            IsWhatIf = false,
            SecretValue1 = "test-secret-value"
        };

        var result = await rotator.RotateAsync(resource, store, context, CancellationToken.None);
        
        result.ShouldNotBeNull();
        result.WasRotated.ShouldBeFalse();
        result.Notes.ShouldContain("not found");
    }
}