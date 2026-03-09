import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { categoriesApi, emissionsApi } from '../api';
import { useAuthStore } from '../store/authStore';
import type { EmissionCategory } from '../types/api';
import { useI18n } from '../i18n';

const CATEGORY_UNIT_HINTS: Array<{ match: RegExp; unit: string }> = [
  { match: /electric/i, unit: 'kWh' },
  { match: /natural gas|gas/i, unit: 'm3' },
  { match: /fleet fuel|fuel|diesel|petrol|benzin/i, unit: 'liters' },
  { match: /business travel|employee commuting|travel|transport/i, unit: 'km' },
  { match: /water/i, unit: 'm3' },
  { match: /waste/i, unit: 'kg' },
  { match: /purchased goods/i, unit: 'kg' },
];

function getUnitHint(categoryName: string | undefined, defaultHelper: string): { unit: string; helper: string } {
  if (!categoryName) return { unit: 'units', helper: defaultHelper };
  const found = CATEGORY_UNIT_HINTS.find((x) => x.match.test(categoryName));
  return { unit: found?.unit ?? 'units', helper: defaultHelper };
}

export default function CreateEmissionPage() {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const { t, locale } = useI18n();

  const [categories, setCategories] = useState<EmissionCategory[]>([]);
  const [categoryId, setCategoryId] = useState('');
  const [amount, setAmount] = useState('');
  const [reportedDate, setReportedDate] = useState(new Date().toISOString().slice(0, 16));
  const [rawData, setRawData] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const companyId = useMemo(() => user?.companyId ?? '', [user?.companyId]);
  const hasCategories = categories.length > 0;

  useEffect(() => {
    const load = async () => {
      try {
        const data = await categoriesApi.getAll();
        setCategories(data);
        setCategoryId((prev) => {
          if (prev && data.some((c) => c.id === prev)) return prev;
          return data.length > 0 ? data[0].id : '';
        });
      } catch {
        setError(t('failedLoadCategories'));
      }
    };
    load();
  }, [locale]);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!companyId) {
      setError(t('accountNotLinked'));
      return;
    }

    if (!hasCategories) {
      setError(t('noCategoriesAvailable'));
      return;
    }

    setLoading(true);
    try {
      const created = await emissionsApi.create({
        companyId,
        categoryId,
        amount: Number(amount),
        reportedDate: new Date(reportedDate).toISOString(),
        rawData,
      });
      navigate(`/dashboard`, { replace: true, state: { createdEmissionId: created.id } });
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError((err.response?.data as { error?: string } | undefined)?.error || t('failedCreateEmission'));
      } else {
        setError(t('failedCreateEmission'));
      }
    } finally {
      setLoading(false);
    }
  };

  const selectedCategory = categories.find((c) => c.id === categoryId);
  const unitHint = getUnitHint(selectedCategory?.name, t('unitHelperDefault'));
  const unitLabel = unitHint.unit === 'units' ? t('units') : unitHint.unit;

  return (
    <div className="min-h-screen bg-gray-50 p-4 sm:p-6">
      <div className="max-w-3xl mx-auto card">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">{t('createEmission')}</h1>

        {!hasCategories && (
          <div className="mb-4 p-3 bg-amber-50 border border-amber-200 rounded-lg text-sm text-amber-800">
            {t('categoriesEmptyRestart')}
          </div>
        )}

        {error && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>}

        <form onSubmit={submit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">{t('category')}</label>
            <select
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
              className="input-field"
              disabled={!hasCategories}
              required
            >
              {!hasCategories && <option value="">{t('noCategoriesOption')}</option>}
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">{t('amount')} ({unitLabel})</label>
              <input
                className="input-field"
                type="number"
                min="0.01"
                step="0.01"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                placeholder={`${t('enterValueIn')} ${unitLabel}`}
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">{t('reportedDate')}</label>
              <input
                className="input-field !w-auto min-w-[220px] max-w-full"
                type="datetime-local"
                value={reportedDate}
                onChange={(e) => setReportedDate(e.target.value)}
                required
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">{t('rawDataNote')}</label>
            <textarea className="input-field min-h-32" value={rawData} onChange={(e) => setRawData(e.target.value)} placeholder={t('aiTextExample')} required />
          </div>

          <div className="flex gap-3">
            <button type="submit" disabled={loading || !hasCategories} className="btn-primary disabled:opacity-50">{loading ? t('saving') : t('saveEmission')}</button>
            <button type="button" className="btn-secondary" onClick={() => navigate('/dashboard')}>{t('cancel')}</button>
          </div>
        </form>
      </div>
    </div>
  );
}
