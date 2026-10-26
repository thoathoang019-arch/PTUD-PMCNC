using Demo.Domain.Entities;

namespace Demo.Tests.Architecture;

public class DependencyRuleTests
{
    [Fact]
    public void Domain_DoesNotReferenceFrameworkOrOuterLayers()
    {
        var references = typeof(Product).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", references);
        Assert.DoesNotContain("MediatR", references);
        Assert.DoesNotContain("Demo.Infrastructure", references);
        Assert.DoesNotContain("Demo.API", references);
    }
}
