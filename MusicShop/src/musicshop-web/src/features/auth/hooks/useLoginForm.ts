import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { CredentialResponse } from '@react-oauth/google';
import { authService } from '@/features/auth/services/authService';
import { useAuthRedirect } from '@/shared/hooks/useAuthRedirect';

import { loginSchema, LoginFormValues } from '../schemas';


import { UseFormRegister, FieldErrors } from 'react-hook-form';

interface UseLoginFormReturn {
  register: UseFormRegister<LoginFormValues>;
  handleSubmit: (e?: React.BaseSyntheticEvent) => Promise<void>;
  errors: FieldErrors<LoginFormValues>;
  isSubmitting: boolean;
  serverError: string | null;
  handleGoogleSuccess: (credentialResponse: CredentialResponse) => void;
}


export function useLoginForm(): UseLoginFormReturn {

  const { redirectAfterAuth } = useAuthRedirect();

  // Standard Login Mutation
  const loginMutation = useMutation({
    mutationFn: authService.login,
    onSuccess: (data) => {
      redirectAfterAuth(data.accessToken, data.user);
    },
  });

  // Google Login Mutation
  const googleLoginMutation = useMutation({
    mutationFn: authService.googleLogin,
    onSuccess: (data) => {
      redirectAfterAuth(data.accessToken, data.user);
    },
  });

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),

    defaultValues: {
      email: '',
      password: '',
    },
  });

  const onSubmit = async (value: LoginFormValues) => {
    loginMutation.mutate(value);
  };

  const handleGoogleSuccess = (credentialResponse: CredentialResponse) => {
    const idToken = credentialResponse.credential;

    if (!idToken) {
      return;
    }

    googleLoginMutation.mutate(idToken);
  };

  // Combine loading and error states for the UI
  const error = loginMutation.error || googleLoginMutation.error;
  const serverError = error instanceof Error ? error.message : null;

  return {
    register,
    handleSubmit: handleSubmit(onSubmit),
    errors,
    isSubmitting: isSubmitting || loginMutation.isPending || googleLoginMutation.isPending,
    serverError,
    handleGoogleSuccess
  };
}

