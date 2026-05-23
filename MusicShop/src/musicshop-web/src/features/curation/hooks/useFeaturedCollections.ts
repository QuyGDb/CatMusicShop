import { useQuery } from '@tanstack/react-query';
import { curationService } from '../services/curationService';
import { curationKeys } from '../queryKeys';

export function useFeaturedCollections(count = 3) {
  const featuredQuery = useQuery({
    queryKey: curationKeys.featured(count),
    queryFn: () => curationService.getFeaturedCollections(count),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  return {
    collections: featuredQuery.data || [],
    isLoading: featuredQuery.isLoading,
    isError: featuredQuery.isError,
  };
}
