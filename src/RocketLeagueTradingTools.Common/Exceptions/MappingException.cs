namespace RocketLeagueTradingTools.Common.Exceptions;

public class MappingException : Exception
{
    private readonly object value;
    private readonly Type source;
    private readonly Type target;
    
    public MappingException(object value, Type source, Type target)
    {
        this.value = value;
        this.source = source;
        this.target = target;
    }

    public override string Message => $"Unable to map '{value}' value from {source.Name} to {target.Name}.";
}