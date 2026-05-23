import { useMutation, useQueryClient } from '@tanstack/react-query';
import { checkoutService } from '../services/checkoutService';
import { CreateOrderRequest } from '../types';
import { toast } from 'sonner';

export function useCheckout() {
  const queryClient = useQueryClient();

  const checkoutMutation = useMutation({
    mutationFn: (request: CreateOrderRequest) => checkoutService.createOrder(request),
    onSuccess: (data) => {
      // Clear cart locally since backend clears it
      queryClient.setQueryData(['cart'], null);
      
      // Redirect to Stripe checkout
      if (data.payment.checkoutUrl) {
        window.location.href = data.payment.checkoutUrl;
      } else {
        toast.error('Failed to get checkout URL');
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to create order');
    },
  });

  return {
    handleCheckout: checkoutMutation.mutateAsync,
    isCheckingOut: checkoutMutation.isPending,
    error: checkoutMutation.error,
  };
}
