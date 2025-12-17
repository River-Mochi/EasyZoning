// File: src/UI/src/mods/ToolOptionsVisible/toolOptionsVisible.tsx
// Purpose:
//   Keep the Tool Options panel visible while the Easy Zoning tool is active.
//   Photo Mode: do not force panel visibility.

import { bindValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { ZONING_TOOL_ID } from "../../shared/tool-ids";
import mod from "mod.json";

// C# binding:
const isPhotoMode$ = bindValue<boolean>(mod.id, "IsPhotoMode");

type UseToolOptionsVisible = (...args: any[]) => boolean;
type ExtendHook<T extends (...args: any[]) => any> = (original: T) => T;

export const ToolOptionsVisibility: ExtendHook<UseToolOptionsVisible> = (useToolOptionsVisible) => {
    return (...args: any[]) => {
        const vanillaVisible = !!useToolOptionsVisible?.(...args);

        const activeId = tool.activeTool$.value?.id;
        const ours = activeId === ZONING_TOOL_ID;

        const photoMode = !!isPhotoMode$?.value;
        if (photoMode) return vanillaVisible;

        // Only force visibility for our tool (NOT for road-prefab tools).
        return vanillaVisible || ours;
    };
};
