import { useForm, UseFormRegister, Control, FieldErrors } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateArtist, useUpdateArtist } from '@/features/catalog/hooks/useArtists';
import { Artist } from '@/features/catalog/types';
import { cn, slugify } from '@/shared/lib/utils';
import { uploadService } from '@/shared/services/uploadService';
import { toast } from 'sonner';

import { artistSchema, ArtistFormValues } from '../types/artist';

interface UseArtistFormProps {
  editingArtist: Artist | null;
  onSuccess: () => void;
}

interface UseArtistFormReturn {
  register: UseFormRegister<ArtistFormValues>;
  control: Control<ArtistFormValues>;
  handleSubmit: (e?: React.BaseSyntheticEvent) => Promise<void>;
  errors: FieldErrors<ArtistFormValues>;
  handleNameChange: (name: string) => void;
  toggleGenre: (genreId: string) => void;
  isPending: boolean;
}


export function useArtistForm({ editingArtist, onSuccess }: UseArtistFormProps): UseArtistFormReturn {

  const createMutation = useCreateArtist();
  const updateMutation = useUpdateArtist();

  const {
    register,
    control,
    handleSubmit,
    setValue,
    getValues,
    formState: { errors, isSubmitting }
  } = useForm<ArtistFormValues>({
    resolver: zodResolver(artistSchema) as any,
    defaultValues: {
      name: editingArtist?.name ?? '',
      slug: editingArtist?.slug ?? '',
      country: editingArtist?.country ?? '',
      bio: editingArtist?.bio ?? '',
      imageUrl: editingArtist?.imageUrl ?? '',
      genreIds: editingArtist?.genres.map((genre: any) => genre.id) ?? [],
    }
  });

  const onSubmit = async (value: ArtistFormValues) => {

    try {
      let finalImageUrl = value.imageUrl;

      if (value.imageUrl instanceof File) {
        finalImageUrl = await uploadService.uploadImage(value.imageUrl, 'artists');
      }

      const payload = {
        ...value,
        imageUrl: finalImageUrl as string,
      };


      if (editingArtist) {
        await updateMutation.mutateAsync({ id: editingArtist.id, data: payload });
      } else {
        await createMutation.mutateAsync(payload);
      }

      onSuccess();
    } catch (error: any) {
      toast.error(error.message)
    }
  };

  const handleNameChange = (name: string) => {
    setValue('name', name, { shouldValidate: true });
    setValue('slug', slugify(name), { shouldValidate: true });
  };

  const toggleGenre = (genreId: string) => {
    const currentGenres = getValues('genreIds') || [];
    const nextGenres = currentGenres.includes(genreId)
      ? currentGenres.filter((id: string) => id !== genreId)
      : [...currentGenres, genreId];

    setValue('genreIds', nextGenres, { shouldValidate: true });
  };


  return {
    register,
    control,
    handleSubmit: handleSubmit(onSubmit) as any,
    errors,
    handleNameChange,
    toggleGenre,
    isPending: createMutation.isPending || updateMutation.isPending || isSubmitting,
  };
}