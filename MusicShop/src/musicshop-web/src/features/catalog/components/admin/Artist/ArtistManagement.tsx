import { Users } from 'lucide-react';
import { Button, Skeleton, ManagementLayout, EmptyState } from '@/shared/components';
import { cn } from '@/shared/lib/utils';
import { ArtistForm } from './ArtistForm';
import { ArtistCard } from './ArtistCard';
import { useArtistManagement } from '../../../hooks/useArtistManagement';

export function ArtistManagement() {
  const {
    items: artists,
    isLoading,
    error,
    isEmpty,
    form,
    actions,
    page,
    setPage,
    totalPages,
    searchQuery,
    setSearchQuery
  } = useArtistManagement();

  return (
    <ManagementLayout
      title="Catalog Artists"
      subtitle="Manage the creators behind your music collection."
      createLabel="Register Artist"
      onCreate={form.openCreate}
      searchQuery={searchQuery}
      onSearchChange={setSearchQuery}
      searchPlaceholder="Search artists by name, country, or genre..."
      isLoading={isLoading}
      isEmpty={isEmpty}
      error={error}
      pagination={{ page, totalPages, onPageChange: setPage }}
      skeleton={Array.from({ length: 8 }).map((_, i) => (
        <Skeleton key={i} className="h-80 w-full rounded-3xl bg-muted/50" />
      ))}
      emptyState={
        <EmptyState
          icon={Users}
          title="No artists found"
          description="No artists found. Time to discover some talent!"
        />
      }
    >
      {form.isOpen && (
        <ArtistForm
          editingArtist={form.editingEntity}
          onClose={form.close}
        />
      )}

      {!form.isOpen && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {artists.map((artist) => (
            <ArtistCard
              key={artist.id}
              artist={artist}
              onEdit={form.openEdit}
              onDelete={actions.delete}
              isDeleting={actions.isDeleting && actions.deletingId === artist.id}
            />
          ))}
        </div>
      )}
    </ManagementLayout>
  );
}
