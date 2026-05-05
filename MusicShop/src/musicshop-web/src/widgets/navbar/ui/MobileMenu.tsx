import React from 'react';
import { Link } from 'react-router-dom';
import { X, User as UserIcon, Shield, Package, LogOut, Search } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { Button } from '@/shared/components';

interface MobileMenuProps {
  isOpen: boolean;
  onClose: () => void;
  isAuthenticated: boolean;
  user: any;
  logout: () => void;
  navLinks: Array<{ name: string; href: string }>;
  pathname: string;
}

export function MobileMenu({
  isOpen,
  onClose,
  isAuthenticated,
  user,
  logout,
  navLinks,
  pathname
}: MobileMenuProps) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[100] md:hidden">
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-background/80 backdrop-blur-sm animate-in fade-in duration-300" 
        onClick={onClose}
      />
      
      {/* Menu Content */}
      <div className={cn(
        "fixed inset-y-0 right-0 w-[280px] bg-surface border-l border-border shadow-2xl p-6 flex flex-col gap-8 animate-in slide-in-from-right duration-300 ease-out"
      )}>
        <div className="flex items-center justify-between">
          <span className="text-lg font-black tracking-tighter uppercase text-foreground">Menu</span>
          <button onClick={onClose} className="p-2 text-muted-foreground hover:text-primary">
            <X className="h-6 w-6" />
          </button>
        </div>

        {/* Search in Mobile Menu */}
        <div className="relative">
          <Search className="h-4 w-4 absolute left-3 top-1/2 -translate-y-1/2 text-subtle" />
          <input
            type="text"
            placeholder="Search albums..."
            className="w-full bg-muted border border-border rounded-xl py-2.5 pl-10 pr-4 text-sm text-foreground focus:outline-none focus:border-primary transition-all"
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                const query = e.currentTarget.value.trim();
                if (query) {
                  window.location.href = `/products?q=${encodeURIComponent(query)}`;
                  onClose();
                }
              }
            }}
          />
        </div>

        {/* Navigation Links */}
        <nav className="flex flex-col gap-2">
          {navLinks.map((link) => (
            <Link
              key={link.href}
              to={link.href}
              onClick={onClose}
              className={cn(
                "px-4 py-3 rounded-xl text-sm font-semibold transition-all",
                pathname === link.href 
                  ? "bg-primary/10 text-primary" 
                  : "text-muted-foreground hover:bg-muted hover:text-foreground"
              )}
            >
              {link.name}
            </Link>
          ))}
        </nav>

        <div className="mt-auto pt-8 border-t border-border flex flex-col gap-4">
          {isAuthenticated ? (
            <>
              <div className="px-4 py-2">
                <p className="text-xs font-black uppercase tracking-widest text-subtle mb-3">Account</p>
                <div className="flex flex-col gap-2">
                  {user?.role?.toLowerCase() === 'admin' && (
                    <Link 
                      to="/admin" 
                      onClick={onClose}
                      className="flex items-center gap-3 text-sm font-medium text-muted-foreground hover:text-primary transition-colors"
                    >
                      <Shield className="h-4 w-4" />
                      Admin Panel
                    </Link>
                  )}
                  {user?.role?.toLowerCase() !== 'admin' && (
                    <Link 
                      to="/orders" 
                      onClick={onClose}
                      className="flex items-center gap-3 text-sm font-medium text-muted-foreground hover:text-primary transition-colors"
                    >
                      <Package className="h-4 w-4" />
                      My Orders
                    </Link>
                  )}
                  <Link 
                    to="/profile" 
                    onClick={onClose}
                    className="flex items-center gap-3 text-sm font-medium text-muted-foreground hover:text-primary transition-colors"
                  >
                    <UserIcon className="h-4 w-4" />
                    {user?.fullName}
                  </Link>
                </div>
              </div>
              <Button 
                variant="ghost" 
                onClick={() => {
                  logout();
                  onClose();
                }}
                className="w-full justify-start text-red-500 hover:text-red-600 hover:bg-red-50"
              >
                <LogOut className="h-4 w-4 mr-3" />
                Sign Out
              </Button>
            </>
          ) : (
            <Link
              to="/login"
              onClick={onClose}
              className="w-full bg-primary hover:bg-primary-dark text-primary-foreground py-3 rounded-xl text-center font-bold shadow-lg shadow-primary/20 transition-all"
            >
              Sign In
            </Link>
          )}
        </div>
      </div>
    </div>
  );
}
