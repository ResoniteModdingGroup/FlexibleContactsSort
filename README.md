Flexible Contacts Sort
======================

A [MonkeyLoader](https://github.com/MonkeyModdingTroop/MonkeyLoader) mod for [Resonite](https://resonite.com/) that sorts contacts Better™ and to your liking.

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
Sort Order can be changed to liking. Ordering of friends can additionally include sorting by whether
a contact is in a world you can just join.

0. Neos Bot
1. Unread messages and pinned contacts
2. Incoming Friend Requests
3. Online status
   1. Online Friends
   2. Away Friends
   3. Busy Friends
   
4. Sent Requests (background color changed from gray to yellow!)
5. Offline Friends
6. Search results
7. Remaining ties broken by username alphabetical order
