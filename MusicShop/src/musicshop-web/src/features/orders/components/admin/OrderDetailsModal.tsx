import { X, ShoppingBag, Calendar } from 'lucide-react';
import { Button, Card, CardHeader, CardTitle, CardContent } from '@/shared/components';
import { OrderStatus } from '../../types';

// New specialized components and hooks
import { useOrderForm } from '../../hooks/useOrderForm';
import { OrderItemsList } from './OrderForm/OrderItemsList';
import { OrderSummary } from './OrderForm/OrderSummary';
import { OrderFulfillment } from './OrderForm/OrderFulfillment';
import { CustomerDetails } from './OrderForm/CustomerDetails';

import { useOrderDetail } from '../../hooks/useOrderDetail';

interface OrderDetailsModalProps {
  orderId: string;
  onClose: () => void;
}

export function OrderDetailsModal({ orderId, onClose }: OrderDetailsModalProps) {
  const { order, isLoading, error } = useOrderDetail(orderId);

  const {
    register,
    handleSubmit,
    errors,
    control,
    isSubmitting
  } = useOrderForm({
    orderId,
    initialStatus: order?.status || OrderStatus.Pending,
    onSuccess: onClose,
  });

  if (isLoading) return null; // Or a loading spinner
  if (!order) return null;

  // Map backend order to what sub-components expect
  const mappedData = {
    customer: {
      name: order.recipientName,
      email: order.email, // We will add email to OrderDetail too
      phone: order.phone,
      address: order.shippingAddress
    },
    items: order.items.map(item => ({
      id: item.id,
      title: item.productName,
      format: '', // Not in DTO
      price: item.unitPrice,
      quantity: item.quantity,
      cover: item.productCoverUrl || ''
    })),
    summary: {
      subtotal: order.totalAmount,
      shipping: 0,
      tax: 0,
      total: order.totalAmount
    },
    payment: {
      method: order.payment?.method || 'N/A',
      status: order.payment?.status || 'Pending',
      transactionId: order.payment?.transactionCode || ''
    },
    currentStatus: order.status
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-in fade-in duration-300">
      <Card className="w-full max-w-4xl max-h-[90vh] overflow-hidden bg-surface border-border shadow-2xl flex flex-col animate-in zoom-in-95 duration-300">
        <CardHeader className="border-b border-border bg-muted/20 flex flex-row items-center justify-between p-6">
          <div className="flex items-center gap-4">
            <div className="bg-primary/10 p-2 rounded-xl">
              <ShoppingBag className="h-6 w-6 text-primary" />
            </div>
            <div>
              <CardTitle className="text-xl font-bold text-foreground">Order #{orderId}</CardTitle>
              <div className="flex items-center gap-2 mt-1">
                <Calendar className="h-3 w-3 text-subtle" />
                <span className="text-xs text-muted-foreground uppercase tracking-widest font-bold">
                  {new Date(order.createdAt).toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' })}
                </span>
              </div>
            </div>
          </div>
          <Button variant="ghost" size="icon" onClick={onClose} className="rounded-full">
            <X className="h-6 w-6" />
          </Button>
        </CardHeader>

        <CardContent className="flex-1 overflow-y-auto p-0 custom-scrollbar">
          <div className="grid grid-cols-1 lg:grid-cols-3">
            {/* Left Column: Line Items & Summary */}
            <div className="lg:col-span-2 p-8 border-r border-border space-y-8">
              <OrderItemsList items={mappedData.items} />
              <OrderSummary summary={mappedData.summary} />
            </div>

            {/* Right Column: Customer & Status */}
            <div className="bg-muted/20 p-8 space-y-10">
              <OrderFulfillment
                register={register}
                control={control}
                handleSubmit={handleSubmit}
                isSubmitting={isSubmitting}
              />
              <CustomerDetails
                customer={mappedData.customer}
                payment={mappedData.payment}
              />
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

