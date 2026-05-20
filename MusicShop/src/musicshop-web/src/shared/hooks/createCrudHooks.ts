import { useQuery, useMutation, useQueryClient, UseQueryResult } from '@tanstack/react-query';
import { PaginatedResponse } from '@/shared/types/api';
import { toast } from 'sonner';

/**
 * Interface that any entity service must implement to be used with createCrudHooks.
 * Services already follow this shape naturally (genreService, artistService, etc.).
 */
export interface CrudService<TEntity, TCreate, TUpdate> {
  getAll: (page: number, limit: number, search?: string) => Promise<PaginatedResponse<TEntity>>;
  getBySlug?: (slug: string) => Promise<TEntity>;
  create: (data: TCreate) => Promise<string>;
  update: (id: string, data: TUpdate) => Promise<unknown>;
  delete: (id: string) => Promise<void>;
}

interface CrudHooksConfig<TEntity, TCreate, TUpdate> {
  queryKey: string;
  service: CrudService<TEntity, TCreate, TUpdate>;
  entityName: string;
}

interface CrudHooks<TEntity, TCreate, TUpdate> {
  useList: (page?: number, limit?: number, search?: string) => UseQueryResult<PaginatedResponse<TEntity>>;
  useDetail: (slug: string) => UseQueryResult<TEntity>;
  useCreate: () => ReturnType<typeof useMutation<string, Error, TCreate>>;
  useUpdate: () => ReturnType<typeof useMutation<unknown, Error, { id: string; data: TUpdate }>>;
  useDelete: () => ReturnType<typeof useMutation<void, Error, string>>;
}

/**
 * Factory that generates standard TanStack Query hooks for any CRUD entity.
 * Eliminates boilerplate across Genre, Artist, Label, Release services.
 *
 * @example
 * const genreHooks = createCrudHooks({ queryKey: 'genres', service: genreService, entityName: 'Genre' });
 * export const useGenres = genreHooks.useList;
 * export const useCreateGenre = genreHooks.useCreate;
 */
export function createCrudHooks<TEntity, TCreate, TUpdate>(
  config: CrudHooksConfig<TEntity, TCreate, TUpdate>
): CrudHooks<TEntity, TCreate, TUpdate> {
  const { queryKey, service, entityName } = config;

  function useList(page = 1, limit = 10, search?: string): UseQueryResult<PaginatedResponse<TEntity>> {
    return useQuery({
      queryKey: [queryKey, { page, limit, search }],
      queryFn: () => service.getAll(page, limit, search),
    });
  }

  function useDetail(slug: string): UseQueryResult<TEntity> {
    return useQuery({
      queryKey: [queryKey, slug],
      queryFn: () => {
        if (!service.getBySlug) {
          throw new Error(`${entityName} service does not support getBySlug`);
        }
        return service.getBySlug(slug);
      },
      enabled: !!slug && !!service.getBySlug,
    });
  }

  function useCreate() {
    const queryClient = useQueryClient();

    return useMutation<string, Error, TCreate>({
      mutationFn: (data: TCreate) => service.create(data),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: [queryKey] });
        toast.success(`${entityName} created successfully`);
      },
      onError: (error: Error) => {
        toast.error(error.message || `Failed to create ${entityName.toLowerCase()}`);
      },
    });
  }

  function useUpdate() {
    const queryClient = useQueryClient();

    return useMutation<unknown, Error, { id: string; data: TUpdate }>({
      mutationFn: ({ id, data }: { id: string; data: TUpdate }) => service.update(id, data),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: [queryKey] });
        toast.success(`${entityName} updated successfully`);
      },
      onError: (error: Error) => {
        toast.error(error.message || `Failed to update ${entityName.toLowerCase()}`);
      },
    });
  }

  function useDelete() {
    const queryClient = useQueryClient();

    return useMutation<void, Error, string>({
      mutationFn: (id: string) => service.delete(id),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: [queryKey] });
        toast.success(`${entityName} deleted successfully`);
      },
      onError: (error: Error) => {
        toast.error(error.message || `Failed to delete ${entityName.toLowerCase()}`);
      },
    });
  }

  return { useList, useDetail, useCreate, useUpdate, useDelete };
}
