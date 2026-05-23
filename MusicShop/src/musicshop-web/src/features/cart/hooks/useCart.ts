import { useQuery } from '@tanstack/react-query';
import { cartService } from '../services/cartService';
import { cartKeys } from '../queryKeys';
import { useAuthStore } from '@/store/useAuthStore';

export function useCart() {
  const user = useAuthStore((state) => state.user);
  const isAuthenticated = useAuthStore((state) => !!state.accessToken);
  const isCustomer = isAuthenticated && user?.role?.toLowerCase() !== 'admin';

  const { data: cart, isLoading, error } = useQuery({
    queryKey: cartKeys.all,
    queryFn: () => cartService.getCart(),
    enabled: isCustomer,
    retry: 1,
  });

  return {
    cart,
    isLoading,
    error: error instanceof Error ? error.message : null,
    isEmpty: !cart || cart.items.length === 0,
  };
}
