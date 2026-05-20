import { useState } from 'react';
import { useProductsList } from './useProducts';
import { useProductFilters } from './useProductFilters';

export function useProductList() {
  const {
    products,
    loading,
    error,
    totalItems,
    totalPages,
    currentPage,
  } = useProductsList();

  const { setPage } = useProductFilters();
  const [showFilters, setShowFilters] = useState(false);

  const toggleFilters = () => {
    setShowFilters((previous: boolean) => !previous);
  };

  return {
    products,
    loading,
    error,
    totalItems,
    totalPages,
    currentPage,
    showFilters,
    toggleFilters,
    setPage,
  };
}
