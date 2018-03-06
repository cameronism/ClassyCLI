using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ClassyCLI.Test")]
namespace ClassyCLI
{
    public class Runner
    {
        private static bool IsPublicIsh(Type t)
        {
            while (t.IsNested)
            {
                if (!t.IsNestedPublic)
                {
                    return false;
                }

                t = t.DeclaringType;
            }

            return t.IsPublic;
        }

        #region Run overloads
        public static object Run() => Run(Assembly.GetEntryAssembly());
        public static object Run(Assembly assembly) => Run(new[] { assembly });
        public static object Run(IEnumerable<Assembly> assemblies) =>
            Run(assemblies
                .SelectMany(a => a.GetTypes())
                .Where(IsPublicIsh));

        public static object Run(IEnumerable<Type> types) => Run(Environment.GetCommandLineArgs(), types);
        #endregion

        public static object Run(IEnumerable<string> arguments, IEnumerable<Type> types)
        {
            // may want to intelligently (or based on configuration) 
            // *not* skip the (assumed) assmebly name arg someday

            // definitely want case sensitivity configurable someday

            var invocation = new Invocation(stdout: Console.Out, stderr: Console.Error, ignoreCase: true);
            var result = invocation.Invoke(arguments.Skip(1), types);
            return result.Result;
            // return Run(arguments.Skip(1), types, ignoreCase: true);
        }
    }
}
