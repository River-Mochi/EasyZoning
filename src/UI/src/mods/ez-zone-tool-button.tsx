// File: src/UI/src/mods/ez-zone-tool-button.tsx
// Purpose: Floating GameTopLeft launcher button (icon + tooltip).
//          Triggers ToggleZoneControllerTool on the C# side.

import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
import mod from "mod.json";

// Icon emitted by webpack to coui://ui-mods/images/
import MainIconPath from "../../images/ico-zones-color02.svg";

export default function EZZoneToolButton() {
    const { translate } = useLocalization();

    const tooltipLabel = translate(
        "EasyZoning.Zone_Controller.ToolName",
        "Easy Zoning"
    );

    const handleClick = () => {
        // C# side listens for this and toggles the zoning controller tool.
        trigger(mod.id, "ToggleZoneControllerTool");
        try {
            console.log("[EZ][UI] GameTopLeft button → ToggleZoneControllerTool");
        } catch {
            // Ignore console failures
        }
    };

    return (
        <Button
            variant="floating"
            src={MainIconPath}
            tooltipLabel={tooltipLabel}
            onClick={handleClick}
        />
    );
}
