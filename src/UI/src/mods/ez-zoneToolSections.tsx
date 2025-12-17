// File: src/UI/src/mods/ez-zoneToolSections.tsx
// Purpose:
//   Renders the “Zone Change” 3-button group in the Tool Options panel.
//   UI-only: handles left-mouse clicks on the three icons; world input is
//   handled in ZoningControllerToolSystem (C#).

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

export enum ZoningMode {
    None = 0,
    Right = 1,
    Left = 2,
    Both = 3,
}

const RoadZoningMode$ = bindValue<number>(mod.id, "RoadZoningMode");
const ToolZoningMode$ = bindValue<number>(mod.id, "ToolZoningMode");
const IsRoadPrefab$ = bindValue<boolean>(mod.id, "IsRoadPrefab");

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

export const ZoningToolController: ModuleRegistryExtend = (Component: any) => {
    return (props: any) => {
        const result = Component(props);

        const resolver = VanillaComponentResolver.instance;
        const Section = resolver.Section;
        const ToolButton = resolver.ToolButton;
        const FOCUS_DISABLED = resolver.FOCUS_DISABLED;

        const toolButtonClass = resolver.toolButtonTheme?.ToolButton ?? undefined;

        const rowClass = styles.ezRow ?? undefined;

        const activeToolId = useValue(tool.activeTool$)?.id;
        const roadPrefabActive = useValue(IsRoadPrefab$) === true;
        const zoningToolOn = activeToolId === ZONING_TOOL_ID;

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
            const usingRoadState = roadPrefabActive && !zoningToolOn;
            const selectedMode = usingRoadState ? roadMode : toolMode;

            const onLeft = () => (usingRoadState ? setRoadZoningMode(ZoningMode.Left) : setToolZoningMode(ZoningMode.Left));
            const onRight = () => (usingRoadState ? setRoadZoningMode(ZoningMode.Right) : setToolZoningMode(ZoningMode.Right));
            const onBoth = () => (usingRoadState ? flipRoadBothMode() : flipToolBothMode());

            result.props.children?.push(
                <Section title={title}>
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
