import { ProductFilters } from '@/features/products/services/productService';

export const productKeys = {
  all: ['products'] as const,
  lists: () => [...productKeys.all, 'list'] as const,
  list: (filters: ProductFilters) => [...productKeys.all, filters] as const,
  details: () => [...productKeys.all, 'detail'] as const,
  detail: (slug: string) => [...productKeys.all, slug] as const,
  byId: (id: string) => [...productKeys.all, 'id', id] as const,
  admin: (id: string) => [...productKeys.all, 'admin', id] as const,
};
