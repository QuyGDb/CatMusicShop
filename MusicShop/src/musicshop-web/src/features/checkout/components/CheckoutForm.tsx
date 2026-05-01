import React from 'react';
import { useCheckout } from '../hooks/useCheckout';
import { useCheckoutForm } from '../hooks/useCheckoutForm';
import { Button, Input } from '@/shared/components';
import { CreditCard, Truck, User, Phone, MapPin } from 'lucide-react';
import { useCart } from '@/features/cart/hooks/useCart';

export function CheckoutForm() {
  const { cart, isLoading: isCartLoading } = useCart();
  const { handleCheckout, isCheckingOut } = useCheckout();
  const { form, handleSubmit, isSubmitting } = useCheckoutForm(handleCheckout);
  const { register, formState: { errors } } = form;

  if (isCartLoading) {
    return <div className="p-8 text-center">Loading cart details...</div>;
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="p-8 text-center space-y-4">
        <h2 className="text-2xl font-black text-foreground">Your cart is empty</h2>
        <p className="text-muted-foreground">Please add some items to your cart before proceeding to checkout.</p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-12 gap-8">
      {/* Form Details */}
      <div className="lg:col-span-7 space-y-8">
        <div className="bg-surface rounded-2xl border border-border p-6 md:p-8 shadow-sm">
          <h2 className="text-2xl font-black text-foreground mb-6 flex items-center">
            <Truck className="h-6 w-6 mr-3 text-primary" />
            Shipping Information
          </h2>
          
          <div className="space-y-6">
            <div>
              <label className="block text-sm font-medium text-foreground mb-2 flex items-center">
                <User className="h-4 w-4 mr-2 text-muted-foreground" />
                Recipient Name
              </label>
              <Input
                {...register('recipientName')}
                placeholder="Full Name"
                className="h-12 bg-background border-border"
                disabled={isSubmitting || isCheckingOut}
              />
              {errors.recipientName && (
                <p className="text-red-500 text-xs mt-1">{errors.recipientName.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-2 flex items-center">
                <MapPin className="h-4 w-4 mr-2 text-muted-foreground" />
                Email Address
              </label>
              <Input
                {...register('email')}
                placeholder="email@example.com"
                className="h-12 bg-background border-border"
                disabled={isSubmitting || isCheckingOut}
              />
              {errors.email && (
                <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-2 flex items-center">
                <Phone className="h-4 w-4 mr-2 text-muted-foreground" />
                Phone Number
              </label>
              <Input
                {...register('phone')}
                placeholder="+1 (555) 000-0000"
                className="h-12 bg-background border-border"
                disabled={isSubmitting || isCheckingOut}
              />
              {errors.phone && (
                <p className="text-red-500 text-xs mt-1">{errors.phone.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-2 flex items-center">
                <MapPin className="h-4 w-4 mr-2 text-muted-foreground" />
                Shipping Address
              </label>
              <Input
                {...register('shippingAddress')}
                placeholder="123 Main St, City, Country, Zip"
                className="h-12 bg-background border-border"
                disabled={isSubmitting || isCheckingOut}
              />
              {errors.shippingAddress && (
                <p className="text-red-500 text-xs mt-1">{errors.shippingAddress.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-2">
                Order Notes (Optional)
              </label>
              <textarea
                {...register('note')}
                placeholder="Special instructions for delivery"
                className="w-full rounded-xl border border-border bg-background px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 min-h-[100px]"
                disabled={isSubmitting || isCheckingOut}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Order Summary */}
      <div className="lg:col-span-5">
        <div className="bg-surface rounded-2xl border border-border p-6 md:p-8 shadow-sm sticky top-24">
          <h2 className="text-2xl font-black text-foreground mb-6">Order Summary</h2>
          
          <div className="space-y-4 mb-6 max-h-[300px] overflow-y-auto custom-scrollbar pr-2">
            {cart.items.map((item) => (
              <div key={item.id} className="flex gap-4">
                <div className="h-16 w-16 rounded-md bg-muted overflow-hidden flex-shrink-0">
                  <img src={item.coverUrl || '/assets/placeholder-record.jpg'} alt={item.productName} className="h-full w-full object-cover" />
                </div>
                <div className="flex-1 flex flex-col justify-center">
                  <span className="font-bold text-sm line-clamp-1">{item.productName}</span>
                  <span className="text-xs text-muted-foreground">Qty: {item.quantity}</span>
                </div>
                <div className="flex items-center">
                  <span className="font-bold">${(item.unitPrice * item.quantity).toFixed(2)}</span>
                </div>
              </div>
            ))}
          </div>
          
          <div className="space-y-3 pt-6 border-t border-border mb-8">
            <div className="flex justify-between text-sm text-muted-foreground">
              <span>Subtotal</span>
              <span className="font-medium text-foreground">${cart.totalAmount.toFixed(2)}</span>
            </div>
            <div className="flex justify-between text-sm text-muted-foreground">
              <span>Shipping</span>
              <span>Calculated at next step</span>
            </div>
            <div className="h-px bg-border my-2" />
            <div className="flex justify-between items-end">
              <span className="font-bold text-foreground">Total</span>
              <span className="text-3xl font-black text-primary">${cart.totalAmount.toFixed(2)}</span>
            </div>
          </div>
          
          <Button 
            type="submit" 
            className="w-full h-14 text-lg rounded-2xl bg-primary hover:bg-primary-dark text-primary-foreground shadow-xl shadow-primary/20"
            disabled={isSubmitting || isCheckingOut}
          >
            {isCheckingOut || isSubmitting ? (
              <div className="h-5 w-5 border-2 border-primary-foreground border-t-transparent rounded-full animate-spin mr-2" />
            ) : (
              <CreditCard className="h-5 w-5 mr-2" />
            )}
            {isCheckingOut || isSubmitting ? 'Processing...' : 'Proceed to Payment'}
          </Button>
          <p className="text-center text-xs text-muted-foreground mt-4">
            You will be redirected to Stripe for secure payment.
          </p>
        </div>
      </div>
    </form>
  );
}
