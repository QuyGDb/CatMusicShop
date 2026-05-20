import { useParams } from 'react-router-dom';
import { OrderHistory } from '@/features/orders/components/storefront/OrderHistory';
import { OrderDetailView } from '@/features/orders/components/storefront/OrderDetailView';

export default function OrderHistoryPage() {
  const { id } = useParams<{ id: string }>();

  return id ? <OrderDetailView orderId={id} /> : <OrderHistory />;
}
