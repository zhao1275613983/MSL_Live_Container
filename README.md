# MSL Live Container

Live container framework for Stoneshard ModShardLauncher mods.

This library mod adds reusable live-container parent objects and helper scripts.
Concrete container mods can inherit from these objects so container contents stay
instanced while still using the game's save/load flow.

## What This Provides

The library adds three parent objects:

- `o_inv_live_container_parent` - inventory item parent. Inherits the game's
  moneybag/casket behavior and adds live-container state.
- `o_live_container_holder_parent` - hidden GUI holder. Closed containers keep
  their contents here as live item instances.
- `o_container_live_container_parent` - visible opened-container panel parent.
  Closing this panel moves items back to the hidden holder and syncs `lootList`.

It also adds helper scripts named `scr_live_container_*` for opening, syncing,
loading from `lootList`, moving between holder and visible panel, item filtering,
and building custom cell layouts.

## Load Order

Container mods should be loaded after this library. The addon does not reference
this DLL directly; it relies on the objects and GML scripts patched into
`data.win` by this mod.

A simple convention is to name addons like `Live Containers - My Container` so
they sort after `Live Containers` in ModShardLauncher.

## Creating a Container Mod

Use `MSL_Live_Container_Glowing_Relic_Casket` as a complete example. A minimal
addon usually needs the pieces below.

### 1. Add Concrete Objects

Create your own inventory item, optional loot/drop object, hidden holder, and
opened panel. Use unique names for every object and script.

```csharp
Msl.AddObject(
    name: "o_inv_my_live_container",
    spriteName: "s_inv_my_live_container",
    parentName: "o_inv_live_container_parent",
    isVisible: true,
    isAwake: true,
    collisionShapeFlags: CollisionShapeFlags.Circle);

Msl.AddObject(
    name: "o_loot_my_live_container",
    spriteName: "s_loot_my_live_container",
    parentName: "o_loot_backpack_parent",
    isVisible: true,
    isAwake: true,
    collisionShapeFlags: CollisionShapeFlags.Circle);

Msl.AddObject(
    name: "o_my_live_container_holder",
    spriteName: "s_point",
    parentName: "o_live_container_holder_parent",
    isVisible: false,
    isAwake: true,
    collisionShapeFlags: CollisionShapeFlags.Box);

Msl.AddObject(
    name: "o_container_my_live_container",
    spriteName: "s_container",
    parentName: "o_container_live_container_parent",
    isVisible: true,
    isAwake: true,
    collisionShapeFlags: CollisionShapeFlags.Box);
```

The hidden holder is per item instance, so multiple live containers do not share
contents unless your addon explicitly points them at the same holder.

### 2. Configure The Inventory Item

In the inventory object's Create event, call `event_inherited()` first and then
set the live-container fields.

```gml
event_inherited()

scr_consum_atr("my_live_container")
live_container = true
live_container_holder_type = __asset_get_index("o_my_live_container_holder")
container_type = __asset_get_index("o_container_my_live_container")
contentType = 4769
live_container_cells_data = scr_my_live_container_cells_data()
live_container_accept_script = __asset_get_index("scr_my_live_container_accepts_item")

name = "My Live Container"
idName = "my_live_container"
if variable_instance_exists(id, "data")
{
    ds_map_replace(data, "idName", "my_live_container")
    ds_map_replace(data, "Name", "My Live Container")
}
```

`contentType = 4769` matches the normal bag/casket item content type used by the
current library patches. Other content types may need extra vanilla patches.

### 3. Define Layout Data

Layout entries are:

```text
[columns, rows, contentType, offsetX, offsetY]
```

`offsetX` and `offsetY` are optional and default to `0`. The same layout is used
for the opened panel and the hidden holder, which keeps visible capacity and
closed-container capacity in sync.

```gml
function scr_my_live_container_cells_data()
{
    return [[7, 10, 4769, 0, 0]]
}

function scr_my_live_container_cell_param()
{
    return scr_live_container_layout_to_cell_param(scr_my_live_container_cells_data())
}
```

If you change the container size later, update this one layout script first.

### 4. Build The Opened Panel

The opened panel object should inherit the parent behavior, create the usual
buttons/container GUI, and then call `scr_live_container_build_layout`.

```gml
event_inherited()

closeButton = scr_adaptiveCloseButtonCreate(id, (depth - 1), 229, 3)
with (closeButton)
    drawHover = true

getbutton = scr_adaptiveTakeAllButtonCreate(id, (depth - 1), 230, 27)
with (getbutton)
    owner = other.id

itemsContainer = scr_guiCreateContainer(id, o_guiContainerEmpty, depth, adaptiveOffsetX, adaptiveOffsetY)
scr_live_container_build_layout(id, true)
```

The button offsets are sprite/layout specific. Copy them from a vanilla panel or
from the glowing relic casket addon and adjust for your panel art.

### 5. Register Events And Scripts

Add your GML events and helper scripts through MSL. At minimum, a concrete
container normally registers the inventory Create event, the opened panel
User/Other event that builds the GUI, and the layout/acceptance scripts.

```csharp
Msl.AddNewEvent(
    "o_inv_my_live_container",
    Code("gml_Object_o_inv_my_live_container_Create_0.gml"),
    EventType.Create,
    0);

Msl.AddNewEvent(
    "o_container_my_live_container",
    Code("gml_Object_o_container_my_live_container_Other_10.gml"),
    EventType.Other,
    10);

Msl.AddFunction(Code("scr_my_live_container_cells_data.gml"), "scr_my_live_container_cells_data");
Msl.AddFunction(Code("scr_my_live_container_cell_param.gml"), "scr_my_live_container_cell_param");
Msl.AddFunction(Code("scr_my_live_container_accepts_item.gml"), "scr_my_live_container_accepts_item");
```

Add more events as needed for custom drawing, loot-object initialization,
localization/data registration, sounds, or debug hotkeys.

### 6. Patch Cell Parameters

Patch `scr_inventory_get_cell_param` so the game knows the real size of your
closed inventory item and loot item.

```csharp
Msl.LoadGML("gml_GlobalScript_scr_inventory_get_cell_param")
    .MatchFrom(@"        default:
            _cellsDataArray = [[7, 5, 4769]]")
    .ReplaceBy(@"        case o_inv_my_live_container:
        case o_loot_my_live_container:
            _cellsDataArray = scr_my_live_container_cell_param()
            break
        default:
            _cellsDataArray = [[7, 5, 4769]]")
    .Save();
```

Without this patch, closed-container insertion and capacity checks will use the
vanilla default layout instead of your custom one.

### 7. Filter Accepted Items

Set `live_container_accept_script` to a script that receives the item instance
or object and returns `true` or `false`.

```gml
function scr_my_live_container_accepts_item(argument0)
{
    if (!instance_exists(argument0))
        return false

    // Replace this with your own rule.
    return true
}
```

The library calls this through `scr_live_container_accepts_item(container, item)`.
The same filter is used for quick transfer, open-panel insertion, and dragging
an item onto a closed live container.

### 8. Optional Positioning Patch

If your opened panel uses a custom sprite height or a layout taller than the
vanilla panel, patch `scr_adaptiveMenusPositionUpdate` for your panel object and
use `scr_live_container_panel_height(id)` when calculating vertical placement.

The glowing relic casket addon has an example of this because its panel is taller
than the vanilla `s_container` sprite.

### 9. Register Item Data And Resources

The library does not create item table rows, localization, sprites, sounds, or
loot rules for concrete items. Your addon still needs to add or clone:

- inventory and loot sprites;
- optional custom opened-panel sprite;
- pickup/drop/open/close sounds if you do not want vanilla moneybag sounds;
- item data and localization (`Name`, `desc`, category, tags, color, etc.);
- any debug hotkey, recipe, vendor, quest reward, or loot-table entry that gives
  the item to the player.

## Lifecycle Notes

- Opening calls `scr_live_container_open(item)`, loads `lootList` into the hidden
  holder if needed, creates the visible panel, then moves live item instances to
  that panel.
- Closing the panel moves items back to the hidden holder and saves them into the
  item's `lootList`.
- Saving calls `scr_live_container_sync_all()`, so live contents are written back
  before the game saves inventory data.
- Destroying a live-container item syncs and destroys its hidden holder.
- Contents remain live while the owning item instance exists and has been loaded.

## Build

The project targets `net6.0-windows` and references ModShardLauncher assemblies
from `J:\msl`.

```powershell
dotnet build .\LiveContainers.csproj -c Release
```

To pack an `.sml`, use the workspace packer from the development workspace:

```powershell
.\tools\build-msl-mod.ps1 -Project .\outputs\LiveContainersMSL\LiveContainers.csproj -NoInstall
```
