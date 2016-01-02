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
    class Session_should
    {
        internal readonly Update _upd = Update.FromString("{\"update_id\": 0,\"message\":{ \"message_id\":0,\"from\":{ \"id\":15263748,\"first_name\":\"Joe\",\"username\":\"joe_soap\"},\"chat\":{ \"id\":15263748,\"first_name\":\"Joe\",\"username\":\"joe_soap\", \"type\": 0},\"date\":1434495391,\"text\":\"/help\"}}");

        [Test]
        public void ProcessUpdateWithNoExceptionIfCommandHasNoArgs()
        {
            var command = new Bot.Command(null);
            var session = new Bot.Session(command);

            Assert.DoesNotThrow(() => session.ProceedUpdate(_upd));
        }

        [Test]
        public void ProcessUpdate()
        {
            const string successMarker = "success";
            const string failMarker = "fail";

            var results = new List<string>();
            var command = new Bot.Command(null);
            var session = new Bot.Session(command);

            Assert.DoesNotThrow(() => session.ProceedUpdate(_upd));

            command.AddArgument(argument => 
                argument.OnAction((botInstance, update) => 
                            {
                                results.Add(update.Message.Text);
                                return true;
                            } )
                        .OnSuccess((botInstance) => results.Add(successMarker))
                        .OnFail((botInstance) => results.Add(failMarker)));

            session.ProceedUpdate(_upd);

            Assert.IsTrue(2 == results.Count);
        }

        [Test]
        public void CallPreAct()
        {
            const string preActionMarker = "preAction";
            const string actionMarker = "action";

            var results = new List<string>();
            var command = new Bot.Command(null);
            var session = new Bot.Session(command);

            command.AddArgument(argument => argument.AddTrigger("\\help")
                                                    .OnAction((botInstance, upd) => { results.Add(actionMarker); return true; })
                                                    .OnPreAction((botInstance, chatId) =>
                                                    {
                                                        results.Add(preActionMarker);
                                                    }))
                   .AddArgument(argument => argument.AddTrigger("\\quit")
                                                    .OnAction((botInstance, upd) => { results.Add(actionMarker); return true; })
                                                    .OnPreAction((botInstance, chatId) =>
                                                    {
                                                        results.Add(preActionMarker);
                                                    }));

            Assert.IsTrue(results.Where(elem => elem == actionMarker).ToList().Count == 0);

            session.ProceedUpdate(_upd);

            Assert.Contains(actionMarker, results);
            Assert.IsTrue(results.Where(elem => elem == preActionMarker).ToList().Count == 1);
            Assert.IsTrue(results.Where(elem => elem == actionMarker).ToList().Count == 1);

            session.ProceedUpdate(_upd);
            
            Assert.IsTrue(results.Where(elem => elem == preActionMarker).ToList().Count == 1);
            Assert.IsTrue(results.Where(elem => elem == actionMarker).ToList().Count == 2);
        }
    }
}
