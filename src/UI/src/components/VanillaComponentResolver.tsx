// File: src/UI/src/components/VanillaComponentResolver.tsx
// Purpose:
//   Helper wrapper around CS2 ModuleRegistry for a small set of vanilla UI
//   components and themes used by this mod.
//
// These are specific to the types of components that this mod uses.
// In the UI dev tools at http://localhost:9444/ → Sources → index.js (pretty-print).
// Search for the tsx/scss paths, find the exported functions and their props.
// Types below are intentionally loose to tolerate minor game updates.

import type { HTMLAttributes, ReactElement, ReactNode } from "react";
import { BalloonDirection, Color, FocusKey, Theme, UniqueFocusKey } from "cs2/bindings";
import { InputAction } from "cs2/input";
import type { ModuleRegistry } from "cs2/modding";

type PropsToolButton = {
    focusKey?: UniqueFocusKey | null;
    src?: string;
    selected?: boolean;
    multiSelect?: boolean;
    disabled?: boolean;
    tooltip?: ReactNode | null;
    selectSound?: any;
    uiTag?: string;
    className?: string;
    children?: ReactNode;
    onSelect?: (x: any) => any;
} & HTMLAttributes<any>;

type PropsSection = {
    title?: string | null;
    uiTag?: string;
    children: ReactNode;
};

type PropsColorField = {
    focusKey?: FocusKey;
    disabled?: boolean;
    value?: Color; // UnityEngine.Color in C#
    className?: string;
    selectAction?: InputAction;
    alpha?: any;
    popupDirection?: BalloonDirection;
    onChange?: (e: Color) => void;
    onClick?: (e: any) => void;
    onMouseEnter?: (e: any) => void;
    onMouseLeave?: (e: any) => void;
};

const registryIndex = {
    Section: [
        "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
        "Section",
    ],
    ToolButton: [
        "game-ui/game/components/tool-options/tool-button/tool-button.tsx",
        "ToolButton",
    ],
    toolButtonTheme: [
        "game-ui/game/components/tool-options/tool-button/tool-button.module.scss",
        "classes",
    ],
    mouseToolOptionsTheme: [
        "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.module.scss",
        "classes",
    ],
    FOCUS_DISABLED: ["game-ui/common/focus/focus-key.ts", "FOCUS_DISABLED"],
    FOCUS_AUTO: ["game-ui/common/focus/focus-key.ts", "FOCUS_AUTO"],
    useUniqueFocusKey: ["game-ui/common/focus/focus-key.ts", "useUniqueFocusKey"],
    assetGridTheme: [
        "game-ui/game/components/asset-menu/asset-grid/asset-grid.module.scss",
        "classes",
    ],
    descriptionTooltipTheme: [
        "game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss",
        "classes",
    ],
    ColorField: ["game-ui/common/input/color-picker/color-field/color-field.tsx", "ColorField"],
} as const;

export class VanillaComponentResolver {
    public static get instance(): VanillaComponentResolver {
        return this._instance!;
    }

    private static _instance?: VanillaComponentResolver;

    public static setRegistry(in_registry: ModuleRegistry) {
        this._instance = new VanillaComponentResolver(in_registry);
    }

    private readonly registryData: ModuleRegistry;

    private readonly cachedData: Partial<Record<keyof typeof registryIndex, any>> = {};

    private constructor(in_registry: ModuleRegistry) {
        this.registryData = in_registry;
    }

    private updateCache(entry: keyof typeof registryIndex) {
        const [path, exportId] = registryIndex[entry];
        const mod = this.registryData.registry.get(path);
        const value = mod ? (mod as any)[exportId] : undefined;
        this.cachedData[entry] = value;
        return value;
    }

    public get Section(): (props: PropsSection) => ReactElement {
        return this.cachedData["Section"] ?? this.updateCache("Section");
    }

    public get ToolButton(): (props: PropsToolButton) => ReactElement {
        return this.cachedData["ToolButton"] ?? this.updateCache("ToolButton");
    }

    public get ColorField(): (props: PropsColorField) => ReactElement {
        return this.cachedData["ColorField"] ?? this.updateCache("ColorField");
    }

    public get toolButtonTheme(): Theme | any {
        return this.cachedData["toolButtonTheme"] ?? this.updateCache("toolButtonTheme");
    }

    public get mouseToolOptionsTheme(): Theme | any {
        return this.cachedData["mouseToolOptionsTheme"] ?? this.updateCache("mouseToolOptionsTheme");
    }

    public get assetGridTheme(): Theme | any {
        return this.cachedData["assetGridTheme"] ?? this.updateCache("assetGridTheme");
    }

    public get descriptionTooltipTheme(): Theme | any {
        return this.cachedData["descriptionTooltipTheme"] ?? this.updateCache("descriptionTooltipTheme");
    }

    public get FOCUS_DISABLED(): UniqueFocusKey {
        return this.cachedData["FOCUS_DISABLED"] ?? this.updateCache("FOCUS_DISABLED");
    }

    public get FOCUS_AUTO(): UniqueFocusKey {
        return this.cachedData["FOCUS_AUTO"] ?? this.updateCache("FOCUS_AUTO");
    }

    public get useUniqueFocusKey(): (focusKey: FocusKey, debugName: string) => UniqueFocusKey | null {
        return this.cachedData["useUniqueFocusKey"] ?? this.updateCache("useUniqueFocusKey");
    }
}
