import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { UseQueryResult, UseMutationResult } from '@tanstack/react-query';
import { PaginatedResponse } from '@/shared/types/api';

interface CrudManagementConfig<TEntity extends { id: string }> {
  useList: (page: number, limit: number, search?: string) => UseQueryResult<PaginatedResponse<TEntity>>;
  useDelete: () => UseMutationResult<void, Error, string>;
  pageSize: number;
  deleteConfirmMessage?: string;
}

interface CrudManagementReturn<TEntity extends { id: string }> {
  items: TEntity[];
  isLoading: boolean;
  error: Error | null;
  isEmpty: boolean;
  page: number;
  setPage: (page: number) => void;
  totalPages: number;
  searchQuery: string;
  setSearchQuery: (query: string) => void;
  form: {
    isOpen: boolean;
    editingEntity: TEntity | null;
    openCreate: () => void;
    openEdit: (entity: TEntity) => void;
    close: () => void;
  };
  actions: {
    delete: (id: string) => void;
    isDeleting: boolean;
    deletingId: string | undefined;
  };
}

/**
 * Generic management hook for admin CRUD pages.
 * Handles debounced search, URL-synced pagination, form state, and delete confirmation.
 *
 * Replaces the identical useGenreManagement / useArtistManagement / useLabelManagement hooks.
 *
 * @example
 * export function useGenreManagement() {
 *   return useCrudManagement<Genre>({
 *     useList: (page, limit, search) => useGenres(page, limit, search),
 *     useDelete: useDeleteGenre,
 *     pageSize: 20,
 *     deleteConfirmMessage: 'Are you sure you want to delete this genre?',
 *   });
 * }
 */
export function useCrudManagement<TEntity extends { id: string }>(
  config: CrudManagementConfig<TEntity>
): CrudManagementReturn<TEntity> {
  const { pageSize, deleteConfirmMessage = 'Are you sure you want to delete this item?' } = config;

  const [searchParams, setSearchParams] = useSearchParams();
  const page = parseInt(searchParams.get('page') || '1');
  const [searchQuery, setSearchQuery] = useState(searchParams.get('q') || '');
  const [debouncedSearch, setDebouncedSearch] = useState(searchQuery);

  const [showForm, setShowForm] = useState(false);
  const [editingEntity, setEditingEntity] = useState<TEntity | null>(null);

  // Debounce search input
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(searchQuery), 500);
    return () => clearTimeout(timer);
  }, [searchQuery]);

  // Sync debounced search to URL
  useEffect(() => {
    const params = new URLSearchParams(searchParams);
    if (debouncedSearch) {
      params.set('q', debouncedSearch);
    } else {
      params.delete('q');
    }
    params.set('page', '1');
    setSearchParams(params, { replace: true });
  }, [debouncedSearch]);

  const { data, isLoading, error } = config.useList(page, pageSize, debouncedSearch || undefined);
  const deleteMutation = config.useDelete();

  const handleOpenCreate = () => {
    setEditingEntity(null);
    setShowForm(true);
  };

  const handleOpenEdit = (entity: TEntity) => {
    setEditingEntity(entity);
    setShowForm(true);
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditingEntity(null);
  };

  const handleDelete = (id: string) => {
    if (window.confirm(deleteConfirmMessage)) {
      deleteMutation.mutate(id);
    }
  };

  const setPage = (pageNum: number) => {
    const params = new URLSearchParams(searchParams);
    params.set('page', pageNum.toString());
    setSearchParams(params);
  };

  return {
    items: data?.items ?? [],
    isLoading,
    error: error as Error | null,
    isEmpty: !isLoading && (data?.items.length === 0),
    page,
    setPage,
    totalPages: data?.meta ? Math.ceil(data.meta.total / pageSize) : 1,
    searchQuery,
    setSearchQuery,
    form: {
      isOpen: showForm,
      editingEntity,
      openCreate: handleOpenCreate,
      openEdit: handleOpenEdit,
      close: handleCloseForm,
    },
    actions: {
      delete: handleDelete,
      isDeleting: deleteMutation.isPending,
      deletingId: deleteMutation.variables,
    },
  };
}
