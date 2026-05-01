import { Button } from '@/shared/components';
import { Loader2 } from 'lucide-react';
import { useWatch } from 'react-hook-form';

interface OrderFulfillmentProps {
  register: any;
  control: any;
  handleSubmit: () => void;
  isSubmitting: boolean;
}

export function OrderFulfillment({ register, control, handleSubmit, isSubmitting }: any) {

  const status = useWatch({
    control,
    name: 'status',
  });

  return (
    <div className="space-y-4">
      <label className="text-xs font-black uppercase tracking-widest text-subtle block mb-4">Manage Fulfillment</label>
      <div className="space-y-3">
        <select 
          {...register('status')}
          className="w-full h-12 bg-surface border border-border rounded-xl px-4 focus:outline-none focus:border-primary transition-all font-bold text-sm"
        >
          <option value="Pending">Pending Approval</option>
          <option value="Confirmed">Confirmed Order</option>
          <option value="Shipped">Dispatched / Shipped</option>
          <option value="Delivered">Mark as Delivered</option>
          <option value="Cancelled">Void / Cancelled</option>
        </select>

        {status === 'Shipped' && (
          <input 
            type="text" 
            {...register('trackingNumber')}
            placeholder="Tracking Number (e.g. UPS-123...)"
            className="w-full h-12 bg-surface border border-border rounded-xl px-4 focus:outline-none focus:border-primary transition-all text-sm animate-in slide-in-from-top-2"
          />
        )}

        <Button 
          onClick={handleSubmit} 
          className="w-full h-12 bg-primary text-white font-bold rounded-xl"
          disabled={isSubmitting}
        >
          {isSubmitting ? <Loader2 className="h-5 w-5 animate-spin" /> : "Update Order Status"}
        </Button>
      </div>
    </div>
  );
}

