import { Link } from 'react-router-dom';
import { CheckCircle2, ShoppingBag, ArrowRight, Package, MapPin, CreditCard, Clock } from 'lucide-react';
import { buttonVariants } from '@/shared/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { cn } from '@/shared/lib/utils';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { PaymentStatus } from '@/features/orders/types';
import { useCheckoutSuccess } from '@/features/checkout/hooks/useCheckoutSuccess';

export function CheckoutSuccess() {
  const { order, isLoading } = useCheckoutSuccess();

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-20 flex items-center justify-center min-h-[70vh]">
        <Card className="w-full max-w-2xl border-none shadow-2xl bg-card/50 backdrop-blur-sm">
          <CardHeader className="space-y-4">
            <Skeleton className="h-16 w-16 rounded-full mx-auto" />
            <Skeleton className="h-8 w-48 mx-auto" />
          </CardHeader>
          <CardContent className="space-y-4">
            <Skeleton className="h-24 w-full" />
            <Skeleton className="h-32 w-full" />
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-20 flex items-center justify-center min-h-[70vh]">
      <Card className="w-full max-w-2xl border-none shadow-2xl bg-card/50 backdrop-blur-sm overflow-hidden">
        <div className="absolute top-0 left-0 w-full h-1 bg-green-500" />
        <CardHeader className="text-center pb-2 pt-10">
          <div className="flex justify-center mb-6">
            <div className="p-3 rounded-full bg-green-500/10 animate-pulse">
              <CheckCircle2 className="w-16 h-16 text-green-500" />
            </div>
          </div>
          <CardTitle className="text-3xl font-bold text-foreground">Order Confirmed!</CardTitle>
          <p className="text-muted-foreground mt-2">Thanks for your purchase. We've received your order.</p>
        </CardHeader>
        <CardContent className="space-y-8 px-8">
          <div className="space-y-4">
            <div className="flex items-center gap-2 text-sm font-semibold uppercase tracking-wider text-muted-foreground">
              <Package className="w-4 h-4" /> Order Summary
            </div>
            <div className="rounded-xl border border-border bg-muted/30 divide-y divide-border">
              {order?.items.map((item) => (
                <div key={item.id} className="p-4 flex justify-between items-center">
                  <div className="flex items-center gap-3">
                    {item.productCoverUrl && (
                      <img src={item.productCoverUrl} alt={item.productName} className="w-12 h-12 rounded object-cover shadow-sm" />
                    )}
                    <div>
                      <p className="font-medium text-sm">{item.productName}</p>
                      <p className="text-xs text-muted-foreground">Qty: {item.quantity}</p>
                    </div>
                  </div>
                  <p className="font-semibold text-sm">${item.subtotal.toFixed(2)}</p>
                </div>
              ))}
              <div className="p-4 flex justify-between items-center bg-primary/5">
                <p className="font-bold">Total Amount</p>
                <p className="font-bold text-lg text-primary">${order?.totalAmount.toFixed(2)}</p>
              </div>
            </div>
          </div>
          <div className="grid md:grid-cols-2 gap-6">
            <div className="space-y-3">
              <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-muted-foreground">
                <MapPin className="w-3.5 h-3.5" /> Shipping To
              </div>
              <div className="text-sm space-y-1">
                <p className="font-semibold">{order?.recipientName}</p>
                <p className="text-muted-foreground leading-relaxed">{order?.shippingAddress}</p>
                <p className="text-muted-foreground">{order?.phone}</p>
              </div>
            </div>
            <div className="space-y-3">
              <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-muted-foreground">
                <CreditCard className="w-3.5 h-3.5" /> Payment Status
              </div>
              <div className="space-y-2">
                <div className={cn(
                  "inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold",
                  order?.payment?.status === PaymentStatus.Paid
                    ? "bg-green-500/10 text-green-500"
                    : "bg-amber-500/10 text-amber-500"
                )}>
                  {order?.payment?.status === PaymentStatus.Paid ? (
                    <><CheckCircle2 className="w-3.5 h-3.5" /> Paid Successfully</>
                  ) : (
                    <><Clock className="w-3.5 h-3.5" /> Processing Payment...</>
                  )}
                </div>
                {order?.payment?.transactionCode && (
                  <p className="text-[10px] font-mono text-muted-foreground break-all">
                    Ref: {order.payment.transactionCode}
                  </p>
                )}
              </div>
            </div>
          </div>
        </CardContent>
        <CardFooter className="flex flex-col sm:flex-row gap-3 p-8 bg-muted/20 border-t border-border">
          <Link to="/orders" className={cn(buttonVariants({ variant: 'default' }), "flex-1 h-12 text-md font-semibold group")}>
            Track Order <ArrowRight className="ml-2 w-5 h-5 transition-transform group-hover:translate-x-1" />
          </Link>
          <Link to="/" className={cn(buttonVariants({ variant: 'outline' }), "flex-1 h-12 text-md font-medium")}>
            <ShoppingBag className="mr-2 w-5 h-5" /> Shop More
          </Link>
        </CardFooter>
      </Card>
    </div>
  );
}
