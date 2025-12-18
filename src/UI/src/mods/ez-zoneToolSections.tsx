// File: src/UI/src/mods/ez-zoneToolSections.tsx
// Purpose:
//   Injects Easy Zoning controls into the Tool Options panel.
//   When active on roads, replaces the vanilla sections so the snap
//   and Underground rows are hidden and only the EZ controls are shown.

import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";

import mod from "../../mod.json";
import { ZONING_TOOL_ID } from "../shared/tool-ids";
import { VanillaComponentResolver } from "../components/VanillaComponentResolver";

import styles from "./ez-zoneToolSections.module.scss";

// Icon assets (webpack emits to coui://ui-mods/images/)
import IconBoth from "../../images/icons/mode-icon-both.svg";
import IconLeft from "../../images/icons/mode-icon-left.svg";
import IconRight from "../../images/icons/mode-icon-right.svg";
import IconContour from "../../images/icons/ContourLines.svg";

// NOTE: These numeric values must match the C# ZoningMode enum.
export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3,
}

const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const IsRoadPrefab$ = bindValue<boolean>(mod.id, "IsRoadPrefab");
const ContourEnabled$ = bindValue<boolean>(mod.id, "ContourEnabled");

function setToolZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeToolZoningMode", value);
}

function setRoadZoningMode(value: ZoningMode) {
    trigger(mod.id, "ChangeRoadZoningMode", value);
}

function flipRoadBothMode() {
    trigger(mod.id, "FlipRoadBothMode");
}

function flipToolBothMode() {
    trigger(mod.id, "FlipToolBothMode");
}

function toggleContourLines() {
    trigger(mod.id, "ToggleContourLines");
}

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props: any) => {
        // Render vanilla ToolOptionsSections first.
        const result = Component(props);

        const resolver = VanillaComponentResolver.instance;
        const Section = resolver.Section;
        const ToolButton = resolver.ToolButton;
        const FOCUS_DISABLED = resolver.FOCUS_DISABLED;

        const toolButtonClass = resolver.toolButtonTheme?.ToolButton ?? undefined;
        const rowClass = styles.row ?? undefined;

        const activeToolId = useValue(tool.activeTool$)?.id;
        const roadPrefabActive = useValue(IsRoadPrefab$) === true;
        const zoningToolOn = activeToolId === ZONING_TOOL_ID;

        const toolMode = useValue(ToolZoningMode$) as ZoningMode;
        const roadMode = useValue(RoadZoningMode$) as ZoningMode;
        const contourEnabled = !!useValue(ContourEnabled$);

        // When a road prefab is active and the EZ tool is not,
        // the buttons act on RoadZoningMode (new roads).
        const usingRoadState = roadPrefabActive && !zoningToolOn;

        const { translate } = useLocalization();

        // Localized strings are kept for tooltips and optional section titles.
        const titleZone = translate(
            "ToolOptions.SECTION[EasyZoning.Zone_Controller.SectionTitle]",
            "Zone Change"
        );
        const titleContour = translate(
            "ToolOptions.SECTION[EasyZoning.Zone_Controller.ContourTitle]",
            "Contour"
        );

        const tipBoth = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeBothDescription]",
            "Toggle zoning on BOTH sides."
        );
        const tipLeft = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeLeftDescription]",
            "Zone LEFT side."
        );
        const tipRight = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeRightDescription]",
            "Zone RIGHT side."
        );
        const tipContour = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ContourDescription]",
            "Toggle terrain contour lines while Easy Zoning is active."
        );

        const shouldShowZoneSection = roadPrefabActive || zoningToolOn;

        // When no road prefab is active and the EZ tool is not active,
        // vanilla content should remain unchanged.
        if (!shouldShowZoneSection && !zoningToolOn) {
            return result;
        }

        // Build new section list. This replaces the vanilla children while
        // the EZ controls are relevant so underground and snap rows are hidden.
        const sections: any[] = [];

        // Contour row: only when EZ tool is active (update-existing mode).
        if (zoningToolOn) {
            sections.push(
                <Section title={titleContour}>
                    <div className={rowClass}>
                        <ToolButton
                            selected={contourEnabled}
                            tooltip={tipContour}
                            onSelect={toggleContourLines}
                            src={IconContour}
                            focusKey={FOCUS_DISABLED}
                            className={toolButtonClass}
                        />
                    </div>
                </Section>
            );
        }

        // Zone row: shown for both vanilla road tool (new roads)
        // and EZ tool (update mode).
        if (shouldShowZoneSection) {
            const selectedMode = usingRoadState ? roadMode : toolMode;

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

            sections.push(
                // Empty title keeps the panel compact; icons and tooltips
                // provide the necessary context.
                <Section title="">
                    <div className={rowClass}>
                        <ToolButton
                            selected={(selectedMode & ZoningMode.Both) === ZoningMode.Both}
                            tooltip={tipBoth}
                            onSelect={onBoth}
                            src={IconBoth}
                            focusKey={FOCUS_DISABLED}
                            className={toolButtonClass}
                        />
                        <ToolButton
                            selected={(selectedMode & ZoningMode.Left) === ZoningMode.Left}
                            tooltip={tipLeft}
                            onSelect={onLeft}
                            src={IconLeft}
                            focusKey={FOCUS_DISABLED}
                            className={toolButtonClass}
                        />
                        <ToolButton
                            selected={(selectedMode & ZoningMode.Right) === ZoningMode.Right}
                            tooltip={tipRight}
                            onSelect={onRight}
                            src={IconRight}
                            focusKey={FOCUS_DISABLED}
                            className={toolButtonClass}
                        />
                    </div>
                </Section>
            );
        }

        if (sections.length > 0) {
            result.props.children = sections;
        }

        return result;
    };
};
