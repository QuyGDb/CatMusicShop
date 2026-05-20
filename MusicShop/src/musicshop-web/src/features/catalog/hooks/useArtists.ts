import { createCrudHooks } from '@/shared/hooks/createCrudHooks';
import { artistService, CreateArtistRequest, UpdateArtistRequest } from '../services/artistService';
import { Artist } from '../types';

const artistHooks = createCrudHooks<Artist, CreateArtistRequest, UpdateArtistRequest>({
  queryKey: 'artists',
  service: {
    getAll: artistService.getArtists,
    getBySlug: artistService.getArtistBySlug,
    create: artistService.createArtist,
    update: artistService.updateArtist,
    delete: artistService.deleteArtist,
  },
  entityName: 'Artist',
});

export const useArtists = artistHooks.useList;
export const useArtist = artistHooks.useDetail;
export const useCreateArtist = artistHooks.useCreate;
export const useUpdateArtist = artistHooks.useUpdate;
export const useDeleteArtist = artistHooks.useDelete;
