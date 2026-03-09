import { Menu, X, LogOut, Trash2 } from 'lucide-react';
import { useState } from 'react';
import LanguageSwitch from './LanguageSwitch';
import { useI18n } from '../i18n';

interface MobileMenuProps {
  onLogout: () => void;
  onDelete: () => void;
  deletingAccount: boolean;
}

export default function MobileMenu({ onLogout, onDelete, deletingAccount }: MobileMenuProps) {
  const [isOpen, setIsOpen] = useState(false);
  const { t } = useI18n();

  const handleClose = () => setIsOpen(false);

  return (
    <div className="sm:hidden relative">
      <button
        onClick={() => setIsOpen((v) => !v)}
        className="p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
      >
        {isOpen ? (
          <X className="w-6 h-6 text-gray-700" />
        ) : (
          <Menu className="w-6 h-6 text-gray-700" />
        )}
      </button>

      <div
        className={`fixed inset-0 z-40 bg-black/30 transition-opacity duration-300 ${isOpen ? 'opacity-100 pointer-events-auto' : 'opacity-0 pointer-events-none'}`}
        onClick={handleClose}
      />

      <div
        className={`fixed top-[61px] left-0 right-0 z-50 bg-white border-t border-gray-200 shadow-xl transition-all duration-300 ease-out ${isOpen ? 'opacity-100 translate-y-0' : 'opacity-0 -translate-y-3 pointer-events-none'}`}
      >
        <div className="p-4 border-b border-gray-200">
          <LanguageSwitch />
        </div>

        <button
          onClick={() => {
            onDelete();
            handleClose();
          }}
          disabled={deletingAccount}
          className="w-full flex items-center gap-3 px-4 py-4 text-sm text-red-700 hover:bg-red-50 transition-colors cursor-pointer disabled:opacity-50 border-b border-gray-200"
        >
          <Trash2 className="w-4 h-4" />
          <span>{deletingAccount ? t('deleting') : t('deleteAccount')}</span>
        </button>

        <button
          onClick={() => {
            onLogout();
            handleClose();
          }}
          className="w-full flex items-center gap-3 px-4 py-4 text-sm text-gray-700 hover:bg-gray-100 transition-colors cursor-pointer"
        >
          <LogOut className="w-4 h-4" />
          <span>{t('logout')}</span>
        </button>
      </div>
    </div>
  );
}
