using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Telegram.Bot.Fabric.Tests;

namespace Telegram.Bot.Fabric
{
    public class ResultingAction<Type, UpdateType> where Type : class
    {
        // TODO: add ArgumentTypeMismatch, WrongArgument, Fail
        internal Action<Bot, long> _onPreAction;
        internal Action<Bot> _onSuccessAction;
        internal Action<Bot> _onFailAction;
        internal Func<Bot, UpdateType, bool> _onAction;
        public Bot BotInstance;

        public ResultingAction(Bot botInstance)
        {
            BotInstance = botInstance;
        }

        public Type OnPreAction(Action<Bot, long> action)
        {
            _onPreAction = action;

            return this as Type;
        }

        public Type OnSuccess(Action<Bot> action)
        {
            _onSuccessAction = action;

            return this as Type;
        }

        public Type OnFail(Action<Bot> action)
        {
            _onFailAction = action;

            return this as Type;
        }

        public Type OnAction(Func<Bot, UpdateType, bool> func)
        {
            _onAction = func;

            return this as Type;
        }

        public void PreAct(long chatId)
        {
            _onPreAction?.Invoke(BotInstance, chatId);
        }

        public void Success()
        {
            _onSuccessAction?.Invoke(BotInstance);
        }

        public void Fail()
        {
            _onFailAction?.Invoke(BotInstance);
        }

        public bool Act(UpdateType update)
        {
            var invoke = _onAction?.Invoke(BotInstance, update);

            return invoke != null && (bool) invoke;
        }
    }
}
