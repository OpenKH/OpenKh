# [Kingdom Hearts Birth By Sleep](index.md) - Events

Each event is a group of information, like the cutscene to play and the map where it will be played.

The events are described in `event/event_ve`, `event/event_aq` and `event/event_te`.

Every event has its own global unique identifier, which means that there should not be duplicate Event ID across the three files.

`World ID` and `Room ID` fields defines which maps will be loaded for that particular event: `arc/map/{WorldName}{Room}.arc`.

The `Event index` field defines which cutscene has to be loaded: `event/{WorldName}/{WorldName}_{EventIndex}.exa`.

The `WorldName` is calculated from `World ID` using the [world](worlds.md) table.

## Events table format

### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File identifier, always set to `1`
| 04     | int   | [Event entries](#event-entry) count

### Event entry

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Global event unique identifier
| 02     | short | Event index
| 04     | byte  | World ID
| 05     | byte  | Room ID
| 06     | short | Unknown
