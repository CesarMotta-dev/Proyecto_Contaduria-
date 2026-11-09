using System.Reflection;
using AccountingLab.Application.Services;
using AccountingLab.Domain.Model;
using AccountingLab.Infrastructure.Persistence;
using Xunit;

namespace AccountingLab.ArchitectureTests;

public sealed class DependencyRulesTests
{
    private static readonly Assembly Domain = typeof(MoneyRules).Assembly;
    private static readonly Assembly Application = typeof(CatalogService).Assembly;
    private static readonly Assembly Infrastructure = typeof(AccountingDbContext).Assembly;

    private static readonly string[] ForbiddenDomainPrefixes =
    {
        "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "Pomelo",
        "MySql", "Npgsql", "Dapper", "AccountingLab.Application",
        "AccountingLab.Infrastructure", "AccountingLab.Api"
    };

    private static string[] ProjectReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(item => item.Name!)
            .Where(name => name.StartsWith("AccountingLab.", StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

    private static bool HasPrefix(string? name, IEnumerable<string> prefixes) =>
        prefixes.Any(prefix => name?.StartsWith(prefix, StringComparison.Ordinal) == true);

    private static bool ForbiddenDomainType(Type type)
    {
        if (type.HasElementType) return ForbiddenDomainType(type.GetElementType()!);
        if (HasPrefix(type.Assembly.GetName().Name, ForbiddenDomainPrefixes) ||
            HasPrefix(type.Namespace, ForbiddenDomainPrefixes)) return true;
        return type.IsGenericType && type.GetGenericArguments().Any(ForbiddenDomainType);
    }

    private static IEnumerable<Type> ContractTypes(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        if (type.BaseType is not null) yield return type.BaseType;
        foreach (var item in type.GetInterfaces()) yield return item;
        foreach (var property in type.GetProperties(flags)) yield return property.PropertyType;
        foreach (var field in type.GetFields(flags)) yield return field.FieldType;
        foreach (var method in type.GetMethods(flags))
        {
            yield return method.ReturnType;
            foreach (var parameter in method.GetParameters()) yield return parameter.ParameterType;
        }
        foreach (var constructor in type.GetConstructors(flags))
            foreach (var parameter in constructor.GetParameters()) yield return parameter.ParameterType;
    }

    [Fact]
    public void Domain_assembly_has_no_reference_to_outer_layers_or_database_and_web_frameworks()
    {
        var references = Domain.GetReferencedAssemblies();
        Assert.NotEmpty(references);
        Assert.DoesNotContain(references, item => HasPrefix(item.Name, ForbiddenDomainPrefixes));
        Assert.Empty(ProjectReferences(Domain));
    }

    [Fact]
    public void Application_depends_on_domain_as_its_only_project_layer()
    {
        Assert.Equal(new[] { "AccountingLab.Domain" }, ProjectReferences(Application));
        Assert.DoesNotContain(Application.GetReferencedAssemblies(), item =>
            HasPrefix(item.Name, new[] { "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "Pomelo", "MySql", "Npgsql" }));
    }

    [Fact]
    public void Infrastructure_references_inner_contracts_and_does_not_depend_on_api()
    {
        Assert.Equal(new[] { "AccountingLab.Application", "AccountingLab.Domain" }, ProjectReferences(Infrastructure));
        Assert.DoesNotContain(Infrastructure.GetReferencedAssemblies(), item =>
            item.Name?.StartsWith("AccountingLab.Api", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Domain_member_signatures_do_not_leak_framework_or_outer_layer_types()
    {
        var types = Domain.GetTypes();
        Assert.NotEmpty(types);
        foreach (var type in types)
            Assert.DoesNotContain(ContractTypes(type), ForbiddenDomainType);
    }

    [Fact]
    public void Domain_entities_do_not_inherit_database_or_web_framework_base_classes()
    {
        var entities = Domain.GetTypes().Where(type =>
            type.IsClass && type.Namespace == "AccountingLab.Domain.Model").ToArray();
        Assert.NotEmpty(entities);
        foreach (var entity in entities)
        {
            var current = entity.BaseType;
            while (current is not null)
            {
                Assert.False(ForbiddenDomainType(current), entity.FullName + " inherits " + current.FullName);
                current = current.BaseType;
            }
        }
    }

    [Fact]
    public void Monetary_domain_properties_do_not_use_binary_floating_point()
    {
        var properties = Domain.GetTypes()
            .Where(type => type.Namespace == "AccountingLab.Domain.Model")
            .SelectMany(type => type.GetProperties())
            .Where(property => new[] { "Amount", "Debit", "Credit", "Rate", "Balance" }
                .Any(term => property.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        Assert.NotEmpty(properties);
        Assert.DoesNotContain(properties, property =>
            property.PropertyType == typeof(float) || property.PropertyType == typeof(double) ||
            property.PropertyType == typeof(float?) || property.PropertyType == typeof(double?));
        Assert.Contains(properties, property => property.Name == "Amount" && property.PropertyType == typeof(decimal));
    }
}

