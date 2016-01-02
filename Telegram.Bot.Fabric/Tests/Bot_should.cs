using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using NUnit.Framework;
using Telegram.Bot.Types;

namespace Telegram.Bot.Fabric.Tests
{
    [TestFixture]
    class Bot_should
    {
        private string _token = "";

        [Test]
        public void BeCreatedWithApi()
        {
            var bot = new Bot(_token);

            Assert.NotNull(bot.ApiProxy);
        }

        [Test]
        public void AddTextCommand()
        {
            var bot = new Bot(_token);

            bot.AddTextCommand(command => command);

            Assert.IsTrue(bot.Commands.Count == 1);
        }

        [Test]
        public void AddManyTextCommands()
        {
            var bot = new Bot(_token);
            var commandsCount = new Random().Next() % 49;

            for (var i = 0; i < commandsCount; i++)
            {
                bot.AddTextCommand(command => command);
            }

            Assert.IsTrue(bot.Commands.Count == commandsCount);
        }

        [Test]
        public void AddTextCommandWithArguments()
        {
            var bot = new Bot(_token);
            
            bot.AddTextCommand(command => 
                    command.AddArgument(MessageType.TextMessage, 
                             argument => argument.OnAction((botInstance, update) => true)
                                                 .OnSuccess((botInstance) => {})
                                                 .OnFail((botInstance) => {})
                                                 .AddTrigger("/help")
                                                 .AddTrigger("?"))
                .OnSuccess((botInstance) => {})
                .OnFail((botInstance) => {})
                .OnAction((botInstance, update) => true));

            var testCommand = bot.Commands.First();

            Assert.IsNotNull(testCommand);
            Assert.IsTrue(testCommand.Arguments.Count == 1);
            Assert.IsTrue(testCommand.Arguments.First().Triggers.Count == 2);
        }

        [Test]
        public void ProcessUpdate()
        {
            var actionMarker = "action";
            var upd = Update.FromString("{\"update_id\": 0,\"message\":{ \"message_id\":0,\"from\":{ \"id\":15263748,\"first_name\":\"Joe\",\"username\":\"joe_soap\"},\"chat\":{ \"id\":15263748,\"first_name\":\"Joe\",\"username\":\"joe_soap\", \"type\": 0},\"date\":1434495391,\"text\":\"/help\"}}");

            var actionResultMarker = actionMarker + upd;

            var bot = new Bot(_token);
            var results = new List<string>();

            bot.AddTextCommand(command => 
                command.AddArgument(MessageType.TextMessage, 
                        argument => argument.AddTrigger("/help")
                                            .OnAction((botInstance, update) => { results.Add(actionResultMarker); return true; }))
                       .AddArgument(MessageType.TextMessage,
                        argument => argument.AddTrigger("/comment")
                                            .OnAction((botInstance, update) => { results.Add(actionResultMarker); return true; })
                        ));

            bot.ProcessUpdate(upd);

            Assert.Contains(actionResultMarker, results);
            Assert.IsTrue(results.Count == 1);
        }

        [Test]
        public void Work()
        {
            const string actionMarker = "action";
            const string preActionMarker = "preAction";

            var bot = new Bot("<YOUR-TOKEN>");
            var results = new List<string>();

            bot.AddTextCommand(command => command
                .AddArgument(argument => argument.AddTrigger("/help")
                    .OnAction((botInstance, upd) =>
                    {
                        results.Add(actionMarker);
                        botInstance.ApiProxy.SendTextMessage(upd.Message.Chat.Id, "response: help");

                        return true;
                    })
                    .OnPreAction((botInstance, chatId) =>
                    {
                        results.Add(preActionMarker);
                        botInstance.ApiProxy.SendTextMessage(chatId, "Pre > response: help");
                    }))
                .AddArgument(argument => argument.AddTrigger("/quit")
                    .OnAction((botInstance, upd) =>
                    {
                        results.Add(actionMarker);
                        botInstance.ApiProxy.SendTextMessage(upd.Message.Chat.Id, "response: quit");

                        return true;
                    })
                    .OnPreAction((botInstance, chatId) =>
                    {
                        results.Add(preActionMarker);
                        botInstance.ApiProxy.SendTextMessage(chatId, "Pre > response: quit");
                    })));

            bot.ProcessUpdates();
        }
    }
}
