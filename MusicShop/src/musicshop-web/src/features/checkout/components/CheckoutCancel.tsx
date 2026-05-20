import { Link } from 'react-router-dom';
import { XCircle, ShoppingCart, AlertCircle } from 'lucide-react';
import { buttonVariants } from '@/shared/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { cn } from '@/shared/lib/utils';

export function CheckoutCancel() {
  return (
    <div className="container mx-auto px-4 py-20 flex items-center justify-center min-h-[70vh]">
      <Card className="w-full max-w-md border-none shadow-2xl bg-card/50 backdrop-blur-sm">
        <CardHeader className="text-center pb-2">
          <div className="flex justify-center mb-6">
            <div className="p-3 rounded-full bg-destructive/10">
              <XCircle className="w-16 h-16 text-destructive" />
            </div>
          </div>
          <CardTitle className="text-3xl font-bold text-foreground">Payment Cancelled</CardTitle>
        </CardHeader>
        <CardContent className="text-center space-y-4">
          <div className="flex items-center justify-center gap-2 text-amber-500 bg-amber-500/10 p-3 rounded-lg border border-amber-500/20">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <p className="text-sm font-medium">No charges were made to your account.</p>
          </div>
          <p className="text-muted-foreground text-lg">Your transaction was cancelled. If you encountered any issues, please try again or contact our support.</p>
        </CardContent>
        <CardFooter className="flex flex-col gap-3 pt-6">
          <Link to="/checkout" className={cn(buttonVariants({ variant: 'default' }), "w-full h-12 text-lg font-semibold")}>
            <ShoppingCart className="mr-2 w-5 h-5" /> Return to Checkout
          </Link>
          <Link to="/" className={cn(buttonVariants({ variant: 'ghost' }), "w-full h-12 text-lg")}>
            Go to Homepage
          </Link>
        </CardFooter>
      </Card>
    </div>
  );
}
