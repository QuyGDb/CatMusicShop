import { useCrudManagement } from '@/shared/hooks/useCrudManagement';
import { useGenres, useDeleteGenre } from './useGenres';
import { Genre } from '../types';

export function useGenreManagement() {
  return useCrudManagement<Genre>({
    useList: (page, limit, search) => useGenres(page, limit, search),
    useDelete: useDeleteGenre,
    pageSize: 20,
    deleteConfirmMessage: 'Are you sure you want to delete this genre?',
  });
}
