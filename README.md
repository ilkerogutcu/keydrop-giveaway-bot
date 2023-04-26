
# KeyDrop Giveaway Bot

KeyDrop Giveaway Bot is an request-based automatic join bot for the KeyDrop website's giveaways. Its purpose is to joinable users to join these giveaways without having to manually join their details each time. With this bot, users can simply input their desired giveaway and wait for the results. It streamlines the giveaway process and makes it easier for users to participate in these events.

## Installation 

Use [git](https://git-scm.com/downloads) to download source code.

## Building

Clone the project

```bash
  git clone https://github.com/ilkerogutcu/keydrop-giveaway-bot
```

Go to the project directory

```bash
  cd keydrop-giveaway-bot/src
```

Build Project
```bash
  dotnet build
```

Start Project

```bash
  dotnet run
```

  
## Environment Variables

To run this project, you will need to add the following environment variables to your appsettings.json file.

`sessionId`: While on the KeyDrop website, open the DevTools on your browser and navigate to the Application tab. Under the Storage section, expand the Cookies dropdown and find the "session_id". Replace the default value with the name of your "session_id" value.

`userAgent`: Get your user-agent information from https://www.whatsmyua.info/


## Screenshots

![screenshot1](https://github.com/ilkerogutcu/keydrop-giveaway-bot/blob/master/images/Screenshot_1.png)

  
![screenshot2](https://github.com/ilkerogutcu/keydrop-giveaway-bot/blob/master/images/Screenshot_3.png)

## Contributing

Thank you for your interest in contributing to this project! Contributions are welcome and appreciated.

If you have any questions or suggestions, please feel free to open an issue or submit a pull request. I will review your contributions as soon as possible.

Together, we can make this project better!
