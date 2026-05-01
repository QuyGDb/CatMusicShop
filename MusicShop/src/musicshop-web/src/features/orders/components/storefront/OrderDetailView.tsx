import React from 'react';
import { useOrderDetail } from '../../hooks/useOrderDetail';
import { OrderStatus } from '../../types';
import { Card, CardContent, CardHeader, CardTitle, Badge, Button } from '@/shared/components';
import { ArrowLeft, Package, Clock, Truck, CheckCircle2, X, AlertCircle, CreditCard, MapPin, Receipt, XCircle } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { Link } from 'react-router-dom';

const statusStyles: Record<OrderStatus, { color: string, icon: any, description: string }> = {
  [OrderStatus.Pending]: { color: 'bg-amber-100 text-amber-700', icon: Clock, description: 'We are verifying your order details and payment.' },
  [OrderStatus.Confirmed]: { color: 'bg-blue-100 text-blue-700', icon: AlertCircle, description: 'Your order is confirmed and being prepared for shipment.' },
  [OrderStatus.Shipped]: { color: 'bg-purple-100 text-purple-700', icon: Truck, description: 'Your order is on the way!' },
  [OrderStatus.Delivered]: { color: 'bg-emerald-100 text-emerald-700', icon: CheckCircle2, description: 'Your order has been delivered. Enjoy the music!' },
  [OrderStatus.Cancelled]: { color: 'bg-muted text-subtle', icon: X, description: 'This order was cancelled.' }
};

interface OrderDetailViewProps {
  orderId: string;
}

export function OrderDetailView({ orderId }: OrderDetailViewProps) {
  const { order, isLoading, error, handleBack, handleCancel, isCancelling, isAdmin } = useOrderDetail(orderId);

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center py-24">
        <Receipt className="h-12 w-12 text-primary animate-pulse mb-4" />
        <p className="text-muted-foreground">Loading order details...</p>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="bg-red-50 border border-red-200 p-8 rounded-3xl text-center max-w-lg mx-auto mt-12">
        <AlertCircle className="h-12 w-12 text-red-500 mx-auto mb-4" />
        <h2 className="text-xl font-bold mb-2">Order Not Found</h2>
        <p className="text-muted-foreground mb-6">{error || "We couldn't find the details for this order."}</p>
        <Button onClick={handleBack} className="rounded-xl">Go Back</Button>
      </div>
    );
  }

  const style = statusStyles[order.status];
  const StatusIcon = style.icon;

  return (
    <div className="max-w-4xl mx-auto space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500 pb-12">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <button
            onClick={handleBack}
            className="flex items-center gap-2 text-sm text-muted-foreground hover:text-primary transition-colors mb-4 group"
          >
            <ArrowLeft className="h-4 w-4 group-hover:-translate-x-1 transition-transform" />
            Back to Orders
          </button>
          <div className="flex items-center gap-4">
            <h1 className="text-3xl font-black text-foreground">
              Order <span className="font-mono text-primary">#{order.id.split('-')[0]}</span>
            </h1>
            <div className={cn("inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-wider", style.color)}>
              <StatusIcon className="h-3 w-3" />
              {order.status}
            </div>
          </div>
          <p className="text-muted-foreground mt-2 flex items-center gap-2">
            <Clock className="h-4 w-4" />
            Placed on {new Date(order.createdAt).toLocaleDateString()} at {new Date(order.createdAt).toLocaleTimeString()}
          </p>
        </div>

        {((order.status === OrderStatus.Pending) || (isAdmin && order.status !== OrderStatus.Cancelled)) && (
          <Button
            variant="destructive"
            className="rounded-xl shrink-0"
            onClick={handleCancel}
            disabled={isCancelling}
          >
            {isCancelling ? <Clock className="h-4 w-4 mr-2 animate-spin" /> : <XCircle className="h-4 w-4 mr-2" />}
            {isAdmin ? "Void Order (Admin)" : "Cancel Order"}
          </Button>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Main Content - Items */}
        <div className="md:col-span-2 space-y-6">
          <Card className="bg-surface border-border overflow-hidden">
            <CardHeader className="bg-muted/30 border-b border-border">
              <CardTitle className="text-lg flex items-center gap-2">
                <Package className="h-5 w-5 text-primary" />
                Items Ordered
              </CardTitle>
            </CardHeader>
            <CardContent className="p-0">
              <div className="divide-y divide-border/50">
                {order.items.map((item) => (
                  <div key={item.id} className="flex gap-4 p-6 hover:bg-muted/10 transition-colors">
                    <div className="h-20 w-20 rounded-xl overflow-hidden bg-muted shrink-0 border border-border shadow-sm">
                      <img
                        src={item.productCoverUrl || '/images/placeholder-album.png'}
                        alt={item.productName}
                        className="h-full w-full object-cover"
                      />
                    </div>
                    <div className="flex-1 flex flex-col justify-center">
                      <h4 className="font-bold text-foreground line-clamp-1 hover:text-primary transition-colors">
                        <Link to={`/products/${item.productId}`}>
                          {item.productName}
                        </Link>
                      </h4>
                      <div className="text-sm text-muted-foreground mt-1">
                        Qty: {item.quantity} × ${item.unitPrice.toFixed(2)}
                      </div>
                    </div>
                    <div className="text-right flex flex-col justify-center">
                      <span className="font-black text-foreground">${item.subtotal.toFixed(2)}</span>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar - Summary & Details */}
        <div className="space-y-6">
          <Card className="bg-surface border-border">
            <CardHeader className="bg-muted/30 border-b border-border">
              <CardTitle className="text-lg flex items-center gap-2">
                <Receipt className="h-5 w-5 text-primary" />
                Order Summary
              </CardTitle>
            </CardHeader>
            <CardContent className="p-6 space-y-4">
              <div className="space-y-2 text-sm">
                <div className="flex justify-between text-muted-foreground">
                  <span>Subtotal</span>
                  <span className="text-foreground font-medium">${order.totalAmount.toFixed(2)}</span>
                </div>
                <div className="flex justify-between text-muted-foreground">
                  <span>Shipping</span>
                  <span className="text-foreground font-medium">$0.00</span>
                </div>
                <div className="flex justify-between text-muted-foreground">
                  <span>Tax</span>
                  <span className="text-foreground font-medium">$0.00</span>
                </div>
              </div>
              <hr className="border-border" />
              <div className="flex justify-between items-end">
                <span className="font-bold text-foreground">Total</span>
                <span className="text-2xl font-black text-primary">${order.totalAmount.toFixed(2)}</span>
              </div>
            </CardContent>
          </Card>

          <Card className="bg-surface border-border">
            <CardHeader className="bg-muted/30 border-b border-border">
              <CardTitle className="text-lg flex items-center gap-2">
                <MapPin className="h-5 w-5 text-primary" />
                Shipping Info
              </CardTitle>
            </CardHeader>
            <CardContent className="p-6 space-y-4 text-sm">
              <div>
                <p className="font-bold text-foreground">{order.recipientName}</p>
                <p className="text-muted-foreground mt-1">{order.phone}</p>
              </div>
              <div>
                <p className="text-subtle font-medium uppercase tracking-wider text-xs mb-1">Address</p>
                <p className="text-muted-foreground leading-relaxed">{order.shippingAddress}</p>
              </div>
              {order.note && (
                <div>
                  <p className="text-subtle font-medium uppercase tracking-wider text-xs mb-1">Note</p>
                  <p className="text-muted-foreground italic bg-muted/50 p-3 rounded-lg">{order.note}</p>
                </div>
              )}
              {order.trackingNumber && (
                <div className="bg-primary/5 p-4 rounded-xl border border-primary/20">
                  <p className="text-primary font-bold uppercase tracking-wider text-xs mb-1 flex items-center gap-2">
                    <Truck className="h-3 w-3" />
                    Tracking Number
                  </p>
                  <p className="font-mono text-foreground font-bold">{order.trackingNumber}</p>
                </div>
              )}
            </CardContent>
          </Card>

          {order.payment && (
            <Card className="bg-surface border-border">
              <CardHeader className="bg-muted/30 border-b border-border">
                <CardTitle className="text-lg flex items-center gap-2">
                  <CreditCard className="h-5 w-5 text-primary" />
                  Payment Info
                </CardTitle>
              </CardHeader>
              <CardContent className="p-6 space-y-4 text-sm">
                <div className="flex justify-between items-center">
                  <span className="text-muted-foreground">Method</span>
                  <Badge variant="outline" className="font-bold">{order.payment.method}</Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-muted-foreground">Status</span>
                  <Badge
                    className={cn(
                      "font-bold border-none",
                      order.payment.status === 'Paid' ? "bg-emerald-100 text-emerald-700" :
                        order.payment.status === 'Pending' ? "bg-amber-100 text-amber-700" :
                          "bg-red-100 text-red-700"
                    )}
                  >
                    {order.payment.status}
                  </Badge>
                </div>
                {order.payment.transactionCode && (
                  <div className="pt-2">
                    <p className="text-subtle font-medium uppercase tracking-wider text-xs mb-1">Transaction</p>
                    <p className="font-mono text-xs text-muted-foreground break-all">{order.payment.transactionCode}</p>
                  </div>
                )}
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
