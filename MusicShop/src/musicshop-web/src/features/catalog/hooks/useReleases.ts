import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { createCrudHooks } from '@/shared/hooks/createCrudHooks';
import { releaseService, CreateReleaseRequest, UpdateReleaseRequest, CreateReleaseVersionRequest, UpdateReleaseVersionRequest } from '../services/releaseService';
import { Release } from '../types';
import { toast } from 'sonner';

// Standard CRUD hooks via factory
const releaseHooks = createCrudHooks<Release, CreateReleaseRequest, UpdateReleaseRequest>({
  queryKey: 'releases',
  service: {
    getAll: releaseService.getReleases,
    getBySlug: releaseService.getReleaseBySlug,
    create: releaseService.createRelease,
    update: releaseService.updateRelease,
    delete: releaseService.deleteRelease,
  },
  entityName: 'Release',
});

export const useReleases = releaseHooks.useList;
export const useRelease = releaseHooks.useDetail;
export const useCreateRelease = releaseHooks.useCreate;
export const useUpdateRelease = releaseHooks.useUpdate;
export const useDeleteRelease = releaseHooks.useDelete;

// Release-specific hooks that don't fit the generic pattern

export function useReleaseFormats() {
  return useQuery({
    queryKey: ['releases', 'formats'],
    queryFn: () => releaseService.getReleaseFormats(),
    staleTime: Infinity,
  });
}

export function useReleaseTracks(releaseId: string) {
  return useQuery({
    queryKey: ['releases', releaseId, 'tracks'],
    queryFn: () => releaseService.getReleaseTracks(releaseId),
    enabled: !!releaseId,
  });
}

export function useReleaseVersions(releaseId: string) {
  return useQuery({
    queryKey: ['releases', releaseId, 'versions'],
    queryFn: () => releaseService.getReleaseVersions(releaseId),
    enabled: !!releaseId,
  });
}

export function useCreateReleaseVersion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateReleaseVersionRequest) => releaseService.createReleaseVersion(data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['releases', variables.releaseId, 'versions'] });
      toast.success('Version added to release');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to add version');
    }
  });
}

export function useUpdateReleaseVersion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string, data: UpdateReleaseVersionRequest }) => releaseService.updateReleaseVersion(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['releases', variables.data.releaseId, 'versions'] });
      toast.success('Version updated');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to update version');
    }
  });
}

export function useDeleteReleaseVersion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, releaseId }: { id: string, releaseId: string }) => releaseService.deleteReleaseVersion(id),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['releases', variables.releaseId, 'versions'] });
      toast.success('Version removed');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to remove version');
    }
  });
}
