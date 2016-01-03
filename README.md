## Telegram Bot Fabric Library

C# library for buiding Telegram Bots using Telegram API (https://core.telegram.org/bots/api)

## Usage

```C#
public void startEchoBot()
{
    var bot = new Telegram.Bot.Fabric.Bot("your Telegram API token");

    bot.AddTextCommand(command => 
        command.AddArgument(argument => 
                argument.OnAction((botInstance, upd) =>
                {
                    botInstance.ApiProxy.SendTextMessage(upd.Message.Chat.Id, "Re: " + upd.Message.Text);
                    return true;
                })));
            
    bot.ProcessUpdates();
}
```

## Installation

Install as [NuGet package](https://www.nuget.org/packages/Telegram.Bot.Fabric/):

    Install-Package Telegram.Bot.Fabric
    
## API Coverage

There are functions for all available API methods. (2015-11-17)
Missing: [Making requests when getting updates](https://core.telegram.org/bots/api#making-requests-when-getting-updates)
