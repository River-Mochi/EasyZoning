// File: src/UI/src/index.tsx
// Purpose: Hook the UI into vanilla, register top-left button + Tool Options
// section, and keep the options panel visible when the zoning tool is active.

import type { ModRegistrar, ModuleRegistry } from "cs2/modding";
import { VanillaComponentResolver } from "./components/VanillaComponentResolver";

import EasyZoningToolButton from "./mods/ez-zone-tool-button";
import { ZoningToolController } from "./mods/ez-zoneToolSections";
import { ToolOptionsVisibility } from "./mods/ToolOptionsVisible/toolOptionsVisible";

// Ensure assets are emitted to coui://ui-mods/images/
import "../images/ico-zones-color02.svg"; // Top-left FAB icon

// Mode icons used in the Tool Options section
import "../images/icons/mode-icon-both.svg";
import "../images/icons/mode-icon-left.svg";
import "../images/icons/mode-icon-right.svg";
import "../images/icons/ContourLines.svg";

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

// Helper: extend a vanilla module export with error guarding.
function extendSafe(
    registry: ModuleRegistry,
    modulePath: string,
    exportId: string,
    extension: any
) {
    try {
        registry.extend(modulePath, exportId, extension);
    } catch (err) {
        try {
            console.error(`[EZ][UI] extend failed for ${modulePath}#${exportId}`, err);
        } catch {
            // Ignore console failures
        }
    }
}

const register: ModRegistrar = (moduleRegistry) => {
    // Inject ModuleRegistry into the resolver singleton.
    VanillaComponentResolver.setRegistry(moduleRegistry);

    // Floating button in GameTopLeft that toggles the zoning controller tool.
    moduleRegistry.append("GameTopLeft", EasyZoningToolButton);

    // Inject custom Tool Options section + visibility rules.
    extendSafe(
        moduleRegistry,
        VANILLA.MouseToolOptions.path,
        VANILLA.MouseToolOptions.exportId,
        ZoningToolController
    );
    extendSafe(
        moduleRegistry,
        VANILLA.ToolOptionsPanelVisible.path,
        VANILLA.ToolOptionsPanelVisible.exportId,
        ToolOptionsVisibility
    );
};

export default register;
