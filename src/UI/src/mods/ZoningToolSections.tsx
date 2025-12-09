// File: src/UI/src/mods/ZoningToolSections.tsx
// Purpose: Renders the “Zone Change” 3-button group in the Tool Options panel.
// UI-only: Handles LMB on the three buttons. No world/road mouse input here.
//
// • Click Left/Right button sets that exact side.
// • Click “Both” button toggles Both <-> None (all highlighted or none highlighted).
// World interactions (hover preview, RMB flip, LMB apply) are implemented in C#
// ZoningControllerToolSystem and are independent from this file.

import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import mod from "../../mod.json";
import { VanillaComponentResolver } from "../YenYang/VanillaComponentResolver";
// NOTE: No custom SCSS import — we let vanilla ToolButton center its own icon.

// Tool id shared with C# side
import { ZONING_TOOL_ID } from "../shared/tool-ids";

// Icon assets (emitted by webpack to coui://ui-mods/images/)
import IconBoth from "../../images/icons/mode-icon-both.svg";
import IconLeft from "../../images/icons/mode-icon-left.svg";
import IconRight from "../../images/icons/mode-icon-right.svg";

export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3,
}

const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const isRoadPrefab$ = bindValue<boolean>(mod.id, "IsRoadPrefab"); // true when a road prefab tool is active

function setToolZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeToolZoningMode", value);
    try { console.log("[EZ][UI] setToolZoningMode →", value); } catch { }
}
function setRoadZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeRoadZoningMode", value);
    try { console.log("[EZ][UI] setRoadZoningMode →", value); } catch { }
}
function flipRoadBothMode() {
    trigger(mod.id, "FlipRoadBothMode");
    try { console.log("[EZ][UI] flipRoadBothMode"); } catch { }
}
function flipToolBothMode() {
    trigger(mod.id, "FlipToolBothMode");
    try { console.log("[EZ][UI] flipToolBothMode"); } catch { }
}

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const result = Component(props);

        const activeTool = useValue(tool.activeTool$)?.id;
        const roadPrefabActive = useValue(isRoadPrefab$) === true;
        const zoningToolOn = activeTool === ZONING_TOOL_ID;

        // ✅ FIX: show the section if EITHER a road prefab is active OR our tool is active
        const shouldShowSection = roadPrefabActive || zoningToolOn;

        const toolMode = useValue(ToolZoningMode$) as ZoningMode;
        const roadMode = useValue(RoadZoningMode$) as ZoningMode;

        const { translate } = useLocalization();
        const title = translate(
            "ToolOptions.SECTION[EasyZoning.Zone_Controller.SectionTitle]",
            "Zone Change"
        );
        const tipBoth = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeBothDescription]",
            "Toggle Both/None."
        );
        const tipLeft = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeLeftDescription]",
            "Zone only the left side."
        );
        const tipRight = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeRightDescription]",
            "Zone only the right side."
        );

        if (shouldShowSection) {
            // When a road prefab is active AND our tool is not, operate on the road state.
            const usingRoadState = roadPrefabActive && !zoningToolOn;
            const selected = usingRoadState ? roadMode : toolMode;

            const onLeft = () =>
                usingRoadState
                    ? setRoadZoningMode(ZoningMode.Left)
                    : setToolZoningMode(ZoningMode.Left);

            const onRight = () =>
                usingRoadState
                    ? setRoadZoningMode(ZoningMode.Right)
                    : setToolZoningMode(ZoningMode.Right);

            const onBoth = () =>
                usingRoadState ? flipRoadBothMode() : flipToolBothMode();

            // Be defensive: themes may be missing on some game builds — optional-chain className
            const toolBtnClass = VanillaComponentResolver.instance.toolButtonTheme?.ToolButton
                ?? undefined;

            // Push our section + three buttons to the vanilla MouseToolOptions panel
            result.props.children?.push(
                <VanillaComponentResolver.instance.Section title={title}>
                    <>
                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={tipBoth}
                            onSelect={onBoth}
                            src={IconBoth}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={toolBtnClass}
                        />

                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={tipLeft}
                            onSelect={onLeft}
                            src={IconLeft}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={toolBtnClass}
                        />

                        <VanillaComponentResolver.instance.ToolButton
                            selected={(selected & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={tipRight}
                            onSelect={onRight}
                            src={IconRight}
                            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                            className={toolBtnClass}
                        />
                    </>
                </VanillaComponentResolver.instance.Section>
            );
        }

        return result;
    };
};
