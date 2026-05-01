import http from '@/shared/api/axiosInstance';
import { ApiResponse, PaginatedResponse } from '@/shared/types/api';
import { OrderListItem, OrderDetail } from '../types';

export interface OrderHistoryFilters {
  status?: string;
  page?: number;
  limit?: number;
}

export const orderService = {
  getOrderHistory: async (filters: OrderHistoryFilters): Promise<PaginatedResponse<OrderListItem>> => {
    const response = await http.get<ApiResponse<OrderListItem[]>>('/Orders', {
      params: {
        Status: filters.status,
        PageNumber: filters.page,
        PageSize: filters.limit,
      },
    });

    return {
      items: response.data || [],
      meta: response.meta,
    };
  },

  getOrderDetail: async (id: string): Promise<OrderDetail> => {
    const response = await http.get<ApiResponse<OrderDetail>>(`/Orders/${id}`);
    if (!response.data) throw new Error('Order not found');
    return response.data;
  },

  cancelOrder: async (id: string): Promise<void> => {
    await http.post(`/Orders/${id}/cancel`);
  },

  getAdminOrders: async (filters: OrderHistoryFilters): Promise<PaginatedResponse<OrderListItem>> => {
    const response = await http.get<ApiResponse<OrderListItem[]>>('/admin/orders', {
      params: {
        Status: filters.status,
        PageNumber: filters.page,
        PageSize: filters.limit,
      },
    });

    return {
      items: response.data || [],
      meta: response.meta,
    };
  },

  updateOrderStatus: async (id: string, status: string, trackingNumber?: string): Promise<void> => {
    await http.patch(`/admin/orders/${id}/status`, {
      status,
      trackingNumber,
    });
  },
};
