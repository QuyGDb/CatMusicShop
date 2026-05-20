import { useCrudManagement } from '@/shared/hooks/useCrudManagement';
import { useArtists, useDeleteArtist } from './useArtists';
import { Artist } from '../types';

export function useArtistManagement() {
  return useCrudManagement<Artist>({
    useList: (page, limit, search) => useArtists(page, limit, search),
    useDelete: useDeleteArtist,
    pageSize: 12,
    deleteConfirmMessage: 'Are you sure you want to delete this artist? This will remove them from the catalog.',
  });
}
