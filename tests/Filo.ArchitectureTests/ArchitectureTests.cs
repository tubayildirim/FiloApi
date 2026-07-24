using Xunit;
using NetArchTest.Rules;
using System.Reflection;
using Filo.Domain.Common;
using Filo.Application.Common.Settings;
using Filo.Infrastructure.Persistence;

namespace Filo.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CacheSettings).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(AppDbContext).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    private const string DomainNamespace = "Filo.Domain";
    private const string ApplicationNamespace = "Filo.Application";
    private const string InfrastructureNamespace = "Filo.Infrastructure";
    private const string ApiNamespace = "Filo.Api";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnOtherProjects()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer should not depend on Application, Infrastructure, or Api layers.");
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer should not depend on Infrastructure or Api layers.");
    }

    [Fact]
    public void Infrastructure_Should_Not_HaveDependencyOnApi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Infrastructure layer should not depend on Api layer.");
    }

    [Fact]
    public void Commands_And_Queries_Should_Be_Sealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Command")
            .Or()
            .HaveNameEndingWith("Query")
            .Should()
            .BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, "All Command and Query classes in Application should be sealed.");
    }

    [Fact]
    public void Handlers_Should_Have_DependencyOnDomain()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .Or()
            .HaveNameEndingWith("QueryHandler")
            .Should()
            .HaveDependencyOn(DomainNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "All CQRS Handlers should have dependency on Domain layer.");
    }

    [Fact]
    public void Repositories_In_Infrastructure_Should_EndWithRepository()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ResideInNamespace("Filo.Infrastructure.Persistence.Repositories")
            .And()
            .DoNotHaveName("GenericRepository`1") // GenericRepository uses generic parameter backtick syntax
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        Assert.True(result.IsSuccessful, "All repositories in Infrastructure should end with 'Repository'.");
    }
}
