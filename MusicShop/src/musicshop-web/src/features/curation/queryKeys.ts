export const curationKeys = {
  all: ['curated-collections'] as const,
  list: (params: { includeUnpublished: boolean; page: number; limit: number; search: string }) =>
    [...curationKeys.all, params] as const,
  featured: (count: number) => ['featured-collections', count] as const,
};
