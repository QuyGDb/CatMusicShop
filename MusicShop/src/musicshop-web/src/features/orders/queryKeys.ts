import { OrderHistoryFilters } from '@/features/orders/services/orderService';

export const orderKeys = {
  all: ['orders'] as const,
  history: (filters: OrderHistoryFilters) => [...orderKeys.all, 'history', filters] as const,
  detail: (orderId: string) => [...orderKeys.all, 'detail', orderId] as const,
  admin: {
    all: ['admin', 'orders'] as const,
    list: (statusFilter?: string) => ['admin', 'orders', statusFilter] as const,
  },
};
