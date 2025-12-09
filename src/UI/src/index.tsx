// File: src/UI/src/index.tsx

// Purpose: Hook our UI into vanilla, register top-left button + Tool Options section,
// and keep the options panel visible when our tool is active.

import type { ModRegistrar, ModuleRegistry } from "cs2/modding";
import { VanillaComponentResolver } from "./YenYang/VanillaComponentResolver";
import EasyZoningToolButton from "./mods/easyzoning-tool-button";
import { ZoningToolController } from "./mods/ZoningToolSections";
import { ToolOptionsVisibility } from "./mods/ToolOptionsVisible/toolOptionsVisible";

// Ensure assets are emitted to coui://ui-mods/images/
import "../images/ico-zones-color02.svg";  // Top-left FAB icon
import "../images/ico-zones-color02.svg";  // Road Services panel tile

// Mode icons used in the Tool Options section
import "../images/icons/mode-icon-both.svg";
import "../images/icons/mode-icon-left.svg";
import "../images/icons/mode-icon-right.svg";

const DIAG_TOPO = false;

const VANILLA = {
    MouseToolOptions: {
        path: "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
        exportId: "MouseToolOptions",
    },
    ToolOptionsPanelVisible: {
        path: "game-ui/game/components/tool-options/tool-options-panel.tsx",
        exportId: "useToolOptionsVisible",
    },
};

function extendSafe(registry: ModuleRegistry, modulePath: string, exportId: string, extension: any) {
    try {
        registry.extend(modulePath, exportId, extension);
    } catch (err) {
        try { console.error(`[EZ][UI] extend failed for ${modulePath}#${exportId}`, err); } catch { }
    }
}

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);

    if (DIAG_TOPO) {
        try {
            const candidates = moduleRegistry.find(/topograph|contour|terrain|overlay|elev|height/i);
            if (Array.isArray(candidates) && candidates.length) {
                for (const [path, ...exports] of candidates ?? []) {
                    if (/tool-options|topograph|contour/i.test(path)) {
                        console.log(`[EZ][diag] candidate: ${path}  ->  ${exports.join(",")}`);
                    }
                }
            } else {
                console.log("[EZ][diag] no topo/contour candidates found");
            }
        } catch { /* silent */ }
    }

    moduleRegistry.append("GameTopLeft", EasyZoningToolButton);

    extendSafe(moduleRegistry, VANILLA.MouseToolOptions.path, VANILLA.MouseToolOptions.exportId, ZoningToolController);
    extendSafe(moduleRegistry, VANILLA.ToolOptionsPanelVisible.path, VANILLA.ToolOptionsPanelVisible.exportId, ToolOptionsVisibility);
};

export default register;
