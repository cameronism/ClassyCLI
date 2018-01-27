# ClassyCLI

`ClassyCLI.Run()` provides an excellent command line interface to your existing methods and 
requires no extra boilerplate or configuration.  `ClassyCLI` also supports command line completion
for PowerShell, bash and zsh.

Minimal working example:

```C#
using System;
using ClassyCLI;

namespace Demo
{
	class Program
	{
		static void Main(string[] args)
		{
			ClassyCLI.Run();
		}
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
$ Demo hello world
Hello world!
$ Demo hello world classy
Hello classy!
$ Demo hello world -name Ron
Hello Ron!
```


## Why?

.NET is a highly productive environment, the command line is a highly productive environment, 
let's bring them together with as little ceremony as possible (as nicely as possible).

There are already [many awesome command line parsers for .NET](https://github.com/quozd/awesome-dotnet#cli);
`ClassyCLI` is different.  .NET (C# in particular) already requires 
plenty of boilerplate: classes, methods, return types, parameter types, names on all of them and (basically always) namespaces.
The goal of `ClassyCLI` is to provide an excellent command line interface based on the boilerplate you have to write anyway.
Your `Main` method really can be just `ClassyCLI.Run()` then 
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

TODO: 

- optional class or method names 
	- (sometimes) 
	- or just ignore class names and assume method name is first
- completion
- help
- help for incomplete params
- runme attribute
- marker interface
- deserialize stdin from json or csv (or maybe even xml)
- serialize return value to stdout as json or csv (or maybe that other one)
- async method support


## Configuration

`ClassyCLI` strives to do the right thing out of the box but can be customized if needed.

_all TODO_

- case senstivity
- unique prefix match vs full match
- named parameter marker character (sigil)
	default: `{ '-', '/', '@', '=' }`
- rename or disable completion command
- rename or disable help command
- default method and arguments if invoked with no parameters
	by default usage is printed
- explicitly specify command classes
	- by default all public methods of public classes of executing assembly are considered
	- if any classes found in previous step implement `ICommandLine` interface then
		only classes implementing `ICommandLine` will be considered
	- assemblies or types can be explicitly specified


## Completion

By default, a first argument of `--complete` is used to get completions for use in PowerShell, bash or zsh 
with minimal setup for your program.

_Shamelessly borrowed from [.NET CLI Tab Completion](https://github.com/dotnet/cli/blob/master/Documentation/general/tab-completion.md)_

PowerShell:

```powershell
# PowerShell parameter completion shim for your-program
Register-ArgumentCompleter -Native -CommandName your-program -ScriptBlock {
     param($commandName, $wordToComplete, $cursorPosition)
         your-program --complete --position $cursorPosition "$wordToComplete" | ForEach-Object {
            [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
         }
 }
```

bash:

```bash
#!/bin/bash
# bash parameter completion for the your-program CLI

_your_program_bash_complete()
{
  local word=${COMP_WORDS[COMP_CWORD]}
  local your_programPath=${COMP_WORDS[1]}

  local completions=("$(your-program --complete --position ${COMP_POINT} "${COMP_LINE}")")

  COMPREPLY=( $(compgen -W "$completions" -- "$word") )
}

complete -f -F _your_program_bash_complete your-program
```

By default a first argument of `--complete-powershell`, `--complete-bash`, `--complete-zsh` will output a snippet
suitable to append to your PowerShell profile, `.bashrc`, `.zshrc` respectively.


## Next

Before Publish:

- boolean value completion
- value completion for "collections of completables"
- use TypeConverter
- Task support
- --help
- print usage for insufficient args
- coherent class / namespace usage story
- coherent method name omission story
- minimal `ClassyCLI.Run()`
	- the right types
	- --help
	- --complete
- cleanup README


Soon:

- configuration / customization
- shell completion snippet generation
- (de)serialization support -- possibly in sibling package(s)


Someday:

- custom value provider


## License

Apache 2.0
