# LambdaCalculusEvaluator
An implementation of a lambda calculus evaluator based on https://opendsa.cs.vt.edu/ODSA/Books/PL/html/index.html#lambda-calculus

It was implemented as a learning exercise. It can be used as a learning reference in C#.

## Usage
Can be ran by cloning the repo and running the Console Application project.
Built with .NET 8.

## Lambda Calculus
Lambda Calculus is the simplest functional programming language. There are many variations. The variation implemented in this repo has the following BNF grammar (chosen for simplicity):
- <λexp> ::= \<var> | λ\<var>.<λexp> | (<λexp><λexp>)

Therefore:
 - The syntax is different than https://lambdaexplorer.com/ ; expression will not translate directly.
 - Spaces are not used or supported.
 - Parentheses are used for *application*, they are not used for grouping.

The reduction algorithm used in this repo is [normal order reduction](https://opendsa.cs.vt.edu/ODSA/Books/PL/html/ReductionStrategies.html#normal-order)

## Example
    "(λa.λb.λc.λd.(((ab)c)d)(bc))".Evaluate();
    //Result: λe.λf.λd.((((bc)e)f)d)

## Implementation Details
This repo is an implementation of a lambda calculus evaluator in C# simply because that is the language I am most familiar with.
The implementation is made up of the following components:

- Expression records (Variable, Function, and Application)
- Token records (LeftParentheses, Letter, Lambda, Dot, and RightParentheses)
- Lexer (Tokenizes a string into Tokens)
- Parser (Parses Tokens into Expression)
- Interpreter (Interprets an Expression by reducing it)
- Evaluator (Evaluates a string by Tokenizing, Parsing, and Interpreting it)

The project is implemented in a "functional" way, using records and extension methods (as apposed to classes with methods).
