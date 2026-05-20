# Frontend Architecture — Improvement Areas

These are concrete issues found in the codebase, ordered by impact.

---

## 1. Massive CRUD Hook Duplication (High Impact)

`useGenreManagement`, `useArtistManagement`, `useLabelManagement` are **structurally identical** — only the entity type and query key differ. Same goes for `useGenres`, `useArtists`, `useLabels` (the CRUD mutation hooks).

Compare side by side:

```diff
 // useGenreManagement.ts
 export function useGenreManagement() {
-  const [editingGenre, setEditingGenre] = useState<Genre | null>(null);
-  const { data: genresData } = useGenres(page, 20, debouncedSearch);
-  const deleteMutation = useDeleteGenre();

 // useArtistManagement.ts — exact same structure
+  const [editingArtist, setEditingArtist] = useState<Artist | null>(null);
+  const { data: artistsData } = useArtists(page, 12, debouncedSearch);
+  const deleteArtistMutation = useDeleteArtist();
```

**Problem**: Every new entity (e.g., adding "Format" or "Tag") requires copy-pasting ~90 LOC for the management hook + ~60 LOC for the CRUD hooks + ~50 LOC for the service.

**Fix**: Extract a generic `useCrudManagement<T>` hook and a `createCrudHooks<T>` factory.

```tsx
// shared/hooks/useCrudManagement.ts
interface CrudManagementConfig<T> {
  queryKey: string;
  useList: (page: number, limit: number, search?: string) => UseQueryResult;
  useDelete: () => UseMutationResult;
  pageSize: number;
  confirmMessage: string;
}

function useCrudManagement<T extends { id: string }>(config: CrudManagementConfig<T>) {
  // All the debounce, pagination, URL sync, form open/close logic — once
}
```

```tsx
// shared/hooks/createCrudHooks.ts
function createCrudHooks<TEntity, TCreate, TUpdate>(config: {
  queryKey: string;
  service: CrudService<TEntity, TCreate, TUpdate>;
  entityName: string;
}) {
  return {
    useList: (...) => useQuery(...),
    useCreate: () => useMutation(...),
    useUpdate: () => useMutation(...),
    useDelete: () => useMutation(...),
  };
}
```

This eliminates ~500+ LOC of duplication and makes adding new entities a 5-line config.

---

## 2. Pages Violate "Thin Shell" Rule Inconsistently (Medium Impact)

Admin pages follow the pattern correctly:

```tsx
// GenreManagementPage.tsx — 6 LOC, perfect
export default function GenreManagementPage() {
  return <GenreManagement />;
}
```

But shop pages break this rule — [ProductListPage.tsx](file:///d:/CatMusicShop/MusicShop/src/musicshop-web/src/pages/shop/ProductListPage.tsx) has **110 LOC** with:
- `useState` for `showFilters` (UI state, acceptable)
- Full pagination rendering logic
- Conditional loading/error/empty state rendering
- Grid layout

Similarly, [CheckoutSuccessPage.tsx](file:///d:/CatMusicShop/MusicShop/src/musicshop-web/src/pages/shop/CheckoutSuccessPage.tsx) is **154 LOC** with `useSearchParams` + `useOrderDetail` + conditional rendering.

[ProfilePage.tsx](file:///d:/CatMusicShop/MusicShop/src/musicshop-web/src/pages/shop/ProfilePage.tsx) directly accesses `useAuthStore` + has a redundant auth guard (already handled in `App.tsx` routing).

**Fix**: Extract feature components (e.g., `ProductList`, `CheckoutSuccess`, `UserProfile`) and make pages thin shells like admin pages.

---

## 3. `any` Type Casts in Critical Paths (Medium Impact)

Several places use `any` which your own rules explicitly prohibit:

```tsx
// useLoginForm.ts:48
resolver: zodResolver(loginSchema) as any,

// useLoginForm.ts:76
handleSubmit: handleSubmit(onSubmit) as any,

// useGenres.ts:21, useArtists.ts:29, useLabels.ts:21 (repeated pattern)
onError: (error: any) => {
  toast.error(error.message || 'Failed to create genre');
}

// MusicHeroScene.tsx:14-15
const cameraRef = useRef<any>(null);
const controlsRef = useRef<any>(null);
```

**Fix**:
- The `zodResolver` cast is a known compatibility issue between RHF v7 and Zod v3 — use `zodResolver(loginSchema) as Resolver<LoginFormValues>` for partial type safety.
- Mutation `onError` should use `Error` type: `(error: Error) => { ... }`.
- R3F refs should use proper types: `useRef<PerspectiveCamera>(null)`, `useRef<OrbitControlsImpl>(null)`.

---

## 4. Duplicate Auth Guards (Low-Medium Impact)

Auth protection is applied in **three different places**:

1. **Route-level** in `App.tsx`: `accessToken ? <CheckoutPage /> : <Navigate to="/login" />`
2. **Component-level** in `ProfilePage.tsx`: `if (!accessToken) return <Navigate to="/login" />`
3. **`AdminRoute` wrapper** component for admin routes

This leads to:
- Inconsistent guard behavior (some routes redirect silently, `AdminRoute` logs a warning)
- Double-checking in `ProfilePage` (route already guards it)

**Fix**: Consolidate into a single `<ProtectedRoute>` component and an `<AdminRoute>`, used at the route level only. Remove all in-page auth checks.

```tsx
// App.tsx — clean
<Route element={<ProtectedRoute />}>
  <Route path="/checkout" element={<CheckoutPage />} />
  <Route path="/profile" element={<ProfilePage />} />
  <Route path="/orders" element={<OrderHistoryPage />} />
</Route>
```

---

## 5. No Query Key Factory (Low-Medium Impact)

Query keys are scattered as raw strings across hooks:

```tsx
queryKey: ['genres', { page, limit, search }]
queryKey: ['artists', { page, limit, search }]
queryKey: ['artists', slug]
```

If you rename a key or need to invalidate all artist-related queries, you have to grep for string literals.

**Fix**: Centralize query keys per feature.

```tsx
// features/catalog/queryKeys.ts
export const catalogKeys = {
  genres: {
    all: ['genres'] as const,
    list: (params: { page: number; limit: number; search?: string }) =>
      [...catalogKeys.genres.all, params] as const,
  },
  artists: {
    all: ['artists'] as const,
    list: (params: {...}) => [...catalogKeys.artists.all, params] as const,
    detail: (slug: string) => [...catalogKeys.artists.all, slug] as const,
  },
};
```

---

## 6. Missing Error Boundary (Medium Impact)

The Three.js scene (`MusicHeroScene`) is heavy — WebGL context failures, missing GLB assets, or shader compilation errors will crash the entire app. There's no `ErrorBoundary` anywhere in the component tree.

**Fix**: Wrap `<MusicHeroScene />` in a React `ErrorBoundary` that renders a static fallback hero. Also add a top-level `ErrorBoundary` in `main.tsx`.

---

## 7. `HomePage` Dead Code (Low Impact)

[HomePage.tsx](file:///d:/CatMusicShop/MusicShop/src/musicshop-web/src/pages/shop/HomePage.tsx) imports and reads `accessToken` and `user` from `useAuthStore` but **never uses them** in the JSX:

```tsx
const accessToken = useAuthStore((state) => state.accessToken);  // unused
const user = useAuthStore((state) => state.user);                // unused
const isAuthenticated = !!accessToken;                           // unused
```

These cause unnecessary re-renders on every auth state change.

---

## 8. No Route Configuration File (Low Impact)

Routes are defined inline in `App.tsx` as JSX. Your own rules specify:

> Route definitions in single `src/routes.tsx`

Currently route paths are hardcoded strings in `App.tsx`, sidebar links in `AdminLayout.tsx`, and `<Link to="/login">` scattered across components. A typo in any of these is a silent bug.

**Fix**: Create `src/routes.ts` with route constants and a `src/routes.tsx` with the route tree.

---

## Summary — Priority Order

| # | Issue | Impact | Effort |
|---|---|---|---|
| 1 | CRUD hook/service duplication | High | Medium |
| 2 | Fat shop pages | Medium | Low |
| 3 | `any` casts | Medium | Low |
| 4 | Duplicate auth guards | Low-Medium | Low |
| 5 | No query key factory | Low-Medium | Low |
| 6 | Missing error boundaries | Medium | Low |
| 7 | Dead code in HomePage | Low | Trivial |
| 8 | No route config file | Low | Low |

Items 2–8 are all quick wins (< 1 hour each). Item 1 is the structural improvement that pays off most as the catalog grows.
