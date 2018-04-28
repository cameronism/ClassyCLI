using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ClassyCLI.Test
{
    public class RunnerBuilderTest
    {
        private class Hello
        {
            public string World() => "and all who inhabit it";
        }

        private class Greetings
        {
            public string World() => "and all who inhabit it";
        }

        private object Invoke(IRunnerBuilder builder, string arg)
        {
            var result = builder
                .Run(new[] { "my.exe", arg })
                .Result;

            Assert.IsType<string>(result);
            Assert.Equal(result as string, "and all who inhabit it");
            return result;
        }

        [Fact]
        public void Run()
        {
            object instance = null;
            Func<Type, object> factory = type =>
            {
                var it = Activator.CreateInstance(type);
                instance = it;
                return it;
            };

            Invoke(Runner.Configure()
                    .WithInstanceProvider(factory)
                    .WithType<Hello>(),
                "hello.world");

            Assert.IsType<Hello>(instance);
        }

        [Fact]
        public void MultipleTypes()
        {
            Invoke(
                Runner.Configure()
                    .WithType<Hello>(),
                "hello.world");


            Invoke(
                Runner.Configure()
                    .WithType<Hello>()
                    .WithType<Greetings>(),
                "greetings.world");

            Invoke(
                Runner.Configure()
                    .WithType<Greetings>()
                    .WithType<Hello>(),
                "greetings.world");


            Invoke(
                Runner.Configure()
                    .WithTypes(new[] { typeof(Greetings) })
                    .WithTypes(new[] { typeof(Hello) }),
                "greetings.world");

            Invoke(
                Runner.Configure()
                    .WithTypes(new[] { typeof(Hello) })
                    .WithTypes(new[] { typeof(Greetings) }),
                "greetings.world");


            Invoke(
                Runner.Configure()
                    .WithType(typeof(Hello))
                    .WithTypes(new[] { typeof(Greetings) }),
                "greetings.world");

            Invoke(
                Runner.Configure()
                    .WithTypes(new[] { typeof(Greetings) })
                    .WithType(typeof(Hello)),
                "greetings.world");


            Invoke(
                Runner.Configure()
                    .WithType(typeof(Hello))
                    .WithTypes(new[] { typeof(Greetings) }),
                "hello.world");

            Invoke(
                Runner.Configure()
                    .WithTypes(new[] { typeof(Greetings) })
                    .WithType(typeof(Hello)),
                "hello.world");
        }
    }
}
