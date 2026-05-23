import { useState, useEffect } from 'react';
import { useForm, useFieldArray, UseFormRegister, Control, FieldErrors, FieldArrayWithId, Resolver } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateRelease, useUpdateRelease } from '@/features/catalog/hooks/useReleases';
import { useGenres } from '@/features/catalog/hooks/useGenres';
import { Release } from '@/features/catalog/types';
import { releaseSchema, ReleaseFormValues, TrackValues } from '../types/release';
import { CreateReleaseRequest } from '../services/releaseService';
import { PaginatedResponse } from '@/shared/types/api';
import { Genre } from '@/features/catalog/types';
import { uploadService } from '@/shared/services/uploadService';
import { slugify } from '@/shared/lib/utils';

interface UseReleaseFormProps {
  editingRelease?: Release | null;
  onSuccess: () => void;
}
interface UseReleaseFormReturn {
  register: UseFormRegister<ReleaseFormValues>;
  control: Control<ReleaseFormValues>;
  handleSubmit: (e?: React.BaseSyntheticEvent) => Promise<void>;
  errors: FieldErrors<ReleaseFormValues>;
  step: number;
  setStep: (step: number) => void;
  nextStep: () => void;
  prevStep: () => void;
  handleAddTrack: () => void;
  handleRemoveTrack: (index: number) => void;
  handleTitleChange: (title: string) => void;
  toggleGenre: (genreId: string) => void;
  isPending: boolean;
  fields: FieldArrayWithId<ReleaseFormValues, 'tracks', 'id'>[];
  genresData: PaginatedResponse<Genre> | undefined;
  loadingGenres: boolean;
}
export function useReleaseForm({ editingRelease, onSuccess }: UseReleaseFormProps): UseReleaseFormReturn {
  const [step, setStep] = useState(1);

  const createReleaseMutation = useCreateRelease();
  const updateReleaseMutation = useUpdateRelease();
  const { data: genresData, isLoading: loadingGenres } = useGenres(1, 100);
  const {
    register,
    control,
    handleSubmit,
    setValue,
    getValues,
    reset,
    formState: { errors, isSubmitting }
  } = useForm<ReleaseFormValues>({
    resolver: zodResolver(releaseSchema) as Resolver<ReleaseFormValues>,

    defaultValues: {
      title: editingRelease?.title ?? '',
      slug: editingRelease?.slug ?? '',
      artistId: editingRelease?.artistId ?? '',
      year: editingRelease?.year ?? new Date().getFullYear(),
      description: editingRelease?.description ?? '',
      coverUrl: editingRelease?.coverUrl ?? '',
      genreIds: editingRelease?.genres.map(g => g.id) ?? [],
      tracks: editingRelease?.tracks?.map(t => ({
        position: t.position,
        title: t.title,
        durationSeconds: t.durationSeconds,
        side: t.side
      })) ?? []
    }
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'tracks'
  });

  // Sync initialData when it changes (e.g. when loading finishes)
  useEffect(() => {
    if (editingRelease) {
      const mappedTracks = editingRelease.tracks?.map(t => ({
        position: t.position,
        title: t.title,
        durationSeconds: t.durationSeconds,
        side: t.side
      })) || [];

      reset({
        title: editingRelease.title,
        slug: editingRelease.slug,
        artistId: editingRelease.artistId,
        year: editingRelease.year,
        description: editingRelease.description || '',
        coverUrl: editingRelease.coverUrl || '',
        genreIds: editingRelease.genres.map(g => g.id) || [],
        tracks: mappedTracks
      });
    }
  }, [editingRelease, reset]);

  const onSubmit = async (value: ReleaseFormValues) => {
    try {
      let finalCoverUrl = value.coverUrl;
      if (value.coverUrl instanceof File) {
        finalCoverUrl = await uploadService.uploadImage(value.coverUrl, 'releases');
      }

      const payload: CreateReleaseRequest = {
        title: value.title,
        slug: value.slug || slugify(value.title),
        year: value.year,
        artistId: value.artistId,
        coverUrl: finalCoverUrl as string,
        description: value.description,
        genreIds: value.genreIds,
        tracks: value.tracks.map(t => ({
          position: t.position,
          title: t.title,
          durationSeconds: t.durationSeconds,
          side: t.side
        }))
      };
      if (editingRelease) {
        await updateReleaseMutation.mutateAsync(
          { id: editingRelease.id, data: payload }
        );
      } else {
        await createReleaseMutation.mutateAsync(payload);
      }
      onSuccess();
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Submission failed';
      console.error(' [useReleaseForm] Submission error:', message);
    }
  };

  const nextStep = () => setStep((prev) => Math.min(prev + 1, 2));
  const prevStep = () => setStep((prev) => Math.max(prev - 1, 1));

  const handleAddTrack = () => {
    append({ position: fields.length + 1, title: '', durationSeconds: 0 });
  };

  const handleRemoveTrack = (index: number) => {
    remove(index);
    // Re-calculate positions for remaining tracks
    const currentTracks = getValues('tracks');
    currentTracks.forEach((track, i) => {
      setValue(`tracks.${i}.position`, i + 1);
    });
  };

  const handleTitleChange = (title: string) => {
    setValue('title', title, { shouldValidate: true });
    if (!editingRelease) {
      setValue('slug', slugify(title), { shouldValidate: true });
    }
  };

  const isPending = createReleaseMutation.isPending || updateReleaseMutation.isPending || isSubmitting;

  return {
    register,
    control,
    handleSubmit: handleSubmit(onSubmit),
    errors,
    step,
    setStep,
    nextStep,
    prevStep,
    handleAddTrack,
    handleRemoveTrack,
    handleTitleChange,
    toggleGenre: (genreId: string) => {
      const current = getValues('genreIds') || [];
      const next = current.includes(genreId)
        ? current.filter(id => id !== genreId)
        : [...current, genreId];
      setValue('genreIds', next, { shouldValidate: true });
    },
    isPending,
    fields,
    genresData,
    loadingGenres
  };
}

