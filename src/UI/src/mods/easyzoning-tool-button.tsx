// File: src/UI/src/mods/easyzoning-tool-button.tsx
// Purpose: Floating GameTopLeft launcher button (icon + tooltip). Triggers ToggleZoneControllerTool.
// Notes:
//   • Current cs2/ui Button typings expose onClick (not onPress), so use onClick here.
//   • Keep everything else (variant, src, tooltipLabel, trigger) unchanged.

import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { trigger } from "cs2/api";
// Use the webpack alias so this works from any folder depth:
import mod from "mod.json";

import MainIconPath from "../../images/ico-zones-color02.svg";

export default function EasyZoningToolButton() {
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
        } catch { /* ignore console failures */ }
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
