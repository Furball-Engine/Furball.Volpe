using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Volpe.LexicalAnalysis.Exceptions;

namespace Volpe.LexicalAnalysis
{
    public class Lexer: IEnumerable<WithPositionInText<Token>?>
    {
        private string _source;

        private int _position;
        private PositionInText _positionInText;
        
        public Lexer(string source)
        {
            _source = source;
            _position = -1;
        }

        private char _lastConsumedCharacter;

        private char? PeekNextCharacter()
        {
            int nextPosition = _position + 1;
            return GetCharAtPosition(nextPosition);
        }

        private void Step() => ++_position;

        private TResult StepAnd<TResult>(Func<TResult> action)
        {
            ++_position;
            return action();
        }
        
        private void StepAnd(Action action)
        {
            ++_position;
            action();
        }

        private char? GetCharAtPosition(int position)
        {
            if (position < 0 || position >= _source.Length)
                return null;

            return _source[position];
        }
        
        private char? ConsumeNextCharacter()
        {
            Step();
            return GetCharAtPosition(_position);
        }

        private string ConsumeNextString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (ConsumeNextCharacter() != '"')
                throw new InvalidOperationException();

            for(;;)
            {
                char? c = ConsumeNextCharacter();

                switch (c)
                {
                    case '"':
                        return stringBuilder.ToString();
                    
                    case null:
                        throw new LexerUnexpectedEofException(_positionInText);
                    
                    default:
                        stringBuilder.Append(c.Value);
                        break;
                }
            }
        }

        private string ConsumeNextLiteral()
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            for(;;)
            {
                char? c = PeekNextCharacter();

                switch (c)
                {
                    case >= '¡':
                    case >= 'a' and <= 'z':
                    case >= '@' and <= 'Z':
                    case >= '0' and <= '9':
                        stringBuilder.Append(c.Value);
                        ConsumeNextCharacter(); 
                        break;
                    
                    default:
                        return stringBuilder.ToString();
                }
            }
        }

        private double ConsumeNextNumber()
        {
            double num = 0;
            int exp = 0;
            bool isRational = false;
            
            for(;;)
            {
                char? c = ConsumeNextCharacter();

                switch (c)
                {
                    case >= '0' and <= '9':
                        num = num * 10 + (c.Value - '0');

                        if (isRational)
                            exp--;

                        break;

                    case '.':
                        if (isRational)
                            throw new LexerUnexceptedSymbolException('.', _positionInText);
                        
                        isRational = true;
                        break;
                    
                    default:
                        return Math.Pow(10, exp) * num;
                }
            }
        }

        private void SkipWhiteSpaces()
        {
            while (PeekNextCharacter() is {} character && char.IsWhiteSpace(character))
                ConsumeNextCharacter();
        }

        public WithPositionInText<Token>? ConsumeNextToken()
        {
            SkipWhiteSpaces();
            PositionInText currentPositionInText = _positionInText;

            Token? token = PeekNextCharacter() switch
            {
                '"' => new Token.String(ConsumeNextString()),
                ':' => StepAnd(() => new Token.Column()),
                >= '0' and <= '9' => new Token.Number(ConsumeNextNumber()),

                null => null,

                _ => ConsumeNextLiteral() switch 
                {
                    "true" => new Token.True(),
                    "false" => new Token.False(),
                    
                    {} value => new Token.Literal(value) 
                }
            };

            if (token == null)
                return null;

            return new WithPositionInText<Token>
            {
                Position = currentPositionInText,
                Value = token
            };
        }

        public IEnumerator<WithPositionInText<Token>?> GetEnumerator()
        {
            WithPositionInText<Token>? token;
            while ((token = ConsumeNextToken()) != null)
                yield return token;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        // <variable_name> <value> 
        // <value> : <number> | <"string">
        // :<function_name> <value...>
        // $<variable_name>
        // #()
    }
}
