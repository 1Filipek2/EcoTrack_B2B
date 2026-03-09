import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  BarChart3,
  TrendingUp,
  Leaf,
  Plus,
  Building2,
  Info,
  ChevronDown,
} from 'lucide-react';
import axios from 'axios';
import { authApi, companiesApi, emissionsApi } from '../api';
import { useAuthStore } from '../store/authStore';
import type { Company, SustainabilityReport, PagedResult, EmissionEntry } from '../types/api';
import LanguageSwitch from '../components/LanguageSwitch';
import MobileMenu from '../components/MobileMenu';
import { useI18n } from '../i18n';

export default function DashboardPage() {
  const { user, logout, updateAuth } = useAuthStore();
  const navigate = useNavigate();
  const { t } = useI18n();

  const [report, setReport] = useState<SustainabilityReport | null>(null);
  const [emissions, setEmissions] = useState<PagedResult<EmissionEntry> | null>(null);
  const [loading, setLoading] = useState(true);
  const [linkError, setLinkError] = useState('');
  const [companyName, setCompanyName] = useState('');
  const [vatNumber, setVatNumber] = useState('');
  const [linking, setLinking] = useState(false);
  const [company, setCompany] = useState<Company | null>(null);
  const [expandedEmissionId, setExpandedEmissionId] = useState<string | null>(null);
  const [deletingAccount, setDeletingAccount] = useState(false);
  const [showWelcome, setShowWelcome] = useState(false);
  const [hideWelcome, setHideWelcome] = useState(false);

  useEffect(() => {
    const loadData = async () => {
      try {
        const emissionsData = await emissionsApi.getAll({ pageNumber: 1, pageSize: 5 });
        setEmissions(emissionsData);

        if (user?.companyId) {
          const [reportData, companyData] = await Promise.all([
            companiesApi.getSustainabilityReport(user.companyId),
            companiesApi.getById(user.companyId),
          ]);
          setReport(reportData);
          setCompany(companyData);
        } else {
          // Admin users may not be linked to a specific company.
          setReport({
            companyId: '',
            totalEmissions: 0,
            scope1: 0,
            scope2: 0,
            scope3: 0,
          });
          setCompany(null);
        }
      } catch (error) {
        console.error('Failed to load data:', error);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [user?.companyId]);

  useEffect(() => {
    if (sessionStorage.getItem('showWelcomeOnce') === '1') {
      setShowWelcome(true);
      sessionStorage.removeItem('showWelcomeOnce');

      const hideTimer = setTimeout(() => {
        setHideWelcome(true);
      }, 1500);

      const removeTimer = setTimeout(() => {
        setShowWelcome(false);
        setHideWelcome(false);
      }, 2000);

      return () => {
        clearTimeout(hideTimer);
        clearTimeout(removeTimer);
      };
    }
  }, []);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const handleCreateAndLinkCompany = async (e: React.FormEvent) => {
    e.preventDefault();
    setLinkError('');
    setLinking(true);

    try {
      const createdCompany = await companiesApi.create({ name: companyName, vatNumber });
      const refreshedAuth = await authApi.linkCompany({ companyId: createdCompany.id });
      updateAuth(refreshedAuth);
      setCompanyName('');
      setVatNumber('');
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setLinkError((err.response?.data as { error?: string } | undefined)?.error || t('failedLinkCompany'));
      } else {
        setLinkError(t('failedLinkCompany'));
      }
    } finally {
      setLinking(false);
    }
  };

  const handleDeleteAccount = async () => {
    if (!window.confirm(t('confirmDeleteAccount')))
      return;

    setDeletingAccount(true);
    try {
      await authApi.deleteMe();
      logout();
      navigate('/login');
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        alert((err.response?.data as { error?: string } | undefined)?.error || t('failedDeleteAccount'));
      } else {
        alert(t('failedDeleteAccount'));
      }
    } finally {
      setDeletingAccount(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {showWelcome && (
        <div className={`fixed top-4 sm:top-6 left-1/2 -translate-x-1/2 z-50 ${hideWelcome ? 'animate-slide-out-top' : 'animate-slide-in-top'}`}>
          <div className="px-4 sm:px-6 py-2 sm:py-3 rounded-full bg-emerald-600 text-white shadow-lg font-semibold text-sm sm:text-base whitespace-nowrap">
            {t('welcome')}
          </div>
        </div>
      )}

      {/* Header */}
      <header className="bg-white border-b border-gray-200 sticky top-0 z-40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-3 sm:py-4">
            <div className="flex items-center gap-2 sm:gap-3 flex-1">
              <div className="w-8 sm:w-10 h-8 sm:h-10 bg-emerald-600 rounded-xl flex items-center justify-center flex-shrink-0">
                <Leaf className="w-5 sm:w-6 h-5 sm:h-6 text-white" />
              </div>
              <div className="min-w-0">
                <h1 className="text-lg sm:text-xl font-bold text-gray-900">{t('appName')}</h1>
                <p className="text-xs sm:text-sm text-gray-600 truncate">{user?.email}</p>
              </div>
            </div>

            {/* Desktop Menu */}
            <div className="hidden sm:flex items-center gap-2">
              <LanguageSwitch />

              <button
                type="button"
                onClick={handleDeleteAccount}
                disabled={deletingAccount}
                className="flex items-center gap-1 px-3 sm:px-4 py-2 text-xs sm:text-sm text-red-700 hover:bg-red-50 rounded-lg transition-colors cursor-pointer disabled:opacity-50 whitespace-nowrap"
              >
                <span>{deletingAccount ? t('deleting') : t('deleteAccount')}</span>
              </button>

              <button
                onClick={handleLogout}
                className="flex items-center gap-1 px-2 sm:px-4 py-2 text-xs sm:text-sm text-gray-700 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
              >
                <span>{t('logout')}</span>
              </button>
            </div>

            {/* Mobile Menu */}
            <MobileMenu onLogout={handleLogout} onDelete={handleDeleteAccount} deletingAccount={deletingAccount} />
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 sm:py-8">
        {company && (
          <div className="card mb-4 sm:mb-6 border-emerald-200 bg-emerald-50/40">
            <p className="text-xs sm:text-sm text-gray-600">{t('linkedCompany')}</p>
            <p className="text-base sm:text-lg font-semibold text-gray-900 mt-1">{company.name}</p>
            <p className="text-xs sm:text-sm text-gray-600 mt-1">{t('vat')}: {company.vatNumber}</p>
          </div>
        )}

        {!user?.companyId && (
          <div className="card mb-6 sm:mb-8 border-emerald-200 bg-emerald-50/40">
            <div className="flex gap-2 sm:gap-3 mb-3 sm:mb-4">
              <div className="p-2 rounded-lg bg-emerald-100 text-emerald-700 flex-shrink-0">
                <Building2 className="w-4 sm:w-5 h-4 sm:h-5" />
              </div>
              <div className="min-w-0">
                <h2 className="text-sm sm:text-lg font-semibold text-gray-900">{t('linkAccountTitle')}</h2>
                <p className="text-xs sm:text-sm text-gray-600 mt-0.5">{t('linkAccountDesc')}</p>
              </div>
            </div>

            {linkError && (
              <div className="mb-3 p-2 sm:p-3 bg-red-50 border border-red-200 rounded-lg text-xs sm:text-sm text-red-700">{linkError}</div>
            )}

            <form onSubmit={handleCreateAndLinkCompany} className="grid grid-cols-1 sm:grid-cols-3 gap-2 sm:gap-3">
              <input
                className="input-field text-xs sm:text-sm py-2 sm:py-2.5"
                placeholder={t('companyName')}
                value={companyName}
                onChange={(e) => setCompanyName(e.target.value)}
                required
              />
              <input
                className="input-field text-xs sm:text-sm py-2 sm:py-2.5"
                placeholder={t('vatNumber')}
                value={vatNumber}
                onChange={(e) => setVatNumber(e.target.value)}
                required
              />
              <button type="submit" className="btn-primary text-xs sm:text-sm py-2 sm:py-2.5 disabled:opacity-50" disabled={linking}>
                {linking ? t('linking') : t('createAndLink')}
              </button>
            </form>
          </div>
        )}

        {/* Stats Grid */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4 sm:gap-6 mb-4 sm:mb-6">
          <StatsCard
            title={t('totalEmissions')}
            value={`${report?.totalEmissions.toFixed(2) || 0} kg`}
            icon={<TrendingUp className="w-4 sm:w-6 h-4 sm:h-6" />}
            color="bg-blue-500"
          />
          <StatsCard
            title={t('scope1')}
            value={`${report?.scope1.toFixed(2) || 0} kg`}
            icon={<BarChart3 className="w-4 sm:w-6 h-4 sm:h-6" />}
            color="bg-red-500"
          />
          <StatsCard
            title={t('scope2')}
            value={`${report?.scope2.toFixed(2) || 0} kg`}
            icon={<BarChart3 className="w-4 sm:w-6 h-4 sm:h-6" />}
            color="bg-yellow-500"
          />
          <StatsCard
            title={t('scope3')}
            value={`${report?.scope3.toFixed(2) || 0} kg`}
            icon={<BarChart3 className="w-4 sm:w-6 h-4 sm:h-6" />}
            color="bg-green-500"
          />
        </div>

        <div className="card mb-6 sm:mb-8 border-blue-100 bg-blue-50/40">
          <div className="flex gap-2 sm:gap-3">
            <Info className="w-4 sm:w-5 h-4 sm:h-5 text-blue-700 mt-0.5 flex-shrink-0" />
            <div className="text-xs sm:text-sm text-gray-700 space-y-1">
              <p><strong>{t('scope1')}:</strong> {t('scope1Help')}</p>
              <p><strong>{t('scope2')}:</strong> {t('scope2Help')}</p>
              <p><strong>{t('scope3')}:</strong> {t('scope3Help')}</p>
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-6 mb-6 sm:mb-8">
          <ActionCard
            title={t('addEmissionEntry')}
            description={t('addEmissionDesc')}
            icon={<Plus className="w-5 sm:w-6 h-5 sm:h-6" />}
            onClick={() => navigate('/emissions/create')}
          />
          <ActionCard
            title={t('aiExtraction')}
            description={t('aiExtractionDesc')}
            icon={<Leaf className="w-5 sm:w-6 h-5 sm:h-6" />}
            onClick={() => navigate('/emissions/ai')}
          />
        </div>

        {/* Recent Emissions */}
        <div className="card">
          <h2 className="text-lg sm:text-xl font-semibold text-gray-900 mb-3 sm:mb-4">{t('recentEmissions')}</h2>
          {emissions && emissions.items.length > 0 ? (
            <div className="space-y-2 sm:space-y-3">
              {emissions.items.map((emission) => {
                const expanded = expandedEmissionId === emission.id;
                return (
                  <button
                    type="button"
                    key={emission.id}
                    onClick={() => setExpandedEmissionId(expanded ? null : emission.id)}
                    className="w-full text-left flex flex-col gap-2 p-3 sm:p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors cursor-pointer"
                  >
                    <div className="flex justify-between items-center gap-3">
                      <div className="min-w-0 flex-1">
                        <p className="font-medium text-sm sm:text-base text-gray-900 truncate">{emission.category}</p>
                      </div>
                      <div className="flex items-center gap-2 flex-shrink-0">
                        <p className="text-xs sm:text-sm font-medium text-gray-700 whitespace-nowrap">
                          {emission.amount} {t('units')}
                        </p>
                        <ChevronDown className={`w-4 sm:w-5 h-4 sm:h-5 text-gray-500 transition-transform duration-300 ${expanded ? 'rotate-180' : ''}`} />
                      </div>
                    </div>

                    <div className="flex justify-between items-center gap-2">
                      <p className="text-xs sm:text-sm text-gray-600">
                        {new Date(emission.reportDate).toLocaleDateString()}
                      </p>
                      <p className="font-semibold text-sm sm:text-base text-gray-900 whitespace-nowrap">
                        {emission.co2Equivalent.toFixed(2)} kg CO₂
                      </p>
                    </div>

                    <div className={`overflow-hidden transition-all duration-300 ease-in-out ${expanded ? 'max-h-40 opacity-100' : 'max-h-0 opacity-0'}`}>
                      <div className="pt-2 text-xs sm:text-sm text-gray-600 border-t border-gray-200 mt-1">
                        {emission.rawData}
                      </div>
                    </div>
                  </button>
                );
              })}
            </div>
          ) : (
            <p className="text-gray-600 text-center py-6 sm:py-8 text-sm sm:text-base">{t('noEmissions')}</p>
          )}
        </div>
      </main>
    </div>
  );
}

function StatsCard({ 
  title, 
  value, 
  icon, 
  color 
}: { 
  title: string; 
  value: string; 
  icon: React.ReactNode; 
  color: string 
}) {
  return (
    <div className="card p-4 sm:p-6">
      <div className="flex items-center justify-between gap-2 mb-2 sm:mb-3">
        <p className="text-xs sm:text-sm font-medium text-gray-600 line-clamp-2">{title}</p>
        <div className={`${color} p-1.5 sm:p-2 rounded-lg text-white flex-shrink-0`}>{icon}</div>
      </div>
      <p className="text-lg sm:text-2xl font-bold text-gray-900">{value}</p>
    </div>
  );
}

function ActionCard({ 
  title, 
  description, 
  icon, 
  onClick 
}: { 
  title: string; 
  description: string; 
  icon: React.ReactNode; 
  onClick: () => void 
}) {
  return (
    <button
      onClick={onClick}
      className="card text-left hover:shadow-md transition-shadow group cursor-pointer p-4 sm:p-6"
    >
      <div className="flex gap-3 sm:gap-4">
        <div className="bg-emerald-100 p-2 sm:p-3 rounded-xl text-emerald-600 group-hover:bg-emerald-600 group-hover:text-white transition-colors flex-shrink-0">
          {icon}
        </div>
        <div className="min-w-0">
          <h3 className="text-sm sm:text-lg font-semibold text-gray-900 mb-0.5 sm:mb-1">{title}</h3>
          <p className="text-xs sm:text-sm text-gray-600">{description}</p>
        </div>
      </div>
    </button>
  );
}
