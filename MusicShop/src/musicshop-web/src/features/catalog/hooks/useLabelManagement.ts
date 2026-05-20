import { useCrudManagement } from '@/shared/hooks/useCrudManagement';
import { useLabels, useDeleteLabel } from './useLabels';
import { Label } from '../types';

export function useLabelManagement() {
  return useCrudManagement<Label>({
    useList: (page, limit, search) => useLabels(page, limit, search),
    useDelete: useDeleteLabel,
    pageSize: 12,
    deleteConfirmMessage: 'Are you sure you want to delete this label?',
  });
}
