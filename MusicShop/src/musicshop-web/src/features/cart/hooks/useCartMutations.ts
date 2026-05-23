import { useMutation, useQueryClient } from '@tanstack/react-query';
import { cartService } from '../services/cartService';
import { AddToCartRequest, UpdateCartItemRequest } from '../types';
import { cartKeys } from '../queryKeys';
import { toast } from 'sonner';

export function useCartMutations() {
  const queryClient = useQueryClient();

  const handleSuccess = () => {
    queryClient.invalidateQueries({ queryKey: cartKeys.all });
  };

  const handleError = (error: any) => {
    toast.error(error.message || 'Failed to update cart');
  };

  const addToCart = useMutation({
    mutationFn: (request: AddToCartRequest) => cartService.addToCart(request),
    onSuccess: () => {
      handleSuccess();
      toast.success('Added to cart');
    },
    onError: handleError,
  });

  const updateItem = useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateCartItemRequest }) =>
      cartService.updateCartItem(id, request),
    onSuccess: handleSuccess,
    onError: handleError,
  });

  const removeItem = useMutation({
    mutationFn: (id: string) => cartService.removeFromCart(id),
    onSuccess: () => {
      handleSuccess();
      toast.success('Item removed from cart');
    },
    onError: handleError,
  });

  const clearCart = useMutation({
    mutationFn: () => cartService.clearCart(),
    onSuccess: () => {
      handleSuccess();
      toast.success('Cart cleared');
    },
    onError: handleError,
  });

  return {
    addToCart,
    updateItem,
    removeItem,
    clearCart,
  };
}
