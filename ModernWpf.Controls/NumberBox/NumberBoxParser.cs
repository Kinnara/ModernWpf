// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ModernWpf.Controls
{
    internal enum MathTokenType
    {
        Numeric,
        Operator,
        Parenthesis,
    }

    internal struct MathToken
    {
        public MathToken(MathTokenType t, char c)
        {
            Type = t;
            Char = c;
            Value = double.NaN;
        }

        public MathToken(MathTokenType t, double d)
        {
            Type = t;
            Value = d;
            Char = char.MinValue;
        }

        public MathTokenType Type;
        public char Char;
        public double Value;
    };

    internal class NumberBoxParser
    {
        const string c_numberBoxOperators = "+-*/^";

        static IList<MathToken> GetTokens(string input, INumberBoxNumberFormatter numberParser)
        {
            var tokens = new List<MathToken>();

            bool expectNumber = true;
            while (input.Length > 0)
            {
                // Skip spaces
                var nextChar = input[0];
                if (nextChar != ' ')
                {
                    if (expectNumber)
                    {
                        if (nextChar == '(')
                        {
                            // Open parens are also acceptable, but don't change the next expected token type.
                            tokens.Add(new MathToken(MathTokenType.Parenthesis, nextChar));
                        }
                        else
                        {
                            var (value, charLength) = GetNextNumber(input, numberParser);

                            if (charLength > 0)
                            {
                                tokens.Add(new MathToken(MathTokenType.Numeric, value));
                                input = input.SafeSubstring(charLength - 1); // advance the end of the token
                                expectNumber = false; // next token should be an operator
                            }
                            else
                            {
                                // Error case -- next token is not a number
                                return s_emptyTokens;
                            }
                        }
                    }
                    else
                    {
                        if (c_numberBoxOperators.IndexOf(nextChar) >= 0)
                        {
                            tokens.Add(new MathToken(MathTokenType.Operator, nextChar));
                            expectNumber = true; // next token should be a number
                        }
                        else if (nextChar == ')')
                        {
                            // Closed parens are also acceptable, but don't change the next expected token type.
                            tokens.Add(new MathToken(MathTokenType.Parenthesis, nextChar));
                        }
                        else
                        {
                            // Error case -- could not evaluate part of the expression
                            return s_emptyTokens;
                        }
                    }
                }

                input = input.SafeSubstring(1);
            }

            return tokens;
        }

        static (double, int) GetNextNumber(string input, INumberBoxNumberFormatter numberParser)
        {
            // Attempt to parse anything before an operator or space as a number
            Regex regex = new Regex("^-?([^-+/*\\(\\)\\^\\s]+)");
            Match match = regex.Match(input);
            if (match.Success)
            {
                // Might be a number
                var matchLength = match.Value.Length;
                var parsedNum = numberParser.ParseDouble(input.Substring(0, matchLength));

                if (parsedNum.HasValue)
                {
                    // Parsing was successful
                    return (parsedNum.Value, matchLength);
                }
            }

            return (double.NaN, 0);
        }

        static int GetPrecedenceValue(char c)
        {
            int opPrecedence = 0;
            if (c == '*' || c == '/')
            {
                opPrecedence = 1;
            }
            else if (c == '^')
            {
                opPrecedence = 2;
            }

            return opPrecedence;
        }

        static IList<MathToken> ConvertInfixToPostfix(IList<MathToken> infixTokens)
        {
            List<MathToken> postfixTokens = new List<MathToken>();
            Stack<MathToken> operatorStack = new Stack<MathToken>();

            foreach (var token in infixTokens)
            {
                if (token.Type == MathTokenType.Numeric)
                {
                    postfixTokens.Add(token);
                }
                else if (token.Type == MathTokenType.Operator)
                {
                    while (operatorStack.Count > 0)
                    {
                        var top = operatorStack.Peek();
                        if (top.Type != MathTokenType.Parenthesis && (GetPrecedenceValue(top.Char) >= GetPrecedenceValue(token.Char)))
                        {
                            postfixTokens.Add(operatorStack.Pop());
                        }
                        else
                        {
                            break;
                        }
                    }
                    operatorStack.Push(token);
                }
                else if (token.Type == MathTokenType.Parenthesis)
                {
                    if (token.Char == '(')
                    {
                        operatorStack.Push(token);
                    }
                    else
                    {
                        while (operatorStack.Count > 0 && operatorStack.Peek().Char != '(')
                        {
                            // Pop operators onto output until we reach a left paren
                            postfixTokens.Add(operatorStack.Pop());
                        }

                        if (operatorStack.Count == 0)
                        {
                            // Broken parenthesis
                            return s_emptyTokens;
                        }

                        // Pop left paren and discard
                        operatorStack.Pop();
                    }
                }
            }

            // Pop all remaining operators.
            while (operatorStack.Count > 0)
            {
                if (operatorStack.Peek().Type == MathTokenType.Parenthesis)
                {
                    // Broken parenthesis
                    return s_emptyTokens;
                }

                postfixTokens.Add(operatorStack.Pop());
            }

            return postfixTokens;
        }

        static double? ComputePostfixExpression(IList<MathToken> tokens)
        {
            Stack<double> stack = new Stack<double>();

            foreach (var token in tokens)
            {
                if (token.Type == MathTokenType.Operator)
                {
                    // There has to be at least two values on the stack to apply
                    if (stack.Count < 2)
                    {
                        return null;
                    }

                    var op1 = stack.Pop();
                    var op2 = stack.Pop();

                    double result;

                    switch (token.Char)
                    {
                        case '-':
                            result = op2 - op1;
                            break;

                        case '+':
                            result = op1 + op2;
                            break;

                        case '*':
                            result = op1 * op2;
                            break;

                        case '/':
                            if (op1 == 0)
                            {
                                // divide by zero
                                return double.NaN;
                            }
                            else
                            {
                                result = op2 / op1;
                            }
                            break;

                        case '^':
                            result = Math.Pow(op2, op1);
                            break;

                        default:
                            return null;
                    }

                    stack.Push(result);
                }
                else if (token.Type == MathTokenType.Numeric)
                {
                    stack.Push(token.Value);
                }
            }

            // If there is more than one number on the stack, we didn't have enough operations, which is also an error.
            if (stack.Count != 1)
            {
                return null;
            }

            return stack.Peek();
        }

        public static double? Compute(string expr, INumberBoxNumberFormatter numberParser)
        {
            // Tokenize the input string
            var tokens = GetTokens(expr, numberParser);
            if (tokens.Count > 0)
            {
                // Rearrange to postfix notation
                var postfixTokens = ConvertInfixToPostfix(tokens);
                if (postfixTokens.Count > 0)
                {
                    // Compute expression
                    return ComputePostfixExpression(postfixTokens);
                }
            }

            return null;
        }

        private static readonly MathToken[] s_emptyTokens = new MathToken[0];
    }
}
