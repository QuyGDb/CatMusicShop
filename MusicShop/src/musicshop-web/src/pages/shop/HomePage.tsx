import React from 'react';
import { Link } from 'react-router-dom';
import { useAuthStore } from '@/store/useAuthStore';
import { buttonVariants } from '@/shared/components';
import { ShoppingBag } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { FeaturedCollections } from '@/features/curation/components/storefront/FeaturedCollections';
import { MusicHeroScene } from '@/features/threejs';

export default function HomePage() {
  const accessToken = useAuthStore((state) => state.accessToken);
  const user = useAuthStore((state) => state.user);
  const isAuthenticated = !!accessToken;

  return (
    <div className="flex flex-col min-h-screen">
      {/* Immersive 3D Hero Section */}
      <div className="flex flex-row w-full h-screen overflow-hidden bg-[#09090b]">
        {/* Left Column: 3D Scene */}
        <div className="flex-1 h-full relative">
          <MusicHeroScene />
        </div>

        <div className="hidden lg:block w-[380px] h-full border-l border-white/5 bg-black">
          <iframe 
            title="Spotify Playlist"
            style={{ borderRadius: '12px' }} 
            src="https://open.spotify.com/embed/playlist/1HYRmT9RhRJ8YCEcPyNesn?utm_source=generator&theme=0" 
            width="100%" 
            height="100%" 
            frameBorder="0" 
            allowFullScreen={true} 
            allow="autoplay; clipboard-write; encrypted-media; fullscreen; picture-in-picture" 
            loading="lazy"
          ></iframe>
        </div>
      </div>

      {/* Featured Collections Section */}
      <div className="container mx-auto px-4 pb-32">
        <FeaturedCollections />
      </div>
    </div>
  );
}
