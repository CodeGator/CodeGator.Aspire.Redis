using System.Collections;
using System.Reflection;
using Aspire.Hosting;

namespace CodeGator.Aspire.Redis.UnitTests;

[TestClass]
public sealed class RedisResourceBuilderExtensionsTests
{
    [TestMethod]
    public void WithClearCommand_registers_clear_cache_command()
    {
        var app = DistributedApplication.CreateBuilder();
        var redis = app.AddRedis("redis");

        redis.WithClearCommand();

        var annotations = GetAnnotations(redis.Resource).ToArray();
        var commandAnnotation = annotations.FirstOrDefault(a => HasName(a, "clear-cache"));

        Assert.IsNotNull(commandAnnotation);
        var name = GetOptionalMemberValue<string>(commandAnnotation, "Name", out var nameFound);
        Assert.IsTrue(nameFound);
        Assert.AreEqual("clear-cache", name);

        var displayName = GetOptionalMemberValue<string>(commandAnnotation, "DisplayName", out var displayNameFound);
        if (displayNameFound)
        {
            Assert.AreEqual("Clear Cache", displayName);
        }
    }

    private static IEnumerable<object?> GetAnnotations(object resource)
    {
        var annotationsProp = resource.GetType().GetProperty("Annotations", BindingFlags.Instance | BindingFlags.Public);
        var annotationsValue = annotationsProp?.GetValue(resource);

        if (annotationsValue is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return item;
            }

            yield break;
        }

        Assert.Fail("Unable to locate resource annotations for command verification.");
    }

    private static bool HasName(object? annotation, string expectedName)
    {
        if (annotation is null)
        {
            return false;
        }

        var name = GetOptionalMemberValue<string?>(annotation, "Name", out var foundName);
        if (!foundName)
        {
            return false;
        }

        return string.Equals(name, expectedName, StringComparison.Ordinal);
    }

    private static T? GetOptionalMemberValue<T>(object instance, string memberName, out bool found)
    {
        found = false;
        var type = instance.GetType();

        var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public);
        if (prop is not null && prop.GetIndexParameters().Length == 0)
        {
            found = true;
            return (T?)prop.GetValue(instance);
        }

        var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public);
        if (field is not null)
        {
            found = true;
            return (T?)field.GetValue(instance);
        }

        return default;
    }
}

