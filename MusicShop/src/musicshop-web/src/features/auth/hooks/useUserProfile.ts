import { useAuthStore } from '@/store/useAuthStore';

export function useUserProfile() {
  const user = useAuthStore((state) => state.user);

  return {
    user,
  };
}
