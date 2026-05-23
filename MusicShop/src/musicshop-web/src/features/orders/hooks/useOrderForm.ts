import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { OrderStatus } from '../types';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { orderService } from '../services/orderService';
import { toast } from 'sonner';
import { orderKeys } from '../queryKeys';

const orderStatusSchema = z.object({
  status: z.enum(['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled']),
  trackingNumber: z.string().optional(),
});

type OrderFormValues = z.infer<typeof orderStatusSchema>;

interface UseOrderFormProps {
  orderId: string;
  initialStatus: OrderStatus;
  onSuccess: () => void;
}

import { UseFormRegister, Control, FieldErrors } from 'react-hook-form';

interface UseOrderFormReturn {
  register: UseFormRegister<OrderFormValues>;
  handleSubmit: (e?: React.BaseSyntheticEvent) => Promise<void>;
  errors: FieldErrors<OrderFormValues>;
  control: Control<OrderFormValues>;
  isSubmitting: boolean;
}

export function useOrderForm({ orderId, initialStatus, onSuccess }: UseOrderFormProps): UseOrderFormReturn {
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    control,
    formState: { errors }
  } = useForm<OrderFormValues>({
    resolver: zodResolver(orderStatusSchema) as any,
    defaultValues: {
      status: initialStatus as any,
      trackingNumber: '',
    },
  });

  const mutation = useMutation({
    mutationFn: (values: OrderFormValues) => 
      orderService.updateOrderStatus(orderId, values.status, values.trackingNumber),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: orderKeys.admin.all });
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(orderId) });
      toast.success('Order status updated successfully');
      onSuccess();
    },
    onError: (err: any) => {
      toast.error(err.message || 'Failed to update order status');
    }
  });

  const onSubmit = (values: OrderFormValues) => {
    mutation.mutate(values);
  };

  return {
    register,
    handleSubmit: handleSubmit(onSubmit) as any,
    errors,
    control,
    isSubmitting: mutation.isPending,
  };
}

