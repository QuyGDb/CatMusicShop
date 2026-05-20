import { useSearchParams } from 'react-router-dom';
import { useOrderDetail } from '@/features/orders/hooks/useOrderDetail';

export function useCheckoutSuccess() {
  const [searchParams] = useSearchParams();
  const sessionId: string | null = searchParams.get('session_id');
  const orderId: string | null = searchParams.get('order_id');

  const { order, isLoading } = useOrderDetail(orderId || '');

  return {
    order,
    isLoading,
    sessionId,
    orderId,
  };
}
