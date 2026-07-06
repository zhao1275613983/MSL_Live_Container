using ModShardLauncher;
using ModShardLauncher.Mods;
using System.IO;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Util;

namespace LiveContainersMSL;

public class LiveContainers : Mod
{
    public override string Author => "Codex";
    public override string Name => "Live Containers";
    public override string Description => "Adds reusable live-container base classes whose closed contents remain instantiated.";
    public override string ShortDesc => "Reusable live container framework.";
    public override string Version => "0.1.0.0";
    public override string TargetVersion => "v0.13.2.0";

    public override void PatchMod()
    {
        AddObjects();
        AddFunctions();
        AddEvents();
        PatchContainerRules();
        PatchClosedContainerInsertion();
        PatchPersistence();
    }

    private static string Code(string fileName)
    {
        return fileName switch
        {
            "gml_Object_o_container_live_container_parent_Alarm_0.gml" => @"if (parent != noone)
    instance_activate_object(parent)
if (parent != noone && instance_exists(parent))
{
    var _list = -4
    var _stack = 0
    var _id = id
    var _holder = noone
    with (parent)
    {
        if scr_live_container_is_item(id)
        {
            _list = ds_map_find_value(data, ""lootList"")
            _holder = scr_live_container_get_holder(id)
            with (o_inv_slot)
            {
                if (owner == _id && (!select))
                {
                    if (object_index == o_inv_gold)
                        _stack += stack
                    else if (object_is_ancestor(object_index, o_inv_bag_parent) || object_is_ancestor(object_index, o_inv_casket_parent) || scr_live_container_is_item(id))
                        _stack += ds_map_find_value_ext(data, ""Stack"", 0)
                }
            }
            ds_map_replace(data, ""Stack"", _stack)
        }
    }
    if (_list != -4)
    {
        if instance_exists(_holder)
        {
            scr_live_container_move_items_between(id, _holder, false)
            with (parent)
            {
                container_id = _holder
            }
            scr_live_container_sync(parent)
        }
    }
}
instance_destroy()
",
            "gml_Object_o_container_live_container_parent_Create_0.gml" => @"event_inherited()
var _live_container_offset = scr_adaptiveMenusGetOffset(o_container)
adaptiveOffsetX = _live_container_offset[0]
adaptiveOffsetY = _live_container_offset[1]
live_container_panel = true
",
            "gml_Object_o_container_live_container_parent_Other_10.gml" => @"scr_adaptiveMenusPositionUpdate()
",
            "gml_Object_o_inv_live_container_parent_Create_0.gml" => @"event_inherited()
if __is_undefined(ds_map_find_value(data, ""lootList""))
    ds_map_add_list(data, ""lootList"", __dsDebuggerListCreate())
if __is_undefined(ds_map_find_value(data, ""Stack""))
    ds_map_add(data, ""Stack"", 0)
live_container = true
live_container_holder = -4
live_container_loaded = false
live_container_holder_type = __asset_get_index(""o_live_container_holder_parent"")
is_container = true
is_open = false
draw_full_ico = true
use_text = ds_list_find_value(global.context_menu, 1)
live_container_accept_script = -4
",
            "gml_Object_o_inv_live_container_parent_Destroy_0.gml" => @"if (variable_instance_exists(id, ""live_container_holder"") && instance_exists(live_container_holder))
{
    scr_live_container_sync(id)
    with (live_container_holder)
        instance_destroy()
}
event_inherited()
",
            "gml_Object_o_inv_live_container_parent_Mouse_5.gml" => @"if (owner.object_index == o_trade_inventory)
    scr_create_context_menu(""Buy"")
else
    scr_create_context_menu(""Use"", ""Drop"")
",
            "gml_Object_o_inv_live_container_parent_Other_24.gml" => @"container_id = -4
if ((!is_open) && (!(object_is_ancestor(owner.object_index, o_stash_inventory))) && owner.object_index != o_trade_inventory)
    scr_live_container_open(id)
",
            "gml_Object_o_live_container_holder_parent_Create_0.gml" => @"event_inherited()
live_container_holder = true
live_container_source = -4
active = false
contentType = 4769
parent = -4
depth = -20000
itemsContainer = -4
cellsContainer = -4
scr_guiVisibleUpdate(id, false)
scr_guiLayoutOffsetUpdate(id, -10000, -10000)
",
            "scr_live_container_bootstrap_all.gml" => @"function scr_live_container_bootstrap_all() //gml_Script_scr_live_container_bootstrap_all
{
    if (!instance_exists(o_inventory))
        return false
    if (!variable_global_exists(""guiBaseContainerHidden"") || !instance_exists(global.guiBaseContainerHidden))
        return false
    with (o_inv_live_container_parent)
    {
        if (instance_exists(owner) && owner == o_inventory.id)
            scr_live_container_load(id)
        else if (variable_instance_exists(id, ""live_container_holder"") && instance_exists(live_container_holder))
            scr_live_container_unload(id)
    }
    return true
}
",
            "scr_live_container_accepts_item.gml" => @"function scr_live_container_accepts_item(argument0, argument1) //gml_Script_scr_live_container_accepts_item
{
    if (!instance_exists(argument0))
        return true
    var _source = argument0
    if (variable_instance_exists(argument0, ""owner"") && instance_exists(argument0.owner))
        _source = argument0.owner
    if (object_is_ancestor(_source.object_index, __asset_get_index(""o_container_live_container_parent"")))
    {
        if (variable_instance_exists(_source, ""parent"") && instance_exists(_source.parent))
            _source = _source.parent
    }
    else if (object_is_ancestor(_source.object_index, __asset_get_index(""o_live_container_holder_parent"")))
    {
        if (variable_instance_exists(_source, ""live_container_source"") && instance_exists(_source.live_container_source))
            _source = _source.live_container_source
    }
    if (instance_exists(_source) && variable_instance_exists(_source, ""live_container_accept_script"") && _source.live_container_accept_script != -4)
        return script_execute(_source.live_container_accept_script, argument1)
    return true
}
",
            "scr_live_container_is_item.gml" => @"function scr_live_container_is_item(argument0) //gml_Script_scr_live_container_is_item
{
    if (!instance_exists(argument0))
        return false
    if (!variable_instance_exists(argument0, ""live_container""))
        return false
    return argument0.live_container
}
",
            "scr_live_container_layout_to_cell_param.gml" => @"function scr_live_container_layout_to_cell_param(argument0) //gml_Script_scr_live_container_layout_to_cell_param
{
    var _result = []
    for (var _i = 0; _i < array_length(argument0); _i++)
    {
        var _entry = argument0[_i]
        array_push(_result, [_entry[0], _entry[1], _entry[2]])
    }
    return _result;
}
",
            "scr_live_container_build_layout.gml" => @"function scr_live_container_build_layout(argument0, argument1) //gml_Script_scr_live_container_build_layout
{
    if (argument1 == undefined)
        argument1 = true
    if (!instance_exists(argument0))
        return false;
    var _layout = scr_live_container_get_layout_data(argument0)
    with (argument0)
    {
        if (!instance_exists(itemsContainer))
            itemsContainer = scr_guiCreateContainer(id, o_guiContainerEmpty, depth, 0, 0)
        cellsContainer = -4
        for (var _i = 0; _i < array_length(_layout); _i++)
        {
            var _entry = _layout[_i]
            var _columns = _entry[0]
            var _rows = _entry[1]
            var _content_type = _entry[2]
            var _offset_x = 0
            var _offset_y = 0
            if (array_length(_entry) > 3)
                _offset_x = _entry[3]
            if (array_length(_entry) > 4)
                _offset_y = _entry[4]
            var _cells_container = scr_inventory_container_create(itemsContainer, _columns, _content_type, _offset_x, _offset_y)
            if (_i == 0)
                cellsContainer = _cells_container
            scr_inventory_container_cells_add(id, _cells_container, _rows, argument1)
        }
        scr_guiVisibleUpdate(id, argument1)
    }
    return true;
}
",
            "scr_live_container_get_holder.gml" => @"function scr_live_container_get_holder(argument0) //gml_Script_scr_live_container_get_holder
{
    if (!scr_live_container_is_item(argument0))
        return noone
    if (!variable_global_exists(""guiBaseContainerHidden"") || !instance_exists(global.guiBaseContainerHidden))
        return noone
    var _holder = noone
    with (argument0)
    {
        if (variable_instance_exists(id, ""live_container_holder"") && instance_exists(live_container_holder))
            _holder = live_container_holder
        else
        {
            var _holder_type = live_container_holder_type
            if (_holder_type == -1)
                _holder_type = __asset_get_index(""o_live_container_holder_parent"")
            live_container_holder = scr_guiCreateContainer(global.guiBaseContainerHidden, _holder_type, -20000)
            _holder = live_container_holder
            with (_holder)
            {
                live_container_source = other.id
                parent = other.id
                scr_live_container_build_layout(id, false)
                scr_guiVisibleUpdate(id, false)
                scr_guiLayoutOffsetUpdate(id, -10000, -10000)
            }
        }
    }
    return _holder
}
",
            "scr_live_container_get_layout_data.gml" => @"function scr_live_container_get_layout_data(argument0) //gml_Script_scr_live_container_get_layout_data
{
    var _layout = [[7, 5, 4769, 0, 0]]
    if (!instance_exists(argument0))
        return _layout
    with (argument0)
    {
        if variable_instance_exists(id, ""live_container_cells_data"")
            _layout = live_container_cells_data
        else if (variable_instance_exists(id, ""parent"") && instance_exists(parent) && variable_instance_exists(parent, ""live_container_cells_data""))
            _layout = parent.live_container_cells_data
        else if (variable_instance_exists(id, ""live_container_source"") && instance_exists(live_container_source) && variable_instance_exists(live_container_source, ""live_container_cells_data""))
            _layout = live_container_source.live_container_cells_data
    }
    return _layout
}
",
            "scr_live_container_load.gml" => @"function scr_live_container_load(argument0) //gml_Script_scr_live_container_load
{
    if (!scr_live_container_is_item(argument0))
        return false
    var _holder = scr_live_container_get_holder(argument0)
    if (!instance_exists(_holder))
        return false
    var _loaded = false
    with (argument0)
        _loaded = live_container_loaded
    if _loaded
        return true
    var _list = -4
    with (argument0)
        _list = ds_map_find_value(data, ""lootList"")
    if (!__is_undefined(_list))
        scr_loadContainerContent(_list, _holder.object_index, -4, _holder, false, false)
    with (argument0)
        live_container_loaded = true
    scr_guiVisibleUpdate(_holder, false)
    scr_guiLayoutOffsetUpdate(_holder, -10000, -10000)
    return true
}
",
            "scr_live_container_open.gml" => @"function scr_live_container_open(argument0) //gml_Script_scr_live_container_open
{
    if (!scr_live_container_is_item(argument0))
        return false
    if (!instance_exists(argument0.owner))
        return false
    if (object_is_ancestor(argument0.owner.object_index, o_stash_inventory))
        return false
    if (argument0.owner.object_index == o_trade_inventory)
        return false
    if argument0.is_open
        return true
    scr_live_container_load(argument0)
    var _holder = scr_live_container_get_holder(argument0)
    if (!instance_exists(_holder))
        return false
    var _panel = noone
    with (argument0)
    {
        container_id = -4
        container_id = scr_container_create(container_type, global.guiBaseContainerSideCenter, false)
        _panel = container_id
        with (_panel)
        {
            live_container_panel = true
            parent = other.id
            event_user(0)
            event_user(14)
        }
        is_open = true
    }
    scr_live_container_move_items_between(_holder, _panel, true)
    if instance_exists(_panel)
    {
        with (_panel)
            scr_adaptiveMenusPositionUpdate()
    }
    scr_live_container_sync(argument0)
    return true
}
",
            "scr_live_container_store_item.gml" => @"function scr_live_container_store_item(argument0, argument1) //gml_Script_scr_live_container_store_item
{
    if (!scr_live_container_is_item(argument0))
        return false
    if (!instance_exists(argument1))
        return false
    if argument0.is_open
        return false
    if (!scr_live_container_accepts_item(argument0, argument1))
        return false
    scr_live_container_load(argument0)
    var _holder = scr_live_container_get_holder(argument0)
    if (!instance_exists(_holder))
        return false
    var _cell = scr_inventory_get_cell_free(_holder, argument1)
    if (_cell == noone || _cell == -4)
        return false
    with (argument1)
    {
        scr_item_select(false)
        owner = _holder
        equipped = false
        equipped_id = -4
        equipped_highlight_alpha = 0
        scr_item_attach_to_cell(id, _cell)
        scr_guiVisibleUpdate(id, false)
        scr_item_placed()
        event_user(1)
        audio_play_sound(drop_gui_sound, 4, 0)
        scr_allturn()
    }
    scr_live_container_sync(argument0)
    with (o_player)
        scr_atr_calc(id)
    return true
}
",
            "scr_live_container_sync.gml" => @"function scr_live_container_sync(argument0) //gml_Script_scr_live_container_sync
{
    if (!scr_live_container_is_item(argument0))
        return false
    var _owner = noone
    var _list = -4
    with (argument0)
    {
        _list = ds_map_find_value(data, ""lootList"")
        if (is_open && instance_exists(container_id))
            _owner = container_id
        else if (variable_instance_exists(id, ""live_container_holder"") && instance_exists(live_container_holder))
            _owner = live_container_holder
    }
    if (!instance_exists(_owner) || __is_undefined(_list))
        return false
    var _stack = 0
    with (o_inv_slot)
    {
        if (owner == _owner && (!select))
        {
            if (object_index == o_inv_gold)
                _stack += stack
            else if (object_is_ancestor(object_index, o_inv_bag_parent) || object_is_ancestor(object_index, o_inv_casket_parent) || scr_live_container_is_item(id))
                _stack += ds_map_find_value_ext(data, ""Stack"", 0)
        }
    }
    with (argument0)
        ds_map_replace(data, ""Stack"", _stack)
    scr_save_item(_list, _owner, false, false)
    return true
}
",
            "scr_live_container_unload.gml" => @"function scr_live_container_unload(argument0) //gml_Script_scr_live_container_unload
{
    if (!scr_live_container_is_item(argument0))
        return false
    var _holder = noone
    with (argument0)
    {
        if (variable_instance_exists(id, ""live_container_holder"") && instance_exists(live_container_holder))
            _holder = live_container_holder
    }
    if (!instance_exists(_holder))
    {
        with (argument0)
        {
            live_container_holder = -4
            live_container_loaded = false
            if (variable_instance_exists(id, ""container_id""))
                container_id = -4
        }
        return false
    }
    with (argument0)
        container_id = _holder
    scr_live_container_sync(argument0)
    with (o_inv_slot)
    {
        if (owner == _holder && (!select))
            scr_item_destroy(id, false)
    }
    with (_holder)
        instance_destroy()
    with (argument0)
    {
        live_container_holder = -4
        live_container_loaded = false
        container_id = -4
    }
    with (o_player)
        scr_atr_calc(id)
    return true
}
",
            "scr_live_container_move_items_between.gml" => @"function scr_live_container_move_items_between(argument0, argument1, argument2) //gml_Script_scr_live_container_move_items_between
{
    if (argument2 == undefined)
        argument2 = true
    if (!instance_exists(argument0) || !instance_exists(argument1))
        return false
    var _from = argument0
    var _to = argument1
    var _visible = argument2
    with (o_inv_slot)
    {
        if (owner == _from && (!select))
        {
            var _container_index = -4
            var _cell_index = -4
            if instance_is(guiParent, o_inv_cell)
            {
                _container_index = scr_inventory_cell_get_container_index(guiParent)
                _cell_index = scr_inventory_cell_get_index(guiParent)
                scr_item_cells_update(id, false)
                scr_guiContainerChildRemove(guiParent, id)
            }
            owner = _to
            equipped = false
            equipped_id = -4
            equipped_highlight_alpha = 0
            var _cell = noone
            if (_container_index != -4 && _cell_index != -4)
                _cell = scr_inventory_container_get_cell_by_index(_to, _container_index, _cell_index)
            if (_cell != noone && _cell != -4)
                scr_item_attach_to_cell(id, _cell)
            else
                scr_inventory_add(_to, id)
            scr_guiVisibleUpdate(id, _visible)
            event_user(1)
        }
    }
    return true
}
",
            "scr_live_container_sync_all.gml" => @"function scr_live_container_sync_all() //gml_Script_scr_live_container_sync_all
{
    with (o_inv_live_container_parent)
        scr_live_container_sync(id)
    return true
}
",
            "scr_live_container_step.gml" => @"function scr_live_container_step() //gml_Script_scr_live_container_step
{
    return scr_live_container_bootstrap_all()
}
",
            "scr_live_container_panel_height.gml" => @"function scr_live_container_panel_height(argument0) //gml_Script_scr_live_container_panel_height
{
    var _layout = scr_live_container_get_layout_data(argument0)
    var _bottom = 0
    for (var _i = 0; _i < array_length(_layout); _i++)
    {
        var _entry = _layout[_i]
        var _offset_y = 0
        if (array_length(_entry) > 4)
            _offset_y = _entry[4]
        var _entry_bottom = _offset_y + _entry[1] * 27
        if (_bottom < _entry_bottom)
            _bottom = _entry_bottom
    }
    var _sprite_height = 0
    if instance_exists(argument0)
        _sprite_height = argument0.sprite_height
    return max(_sprite_height, (_bottom + 65))
}
",
            _ => throw new FileNotFoundException(fileName),
        };
    }

    private void AddObjects()
    {
        Msl.AddObject(
            name: "o_inv_live_container_parent",
            spriteName: "s_inv_casket",
            parentName: "c_inv_moneybag",
            isVisible: true,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        Msl.AddObject(
            name: "o_live_container_holder_parent",
            spriteName: "s_point",
            parentName: "o_menuParent",
            isVisible: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Box);

        Msl.AddObject(
            name: "o_container_live_container_parent",
            spriteName: "s_container",
            parentName: "o_container_inventory",
            isVisible: true,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Box);
    }

    private void AddEvents()
    {
        Msl.AddNewEvent("o_inv_live_container_parent", Code("gml_Object_o_inv_live_container_parent_Create_0.gml"), EventType.Create, 0);
        Msl.AddNewEvent("o_inv_live_container_parent", Code("gml_Object_o_inv_live_container_parent_Destroy_0.gml"), EventType.Destroy, 0);
        Msl.AddNewEvent("o_inv_live_container_parent", Code("gml_Object_o_inv_live_container_parent_Other_24.gml"), EventType.Other, 24);
        Msl.AddNewEvent("o_inv_live_container_parent", Code("gml_Object_o_inv_live_container_parent_Mouse_5.gml"), EventType.Mouse, 5);

        Msl.AddNewEvent("o_live_container_holder_parent", Code("gml_Object_o_live_container_holder_parent_Create_0.gml"), EventType.Create, 0);

        Msl.AddNewEvent("o_container_live_container_parent", Code("gml_Object_o_container_live_container_parent_Create_0.gml"), EventType.Create, 0);
        Msl.AddNewEvent("o_container_live_container_parent", Code("gml_Object_o_container_live_container_parent_Other_10.gml"), EventType.Other, 10);
        Msl.AddNewEvent("o_container_live_container_parent", Code("gml_Object_o_container_live_container_parent_Alarm_0.gml"), EventType.Alarm, 0);
    }

    private void AddFunctions()
    {
        Msl.AddFunction(Code("scr_live_container_is_item.gml"), "scr_live_container_is_item");
        Msl.AddFunction(Code("scr_live_container_accepts_item.gml"), "scr_live_container_accepts_item");
        Msl.AddFunction(Code("scr_live_container_layout_to_cell_param.gml"), "scr_live_container_layout_to_cell_param");
        Msl.AddFunction(Code("scr_live_container_get_layout_data.gml"), "scr_live_container_get_layout_data");
        Msl.AddFunction(Code("scr_live_container_panel_height.gml"), "scr_live_container_panel_height");
        Msl.AddFunction(Code("scr_live_container_build_layout.gml"), "scr_live_container_build_layout");
        Msl.AddFunction(Code("scr_live_container_get_holder.gml"), "scr_live_container_get_holder");
        Msl.AddFunction(Code("scr_live_container_load.gml"), "scr_live_container_load");
        Msl.AddFunction(Code("scr_live_container_move_items_between.gml"), "scr_live_container_move_items_between");
        Msl.AddFunction(Code("scr_live_container_sync.gml"), "scr_live_container_sync");
        Msl.AddFunction(Code("scr_live_container_open.gml"), "scr_live_container_open");
        Msl.AddFunction(Code("scr_live_container_store_item.gml"), "scr_live_container_store_item");
        Msl.AddFunction(Code("scr_live_container_unload.gml"), "scr_live_container_unload");
        Msl.AddFunction(Code("scr_live_container_sync_all.gml"), "scr_live_container_sync_all");
        Msl.AddFunction(Code("scr_live_container_bootstrap_all.gml"), "scr_live_container_bootstrap_all");
        Msl.AddFunction(Code("scr_live_container_step.gml"), "scr_live_container_step");
    }

    private void PatchContainerRules()
    {
        Msl.LoadGML("gml_GlobalScript_scr_inventory_get_containers")
            .MatchFrom(@"                    if (contentType == 4769)
                        array_push(_array, id)")
            .ReplaceBy(@"                    if (contentType == 4769 && scr_live_container_accepts_item(id, argument1))
                        array_push(_array, id)")
            .Save();

        Msl.LoadGML("gml_GlobalScript_scr_item_can_add_to_container")
            .MatchFrom(@"        var _container_object = argument0.object_index
        var _container_content_type = argument0.contentType
        if (_container_object == o_container_gold || _container_object == o_inv_moneybag)")
            .ReplaceBy(@"        var _container_object = argument0.object_index
        var _container_content_type = argument0.contentType
        if (!scr_live_container_accepts_item(argument0, argument1))
            return false;
        if (_container_object == o_container_gold || _container_object == o_inv_moneybag)")
            .Save();
    }

    private void PatchClosedContainerInsertion()
    {
        Msl.LoadGML("gml_Object_o_inv_slot_Other_21")
            .MatchFrom(@"with (_slotID)
{")
            .InsertBelow(@"    if (mouse_check_button_pressed(mb_left) && _item_select && _item_id.alarm[1] == -1 && scr_live_container_is_item(id) && _item_id.owner.object_index != o_trade_inventory && owner.object_index != o_trade_inventory && scr_live_container_accepts_item(id, _item_id))
    {
        if (scr_live_container_store_item(id, _item_id))
        {
            _is_exit = true
            return;
        }
    }")
            .MatchFrom("self.do_time_set()")
            .ReplaceBy("script_execute(self.do_time_set)")
            .MatchFrom("self.update_ammo_order()")
            .ReplaceBy("script_execute(self.update_ammo_order)")
            .Save();
    }

    private void PatchPersistence()
    {
        Msl.LoadGML("gml_GlobalScript_scr_savegame")
            .MatchFrom("        scr_save_item(global.inventoryDataList, o_inventory.id, false, false)")
            .InsertAbove("        scr_live_container_sync_all()")
            .Save();

        Msl.LoadGML("gml_Object_o_steamController_Step_0")
            .MatchFrom("steam_update()")
            .InsertBelow("scr_live_container_step()")
            .Save();
    }
}
