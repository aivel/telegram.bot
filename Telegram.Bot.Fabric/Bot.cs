using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace Telegram.Bot.Fabric
{
    class Bot
    {
        internal class Command: ResultingAction<Command, Update>
        {
            internal class Argument: ResultingAction<Argument, Update>
            {
                internal readonly IList<object> Triggers;
                internal readonly Bot BotInstance;
                internal readonly MessageType Type;

                public bool IsTriggerExactMatchingExpected => Triggers.Count > 0;

                public Argument(Bot botInstance, MessageType type) : base(botInstance)
                {
                    Triggers = new List<object>();
                    BotInstance = botInstance;
                    Type = type;
                }

                public Argument AddTrigger(object newTrigger)
                {
                    Triggers.Add(newTrigger);

                    return this;
                }

                public bool CanBeTriggered(object trigger)
                {
                    return Triggers.Contains(trigger);
                }

                public bool IsTriggered(object trigger, MessageType messageType)
                {
                    return (IsTriggerExactMatchingExpected && CanBeTriggered(trigger)) || (!IsTriggerExactMatchingExpected && messageType == Type);
                }
            }

            public readonly IList<Argument> Arguments;
            private readonly Bot BotInstance;

            public Command(Bot botInstance) : base(botInstance)
            {
                Arguments = new List<Argument>();
                BotInstance = botInstance;
            }

            public Command AddArgument(Func<Argument, Argument> applicator)
            {
                return AddArgument(MessageType.TextMessage, applicator);
            }

            public Command AddArgument(MessageType argumentType, Func<Argument, Argument> applicator)
            {
                var argument = new Argument(BotInstance, argumentType);

                applicator(argument);

                Arguments.Add(argument);

                return this;
            }
        }

        internal class Session
        {
            internal Command CurrentCommand;
            internal int CurrentStep;
            internal Command.Argument CurrentArgument => IsFinished ? null : CurrentCommand.Arguments[CurrentStep];
            public bool IsFinished => CurrentStep < 0 || CurrentStep >= CurrentCommand.Arguments.Count;

            public Session(int currentStep, Command currentCommand)
            {
                CurrentStep = currentStep;
                CurrentCommand = currentCommand;
            }

            public Session(Command currentCommand)
            {
                CurrentStep = 0;
                CurrentCommand = currentCommand;
            }

            public void ProceedUpdate(Update update)
            {
                if (CurrentArgument == null) return; // IsFinished

                var isTriggered = CurrentArgument.IsTriggered(GetMatchingMessageAttachment(update), update.Message.Type);

                if (isTriggered)
                {
                    if (CurrentArgument.Act(update))
                    {
                        CurrentArgument.Success();
                        CurrentStep++;

                        CurrentArgument?.PreAct(update.Message.Chat.Id);
                    }
                    else
                    {
                        CurrentArgument.Fail();
                        CurrentArgument.PreAct(update.Message.Chat.Id);
                    }
                }
            }
        }

        internal readonly Api ApiProxy;
        internal readonly IList<Command> Commands;
        internal readonly Dictionary<long, Session> Sessions;

        public Bot(string token)
        {
            ApiProxy = new Api(token);
            Commands = new List<Command>();
            Sessions = new Dictionary<long, Session>();
        }

        public Bot AddTextCommand(Func<Command, Command> applicator)
        {
            var command = new Command(this);

            applicator(command);

            Commands.Add(command);

            return this;
        }

        private static object GetMatchingMessageAttachment(Update update)
        {
            object matchingObject;

            switch (update.Message.Type)
            {
                case MessageType.TextMessage:
                    matchingObject = update.Message.Text;
                    break;
                case MessageType.PhotoMessage:
                    matchingObject = update.Message.Photo;
                    break;
                case MessageType.AudioMessage:
                    matchingObject = update.Message.Audio;
                    break;
                case MessageType.VideoMessage:
                    matchingObject = update.Message.Video;
                    break;
                case MessageType.VoiceMessage:
                    matchingObject = update.Message.Voice;
                    break;
                case MessageType.DocumentMessage:
                    matchingObject = update.Message.Document;
                    break;
                case MessageType.StickerMessage:
                    matchingObject = update.Message.Sticker;
                    break;
                case MessageType.LocationMessage:
                    matchingObject = update.Message.Location;
                    break;
                case MessageType.ContactMessage:
                    matchingObject = update.Message.Contact;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return matchingObject;
        }

        public void ProcessUpdate(Update update)
        {
            var chatId = update.Message.Chat.Id;
            Session userSession;

            if (!Sessions.TryGetValue(chatId, out userSession))
            {
                Command currentCommand = null;

                foreach (var command in Commands)
                {
                    var argument = command.Arguments.First();

                    try
                    {
                        if (update.Message.Type != argument.Type) continue;
                    }
                    catch (FormatException)
                    {
                        continue;
                    }

                    var matchingObject = GetMatchingMessageAttachment(update);

                    if (!argument.IsTriggered(matchingObject, update.Message.Type)) continue;

                    currentCommand = command;
                    break;
                }

                if (currentCommand == null)
                {
                    //throw new InstanceNotFoundException();
                    // fine, just return
                    return;
                }

                userSession = new Session(currentCommand);
                Sessions.Add(chatId, userSession);
            }

            // TODO: Parallel threads
            userSession.ProceedUpdate(update);

            if (userSession.IsFinished)
            {
                Sessions.Remove(chatId);
            }
        }

        public void ProcessUpdates()
        {
            var offest = 0;

            while (true)
            {
                var updates = ApiProxy.GetUpdates(offest).Result;
                var offsetHasChanged = false;

                foreach (var update in updates)
                {
                    ProcessUpdate(update);

                    if (update.Id >= offest)
                    {
                        offest = update.Id;
                        offsetHasChanged = true;
                    }
                }

                if (offsetHasChanged)
                {
                    offest += 1;
                }
            }
        }
    }
}
