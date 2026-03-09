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
    <div className="sm:hidden">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
      >
        {isOpen ? (
          <X className="w-6 h-6 text-gray-700" />
        ) : (
          <Menu className="w-6 h-6 text-gray-700" />
        )}
      </button>

      {isOpen && (
        <div className="absolute top-full right-0 mt-0 w-48 bg-white border border-gray-200 rounded-lg shadow-lg z-30">
          <div className="p-3 space-y-2 border-b border-gray-200">
            <LanguageSwitch />
          </div>

          <button
            onClick={() => {
              onDelete();
              handleClose();
            }}
            disabled={deletingAccount}
            className="w-full flex items-center gap-2 px-4 py-3 text-sm text-red-700 hover:bg-red-50 transition-colors cursor-pointer disabled:opacity-50 border-b border-gray-200"
          >
            <Trash2 className="w-4 h-4" />
            <span>{deletingAccount ? t('deleting') : t('deleteAccount')}</span>
          </button>

          <button
            onClick={() => {
              onLogout();
              handleClose();
            }}
            className="w-full flex items-center gap-2 px-4 py-3 text-sm text-gray-700 hover:bg-gray-100 transition-colors cursor-pointer"
          >
            <LogOut className="w-4 h-4" />
            <span>{t('logout')}</span>
          </button>
        </div>
      )}
    </div>
  );
}

