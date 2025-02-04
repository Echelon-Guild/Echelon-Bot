# Echelon-Bot

Echelon Bot is a guild bot for the guild Echelon on Icecrown.

It is primarily a World of Warcraft event scheduling service that integrated with Discord.

You will need several environment variables configured on the container.

AZURE_STORAGE_CONNECTION_STRING should contain a connection string to an Azure Storage Account that can run Tables.

DISCORD_TOKEN should have your bot token from the discord developer portal.

DISCORD_SERVER_ID should have the ID of the Discord server you want the bot to run on.

You also need a Battle.NET api client id and secret to use that module.

BATTLE_NET_CLIENT_ID

BATTLE_NET_CLIENT_SECRET

If running locally, add them to a launchSettings.json file in the /Properties folder. It should look like this:
```
{
  "profiles": {
    "Echelon-Bot": {
      "commandName": "Project"
    },
    "Container (Dockerfile)": {
      "commandName": "Docker",
      "environmentVariables": {
        "AZURE_STORAGE_CONNECTION_STRING": "",
        "DISCORD_TOKEN": "",
        "DISCORD_SERVER_ID": ""
        "BATTLE_NET_CLIENT_ID": ""
        "BATTLE_NET_CLIENT_SECRET": ""
      }
    }
  }
}
```
DO NOT check this launchSettings.json into source control. It's in the gitignore but I thought it was worth mentioning.

Make sure those environment variables are filled out appropriately or the bot cannot run.