using CommandLine;

namespace Terces.Console;

using Console = System.Console;

[Verb("init", HelpText = "Initialize the secret for a resource.")]
public class InitializeOptions
{
    [Option('f', "force", Required = false, HelpText = "Force initialization of the secret.")]
    public bool Force { get; set; }
    
    [Option("whatif", Required = false, HelpText = "Simulate initialization of the secret.")]
    public bool WhatIf { get; set; }
}

public class InitializeCommand
{
    private readonly OperationContext _context;
    private readonly IReadOnlyList<ResourceConfiguration> _resources;

    public InitializeCommand(OperationContext context, IReadOnlyList<ResourceConfiguration> resources)
    {
        _context = context;
        _resources = resources;
    }

    public async Task<int> Execute(InitializeOptions options)
    {
        _context.IsWhatIf = options.WhatIf;
        _context.Force = options.Force;
        
        foreach (var resource in _resources)
        {
            if (!_context.Rotators.TryGetValue(resource.StrategyType, out var rotator))
            {
                Console.WriteLine($"Failed to find rotator for secret: ${resource.Name}");
                continue;
            }

            var result = await rotator.InitializeAsync(resource, _context.Stores[resource.StoreName]!, _context,
                CancellationToken.None);
            Console.WriteLine(result.Notes);
        }
        
        return 0;
    }
}