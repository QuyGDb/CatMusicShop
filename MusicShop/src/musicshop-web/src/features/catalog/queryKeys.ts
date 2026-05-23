export const catalogKeys = {
  genres: {
    all: ['genres'] as const,
    list: (params: { page: number; limit: number; search?: string }) =>
      ['genres', params] as const,
    detail: (slug: string) => ['genres', slug] as const,
  },
  artists: {
    all: ['artists'] as const,
    list: (params: { page: number; limit: number; search?: string }) =>
      ['artists', params] as const,
    detail: (slug: string) => ['artists', slug] as const,
  },
  labels: {
    all: ['labels'] as const,
    list: (params: { page: number; limit: number; search?: string }) =>
      ['labels', params] as const,
    detail: (slug: string) => ['labels', slug] as const,
  },
  releases: {
    all: ['releases'] as const,
    list: (params: { page: number; limit: number; search?: string }) =>
      ['releases', params] as const,
    detail: (slug: string) => ['releases', slug] as const,
    formats: () => ['releases', 'formats'] as const,
    tracks: (releaseId: string) => ['releases', releaseId, 'tracks'] as const,
    versions: (releaseId: string) => ['releases', releaseId, 'versions'] as const,
  },
};
