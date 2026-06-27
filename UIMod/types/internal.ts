//from Traffic mod

import { ReactElement, PropsWithChildren, ReactNode } from "react";
import { TooltipProps } from "cs2/ui";
import { getModule } from "cs2/modding";

interface DescriptionTooltipProps extends Omit<TooltipProps, 'tooltip'> {
    title: string | null;
    description: string | null;
}

interface TabBarProps {
    className?: string;
    children?: ReactNode;
}

interface TabProps {
    id: unknown;
    selectedId: unknown;
    uiTag?: string;
    disabled?: boolean;
    selectSound?: unknown;
    className?: string;
    children?: ReactNode;
    onSelect: (id: unknown) => void;
}

export class VanillaComponentsResolver {
    public static get instance() {
        return this._instance
    }

    public get DescriptionTooltip(): (props: PropsWithChildren<DescriptionTooltipProps>) => ReactElement {
        return this._descriptionTooltip
    }

    public get TabBar(): (props: TabBarProps) => ReactElement {
        return this._tabBar ??= getModule("game-ui/common/tabs/tabs.tsx", "TabBar");
    }

    public get Tab(): (props: TabProps) => ReactElement {
        return this._tab ??= getModule("game-ui/common/tabs/tabs.tsx", "Tab");
    }

    private static _instance: VanillaComponentsResolver = new VanillaComponentsResolver();
    private readonly _descriptionTooltip: (props: PropsWithChildren<DescriptionTooltipProps>) => ReactElement;
    private _tabBar?: (props: TabBarProps) => ReactElement;
    private _tab?: (props: TabProps) => ReactElement;

    private constructor() {
        this._descriptionTooltip = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.tsx", "DescriptionTooltip");
    }
}
