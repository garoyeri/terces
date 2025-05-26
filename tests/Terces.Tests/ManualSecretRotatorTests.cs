using System.Collections.ObjectModel;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace Terces.Tests;

public class ManualSecretRotatorTests
{
    (FakeTimeProvider, ISecretStore, OperationContext) Setup(bool force = false, bool whatIf = false)
    {
        var time = new FakeTimeProvider();
        var store = new InMemorySecretStore(time);
        var context = new OperationContext
        {
            Stores = new Dictionary<string, ISecretStore>() {
                { "test-store", store }},
            Force = force,
            IsWhatIf = whatIf,
            SecretValue1 = "test-secret-value"
        };

        return (time, store, context);
    }
    
    [Fact]
    public async Task SkipsWhenSecretUninitialized()
    {
        var (time, store, context) = Setup();

        var rotator = new ManualSecretRotator(time);
        var resource = new ResourceConfiguration()
        {
            Name = "test-secret",
            StoreName = "test-store",
            ExpirationDays = 90,
            ContentType = "text/plain",
            StrategyType = ManualSecretRotator.StrategyType
        };

        var result = await rotator.RotateAsync(resource, store, context, CancellationToken.None);
        
        result.ShouldNotBeNull();
        result.WasRotated.ShouldBeFalse();
        result.Notes.ShouldContain("not found");
        
        var resultSecret = await store.GetSecretAsync("test-secret", CancellationToken.None);
        resultSecret.ShouldBeNull();
    }

    [Fact]
    public async Task SkipsWhenSecretInitializedAndNotExpired()
    {
        var (time, store, context) = Setup();
        var start = new DateTimeOffset(2025, 3,1,0,0,0, TimeSpan.Zero);
        time.SetUtcNow(start);
        
        // set the initial secret to be expired in 90 days from March 1, 2025
        await store.UpdateSecretAsync("test-secret", "old-value", time.GetUtcNow().AddDays(90), "text/plain", CancellationToken.None);

        // set today to past the expiration date
        time.SetUtcNow(start.AddDays(60));
        
        var rotator = new ManualSecretRotator(time);
        var resource = new ResourceConfiguration()
        {
            Name = "test-secret",
            StoreName = "test-store",
            ExpirationDays = 90,
            ContentType = "text/plain",
            StrategyType = ManualSecretRotator.StrategyType
        };
        
        var result = await rotator.RotateAsync(resource, store, context, CancellationToken.None);

        result.ShouldNotBeNull();
        result.WasRotated.ShouldBeFalse();
        result.Notes.ShouldContain("not due");
        
        var resultSecret = await store.GetSecretAsync("test-secret", CancellationToken.None);
        resultSecret.ShouldNotBeNull();
        resultSecret.Name.ShouldBe("test-secret");
        resultSecret.ContentType.ShouldBe("text/plain");
        resultSecret.ExpiresOn.ShouldBe(start.AddDays(90));
    }

    [Fact]
    public async Task RotatesWhenSecretInitializedAndExpired()
    {
        var (time, store, context) = Setup();
        var start = new DateTimeOffset(2025, 3,1,0,0,0, TimeSpan.Zero);
        time.SetUtcNow(start);
        
        // set the initial secret to be expired in 90 days from March 1, 2025
        await store.UpdateSecretAsync("test-secret", "old-value", time.GetUtcNow().AddDays(90), "text/plain", CancellationToken.None);

        // set today to past the expiration date
        time.SetUtcNow(start.AddDays(91));
        
        var rotator = new ManualSecretRotator(time);
        var resource = new ResourceConfiguration()
        {
            Name = "test-secret",
            StoreName = "test-store",
            ExpirationDays = 90,
            ContentType = "text/plain",
            StrategyType = ManualSecretRotator.StrategyType
        };
        
        var result = await rotator.RotateAsync(resource, store, context, CancellationToken.None);

        result.ShouldNotBeNull();
        result.WasRotated.ShouldBeTrue();
        
        var resultSecret = await store.GetSecretAsync("test-secret", CancellationToken.None);
        resultSecret.ShouldNotBeNull();
        resultSecret.Name.ShouldBe("test-secret");
        resultSecret.ContentType.ShouldBe("text/plain");
        resultSecret.ExpiresOn.ShouldBe(start.AddDays(91 + 90));
    }

    [Fact]
    public async Task RotatesWhenSecretInitializedAndNotExpiredButOverlapping()
    {
        var (time, store, context) = Setup();
        var start = new DateTimeOffset(2025, 3,1,0,0,0, TimeSpan.Zero);
        time.SetUtcNow(start);
        
        // set the initial secret to be expired in 90 days from March 1, 2025
        await store.UpdateSecretAsync("test-secret", "old-value", time.GetUtcNow().AddDays(90), "text/plain", CancellationToken.None);

        // set today to past the expiration date
        time.SetUtcNow(start.AddDays(61));
        
        var rotator = new ManualSecretRotator(time);
        var resource = new ResourceConfiguration()
        {
            Name = "test-secret",
            StoreName = "test-store",
            ExpirationDays = 90,
            ExpirationOverlapDays = 30,
            ContentType = "text/plain",
            StrategyType = ManualSecretRotator.StrategyType
        };
        
        var result = await rotator.RotateAsync(resource, store, context, CancellationToken.None);

        result.ShouldNotBeNull();
        result.WasRotated.ShouldBeTrue();
        
        var resultSecret = await store.GetSecretAsync("test-secret", CancellationToken.None);
        resultSecret.ShouldNotBeNull();
        resultSecret.Name.ShouldBe("test-secret");
        resultSecret.ContentType.ShouldBe("text/plain");
        resultSecret.ExpiresOn.ShouldBe(start.AddDays(61 + 90));
    }
}