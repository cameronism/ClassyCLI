namespace ClassyCLI
{
    public enum InvocationStatus
    {
        MethodInvoked,
        MethodFaulted,
        NoMethodSpecified,
        NoMethodFound,
        Help,
        Complete,
        ArgumentMissing,
        ArgumentConversionFailed,
        BashCompletionScript,
        PowerShellCompletionScript,
        InstanceCreationFailed,
        UnknownParameter,
        ArgumentValidationFailed,
    }
}