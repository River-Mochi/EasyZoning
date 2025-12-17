// File: src/UI/src/mods/ToolOptionsVisible/toolOptionsVisible.tsx
// Purpose:
//   Keep the Tool Options panel visible while the zoning controller tool
//   or a road prefab tool is active.
//   Photo Mode: do not force panel visibility.

import { bindValue } from "cs2/api";
import { tool } from "cs2/bindings";
import mod from "mod.json";
import { ZONING_TOOL_ID } from "../../shared/tool-ids";

// C# bindings:
//   • IsRoadPrefab (true when a road prefab tool is active)
//   • IsPhotoMode  (true when Photo Mode is active)
const isRoadPrefab$ = bindValue<boolean>(mod.id, "IsRoadPrefab");
const isPhotoMode$ = bindValue<boolean>(mod.id, "IsPhotoMode");

// Extension signature: takes original hook, returns replacement hook.
type UseToolOptionsVisible = (...args: any[]) => boolean;
type ExtendHook<T extends (...args: any[]) => any> = (original: T) => T;

export const ToolOptionsVisibility: ExtendHook<UseToolOptionsVisible> = (useToolOptionsVisible) => {
    return (...args: any[]) => {
        const vanillaVisible = !!useToolOptionsVisible?.(...args);

        const activeId = tool.activeTool$.value?.id;
        const ours = activeId === ZONING_TOOL_ID;

        const roadPrefab = !!isRoadPrefab$.value;
        const photoMode = !!isPhotoMode$.value;

        // Photo Mode: never force visibility.
        if (photoMode) return vanillaVisible;

        return vanillaVisible || ours || roadPrefab;
    };
};
