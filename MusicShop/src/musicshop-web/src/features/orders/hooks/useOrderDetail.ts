import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { orderService } from '../services/orderService';
import { toast } from 'sonner';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/useAuthStore';

export function useOrderDetail(orderId: string) {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const { data: order, isLoading, error } = useQuery({
    queryKey: ['orders', 'detail', orderId],
    queryFn: () => orderService.getOrderDetail(orderId),
    enabled: !!orderId,
  });

  const { user } = useAuthStore();
  const isAdmin = user?.role.toLowerCase() === 'admin';

  const cancelMutation = useMutation({
    mutationFn: () => isAdmin 
      ? orderService.updateOrderStatus(orderId, 'Cancelled')
      : orderService.cancelOrder(orderId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] });
      queryClient.invalidateQueries({ queryKey: ['orders', 'detail', orderId] });
      toast.success('Order cancelled successfully');
    },
    onError: (err: any) => {
      toast.error(err.message || 'Failed to cancel order');
    },
  });

  const handleCancel = () => {
    const message = isAdmin 
      ? 'Admin: Are you sure you want to VOID this order? This action will restore stock and process refunds if applicable.'
      : 'Are you sure you want to cancel this order? This action cannot be undone.';
      
    if (window.confirm(message)) {
      cancelMutation.mutate();
    }
  };

  const handleBack = () => {
    navigate('/orders');
  };

  return {
    order,
    isLoading,
    error: error instanceof Error ? error.message : null,
    handleCancel,
    isCancelling: cancelMutation.isPending,
    handleBack,
    isAdmin,
  };
}
