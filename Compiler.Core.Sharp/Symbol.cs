using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Core.Sharp;

internal enum SymbolKind
{
    ArrayType,
    PointerType,
    FunctionPointerType,
    ErrorType,
    TypeParameter,
    Discard,
    Method,
    Field,
    Property,
    Event,
    Parameter,
    Local,
}

internal abstract class Symbol
{
    public abstract SymbolKind Kind { get; }
    public virtual string Name => string.Empty;
}

internal abstract class TypeSymbol : Symbol // NamespaceOrTypeSymbol
{

}

internal abstract class NamedTypeSymbol : TypeSymbol
{
    
}

internal abstract class ArrayTypeSymbol : TypeSymbol
{
    public sealed override SymbolKind Kind => SymbolKind.ArrayType;
    public abstract TypeSymbol ElementType { get; }
}

internal abstract class PointerTypeSymbol : TypeSymbol
{
    public sealed override SymbolKind Kind => SymbolKind.PointerType;
    public abstract TypeSymbol PointedAtType { get; }
}

internal abstract class FunctionPointerTypeSymbol : TypeSymbol
{
    public sealed override SymbolKind Kind => SymbolKind.FunctionPointerType;
    public abstract TypeSymbol ReturnType { get; }
    public abstract ImmutableArray<ParameterSymbol> Parameters { get; }
}

internal abstract class ErrorTypeSymbol : TypeSymbol
{
    public sealed override SymbolKind Kind => SymbolKind.ErrorType;
}

internal abstract class TypeParameterSymbol : TypeSymbol
{
    public sealed override SymbolKind Kind => SymbolKind.TypeParameter;
    public abstract ImmutableArray<TypeSymbol> ConstraintTypes { get; }
}

// DynamicTypeSymbol not used in the current implementation

internal abstract class DiscardSymbol : Symbol
{
    public sealed override SymbolKind Kind => SymbolKind.Discard;
}

internal abstract class MethodSymbol : Symbol
{
    public sealed override SymbolKind Kind => SymbolKind.Method;
    public abstract TypeSymbol ReturnType { get; }
    public abstract ImmutableArray<ParameterSymbol> Parameters { get; }
}

internal abstract class FieldSymbol : Symbol
{
    public sealed override SymbolKind Kind => SymbolKind.Field;
    public abstract TypeSymbol Type { get; }
}

internal abstract class PropertySymbol : Symbol
{
    public sealed override SymbolKind Kind => SymbolKind.Property;
    public abstract TypeSymbol Type { get; }
}

internal abstract class EventSymbol : Symbol
{
    public sealed override SymbolKind Kind => SymbolKind.Event;
    public abstract TypeSymbol Type { get; }
}

internal abstract class ParameterSymbol : Symbol
{
    public sealed override SymbolKind Kind => SymbolKind.Parameter;
    public abstract TypeSymbol Type { get; }
}

internal abstract class LocalSymbol : Symbol
{
    public sealed override SymbolKind Kind => SymbolKind.Local;
    public abstract TypeSymbol Type { get; }
}
