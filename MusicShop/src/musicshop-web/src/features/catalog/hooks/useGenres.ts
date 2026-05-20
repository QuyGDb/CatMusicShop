import { createCrudHooks } from '@/shared/hooks/createCrudHooks';
import { genreService, CreateGenreRequest, UpdateGenreRequest } from '../services/genreService';
import { Genre } from '../types';

const genreHooks = createCrudHooks<Genre, CreateGenreRequest, UpdateGenreRequest>({
  queryKey: 'genres',
  service: {
    getAll: genreService.getGenres,
    getBySlug: genreService.getGenreBySlug,
    create: genreService.createGenre,
    update: genreService.updateGenre,
    delete: genreService.deleteGenre,
  },
  entityName: 'Genre',
});

export const useGenres = genreHooks.useList;
export const useCreateGenre = genreHooks.useCreate;
export const useUpdateGenre = genreHooks.useUpdate;
export const useDeleteGenre = genreHooks.useDelete;
