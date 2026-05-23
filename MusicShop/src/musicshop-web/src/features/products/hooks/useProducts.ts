import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productService, ProductFilters } from '../services/productService';
import { useProductFilters } from './useProductFilters';
import { toast } from 'sonner';

export function useProductsList() {
  const {
    selectedFormat,
    selectedGenre,
    page,
    searchQuery
  } = useProductFilters();

  const filters: ProductFilters = {
    format: selectedFormat || undefined,
    genre: selectedGenre || undefined,
    page: page,
    limit: 12,
    searchQuery: searchQuery || undefined,
    isActive: true
  };

  const { data, isLoading: loading, error } = useQuery({
    queryKey: ['products', filters],
    queryFn: () => productService.getProducts(filters),
  });

  return {
    products: data?.items || [],
    loading,
    error: error instanceof Error ? error.message : null,
    totalItems: data?.meta?.total || 0,
    totalPages: data?.meta ? Math.ceil(data.meta.total / 12) : 1,
    currentPage: page
  };
}

export function useProducts(filters: ProductFilters) {
  return useQuery({
    queryKey: ['products', filters],
    queryFn: () => productService.getProducts(filters),
  });
}

export function useProduct(slug: string) {
  return useQuery({
    queryKey: ['products', slug],
    queryFn: () => productService.getProductBySlug(slug),
    enabled: !!slug,
  });
}

export function useProductId(id: string) {
  return useQuery({
    queryKey: ['products', 'id', id],
    queryFn: () => productService.getProductById(id),
    enabled: !!id,
  });
}

export function useAdminProduct(id: string) {
  return useQuery({
    queryKey: ['products', 'admin', id],
    queryFn: () => productService.getProductById(id),
    enabled: !!id,
  });
}

export function useCreateProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: unknown) => productService.createProduct(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      toast.success('Product created successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to create product');
    }
  });
}

export function useUpdateProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string, data: unknown }) =>
      productService.updateProduct(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      toast.success('Product updated successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to update product');
    }
  });
}

export function useDeleteProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => productService.deleteProduct(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      toast.success('Product deleted successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to delete product');
    }
  });
}
