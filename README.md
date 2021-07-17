# MasterBot
## **[Add the bot to your server](https://discord.com/api/oauth2/authorize?client_id=855773619174768641&permissions=8&scope=bot)**
# About the bot
This bot includes many features that could be used by both big gaming communities and smaller "friends" servers. I began developing this bot mid June 2021.
**Master Bot is currently in development, see the current process below.**

## List of features (will be expanded as I come up with ideas)
- [x] Auto-roles & ranks
- [ ] Moderation:
    - [ ] Banned words (deleting the message, giving a warning and banning the user upon going over the warning limit)
    - [x] Purging (deleting) a set amount of messages 
    - [ ] Deleting links
- [x] **LFG**
- [x] Activity Checks
- [x] Music player

The items in **bold** are not yet on production server but are finished and are waiting to be pushed.
## List of commands so far and their description:
* ### Configuration commands: (Only administrators can use these)
    * #### prefix:
    Changes the prefix used on the server to call the bot commands. E.g. an admin would use "!command" by default, but after sending "!prefix ." the prefix changes to ".", so one would use ".command".

    * #### ranks:
    Displays all the ranks available on the server. (Fetched from the database)
    * #### delete-rank:
    Deletes a specified rank from the server and the database. Use: "!delete-rank rank" removes 'rank' from the database and deletes 'rank' from all the users who previously had it.
    * #### add-rank:
    Adds the specified rank to the server and the database. Use: "!add-rank rank" adds 'rank' to the database and allows users to add it to their ranks.
    * #### autoroles:
    Displays the list of the roles given to all users when they join the server. (Fetched from the database)
    * #### delete-autorole:
    Deletes the specified autorole from the server. The role itself is not deleted but it is no longer given to users on server join. E.g. "!delete-autorole role"  will no longer add 'role' to users who join the server after it is called.
    * #### add-autorole:
    Adds the specified role to the list of autoroles. After the role is added, it is given to users who join the server. E.g. '!add-autorole role" will add 'role' to anyone who joins the server after this command is called.
    * #### adminchannel:
    Sets the channel as the admin channel. It has to be set up for activity-check features to work. E.g. '!adminchannel' sets the channel this is used in as the admin channel; '!adminchannel channel' sets 'channel' as the admin channel.

    * #### logging:
    Sets the logging option to the given value. Example: '!logging true' sets logging to true. Now will log all the bot activity to LogChannel.
    * #### logchannel:
    Sets the log channel as the logging channel. If command is used while logging is false, sets it to true automatically. Example: '!logchannel #log-channel' will set #log-channel as the default channel. If the channel is not given, then sets the channel the command was issued as the logging channel.  
    * #### inactivity-period:
    Sets the inactivity period in days. It's a must-set command. E.g. '!inactivity-period 10' sets the inactivity period to 10 days, so !activity-check will check on people who have not been active 10 days. Value cannot be less than 1 or greater than 14.

* ### General commands: (Anyone can call these)
    * #### info:
    Displays the information about the specified user in an embed: user avatar, user ID, discriminator (the numbers after the #), account creation date, server join date, assigned roles. If the user is not specified, displays the information about the caller.

    * #### server:
    Displays the information about the server: server avatar, server creation date, number of people on the server, number of people online at the moment.

    * #### rank:
    Adds (if the user does not already have the rank) or removes (if the user does have the rank) the specified rank from the callee. The argument could either be the rank name or the rank ID. Tip: ranks & roles should have identical names. Use: "!rank rank" will add 'rank' to the user's ranks.
    * #### help:
    Displays a paginator with descirptions of commands.

* ### Fun commands: (Anyone can call these)
    * #### meme (alias reddit):
    Sends a random post from the specified subreddit. If the subreddit is not specified, sends a random meme from /r/dankmemes. Use: "!reddit memes", "!meme", "!meme allthingsprotoss", "!reddit"

* ### Moderation commands: (Only people who can manage messages can use these):
    * #### purge:
    Deletes a specified number of messages from the channel it was called in. Use: "!purge 50" will remove 50 messages.
    * #### ban:
    Bans the mentioned user for a specified reason. E.g. `!ban @User [reason]`. Alias: `!b`.
    * #### unban:
    Unbans the user with the specified username. E.g. `!unban username`. Alias: `!ub`. 
    * #### kick:
    Kicks the mentioned user for a specified reason. E.g. `!kick @User [reason]`. Alias: `!k`.
    * #### add-banned-word:
    Adds the given word to the list of banned words. If a user sends a message containing this word, it will be deleted. E.g. `!add-banned-word [word]`.
    * #### remove-banned-word:
    Removes the given word from the list of banned words. E.g. `!remove-banned-word [word]`

* ### Music commands: (Anyone can call these)
    * #### join
    Makes the bot join the voice channel that the user is connected to.
    * #### disconnect
    Stops the music from playing and disconnects the bot from the voice channel.
    * #### play
    Searches YouTube for the specified keywords and plays the top result. If there is a track already playing, adds it to the queue.
    * #### skip
    If the track queue is not empty, skips ot the next track.
    * #### pause
    Pauses the current track.
    * #### resume
    Resumes the paused track, if there is one.

* ### LFG Commands: (Anyone can use these)
    * #### lfg-register:
    Registers the user. The user has to provide some information, namely: GameName, Rank and Region. The format of the command is: !lfg-register GameName Rank Region.

    * #### lfg:
    Adds user to the LFG database, mentions all the users on the server who fit the rating requirement set by the user and are also looking for game. Format:
        - !lfg Game(could use short name or full name) Mode (optional if Mode is not ranked)LowerRatingBoundary (optional if Mode is not ranked)UpperRatingBoundary
        - !lfg Game Mode remove ----- removes the user from the lfg database for that mode. 

* ### Activity checks: (Only users with Kick permissions can use these)
    * #### activity-check:
    Posts the message with users who have not messaged in `activity period`. Sends them a message upon reacting to which the message that has been posted is updated. The users who reacted to the DM are given a `grace period`, the end of which is noted in the modified message. As of now there are no auto-kick features. 