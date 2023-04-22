
# KeyDrop Giveaway Bot

KeyDrop Giveaway Bot is an automatic entry bot for the KeyDrop website's request-based giveaways. Its purpose is to enable users to enter these giveaways without having to manually enter their details each time. With this bot, users can simply input their desired giveaway and wait for the results. It streamlines the giveaway process and makes it easier for users to participate in these events.


## Installation 

Use [git](https://git-scm.com/downloads) to download source code.

## Building

Clone the project

```bash
  git clone https://github.com/ilkerogutcu/keydrop-giveaway-bot
```

Go to the project directory

```bash
  cd keydrop-giveaway-bot
```


Build Project
```bash
  dotnet build
```

Start Project

```bash
  dotnet run KeyDropGiveawayBot.exe
```

  
## Environment Variables

To run this project, you will need to add the following environment variables to your appsettings.json file.

`cookie`: Please set the cookie information from the "request headers" section of the request sent in the network tab of the DevTools panel, after opening the https://key-drop.com/tr/token link in your browser.

`userAgent`: Get your user-agent information from https://www.whatsmyua.info/


  
## Screenshots

![screenshot1](https://github.com/ilkerogutcu/keydrop-giveaway-bot/blob/master/images/Screenshot_1.png)

  
![screenshot2](https://github.com/ilkerogutcu/keydrop-giveaway-bot/blob/master/images/Screenshot_2.png)