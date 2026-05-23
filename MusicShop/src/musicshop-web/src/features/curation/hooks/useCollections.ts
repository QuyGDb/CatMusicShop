import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { curationService } from '../services/curationService';
import { curationKeys } from '../queryKeys';
import { toast } from 'sonner';

export function useCollections() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [limit] = useState(10);
  const [search, setSearch] = useState('');

  const collectionsQuery = useQuery({
    queryKey: curationKeys.list({ includeUnpublished: true, page, limit, search }),
    queryFn: () => curationService.getCollections(true, page, limit, search),
  });

  const togglePublishMutation = useMutation({
    mutationFn: ({ id, isPublished }: { id: string; isPublished: boolean }) =>
      curationService.updateCollectionStatus(id, isPublished),
    onSuccess: (_, { isPublished }) => {
      queryClient.invalidateQueries({ queryKey: curationKeys.all });
      toast.success(isPublished ? 'Collection published' : 'Collection moved to drafts');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to update status');
    }
  });

  const deleteMutation = useMutation({
    mutationFn: curationService.deleteCollection,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: curationKeys.all });
      toast.success('Collection deleted successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to delete collection');
    }
  });

  return {
    collections: collectionsQuery.data?.data || [],
    meta: collectionsQuery.data?.meta,
    isLoading: collectionsQuery.isLoading,
    isError: collectionsQuery.isError,
    searchQuery: search,
    onSearchChange: setSearch,
    onPageChange: setPage,
    togglePublish: (id: string, currentStatus: boolean) => 
      togglePublishMutation.mutate({ id, isPublished: !currentStatus }),
    deleteCollection: (id: string) => deleteMutation.mutate(id),
  };
}
