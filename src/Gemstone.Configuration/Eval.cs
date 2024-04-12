//******************************************************************************************************
//  Eval.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/11/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.ComponentModel;
using Gemstone.Expressions.Evaluator;

namespace Gemstone.Configuration;

/// <summary>
/// Represents an evaluation type used to hold an expression to evaluate
/// as well as methods to execute the evaluation.
/// </summary>
/// <remarks>
/// Expected use of this class is with expression-based <see cref="Settings"/>.
/// </remarks>
[TypeConverter(typeof(EvalConverter))]
public class Eval(string expression)
{
    private ExpressionCompiler? m_expressionCompiler;
    private object? m_value;

    /// <summary>
    /// Gets or sets the expression to evaluate.
    /// </summary>
    /// <remarks>
    /// Expression is expected to return a value.
    /// </remarks>
    public string Expression { get; set; } = expression;

    /// <summary>
    /// Gets or sets the transpiled expression.
    /// </summary>
    public string? TranspiledExpression { get; set; }

    /// <summary>
    /// Gets or sets the result of the expression evaluation.
    /// </summary>
    /// <remarks>
    /// When <see cref="Value"/> is uninitialized, i.e., <c>null</c>, expression
    /// will be evaluated to acquire a result. Further requests will return
    /// cached result value. To re-evaluate expression, set <see cref="Value"/>
    /// to <c>null</c> before getting property.
    /// </remarks>
    public object? Value
    {
        get => m_value ??= Evaluate(); 
        set => m_value = value;
    }

    // Evaluates expression and returns the result
    private object? Evaluate()
    {
        m_expressionCompiler ??= BuildExpressionCompiler();
        return m_expressionCompiler.ExecuteFunction();
    }

    /// <summary>
    /// Builds a new <see cref="ExpressionCompiler"/> using the existing <see cref="Expression"/>
    /// and the global pre-defined <see cref="TypeRegistry"/>.
    /// </summary>
    /// <returns>New <see cref="ExpressionCompiler"/>.</returns>
    public ExpressionCompiler BuildExpressionCompiler()
    {
        return new ExpressionCompiler(TranspiledExpression ?? Expression) { TypeRegistry = TypeRegistry };
    }

    /// <summary>
    /// Gets an instance of <see cref="Eval"/> that represents a <c>null</c> expression;
    /// </summary>
    public static readonly Eval Null = new("null");

    /// <summary>
    /// Gets the global <see cref="Expressions.Evaluator.TypeRegistry"/> used to resolve types for
    /// <c>[eval]</c> typed settings.
    /// </summary>
    /// <remarks>
    /// Applications can register more types and symbols with this property for use with <c>[eval]</c>
    /// settings. Ensure the types are registered before settings get bound to configurations sources,
    /// see <see cref="Settings.Bind"/>.
    /// </remarks>
    public static TypeRegistry TypeRegistry { get; }

    static Eval()
    {
        TypeRegistry = new TypeRegistry();
            
        TypeRegistry.RegisterType<Settings>();
        TypeRegistry.RegisterSymbol("settings", Settings.Instance);
        TypeRegistry.RegisterType(typeof(List<>));
    }
}
