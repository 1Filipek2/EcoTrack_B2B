import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  BarChart3,
  TrendingUp,
  Leaf,
  LogOut,
  Plus,
  Building2,
} from 'lucide-react';
import axios from 'axios';
import { authApi, companiesApi, emissionsApi } from '../api';
import { useAuthStore } from '../store/authStore';
import type { SustainabilityReport, PagedResult, EmissionEntry } from '../types/api';

export default function DashboardPage() {
  const { user, logout, updateAuth } = useAuthStore();
  const navigate = useNavigate();

  const [report, setReport] = useState<SustainabilityReport | null>(null);
  const [emissions, setEmissions] = useState<PagedResult<EmissionEntry> | null>(null);
  const [loading, setLoading] = useState(true);
  const [linkError, setLinkError] = useState('');
  const [companyName, setCompanyName] = useState('');
  const [vatNumber, setVatNumber] = useState('');
  const [linking, setLinking] = useState(false);

  useEffect(() => {
    const loadData = async () => {
      try {
        const emissionsData = await emissionsApi.getAll({ pageNumber: 1, pageSize: 5 });
        setEmissions(emissionsData);

        if (user?.companyId) {
          const reportData = await companiesApi.getSustainabilityReport(user.companyId);
          setReport(reportData);
        } else {
          // Admin users may not be linked to a specific company.
          setReport({
            companyId: '',
            totalEmissions: 0,
            scope1: 0,
            scope2: 0,
            scope3: 0,
          });
        }
      } catch (error) {
        console.error('Failed to load data:', error);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [user?.companyId]);

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
        setLinkError((err.response?.data as { error?: string } | undefined)?.error || 'Failed to link company.');
      } else {
        setLinkError('Failed to link company.');
      }
    } finally {
      setLinking(false);
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
      {/* Header */}
      <header className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-4">
            <div className="flex items-center space-x-3">
              <div className="w-10 h-10 bg-emerald-600 rounded-xl flex items-center justify-center">
                <Leaf className="w-6 h-6 text-white" />
              </div>
              <div>
                <h1 className="text-xl font-bold text-gray-900">EcoTrack B2B</h1>
                <p className="text-sm text-gray-600">{user?.email}</p>
              </div>
            </div>
            
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <LogOut className="w-5 h-5" />
              <span>Logout</span>
            </button>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {!user?.companyId && (
          <div className="card mb-8 border-emerald-200 bg-emerald-50/40">
            <div className="flex items-start gap-3 mb-4">
              <div className="p-2 rounded-lg bg-emerald-100 text-emerald-700">
                <Building2 className="w-5 h-5" />
              </div>
              <div>
                <h2 className="text-lg font-semibold text-gray-900">Link Your Account to a Company</h2>
                <p className="text-sm text-gray-600">You need a linked company before creating emissions.</p>
              </div>
            </div>

            {linkError && (
              <div className="mb-3 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{linkError}</div>
            )}

            <form onSubmit={handleCreateAndLinkCompany} className="grid grid-cols-1 md:grid-cols-3 gap-3">
              <input
                className="input-field"
                placeholder="Company name"
                value={companyName}
                onChange={(e) => setCompanyName(e.target.value)}
                required
              />
              <input
                className="input-field"
                placeholder="VAT number"
                value={vatNumber}
                onChange={(e) => setVatNumber(e.target.value)}
                required
              />
              <button type="submit" className="btn-primary disabled:opacity-50" disabled={linking}>
                {linking ? 'Linking...' : 'Create & Link Company'}
              </button>
            </form>
          </div>
        )}

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <StatsCard
            title="Total Emissions"
            value={`${report?.totalEmissions.toFixed(2) || 0} kg`}
            icon={<TrendingUp className="w-6 h-6" />}
            color="bg-blue-500"
          />
          <StatsCard
            title="Scope 1"
            value={`${report?.scope1.toFixed(2) || 0} kg`}
            icon={<BarChart3 className="w-6 h-6" />}
            color="bg-red-500"
          />
          <StatsCard
            title="Scope 2"
            value={`${report?.scope2.toFixed(2) || 0} kg`}
            icon={<BarChart3 className="w-6 h-6" />}
            color="bg-yellow-500"
          />
          <StatsCard
            title="Scope 3"
            value={`${report?.scope3.toFixed(2) || 0} kg`}
            icon={<BarChart3 className="w-6 h-6" />}
            color="bg-green-500"
          />
        </div>

        {/* Actions */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <ActionCard
            title="Add Emission Entry"
            description="Record a new emission manually"
            icon={<Plus className="w-6 h-6" />}
            onClick={() => navigate('/emissions/create')}
          />
          <ActionCard
            title="AI Text Extraction"
            description="Extract emissions from text using AI"
            icon={<Leaf className="w-6 h-6" />}
            onClick={() => navigate('/emissions/ai')}
          />
        </div>

        {/* Recent Emissions */}
        <div className="card">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Recent Emissions</h2>
          {emissions && emissions.items.length > 0 ? (
            <div className="space-y-3">
              {emissions.items.map((emission) => (
                <div
                  key={emission.id}
                  className="flex justify-between items-center p-4 bg-gray-50 rounded-lg"
                >
                  <div>
                    <p className="font-medium text-gray-900">{emission.category}</p>
                    <p className="text-sm text-gray-600">
                      {new Date(emission.reportDate).toLocaleDateString()}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="font-semibold text-gray-900">
                      {emission.co2Equivalent.toFixed(2)} kg CO₂
                    </p>
                    <p className="text-sm text-gray-600">{emission.amount} units</p>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-gray-600 text-center py-8">No emissions recorded yet</p>
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
    <div className="card">
      <div className="flex items-center justify-between mb-2">
        <p className="text-sm font-medium text-gray-600">{title}</p>
        <div className={`${color} p-2 rounded-lg text-white`}>{icon}</div>
      </div>
      <p className="text-2xl font-bold text-gray-900">{value}</p>
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
      className="card text-left hover:shadow-md transition-shadow group"
    >
      <div className="flex items-start space-x-4">
        <div className="bg-emerald-100 p-3 rounded-xl text-emerald-600 group-hover:bg-emerald-600 group-hover:text-white transition-colors">
          {icon}
        </div>
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-1">{title}</h3>
          <p className="text-sm text-gray-600">{description}</p>
        </div>
      </div>
    </button>
  );
}
