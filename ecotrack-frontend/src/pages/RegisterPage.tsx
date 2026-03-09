import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Building2, Leaf, Lock, Mail, Code } from 'lucide-react';
import axios from 'axios';
import { authApi } from '../api';
import { useI18n } from '../i18n';
import LanguageSwitch from '../components/LanguageSwitch';

export default function RegisterPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [companyId, setCompanyId] = useState('');
  const [verificationCode, setVerificationCode] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);
  const [step, setStep] = useState<'register' | 'verify'>('register');

  const navigate = useNavigate();
  const { t } = useI18n();

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      const response = await authApi.register({
        email,
        password,
        companyId: companyId.trim() ? companyId.trim() : undefined,
      });

      // Check if dev mode (email verification skipped)
      if (response.message.includes('Dev mode') || response.message.includes('verification skipped')) {
        setSuccess(t('registerSuccess'));
        setTimeout(() => navigate('/login'), 1500);
      } else {
        setSuccess(t('checkEmailForCode'));
        setStep('verify');
      }
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError((err.response?.data as { error?: string } | undefined)?.error || t('registerFailed'));
      } else {
        setError(t('registerFailed'));
      }
    } finally {
      setLoading(false);
    }
  };

  const handleVerify = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      await authApi.verifyEmail(email, verificationCode);
      setSuccess(t('verifySuccess'));
      setTimeout(() => navigate('/login'), 1500);
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError((err.response?.data as { error?: string } | undefined)?.error || t('verifyFailed'));
      } else {
        setError(t('verifyFailed'));
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-emerald-50 to-gray-100 px-4 py-8 sm:px-6 lg:px-8">
      <div className="w-full max-w-md">
        <div className="flex justify-end mb-3">
          <LanguageSwitch />
        </div>

        <div className="text-center mb-10 sm:mb-12">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-emerald-600 rounded-2xl mb-4 sm:mb-6">
            <Leaf className="w-10 h-10 text-white" />
          </div>
          <h1 className="text-3xl sm:text-4xl font-bold text-gray-900">
            {step === 'register' ? t('createAccount') : t('verifyEmailTitle')}
          </h1>
          <p className="text-gray-600 text-sm sm:text-base mt-3">
            {step === 'register' ? t('startMeasuring') : t('verifyEmailDesc')}
          </p>
        </div>

        <div className="bg-white rounded-2xl shadow-sm border border-gray-200 p-6 sm:p-8">
          {error && (
            <div className="mb-4 p-3 sm:p-4 bg-red-50 border border-red-200 rounded-lg text-xs sm:text-sm text-red-700">
              {error}
            </div>
          )}
          {success && (
            <div className="mb-4 p-3 sm:p-4 bg-green-50 border border-green-200 rounded-lg text-xs sm:text-sm text-green-700">
              {success}
            </div>
          )}

          {step === 'register' ? (
            <form onSubmit={handleRegister} className="space-y-5 sm:space-y-6">
              <div>
                <label className="block text-xs sm:text-sm font-medium text-gray-700 mb-2">{t('email')}</label>
                <div className="relative">
                  <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 sm:w-5 h-4 sm:h-5 text-gray-400" />
                  <input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="w-full pl-10 pr-4 py-2.5 sm:py-3 text-sm sm:text-base border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition"
                    placeholder="you@company.com"
                    required
                  />
                </div>
              </div>

              <div>
                <label className="block text-xs sm:text-sm font-medium text-gray-700 mb-2">{t('password')}</label>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 sm:w-5 h-4 sm:h-5 text-gray-400" />
                  <input
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="w-full pl-10 pr-4 py-2.5 sm:py-3 text-sm sm:text-base border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition"
                    placeholder={t('passwordHint')}
                    minLength={6}
                    required
                  />
                </div>
              </div>

              <div>
                <label className="block text-xs sm:text-sm font-medium text-gray-700 mb-2">{t('companyIdOptional')}</label>
                <div className="relative">
                  <Building2 className="absolute left-3 top-1/2 -translate-y-1/2 w-4 sm:w-5 h-4 sm:h-5 text-gray-400" />
                  <input
                    type="text"
                    value={companyId}
                    onChange={(e) => setCompanyId(e.target.value)}
                    className="w-full pl-10 pr-4 py-2.5 sm:py-3 text-sm sm:text-base border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition"
                    placeholder={t('companyIdHint')}
                  />
                </div>
              </div>

              <button type="submit" disabled={loading} className="w-full py-2.5 sm:py-3 bg-emerald-600 hover:bg-emerald-700 text-white text-sm sm:text-base font-medium rounded-lg transition-colors disabled:opacity-50">
                {loading ? t('creatingAccount') : t('signUp')}
              </button>
            </form>
          ) : (
            <form onSubmit={handleVerify} className="space-y-5 sm:space-y-6">
              <div>
                <label className="block text-xs sm:text-sm font-medium text-gray-700 mb-2">{t('verificationCode')}</label>
                <div className="relative">
                  <Code className="absolute left-3 top-1/2 -translate-y-1/2 w-4 sm:w-5 h-4 sm:h-5 text-gray-400" />
                  <input
                    type="text"
                    value={verificationCode}
                    onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                    className="w-full pl-10 pr-4 py-2.5 sm:py-3 text-sm sm:text-base border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition text-center tracking-widest font-mono text-lg"
                    placeholder="000000"
                    maxLength={6}
                    required
                  />
                </div>
                <p className="text-xs text-gray-500 mt-2">{t('checkEmailForCode')}</p>
              </div>

              <button type="submit" disabled={loading || verificationCode.length !== 6} className="w-full py-2.5 sm:py-3 bg-emerald-600 hover:bg-emerald-700 text-white text-sm sm:text-base font-medium rounded-lg transition-colors disabled:opacity-50">
                {loading ? t('verifying') : t('verifyEmailButton')}
              </button>

              <button
                type="button"
                onClick={() => {
                  setStep('register');
                  setVerificationCode('');
                  setError('');
                  setSuccess('');
                }}
                className="w-full py-2.5 sm:py-3 bg-gray-200 hover:bg-gray-300 text-gray-700 text-sm sm:text-base font-medium rounded-lg transition-colors"
              >
                {t('back')}
              </button>
            </form>
          )}

          <p className="text-xs sm:text-sm text-center text-gray-600 mt-8 sm:mt-10">
            {t('alreadyHaveAccount')}{' '}
            <Link to="/login" className="text-emerald-700 font-semibold hover:underline">
              {t('signIn')}
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
