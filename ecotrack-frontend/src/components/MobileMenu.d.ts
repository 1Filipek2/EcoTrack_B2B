interface MobileMenuProps {
    onLogout: () => void;
    onDelete: () => void;
    deletingAccount: boolean;
}
export default function MobileMenu({ onLogout, onDelete, deletingAccount }: MobileMenuProps): import("react/jsx-runtime").JSX.Element;
export {};
