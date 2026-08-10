import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { authApi } from '../../api/auth';
import { useAuth } from '../../context/AuthContext';

export default function Register() {
  const [form, setForm] = useState({
    username: '',
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const res = await authApi.register(form);
      await login(res.token);
      navigate('/dashboard');
    } catch (err: any) {
      const data = err.response?.data;
      if (data?.errors) {
        const messages = Object.values(data.errors).flat().join(', ');
        setError(messages);
      } else {
        setError(data?.message || data || 'Kayıt başarısız');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 px-4">
      <div className="card w-full max-w-md">
        <h1 className="text-2xl font-bold text-center mb-2">Makerspace Fablab</h1>
        <h2 className="text-lg text-gray-600 text-center mb-6">Kayıt Ol</h2>

        {error && (
          <div className="error-banner mb-6">{error}</div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div className="form-group">
              <label className="label">Ad</label>
              <input
                name="firstName"
                value={form.firstName}
                onChange={handleChange}
                required
                className="input"
              />
            </div>
            <div className="form-group">
              <label className="label">Soyad</label>
              <input
                name="lastName"
                value={form.lastName}
                onChange={handleChange}
                required
                className="input"
              />
            </div>
          </div>
          <div className="form-group">
            <label className="label">Kullanıcı Adı</label>
            <input
              name="username"
              value={form.username}
              onChange={handleChange}
              required
              className="input"
            />
          </div>
          <div className="form-group">
            <label className="label">E-posta</label>
            <input
              name="email"
              type="email"
              value={form.email}
              onChange={handleChange}
              required
              className="input"
            />
          </div>
          <div className="form-group">
            <label className="label">Telefon</label>
            <input
              name="phoneNumber"
              value={form.phoneNumber}
              onChange={handleChange}
              className="input"
            />
          </div>
          <div className="form-group">
            <label className="label">Şifre</label>
            <input
              name="password"
              type="password"
              value={form.password}
              onChange={handleChange}
              required
              className="input"
            />
          </div>
          <button
            type="submit"
            disabled={loading}
            className="btn-primary w-full"
          >
            {loading ? 'Kayıt yapılıyor...' : 'Kayıt Ol'}
          </button>
        </form>

        <p className="text-center text-sm text-gray-500 mt-4">
          Zaten hesabın var mı?{' '}
          <Link to="/login" className="text-blue-600 hover:underline">
            Giriş Yap
          </Link>
        </p>
      </div>
    </div>
  );
}
