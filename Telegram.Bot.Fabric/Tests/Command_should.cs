using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Telegram.Bot.Types;

namespace Telegram.Bot.Fabric.Tests
{
    [TestFixture]
    class Command_should
    {
        private string _token = "";

        [Test]
        public void HaveEvents()
        {
            var command = new Bot.Command(null);

            command.OnSuccess((botInstance) => {})
                   .OnFail((botInstance) => {});

            Assert.NotNull(command._onSuccessAction);
            Assert.NotNull(command._onFailAction);
        }

        [Test]
        public void InvokeEvents()
        {
            const string successMarker = "success";
            const string failMarker = "fail";

            var results = new List<string>();
            var command = new Bot.Command(null);

            command.OnSuccess((botInstance) => results.Add(successMarker))
                   .OnFail((botInstance) => results.Add(failMarker));

            command.Success();
            command.Fail();

            Assert.Contains(failMarker, results);
            Assert.Contains(successMarker, results);
        }

        [Test]
        public void HaveOneArgument()
        {
            var command = new Bot.Command(null)
                .AddArgument(MessageType.TextMessage, argument => argument);

            Assert.IsTrue(command.Arguments.Count == 1);
        }

        [Test]
        public void HaveManyArguments()
        {
            var command = new Bot.Command(null);
            var argumentsCount = new Random().Next() % 49;

            for (var i = 0; i < argumentsCount; i++)
            {
                command.AddArgument(MessageType.TextMessage, argument => argument);
            }

            Assert.IsTrue(command.Arguments.Count == argumentsCount);
        }
    }
}
