import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ShoppingBag, Clock, Truck, AlertCircle, CheckCircle2, XCircle } from 'lucide-react';
import { OrderListItem, OrderStatus } from '../types';
import { orderService } from '../services/orderService';

const statusStyles: Record<OrderStatus, { color: string, icon: any }> = {
  [OrderStatus.Pending]: { color: 'bg-amber-100 text-amber-700 border-amber-200', icon: Clock },
  [OrderStatus.Confirmed]: { color: 'bg-blue-100 text-blue-700 border-blue-200', icon: AlertCircle },
  [OrderStatus.Shipped]: { color: 'bg-purple-100 text-purple-700 border-purple-200', icon: Truck },
  [OrderStatus.Delivered]: { color: 'bg-emerald-100 text-emerald-700 border-emerald-200', icon: CheckCircle2 },
  [OrderStatus.Cancelled]: { color: 'bg-muted text-subtle border-border', icon: XCircle }
};

export function useOrderManagement() {
  const [selectedOrderId, setSelectedOrderId] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<string | undefined>(undefined);
  const [searchTerm, setSearchTerm] = useState('');

  const queryClient = useQueryClient();

  const { data, isLoading, error } = useQuery({
    queryKey: ['admin', 'orders', statusFilter],
    queryFn: () => orderService.getAdminOrders({ status: statusFilter, page: 1, limit: 50 }),
  });

  const cancelMutation = useMutation({
    mutationFn: (orderId: string) => orderService.updateOrderStatus(orderId, 'Cancelled'),
    onSuccess: (_, orderId) => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] });
      queryClient.invalidateQueries({ queryKey: ['orders', 'detail', orderId] });
      toast.success('Order cancelled successfully');
    },
    onError: (err: any) => {
      toast.error(err.message || 'Failed to cancel order');
    }
  });

  const handleCancel = (orderId: string) => {
    if (window.confirm('Admin: Are you sure you want to VOID this order? This will restore stock and process refunds.')) {
      cancelMutation.mutate(orderId);
    }
  };

  const allOrders = data?.items ?? [];

  // Client-side search filtering
  const orders = useMemo(() => {
    if (!searchTerm) return allOrders;
    
    const query = searchTerm.toLowerCase();
    return allOrders.filter(order => 
      order.id.toLowerCase().includes(query) ||
      order.recipientName.toLowerCase().includes(query) ||
      order.email.toLowerCase().includes(query)
    );
  }, [allOrders, searchTerm]);

  const stats = [
    { label: 'Pending', value: allOrders.filter(o => o.status === OrderStatus.Pending).length.toString(), icon: Clock, color: 'text-amber-500', bg: 'bg-amber-50' },
    { label: 'Confirmed', value: allOrders.filter(o => o.status === OrderStatus.Confirmed).length.toString(), icon: AlertCircle, color: 'text-blue-500', bg: 'bg-blue-50' },
    { label: 'Shipped', value: allOrders.filter(o => o.status === OrderStatus.Shipped).length.toString(), icon: Truck, color: 'text-purple-500', bg: 'bg-purple-50' },
    { label: 'Delivered', value: allOrders.filter(o => o.status === OrderStatus.Delivered).length.toString(), icon: CheckCircle2, color: 'text-emerald-500', bg: 'bg-emerald-50' },
    { label: 'Cancelled', value: allOrders.filter(o => o.status === OrderStatus.Cancelled).length.toString(), icon: XCircle, color: 'text-red-500', bg: 'bg-red-50' },
  ];

  return {
    orders,
    isLoading,
    error: error instanceof Error ? error.message : null,
    stats,
    statusStyles,
    selectedOrderId,
    statusFilter,
    searchTerm,
    actions: {
      openDetails: (order: OrderListItem) => setSelectedOrderId(order.id),
      closeDetails: () => setSelectedOrderId(null),
      setStatusFilter,
      setSearchTerm,
      handleCancel
    }
  };
}
