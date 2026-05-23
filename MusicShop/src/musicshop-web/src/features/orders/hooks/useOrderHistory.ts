import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { orderService, OrderHistoryFilters } from '../services/orderService';
import { OrderStatus } from '../types';
import { orderKeys } from '../queryKeys';

export function useOrderHistory() {
  const [page, setPage] = useState(1);
  const [selectedStatus, setSelectedStatus] = useState<string | undefined>(undefined);

  const filters: OrderHistoryFilters = {
    status: selectedStatus,
    page,
    limit: 10,
  };

  const { data, isLoading, error } = useQuery({
    queryKey: orderKeys.history(filters),
    queryFn: () => orderService.getOrderHistory(filters),
  });

  const orders = data?.items || [];
  const totalItems = data?.meta?.total || 0;
  const totalPages = data?.meta ? Math.ceil(data.meta.total / 10) : 1;

  const handleStatusFilter = (status: string | undefined) => {
    setSelectedStatus(status);
    setPage(1);
  };

  return {
    orders,
    isLoading,
    isEmpty: !isLoading && orders.length === 0,
    error: error instanceof Error ? error.message : null,
    pagination: {
      currentPage: page,
      totalPages,
      totalItems,
      setPage,
    },
    selectedStatus,
    handleStatusFilter,
    statuses: Object.values(OrderStatus),
  };
}
