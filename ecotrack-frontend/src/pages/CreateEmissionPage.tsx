import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { categoriesApi, emissionsApi } from '../api';
import { useAuthStore } from '../store/authStore';
import type { EmissionCategory } from '../types/api';

const CATEGORY_UNIT_HINTS: Array<{ match: RegExp; unit: string; helper: string }> = [
  { match: /electric/i, unit: 'kWh', helper: 'Electricity is typically tracked in kilowatt-hours.' },
  { match: /natural gas|gas/i, unit: 'm3', helper: 'Natural gas is usually entered in cubic meters.' },
  { match: /fleet fuel|fuel|diesel|petrol|benzin/i, unit: 'liters', helper: 'Fuel usage is typically tracked in liters.' },
  { match: /business travel|employee commuting|travel|transport/i, unit: 'km', helper: 'Travel is usually tracked by distance in kilometers.' },
  { match: /water/i, unit: 'm3', helper: 'Water consumption is usually tracked in cubic meters.' },
  { match: /waste/i, unit: 'kg', helper: 'Waste is commonly tracked in kilograms.' },
  { match: /purchased goods/i, unit: 'kg', helper: 'Purchased goods can be tracked by material weight.' },
];

function getUnitHint(categoryName?: string): { unit: string; helper: string } {
  if (!categoryName) return { unit: 'units', helper: 'Use the unit appropriate for the selected category.' };
  const found = CATEGORY_UNIT_HINTS.find((x) => x.match.test(categoryName));
  return found ?? { unit: 'units', helper: 'Use the unit appropriate for the selected category.' };
}

export default function CreateEmissionPage() {
  const navigate = useNavigate();
  const { user } = useAuthStore();

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
        if (data.length > 0) setCategoryId(data[0].id);
      } catch {
        setError('Failed to load categories.');
      }
    };
    load();
  }, []);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!companyId) {
      setError('Your account is not linked to a company.');
      return;
    }

    if (!hasCategories) {
      setError('No emission categories available yet. Please refresh after backend seeding.');
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
        setError((err.response?.data as { error?: string } | undefined)?.error || 'Failed to create emission.');
      } else {
        setError('Failed to create emission.');
      }
    } finally {
      setLoading(false);
    }
  };

  const selectedCategory = categories.find((c) => c.id === categoryId);
  const unitHint = getUnitHint(selectedCategory?.name);

  return (
    <div className="min-h-screen bg-gray-50 p-4 sm:p-6">
      <div className="max-w-3xl mx-auto card">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Create Emission Entry</h1>

        {!hasCategories && (
          <div className="mb-4 p-3 bg-amber-50 border border-amber-200 rounded-lg text-sm text-amber-800">
            Categories are empty. Restart backend once to trigger default category seed.
          </div>
        )}

        {error && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>}

        <form onSubmit={submit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Category</label>
            <select
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
              className="input-field"
              disabled={!hasCategories}
              required
            >
              {!hasCategories && <option value="">No categories available</option>}
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Amount ({unitHint.unit})</label>
              <input
                className="input-field"
                type="number"
                min="0.01"
                step="0.01"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                placeholder={`Enter value in ${unitHint.unit}`}
                required
              />
              <p className="text-xs text-gray-500 mt-1">{unitHint.helper}</p>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Reported Date</label>
              <input className="input-field" type="datetime-local" value={reportedDate} onChange={(e) => setReportedDate(e.target.value)} required />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Raw Data Note</label>
            <textarea className="input-field min-h-32" value={rawData} onChange={(e) => setRawData(e.target.value)} placeholder="Example: Company used 100 kWh electricity" required />
          </div>

          <div className="flex gap-3">
            <button type="submit" disabled={loading || !hasCategories} className="btn-primary disabled:opacity-50">{loading ? 'Saving...' : 'Save Emission'}</button>
            <button type="button" className="btn-secondary" onClick={() => navigate('/dashboard')}>Cancel</button>
          </div>
        </form>
      </div>
    </div>
  );
}
