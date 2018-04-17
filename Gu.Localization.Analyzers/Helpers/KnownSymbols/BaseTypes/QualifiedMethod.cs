﻿namespace Gu.Localization.Analyzers
{
    using Microsoft.CodeAnalysis;

    internal class QualifiedMethod : QualifiedMember<IMethodSymbol>
    {
        public QualifiedMethod(QualifiedType containingType, string name)
            : base(containingType, name)
        {
        }
    }
}