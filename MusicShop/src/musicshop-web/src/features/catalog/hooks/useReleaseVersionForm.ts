import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateReleaseVersion, useUpdateReleaseVersion } from '@/features/catalog/hooks/useReleases';
import { releaseVersionSchema, ReleaseVersionFormValues } from '../types/release';
import { ReleaseVersion } from '../types';
import { ReleaseFormat } from '@/features/products/types';
import { useEffect } from 'react';

interface UseReleaseVersionFormProps {
  releaseId: string;
  editingVersion?: ReleaseVersion | null;
  onSuccess: () => void;
}

export function useReleaseVersionForm({ releaseId, editingVersion, onSuccess }: UseReleaseVersionFormProps) {
  const createMutation = useCreateReleaseVersion();
  const updateMutation = useUpdateReleaseVersion();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting }
  } = useForm<ReleaseVersionFormValues>({
    resolver: zodResolver(releaseVersionSchema),
    defaultValues: {
      labelId: '',
      format: ReleaseFormat.Vinyl,
      catalogNumber: '',
      pressingCountry: '',
      pressingYear: new Date().getFullYear(),
      notes: ''
    }
  });

  useEffect(() => {
    if (editingVersion) {
      reset({
        labelId: editingVersion.labelId,
        format: editingVersion.format,
        catalogNumber: editingVersion.catalogNumber,
        pressingCountry: editingVersion.pressingCountry,
        pressingYear: editingVersion.pressingYear,
        notes: editingVersion.notes || ''
      });
    } else {
      reset({
        labelId: '',
        format: ReleaseFormat.Vinyl,
        catalogNumber: '',
        pressingCountry: '',
        pressingYear: new Date().getFullYear(),
        notes: ''
      });
    }
  }, [editingVersion, reset]);

  const onSubmit = async (values: ReleaseVersionFormValues) => {
    try {
      if (editingVersion) {
        await updateMutation.mutateAsync({
          id: editingVersion.id,
          data: { ...values, releaseId, id: editingVersion.id }
        });
      } else {
        await createMutation.mutateAsync({
          ...values,
          releaseId
        });
      }
      onSuccess();
      reset();
    } catch (error) {
      console.error('Failed to save edition:', error);
    }
  };

  return {
    register,
    handleSubmit: handleSubmit(onSubmit),
    errors,
    isPending: createMutation.isPending || updateMutation.isPending || isSubmitting,
    reset
  };
}
