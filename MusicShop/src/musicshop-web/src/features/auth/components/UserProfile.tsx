import { Card, CardHeader, CardTitle, CardContent } from '@/shared/components';
import { User, Mail, Shield } from 'lucide-react';
import { useUserProfile } from '@/features/auth/hooks/useUserProfile';

export function UserProfile() {
  const { user } = useUserProfile();

  return (
    <div className="container mx-auto px-4 py-24">
      <div className="max-w-2xl mx-auto">
        <h1 className="text-3xl font-bold mb-8 text-white">Your Profile</h1>
        <Card className="bg-neutral-900 border-neutral-800 text-white">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <User className="h-5 w-5 text-blue-500" />
              Account Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="flex flex-col space-y-1">
              <label className="text-xs text-neutral-500 uppercase font-bold tracking-wider">Full Name</label>
              <div className="text-lg">{user?.fullName || 'N/A'}</div>
            </div>
            <div className="flex flex-col space-y-1">
              <label className="text-xs text-neutral-500 uppercase font-bold tracking-wider">Email Address</label>
              <div className="flex items-center gap-2 text-lg">
                <Mail className="h-4 w-4 text-neutral-600" />
                {user?.email || 'N/A'}
              </div>
            </div>
            <div className="flex flex-col space-y-1">
              <label className="text-xs text-neutral-500 uppercase font-bold tracking-wider">Role</label>
              <div className="flex items-center gap-2 text-lg">
                <Shield className="h-4 w-4 text-neutral-600" />
                {user?.role || 'N/A'}
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
