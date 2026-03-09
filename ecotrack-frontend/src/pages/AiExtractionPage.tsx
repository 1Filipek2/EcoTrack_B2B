import { useMemo, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import { emissionsApi } from '../api';
import { useAuthStore } from '../store/authStore';

export default function AiExtractionPage() {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const companyId = useMemo(() => user?.companyId ?? '', [user?.companyId]);

  const [rawText, setRawText] = useState('');
  const [reportedDate, setReportedDate] = useState(new Date().toISOString().slice(0, 16));
  const [createdId, setCreatedId] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setCreatedId('');

    if (!companyId) {
      setError('Your account is not linked to a company.');
      return;
    }

    setLoading(true);
    try {
      const id = await emissionsApi.processUnstructured({
        companyId,
        rawText,
        reportedDate: new Date(reportedDate).toISOString(),
      });
      setCreatedId(String(id));
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError((err.response?.data as { error?: string } | undefined)?.error || 'AI extraction failed.');
      } else {
        setError('AI extraction failed.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 p-4 sm:p-6">
      <div className="max-w-3xl mx-auto card">
        <h1 className="text-2xl font-bold text-gray-900 mb-2">AI Text Extraction</h1>
        <p className="text-gray-600 mb-6">Paste unstructured text (e.g. "Let BA-London, 2 osoby") and backend will create emission entry.</p>

        {error && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>}
        {createdId && <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg text-sm text-green-700">Created emission ID: {createdId}</div>}

        <form onSubmit={submit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Reported Date</label>
            <input className="input-field" type="datetime-local" value={reportedDate} onChange={(e) => setReportedDate(e.target.value)} required />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Raw Text</label>
            <textarea
              className="input-field min-h-40"
              value={rawText}
              onChange={(e) => setRawText(e.target.value)}
              placeholder="Example: Let BA-Londyn, 2 osoby"
              required
            />
          </div>

          <div className="flex gap-3">
            <button type="submit" disabled={loading} className="btn-primary disabled:opacity-50">{loading ? 'Processing...' : 'Process with AI'}</button>
            <button type="button" className="btn-secondary" onClick={() => navigate('/dashboard')}>Back</button>
          </div>
        </form>
      </div>
    </div>
  );
}

