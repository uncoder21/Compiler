using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Core.Sharp;

// from the root node of the syntax tree,
// we can traverse the tree to get the declarations,
// and from the declarations, we can get the symbols
// for namespaces and types, then we can get the symbols
// for the members of the types or nested types.
//
// the symbols are then transformed into bound symbols
// which are used to create a flow graph for the code
// and then the flow graph is used to generate the
// intermediate language code.
//
// from the IL code, we can generate the machine code
// for the target platform.
//
// knowing that the IL is the "last" item in the chain,
// all the instances can be required from the IL generator
// and they do the work of building themselves.
//
// which strategy should be used to build the IL code?
//
// the IL code can be built using a visitor pattern
// where the visitor is the IL generator and the nodes
// are the bound symbols.
//
// the bound symbols can be transformed into IL nodes
// and the IL nodes can be visited by the IL generator
// to generate the IL code.
//

internal partial class BoundVisitor
{
    public void Visit(BoundNode node)
    {

    }

    public void DefaultVisit(BoundNode node)
    {

    }
}

internal abstract class BoundNode
{
    public abstract BoundKind Kind { get; }

    public abstract void Accept(BoundVisitor visitor);
}

internal enum BoundKind : byte
{
    None = 0x00,
    BoundNamespace = 0x01,
    BoundType = 0x02,
    BoundField = 0x03,
    BoundProperty = 0x04,
    BoundMethod = 0x05,
    BoundParameter = 0x06,
    BoundLocal = 0x07,
    BoundStatement = 0x08,
    BoundExpression = 0x09,
    BoundUnaryExpression = 0x0A,
    BoundBinaryExpression = 0x0B,
    BoundBlock = 0x0C,
    BoundAssignmentExpression = 0xD,
}

[BoundPattern]
internal class BoundNamespace : BoundNode
{
    public override BoundKind Kind => BoundKind.BoundNamespace;

    public override void Accept(BoundVisitor visitor)
    {
        visitor.VisitBoundNamespace(this);
    }
}

internal class BoundStatement
{

}

internal abstract class BoundExpression : BoundNode
{
    public abstract TypeKind ResultingType { get; }
}

internal class BoundConversionExpression
{
    public TypeKind TargetType { get; }
    public BoundExpression Operand { get; }
}


internal class BoundUnaryExpression : BoundExpression
{
    public override BoundKind Kind => BoundKind.BoundUnaryExpression;

    public UnaryOperatorKind OperatorKind { get; }
    public BoundExpression Operand { get; }

    public override TypeKind ResultingType { get; }


}

internal enum UnaryOperatorKind : byte
{
    Identity,

    LogicalNegation,
    BitwiseNegation,

    PreIncrement,
    PreDecrement,

    PostIncrement,
    PostDecrement,

    Conversion,
}

internal static class UnaryOperatorKindExtensions
{
    public static bool IsIncrement(this UnaryOperatorKind kind)
    {
        return kind switch
        {
            UnaryOperatorKind.PreIncrement => true,
            UnaryOperatorKind.PostIncrement => true,
            _ => false,
        };
    }
    public static bool IsDecrement(this UnaryOperatorKind kind)
    {
        return kind switch
        {
            UnaryOperatorKind.PreDecrement => true,
            UnaryOperatorKind.PostDecrement => true,
            _ => false,
        };
    }
    public static bool IsLogical(this UnaryOperatorKind kind)
    {
        return kind switch
        {
            UnaryOperatorKind.LogicalNegation => true,
            _ => false,
        };
    }
    public static bool IsBitwise(this UnaryOperatorKind kind)
    {
        return kind switch
        {
            UnaryOperatorKind.BitwiseNegation => true,
            _ => false,
        };
    }
}

internal class BoundBinaryExpression : BoundExpression
{
    public BinaryOperatorKind OperatorKind { get; }
    public BoundExpression Left { get; }
    public BoundExpression Right { get; }

    public TypeKind ResultingType { get; }
}

internal enum BinaryOperatorKind
{
    None = 0,

    Addition,  // +
    Subtraction,  // -
    Multiplication, // *
    Division,  // /
    Modulus,  // %

    LogicalAnd,  // &&
    LogicalOr,  // ||

    BitwiseAnd,  // &
    BitwiseOr,  // |

    ShiftLeft,  // <<
    ShiftRight, // >>

    Comparison,  // <, >, ==, !=, <=, >=
}

internal static class BinaryOperatorKindExtensions
{
    public static bool IsComparison(this BinaryOperatorKind kind)
    {
        return kind switch
        {
            BinaryOperatorKind.Equality => true,
            BinaryOperatorKind.Inequality => true,
            BinaryOperatorKind.LessThan => true,
            BinaryOperatorKind.LessThanOrEqual => true,
            BinaryOperatorKind.GreaterThan => true,
            BinaryOperatorKind.GreaterThanOrEqual => true,
            _ => false,
        };
    }
    public static bool IsLogical(this BinaryOperatorKind kind)
    {
        return kind switch
        {
            BinaryOperatorKind.LogicalAnd => true,
            BinaryOperatorKind.LogicalOr => true,
            _ => false,
        };
    }
    public static bool IsBitwise(this BinaryOperatorKind kind)
    {
        return kind switch
        {
            BinaryOperatorKind.BitwiseAnd => true,
            BinaryOperatorKind.BitwiseOr => true,
            BinaryOperatorKind.BitwiseXor => true,
            _ => false,
        };
    }
    public static bool IsArithmetic(this BinaryOperatorKind kind)
    {
        return kind switch
        {
            BinaryOperatorKind.Addition => true,
            BinaryOperatorKind.Subtraction => true,
            BinaryOperatorKind.Multiplication => true,
            BinaryOperatorKind.Division => true,
            BinaryOperatorKind.Remainder => true,
            _ => false,
        };
    }
    public static bool IsShift(this BinaryOperatorKind kind)
    {
        return kind switch
        {
            BinaryOperatorKind.LeftShift => true,
            BinaryOperatorKind.RightShift => true,
            _ => false,
        };
    }
}

internal class BoundAssignmentExpression : BoundExpression
{
    public override BoundKind Kind => BoundKind.BoundAssignmentExpression;

    public BoundExpression Target { get; }
    public BoundExpression Value { get; }

    public override TypeKind ResultingType => Target.ResultingType;
}

internal abstract class BoundVariable : BoundNode
{
    public abstract TypeKind Type { get; }
}

internal class BoundParameter : BoundVariable
{
    public override BoundKind Kind => BoundKind.BoundParameter;
    public override TypeKind Type { get; }
    public bool IsRef { get; }
    public bool IsOut { get; }
    public bool IsIn { get; }
}

[Flags]
internal enum TypeKind : ushort
{
    None = 0,

    Bool,

    Int8,
    Int16,
    Int32,
    Int64,

    UInt64,
    UInt32,
    UInt8,
    UInt16,

    Float32,
    Float64,

    Array,
    Reference,
    Object,
    String,

    Char = UInt16,
}

internal static class TypeKindExtensions
{

    public static bool IsBooleanType(this TypeKind type)
    {
        return type == TypeKind.Bool;
    }

    public static bool IsSignedType(this TypeKind type)
    {
        return type switch
        {
            TypeKind.Int8 => true,
            TypeKind.Int16 => true,
            TypeKind.Int32 => true,
            TypeKind.Int64 => true,
            _ => false,
        };
    }

    public static bool IsUnsignedType(this TypeKind type)
    {
        return type switch
        {
            TypeKind.UInt8 => true,
            TypeKind.UInt16 => true,
            TypeKind.UInt32 => true,
            TypeKind.UInt64 => true,
            _ => false,
        };
    }

    public static bool IsFloatingPointType(this TypeKind type)
    {
        return type switch
        {
            TypeKind.Float32 => true,
            TypeKind.Float64 => true,
            _ => false,
        };
    }

    public static bool IsIntegralType(this TypeKind type)
    {
        return IsSignedType(type) || IsUnsignedType(type);
    }

    public static bool IsPointerType(this TypeKind type)
    {
        return type switch
        {
            TypeKind.Reference => true,
            TypeKind.Object => true,
            TypeKind.String => true,
            _ => false,
        };
    }

    public static bool IsNumericType(this TypeKind type)
    {
        return IsIntegralType(type) || IsFloatingPointType(type);
    }

    public static uint GetSize(this TypeKind type)
    {
        return type switch
        {
            TypeKind.Bool => 1,

            TypeKind.Int8 => 1,
            TypeKind.Int16 => 2,
            TypeKind.Int32 => 4,
            TypeKind.Int64 => 8,

            TypeKind.UInt8 => 1,
            TypeKind.UInt16 => 2,
            TypeKind.UInt32 => 4,
            TypeKind.UInt64 => 8,

            TypeKind.Float32 => 4,
            TypeKind.Float64 => 8,

            TypeKind.Array => 4,
            TypeKind.Reference => 4,
            TypeKind.Object => 4,
            TypeKind.String => 4,
            _ => 0,
        };
    }

    public static uint EncodeOperationKey(BinaryOperatorKind operatorKind, TypeKind left, TypeKind right)
    {
        return ((uint)operatorKind << 26) | ((uint)left << 13) | (uint)right;
    }

    public static uint EncodeOperationKey(UnaryOperatorKind operatorKind, TypeKind operand)
    {
        return ((uint)operatorKind << 26) | ((uint)operand << 13);
    }

    public static TypeKind GetResultingType(BinaryOperatorKind operatorKind, TypeKind left, TypeKind right)
    {
        // floating point | integral -> floating point
        // integral | floating point -> floating point
        // signed | unsigned -> explicit conversion needed
        // unsigned | signed -> explicit conversion needed
        // signed | signed -> signed
        // unsigned | unsigned -> unsigned
        // same type -> same type
        // pointer | integral -> pointer
        // integral | pointer -> pointer

        if (IsIntegralType(left) && IsFloatingPointType(right))
        {
            return right;
        }

        if (IsFloatingPointType(left) && IsIntegralType(right))
        {
            return left;
        }
        
        if (IsSignedType(left) && IsSignedType(right))
        {
            return left > right ? left : right;
        }

        if (IsUnsignedType(left) && IsUnsignedType(right))
        {
            return left > right ? left : right;
        }

        if (left == right)
        {
            return left;
        }

        if (IsPointerType(left) && IsIntegralType(right))
        {
            return left;
        }

        if (IsIntegralType(left) && IsPointerType(right))
        {
            return right;
        }

        return left;
    }

    public static bool NeedsImplicitlyConversion(TypeKind from, TypeKind to)
    {
        if (from == to) return true;

        return (from, to) switch
        {
            // Integer widening conversions
            (TypeKind.Int8, TypeKind.Int16) => true,
            (TypeKind.Int8, TypeKind.Int32) => true,
            (TypeKind.Int8, TypeKind.Int64) => true,

            (TypeKind.Int16, TypeKind.Int32) => true,
            (TypeKind.Int16, TypeKind.Int64) => true,

            (TypeKind.Int32, TypeKind.Int64) => true,

            (TypeKind.UInt8, TypeKind.UInt16) => true,
            (TypeKind.UInt8, TypeKind.UInt32) => true,
            (TypeKind.UInt8, TypeKind.UInt64) => true,

            (TypeKind.UInt16, TypeKind.UInt32) => true,
            (TypeKind.UInt16, TypeKind.UInt64) => true,

            (TypeKind.UInt32, TypeKind.UInt64) => true,

            // Float widening conversions
            (TypeKind.Int32, TypeKind.Float32) => true,
            (TypeKind.Int32, TypeKind.Float64) => true,

            (TypeKind.Int64, TypeKind.Float64) => true,

            (TypeKind.UInt32, TypeKind.Float32) => true,
            (TypeKind.UInt32, TypeKind.Float64) => true,

            (TypeKind.UInt64, TypeKind.Float64) => true,

            (TypeKind.Float32, TypeKind.Float64) => true,

            // Implicit integer to pointer conversion
            (TypeKind.Int32, TypeKind.Reference) => true,
            (TypeKind.UInt32, TypeKind.Reference) => true,

            _ => false
        };
    }

    public static ILOpcode GetImplicitConversionOpcode(TypeKind from, TypeKind to)
    {
        return (from, to) switch
        {
            (TypeKind.Int8, TypeKind.Int16) => ILOpcode.Conv_I2,
            (TypeKind.Int8, TypeKind.Int32) => ILOpcode.Conv_I4,
            (TypeKind.Int8, TypeKind.Int64) => ILOpcode.Conv_I8,

            (TypeKind.Int16, TypeKind.Int32) => ILOpcode.Conv_I4,
            (TypeKind.Int16, TypeKind.Int64) => ILOpcode.Conv_I8,

            (TypeKind.Int32, TypeKind.Int64) => ILOpcode.Conv_I8,

            (TypeKind.UInt8, TypeKind.UInt16) => ILOpcode.Conv_U2,
            (TypeKind.UInt8, TypeKind.UInt32) => ILOpcode.Conv_U4,
            (TypeKind.UInt8, TypeKind.UInt64) => ILOpcode.Conv_U8,

            (TypeKind.UInt16, TypeKind.UInt32) => ILOpcode.Conv_U4,
            (TypeKind.UInt16, TypeKind.UInt64) => ILOpcode.Conv_U8,

            (TypeKind.UInt32, TypeKind.UInt64) => ILOpcode.Conv_U8,

            (TypeKind.Int32, TypeKind.Float32) => ILOpcode.Conv_R4,
            (TypeKind.Int32, TypeKind.Float64) => ILOpcode.Conv_R8,

            (TypeKind.Int64, TypeKind.Float64) => ILOpcode.Conv_R8,

            (TypeKind.UInt32, TypeKind.Float32) => ILOpcode.Conv_R4,
            (TypeKind.UInt32, TypeKind.Float64) => ILOpcode.Conv_R8,

            (TypeKind.UInt64, TypeKind.Float64) => ILOpcode.Conv_R8,

            (TypeKind.Float32, TypeKind.Float64) => ILOpcode.Conv_R8,

            // Integer to pointer conversion
            (TypeKind.Int32, TypeKind.Reference) => ILOpcode.Conv_U,
            (TypeKind.UInt32, TypeKind.Reference) => ILOpcode.Conv_U,

            _ => throw new InvalidOperationException($"No implicit conversion opcode found for {from} to {to}")
        };
    }

    public static bool TryGetConversionOpcode(TypeKind from, TypeKind to, out ILOpcode? opcode)
    {
        // type    -> stack representation
        //
        // int8    -> int32
        // int16   -> int32
        // int32   -> int32
        // int64   -> int64
        //
        // uint8   -> int32
        // uint16  -> int32
        // uint32  -> int32
        // uint64  -> int64
        //
        // float32 -> float32
        // float64 -> float64
        //
        // conversions are needed for:
        //
        // int8    -> float32
        // int8    -> float64
        // int16   -> float32
        // int16   -> float64
        // int32   -> float32
        // int32   -> float64
        // int64   -> float32
        // int64   -> float64
        //
        // uint8   -> float32
        // uint8   -> float64
        // uint16  -> float32
        // uint16  -> float64
        // uint32  -> float32
        // uint32  -> float64
        // uint64  -> float32
        // uint64  -> float64
        //
        // also needs conversions for types that are smaller on size
        //
        // int16   -> int8
        // int32   -> int8
        // int32   -> int16
        // int64   -> int8
        // int64   -> int16
        // int64   -> int32
        //
        // uint16  -> uint8
        // uint32  -> uint8
        // uint32  -> uint16
        // uint64  -> uint8
        // uint64  -> uint16
        // uint64  -> uint32
        //
        // float32 -> int8
        // float32 -> int16
        // float32 -> int32
        // float32 -> int64
        // float32 -> uint8
        // float32 -> uint16
        // float32 -> uint32
        // float32 -> uint64
        //
        // float64 -> int8
        // float64 -> int16
        // float64 -> int32
        // float64 -> int64
        // float64 -> uint8
        // float64 -> uint16
        // float64 -> uint32
        // float64 -> uint64
        //
        // float64 -> float32
        //
        // also signed to unsigned and unsigned to signed
        //
        // int8    -> uint8
        // int8    -> uint16
        // int8    -> uint32
        // int8    -> uint64
        // int16   -> uint8
        // int16   -> uint16
        // int16   -> uint32
        // int16   -> uint64
        // int32   -> uint8
        // int32   -> uint16
        // int32   -> uint32
        // int32   -> uint64
        // int64   -> uint8
        // int64   -> uint16
        // int64   -> uint32
        // int64   -> uint64
        //
        // uint8   -> int8
        // uint8   -> int16
        // uint8   -> int32
        // uint8   -> int64
        // uint16  -> int8
        // uint16  -> int16
        // uint16  -> int32
        // uint16  -> int64
        // uint32  -> int8
        // uint32  -> int16
        // uint32  -> int32
        // uint32  -> int64
        // uint64  -> int8
        // uint64  -> int16
        // uint64  -> int32
        // uint64  -> int64
        //
        // in x86_64, native int is 32 bits
        // so we don't need to convert int32 to nint
        // or uint32 to nuint
        //
        // int64   -> nint
        // uint64  -> nuint
        //
        // nint    -> int64
        // nuint   -> uint64
        //
        // also, we need to convert pointers to integers
        // int x86_64, pointers are 64 bits
        // so we don't need to convert uint64 to pointer
        //
        // int32   -> pointer
        // uint32  -> pointer
        //
        // pointer -> int32
        // pointer -> uint32
        //

        switch (from, to)
        {
            case (TypeKind.Int8, TypeKind.Int16):
                opcode = ILOpcode.Conv_I2;
                return true;
            case (TypeKind.Int8, TypeKind.Int32):
                opcode = ILOpcode.Conv_I4;
                return true;
            case (TypeKind.Int8, TypeKind.Int64):
                opcode = ILOpcode.Conv_I8;
                return true;

            default:
                opcode = null;
                return false;
        }
    }
}

internal class BoundBlock
{
    public bool HasLocals => throw new NotImplementedException();
    public ImmutableArray<BoundStatement> Statements => throw new NotImplementedException();
}

internal class ILGenerator
{
    public void EmitBlock(BoundBlock block)
    {
        if (block.HasLocals)
        {
            OpenLocalScope(block);
        }

        EmitStatements(block);

        if (block.HasLocals)
        {
            CloseLocalScope(block);
        }
    }

    private void EmitStatements(BoundBlock block)
    {
        foreach (var statement in block.Statements)
        {
            EmitStatement(statement);
        }
    }

    private void EmitStatement(BoundStatement statement)
    {
        throw new NotImplementedException();
    }

    private void EmitExpression(BoundExpression expression)
    {
        throw new NotImplementedException();
    }

    private void EmitBinaryExpression(BoundBinaryExpression expression)
    {
        if (BinaryOperatorKindExtensions.IsLogical(expression.OperatorKind))
        {
            switch (expression.OperatorKind)
            {
                case BinaryOperatorKind.LogicalAnd:
                    EmitLogicalAnd(expression);
                    break;
                case BinaryOperatorKind.LogicalOr:
                    EmitLogicalOr(expression);
                    break;
                default:
                    throw new UnreachableException();
            }

            return;
        }

        EmitExpression(expression.Left);
        EmitExpression(expression.Right);

        switch (expression.OperatorKind)
        {
            case BinaryOperatorKind.Addition:
                EmitAddition(expression);
                break;
            case BinaryOperatorKind.Subtraction:
                EmitSubtraction(expression);
                break;
            case BinaryOperatorKind.Multiplication:
                EmitMultiplication(expression);
                break;
            case BinaryOperatorKind.Division:
                EmitDivision(expression);
                break;
            case BinaryOperatorKind.Remainder:
                EmitRemainder(expression);
                break;

            case BinaryOperatorKind.BitwiseAnd:
                EmitBitwiseAnd(expression);
                break;
            case BinaryOperatorKind.BitwiseOr:
                EmitBitwiseOr(expression);
                break;
            case BinaryOperatorKind.BitwiseXor:
                EmitBitwiseXor(expression);
                break;

            case BinaryOperatorKind.Equality:
                EmitEquality(expression);
                break;
            case BinaryOperatorKind.Inequality:
                EmitInequality(expression);
                break;

            case BinaryOperatorKind.LessThan:
                EmitLessThan(expression);
                break;
            case BinaryOperatorKind.LessThanOrEqual:
                EmitLessThanEqual(expression);
                break;
            case BinaryOperatorKind.GreaterThan:
                EmitGreaterThan(expression);
                break;
            case BinaryOperatorKind.GreaterThanOrEqual:
                EmitGreaterThanEqual(expression);
                break;

            case BinaryOperatorKind.LeftShift:
                EmitShiftLeft(expression);
                break;
            case BinaryOperatorKind.RightShift:
                EmitShiftRight(expression);
                break;
        }
    }

    private void EmitAddition(BoundBinaryExpression expression)
    {
        if (_currentBlock.IsChecked)
        {
            if (TypeKindExtensions.IsUnsignedType(expression.ResultingType))
            {
                EmitOpcode(ILOpcode.Add_Ovf_Un);
            }
            else
            {
                EmitOpcode(ILOpcode.Add_Ovf);
            }
        }
        else
        {
            EmitOpcode(ILOpcode.Add);
        }
    }

    private void EmitSubtraction(BoundBinaryExpression expression)
    {
        if (_currentBlock.IsChecked)
        {
            if (TypeKindExtensions.IsUnsignedType(expression.ResultingType))
            {
                EmitOpcode(ILOpcode.Sub_Ovf_Un);
            }
            else
            {
                EmitOpcode(ILOpcode.Sub_Ovf);
            }
        }
        else
        {
            EmitOpcode(ILOpcode.Sub);
        }
    }

    private void EmitMultiplication(BoundBinaryExpression expression)
    {
        if (_currentBlock.IsChecked)
        {
            if (TypeKindExtensions.IsUnsignedType(expression.ResultingType))
            {
                EmitOpcode(ILOpcode.Mul_Ovf_Un);
            }
            else
            {
                EmitOpcode(ILOpcode.Mul_Ovf);
            }
        }
        else
        {
            EmitOpcode(ILOpcode.Mul);
        }
    }

    private void EmitDivision(BoundBinaryExpression expression)
    {

        if (TypeKindExtensions.IsUnsignedType(expression.ResultingType))
        {
            EmitOpcode(ILOpcode.Div_Un);
        }
        else
        {
            EmitOpcode(ILOpcode.Div);
        }
    }

    private void EmitRemainder(BoundBinaryExpression expression)
    {
        if (TypeKindExtensions.IsUnsignedType(expression.ResultingType))
        {
            EmitOpcode(ILOpcode.Rem_Un);
        }
        else
        {
            EmitOpcode(ILOpcode.Rem);
        }
    }

    private void EmitBitwiseAnd(BoundBinaryExpression expression)
    {
        EmitOpcode(ILOpcode.And);
    }

    private void EmitBitwiseOr(BoundBinaryExpression expression)
    {
        EmitOpcode(ILOpcode.Or);
    }

    private void EmitBitwiseXor(BoundBinaryExpression expression)
    {
        EmitOpcode(ILOpcode.Xor);
    }

    private void EmitLogicalAnd(BoundBinaryExpression expression)
    {
        // validate the left operand
        // if the left operand is false
        // then the result is false
        // jump to the short circuit
        EmitExpression(expression.Left);
        EmitBranch(ILOpcode.Brfalse, "short_circuit");

        // validate the right operand
        // the result is the right operand
        // jump to the end
        EmitExpression(expression.Right);
        EmitBranch(ILOpcode.Br, "end");

        // exit early if the left operand is false
        EmitLabel("short_circuit");
        EmitOpcode(ILOpcode.Ldc_I4_0);

        // the end of the logical and
        EmitLabel("end");
    }

    private void EmitLogicalOr(BoundBinaryExpression expression)
    {
        // validate the left operand
        // if the left operand is true
        // then the result is true
        // jump to the end
        EmitExpression(expression.Left);
        EmitBranch(ILOpcode.Brtrue, "short_circuit");

        // validate the right operand
        // the result is the right operand
        // jump to the end
        EmitExpression(expression.Right);
        EmitBranch(ILOpcode.Br, "end");

        // exit early if the left operand is true
        EmitLabel("short_circuit");
        EmitOpcode(ILOpcode.Ldc_I4_1);

        // the end of the logical or
        EmitLabel("end");
    }

    private void EmitEquality(BoundBinaryExpression expression)
    {
        EmitOpcode(ILOpcode.Ceq);
    }

    private void EmitInequality(BoundBinaryExpression expression)
    {
        EmitOpcode(ILOpcode.Ceq);
        EmitOpcode(ILOpcode.Not);
    }

    private void EmitLessThan(BoundBinaryExpression expression)
    {
        if (TypeKindExtensions.IsUnsignedType(expression.ResultingType))
        {
            EmitOpcode(ILOpcode.Clt_Un);
        }
        else
        {
            EmitOpcode(ILOpcode.Clt);
        }
    }

    private void EmitLessThanEqual(BoundBinaryExpression expression)
    {
        if (TypeKindExtensions.IsUnsignedType(expression.ResultingType))
        {
            EmitOpcode(ILOpcode.Cgt_Un);
            EmitOpcode(ILOpcode.Not);
        }
        else
        {
            EmitOpcode(ILOpcode.Cgt);
            EmitOpcode(ILOpcode.Not);
        }
    }

    private void EmitGreaterThan(BoundBinaryExpression expression)
    {
        if (TypeKindExtensions.IsUnsignedType(expression.ResultingType))
        {
            EmitOpcode(ILOpcode.Cgt_Un);
        }
        else
        {
            EmitOpcode(ILOpcode.Cgt);
        }
    }

    private void EmitGreaterThanEqual(BoundBinaryExpression expression)
    {
        if (TypeKindExtensions.IsUnsignedType(expression.ResultingType))
        {
            EmitOpcode(ILOpcode.Clt_Un);
            EmitOpcode(ILOpcode.Not);
        }
        else
        {
            EmitOpcode(ILOpcode.Clt);
            EmitOpcode(ILOpcode.Not);
        }
    }

    private void EmitShiftLeft(BoundBinaryExpression expression)
    {
        EmitOpcode(ILOpcode.Shl);
    }

    private void EmitShiftRight(BoundBinaryExpression expression)
    {
        if (TypeKindExtensions.IsUnsignedType(expression.ResultingType))
        {
            EmitOpcode(ILOpcode.Shr_Un);
        }
        else
        {
            EmitOpcode(ILOpcode.Shr);
        }
    }

    private void EmitAssignmentExpression(BoundAssignmentExpression expression)
    {
        EmitExpression(expression.Value);

        var resultType = expression.Value.ResultingType;
    }

    private void EmitConversionExpression(BoundConversionExpression expression)
    {
        EmitExpression(expression.Operand);


    }

    private void EmitBranch(ILOpcode opcode, object label, ILOpcode revOpcode = ILOpcode.Nop)
    {

    }

    private void EmitLabel(object label)
    {
    }

    private void EmitOpcode(ILOpcode opcode)
    {
        throw new NotImplementedException();
    }

    private void OpenLocalScope(BoundBlock block)
    {
        throw new NotImplementedException();
    }

    private void CloseLocalScope(BoundBlock block)
    {
        throw new NotImplementedException();
    }
}

internal class BasicBlock
{
    public bool IsChecked { get; }
    public bool IsUnchecked { get; }
    public bool IsUnsafe { get; }

    public bool IsReachable { get; }

    public ILOpcode? BranchOpcode { get; }

    public BasicBlock? NextBlock { get; }
}

internal class ILInstruction
{
    public ILOpcode Opcode { get; }
    public ImmutableArray<byte> Operands { get; }

    public ILInstruction(ILOpcode opcode, ImmutableArray<byte> operands)
    {
        Opcode = opcode;
        Operands = operands;
    }
}

internal enum ILOpcode
{
    #region Miscellaneous Operations
    /// <summary>
    /// Do nothing (<b>N</b>o <b>op</b>eration).
    /// </summary>
    Nop,
    /// <summary>
    /// Inform a debugger that a <b>break</b>point has been reached.
    /// </summary>
    Break,

    /// <summary>
    /// Subsequent call terminates current method.
    /// </summary>
    Tail,

    /// <summary>
    /// Push the size, in bytes, of a type as an unsigned int32.
    /// </summary>
    /// <remarks><code>sizeof &lt;typeTok&gt;</code></remarks>
    Sizeof,
    #endregion

    #region Protected Blocks
    Leave = 0x04,
    Leave_S,
    #endregion

    #region Object Initialization
    Newarr = 0x08,
    Newobj,
    Localloc,

    Cpblk = 0x0C,
    Cpobj,
    Initblk,
    Initobj,
    #endregion

    #region Stack Load Operations
    Ldarg = 0x10,
    Ldarga,
    Ldarg_S,
    Ldarga_S,

    Ldarg_0,
    Ldarg_1,
    Ldarg_2,
    Ldarg_3,

    Ldloc,
    Ldloca,
    Ldloc_S,
    Ldloca_S,

    Ldloc_0,
    Ldloc_1,
    Ldloc_2,
    Ldloc_3,

    Ldc_I4_0,
    Ldc_I4_1,
    Ldc_I4_2,
    Ldc_I4_3,
    Ldc_I4_4,
    Ldc_I4_5,
    Ldc_I4_6,
    Ldc_I4_7,

    Ldc_I4_8,

    Ldc_I4_S = 0x2A,
    Ldc_I4_M1,

    Ldc_I4,
    Ldc_I8,
    Ldc_R4,
    Ldc_R8,
    #endregion

    #region Arithmetic Operations
    Add = 0x30,
    Add_Ovf,
    Add_Ovf_Un,

    Sub = 0x34,
    Sub_Ovf,
    Sub_Ovf_Un,

    Mul = 0x38,
    Mul_Ovf,
    Mul_Ovf_Un,

    Div = 0x3C,
    Div_Un,
    Rem,
    Rem_Un,
    #endregion

    #region Shift Operations
    Shl = 0x40,
    Shr,
    Shr_Un,
    #endregion

    #region Bitwise Operations
    And = 0x44,
    Or,
    Xor,
    Not,

    Neg,
    #endregion

    #region Special Stack Operations
    Pop = 0x4A,
    Dup,
    #endregion

    #region Type Operations
    Isinst = 0x4C,
    Castclass,
    Constrained,
    #endregion

    #region Exception Handling
    Throw = 0x50,
    Rethrow,

    Endfault = 0x54,
    Endfilter,
    Endfinally,
    #endregion

    #region Comparison Operations
    Ceq = 0x58,
    Cgt,
    Clt,

    Cgt_Un = 0x5C,
    Clt_Un,
    #endregion

    #region Branch Operations
    Switch = 0x5F,

    Beq = 0x60,
    Beq_S,
    Bne_Un,
    Bne_Un_S,

    Bge,
    Bgt,
    Ble,
    Blt,

    Bge_S,
    Bgt_S,
    Ble_S,
    Blt_S,

    Bge_Un,
    Bgt_Un,
    Ble_Un,
    Blt_Un,

    Bge_Un_S,
    Bgt_Un_S,
    Ble_Un_S,
    Blt_Un_S,

    Br,
    Br_S,
    Jmp,
    Ret,

    Brtrue,
    Brfalse,
    Brtrue_S,
    Brfalse_S,

    Brinst = Brtrue,
    Brnull = Brfalse,
    Brinst_S = Brtrue_S,
    Brnull_S = Brfalse_S,

    Brzero = Brfalse,
    Brzero_S = Brfalse_S,
    #endregion

    #region Function Calls
    Call = 0x7C,
    Calli,
    Callvirt,
    Arglist,
    #endregion

    #region Conversion Operations
    Conv_I1 = 0x80,
    Conv_I2,
    Conv_I4,
    Conv_I8,

    Conv_U1,
    Conv_U2,
    Conv_U4,
    Conv_U8,

    Conv_Ovf_I1,
    Conv_Ovf_I2,
    Conv_Ovf_I4,
    Conv_Ovf_I8,

    Conv_Ovf_U1,
    Conv_Ovf_U2,
    Conv_Ovf_U4,
    Conv_Ovf_U8,

    Conv_Ovf_I1_Un,
    Conv_Ovf_I2_Un,
    Conv_Ovf_I4_Un,
    Conv_Ovf_I8_Un,

    Conv_Ovf_U1_Un,
    Conv_Ovf_U2_Un,
    Conv_Ovf_U4_Un,
    Conv_Ovf_U8_Un,

    Conv_I,
    Conv_U,
    Conv_Ovf_I,
    Conv_Ovf_U,

    Conv_Ovf_I_Un,
    Conv_Ovf_U_Un,

    Conv_R4,
    Conv_R8,

    Conv_R_Un,
    #endregion

    #region Box Operations
    Box = 0xA4,
    Unbox,
    Unbox_Any,

    Mkrefany = 0xA8,
    Refanytype,
    Refanyval,
    #endregion

    #region Special Arithmetic Operations
    Unaligned = 0xAC,
    Readonly,
    Volatile,
    Ckfinite,
    #endregion

    #region Array Load Operations
    Ldelem_I1 = 0xB0,
    Ldelem_I2,
    Ldelem_I4,
    Ldelem_I8,

    Ldelem_U1,
    Ldelem_U2,
    Ldelem_U4,
    Ldelem_U8,

    Ldelem_R4,
    Ldelem_R8,
    Ldelem_I,
    Ldelem_Ref,

    Ldelema,
    Ldlen,
    #endregion

    #region Indirect Load Operations
    Ldind_I1 = 0xC0,
    Ldind_I2,
    Ldind_I4,
    Ldind_I8,

    Ldind_U1,
    Ldind_U2,
    Ldind_U4,
    Ldind_U8,

    Ldind_R4,
    Ldind_R8,
    Ldind_I,
    Ldind_Ref,
    #endregion

    #region Field Load Operations
    Ldfld = 0xCC,
    Ldflda,
    Ldsfld,
    Ldsflda,
    #endregion

    #region Function Pointer Load Operations
    Ldfnt = 0xD0,
    Ldvirtfnt,
    #endregion

    #region Object Load Operations
    Ldnull = 0xD4,
    Ldobj,
    Ldstr,

    Ldtoken,
    #endregion

    #region Stack Store Operations
    Starg = 0xD8,
    Starg_S,

    Stloc,
    Stloc_S,

    Stloc_0,
    Stloc_1,
    Stloc_2,
    Stloc_3,
    #endregion

    #region Array Store Operations
    Stelem_I1 = 0xE0,
    Stelem_I2,
    Stelem_I4,
    Stelem_I8,

    Stelem_R4,
    Stelem_R8,
    Stelem_I,
    Stelem_Ref,

    Stelem,
    #endregion

    #region Indirect Store Operations
    Stind_I1 = 0xEC,
    Stind_I2,
    Stind_I4,
    Stind_I8,

    Stind_R4,
    Stind_R8,
    Stind_I,
    Stind_Ref,
    #endregion

    #region Field Store Operations
    Stfld = 0xF4,
    Stsfld,
    #endregion

    #region Object Store Operations
    Stobj = 0xF8,
    #endregion

    #region Unchecked Arithmetic Operations
    No_Typecheck = 0xFA,
    No_Rangecheck = No_Typecheck,
    No_Nullcheck = No_Typecheck,
    #endregion

    // 0xFC
    // 0xFD
    // 0xFE
    // 0xFF
}