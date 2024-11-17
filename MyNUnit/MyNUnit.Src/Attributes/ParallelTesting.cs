namespace MyNUnit.Attributes;

/// <summary>
/// Attribute, that if attached to class, tells MyNUnit to perform all tests in parallel
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class ParallelTesting : Attribute;