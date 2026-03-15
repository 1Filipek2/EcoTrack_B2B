import { useMemo, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import { emissionsApi } from '../api';
import { useAuthStore, UserRole } from '../store/authStore';
import { useI18n } from '../i18n';

export default function AiExtractionPage() {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const { t } = useI18n();
  const companyId = useMemo(() => user?.companyId ?? '', [user?.companyId]);
  const forbidden = user?.role !== UserRole.CompanyUser;

  const [rawText, setRawText] = useState('');
  const [reportedDate, setReportedDate] = useState(new Date().toISOString().slice(0, 16));
  const [createdId, setCreatedId] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setCreatedId('');

    if (forbidden) {
      setError(t('forbiddenEmissionAction'));
      return;
    }
    if (!companyId) {
      setError(t('accountNotLinked'));
      return;
    }
    if (rawText.length > 10000) {
      setError(t('rawDataTooLong'));
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
        setError((err.response?.data as { error?: string } | undefined)?.error || t('aiExtractionFailed'));
      } else {
        setError(t('aiExtractionFailed'));
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 p-4 sm:p-6">
      <div className="max-w-3xl mx-auto card">
        <h1 className="text-2xl font-bold text-gray-900 mb-2">{t('aiExtraction')}</h1>
        <p className="text-gray-600 mb-6">{t('aiDescription')}</p>

        {forbidden && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
            {t('forbiddenEmissionAction')}
          </div>
        )}

        {error && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>}
        {createdId && <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg text-sm text-green-700">{t('createdEmissionId')}: {createdId}</div>}

        <form onSubmit={submit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">{t('reportedDate')}</label>
            <input
              className="input-field !w-auto min-w-[220px] max-w-full"
              type="datetime-local"
              value={reportedDate}
              onChange={(e) => setReportedDate(e.target.value)}
              required
              disabled={forbidden}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">{t('rawText')}</label>
            <textarea
              className="input-field min-h-40"
              value={rawText}
              onChange={(e) => setRawText(e.target.value)}
              placeholder={t('aiTextExample')}
              required
              maxLength={10000}
              disabled={forbidden}
            />
          </div>

          <div className="flex gap-3">
            <button type="submit" disabled={loading || forbidden} className="btn-primary disabled:opacity-50">{loading ? t('processing') : t('processWithAi')}</button>
            <button type="button" className="btn-secondary" onClick={() => navigate('/dashboard')}>{t('back')}</button>
          </div>
        </form>
      </div>
    </div>
  );
}
