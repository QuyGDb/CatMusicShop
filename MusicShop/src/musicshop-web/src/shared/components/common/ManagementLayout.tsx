import React from 'react';
import { Plus, Search, AlertCircle, Loader2 } from 'lucide-react';
import { Button } from '../ui/button';
import { Pagination } from '../ui/pagination';
import { cn } from '@/shared/lib/utils';

interface ManagementLayoutProps {
  title: string;
  subtitle: string;
  createLabel?: string;
  onCreate?: () => void;
  searchQuery?: string;
  onSearchChange?: (val: string) => void;
  searchPlaceholder?: string;
  isLoading?: boolean;
  isEmpty?: boolean;
  error?: any;
  pagination?: {
    page: number;
    totalPages: number;
    onPageChange: (page: number) => void;
  };
  filterContent?: React.ReactNode;
  emptyState?: React.ReactNode;
  skeleton?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
}

export function ManagementLayout({
  title,
  subtitle,
  createLabel,
  onCreate,
  searchQuery,
  onSearchChange,
  searchPlaceholder = "Search...",
  isLoading,
  isEmpty,
  error,
  pagination,
  filterContent,
  emptyState,
  skeleton,
  children,
  className
}: ManagementLayoutProps) {
  if (error) {
    return (
      <div className="flex flex-col items-center justify-center py-20 space-y-4">
        <AlertCircle className="h-12 w-12 text-red-500" />
        <p className="text-xl font-bold">Something went wrong</p>
        <p className="text-muted-foreground text-sm max-w-md text-center">
          We encountered an error while fetching the data. Please try again.
        </p>
        <Button onClick={() => window.location.reload()}>Retry</Button>
      </div>
    );
  }

  return (
    <div className={cn("space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700", className)}>
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex flex-col gap-1">
          <h1 className="text-3xl sm:text-4xl font-extrabold tracking-tight text-foreground">{title}</h1>
          <p className="text-muted-foreground text-sm sm:text-base">{subtitle}</p>
        </div>
        {onCreate && createLabel && (
          <Button
            onClick={onCreate}
            className="w-full sm:w-auto bg-primary hover:bg-primary-dark text-primary-foreground h-12 px-6 rounded-xl shadow-lg shadow-primary/20"
          >
            <Plus className="h-5 w-5 mr-2" />
            {createLabel}
          </Button>
        )}
      </div>

      {/* Filters Bar */}
      {(onSearchChange || filterContent) && (
        <div className="flex flex-col sm:flex-row gap-4 items-stretch sm:items-center">
          {onSearchChange && (
            <div className="relative flex-1">
              <Search className="h-5 w-5 absolute left-4 top-1/2 -translate-y-1/2 text-subtle" />
              <input
                type="text"
                placeholder={searchPlaceholder}
                value={searchQuery}
                onChange={(e) => onSearchChange(e.target.value)}
                className="w-full h-14 bg-surface border border-border rounded-2xl pl-12 pr-4 focus:outline-none focus:border-primary transition-all shadow-sm text-sm"
              />
            </div>
          )}
          {filterContent && <div className="flex gap-2 overflow-x-auto pb-2 sm:pb-0">{filterContent}</div>}
        </div>
      )}


      {/* Main Content */}
      <div className="space-y-8">
        {isLoading ? (
          skeleton
        ) : isEmpty ? (
          emptyState
        ) : (
          children
        )}

        {/* Pagination */}
        {pagination && (
          <Pagination
            currentPage={pagination.page}
            totalPages={pagination.totalPages}
            onPageChange={pagination.onPageChange}
          />
        )}
      </div>
    </div>
  );
}

interface EmptyStateProps {
  icon: React.ComponentType<{ className?: string }>;

  title: string;
  description: string;
  action?: React.ReactNode;
  className?: string;
}

export function EmptyState({ icon: Icon, title, description, action, className }: EmptyStateProps) {
  return (
    <div className={cn("text-center py-20 bg-muted/10 border-2 border-dashed border-border rounded-3xl", className)}>
      <Icon className="h-12 w-12 mx-auto text-muted-foreground/30 mb-4" />
      <h3 className="text-lg font-bold text-foreground mb-1">{title}</h3>
      <p className="text-muted-foreground font-medium mb-6">{description}</p>
      {action && <div>{action}</div>}
    </div>
  );
}
