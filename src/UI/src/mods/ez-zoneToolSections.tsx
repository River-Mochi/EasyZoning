// File: src/UI/src/mods/ez-zoneToolSections.tsx
// Purpose:
//   Renders the “Contour” + “Zone Change” rows in the Tool Options panel.
//   UI-only: handles clicks on the icons; world input is handled in C#
//   (ZoningControllerToolSystem + ZoningControllerToolUISystem).

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
// Copy the vanilla contour icon into your images/icons folder with this name.
import IconContour from "../../images/icons/ContourLines.svg";

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

        // When road prefab is active *and* our tool is NOT, buttons act on RoadZoningMode.
        const usingRoadState = roadPrefabActive && !zoningToolOn;

        const { translate } = useLocalization();

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
            "Toggle zoning on Both sides."
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
            "Toggle terrain CONTOUR lines while Easy Zoning is active."
        );

        // Contour row: only when our tool is active (update-existing-roads mode).
        if (zoningToolOn) {
            result.props.children?.push(
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

        // Zone Change row: show whenever we’re either:
        //  • in vanilla road tool (new roads), OR
        //  • EasyZoning tool (update mode).
        const shouldShowZoneSection = roadPrefabActive || zoningToolOn;

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

            result.props.children?.push(
                <Section title={titleZone}>
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

        return result;
    };
};
