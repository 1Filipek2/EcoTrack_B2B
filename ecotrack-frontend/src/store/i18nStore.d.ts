export type Locale = 'en' | 'sk';
interface I18nState {
    locale: Locale;
    setLocale: (locale: Locale) => void;
}
export declare const useI18nStore: import("zustand").UseBoundStore<Omit<import("zustand").StoreApi<I18nState>, "setState" | "persist"> & {
    setState(partial: I18nState | Partial<I18nState> | ((state: I18nState) => I18nState | Partial<I18nState>), replace?: false | undefined): unknown;
    setState(state: I18nState | ((state: I18nState) => I18nState), replace: true): unknown;
    persist: {
        setOptions: (options: Partial<import("zustand/middleware").PersistOptions<I18nState, I18nState, unknown>>) => void;
        clearStorage: () => void;
        rehydrate: () => Promise<void> | void;
        hasHydrated: () => boolean;
        onHydrate: (fn: (state: I18nState) => void) => () => void;
        onFinishHydration: (fn: (state: I18nState) => void) => () => void;
        getOptions: () => Partial<import("zustand/middleware").PersistOptions<I18nState, I18nState, unknown>>;
    };
}>;
export {};
