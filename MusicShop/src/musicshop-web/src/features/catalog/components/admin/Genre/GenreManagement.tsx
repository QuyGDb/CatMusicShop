import { Hash, Edit2, Trash2, Loader2 } from 'lucide-react';
import { Button, Card, CardContent, Skeleton, ManagementLayout, EmptyState } from '@/shared/components';
import { cn } from '@/shared/lib/utils';
import { GenreForm } from './GenreForm';
import { useGenreManagement } from '../../../hooks/useGenreManagement';

const GENRE_COLORS = [
  'bg-pink-500',
  'bg-purple-600',
  'bg-blue-500',
  'bg-cyan-400',
  'bg-red-600',
  'bg-amber-500',
  'bg-emerald-500'
];

/**
 * Presentational component for genre administration.
 * Logic is delegated to useGenreManagement hook.
 */
export function GenreManagement() {
  const {
    items: genres,
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
  } = useGenreManagement();

  const getGenreColor = (name: string) => {
    const index = name.length % GENRE_COLORS.length;
    return GENRE_COLORS[index];
  };

  return (
    <ManagementLayout
      title="Music Genres"
      subtitle="Categorize your catalog with vibrant musical styles."
      createLabel="Add Genre"
      onCreate={form.openCreate}
      searchQuery={searchQuery}
      onSearchChange={setSearchQuery}
      searchPlaceholder="Search genres by name..."
      isLoading={isLoading}
      isEmpty={isEmpty}
      error={error}
      pagination={{ page, totalPages, onPageChange: setPage }}
      skeleton={Array.from({ length: 8 }).map((_, i) => (
        <Skeleton key={i} className="h-20 w-full rounded-2xl bg-muted/50" />
      ))}
      emptyState={
        <EmptyState
          icon={Hash}
          title="No genres found"
          description="No genres found. Time to create some style!"
        />
      }
    >
      {form.isOpen && (
        <GenreForm
          editingGenre={form.editingEntity}
          onClose={form.close}
        />
      )}

      {!form.isOpen && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {genres.map((genre) => (
            <Card key={genre.id} className="bg-surface border-border hover:border-primary/30 transition-all group overflow-hidden">
              <CardContent className="p-4 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className={cn("h-10 w-10 rounded-lg flex items-center justify-center shadow-inner", getGenreColor(genre.name))}>
                    <Hash className="h-5 w-5 text-white" />
                  </div>
                  <div>
                    <h3 className="font-bold text-foreground truncate max-w-[120px]">{genre.name}</h3>
                    <p className="text-[10px] text-muted-foreground uppercase tracking-tight truncate max-w-[120px]">{genre.slug}</p>
                  </div>
                </div>
                <div className="flex flex-col gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-7 w-7 text-muted-foreground hover:text-primary"
                    onClick={() => form.openEdit(genre)}
                  >
                    <Edit2 className="h-3 w-3" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-7 w-7 text-muted-foreground hover:text-red-500"
                    onClick={() => actions.delete(genre.id)}
                    disabled={actions.isDeleting}
                  >
                    {actions.isDeleting && actions.deletingId === genre.id ? (
                      <Loader2 className="h-3 w-3 animate-spin" />
                    ) : (
                      <Trash2 className="h-3 w-3" />
                    )}
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </ManagementLayout>
  );
}
