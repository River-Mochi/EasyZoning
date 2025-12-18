// File: src/UI/src/mods/ez-zoneToolSections.tsx
// Purpose:
//   Inject Easy Zoning controls into the Tool Options panel.
//   When active on roads, we REPLACE the vanilla sections so we don't
//   show the huge snap + underground row. Only the Contour + L/R/B icons
//   are rendered, with tooltips as the only text.

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

        // When a road prefab is active *and* EZ tool is NOT,
        // the buttons act on RoadZoningMode (new roads).
        const usingRoadState = roadPrefabActive && !zoningToolOn;

        const { translate } = useLocalization();

        // Keep localized strings for tooltips; section titles are
        // either suppressed (zone row) or minimal.
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
            "Toggle zoning on both sides."
        );
        const tipLeft = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeLeftDescription]",
            "Zone only the left side."
        );
        const tipRight = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ZoningModeRightDescription]",
            "Zone only the right side."
        );
        const tipContour = translate(
            "ToolOptions.TOOLTIP_DESCRIPTION[EasyZoning.Zone_Controller.ContourDescription]",
            "Toggle terrain contour lines while Easy Zoning is active."
        );

        // Decide if we should inject EZ UI at all.
        const shouldShowZoneSection = roadPrefabActive || zoningToolOn;

        // If we're not in a road prefab and EZ tool isn't active, leave vanilla
        // UI completely untouched.
        if (!shouldShowZoneSection && !zoningToolOn) {
            return result;
        }

        // Build EZ own sections. *replace* vanilla children
        // to avoid all Underground mode row or snap buttons.
        const sections: any[] = [];

        // Contour row: only when EZ tool itself is active (update-existing mode).
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

        // Zone Change row: show for both vanilla road tool (new roads)
        // and EasyZoning tool (update mode).
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
                // Deliberately pass an empty title here so the panel is
                // visually smaller; the icons + tooltips are the UI.
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

        // Replace the vanilla children entirely while EZ controls are relevant.
        // This is what strips the Underground mode row and snap buttons.
        if (sections.length > 0) {
            result.props.children = sections;
        }

        return result;
    };
};
