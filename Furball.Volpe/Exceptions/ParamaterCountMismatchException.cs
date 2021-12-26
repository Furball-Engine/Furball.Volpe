using System;

namespace Furball.Volpe.Exceptions
{
    public class ParamaterCountMismatchException : VolpeException
    {
        public string FunctionName { get; }
        public int ExpectedParameterCount { get; }
        public int GivenParameterCount { get; }
        
        public ParamaterCountMismatchException(
            string functionName, int expectedParameterCount, int givenParameterCount, 
            PositionInText positionInText) 
            : base(positionInText)
        {
            FunctionName = functionName;
            ExpectedParameterCount = expectedParameterCount;
            GivenParameterCount = givenParameterCount;
        }

        public override string Description => $"too few parameters where given to the function \"{FunctionName}\" " +
                                              $"(expected {ExpectedParameterCount}, given {GivenParameterCount})";
    }
}