import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { curationSchema, CurationFormValues, CurationItem } from '../types/curation';

import { curationService } from '../services/curationService';
import { curationKeys } from '../queryKeys';
import { toast } from 'sonner';

interface UseCurationFormProps {
  collectionId: string | null;
  onSuccess: () => void;
}

import { UseFormRegister, Control, FieldErrors, UseFieldArrayRemove } from 'react-hook-form';

interface UseCurationFormReturn {
  register: UseFormRegister<CurationFormValues>;
  control: Control<CurationFormValues>;
  handleSubmit: (e?: React.BaseSyntheticEvent) => Promise<void>;
  errors: FieldErrors<CurationFormValues>;
  items: any[];
  appendItem: (item: any) => void;
  removeItem: UseFieldArrayRemove;
  moveItem: (from: number, to: number) => void;
  isSubmitting: boolean;
}

import { useEffect, useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';

export function useCurationForm({ collectionId, onSuccess }: UseCurationFormProps): UseCurationFormReturn {
  const queryClient = useQueryClient();
  const [isLoading, setIsLoading] = useState(!!collectionId);

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting }
  } = useForm<CurationFormValues>({
    resolver: zodResolver(curationSchema) as any,
    defaultValues: {
      title: '',
      description: '',
      isPublished: true,
      items: [],
    },
  });

  const [initialItems, setInitialItems] = useState<CurationItem[]>([]);

  useEffect(() => {
    if (collectionId) {
      setIsLoading(true);
      curationService.getCollectionById(collectionId)
        .then(data => {
          setInitialItems(data.items);
          reset({
            title: data.title,
            description: data.description,
            isPublished: data.isPublished,
            items: data.items,
          });
        })
        .finally(() => setIsLoading(false));
    } else {
      setInitialItems([]);
      reset({
        title: '',
        description: '',
        isPublished: true,
        items: [],
      });
    }
  }, [collectionId, reset]);

  const { fields: items, remove: removeItem, append: appendItem, move: moveItem } = useFieldArray({
    control,
    name: 'items',
  });

  const onSubmit = async (value: CurationFormValues) => {
    try {
      let id = collectionId;
      if (id) {
        await curationService.updateCollection(id, value);
      } else {
        id = await curationService.createCollection(value);
      }

      // Synchronize items
      const currentItemProductIds = value.items.map(i => i.productId);
      const initialItemProductIds = initialItems.map(i => i.productId);

      // Items to add
      const toAdd = value.items.filter(i => !initialItemProductIds.includes(i.productId));
      // Items to remove
      const toRemove = initialItems.filter(i => !currentItemProductIds.includes(i.productId));

      // Sequential updates to avoid race conditions on sort order if possible
      for (const item of toRemove) {
        await curationService.removeItem(id, item.productId);
      }

      for (let i = 0; i < value.items.length; i++) {
        const item = value.items[i];
        const isNew = !initialItemProductIds.includes(item.productId);
        
        if (isNew) {
          await curationService.addItem(id, item.productId, i + 1);
        } else {
          // Potentially update sort order if changed
          // For now we just add new ones. 
          // In a real app we might need a Bulk Update Sort Order API
        }
      }

      queryClient.invalidateQueries({ queryKey: curationKeys.all });
      toast.success(collectionId ? 'Collection updated successfully' : 'Collection created successfully');
      onSuccess();
    } catch (error: any) {
      console.error('Failed to save curation:', error);
      toast.error(error.message || 'Failed to save collection. Please try again.');
    }
  };

  return {
    register,
    control,
    handleSubmit: handleSubmit(onSubmit) as any,
    errors,
    items,
    appendItem,
    removeItem,
    moveItem,
    isSubmitting: isSubmitting || isLoading,
  };
}
