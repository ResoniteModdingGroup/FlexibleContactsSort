Flexible Contacts Sort
======================

A [MonkeyLoader](https://github.com/MonkeyModdingTroop/MonkeyLoader) mod for [Resonite](https://resonite.com/) that sorts contacts Better™ and to your liking, including pinning your favorites to the top.

Semi-relevant Resonite issue: [#41](https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/41).

## Sorting Order
I've made a few noteworthy changes to the sorting order:
- No longer sorts by most recent message timestamp
- Incoming friend requests are now the first category, preceding Online friends
- Neos Bot is now forced to the top of the list
- You can pin contacts to always appear at the top of the list, regardless of status
- Sent Requests are separated from Offline friends, and have a yellow background color

### Vanilla Sort
1. Friends with unread messages
2. Ties broken by online status
   1. Online Friends
   2. Incoming Friend Requests
   3. Away Friends
   4. Busy Friends
   5. Offline Friends and Sent Requests
3. Further ties broken by most recent message
4. Even further ties broken by username alphabetical order

### Default Modded Sort
Sort Order can be changed to liking - this is just the default:

0. Your Account
1. Resonite Bot
2. Unread messages
3. Pinned contacts
3. Incoming contacts requests
4. Contacts in joinable sessions
5. Online status
   1. Online
   2. Away
   3. Busy
6. Headless hosts
7. Sent Requests (background color changed from gray to yellow!)
8. Offline Friends
9. Search results
10. Remaining ties broken by Username+UserId alphabetical order
