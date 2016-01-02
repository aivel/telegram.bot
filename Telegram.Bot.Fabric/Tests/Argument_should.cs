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
    class Argument_should
    {
        [Test]
        public void HaveEvents()
        {
            var argument = new Bot.Command.Argument(null, MessageType.TextMessage)
                .OnSuccess((botInstance) => { })
                .OnFail((botInstance) => { });

            Assert.NotNull(argument._onSuccessAction);
            Assert.NotNull(argument._onFailAction);
        }

        [Test]
        public void InvokeEvents()
        {
            const string successMarker = "success";
            const string failMarker = "fail";

            var results = new List<string>();
            var argument = new Bot.Command.Argument(null, MessageType.TextMessage);

            argument.OnSuccess((botInstance) => results.Add(successMarker))
                    .OnFail((botInstance) => results.Add(failMarker));

            argument.Success();
            argument.Fail();

            Assert.Contains(failMarker, results);
            Assert.Contains(successMarker, results);
        }

        [Test]
        public void HaveManyTriggers()
        {
            var argument = new Bot.Command.Argument(null, MessageType.TextMessage);
            var triggersCount = new Random().Next() % 49;

            for (var i = 0; i < triggersCount; i++)
            {
                argument.AddTrigger("/" + i);
            }

            Assert.IsTrue(argument.Triggers.Count == triggersCount);
        }

        [Test]
        public void BePossiblyTriggered()
        {
            var triggers = new object[]{"/command", 3};
            var argument = new Bot.Command.Argument(null, MessageType.TextMessage);

            argument.AddTrigger(triggers[0])
                    .AddTrigger(triggers[1]);

            foreach (var trigger in triggers)
            {
                Assert.IsTrue(argument.CanBeTriggered(trigger));
            }

            Assert.IsFalse(argument.CanBeTriggered("/another_command"));
        }

        [Test]
        public void HaveAction()
        {
            var argument = new Bot.Command.Argument(null, MessageType.TextMessage);

            argument.OnAction((botInstance, update) => true);

            Assert.IsNotNull(argument._onAction);
        }

        [Test]
        public void InvokeAction()
        {
            const string actionMarker = "action";
            var argument = new Bot.Command.Argument(null, MessageType.TextMessage);
            var results = new List<string>();

            argument.OnAction((botInstance, update) => {results.Add(actionMarker); return true; });

            argument.Act(null);

            Assert.Contains(actionMarker, results);
        }

        [Test]
        public void HaveType()
        {
            var argument = new Bot.Command.Argument(null, MessageType.TextMessage);

            Assert.IsTrue(argument.Type == MessageType.TextMessage);
        }
    }
}
