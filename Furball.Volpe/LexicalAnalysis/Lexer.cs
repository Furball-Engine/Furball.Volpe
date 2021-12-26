using System;
using System.Collections.Generic;
using System.Text;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.LexicalAnalysis
{
    public class Lexer
    {
        private readonly TextConsumer _textConsumer;

        public Lexer(string source)
        {
            _textConsumer = new TextConsumer(source);
        }

        private string ConsumeNextString()
        {
            char character;
            StringBuilder stringBuilder = new StringBuilder();
            
            if (!_textConsumer.TryConsumeNext(out character) || character != '"')
                throw new InvalidOperationException();

            for(;;)
            {
                if (!_textConsumer.TryConsumeNext(out character))
                    throw new UnexpectedEofException(_textConsumer.PositionInText);

                switch (character)
                {
                    case '"':
                        return stringBuilder.ToString();
                    
                    default:
                        stringBuilder.Append(character);
                        break;
                }
            }
        }

        private string ConsumeNextLiteral()
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            for(;;)
            {
                if (!_textConsumer.TryPeekNext(out char character))
                    break;
                
                if (character.CanBeInVolpeLiteral())
                {
                    stringBuilder.Append(character);
                    _textConsumer.SkipOne();
                } 
                else 
                    break;
            }

            return stringBuilder.ToString();
        }

        private double ConsumeNextNumber()
        {
            double num = 0;
            int exp = 0;
            bool isRational = false;
            
            for(;;)
            {
                if (!_textConsumer.TryPeekNext(out char character))
                    break;

                if (character.IsVolpeDigit())
                {
                    num = num * 10 + (character - '0');

                    if (isRational)
                        exp--;
                    
                    _textConsumer.TryConsumeNext(out _);
                } 
                else if (character == '.')
                {
                    if (isRational)
                        throw new UnexceptedSymbolException('.', _textConsumer.PositionInText);
                        
                    isRational = true;

                    _textConsumer.TryConsumeNext(out _);
                } 
                else
                    break;
            }

            return Math.Pow(10, exp) * num;
        }

        private void SkipWhiteSpaces()
        {
            while (_textConsumer.TryPeekNext(out char character) && char.IsWhiteSpace(character))
                _textConsumer.TryConsumeNext(out _);
        }

        private TokenValue ConsumeOperator()
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            for(;;)
            {
                if (!_textConsumer.TryPeekNext(out char character))
                    break;
                
                if (character.IsVolpeOperator())
                {
                    stringBuilder.Append(character);
                    _textConsumer.SkipOne();
                } 
                else 
                    break; 
            }

            string op = stringBuilder.ToString();
            return op switch
            {
                "=" => new TokenValue.Assign(),
                "==" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.Eq()),
                ">" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.GreaterThan()),
                "<" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.LessThan()),
                ">=" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.GreaterThanOrEqual()),
                "<=" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.LessThanOrEqual()),
                "&&" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.And()),
                "||" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.Or()),
                "!=" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.NotEq()),
                "!" => new TokenValue.BooleanOperator(new TokenValueBooleanOperator.Not()),
                
                _ when op.EndsWith('=') => 
                    new TokenValue.OperatorWithAssignment(TokenValueOperator.FromCharacter(op[0])),
                
                _ when op.Length == 1 => new TokenValue.ArithmeticalOperator(TokenValueOperator.FromCharacter(op[0])),
                
                _ => throw new InvalidOperatorStringException(op, _textConsumer.PositionInText),
            };
        }

        public bool TryConsumeNextToken(out Token? token)
        {
            token = null;
            
            SkipWhiteSpaces();
            PositionInText currentPositionInText = _textConsumer.PositionInText;

            char character;
            if (!_textConsumer.TryPeekNext(out character))
                return false;

            TokenValue tokenValue = character switch
            {
                '"' => new TokenValue.String(ConsumeNextString()),
                
                ':' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.ArrayAccess()),
                ',' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Comma()),
                '#' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Hashtag()),
                '[' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.LeftBracket()),
                ']' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.RightBracket()),
                '(' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.LeftRoundBracket()),
                ')' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.RightRoundBracket()),
                '{' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.LeftCurlyBracket()),
                '}' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.RightCurlyBracket()),
                '$' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Dollar()),
                ';' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.SemiColon()),
                _ when character.IsVolpeOperator() => ConsumeOperator(),

                _ when character.IsVolpeDigit() => new TokenValue.Number(ConsumeNextNumber()),

                _ when character.CanBeInVolpeLiteral() => ConsumeNextLiteral() switch 
                {
                    "true" => new TokenValue.True(),
                    "false" => new TokenValue.False(),
                    "funcdef" => new TokenValue.FuncDef(),
                    "func" => new TokenValue.Func(),
                    "ret" => new TokenValue.Return(),
                    "if" => new TokenValue.If(),
                    "elif" => new TokenValue.Elif(),
                    "else" => new TokenValue.Else(),
                    "while" => new TokenValue.While(),
                    
                    {} value => new TokenValue.Literal(value) 
                },

                var c => throw new UnexceptedSymbolException(c, currentPositionInText)
            };

            token = new Token {Value = tokenValue, PositionInText = currentPositionInText};

            return true;
        }

        public IEnumerable<Token> GetTokenEnumerator()
        {
            Token? token;
            while (TryConsumeNextToken(out token))
                yield return token!;
        }
    }
}
