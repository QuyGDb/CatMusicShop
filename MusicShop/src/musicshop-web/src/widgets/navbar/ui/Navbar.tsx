import React from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/useAuthStore';
import { useLogout } from '@/features/auth';
import { Button, buttonVariants } from '@/shared/components';
import { Music, LogOut, User as UserIcon, Search, Shield, Package, ShoppingCart, Menu } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { useCartUIStore } from '@/features/cart/store/useCartUIStore';
import { useCart } from '@/features/cart/hooks/useCart';
import { MobileMenu } from './MobileMenu';

export function Navbar() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = React.useState(false);
  const accessToken = useAuthStore((state) => state.accessToken);
  const user = useAuthStore((state) => state.user);
  const isAuthenticated = !!accessToken;
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const { logout } = useLogout();
  const openCart = useCartUIStore((state) => state.openCart);
  const { cart } = useCart();

  const navLinks = [
    { name: 'Home', href: '/' },
    { name: 'Shop', href: '/products' },
  ];

  return (
    <header className="border-b border-border bg-surface/80 backdrop-blur-md sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        <div className="flex items-center gap-8">
          <Link to="/" className="flex items-center gap-2">
            <Music className="h-8 w-8 text-primary" />
            <span className="text-xl font-black tracking-tighter text-foreground uppercase">CAT MUSIC SHOP</span>
          </Link>

          <nav className="hidden md:flex items-center gap-6">
            {navLinks.map((link) => (
              <Link
                key={link.href}
                to={link.href}
                className={cn(
                  "text-sm font-medium transition-colors hover:text-primary",
                  pathname === link.href ? "text-primary font-semibold" : "text-muted-foreground"
                )}
              >
                {link.name}
              </Link>
            ))}
          </nav>
        </div>

        <div className="flex items-center gap-4">
          <form 
            onSubmit={(e) => {
              e.preventDefault();
              const formData = new FormData(e.currentTarget);
              const query = formData.get('q')?.toString().trim();
              if (query) {
                navigate(`/products?q=${encodeURIComponent(query)}`);
              }
            }}
            className="hidden lg:flex items-center relative mr-2"
          >
            <Search className="h-4 w-4 absolute left-3 text-subtle" />
            <input
              type="text"
              name="q"
              placeholder="Search albums..."
              className="bg-muted border border-border rounded-full py-1.5 pl-9 pr-4 text-xs w-48 focus:outline-none focus:border-primary transition-all text-foreground placeholder:text-subtle"
            />
          </form>

          <div className="flex items-center gap-1 sm:gap-2">
            {user?.role?.toLowerCase() !== 'admin' && (
              <button
                onClick={openCart}
                className="relative p-2 text-muted-foreground hover:text-primary transition-colors rounded-full hover:bg-muted"
              >
                <ShoppingCart className="h-5 w-5" />
                {cart && cart.totalItems > 0 && (
                  <span className="absolute top-0 right-0 h-4 min-w-4 px-1 rounded-full bg-primary text-[10px] font-bold text-primary-foreground flex items-center justify-center transform translate-x-1 -translate-y-1">
                    {cart.totalItems > 99 ? '99+' : cart.totalItems}
                  </span>
                )}
              </button>
            )}

            <div className="hidden md:flex items-center gap-4">
              {isAuthenticated ? (
                <>
                  {user?.role?.toLowerCase() === 'admin' && (
                    <Link to="/admin" className={cn(
                      "text-sm font-medium hover:text-primary transition-colors flex items-center gap-1 mr-2",
                      pathname.startsWith('/admin') ? "text-primary" : "text-muted-foreground"
                    )}>
                      <Shield className="h-4 w-4" />
                      <span className="hidden sm:inline">Admin Panel</span>
                    </Link>
                  )}
                  {user?.role?.toLowerCase() !== 'admin' && (
                    <Link to="/orders" className={cn(
                      "text-sm font-medium hover:text-primary transition-colors flex items-center gap-1",
                      pathname.startsWith('/orders') ? "text-primary" : "text-muted-foreground"
                    )}>
                      <Package className="h-4 w-4" />
                      <span className="hidden sm:inline">Orders</span>
                    </Link>
                  )}
                  <Link to="/profile" className={cn(
                    "text-sm font-medium hover:text-primary transition-colors flex items-center gap-1",
                    pathname === '/profile' ? "text-primary" : "text-muted-foreground"
                  )}>
                    <UserIcon className="h-4 w-4" />
                    <span className="hidden sm:inline">{user?.fullName}</span>
                  </Link>
                  <Button variant="ghost" size="sm" onClick={logout} className="text-muted-foreground hover:text-foreground hover:bg-muted">
                    <LogOut className="h-4 w-4 mr-1" />
                    <span className="hidden lg:inline">Logout</span>
                  </Button>
                </>
              ) : (
                <Link
                  to="/login"
                  className={cn(buttonVariants({ variant: 'default' }), "bg-primary hover:bg-primary-dark text-primary-foreground")}
                >
                  Sign In
                </Link>
              )}
            </div>

            {/* Mobile Menu Toggle */}
            <button
              onClick={() => setIsMobileMenuOpen(true)}
              className="p-2 text-muted-foreground hover:text-primary transition-colors rounded-full hover:bg-muted md:hidden"
            >
              <Menu className="h-6 w-6" />
            </button>
          </div>
        </div>
      </div>

      <MobileMenu 
        isOpen={isMobileMenuOpen}
        onClose={() => setIsMobileMenuOpen(false)}
        isAuthenticated={isAuthenticated}
        user={user}
        logout={logout}
        navLinks={navLinks}
        pathname={pathname}
      />
    </header>

  );
}
