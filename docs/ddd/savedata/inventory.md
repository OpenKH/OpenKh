# [Kingdom Hearts Dream Drop Distance](../index.md) - Inventory

Every entry is stored as 0s. Whenever the player obtains an item ingame, the item's specific location is updated with the item id.

Eg: The address where Sora's Ultima Weapon is will be "00 00". Whenever you obtain it, it will change to "08 02".

For items that can stack, 2 additional bytes are used to store the amount of the item the player holds.

| Example     | Description                                             |
| ----------- | ------------------------------------------------------- |
| 00 00 00 00 | Item unobtained. Not shown in menu.                     |
| XX XX 00 00 | Item obtained. Is shown in menu and the player holds 0. |
| XX XX YY Y  | Item obtained. Is shown in menu and the player holds Y. |

Items are contiguous, grouped by category. The whole list is in [Items](../dictionary/items.md) and it is ordered as follows:

| Category      | Description           |
| ------------- | --------------------- |
| Keyblades     | Unique (2 bytes each) |
| Key Items     | Unique (2 bytes each) |
| Recipes       | Unique (2 bytes each) |
| Glossaries    | Unique (2 bytes each) |
| Mementos      | Unique (2 bytes each) |
| Unknowns (10) | Unique (2 bytes each) |
| Dream Pieces  | Stacks (4 bytes each) |
| Treats        | Stacks (4 bytes each) |
| Training Toys | Stacks (4 bytes each) |