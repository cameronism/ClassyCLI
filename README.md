[![NuGet](https://img.shields.io/nuget/v/ClassyCLI.svg)](https://www.nuget.org/packages/ClassyCLI)
[![Apache 2.0 License](https://img.shields.io/github/license/cameronism/ClassyCLI.svg)](https://github.com/cameronism/ClassyCLI/blob/master/LICENSE)

# ClassyCLI

```csharp
using System;

namespace Demo
{
    class Program
    {
        static void Main(string[] args) => ClassyCLI.Runner.Run();
    }

    public class Hello
    {
        public void World(string name = "world")
        {
            Console.WriteLine($"Hello {name}!");
        }
    }
}
```

Example usage:

```
$ demo hello.world
Hello world!
$ demo hello.world classy
Hello classy!
$ demo hello.world -name Ron
Hello Ron!
```

`ClassyCLI.Runner.Run()` provides an excellent command line interface to your existing methods and 
requires no extra boilerplate or configuration.  `ClassyCLI` also supports command line completion
for PowerShell, bash and zsh.

Use `ClassyCLI.Runner.Run()` for your main method and all public methods of public types will be easily callable from the command line.  Optionally, use one of the `ClassyCLI.Runner.Run()` overloads to specify which assemblies and/or types are callable.  Public instance and static methods are callable, instance methods should be specified as if they were static, e.g. `TypeName.MethodName`

**EARLY BUT FUNCTIONAL**

## Why?

.NET is a highly productive environment, the command line is a highly productive environment, 
let's bring them together with as little ceremony as possible (as nicely as possible).

There are already [many awesome command line parsers for .NET](https://github.com/quozd/awesome-dotnet#cli);
`ClassyCLI` is different.  .NET (C# in particular) already requires 
plenty of boilerplate: classes, methods, return types, parameter types, names on all of them and (basically always) namespaces.
The goal of `ClassyCLI` is to provide an excellent command line interface based on the boilerplate you have to write anyway.
Your `Main` method really can be just `ClassyCLI.Runner.Run()` then 
write plain old methods without special considerations for the command line.

One off console app "scripts", power user swiss army knives, and everything in between is supported by `ClassyCLI`.


## Features

- named or positional parameters
- optional / default params
- extensive parameter type support
	- `Stream` from stdin or filename
	- `TextReader` from stdin or filename
	- `FileInfo` from filename
	- `TextWriter` from stdout or filename (by default for safety filename must not exist yet)
	- primitive types
	- enums (supports completion of values)
	- Collections of the above
		- `int[]`
		- `List<string>`
		- `IEnumerable<DateTime>`
		- `IList<DayOfWeek>`
		- etc
		Collections are "greedy" when used positionally but not when used by name
- extensive return type support
- async method support





## Completion

By default, a first argument of `--complete` is used to get completions for use in PowerShell, bash or zsh.

Generate a suitable completion setup script with an argument of `--powershell-completion-script` or `--bash-completion-script`.  These will output a snippet suitable to append to (or include from) your PowerShell profile, `.bashrc`, `.zshrc` respectively.

_Shamelessly borrowed from [.NET CLI Tab Completion](https://github.com/dotnet/cli/blob/master/Documentation/general/tab-completion.md)_


Bash example:

```bash
$ dotnet /path/to/your/assembly --bash-completion-script soclassy
alias soclassy="dotnet /path/to/your/assembly"
_soclassy_bash_complete()
{
  local word=${COMP_WORDS[COMP_CWORD]}
  local soclassypath=${COMP_WORDS[1]}
  local completions=("$(dotnet /path/to/your/assembly --complete --position ${COMP_POINT} "${COMP_LINE}")")
  COMPREPLY=( $(compgen -W "$completions" -- "$word") )
}
complete -f -F _soclassy_bash_complete soclassy
```


PowerShell example:

```powershell
> dotnet /path/to/your/assembly --powershell-completion-script soclassy
function soclassy { dotnet /path/to/your/assembly $args }
Register-ArgumentCompleter -Native -CommandName soclassy -ScriptBlock {
  param($commandName, $wordToComplete, $cursorPosition)
  dotnet /path/to/your/assembly --complete --position $cursorPosition "$wordToComplete" | ForEach-Object {
    [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
  }
}
```

## Configuration

`ClassyCLI` strives to do the right thing out of the box but can be customized if needed (soon).

_all TODO_

- case senstivity
- unique prefix match vs full match
- named parameter marker character (sigil)
	default: `{ '-', '/', '@', '=' }`
- rename or disable completion commands
- rename or disable help command
- default method and arguments if invoked with no parameters
	by default usage is printed
- more flexibility to easily specify command classes
	- **already works** by default all public methods of public classes of executing assembly are considered
	- **already works** assemblies or types can be explicitly specified via `Run()` overloads
	- **TODO** if any classes found in previous step implement `ICommandLine` interface then
		only classes implementing `ICommandLine` will be considered

## TODO

Soon:

- CancellationToken parameter support -- automatically wired up to `ctrl-c`
- numeric enum story
- better flag enum support
- configuration / customization
- instance creation hooks / IoC support
- completion for file names (if shell doesn't do it for us?)
- `--zsh-completion-script`
- more detailed parameter help: optional, allows null, default, etc
- completion argument parsing of escaped quotes inside quoted strings
	- the "unix" shells and powershell handle this very differently
	- "regular" double and single quoted strings already work


Someday:


-   runme attribute
-   marker interface
-   deserialize stdin from json or csv (or maybe even xml) probably in sibling package(s)
-   serialize return value to stdout as json or csv (or maybe that other one)
-	custom completion value provider
-	custom parameter value provider  
	example use case: allow library user to supply a parameter from environment variable or config file



## License

Apache 2.0
