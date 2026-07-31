import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Layout() {
  const { user, isAdmin, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const isActive = (path: string) =>
    location.pathname.startsWith(path) ? 'bg-gray-900 text-white' : 'text-gray-300 hover:bg-gray-700 hover:text-white';

  const isActiveAdmin = (path: string) =>
    location.pathname.startsWith(path)
      ? 'bg-amber-500 text-gray-900'
      : 'text-amber-300 hover:bg-gray-700 hover:text-amber-200';

  return (
    <div className="min-h-screen">
      <nav className="bg-gray-800">
        <div className="max-w-[96rem] mx-auto px-4">
          <div className="flex items-center justify-between h-16">
            <div className="flex items-center gap-4">
              <Link to="/" className="text-white font-bold text-lg">
                Makerspace Fablab
              </Link>
              <div className="hidden md:flex items-center gap-1">
                <Link to="/dashboard" className={`px-3 py-2 rounded-md text-sm font-medium ${isActive('/dashboard')}`}>
                  Ana Sayfa
                </Link>
                <Link to="/announcements" className={`px-3 py-2 rounded-md text-sm font-medium ${isActive('/announcements')}`}>
                  Duyurular
                </Link>
                <Link to="/equipment" className={`px-3 py-2 rounded-md text-sm font-medium ${isActive('/equipment')}`}>
                  Ekipmanlar
                </Link>
                <Link to="/my-equipment" className={`px-3 py-2 rounded-md text-sm font-medium ${isActive('/my-equipment')}`}>
                  Ekipmanlarım
                </Link>
                <Link to="/payments" className={`px-3 py-2 rounded-md text-sm font-medium ${isActive('/payments')}`}>
                  Ödemelerim
                </Link>
              </div>
            </div>
            <div className="flex items-center gap-3">
              {isAdmin && (
                <div className="hidden md:flex items-center gap-1 border-r border-gray-600 pr-3 mr-1">
                  <span className="text-[10px] uppercase tracking-wider text-gray-500 font-semibold mr-1">Yönetim</span>
                  <Link to="/categories" className={`px-3 py-2 rounded-md text-sm font-medium ${isActiveAdmin('/categories')}`}>
                    Kategoriler
                  </Link>
                  <Link to="/users" className={`px-3 py-2 rounded-md text-sm font-medium ${isActiveAdmin('/users')}`}>
                    Kullanıcılar
                  </Link>
                </div>
              )}
              <Link to="/profile" className="text-gray-300 hover:text-white text-sm">
                {user?.firstName} {user?.lastName}
              </Link>
              <button
                onClick={handleLogout}
                className="bg-red-600 hover:bg-red-700 text-white px-3 py-1.5 rounded text-sm"
              >
                Çıkış
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Mobile nav */}
      <div className="md:hidden bg-gray-800 border-t border-gray-700 px-2 pb-2">
        <div className="flex flex-wrap gap-1">
          <Link to="/dashboard" className={`px-3 py-1.5 rounded text-xs font-medium ${isActive('/dashboard')}`}>Ana Sayfa</Link>
          <Link to="/announcements" className={`px-3 py-1.5 rounded text-xs font-medium ${isActive('/announcements')}`}>Duyurular</Link>
          <Link to="/equipment" className={`px-3 py-1.5 rounded text-xs font-medium ${isActive('/equipment')}`}>Ekipmanlar</Link>
          <Link to="/my-equipment" className={`px-3 py-1.5 rounded text-xs font-medium ${isActive('/my-equipment')}`}>Ekipmanlarım</Link>
          <Link to="/payments" className={`px-3 py-1.5 rounded text-xs font-medium ${isActive('/payments')}`}>Ödemelerim</Link>
          {isAdmin && (
            <>
              <Link to="/categories" className={`px-3 py-1.5 rounded text-xs font-medium ${isActive('/categories')}`}>Kategoriler</Link>
              <Link to="/users" className={`px-3 py-1.5 rounded text-xs font-medium ${isActive('/users')}`}>Kullanıcılar</Link>
            </>
          )}
        </div>
      </div>

      <main className="max-w-[96rem] mx-auto px-4 py-6">
        <Outlet />
      </main>
    </div>
  );
}
