using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Core.Sharp;

internal class Declaration
{
    private SyntaxNode _syntaxNode;

    public Declaration(SyntaxNode syntaxNode)
    {
        _syntaxNode = syntaxNode;
    }
}

internal class NamespaceDeclaration : Declaration
{
    public List<Declaration> Declarations { get; set; }

    public NamespaceDeclaration(SyntaxNode syntaxNode) : base(syntaxNode)
    {
    }
}

internal class TypeDeclaration : Declaration
{
    public List<Declaration> Declarations { get; set; }

    public TypeDeclaration(SyntaxNode syntaxNode) : base(syntaxNode)
    {
    }
}
