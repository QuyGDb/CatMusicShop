import { Edit2, Trash2, Globe, ExternalLink, Building, Loader2 } from 'lucide-react';
import { Button, Card, Skeleton, ManagementLayout, EmptyState } from '@/shared/components';
import { cn } from '@/shared/lib/utils';
import { LabelForm } from './LabelForm';
import { useLabelManagement } from '../../../hooks/useLabelManagement';

/**
 * Presentational component for record label administration.
 * Logic is delegated to useLabelManagement hook.
 */
export function LabelManagement() {
  const {
    items: labels,
    isLoading,
    isEmpty,
    error,
    form,
    actions,
    page,
    setPage,
    totalPages,
    searchQuery,
    setSearchQuery
  } = useLabelManagement();

  return (
    <ManagementLayout
      title="Record Labels"
      subtitle="Manage the publishers and partners in your ecosystem."
      createLabel="Add Label"
      onCreate={form.openCreate}
      searchQuery={searchQuery}
      onSearchChange={setSearchQuery}
      searchPlaceholder="Search labels by name or country..."
      isLoading={isLoading}
      isEmpty={isEmpty}
      error={error}
      pagination={{ page, totalPages, onPageChange: setPage }}
      skeleton={Array.from({ length: 4 }).map((_, i) => (
        <Skeleton key={i} className="h-40 w-full rounded-2xl bg-muted/50" />
      ))}
      emptyState={
        <EmptyState 
          icon={Building} 
          title="No labels found" 
          description="No labels found. Start by registering your first partner." 
        />
      }
    >
      {form.isOpen && (
        <LabelForm 
          editingLabel={form.editingEntity} 
          onClose={form.close} 
        />
      )}

      {!form.isOpen && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {labels.map((label) => (
            <Card key={label.id} className="bg-surface border-border shadow-sm hover:shadow-md transition-shadow group overflow-hidden">
              <div className="flex items-stretch h-full">
                <div className="w-24 bg-muted/30 flex items-center justify-center border-r border-border shrink-0">
                    <Building className="h-8 w-8 text-muted-foreground" />
                </div>
                <div className="flex-1 p-6 relative">
                    <div className="flex items-center justify-between mb-4">
                      <h3 className="text-lg font-bold text-foreground">{label.name}</h3>
                      <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                        <Button 
                          variant="ghost" 
                          size="icon" 
                          className="h-8 w-8 text-muted-foreground hover:text-primary"
                          onClick={() => form.openEdit(label)}
                        >
                          <Edit2 className="h-4 w-4" />
                        </Button>
                        <Button 
                          variant="ghost" 
                          size="icon" 
                          className="h-8 w-8 text-muted-foreground hover:text-red-500"
                          onClick={() => actions.delete(label.id)}
                          disabled={actions.isDeleting}
                        >
                          {actions.isDeleting && actions.deletingId === label.id ? (
                            <Loader2 className="h-4 w-4 animate-spin" />
                          ) : (
                            <Trash2 className="h-4 w-4" />
                          )}
                        </Button>
                      </div>
                    </div>
                    
                    <div className="space-y-3">
                      <div className="flex items-center gap-2 text-sm text-muted-foreground">
                        <Globe className="h-4 w-4" />
                        {label.country} {label.foundedYear && `• Est. ${label.foundedYear}`}
                      </div>
                      {label.website && (
                        <a 
                          href={label.website} 
                          target="_blank" 
                          rel="noopener noreferrer"
                          className="flex items-center gap-2 text-sm text-primary hover:underline font-medium"
                        >
                          <ExternalLink className="h-4 w-4" />
                          {label.website.replace(/^https?:\/\//, '')}
                        </a>
                      )}
                    </div>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}
    </ManagementLayout>
  );
}
