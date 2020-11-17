# [Kingdom Hearts Melody of Memory](index.md) Database

Most of the game logic is able to be manipulated using a SQLite database located in: `StreamingAssets\SQLite\Table.db`

## Documented Tables

| Table Name | Notes |
|---|---|
| GameSceneFlowTable | Defines menu layout, eg. Title>Tutorial>Main Screen, comments describe what each flow does, translation required however |
| MusicTable | Defines music track properties, like difficulty values, item drops and drop rates |
| PartyTable | Defines which characters are in each team, and if they're selectable by the player or not. |

## Noteworthy Details

* 3 regions are defined in MusicTable, "Japan", "World", and "Asia", with boolean values on each song defining if they are "included" in that region.
* This may be used for different translations of music like Hikari/Simple and Clean

## Example Edits

Using a SQLite DB editor, such as "DB Browser for SQLite" execute these SQL queries:

### Disabling first start tutorial

Title Screen goes directly to Main Menu:

```sql
UPDATE GameSceneFlowTable
SET NextGameSceneID = 10000034
WHERE GameSceneID=10000005 AND NextGameSceneID=10000077;
```

### Disabling promotion screen after playing a few songs

The trigger still happens automatically after finishing a few songs but drops you at the music list instead of the eShop promo screen:

```sql
UPDATE GameSceneFlowTable
SET NextGameSceneID = 10000000
WHERE GameSceneID=10000003 AND NextGameSceneID=11000000;
```

### Changing Team Classic to contain Sora, Roxas, and Ventus

Can be done with other characters, however you will need to assign them DeformedChara and Ability values in CharacterTable.

```sql
UPDATE PartyTable
SET Character1 = 110000000
WHERE PartyID=120000000;
UPDATE PartyTable
SET Character2 = 110000006
WHERE PartyID=120000000;
UPDATE PartyTable
SET Character3 = 110000009
WHERE PartyID=120000000;
```
