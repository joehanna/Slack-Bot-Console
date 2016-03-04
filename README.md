# Slack
A simple Slack integration library
 
# Building
 * Open in Visual Studio 2015 (currently requires 4.5.2)
 * Restore packages
 * Build Solution

# Simple usage
 * Open a commandline
 * Execute: 
 ```
SlackConsole/bin/debug/SlackConsole.exe {your slack bot token} 
 ```
 * Once connected, go to your Slack client and select the bot under "Direct Messages".
 * Type "echo blah".
 * The bot will reply with "ECHO: blah".

# Responders
The above example triggers the built in EchoResponder.

Simply implement the IMessageResponder interface to provider further integrations
