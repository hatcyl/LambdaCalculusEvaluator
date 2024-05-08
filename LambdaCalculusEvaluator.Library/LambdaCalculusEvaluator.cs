namespace LambdaCalculusEvaluator.Library;

public abstract record Expression;
public record Variable(char Name) : Expression;
public record Function(Variable Parameter, Expression Body) : Expression;
public record Application(Expression Function, Expression Argument) : Expression;

public abstract record Token;
public record LeftParentheses : Token;
public record Letter(char Value) : Token;
public record Lambda : Token;
public record Dot : Token;
public record RightParentheses : Token;

public static class Evaluator
{
    public static string Evaluate(this string program) => new(program.Tokenize().Parse().Interpret().Unparse().Untokenize().ToArray());
}

public static class Interpreter
{
    public static Expression Interpret(this Expression expression) => expression.Interpret(100);

    public static Expression Interpret(this Expression expression, int maxReductions) =>
        expression.CanReduce() && maxReductions >= 0 ? expression.Reduce().Interpret(--maxReductions) : expression;

    public static bool CanReduce(this Expression expression) => expression.HasBetaRedex();

    public static bool HasBetaRedex(this Expression expression) => expression switch
    {
        Variable => false,
        Function function => function.Parameter.HasBetaRedex() || function.Body.HasBetaRedex(),
        Application application => application.Function switch
        {
            Function => true,
            _ => application.Function.HasBetaRedex() || application.Argument.HasBetaRedex()
        },
        _ => throw new Exception()
    };

    public static Expression Reduce(this Expression expression) => expression switch
    {
        Variable variable => variable,
        Function function => function with { Body = function.Body.Reduce() },
        Application application => application.Function switch
        {
            Function function => function.BetaReduce(application.Argument),
            Expression expression1 => new Application(expression1.Reduce(), application.Argument.Reduce())
        },
        _ => throw new Exception()
    };

    public static Expression BetaReduce(this Function function, Expression argument) =>
        Substitude(argument, function.Parameter, function.Body);

    public static Expression Substitude(Expression argument, Variable parameter, Expression body) => body switch
    {
        Variable variable => parameter == body ? argument : variable,
        Function function => parameter == function.Parameter
            ? function
            : !function.Parameter.VariableOccursFree(argument)
                ? function with { Body = Substitude(argument, parameter, function.Body) }
                : Substitude(argument, parameter, function.AlphaConvert()),
        Application application => new Application(Substitude(argument, parameter, application.Function), Substitude(argument, parameter, application.Argument)),
        _ => throw new Exception()
    };

    public static bool VariableOccursFree(this Variable variable, Expression expression) => expression switch
    {
        Variable variable1 => variable == variable1,
        Application application => (variable.VariableOccursFree(application.Function) || variable.VariableOccursFree(application.Argument)),
        Function function => function.Parameter != variable && variable.VariableOccursFree(function.Body),
        _ => throw new Exception()
    };

    public static Function AlphaConvert(this Function function) =>
        function.Parameter.NextVariableThatDoesntOccurInBody(function.Body).Pipe(variable => new Function(variable, Convert(function.Parameter, variable, function.Body)));

    public static Expression Convert(this Variable variableToConvertFrom, Variable variableToConvertTo, Expression expressionToConvert) => expressionToConvert switch
    {
        Variable variableToConvert =>
            variableToConvert == variableToConvertFrom ? variableToConvertTo : variableToConvert,
        Application applicationToConvert =>
            new Application(Convert(variableToConvertFrom, variableToConvertTo, applicationToConvert.Function), Convert(variableToConvertFrom, variableToConvertTo, applicationToConvert.Argument)),
        Function functionToConvert => functionToConvert.Parameter == variableToConvertFrom || functionToConvert.Parameter == variableToConvertTo
            ? functionToConvert
            : new Function(functionToConvert.Parameter, Convert(variableToConvertFrom, variableToConvertTo, functionToConvert.Body)),
        _ => throw new Exception()
    };

    public static Variable NextVariableThatDoesntOccurInBody(this Variable variable, Expression body) =>
        variable.NextVariable().Pipe(nextVariable => IsVariableInBody(nextVariable, body) ? NextVariableThatDoesntOccurInBody(nextVariable, body) : nextVariable);

    public static bool IsVariableInBody(this Variable variable, Expression body) => body switch
    {
        Variable variable1 => variable == variable1,
        Application application => variable.IsVariableInBody(application.Function) || variable.IsVariableInBody(application.Argument),
        Function function => variable.IsVariableInBody(function.Parameter) || variable.IsVariableInBody(function.Body),
        _ => throw new Exception()
    };

    public static Variable NextVariable(this Variable variable) => new Variable(variable.Name.NextLowerCaseLetter());

    public static char NextLowerCaseLetter(this char value) => value switch
    {
        'z' => 'a',
        _ => (char)(value + 1)
    };
}

public static class Parser
{
    public static Expression Parse(this IEnumerable<Token> tokens) => ParseRecursive(tokens).Expression;
    private static (Expression Expression, IEnumerable<Token> UnusedTokens) ParseRecursive(this IEnumerable<Token> tokens) => tokens.First() switch
    {
        LeftParentheses => tokens.Skip(1).ParseRecursive().Pipe(left => left.UnusedTokens.ParseRecursive().Pipe(right => (new Application(left.Expression, right.Expression), right.UnusedTokens.Skip(1)))),
        Letter letter => (new Variable(letter.Value), tokens.Skip(1)),
        Lambda => tokens.Skip(1).ParseRecursive().Pipe(parameter => parameter.UnusedTokens.Skip(1).ParseRecursive().Pipe(body => (new Function((Variable)parameter.Expression, body.Expression), body.UnusedTokens))),
        _ => throw new Exception(),
    };

    public static IEnumerable<Token> Unparse(this Expression expression) => expression switch
    {
        Variable variable => [new Letter(variable.Name)],
        Function function => [new Lambda(), .. Unparse(function.Parameter), new Dot(), .. Unparse(function.Body)],
        Application application => [new LeftParentheses(), .. Unparse(application.Function), .. Unparse(application.Argument), new RightParentheses()],
        _ => throw new Exception()
    };
}

public static class Lexer
{
    public static IEnumerable<Token> Tokenize(this IEnumerable<char> characters) => characters.Select<char, Token>(character => character switch
    {
        '(' => new LeftParentheses(),
        'λ' => new Lambda(),
        '.' => new Dot(),
        ')' => new RightParentheses(),
        _ when char.IsAsciiLetterLower(character) => new Letter(character),
        _ => throw new Exception()
    });

    public static IEnumerable<char> Untokenize(this IEnumerable<Token> tokens) => tokens.Select(token => token switch
    {
        LeftParentheses => '(',
        Lambda => 'λ',
        Dot => '.',
        RightParentheses => ')',
        Letter letter => letter.Value,
        _ => throw new Exception()
    });
}

public static class PipeExtensions
{
    public static TOutput Pipe<TInput, TOutput>(this TInput input, Func<TInput, TOutput> function) => function(input);
}
