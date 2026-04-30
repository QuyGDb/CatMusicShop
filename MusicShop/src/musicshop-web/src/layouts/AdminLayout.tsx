import { ReactNode } from 'react';
import { Link, useLocation, Outlet } from 'react-router-dom';
import { Package, Users, ShoppingBag, Settings, ArrowLeft, Music, UserCircle, Hash, Tag, Layers } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { useAuthStore } from '@/store/useAuthStore';

interface AdminLayoutProps {
  children?: ReactNode;
}

export function AdminLayout({ children }: AdminLayoutProps) {
  const { pathname } = useLocation();
  const user = useAuthStore((state) => state.user);

  const sidebarGroups = [
    {
      title: 'Music Catalog',
      links: [
        { name: 'Artists', href: '/admin/artists', icon: UserCircle },
        { name: 'Releases', href: '/admin/releases', icon: Music },
        { name: 'Labels', href: '/admin/labels', icon: Tag },
        { name: 'Genres', href: '/admin/genres', icon: Hash },
      ]
    },
    {
      title: 'Store & Customers',
      links: [
        { name: 'Products', href: '/admin/products', icon: Package },
        { name: 'Orders', href: '/admin/orders', icon: ShoppingBag },

        { name: 'Curation', href: '/admin/collections', icon: Layers },
      ]
    }
  ];

  return (
    <div className="flex min-h-screen bg-background text-foreground">
      {/* Sidebar */}
      <aside className="w-64 border-r border-border flex flex-col sticky top-0 h-screen bg-surface">
        <div className="p-6 flex items-center gap-3">
          <div className="p-2 bg-primary rounded-lg shadow-lg shadow-primary/20">
            <Music className="h-6 w-6 text-primary-foreground" />
          </div>
          <span className="text-xl font-bold tracking-tight text-foreground">Admin Panel</span>
        </div>

        <nav className="flex-1 px-4 py-4 space-y-8 overflow-y-auto">
          {sidebarGroups.map((group) => (
            <div key={group.title} className="space-y-2">
              <h3 className="px-4 text-[10px] font-black uppercase tracking-[0.2em] text-muted-foreground/50">
                {group.title}
              </h3>
              <div className="space-y-1">
                {group.links.map((link) => {
                  const Icon = link.icon;
                  const isActive = link.href === '/admin'
                    ? pathname === '/admin'
                    : pathname.startsWith(link.href);
                  return (
                    <Link
                      key={link.href}
                      to={link.href}
                      className={cn(
                        "flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium transition-all group",
                        isActive
                          ? "bg-primary/10 text-primary border-l-2 border-primary rounded-l-none"
                          : "text-muted-foreground hover:bg-muted hover:text-foreground border-l-2 border-transparent"
                      )}
                    >
                      <Icon className={cn("h-5 w-5", isActive ? "text-primary" : "text-subtle group-hover:text-muted-foreground")} />
                      {link.name}
                    </Link>
                  );
                })}
              </div>
            </div>
          ))}
        </nav>

        <div className="p-4 border-t border-border">
          <Link
            to="/"
            className="flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium text-muted-foreground hover:bg-muted hover:text-foreground transition-all"
          >
            <ArrowLeft className="h-5 w-5" />
            Back to Shop
          </Link>

          <div className="mt-4 px-4 py-2 flex items-center gap-3 bg-muted/50 rounded-2xl">
            <div className="h-8 w-8 rounded-full bg-primary/20 flex items-center justify-center border border-primary/10 text-xs font-bold text-primary uppercase">
              {user?.fullName?.charAt(0) || 'A'}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-semibold truncate text-foreground">{user?.fullName}</p>
              <p className="text-xs text-muted-foreground truncate">Administrator</p>
            </div>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 flex flex-col min-w-0 overflow-hidden">
        <header className="h-16 border-b border-border flex items-center justify-between px-8 bg-surface/50 backdrop-blur-sm">
          <h2 className="text-sm font-medium text-muted-foreground">
            MusicShop / <span className="text-foreground capitalize">{pathname.split('/').pop() || 'Admin'}</span>
          </h2>
        </header>

        <section className="flex-1 overflow-y-auto p-8">
          {children || <Outlet />}
        </section>
      </main>
    </div>
  );
}
