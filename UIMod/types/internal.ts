//from Traffic mod

import { ReactElement, PropsWithChildren } from "react";
import { TooltipProps } from "cs2/ui";
import { getModule } from "cs2/modding";

interface DescriptionTooltipProps extends Omit<TooltipProps, 'tooltip'> {
    title: string | null;
    description: string | null;
}

export class VanillaComponentsResolver {
    public static get instance() {
        return this._instance
    }

    public get DescriptionTooltip(): (props: PropsWithChildren<DescriptionTooltipProps>) => ReactElement {
        return this._descriptionTooltip
    }

    private static _instance: VanillaComponentsResolver = new VanillaComponentsResolver();
    private readonly _descriptionTooltip: (props: PropsWithChildren<DescriptionTooltipProps>) => ReactElement;

    private constructor() {
        this._descriptionTooltip = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.tsx", "DescriptionTooltip");
    }
}