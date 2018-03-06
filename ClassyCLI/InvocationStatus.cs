namespace ClassyCLI
{
    internal enum InvocationStatus
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
    }
}