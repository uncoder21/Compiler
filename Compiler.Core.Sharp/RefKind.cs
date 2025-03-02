using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Core.Sharp;

/// <summary>
/// Denotes the kind of reference.
/// </summary>
public enum RefKind : byte
{
    /// <summary>
    /// Indicates a "value" parameter or return type.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates a "ref" parameter or return type.
    /// </summary>
    Ref = 1,

    /// <summary>
    /// Indicates an "out" parameter.
    /// </summary>
    Out = 2,

    /// <summary>
    /// Indicates an "in" parameter.
    /// </summary>
    In = 3,

    /// <summary>
    /// Indicates a "ref readonly" return type.
    /// </summary>
    RefReadOnly = In,

    /// <summary>
    /// Indicates a "ref readonly" parameter.
    /// </summary>
    RefReadOnlyParameter = 4,

    // NOTE: There is an additional value of this enum type - RefKindExtensions.StrictIn == RefKind.RefReadOnlyParameter + 1
    //       It is used internally during lowering. 
    //       Consider that when adding values or changing this enum in some other way.
}

internal static class RefKindExtensions
{
    internal static string ToParameterDisplayString(this RefKind kind)
    {
        return kind switch
        {
            RefKind.Out => "out",
            RefKind.Ref => "ref",
            RefKind.In => "in",
            RefKind.RefReadOnlyParameter => "ref readonly",
            _ => throw new InvalidOperationException(ToArgumentDisplayString(kind)),
        };
    }

    internal static string ToArgumentDisplayString(this RefKind kind)
    {
        return kind switch
        {
            RefKind.Out => "out",
            RefKind.Ref => "ref",
            RefKind.In => "in",
            _ => throw new InvalidOperationException(ToArgumentDisplayString(kind)),
        };
    }

    internal static string ToParameterPrefix(this RefKind kind)
    {
        return kind switch
        {
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            RefKind.In => "in ",
            RefKind.RefReadOnlyParameter => "ref readonly ",
            RefKind.None => string.Empty,
            _ => throw new InvalidOperationException(ToArgumentDisplayString(kind)),
        };
    }

    // Used internally to track `In` arguments that were specified with `In` modifier
    // as opposed to those that were specified with no modifiers and matched `In` parameter.
    // There is at least one kind of analysis that cares about this distinction - hoisting
    // of variables to the frame for async rewriting: a variable that was passed without the
    // `In` modifier may be correctly captured by value or by reference.
    internal const RefKind StrictIn = RefKind.RefReadOnlyParameter + 1;
}